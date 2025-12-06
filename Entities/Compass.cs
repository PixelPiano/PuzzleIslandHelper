using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using Celeste.Mod.PuzzleIslandHelper.Entities.GearEntities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Core.Tokens;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [ConstantEntity("PuzzleIslandHelper/CompassManager")]
    [Tracked]
    public class CompassManager : Entity
    {
        public CompassManager() : base()
        {
            Tag |= Tags.TransitionUpdate | Tags.Global | Tags.Persistent;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
        }
        public override void Update()
        {
            base.Update();
            UpdateAllNodes();
        }
        public void UpdateAllNodes()
        {
            foreach (CompassData data in PianoModule.Session.CompassData)
            {
                data.TrySetFirstFound(Scene as Level);
            }
        }
    }
    [CustomEntity("PuzzleIslandHelper/Compass")]
    [Tracked]
    public class Compass : Entity
    {
        public static bool Enabled => enableFlags;
        private static FlagList enableFlags = new FlagList("CompassEnabled", true);
        public class CompassPulse : Entity
        {
            public Pulse Pulse;
            public Circle Circle;
            private List<CompassNode> collided = [];
            private int startRadius;
            public CompassPulse(Compass compass) : base(compass.Position)
            {
                startRadius = (int)(compass.Width / 2);
                Pulse = Pulse.Circle(this, Pulse.Fade.Late, Pulse.Mode.Oneshot, compass.Collider.HalfSize, startRadius, 64, 0.8f, true, Color.White, Color.Transparent, null, Ease.CubeIn);
                Collider = Circle = new Circle(startRadius, startRadius, startRadius);
                Depth = int.MinValue;
            }
            public override void DebugRender(Camera camera)
            {
                base.DebugRender(camera);
                Draw.Point(Center, Color.Red);
            }
            public override void Update()
            {
                base.Update();
                Circle.Radius = startRadius + ((64 - startRadius) * (1 - Pulse.SizePercent));
                foreach (CompassNode node in Scene.Tracker.GetEntities<CompassNode>())
                {
                    if (CollideCheck(node))
                    {
                        if (!collided.Contains(node))
                        {
                            node.OnCompassPulseCollide();
                            collided.Add(node);
                        }
                    }
                }
                if (!Pulse.Active)
                {
                    RemoveSelf();
                }
            }
        }
        public CompassManager Manager;
        public Image Image;
        public Image Sub;
        public Image Flash;
        public Directions Direction
        {
            get => (Directions)SceneAs<Level>().Session.GetCounter("Compass" + ID);
            set => SceneAs<Level>().Session.SetCounter("Compass" + ID, (int)value);
        }
        public Vector2? FlagPosition = null;
        public TalkComponent DotX3;
        public string ID;
        public FlagList Flag;
        private ShakeComponent shaker;
        private Vector2 OrigPosition;
        public CompassData Data;
        public string debug => Data.ToString();
        public bool Leader;
        public float rotateRate;
        private Tween rotateMultTween;
        private float prevRotation;
        private float rotationAdded;
        private float rotateTarget;
        private Tween flashTween;
        private float flashMult;
        private float colorLerp = 0;
        public Color Color = Color.Gray;

        public Compass(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Depth = 1;
            Vector2[] nodes = data.NodesOffset(offset);
            Flag = data.FlagList();
            Flag.Inverted = true;
            if (nodes != null && nodes.Length > 0 && !Flag.Empty)
            {
                FlagPosition = nodes[0];
            }
            OrigPosition = Position;
            Leader = data.Bool("leader");
            ID = data.Attr("compassID");

            string path = "objects/PuzzleIslandHelper/compass/";
            Image = new Image(GFX.Game[path + (Leader ? "dial" : "dialMini")]);
            Flash = new Image(GFX.Game[path + "circle"]);
            Sub = new Image(GFX.Game[path + "dialSub"]);
            Vector2 half = new Vector2(Image.Width - 1, Image.Height - 1) / 2f;
            Image.Origin = half;
            Sub.Origin = half;
            Flash.Origin = half;
            Image.Position += half;
            Sub.Position += half;
            Flash.Position += half;
            Flash.Color = Color.Transparent;
            flashTween = Tween.Create(Tween.TweenMode.Persist, Ease.CubeOut, 0.4f);
            flashTween.OnUpdate = (t) =>
            {
                flashMult = 1 - t.Eased;
            };
            flashTween.OnComplete = (t) =>
            {
                flashMult = 0;
            };
            if (Leader)
            {
                Add(Sub, Image, Flash, flashTween);
            }
            else
            {
                Add(Image);
            }
            Collider = new Hitbox(half.X * 2, half.Y * 2);
            Add(shaker = new ShakeComponent(onShake));
            rotateMultTween = Tween.Create(Tween.TweenMode.Persist, Ease.SineIn, 1f);
            Add(rotateMultTween);
        }
        private void onShake(Vector2 shake)
        {
            Image.Position += shake;
            Flash.Position += shake;
            Sub.Position -= shake;
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Draw.Point(Center, Color.White);
        }
        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            SceneAs<Level>().Session.SetCounter("Compass" + ID, 0);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Manager = scene.Tracker.GetEntity<CompassManager>();
            if (Manager == null)
            {
                scene.Add(Manager = new CompassManager());
            }
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);

            foreach (var c in PianoModule.Session.CompassData)
            {
                if (c.ID == ID)
                {
                    Data = c;
                    break;
                }
            }
            if (Data == null)
            {
                RemoveSelf();
                return;
            }
            if (Enabled)
            {
                colorLerp = 1;
                Image.Color = Sub.Color = Color = Color.White;
            }
            else
            {
                colorLerp = 0;
                Image.Color = Sub.Color = Color = Color.Gray;
            }
            Flash.Color = Color.Transparent;
            if (FlagPosition.HasValue)
            {
                Position = Flag ? OrigPosition : FlagPosition.Value;
            }
            Directions dir = Direction = (Directions)SceneAs<Level>().Session.GetCounter("Compass" + ID);
            Image.Rotation = (float)(Math.PI / -2f) * (float)dir;
            Sub.Rotation = -Image.Rotation;
        }
        public override void Update()
        {
            base.Update();
            if (Enabled)
            {
                if (colorLerp != 1)
                {
                    colorLerp = Calc.Approach(colorLerp, 1, Engine.DeltaTime * 3f);
                }
            }
            else if (colorLerp != 0)
            {
                colorLerp = Calc.Approach(colorLerp, 0, Engine.DeltaTime * 3f);
            }
            Image.Color = Sub.Color = Color = Color.Lerp(Color.Gray, Color.White, Ease.SineInOut(colorLerp));
            Flash.Color = Color.White * flashMult;
            if (rotateTarget != 0 && rotationAdded != rotateTarget)
            {
                rotationAdded = Calc.Approach(rotationAdded, rotateTarget, (4f + (10f * rotateMultTween.Eased)) * Engine.DeltaTime);
                Image.Rotation = prevRotation + rotationAdded;
            }
            else
            {
                if (rotateTarget != 0)
                {
                    Image.Rotation = prevRotation + rotateTarget;
                    rotateTarget = 0;
                    shaker.StartShaking(0.1f);
                }
                rotationAdded = 0;
                rotateRate = 0;
                rotateMultTween.Stop();
                Image.Rotation %= MathHelper.TwoPi;
            }
            Sub.Rotation = -Image.Rotation;
            if (FlagPosition.HasValue)
            {
                Vector2 target = Flag ? OrigPosition : FlagPosition.Value;
                if (Position != target)
                {
                    Position = Calc.Approach(Position, target, 10 * Engine.DeltaTime);
                    shaker.StartShaking();
                }
                else
                {
                    shaker.StopShaking();
                }
            }
        }
        public void Interact(Player player)
        {
            Input.Dash.ConsumePress();
            Scene.Add(new CompassPulse(this));
            Direction = (Directions)(((int)Direction + 1) % 4);
            shaker.StartShaking(0.1f);
            if (rotateTarget == 0 && !rotateMultTween.Active)
            {
                prevRotation = Image.Rotation;
                rotationAdded = 0;
                rotateMultTween.Start();
            }
            rotateTarget += (float)MathHelper.Pi / 2f;
            flashTween.Start();
        }
        [OnLoad]
        public static void Load()
        {
            Everest.Events.LevelLoader.OnLoadingThread += LevelLoader_OnLoadingThread;
            Everest.Events.Player.OnDie += Player_OnDie;
            Everest.Events.Player.OnSpawn += Player_OnSpawn;
        }

        private static void Player_OnSpawn(Player obj)
        {
            foreach (Entity e in removeFromGlobalOnSpawn)
            {
                e.RemoveTag(Tags.Global);
            }
            removeFromGlobalOnSpawn.Clear();
        }

        private static List<Entity> removeFromGlobalOnSpawn = [];
        private static void Player_OnDie(Player obj)
        {
            foreach (CompassNode node in obj.Scene.Tracker.GetEntities<CompassNode>())
            {
                node.AddTag(Tags.Global);
                removeFromGlobalOnSpawn.Add(node);
                //node.SceneAs<Level>().Session.DoNotLoad.Remove(node.EntityID);
            }
            foreach (CompassNodeOrb orb in obj.Scene.Tracker.GetEntities<CompassNodeOrb>())
            {
                orb.AddTag(Tags.Global);
                removeFromGlobalOnSpawn.Add(orb);
                //orb.SceneAs<Level>().Session.DoNotLoad.Remove(orb.EntityID);
            }
        }

        [OnUnload]
        public static void Unload()
        {
            Everest.Events.LevelLoader.OnLoadingThread -= LevelLoader_OnLoadingThread;
            Everest.Events.Player.OnDie -= Player_OnDie;
            Everest.Events.Player.OnSpawn -= Player_OnSpawn;
        }
        public static void ReloadCompassMapData(Level level)
        {
            foreach (var pair in PianoMapDataProcessor.CompassData) //for each area with a compass...
            {
                if (PianoMapDataProcessor.CompassNodeData.ContainsKey(pair.Key)) //if a node also exists in that same area...
                {
                    HashSet<CompassData> set = pair.Value; //list of compasses in the area
                    foreach (var compass in set)
                    {
                        foreach (var n in PianoMapDataProcessor.CompassNodeData[pair.Key])
                        {
                            if (n.ParentID == compass.ID)
                            {
                                compass.Add(n); //link the node data and the compass data together
                            }
                        }
                        compass.OrderNodes();
                    }
                }
            }
            PianoModule.Session.CompassData.Clear();
            if (PianoMapDataProcessor.CompassData.TryGetValue(level.GetAreaKey(), out var data))
            {
                if (data != null)
                {
                    foreach (var d in data)
                    {
                        PianoModule.Session.CompassData.Add(d);
                    }
                }
            }
        }
        private static void ReloadAswiitCodeMapData(Level level)
        {
            PianoModule.Session.AscwiitSequences.Clear();
            if (PianoMapDataProcessor.AscwiitCodes.TryGetValue(level.GetAreaKey(), out var data))
            {
                foreach (var d in data)
                {
                    PianoModule.Session.AscwiitSequences.Add(d);
                }
            }
        }
        private static void LevelLoader_OnLoadingThread(Level level)
        {
            ReloadCompassMapData(level);
            ReloadAswiitCodeMapData(level);
        }

    }
}