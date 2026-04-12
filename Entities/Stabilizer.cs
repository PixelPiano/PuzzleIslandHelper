using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using Celeste.Mod.PuzzleIslandHelper.Entities.FrequencyEntities;
using Celeste.Mod.XaphanHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.WIP
{
    [CustomEntity("PuzzleIslandHelper/Stabilizer")]
    [Tracked]
    public class Stabilizer : Entity
    {
        [CustomEntity("PuzzleIslandHelper/StabilizerTutorial")]
        public class GroupActivator : Entity
        {
            private Color offColor = Color.Lerp(Color.White, Color.Black, 0.8f);
            public List<List<bool>> Combos = [];
            public int Index;
            public float Interval = 1;
            private float whiteLerp;
            private float whiteLerpTimer;
            public GroupActivator(EntityData data, Vector2 offset) : base(data.Position + offset)
            {
                Collider = new Hitbox(data.Width, data.Height);
                string[] split = data.Attr("combos").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                foreach (string s in split)
                {
                    List<bool> combo = [];
                    foreach (char c in s)
                    {
                        switch (c)
                        {
                            case '0':
                                combo.Add(false);
                                break;
                            case '1':
                                combo.Add(true);
                                break;

                        }
                    }
                    if (combo.Count > 0)
                    {
                        Combos.Add(combo);
                    }
                }
            }
            public bool Check(List<bool> comboA, List<bool> comboB)
            {
                if (comboA.Count <= comboB.Count)
                {
                    for (int i = 0; i < comboA.Count; i++)
                    {
                        if (comboA[i] != comboB[i])
                        {
                            return false;
                        }
                    }
                    return true;
                }
                return false;
            }
            public override void Update()
            {
                base.Update();
                if (whiteLerpTimer > 0)
                {
                    whiteLerpTimer -= Engine.DeltaTime;
                    if (whiteLerpTimer <= 0)
                    {
                        whiteLerpTimer = 0;
                        whiteLerp = 0;
                    }
                    else
                    {
                        whiteLerp = Ease.CubeIn(1 - whiteLerpTimer);
                    }
                }
                if (Scene.OnInterval(Interval))
                {
                    Index = (Index + 1) % Combos.Count;
                    whiteLerpTimer = 1;
                    whiteLerp = 1;
                }
                List<bool> combo = Combos[Index];
                /*                foreach (StabilizerGroupTrigger group in Scene.Tracker.GetEntities<StabilizerGroupTrigger>())
                                {
                                    if (Check(group.Combo, combo))
                                    {
                                        group.GlowMultAdd = Calc.Approach(group.GlowMultAdd, 0.5f, Engine.DeltaTime);
                                    }
                                    else
                                    {
                                        group.GlowMultAdd = Calc.Approach(group.GlowMultAdd, 0f, Engine.DeltaTime);
                                    }
                                }*/
            }
            public override void Render()
            {
                base.Render();

                List<bool> combo = Combos[Index];
                float nodeSize = Width / (combo.Count + 1);
                float space = nodeSize / combo.Count;
                Vector2 position = Position + new Vector2(space, Height / 2 - nodeSize / 2);
                Draw.HollowRect(X - 1, Y - 1, Width + 2, Height + 2, Color.Black);
                Draw.Rect(Collider, Color.DarkGray);
                Color outline = Color.Lerp(Color.Gray, Color.White, whiteLerp);
                foreach (bool b in Combos[Index])
                {
                    Color node = Color.Lerp(b ? Color.Lime : offColor, Color.White, whiteLerp);
                    Draw.HollowRect(position.X - 2, position.Y - 2, nodeSize + 4, nodeSize + 4, outline);
                    Draw.Rect(position.X, position.Y, nodeSize, nodeSize, node);
                    position.X += space + nodeSize;
                }
            }
        }
        /*        [CustomEntity("PuzzleIslandHelper/StabilizerGroup")]
                [Tracked]
                public class StabilizerGroupTrigger : Trigger
                {
                    public Vector2[] Nodes;
                    public float GlowMultAdd = 0;
                    public List<bool> TargetCombo = [];
                    public List<bool> Combo = [];
                    public HashSet<Stabilizer> InGroup = [];
                    public string ID;
                    public bool Locked;
                    public FrequencyReceiver Receiver;
                    public StabilizerGroupTrigger(EntityData data, Vector2 offset) : base(data, offset)
                    {
                        Locked = data.Bool("locked");
                        Nodes = data.NodesOffset(offset);
                        TargetCombo = GetCombo(data.Attr("combo"));
                        ID = data.Attr("groupID");
                    }
                    public List<bool> GetCombo(string s)
                    {
                        List<bool> combo = [];
                        foreach (char c in s)
                        {
                            switch (c)
                            {
                                case '0':
                                    combo.Add(false);
                                    break;
                                case '1':
                                    combo.Add(true);
                                    break;
                            }
                        }
                        return combo;
                    }
                    public override void Awake(Scene scene)
                    {
                        base.Awake(scene);
                        foreach (Vector2 v in Nodes)
                        {
                            List<Stabilizer> list = scene.CollideAll<Stabilizer>(v);
                            foreach (var s in scene.CollideAll<Stabilizer>(v))
                            {
                                InGroup.Add(s);
                            }
                        }
                        InGroup = [.. InGroup.OrderBy(item => item.X)];
                        foreach (Stabilizer s in InGroup)
                        {
                            s.Group = this;
                        }
                        UpdateCombo();
                    }
                    public void UpdateCombo()
                    {
                        Combo.Clear();
                        foreach (var s in InGroup)
                        {
                            Combo.Add(s.On);
                        }
                    }
                    public override void Update()
                    {
                        base.Update();
                        UpdateCombo();
                        bool valid = Combo.Count == TargetCombo.Count;
                        int count = Math.Min(Combo.Count, TargetCombo.Count);
                        for (int i = 0; i < count; i++)
                        {
                            valid &= Combo[i] == TargetCombo[i];
                        }
                        SceneAs<Level>().Session.SetFlag("StabilizerGroup{" + ID + "}", valid);
                    }
                }*/
        [CustomEntity("PuzzleIslandHelper/StabilizerGroup")]
        [Tracked]
        public class StabilizerGroupEntity : Entity
        {
            public Vector2[] Nodes;
            public float GlowMultAdd = 0;
            public string Combo = "";
            public string StartCombo;
            public List<(string, string)> ComboFlags = [];
            public List<Stabilizer> InGroup = [];
            public string ID;
            public bool Locked;
            public Facings DishFacing;
            private Image[] cubbies;
            private Image sat;
            private Image satLight;
            private Image[] sides;
            public StabilizerGroupEntity(EntityData data, Vector2 offset) : base(data.Position + offset)
            {
                DishFacing = data.Enum<Facings>("dishFacing");
                Depth = 2;
                Locked = data.Bool("locked");
                Nodes = data.NodesWithPosition(offset);
                ID = data.Attr("groupID");
                StartCombo = data.Attr("combo").Trim();
                if (data.Bool("persistent", true))
                {
                    Tag |= Tags.Persistent;
                }
                string[] combos = data.Attr("combos").Replace(" ", "").Trim().Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                foreach (string s in combos)
                {
                    string[] array = s.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    if (array.Length == 2)
                    {
                        ComboFlags.Add(new(array[0], array[1]));
                    }
                }
                /*                Add(new PlayerCollider(p =>
                                {
                                    if (ForkAmpSound.GlobalPlaying)
                                    {
                                        float[] rates = FrequencyData.GetRates(Scene);
                                        string combo = "";
                                        foreach (float f in rates)
                                        {
                                            if (f != 0 || f != 1) return;
                                            combo += ((int)f).ToString();
                                        }
                                        int start = int.MaxValue;
                                        for (int i = 0; i < InGroup.Count; i++)
                                        {
                                            if (p.CollideCheck(InGroup[i]) && start > i)
                                            {
                                                start = i;
                                            }
                                        }
                                        if (start < InGroup.Count)
                                        {
                                            for (int i = start; i < InGroup.Count && i - start < 4; i++)
                                            {
                                                InGroup[i].On.Set(rates[i - start] == 1);
                                            }
                                        }
                                    }
                                }));*/
            }
            public override void Added(Scene scene)
            {
                base.Added(scene);
                string path = "objects/PuzzleIslandHelper/stabilizer/";
                satLight = new Image(GFX.Game[path + "satTip"]);
                sides = new Image[2];
                cubbies = new Image[InGroup.Count];
                sat = new Image(GFX.Game[path + "sat"]);
                sides[0] = new Image(GFX.Game[path + "side"]);
                sides[1] = new Image(GFX.Game[path + "side"]);
                for (int i = 0; i < InGroup.Count; i++)
                {
                    cubbies[i] = new Image(GFX.Game[path + "cubbyOff"]);
                    cubbies[i].X = i * 16;
                }
                sides[0].X = -16;
                sides[1].X = Width + sides[1].Width * 0.5f;
                sides[1].JustifyOrigin(0.5f, 0);
                sides[1].Scale.X = -1;
                sat.JustifyOrigin(0.5f, 1);
                sat.Position = TopCenter - Position;
                sat.Scale.X = (int)DishFacing;
                satLight.Position = sat.Position;
                satLight.Scale = sat.Scale;
                satLight.Origin = sat.Origin;
                Add(sat);
                Add(satLight);
                Add(cubbies);
                Add(sides);
            }
            public override void Awake(Scene scene)
            {
                base.Awake(scene);
                foreach (Vector2 v in Nodes)
                {
                    Stabilizer s = new Stabilizer(v, new EntityID(Guid.NewGuid().ToString(), 0), false, false);
                    InGroup.Add(s);
                    scene.Add(s);
                }
                foreach (Stabilizer s in InGroup)
                {
                    s.Group = this;
                }
                for (int i = 0; i < InGroup.Count && i < StartCombo.Length; i++)
                {
                    InGroup[i].On.Set(StartCombo[i] == '1');
                }
                UpdateCombo();
                float width = 0, height = 0;
                foreach (Stabilizer s in InGroup)
                {
                    width = (int)(Math.Max(width, s.Right - X));
                    height = (int)(Math.Max(height, s.Bottom - Y));
                }
                Collider = new Hitbox(width, height);
            }
            public void UpdateCombo()
            {
                Combo = "";
                foreach (var s in InGroup)
                {
                    Combo += s.On ? '1' : '0';
                }
                for (int i = 0; i < InGroup.Count; i++)
                {
                    cubbies[i].Texture = GFX.Game["objects/PuzzleIslandHelper/stabilizer/cubby" + (InGroup[i].On ? "On" : "Off")];
                }
            }
            public override void Update()
            {
                base.Update();
                UpdateCombo();
                foreach ((string code, string flag) in ComboFlags)
                {
                    if (!string.IsNullOrEmpty(flag))
                    {
                        SceneAs<Level>().Session.SetFlag(flag, Combo == code);
                    }
                }
            }
        }
        public StabilizerGroupEntity Group;
        public static readonly float[] OnRates = [1, 1, 1, 1];
        public static readonly float[] OffRates = [0, 0, 0, 0];
        public float GlowMult = 1;
        public float Glow
        {
            get
            {
                float mult = Group != null ? (GlowMult + Group.GlowMultAdd) : GlowMult;
                return GlowLerp * ApproachMult * mult * BaseAlpha;
            }
        }
        public float GlowLerp;
        public float BaseAlpha = 0.5f;
        public float ApproachMult;
        public FlagData On;
        public FlagData HasBeenUsed;
        public EntityID ID;
        private bool startOn;
        private bool persistent;
        private Image Image;
        private MTexture onTex, offTex;
        public VertexLight Light;
        public BloomPoint Bloom;
        private bool wasOn;
        public bool Locked;
        public bool State
        {
            get
            {
                bool locked = Locked || (Group != null && Group.Locked);
                return locked ? startOn : On;
            }
        }
        public Stabilizer(EntityData data, Vector2 offset, EntityID id) :
            this(data.Position + offset, id, data.Bool("locked"), data.Bool("startOn"), data.Bool("persistent", true))
        {

        }
        public Stabilizer(Vector2 position, EntityID id, bool locked, bool startOn, bool persistent = true) : base(position)
        {
            Locked = locked;
            Depth = 1;
            ID = id;
            On = new FlagData("Stabilizer:" + ID.ToString());
            HasBeenUsed = new FlagData("StabilizerUsed:" + ID.ToString());
            this.startOn = startOn;
            this.persistent = persistent;
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            /*            if (Group == null)
                        {*/
            GlobalFrequencyReceiver on = null, off = null;
            Add(on = new GlobalFrequencyReceiver(OnRates)
            {
                OnlyIfInside = true,
                RequiresAudibleSound = true,
                OnFullPower = () =>
                {
                    if (!On)
                    {
                        on.Active = false;
                        off.Reset(false, true);
                        HasBeenUsed.State = true;
                        On.State = true;
                    }
                }
            });
            Add(off = new GlobalFrequencyReceiver(OffRates)
            {
                OnlyIfInside = true,
                RequiresAudibleSound = true,
                OnFullPower = () =>
                {
                    if (On)
                    {
                        off.Active = false;
                        on.Reset(false, true);
                        HasBeenUsed.State = true;
                        On.State = true;
                    }
                }
            });
            if (!persistent || !HasBeenUsed)
            {
                On.State = startOn;
            }
            //}
            Image = new Image(On ? onTex : offTex);
            Add(Image);
            Collider = Image.Collider();
            Add(Light = new VertexLight(Collider.HalfSize, Color.Lime, 0, (int)Width, (int)(Width * 1.5f)));
            Add(Bloom = new BloomPoint(Collider.HalfSize, 0, Width));
            Tween.Set(this, Tween.TweenMode.YoyoLooping, 2f, Ease.SineInOut, t =>
            {
                GlowLerp = t.Eased;
            });
            wasOn = On;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            onTex = GFX.Game["objects/PuzzleIslandHelper/stabilizer/on"];
            offTex = GFX.Game["objects/PuzzleIslandHelper/stabilizer/off"];

        }
        public override void Update()
        {
            base.Update();
            bool on = State;
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
            wasOn = on;
        }
    }
}