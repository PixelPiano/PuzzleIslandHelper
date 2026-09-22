using Celeste.Mod.Entities;
using Celeste.Mod.Helpers;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using Celeste.Mod.PuzzleIslandHelper.Entities.FrequencyEntities;
using Celeste.Mod.XaphanHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Core.Tokens;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.WIP
{
    [CustomEntity("PuzzleIslandHelper/StabilizerGroup")]
    [Tracked]
    public class StabilizerGroupEntity : Entity
    {
        public class ChildStabilizer : Stabilizer
        {
            public StabilizerGroupEntity Group;
            public bool State;
            public override bool On
            {
                get => State;
                set => State = value;
            }
            public ChildStabilizer(Vector2 position, bool on, StabilizerGroupEntity group) : base(position)
            {
                State = on;
                Group = group;
                Tag |= Tags.TransitionUpdate;
            }
            public override void Update()
            {
                base.Update();
                GlowMultAdd = Group.GlowMultAdd;
            }
        }
        public class Cubby : GraphicsComponent
        {
            public bool On;
            private MTexture cubbyOn, cubbyOff, shade, glow;
            public float Width => cubbyOn.Width;
            public Cubby(Vector2 position, int facing) : base(true)
            {
                Position = position;
                string path = "objects/PuzzleIslandHelper/stabilizer/";
                cubbyOn = GFX.Game[path + "cubbyOn"];
                cubbyOff = GFX.Game[path + "cubbyOff"];
                glow = GFX.Game[path + "glow"];
                shade = GFX.Game[path + "shade"];
                Scale = new Vector2(-facing, 1);
            }
            public override void Render()
            {
                base.Render();
                bool on = On;
                MTexture c = on ? cubbyOn : cubbyOff;
                Vector2 offset = c.HalfSize();
                Vector2 p = RenderPosition + offset;
                c.Draw(p, offset, Color.White, Scale);
                if (on)
                {
                    glow.Draw(p, offset, Color.White, Scale);
                }
                else
                {
                    shade.Draw(p, offset, Color.White, Scale);
                }
            }
        }
        private class satellite : Entity
        {
            private float[] alphas;
            public Image image;
            public Image light;
            public Color Color = Color.White;
            public bool Warning;
            public satellite(Vector2 bottomCenter, int facing) : base(bottomCenter)
            {
                Add(image = new Image(GFX.Game["objects/PuzzleIslandHelper/stabilizer/sat"]));
                Position -= new Vector2(image.Width / 2, image.Height);
                Add(light = new Image(GFX.Game["objects/PuzzleIslandHelper/stabilizer/satTip"]));
                alphas = [0, 0, 0];
                image.Scale.X = light.Scale.X = facing;
                if (facing < 0)
                {
                    light.X += light.Width;
                    image.X += image.Width;
                }
                Collider = image.Collider();
                light.Y = 2;
                light.X = image.Width / 2 + facing * 3;
                Add(new Coroutine(routine(0, 0.8f, 0.2f)));
                Add(new Coroutine(routine(1, 0.4f, 0.4f)));
                Add(new Coroutine(routine(2, 0f, 0.7f)));
            }

            private IEnumerator routine(int index, float delay, float alpha)
            {
                if (delay > 0) yield return delay;
                while (true)
                {
                    for (float i = 0; i < 1; i += Engine.DeltaTime)
                    {
                        float eased = Ease.SineInOut(i);
                        alphas[index] = eased * alpha;
                        yield return null;
                    }
                    for (float i = 1; i > 0; i -= Engine.DeltaTime)
                    {
                        float eased = Ease.SineInOut(i);
                        alphas[index] = eased * alpha;
                        yield return null;
                    }
                    alphas[index] = 0;
                }
            }
            public override void Render()
            {
                if (CullHelper.IsRectangleVisible(X, Y, Width, Height, 4))
                {
                    image.Render();
                    light.Render();
                    for (int i = 0; i < alphas.Length; i++)
                    {
                        if (alphas[i] > 0)
                        {
                            light.DrawOutline(Color * alphas[i], alphas.Length + 1 - i);
                        }
                    }
                }
            }
            public override void Update()
            {
                base.Update();
                light.Color = Color;
            }
        }
        public string CurrentCombo
        {
            get => PianoModule.Session.StabilizerGroupCombos[EntityID];
            set => PianoModule.Session.StabilizerGroupCombos[EntityID] = value;
        }
        public string LastValidCombo
        {
            get => PianoModule.Session.StabilizersLastValidCombo[EntityID];
            set => PianoModule.Session.StabilizersLastValidCombo[EntityID] = value;
        }
        public string DefaultCombo;
        private string comboOnAdded;
        public List<(string, string)> ComboFlags = [];
        public float GlowMultAdd = 0;
        public string ID;
        public bool Locked;
        public Facings Facing;
        private satellite Satellite;
        private bool resetOnRemoved;
        public EntityID EntityID;
        private Cubby[] Cubbies;
        private ChildStabilizer[] stabilizers;
        public FlagList ValidTrackingFlag;
        private bool validState => ValidTrackingFlag;
        private bool requiresWorldShiftPermissions;
        public string SatelliteID;
        public DotX3 Talk;

        public bool HasPermissions => !requiresWorldShiftPermissions || string.IsNullOrEmpty(SatelliteID) || PianoModule.Session.WorldShifterToStabilizerConnections.Contains(SatelliteID);
        public StabilizerGroupEntity(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
        {
            requiresWorldShiftPermissions = data.Bool("requiresWorldShiftPermissions", false);
            SatelliteID = data.Attr("satelliteID");
            Tag |= Tags.TransitionUpdate;
            EntityID = id;
            Facing = data.Enum<Facings>("facing");
            Depth = 2;
            Locked = data.Bool("locked");
            ID = data.Attr("groupID");
            DefaultCombo = data.Attr("combo").Trim();
            resetOnRemoved = data.Bool("persistent", true);
            ValidTrackingFlag = data.FlagList("validTrackingFlag");
            string[] combos = data.Attr("combos").Replace(" ", "").Trim().Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (string s in combos)
            {
                string[] array = s.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (array.Length == 2)
                {
                    ComboFlags.Add(new(array[0], array[1]));
                }
            }
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            string path = "objects/PuzzleIslandHelper/stabilizer/";
            MTexture on = GFX.Game[path + "on"];

            if (PianoModule.Session.StabilizerGroupCombos.TryAdd(EntityID, DefaultCombo))
            {
                LastValidCombo = DefaultCombo;
            }
            else
            {
                LastValidCombo = PianoModule.Session.StabilizersLastValidCombo[EntityID];
            }

            string combo = comboOnAdded = CurrentCombo;
            Cubbies = new Cubby[combo.Length];
            stabilizers = new ChildStabilizer[combo.Length];
            Vector2 p = Vector2.Zero;
            for (int i = 0; i < combo.Length; i++)
            {
                ChildStabilizer s = new ChildStabilizer(p + Position, combo[i] == '1', this);
                Cubby c = new Cubby(p, (int)Facing);
                Add(Cubbies[i] = c);
                scene.Add(stabilizers[i] = s);
                p.X += on.Width;
            }

            MTexture edge = GFX.Game[path + "edge"];
            Collider = new Hitbox(p.X, on.Height);
            scene.Add(Satellite = new satellite(TopCenter, (int)Facing == -1 ? -1 : 1) { Depth = Depth + 1 });
            Add(Talk = new DotX3(Satellite.Position - Position, Satellite.Width, Satellite.Height + Height, Satellite.Position - Position + Vector2.UnitX * Satellite.Width / 2, (p) =>
            {
                if (requiresWorldShiftPermissions)
                {
                    if (!HasPermissions)
                    {
                        Scene.Add(new talkScene(SatelliteID));
                        //please register this id with world shift machine
                    }
                    else
                    {
                        UpdateFlags();
                        Color color = ValidTrackingFlag ? Color.Lime : Color.Red;
                        Pulse.Circle(this, Pulse.Fade.InAndOut, Pulse.Mode.Oneshot, Satellite.light.RenderPosition, 0, 320, 0.4f, true, color, Color.White);
                    }
                }
                else
                {
                    SetCurrentCombo(DefaultCombo);
                }
                Input.Talk.ConsumePress();
            }));
            Talk.PlayerMustBeFacing = false;
            Add(Cubbies);
            Add(new Image(edge) { X = -edge.Width });
            Add(new Image(edge) { X = Width + edge.Width, Scale = new Vector2(-1, 1) });
            UpdateCurrentCombo();
        }
        public void UpdateFlags()
        {
            bool foundCombo = false;
            foreach (var a in ComboFlags)
            {
                bool valid = a.Item1 == CurrentCombo;
                if(valid) foundCombo = true;
                SceneAs<Level>().Session.SetFlag(a.Item2, valid);
            }
            ValidTrackingFlag.State = foundCombo;
        }
        private class talkScene : CutsceneEntity
        {
            private string id;
            private Textbox textbox;
            public talkScene(string id) : base()
            {
                this.id = id;
            }
            public override void OnBegin(Level level)
            {
                level.DisableMovement();
                Add(new Coroutine(routine()));
            }
            private IEnumerator routine()
            {
                textbox = new Textbox("SatelliteIDWarning");
                string d = Dialog.Get("SatelliteIDWarning", null).Replace("p0p", '[' + id + ']');
                textbox.text = FancyText.Parse(d, (int)textbox.maxLineWidth, textbox.linesPerPage, 0f, null, null);
                Level.Add(textbox);
                while (textbox.Opened)
                {
                    yield return null;
                }
                EndCutscene(Level);
            }
            public override void OnEnd(Level level)
            {
                level.EnableMovement();
                textbox?.RemoveSelf();
            }
        }
        public void SetCurrentCombo(string combo)
        {
            CurrentCombo = "";
            for (int i = 0; i < combo.Length && i < stabilizers.Length && i < Cubbies.Length; i++)
            {
                bool on = combo[i] == '1';
                stabilizers[i].On = on;
                Cubbies[i].On = on;
                CurrentCombo += combo[i];
            }
            if (!requiresWorldShiftPermissions) UpdateFlags();
        }
        public void UpdateCurrentCombo()
        {
            string combo = "";
            for (int i = 0; i < Cubbies.Length; i++)
            {
                bool on = stabilizers[i].On;
                Cubbies[i].On = on;
                combo += on ? '1' : '0';
            }
            CurrentCombo = combo;
            if (!requiresWorldShiftPermissions) UpdateFlags();
        }
        public override void Update()
        {
            base.Update();
            UpdateCurrentCombo();
            if (requiresWorldShiftPermissions)
            {
                Talk.Enabled = !HasPermissions;
                Satellite.Warning = Talk.Enabled && !IsValidCombo(CurrentCombo);
            }
            else
            {
                Talk.Enabled = CurrentCombo != DefaultCombo;
                Satellite.Warning = !IsValidCombo(CurrentCombo);
            }
        }
        public bool IsValidCombo(string combo)
        {
            foreach (var pair in ComboFlags)
            {
                if (pair.Item1 == combo) return true;
            }
            return false;
        }
        public override void Removed(Scene scene)
        {
            if (resetOnRemoved)
            {
                CurrentCombo = comboOnAdded;
                UpdateCurrentCombo();
            }
            base.Removed(scene);
            Satellite?.RemoveSelf();

        }
    }
    [CustomEntity("PuzzleIslandHelper/Stabilizer")]
    [Tracked]
    public class FlagStabilizer : Stabilizer
    {
        public FlagData Flag;
        public string ID;
        public override bool On
        {
            get => Flag;
            set => Flag.State = value;
        }
        public bool StartState;
        public bool ResetOnRemoved;
        public FlagStabilizer(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
        {
            ID = id.ToString();
            StartState = data.Bool("startOn");
            ResetOnRemoved = !data.Bool("persistent", true);
            Flag = new FlagData("Stabilizer:" + ID);
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            if (ResetOnRemoved)
            {
                Flag.State = StartState;
            }
        }
    }
    [Tracked]
    public abstract class Stabilizer : Entity
    {
        public static readonly float[] OnRates = [1, 1, 1, 1];
        public static readonly float[] OffRates = [0, 0, 0, 0];
        public float GlowMult = 1;
        public float GlowMultAdd;
        public float Glow
        {
            get
            {
                float mult = GlowMult + GlowMultAdd;
                return GlowLerp * ApproachMult * mult * BaseAlpha;
            }
        }
        public float GlowLerp;
        public float BaseAlpha = 0.5f;
        public float ApproachMult;
        public abstract bool On { get; set; }
        private Image Image;
        private MTexture onTex, offTex;
        public VertexLight Light;
        public BloomPoint Bloom;
        public Stabilizer(Vector2 position) : base(position)
        {
            Depth = 1;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            onTex = GFX.Game["objects/PuzzleIslandHelper/stabilizer/on"];
            offTex = GFX.Game["objects/PuzzleIslandHelper/stabilizer/off"];
            GlobalFrequencyReceiver on = null, off = null;
            Add(off = new GlobalFrequencyReceiver(OffRates)
            {
                StopAtZeroPower = false,
                OnlyIfInside = true,
                RequiresAudibleSound = true,
                Instant = true,
                OnFullPower = () =>
                {

                    if (On)
                    {
                        off.Active = false;
                        on.Reset(false, true);
                        On = false;
                    }
                }
            });
            Add(on = new GlobalFrequencyReceiver(OnRates)
            {
                OnlyIfInside = true,
                RequiresAudibleSound = true,
                StopAtZeroPower = false,
                Instant = true,
                OnFullPower = () =>
                {
                    if (!On)
                    {
                        on.Active = false;
                        off.Reset(false, true);
                        On = true;
                    }
                }
            });
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Image = new Image(On ? onTex : offTex);
            Add(Image);
            Collider = Image.Collider();
            Add(Light = new VertexLight(Collider.HalfSize, Color.Lime, 0, (int)Width, (int)(Width * 1.5f)));
            Add(Bloom = new BloomPoint(Collider.HalfSize, 0, Width));
            Tween.Set(this, Tween.TweenMode.YoyoLooping, 2f, Ease.SineInOut, t =>
            {
                GlowLerp = t.Eased;
            });
        }
        public override void Update()
        {
            base.Update();
            bool on = On;
            if (on)
            {
                ApproachMult = Calc.Approach(ApproachMult, 1, Engine.DeltaTime);
                Image.Texture = onTex;
            }
            else
            {
                ApproachMult = Calc.Approach(ApproachMult, 0, Engine.DeltaTime);
                Image.Texture = offTex;
            }
            Light.Alpha = Bloom.Alpha = Glow;
        }
    }
}