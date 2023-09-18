using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMOD;
using Celeste.Mod.PuzzleIslandHelper.Triggers;
/*
* TODO: 
*       Sfx:
*       Lever (moving), 
*       Button(clicking),
*       Machine(rattling,whirring,shutters opening/closing)
*       
* Tracked Pattern should be: 1 -> Front
*                            2 -> Right
*                            3 -> Back
*                            4 -> Left
*/
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
        private Sprite Machine;
        private Sprite LeftLight;
        private Sprite RightLight;
        private Sprite square;
        private Player player;
        private Sprite State;
        private string flag;
        private bool GotSolution
        {
            get
            {
                return NodePositionsCorrect && LightPositionsCorrect && AllNodesUsed;
            }
        }

        private bool HitButton; //TODO ADD BUTTON TO HIT 
        private bool NodePositionsCorrect
        {
            get
            {
                bool result = true;
                if (Icons.Length != 4)
                {
                    result = false;
                }
                for (int i = 0; i < 4; i++)
                {
                    if (IconHolder.SetColors[i] != SequenceColor[i])
                    {
                        result = false;
                    }
                }
                return result && IconHolder.LeverState;
            }
        }
        private bool LightPositionsCorrect
        {
            get
            {
                return square.Color == IconHolder.SetColors[0] && RightLight.Color == IconHolder.SetColors[1] && LeftLight.Color == IconHolder.SetColors[3];
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
        private static int CurrentNode = 0;
        private static int LeftNode = 3;
        private static int RightNode = 1;
        private static int BackNode = 2;
        public static Color[] NodeColor = new Color[4];
        public static Color[] SequenceColor = new Color[4];
        private VertexLight VertexLight;
        public static bool ButtonRoutine = false;
        public static int[] SequenceHeld = new int[4];
        private bool inSequence;
        public static bool doneSequence;
        public LightsPuzzle(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Tag = Tags.TransitionUpdate;

            flag = data.Attr("flagOnComplete");
            Add(square = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/chandelier/"));
            square.AddLoop("idle", "square", 1f);
            Add(LeftLight = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/chandelier/"));
            LeftLight.AddLoop("on", "sideLight", 0.1f);
            LeftLight.AddLoop("off", "lightClose", 1, 1);
            LeftLight.Add("open", "lightOpen", 0.1f, "on");
            LeftLight.Add("close", "lightClose", 0.1f, "off");
            Add(RightLight = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/chandelier/"));
            RightLight.AddLoop("on", "sideLight", 0.1f);
            RightLight.AddLoop("off", "lightClose", 1, 1);
            RightLight.Add("open", "lightOpen", 0.1f, "on");
            RightLight.Add("close", "lightClose", 0.1f, "off");
            RightLight.FlipX = true;
            Add(Machine = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/chandelier/"));
            Machine.AddLoop("idle", "machine", 1f);
            Machine.AddLoop("closed", "shut", 1, 1);
            Machine.Add("open", "open", 0.1f, "idle");
            Machine.Add("shut", "shut", 0.1f, "closed");

            Add(State = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/chandelier/"));
            State.AddLoop("idle", "machineState", 10f);

            State.Position.X -= 1;
            Collider = new Hitbox(Machine.Width, Machine.Height);
            square.Play("idle");
            LeftLight.Play("on");
            RightLight.Play("on");
            Machine.Play("idle");
            State.Play("idle");
            Add(VertexLight = new VertexLight(new Vector2(Machine.Width / 2, Machine.Height / 2), Color.White, 0.8f, 3, 30));
        }
        public void Reset()
        {
            square.Color = Color.White;
            RightLight.Color = Color.White;
            LeftLight.Color = Color.White;
            VertexLight.Color = Color.White;
        }
        private IEnumerator Sequence()
        {
            inSequence = true;
            doneSequence = true;
            yield return null;
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            NodeColor = LightMachineInfo.PreviousColors;
            LeftLight.Position = Machine.Position - Vector2.UnitX * LeftLight.Width;
            RightLight.Position = Machine.Position + Vector2.UnitX * Machine.Width;
            LeftLight.Position.Y = RightLight.Position.Y -= 2;

            VertexLight.Color = IconHolder.SetColors[CurrentNode];
            square.SetColor(IconHolder.SetColors[CurrentNode]);
            LeftLight.SetColor(IconHolder.SetColors[LeftNode]);
            RightLight.SetColor(IconHolder.SetColors[RightNode]);
        }
        public static void SetSequence()
        {
            for (int i = 0; i < 4; i++)
            {
                if (SequenceColor[i] != null)
                {
                    SequenceColor[i] = Color.Black;
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

        private bool SolutionDetected()
        {
            bool result = true;
            for (int i = 0; i < 4; i++)
            {
                if (!IconHolder.NodeUsed[i])
                {
                    result = false;
                }
                if (Icons[i].OccupiedNode - 1 != i)
                {
                    result = false;
                }
                if (SequenceColor[i] != IconHolder.SetColors[i])
                {
                    result = false;
                }
            }
            return result;
        }
        private IEnumerator ShakeMachine()
        {
            Vector2 rPos = RightLight.Position;
            Vector2 lPos = LeftLight.Position;
            Vector2 cPos = Machine.Position;
            Vector2 sPos = square.Position;
            Vector2 aPos = State.Position;
            Vector2 random;

            for (int i = 0; i < 4; i++)
            {
                random = Calc.Random.Range(-Vector2.UnitX, Vector2.UnitX) * 1.5f;
                State.Position = aPos + random;
                RightLight.Position = rPos + random;
                LeftLight.Position = lPos + random;
                Machine.Position = cPos + random;
                square.Position = sPos + random;
                yield return null;
                yield return null;
            }
            State.Position = aPos;
            RightLight.Position = rPos;
            LeftLight.Position = lPos;
            Machine.Position = cPos;
            square.Position = sPos;
            yield return null;
        }
        public override void Update()
        {
            base.Update();
            State.SetAnimationFrame(IconHolder.CurrentState);
            if (doneSequence)
            {
                return;
            }
            LeftLight.Position.X = Machine.Position.X - LeftLight.Width;
            RightLight.Position.X = Machine.Position.X + Machine.Width;
            player = Scene.Tracker.GetEntity<Player>();
            if (player is null || Scene as Level is null) { return; }
            Depth = player.Depth + 2;
            VertexLight.Color = IconHolder.SetColors[CurrentNode];
            square.SetColor(IconHolder.SetColors[CurrentNode]);
            LeftLight.SetColor(IconHolder.SetColors[LeftNode]);
            RightLight.SetColor(IconHolder.SetColors[RightNode]);

            if (GotSolution)
            {
                //Machine.Color = Color.DarkBlue;
                Add(new Coroutine(Sequence()));
                SceneAs<Level>().Session.SetFlag("lightPuzzleComplete");
            }

        }
        public static void UpdateColors()
        {
            CurrentNode++;
            RightNode++;
            LeftNode++;
            BackNode++;
            BackNode %= 4;
            CurrentNode %= 4;
            RightNode %= 4;
            LeftNode %= 4;
        }
        private IEnumerator RotateNode()
        {
            ButtonRoutine = false;
            Machine.Play("shut");
            LeftLight.Play("close");
            RightLight.Play("close");
            int div = 1;
            while (Machine.CurrentAnimationID == "shut" || RightLight.CurrentAnimationID == "close" || LeftLight.CurrentAnimationID == "close")
            {
                VertexLight.Alpha = 0.8f / div;
                div++;
                yield return null;
            }
            VertexLight.Alpha = 0;

            CurrentNode++;
            RightNode++;
            LeftNode++;
            BackNode++;
            BackNode %= 4;
            CurrentNode %= 4;
            RightNode %= 4;
            LeftNode %= 4;

            VertexLight.Color = NodeColor[CurrentNode];
            square.SetColor(NodeColor[CurrentNode]);
            LeftLight.SetColor(NodeColor[LeftNode]);
            RightLight.SetColor(NodeColor[RightNode]);
            Add(new Coroutine(ShakeMachine()));
            yield return 0.2f;
            Machine.Play("open");
            LeftLight.Play("open");
            RightLight.Play("open");
            VertexLight.Alpha = 0.8f;
            if (GotSolution)
            {
                Machine.Color = Color.DarkBlue;
            }
            /*
                        if (SolutionDetected())
                        {

                        }*/
            yield return null;
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
        //private Sprite Panel;
        //private Sprite Display;
        private Sprite leverSprite;
        private Sprite buttonSprite;
        //private Sprite State;
        private Entity Button;
        private Entity Lever;
        private Sprite Machine;
        public static int CurrentState;
        public static Color[] SetColors = {Color.White,Color.White,Color.White,Color.White};

        public static bool LeverState = false;

        public IconHolder(EntityData data, Vector2 offset)
        : base(data.Position + offset)
        {
            for (int i = 0; i < 4; i++)
            {
                Collider collider = new Hitbox(12, 12, 34 + (i * 15), 21);
                ColliderList.Add(collider);
                Nodes[i] = ColliderList.colliders[i].Position + new Vector2(collider.Width / 2, collider.Height);
            }
            /*Add(Panel = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/chandelier/"));
            Panel.AddLoop("idle", "panel", 1f);

            Add(Display = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/chandelier/"));
            Display.AddLoop("idle", "display", 1f);

            Add(State = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/chandelier/"));
            State.AddLoop("idle", "state", 10f);*/

            Add(sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/chandelier/"));
            sprite.AddLoop("idle", "mainModule", 10f);
            sprite.Play("idle");
           // sprite.Position -= new Vector2(sprite.Width/2,sprite.Height/2);
            sprite.SetAnimationFrame(CurrentState);
            Depth = 2;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(Lever = new Entity(Position), Button = new Entity(Position));
            Lever.Add(leverSprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/chandelier/"));
            Button.Add(buttonSprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/chandelier/"));

            leverSprite.AddLoop("idleUp", "lever", 1f, 0);
            leverSprite.AddLoop("idleDown", "lever", 1f, 6);
            leverSprite.Add("down", "lever", 0.1f, "idleDown");
            leverSprite.Add("up", "leverReverse", 0.1f, "idleUp");

            buttonSprite.AddLoop("idle", "button", 1f, 0);
            buttonSprite.Add("clicked", "buttonClicked", 0.2f, "idle");

            Lever.Depth = Button.Depth = Depth;
            Lever.Add(new VertexLight(new Vector2(3, 9), Color.White, 1, 0, (int)leverSprite.Height));
            Button.Add(new VertexLight(new Vector2(2, 2), Color.White, 1, 0, (int)buttonSprite.Height));

            Lever.Collider = new Hitbox(leverSprite.Width, leverSprite.Height);
            leverSprite.Play(LeverState ? "idleUp" : "idleDown");
            buttonSprite.Play("idle");

            Rectangle lRect = new Rectangle(0, (int)(leverSprite.Height), (int)leverSprite.Width + 8, 4);
            Lever.Add(new TalkComponent(lRect, new Vector2(leverSprite.Width / 2 - 0.5f, -7), LeverInteract));
            Lever.Collider = null;
            Rectangle bRect = new Rectangle(-3, 9, 8, 4);
            Button.Add(new TalkComponent(bRect, new Vector2(2, -4), ButtonInteract));
            Lever.Position = Position + new Vector2(14, 19);
            Button.Position = Position + new Vector2(5, 27);


        }
        private void LeverInteract(Player player)
        {
            if (leverSprite.CurrentAnimationID == "idleUp")
            {
                leverSprite.Play("down");
                LeverState = false;
            }
            else
            {
                leverSprite.Play("up");
                LeverState = true;
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
        private void ButtonInteract(Player player)
        {
            //LightsPuzzle.ButtonRoutine = true;
            LightsPuzzle.UpdateColors();
            CurrentState++;
            CurrentState %= 4;
        }
        public void Reset()
        {
            foreach(LightsIcon icon in icons)
            {
                icon.IsSet = false;
            }
            for (int i = 0; i < 4; i++)
            {
                SetColors[i] = Color.White;
            }

            if (leverSprite.CurrentAnimationID == "idleUp")
            {
                leverSprite.Play("down");
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
            //SetOnGround();
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
            if (Scene as Level is null)
            {
                return;
            }
            sprite.SetAnimationFrame(CurrentState);
            Lever.Depth = Button.Depth = Depth;
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
                foreach (LightsIcon icon in Scene.Tracker.GetEntities<LightsIcon>())
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
                icon.SetPosition = Calc.ClosestTo(vectors, icon.Position);
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
                Position.X += 20 - (sprite.Width / 2) - 8;
                Lever.Position = Position - new Vector2(16, -1);
                Button.Position = Lever.Position - new Vector2(8, -8f);
            }
        }
    }


    [CustomEntity("PuzzleIslandHelper/LightsIcon")]
    [Tracked]
    public class LightsIcon : Actor
    {
        public Color Color;
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

        public LightsIcon(Vector2 position) : base(position) {}
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
 
            HoldingHitbox = new Hitbox(sprite.Width -4, sprite.Height, 2-sprite.Width * Justify.X, -sprite.Height * Justify.Y);
            IdleHitbox = new Hitbox(sprite.Width, sprite.Height, -sprite.Width * Justify.X,-sprite.Height * Justify.Y);
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
            if (Speed.Y > 0f)
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
                    float target = ((!OnGround(Position + Vector2.UnitX * 3f)) ? 20f : (OnGround(Position - Vector2.UnitX * 3f) ? 0f : (-20f)));
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
                Player entity = base.Scene.Tracker.GetEntity<Player>();
                TempleGate templeGate = CollideFirst<TempleGate>();
                if (templeGate != null && entity != null)
                {
                    templeGate.Collidable = false;
                    MoveH((float)(Math.Sign(entity.X - base.X) * 32) * Engine.DeltaTime);
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