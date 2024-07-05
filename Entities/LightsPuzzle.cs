using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.PuzzleIslandHelper.Triggers;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{


    public class LightMachineInfo : Entity
    {
        public static bool HasBeenSeen;
        public static Color[] PreviousColors = new Color[4];
        public LightMachineInfo()
        {
            Tag = Tags.Global;
        }
        public static void Load()
        {
            if (!HasBeenSeen)
            {
                for (int i = 0; i < 4; i++)
                {
                    PreviousColors[i] = Color.White;
                    HasBeenSeen = true;
                }
            }
        }
    }
    [CustomEntity("PuzzleIslandHelper/LightsPuzzle")]
    [Tracked]
    public class LightsPuzzle : Entity
    {
        public static LightsIcon[] Icons = new LightsIcon[4];
        private Player player;
        private readonly Color[] Answers = { Color.Red, Color.Yellow, Color.Blue, Color.Green };
        private List<WindowDecal> Windows = new();
        private string flag;
        private bool GotSolution
        {
            get
            {
                return LightPositionsCorrect && AllNodesUsed;
            }
        }
        private bool LightPositionsCorrect
        {
            get
            {
                return Answers[0] == IconHolder.SetColors[0] && Answers[1] == IconHolder.SetColors[1] && Answers[2] == IconHolder.SetColors[2] && Answers[3] == IconHolder.SetColors[3];
            }
        }
        private bool AllNodesUsed
        {
            get
            {
                bool result = true;
                for (int i = 0; i < 4; i++)
                {
                    if (!IconHolder.NodeUsed[i])
                    {
                        result = false;
                    }
                }
                return result && IconHolder.LeverState;
            }
        }
        public static Color[] SequenceColor = new Color[4];
        public bool doneSequence;
        public LightsPuzzle(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Depth = 2;
            Tag = Tags.TransitionUpdate;

            flag = data.Attr("flagOnComplete");
        }
        public void Reset()
        {
            for (int i = 0; i < 4; i++)
            {
                Windows[i].Color = Color.White;
            }
        }
        private IEnumerator Sequence()
        {
            doneSequence = true;
            yield return null;
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);

            foreach (WindowDecal window in (scene as Level).Tracker.GetEntities<WindowDecal>())
            {
                if (window.CustomTag == "lightPuzzle")
                {
                    Windows.Add(window);
                    Console.WriteLine("BeenAdded");
                }
            }
            Windows.OrderBy(window => window.Position.X);
            for (int i = 0; i < Windows.Count; i++)
            {
                if (i < LightMachineInfo.PreviousColors.Length)
                {
                    //Windows[i].From = LightMachineInfo.PreviousColors[i];
                }
            }
            foreach (WindowDecal window in Windows)
            {
                Console.WriteLine(window.Color);
            }
            player = Scene.Tracker.GetEntity<Player>();

        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            foreach (WindowDecal window in Windows)
            {
                window.Render();
                Draw.Rect(window.Collider, Color.Green);
            }
        }
        public static void SetSequence()
        {
            for (int i = 0; i < 4; i++)
            {
                if (SequenceColor[i] != null)
                {
                    SequenceColor[i] = Color.White;
                }

            }
            foreach (LightsIcon icon in Icons)
            {
                if (icon is not null)
                {
                    switch (icon.SequenceNumber)
                    {
                        case 1:
                            SequenceColor[0] = icon.Color;
                            break;
                        case 2:
                            SequenceColor[1] = icon.Color;
                            break;
                        case 3:
                            SequenceColor[2] = icon.Color;
                            break;
                        case 4:
                            SequenceColor[3] = icon.Color;
                            break;
                    }
                }
            }
        }

        public override void Update()
        {
            base.Update();
            if (doneSequence)
            {
                return;
            }
            UpdateColors();

            if (GotSolution)
            {
                Add(new Coroutine(Sequence()));
                SceneAs<Level>().Session.SetFlag("lightPuzzleComplete");
            }

        }
        public void UpdateColors()
        {
            for (int i = 0; i < 4; i++)
            {
                Windows[i].Color = IconHolder.SetColors[i];
            }
        }
    }

    [CustomEntity("PuzzleIslandHelper/IconHolder")]
    [Tracked]
    public class IconHolder : Entity
    {
        private List<LightsIcon> icons = new List<LightsIcon>();
        private Sprite sprite;
        public ColliderList ColliderList = new ColliderList();
        private Vector2[] Nodes = new Vector2[4];
        public static bool[] NodeUsed = new bool[4];

        public static bool PulledLever = false;
        public static int CurrentState;
        public static Color[] SetColors = { Color.White, Color.White, Color.White, Color.White };

        public static bool LeverState = false;

        public IconHolder(EntityData data, Vector2 offset)
        : base(data.Position + offset)
        {
            for (int i = 0; i < 4; i++)
            {
                Collider collider = new Hitbox(12, 12, 20 + i * 15, 21);
                ColliderList.Add(collider);
                Nodes[i] = ColliderList.colliders[i].Position + new Vector2(collider.Width / 2, collider.Height);
            }
            Add(sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/chandelier/"));
            sprite.AddLoop("leverUp", "mainModule", 0.1f, 0);
            sprite.AddLoop("leverDown", "mainModule", 0.1f, 6);
            sprite.Add("flipDown", "mainModule", 0.1f, "leverDown");
            sprite.Add("flipUp", "mainModuleReverse", 0.1f, "leverUp");
            sprite.Add("stuck", "mainModuleStuck", 0.08f, "leverDown");
            sprite.Play("leverDown");
            Depth = 2;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);


            int x, y, width, height;
            x = 4;
            y = 22;
            width = 15;
            height = 21;
            Add(new VertexLight(new Vector2(x + 3, y + 4), Color.White, PianoModule.Session.RestoredPower ? 1 : 0.7f, 10, 20));
            Rectangle lRect = new Rectangle(x, y, width, height);
            Add(new TalkComponent(lRect, new Vector2(x + width / 2 - 0.5f, y - 7), LeverInteract));
            Rectangle bRect = new Rectangle(-3, 9, 8, 4);
        }
        private void LeverInteract(Player player)
        {
            if (PianoModule.Session.RestoredPower)
            {
                if (sprite.CurrentAnimationID == "leverUp")
                {
                    sprite.Play("flipDown");
                    LeverState = false;
                }
                else
                {
                    sprite.Play("flipUp");
                    LeverState = true;
                }
            }
            else
            {
                sprite.Play("stuck");
                LeverState = false;
            }
            foreach (LightsIcon icon in SceneAs<Level>().Tracker.GetEntities<LightsIcon>())
            {
                if (icon.IsSet)
                {
                    icon.DisableUntilRelease = true;
                    icon.IsSet = false;
                    icon.Speed = Vector2.Zero;
                }
            }
        }
        public void Reset()
        {
            foreach (LightsIcon icon in icons)
            {
                icon.IsSet = false;
            }
            for (int i = 0; i < 4; i++)
            {
                SetColors[i] = Color.White;
            }

            if (sprite.CurrentAnimationID == "leverUp")
            {
                sprite.Play("flipDown");
                LeverState = false;
            }
            foreach (LightsIcon icon in SceneAs<Level>().Tracker.GetEntities<LightsIcon>())
            {
                if (icon.IsSet)
                {
                    icon.DisableUntilRelease = true;
                    icon.IsSet = false;
                    icon.Speed = Vector2.Zero;
                }
            }

        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Collider = ColliderList;
            int num = 0;
            foreach (LightsIcon icon in scene.Tracker.GetEntities<LightsIcon>())
            {
                if (num < 4)
                {
                    LightsPuzzle.Icons[num] = icon;
                    num++;
                }
            }

            LightsPuzzle.SetSequence();
        }
        public override void Update()
        {
            base.Update();
            CheckColliders();
        }

        public void ManageColors(LightsIcon icon)
        {
            if (icon.IsSet)
            {
                SetColors[icon.OccupiedNode] = icon.Color;
            }
            for (int i = 0; i < 4; i++)
            {
                if (!NodeUsed[i])
                {
                    SetColors[i] = Color.White;
                }
            }
        }
        private void CheckColliders()
        {
            if (LeverState)
            {
                foreach (LightsIcon icon in Scene.Tracker.GetEntities<LightsIcon>().Cast<LightsIcon>())
                {
                    if (CollideCheck(icon) && !icon.Hold.IsHeld)
                    {
                        SetIcon(icon);
                        HandleIdenticalPositions(icon);
                    }
                }
            }
        }
        public void HandleIdenticalPositions(LightsIcon icon)
        {
            foreach (LightsIcon otherIcon in SceneAs<Level>().Tracker.GetEntities<LightsIcon>())
            {
                if (!icon.Equals(otherIcon))
                {
                    if (otherIcon.IsSet && icon.OccupiedNode == otherIcon.OccupiedNode && icon.OccupiedNode != -1)
                    {
                        icon.IsSet = false;
                    }
                }
            }
        }
        public void SetNodePosition(LightsIcon icon)
        {
            if (!icon.IsSet && !icon.DisableUntilRelease)
            {
                icon.IsSet = true;
                Vector2[] vectors = new Vector2[4];
                for (int i = 0; i < 4; i++)
                {
                    vectors[i] = ColliderList.colliders[i].AbsolutePosition + new Vector2(icon.Width / 2, icon.Height);
                }
                icon.SetPosition = vectors.ClosestTo(icon.Position);
                for (int i = 0; i < 4; i++)
                {
                    if (vectors[i] == icon.SetPosition)
                    {
                        icon.OccupiedNode = i;
                        NodeUsed[i] = true;
                        ManageColors(icon);
                        break;
                    }
                }
            }
        }
        public void SetIcon(LightsIcon icon)
        {
            if (!icon.IsSet)
            {
                icons.Add(icon);
                SetNodePosition(icon);
            }
        }
        public void SetOnGround()
        {
            if (Scene as Level is not null)
            {
                Collider = new Hitbox(sprite.Width, sprite.Height);
                try
                {
                    while (!CollideCheck<SolidTiles>())
                    {
                        Position.Y += 8;
                    }
                }
                catch
                {
                    Console.WriteLine($"{this} could not find any SolidTiles below it to set it's Y Position to");
                }
                Position.Y -= 8;
                Position.X += 20 - sprite.Width / 2 - 8;
            }
        }
    }


    [CustomEntity("PuzzleIslandHelper/LightsIcon")]
    [Tracked]
    public class LightsIcon : Actor
    {
        public Color Color;
        private float TimePassed;
        private Hitbox HoldingHitbox;
        private Hitbox IdleHitbox;
        private bool isSet;
        private bool Collided;
        public bool IsSet
        {
            get
            {
                return isSet;
            }
            set
            {
                if (!value)
                {
                    rate = 0;
                    if (OccupiedNode != -1)
                    {
                        IconHolder.SetColors[OccupiedNode] = Color.White;
                        IconHolder.NodeUsed[OccupiedNode] = false;
                    }

                    OccupiedNode = -1;
                    SetPosition = Vector2.Zero;
                    noGravityTimer = 0;
                    Collider = HoldingHitbox;
                }
                else
                {
                    Collider = IdleHitbox;
                }
                isSet = value;
            }
        }

        public int SequenceNumber;
        public int OccupiedNode = -1;
        public Vector2 SetPosition;
        private float rate = 0;
        public bool DisableUntilRelease = true;
        private EntityID id;
        #region Variables
        public Vector2 prevPosition = Vector2.Zero;
        private float noGravityTimer;
        private float swatTimer;
        private float hardVerticalHitSoundCooldown;

        private static Vector2 Justify = new Vector2(0.5f, 1f);
        public Vector2 Speed;
        private Vector2 prevLiftSpeed;
        public Sprite sprite;
        public Holdable Hold;
        public HoldableCollider hitSeeker;
        public VertexLight Light;
        private Collision onCollideV;
        private Collision onCollideH;

        private Player player;
        #endregion
        public struct LightsIconData
        {
            public bool SavedState;
            public Vector2 SavedPosition;
            public Color SavedColor;
            public int SavedNumber;
            public EntityID SavedID;
            public LightsIconData(bool state, Vector2 position, Color color, int number, EntityID id)
            {
                SavedState = state;
                SavedPosition = position;
                SavedColor = color;
                SavedNumber = number;
                SavedID = id;
            }
        }


        public LightsIcon(EntityData data, Vector2 offset, EntityID id) : this(data.Position + offset)
        {
            this.id = id;
            SequenceNumber = data.Int("icon");
            Color = data.HexColor("color");
            Position += Vector2.One * 8;
        }

        public LightsIcon(LightsIconData data) : this(data.SavedPosition)
        {
            SequenceNumber = data.SavedNumber;
            Color = data.SavedColor;
            SetPosition = data.SavedPosition;
            IsSet = data.SavedState;
            id = data.SavedID;
        }

        public LightsIcon(Vector2 position) : base(position) { }
        #region Hook

        private static void LightsIconLoad(Level level, Player.IntroTypes intro, bool fromLoader)
        {
            Dictionary<string, List<LightsIconData>> dict = PianoModule.Session.IconDictionary;
            if (dict.TryGetValue(level.Session.Level, out var datas))
            {
                foreach (LightsIconData data in datas)
                {
                    level.Add(new LightsIcon(data));
                }
                dict[level.Session.Level].Clear();
            }
        }
        internal static void Load()
        {
            Everest.Events.Level.OnLoadLevel += LightsIconLoad;
        }
        internal static void Unload()
        {
            Everest.Events.Level.OnLoadLevel -= LightsIconLoad;
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Dictionary<string, List<LightsIconData>> dict = PianoModule.Session.IconDictionary;
            LightsIconData data = new LightsIconData(IsSet, Position, Color, SequenceNumber, id);
            string room = (scene as Level).Session.Level;
            if (!dict.ContainsKey(room))
            {
                dict[room] = new();
            }

            if (!Collided)
            {
                dict[room].Add(data);
            }
            else
            {
                (scene as Level).Session.DoNotLoad.Remove(id);
            }
        }
        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            Dictionary<string, List<LightsIconData>> dict = PianoModule.Session.IconDictionary;
            LightsIconData data = new LightsIconData(IsSet, Position, Color, SequenceNumber, id);
            string room = (scene as Level).Session.Level;
            if (!dict.ContainsKey(room))
            {
                dict[room] = new();
            }
            if (!Collided)
            {
                dict[room].Add(data);
            }
            else
            {
                (scene as Level).Session.DoNotLoad.Remove(id);
            }
        }
        #endregion

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Collided = false;
            Add(sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/chandelier/icons/"));
            sprite.AddLoop("idle", "default", 0.1f, 0);
            sprite.SetColor(Color);
            sprite.Justify = Justify;
            sprite.JustifyOrigin(Justify);

            Add(Hold = new Holdable(0.5f));

            #region Hold

            HoldingHitbox = new Hitbox(sprite.Width - 4, sprite.Height, 2 - sprite.Width * Justify.X, -sprite.Height * Justify.Y);
            IdleHitbox = new Hitbox(sprite.Width, sprite.Height, -sprite.Width * Justify.X, -sprite.Height * Justify.Y);
            Collider = IdleHitbox;
            Hold.PickupCollider = new Hitbox(sprite.Width, sprite.Height, -sprite.Width * Justify.X, -sprite.Height * Justify.Y);
            Hold.SpeedSetter = delegate (Vector2 speed)
            {
                Speed = speed;
            };
            Hold.SlowFall = false;
            Hold.SlowRun = false;
            Hold.OnPickup = OnPickup;
            Hold.OnRelease = OnRelease;
            Hold.OnHitSpring = HitSpring;
            Hold.SpeedGetter = () => Speed;
            onCollideH = OnCollideH;
            onCollideV = OnCollideV;
            LiftSpeedGraceTime = 0.1f;
            #endregion
            Tag = Tags.Persistent;
            Add(Light = new VertexLight(Collider.Center, Color.White, 0.7f, 32, 64));
            Add(new MirrorReflection());
            sprite.Play("idle");
            SceneAs<Level>().Session.DoNotLoad.Add(id);
        }
        #region Finished Methods
        private void OnCollideV(CollisionData data)
        {
            if (data.Hit is DashSwitch)
            {
                (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitY * Math.Sign(Speed.Y));
            }
            if (Speed.Y > 0f && TimePassed > 1)
            {
                if (hardVerticalHitSoundCooldown <= 0f)
                {

                    Audio.Play("event:/PianoBoy/stool_hit_ground", Position, "crystal_velocity", Calc.ClampedMap(Speed.Y, 0f, 200f));
                    hardVerticalHitSoundCooldown = 0.5f;
                }
                else
                {
                    Audio.Play("event:/PianoBoy/stool_hit_ground", Position, "crystal_velocity", 0f);
                }
            }

            if (Speed.Y > 140f && !(data.Hit is SwapBlock) && !(data.Hit is DashSwitch))
            {
                Speed.Y *= -0.6f;
            }
            else
            {
                Speed.Y = 0f;
            }
        }
        private void OnCollideH(CollisionData data)
        {
            if (data.Hit is DashSwitch)
            {
                (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitX * Math.Sign(Speed.X));
            }
            Audio.Play("event:/PianoBoy/stool_hit_side", Position);
            Speed.X *= -0.4f;
        }
        private void OnPickup()
        {
            Audio.Play("event:/PianoBoy/piano/piano-keys-a/B6");
            Collider = HoldingHitbox;
            IsSet = false;
            Speed = Vector2.Zero;
        }
        public bool HitSpring(Spring spring)
        {
            if (!Hold.IsHeld)
            {
                if (spring.Orientation == Spring.Orientations.Floor && Speed.Y >= 0f)
                {
                    Speed.X *= 0.5f;
                    Speed.Y = -160f;
                    noGravityTimer = 0.15f;
                    return true;
                }
                if (spring.Orientation == Spring.Orientations.WallLeft && Speed.X <= 0f)
                {
                    MoveTowardsY(spring.CenterY + 5f, 4f);
                    Speed.X = 220f;
                    Speed.Y = -80f;
                    noGravityTimer = 0.1f;
                    return true;
                }
                if (spring.Orientation == Spring.Orientations.WallRight && Speed.X >= 0f)
                {
                    MoveTowardsY(spring.CenterY + 5f, 4f);
                    Speed.X = -220f;
                    Speed.Y = -80f;
                    noGravityTimer = 0.1f;
                    return true;
                }
            }
            return false;
        }
        public void OnRelease(Vector2 force)
        {
            DisableUntilRelease = false;
            //Very jank solution to clipping
            //Add(new Coroutine(WaitThenSwitch()));
            if (force.X != 0f && force.Y == 0f)
            {
                force.Y = -0.4f;
            }
            Speed = force * 200f;
            if (Speed != Vector2.Zero)
            {
                noGravityTimer = 0.1f;
            }
        }
        #endregion
        public override void Update()
        {
            base.Update();
            Level level = Scene as Level;
            if (!PianoModule.Session.RestoredPower && level is not null)
            {
                if (level.Session.Level.Contains("ruinsLab"))
                {
                    Light.Alpha = 0.3f;
                }
                else
                {
                    Light.Alpha = 1;
                }
            }
            else
            {
                Light.Alpha = 1;
            }
            TimePassed += Engine.DeltaTime;
            Collided = CollideCheck<ResetHoldableTrigger>();
            Hold.CheckAgainstColliders();
            player = Scene.Tracker.GetEntity<Player>();
            if (player is null || Scene as Level is null)
            {
                return;
            }
            if (OnGround())
            {
                DisableUntilRelease = false;
            }
            ManagePosition();
            Depth = player.Depth + 1;
            #region Copied
            if (swatTimer > 0f)
            {
                swatTimer -= Engine.DeltaTime;
            }
            hardVerticalHitSoundCooldown -= Engine.DeltaTime;
            if (Hold.IsHeld)
            {
                prevLiftSpeed = Vector2.Zero;
            }
            else
            {
                if (OnGround())
                {
                    float target = !OnGround(Position + Vector2.UnitX * 3f) ? 20f : OnGround(Position - Vector2.UnitX * 3f) ? 0f : -20f;
                    Speed.X = Calc.Approach(Speed.X, target, 800f * Engine.DeltaTime);
                    Vector2 liftSpeed = LiftSpeed;
                    if (liftSpeed == Vector2.Zero && prevLiftSpeed != Vector2.Zero)
                    {
                        Speed = prevLiftSpeed;
                        prevLiftSpeed = Vector2.Zero;
                        Speed.Y = Math.Min(Speed.Y * 0.6f, 0f);
                        if (Speed.X != 0f && Speed.Y == 0f)
                        {
                            Speed.Y = -60f;
                        }
                        if (Speed.Y < 0f)
                        {
                            noGravityTimer = 0.15f;
                        }
                    }
                    else
                    {
                        prevLiftSpeed = liftSpeed;
                        if (liftSpeed.Y < 0f && Speed.Y < 0f)
                        {
                            Speed.Y = 0f;
                        }
                    }
                }
                else if (Hold.ShouldHaveGravity)
                {
                    float num = 800f;
                    if (Math.Abs(Speed.Y) <= 30f)
                    {
                        num *= 0.5f;
                    }
                    float num2 = 350f;
                    if (Speed.Y < 0f)
                    {
                        num2 *= 0.5f;
                    }
                    Speed.X = Calc.Approach(Speed.X, 0f, num2 * Engine.DeltaTime);
                    if (noGravityTimer > 0f)
                    {
                        noGravityTimer -= Engine.DeltaTime;
                    }
                    else
                    {
                        Speed.Y = Calc.Approach(Speed.Y, 200f, num * Engine.DeltaTime);
                    }
                }
                MoveH(Speed.X * Engine.DeltaTime, onCollideH);
                MoveV(Speed.Y * Engine.DeltaTime, onCollideV);
                Player entity = Scene.Tracker.GetEntity<Player>();
                TempleGate templeGate = CollideFirst<TempleGate>();
                if (templeGate != null && entity != null)
                {
                    templeGate.Collidable = false;
                    MoveH(Math.Sign(entity.X - X) * 32 * Engine.DeltaTime);
                    templeGate.Collidable = true;
                }
            }
            if (hitSeeker != null && swatTimer <= 0f && !hitSeeker.Check(Hold))
            {
                hitSeeker = null;
            }
            #endregion

        }
        public void ManagePosition()
        {
            if (IsSet)
            {
                Position = Calc.Approach(Position, SetPosition, rate += Engine.DeltaTime);
                Hold.SetSpeed(Vector2.Zero);
                noGravityTimer = Engine.DeltaTime;
            }
        }
    }
}