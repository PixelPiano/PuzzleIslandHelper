using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked]
    public class DebugEmil : Actor
    {
        public Image Image;
        public DebugEmil(Vector2 position) : base(position)
        {
            Depth = 1;
            Add(Image = new Image(GFX.Game["objects/PuzzleIslandHelper/debugEmil"]));
            Collider = Image.Collider();
        }
    }
    public class MemorySlotParticle : Entity
    {
        private bool invert;
        private float intervalOffset;
        private (Color, Color) Colors;
        public float Scale = 1;
        public Vector2 Target;
        public bool Waiting = true;
        public SlotData TargetData;
        private Sprite[] sprites = new Sprite[2];
        public MemorySlotParticle(Vector2 position, SlotData data, Vector2 target, Color color, Color color2) : base(position)
        {
            Colors = (color, color2);

            Target = target;
            Depth = -12999;
            TargetData = data;
            for (int i = 0; i < 2; i++)
            {
                sprites[i] = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/heart/");
                sprites[i].AddLoop("idle", "sparkle", 0.1f);
                sprites[i].CenterOrigin();
                sprites[i].Play("idle");
                sprites[i].Position += sprites[i].HalfSize();
                if (i > 0)
                {
                    sprites[i].Rotation = 90f.ToRad();
                }
            }
            Add(sprites);
            sprites[0].Color = Colors.Item1;
            sprites[1].Color = Colors.Item2;
            Collider = sprites[0].Collider();
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            intervalOffset = Calc.Random.Range(0, 5);
            Add(new Coroutine(Routine()));
        }
        public IEnumerator Routine()
        {
            while (Waiting) yield return null;
            yield return Lerp(Target);
        }
        public IEnumerator Lerp(Vector2 pos)
        {
            MemoryBlockade target = CollideFirst<MemoryBlockade>(pos);
            if (target != null && target.Slots.Count > TargetData.Index)
            {
                pos = target.Slots[TargetData.Index].RenderPosition + target.Slots[TargetData.Index].Width / 2 * Vector2.One;
            }
            pos -= Collider.HalfSize;
            Vector2 from = Position;
            yield return PianoUtils.Lerp(Ease.CubeOut, 3f, f => Position = Vector2.Lerp(from, pos, f));
            yield return 0.5f;
            Add(new Coroutine(ScaleOut(1f)));
            if (target != null)
            {
                target.Fill(target.Slots[TargetData.Index].Flag);
            }
        }
        public IEnumerator ScaleOut(float time)
        {
            float from = Scale;
            yield return PianoUtils.Lerp(Ease.SineIn, time, f => Scale = Calc.LerpClamp(from, 0, f), true);
            RemoveSelf();
        }
        public override void Update()
        {
            base.Update();

            if (Scene.OnInterval(0.1f, intervalOffset))
            {
                invert = !invert;
            }
            if (Scene.OnInterval(0.2f, intervalOffset))
            {
                AfterImage afterimage = new AfterImage(this, DrawZero)
                {
                    Speed = Vector2.UnitY * 16f,
                    DrawOnce = false,
                    Scale = new Vector2(Calc.Random.Range(0.6f, 1f))
                };
                afterimage.OnCollideV = (a, c) =>
                {
                    afterimage.Bottom = c.Hit.Top;
                    afterimage.Acceleration = Vector2.Zero;
                    afterimage.Speed = Vector2.Zero;
                };
                Scene.Add(afterimage);
            }
            sprites[0].Color = Colors.Item1;
            sprites[1].Color = Colors.Item2;
            sprites[0].Scale = sprites[1].Scale = Scale * Vector2.One;
            Colors = (Colors.Item2, Colors.Item1);
        }
        public void DrawZero()
        {
            Vector2 from = Position;
            Position = Vector2.Zero;
            base.Render();
            Position = from;
        }
    }
    public class HeartInventory
    {
        public Dictionary<string, string> Collected = new Dictionary<string, string>(); //spritepath, flag
        public Dictionary<string, string> InMachine = new Dictionary<string, string>(); //spritepath, flag
    }
    [CustomEntity("PuzzleIslandHelper/HeartMachine")]
    public class HeartMachine : Entity
    {
        public DotX3 Talk;
        public string FlagOnComplete;
        public string Flag;
        public bool Inverted;
        public string[] FlagArray;
        public string Flags;
        public static Dictionary<string, Dictionary<string, List<SlotData>>> DataPerArea => PianoMapDataProcessor.SlotData;
        public static Dictionary<string, List<SlotData>> Data;
        public static Dictionary<EntityID, (string, string)> Used => PianoModule.Session.UsedHeartMachines;
        private Image image;
        private EntityID id;
        public bool Occupied;
        public MachineHeart Heart;
        public KeyValuePair<string, string> NextPair;
        public HeartMachine(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
        {
            this.id = id;
            Depth = 2;
            image = new Image(GFX.Game["objects/PuzzleIslandHelper/heart/machine"]);
            Add(image);
            Collider = image.Collider();
            Add(new Rect(Vector2.One * (Width / 2 - 6), 12, 12, Color.Gray));
            FlagArray = data.Attr("flags").Replace(" ", "").Split(',');
            Flags = data.Attr("flags");
            FlagOnComplete = data.Attr("flagOnComplete");
            Flag = data.Attr("flag");
            Inverted = data.Bool("inverted");
            Add(Talk = new DotX3(Collider, Interact));
            Talk.PlayerMustBeFacing = false;
        }
        public void AddHeart(MachineHeart heart)
        {
            Heart = heart;
            heart.Visible = false;
            if (heart.Scene == null)
            {
                Scene.Add(heart);
            }
            heart.Center = Center;
            PianoModule.Session.HeartInventory.InMachine.Add(heart.SpritePath, heart.Flag);
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Heart?.RemoveSelf();
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (DataPerArea.TryGetValue(scene.GetAreaKey(), out var value))
            {
                Data = value;
            }
            if (Used.TryGetValue(id, out (string, string) spriteAndFlag))
            {
                (scene as Level).Session.SetFlag(spriteAndFlag.Item2, true);
                Heart = new MachineHeart(Center, Center, spriteAndFlag.Item2, spriteAndFlag.Item1, true);
                scene.Add(Heart);
                Occupied = true;
            }
        }
        public override void Render()
        {
            base.Render();
            Heart?.Render();
        }
        public override void Update()
        {
            base.Update();
            if (!Occupied && Flag.GetFlag(Inverted) && Data != null && PianoModule.Session.HeartInventory.Collected.Count > 0)
            {
                foreach (var s in PianoModule.Session.HeartInventory.Collected)
                {
                    if (FlagArray.Contains(s.Value))
                    {
                        NextPair = s;
                        Talk.Enabled = true;
                        return;
                    }
                }
            }
            Talk.Enabled = false;
        }
        public void Interact(Player player)
        {
            if (!string.IsNullOrEmpty(NextPair.Key) && !string.IsNullOrEmpty(NextPair.Value))
            {
                PianoModule.Session.HeartInventory.Collected.Remove(NextPair.Key);
                Used.Add(id, (NextPair.Key, NextPair.Value));
                Occupied = true;
                Scene.Add(new Cutscene(this, player, NextPair));
            }
        }
        public class MachineHeart : Entity
        {
            public Vector2 Target;
            public bool Finished;
            public string Flag;
            public string SpritePath;
            public Sprite Sprite;
            public bool FromMachine;
            public MachineHeart(Vector2 center, Vector2 target, string flag, string sprite, bool fromMachine) : base()
            {
                SpritePath = sprite;
                Target = target;
                Tag = Tags.TransitionUpdate;
                Flag = flag;
                Collidable = false;
                Add(Sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/cutsceneHeart/"));
                Sprite.AddLoop("idle", SpritePath, 0.08f);
                Sprite.AddLoop("static", SpritePath, 1f, 0);
                Sprite.Play(fromMachine ? "static" : "idle");
                Sprite.CenterOrigin();
                Sprite.Position += Sprite.HalfSize();
                Collider = Sprite.Collider();
                FromMachine = fromMachine;
                Center = center;
                Sprite.OnLoop = (string s) =>
                {
                    if (Finished && s == "idle")
                    {
                        Sprite.Play("static");
                    }
                };
            }
            public override void Awake(Scene scene)
            {
                base.Awake(scene);
                if (!FromMachine)
                {
                    Tween.Set(this, Tween.TweenMode.Oneshot, 2, Ease.SineIn, t =>
                    {
                        Sprite.Scale = Vector2.Lerp(Vector2.Zero, Vector2.One, t.Eased);
                    });
                    Vector2 from = Center;
                    Tween.Set(this, Tween.TweenMode.Oneshot, 3, Ease.SineOut, t =>
                    {
                        Center = Vector2.Lerp(from, Target, t.Eased);

                    }, t => Finished = true);
                }
            }
        }
        public class Cutscene : CutsceneEntity
        {
            public HeartMachine Machine;
            public Player Player;
            public string SpritePath;
            public string Flag;
            public Cutscene(HeartMachine machine, Player player, KeyValuePair<string, string> heartData) : base()
            {
                SpritePath = heartData.Key;
                Flag = heartData.Value;
                Machine = machine;
                Player = player;
            }
            public override void OnBegin(Level level)
            {
                Player.DisableMovement();
                Add(new Coroutine(Sequence()));
            }

            public override void OnEnd(Level level)
            {
                Player.EnableMovement();
                Machine.FlagOnComplete.SetFlag(true);
            }
            public IEnumerator Sequence()
            {
                //maddy glows
                //one mini heart pops out of her
                //it orbits around her, slows to a stop, then slowly drifts into the center of the machine
                //it spins horizontally in the machine before heart dust spawns around it
                //each particle drifts to a mem blockade, before flashing and filling it's corresponding slot

                if (Scene is not Level level) yield break;
                MapData mapdata = level.Session.MapData;
                Dictionary<string, List<MemorySlotParticle>> particles = [];
                //spawn fake heart visuals
                MachineHeart heart = new MachineHeart(Player.Center, Machine.Center, Flag, SpritePath, false);
                Scene.Add(heart);
                yield return 1f;

                //wait for heart to get in position
                while (!heart.Finished) yield return null;
                Machine.AddHeart(heart);
                foreach (var slotPair in Data)
                {
                    if (slotPair.Key == heart.Flag)
                    {
                        //store references to the particles, grouped by the flag they are targeting
                        if (!particles.TryGetValue(heart.Flag, out List<MemorySlotParticle> value))
                        {
                            value = ([]);
                            particles.Add(heart.Flag, value);
                        }
                        int count = slotPair.Value.Count;
                        for (int i = 0; i < count; i++)
                        {
                            SlotData data2 = slotPair.Value[i];
                            Vector2 position = mapdata.Get(data2.Room).Position + data2.Offset;
                            //spawn particles around the heart
                            MemorySlotParticle p = new(heart.Center + PianoUtils.RotateAroundDeg(Vector2.UnitX * 4, Vector2.Zero, (360f / count) * i), data2, position, Color.LightBlue, Color.Blue);
                            Scene.Add(p);
                            value.Add(p);
                            yield return 0.1f;
                        }
                    }
                }
                yield return 1f;
                List<Coroutine> routines = new();
                foreach (var a in particles)
                {
                    Audio.Play("event:/PianoBoy/invertGlitch2", Center);
                    //send the particles out one by one to the blocks in the map
                    Coroutine routine = new Coroutine(Disperse(a.Value));
                    Add(routine);
                    routines.Add(routine);
                    yield return 0.5f;
                }
                //wait for the particles to finish moving
                foreach (var r in routines)
                {
                    while (!r.Finished) yield return null;
                }
                EndCutscene(Level);
            }
            public IEnumerator Disperse(List<MemorySlotParticle> list)
            {
                foreach (var p in list)
                {
                    p.Waiting = false;
                    yield return null;
                }
            }
            [Obsolete("Use Sequence instead")]
            public IEnumerator orig_Sequence()
            {
                yield break;
                //maddy glows
                //three mini hearts pop out of her
                //they orbit around her, slow to a stop, then slowly drift into the machine
                //they spin horizontally in the machine before shrinking, leaving behind heart dust
                //each particle drifts to a mem blockade, before flashing and causing the mem blockade to break

                /* if (Scene is not Level level) yield break;
                 MapData mapdata = level.Session.MapData;
                 Dictionary<string, List<MemorySlotParticle>> particles = [];
                 var data = Data;

                 if (data != null)
                 {
                     Vector2 centerTarget = Machine.TopCenter - Vector2.UnitY * 50;
                     float radius = 40;
                     List<KeyValuePair<string, string>> heartData = new();
                     foreach (var d in PianoModule.Session.HeartInventory.Collected)
                     {
                         if (PianoModule.Session.HeartInventory.InMachine.Contains(d)) continue;
                         heartData.Add(new(d.Key, d.Value));
                     }
                     PianoModule.Session.HeartInventory.Collected.Clear();
                     List<MachineHeart> hearts = [];
                     //spawn fake heart visuals
                     for (int i = 0; i < heartData.Count; i++)
                     {

                         for (int j = 0; j < Machine.Flags.Length; j++)
                         {
                             if (heartData[i].Value == Machine.Flags[j])
                             {
                                 Vector2 position = Machine.positions[j];
                                 MachineHeart heart = new MachineHeart(Player.Center, position, heartData[i].Value, heartData[i].Key);
                                 Scene.Add(heart);
                                 hearts.Add(heart);
                                 yield return 1f;
                             }
                         }
                     }
                     //wait for hearts to get in position
                     foreach (MachineHeart heart in hearts)
                     {
                         while (!heart.Finished) yield return null;
                         Machine.AddHeart(heart.Sprite, heart.SpritePath, heart.Flag);
                     }

                     foreach (var heart in hearts)
                     {
                         foreach (var slotPair in data)
                         {
                             if (slotPair.Key == heart.Flag)
                             {
                                 //store references to the particles, grouped by the flag they are targeting
                                 if (!particles.TryGetValue(heart.Flag, out List<MemorySlotParticle> value))
                                 {
                                     value = ([]);
                                     particles.Add(heart.Flag, value);
                                 }
                                 int count = slotPair.Value.Count;
                                 for (int i = 0; i < count; i++)
                                 {
                                     SlotData data2 = slotPair.Value[i];
                                     Vector2 position = mapdata.Get(data2.Room).Position + data2.Offset;
                                     //spawn particles around the heart
                                     MemorySlotParticle p = new(heart.Center + PianoUtils.RotateAround(Vector2.UnitX * 4, Vector2.Zero, (360f / count) * i), data2, position, Color.LightBlue, Color.Blue);
                                     Scene.Add(p);
                                     value.Add(p);
                                     yield return 0.1f;
                                 }
                             }
                         }
                     }
                     yield return 1f;
                     List<Coroutine> routines = new();
                     foreach (var a in particles)
                     {
                         Audio.Play("event:/PianoBoy/invertGlitch2", Center);
                         //send the particles out one by one to the blocks in the map
                         Coroutine routine = new Coroutine(Disperse(a.Value));
                         Add(routine);
                         routines.Add(routine);
                         yield return 0.5f;
                     }
                     //wait for the particles to finish moving
                     foreach (var r in routines)
                     {
                         while (!r.Finished) yield return null;
                     }
                 }
                 EndCutscene(Level);*/
            }
        }
    }

    [CustomEntity("PuzzleIslandHelper/MemoryBlockade")]
    [Tracked]
    public class MemoryBlockade : Solid
    {
        [Tracked]
        public class Slot : GraphicsComponent
        {
            public void SetFlag(Scene scene, bool value)
            {
                if (!string.IsNullOrEmpty(Flag))
                {
                    (scene as Level).Session.SetFlag("MemoryBlockadeSlot:" + Flag, value);
                }
            }
            public bool GetFlag(Scene scene)
            {
                if (!string.IsNullOrEmpty(Flag))
                {
                    return (scene as Level).Session.GetFlag("MemoryBlockadeSlot:" + Flag);
                }
                return false;
            }
            public string Path;
            public bool Filled;
            public int Width => FilledTex.Width;
            public int Height => FilledTex.Height;
            public MTexture FilledTex => GFX.Game[Path + "filled"];
            public MTexture EmptyTex => GFX.Game[Path + "empty"];
            public MTexture ShineTex => GFX.Game[Path + "shine"];
            private float shineAlpha;
            public float FillAmount;
            public string Flag;
            public Rectangle Bounds;
            public bool InRoutine;
            public Slot(string path, Color color, string flag) : base(true)
            {
                Path = path;
                Color = color;
                Flag = flag;
                Bounds.Width = 2;
                Bounds.Height = 2;
            }
            public override void Update()
            {
                base.Update();
                Vector2 p = RenderPosition;
                Bounds.X = (int)p.X;
                Bounds.Y = (int)p.Y;
            }
            public override void Added(Entity entity)
            {
                base.Added(entity);
                if (GetFlag(entity.Scene))
                {
                    Filled = true;
                }
                Vector2 p = RenderPosition;
                Bounds.X = (int)p.X;
                Bounds.Y = (int)p.Y;
            }
            public IEnumerator FillRoutine(float filltime, float shineintime, float shineouttime)
            {
                InRoutine = true;
                Filled = false;
                yield return PianoUtils.Lerp(Ease.CubeIn, filltime, f => FillAmount = f, true);
                yield return PianoUtils.Lerp(Ease.ExpoIn, shineintime, f => shineAlpha = f, true);
                yield return 0.1f;
                Filled = true;
                yield return PianoUtils.ReverseLerp(Ease.SineIn, shineouttime, f => shineAlpha = f, true);
                InRoutine = false;
            }
            public override void Render()
            {
                MTexture filled = FilledTex;
                MTexture empty = EmptyTex;
                MTexture shine = ShineTex;

                Vector2 p = RenderPosition;
                if (filled != null && empty != null && shine != null)
                {
                    if (FillAmount < 1)
                    {
                        empty.Draw(p, Origin, Color, Scale, Rotation, Effects);
                    }
                    if (Filled)
                    {
                        filled.Draw(p, Origin, Color, Scale, Rotation, Effects);
                    }
                    else if (FillAmount > 0)
                    {
                        int height = (int)(FillAmount * shine.Height);
                        MTexture subtex = shine.GetSubtexture(new Rectangle(0, shine.Height - height, shine.Width, height));
                        subtex.Draw(p + Vector2.UnitY * (shine.Height - height), Origin, Color, Scale, Rotation);
                    }
                    if (shineAlpha > 0)
                    {
                        shine.Draw(p, Origin, Color.White * shineAlpha, Scale, Rotation, Effects);
                    }
                }

            }
        }
        public int SlotCount;
        public List<Slot> Slots = [];
        public string Flag;
        private readonly char tileType;
        private Vector2 orig;
        private float moveAmount;
        public Vector2 Node;
        public bool Open
        {
            get
            {
                foreach (Slot s in Slots)
                {
                    if (!s.GetFlag(Scene))
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        public static List<string> GetSlotFlagData(string flagstring)
        {
            return
                [.. flagstring.
                Split(',').
                Select(item => item.Trim(' '))];
        }
        public MemoryBlockade(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, true)
        {
            Node = data.NodesOffset(offset)[0];
            orig = Position;
            tileType = data.Char("tiletype", '3');
            SurfaceSoundIndex = SurfaceIndex.TileToIndex[tileType];
            string texPath = data.Attr("path").TrimEnd('/') + '/';
            List<string> colors = data.Attr("colors").Replace(" ", "").Replace(",", "").Segment(6, false);
            List<string> flags = GetSlotFlagData(data.Attr("flags"));
            SlotCount = Math.Max(colors.Count, flags.Count);
            Slots = [];
            for (int i = 0; i < SlotCount; i++)
            {
                Color color = colors.Count > i ? Calc.HexToColor(colors[i]) : default;
                string flag = flags.Count > i ? flags[i] : "";
                Slots.Add(new Slot(texPath, color, flag));
            }
            Tag |= Tags.TransitionUpdate;
            Flag = data.Attr("requiredFlag");
        }
        public void SnapState(bool open)
        {
            foreach (Slot s in Slots)
            {
                s.SetFlag(Scene, open);
                s.Filled = open;
                moveAmount = open ? 1 : 0;
                MoveToY(open ? Node.Y : orig.Y);
                s.FillAmount = 0;
            }
        }
        public override void Update()
        {
            base.Update();
            foreach (Slot slot in Slots)
            {
                if (slot.InRoutine) return;
            }
            if (Open)
            {
                if (moveAmount != 1)
                {
                    moveAmount = Calc.Approach(moveAmount, 1, Engine.DeltaTime);
                }
            }
            else if (moveAmount != 0)
            {
                moveAmount = Calc.Approach(moveAmount, 0, Engine.DeltaTime);
            }
            MoveToY(Calc.LerpClamp(orig.Y, Node.Y, Ease.SineInOut(moveAmount)));
        }
        public void SetSlotPositions()
        {
            if (SlotCount == 0) return;

            float baseX = Slots[0].Width / 2;
            float iw = Slots[0].Width + 2;
            float ih = Slots[0].Height + 2;
            int count = 0;
            float y = 4;
            while (count < Slots.Count)
            {
                for (float i = baseX; i < Width - iw; i += iw)
                {
                    if (count >= Slots.Count) break;
                    Slots[count].X = i;
                    Slots[count].Y = y;
                    count++;
                }
                y += ih;
            }
            float top = int.MaxValue, bottom = int.MinValue;
            if (Slots.Count > 0)
            {
                foreach (Slot slot in Slots)
                {
                    top = Math.Min(top, slot.Y);
                    bottom = Math.Max(bottom, slot.Y + slot.Height);
                }
                float height = bottom - top;
                foreach (Slot slot in Slots)
                {
                    slot.Y += (Height - height) / 4;
                }
            }
        }
        public void Fill(params string[] flags)
        {
            Add(new Coroutine(FillSequence(flags)));
        }
        public IEnumerator FillSequence(params string[] flags)
        {
            float wait = 0.2f;
            float decrement = 0.005f;
            float min = Engine.DeltaTime;
            foreach (Slot s in Slots.Where(slot => flags.Contains(slot.Flag) && !slot.Filled && !slot.InRoutine))
            {
                s.SetFlag(Scene, true);
                Add(new Coroutine(s.FillRoutine(0.7f, 0.6f, 1.4f)));
                yield return wait;
                wait = Math.Max(min, wait - decrement);
            }
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            TileGrid grid = PianoUtils.GetTileGridBox(Width, Height, tileType);
            Add(grid);
            Add(new TileInterceptor(grid, false));
            Add(new LightOcclude());
            foreach (Slot s in Slots)
            {
                Add(s);
            }
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (Open)
            {
                SnapState(true);
            }
            SetSlotPositions();

        }
    }
    [Obsolete("Use HeartMachine instead")]
    public class MultiHeartMachine : Entity
    {
        public DotX3 Talk;
        public string FlagOnComplete;
        public string Flag;
        public bool Inverted;
        public string[] Flags;
        public int HeartsCollected;
        public static Dictionary<string, Dictionary<string, List<SlotData>>> DataPerArea => PianoMapDataProcessor.SlotData;
        public static Dictionary<string, List<SlotData>> Data;
        public List<Vector2> positions = [];
        private List<Sprite> sprites = [];
        private Image image;
        public MultiHeartMachine(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
        {
            Depth = 2;
            image = new Image(GFX.Game["objects/PuzzleIslandHelper/heart/machine"]);
            Add(image);
            Collider = image.Collider();
            Flags = data.Attr("flags").Replace(" ", "").Split(',');
            FlagOnComplete = data.Attr("flagOnComplete");
            Flag = data.Attr("flag");
            Inverted = data.Bool("inverted");
            Add(Talk = new DotX3(Collider, Interact));
        }
        public void AddHeart(MachineHeart heart)
        {
            PianoModule.Session.HeartInventory.InMachine.Add(heart.SpritePath, heart.Flag);
            sprites.Add(heart.Sprite);
            Add(heart.Sprite);
            heart.Sprite.RenderPosition = Center - heart.Sprite.HalfSize();
        }
        public override void Render()
        {
            image.Render();
            for (int i = 0; i < positions.Count; i++)
            {
                Draw.Rect(Center + positions[i] - Vector2.One * 6, 12, 12, Color.Magenta);
            }
            foreach (Sprite s in sprites)
            {
                s.Render();
            }
        }
        public string FlagString
        {
            get
            {
                string output = "";
                foreach (string s in Flags)
                {
                    output += s + " ";
                }
                return output;
            }
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (DataPerArea.TryGetValue(scene.GetAreaKey(), out var value))
            {
                Data = value;
            }
            if (Flags.Length > 0)
            {
                for (int i = 0; i < 360; i += 360 / Flags.Length)
                {
                    positions.Add(PianoUtils.RotateAroundDeg(-Vector2.UnitY * Height / 4f, Vector2.Zero, i));
                }
            }
            int count = 0;
            foreach (KeyValuePair<string, string> pair in PianoModule.Session.HeartInventory.InMachine)
            {
                if (count >= Flags.Length) break;
                if (Flags.Contains(pair.Key))
                {
                    Sprite sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/cutsceneHeart/");
                    sprite.AddLoop("idle", pair.Key, 0.08f);
                    sprite.AddLoop("static", pair.Key, 1f, 0);
                    sprite.Play("idle");
                    sprite.CenterOrigin();
                    sprite.Position = Collider.HalfSize + positions[count];
                    sprites.Add(sprite);
                    Add(sprite);
                    count++;
                }
            }
        }
        public override void Update()
        {
            base.Update();
            Talk.Enabled = Flag.GetFlag(Inverted);
        }
        public void Interact(Player player)
        {
            //Scene.Add(new Cutscene(this, player));
        }
        public class MachineHeart : Entity
        {
            public Vector2 Target;
            public bool Finished;
            public string Flag;
            public string SpritePath;
            public Sprite Sprite;
            public MachineHeart(Vector2 position, Vector2 target, string flag, string sprite) : base(position)
            {
                SpritePath = sprite;
                Target = target;
                Tag = Tags.TransitionUpdate;
                Flag = flag;
                Collidable = false;
                Add(Sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/cutsceneHeart/"));
                Sprite.AddLoop("idle", SpritePath, 0.08f);
                Sprite.AddLoop("static", SpritePath, 1f, 0);
                Sprite.Play("idle");
                Sprite.CenterOrigin();
                Sprite.Position += Sprite.HalfSize();
                Collider = Sprite.Collider();
            }
            public override void Awake(Scene scene)
            {
                base.Awake(scene);
                Tween.Set(this, Tween.TweenMode.Oneshot, 2, Ease.SineIn, t =>
                {
                    Sprite.Scale = Vector2.Lerp(Vector2.Zero, Vector2.One, t.Eased);
                });
                Vector2 from = Center;
                Tween.Set(this, Tween.TweenMode.Oneshot, 3, Ease.SineOut, t =>
                {
                    Center = Vector2.Lerp(from, Target, t.Eased);
                    Sprite.Scale = Vector2.Lerp(Vector2.Zero, Vector2.One, t.Eased);

                }, t => Finished = true);
            }
        }
        public class Cutscene : CutsceneEntity
        {
            public HeartMachine Machine;
            public Player Player;
            public Cutscene(HeartMachine machine, Player player) : base()
            {
                Machine = machine;
                Player = player;
            }
            public override void OnBegin(Level level)
            {
                Player.DisableMovement();
                Add(new Coroutine(Sequence()));
            }

            public override void OnEnd(Level level)
            {
                Player.EnableMovement();
                Machine.FlagOnComplete.SetFlag(true);
            }

            public IEnumerator Sequence()
            {
                //maddy glows
                //three mini hearts pop out of her
                //they orbit around her, slow to a stop, then slowly drift into the machine
                //they spin horizontally in the machine before shrinking, leaving behind heart dust
                //each particle drifts to a mem blockade, before flashing and causing the mem blockade to break

                if (Scene is not Level level) yield break;
                MapData mapdata = level.Session.MapData;
                Dictionary<string, List<MemorySlotParticle>> particles = [];
                var data = Data;

                if (data != null)
                {
                    Vector2 centerTarget = Machine.TopCenter - Vector2.UnitY * 50;
                    float radius = 40;
                    List<KeyValuePair<string, string>> heartData = new();
                    foreach (var d in PianoModule.Session.HeartInventory.Collected)
                    {
                        if (PianoModule.Session.HeartInventory.InMachine.Contains(d)) continue;
                        heartData.Add(new(d.Key, d.Value));
                    }
                    PianoModule.Session.HeartInventory.Collected.Clear();
                    List<MachineHeart> hearts = [];
                    //spawn fake heart visuals
                    for (int i = 0; i < heartData.Count; i++)
                    {

                        for (int j = 0; j < Machine.FlagArray.Length; j++)
                        {
                            if (heartData[i].Value == Machine.FlagArray[j])
                            {
                                //Vector2 position = Machine.positions[j];
                                MachineHeart heart = new MachineHeart(Player.Center, Vector2.Zero, heartData[i].Value, heartData[i].Key);
                                Scene.Add(heart);
                                hearts.Add(heart);
                                yield return 1f;
                            }
                        }
                    }
                    //wait for hearts to get in position
                    foreach (MachineHeart heart in hearts)
                    {
                        while (!heart.Finished) yield return null;
                        //Machine.AddHeart(heart);
                    }

                    foreach (var heart in hearts)
                    {
                        foreach (var slotPair in data)
                        {
                            if (slotPair.Key == heart.Flag)
                            {
                                //store references to the particles, grouped by the flag they are targeting
                                if (!particles.TryGetValue(heart.Flag, out List<MemorySlotParticle> value))
                                {
                                    value = ([]);
                                    particles.Add(heart.Flag, value);
                                }
                                int count = slotPair.Value.Count;
                                for (int i = 0; i < count; i++)
                                {
                                    SlotData data2 = slotPair.Value[i];
                                    Vector2 position = mapdata.Get(data2.Room).Position + data2.Offset;
                                    //spawn particles around the heart
                                    MemorySlotParticle p = new(heart.Center + PianoUtils.RotateAroundDeg(Vector2.UnitX * 4, Vector2.Zero, (360f / count) * i), data2, position, Color.LightBlue, Color.Blue);
                                    Scene.Add(p);
                                    value.Add(p);
                                    yield return 0.1f;
                                }
                            }
                        }
                    }
                    yield return 1f;
                    List<Coroutine> routines = new();
                    foreach (var a in particles)
                    {
                        Audio.Play("event:/PianoBoy/invertGlitch2", Center);
                        //send the particles out one by one to the blocks in the map
                        Coroutine routine = new Coroutine(Disperse(a.Value));
                        Add(routine);
                        routines.Add(routine);
                        yield return 0.5f;
                    }
                    //wait for the particles to finish moving
                    foreach (var r in routines)
                    {
                        while (!r.Finished) yield return null;
                    }
                }
                EndCutscene(Level);
            }
            public IEnumerator Disperse(List<MemorySlotParticle> list)
            {
                foreach (var p in list)
                {
                    p.Waiting = false;
                    yield return null;
                }
            }
        }
    }
}