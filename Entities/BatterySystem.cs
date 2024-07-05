using Celeste.Mod.Entities;
using Celeste.Mod.Meta;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Components.Visualizers;
using Celeste.Mod.PuzzleIslandHelper.Components.Visualizers.DSPs;
using Celeste.Mod.PuzzleIslandHelper.Cutscenes.GameshowEntities;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System.Collections;
using System.Drawing.Design;
using System.Linq;
using System.Linq.Expressions;
using VivHelper;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/BatterySystem")]
    [Tracked]
    public class BatterySystem : Entity
    {
        public static MTexture Box = GFX.Game["objects/PuzzleIslandHelper/forkAmp/batteryContainer"];
        public static MTexture Middle = GFX.Game["objects/PuzzleIslandHelper/forkAmp/middleMachine"];
        public static MTexture PlateBox = GFX.Game["objects/PuzzleIslandHelper/forkAmp/plateBox"];
        public static MTexture PlateTex => GFX.Game["objects/PuzzleIslandHelper/forkAmp/plate"];
        public static MTexture Battery => GFX.Game["objects/PuzzleIslandHelper/forkAmp/battery"];
        private Plate plate;

        public Node[] Nodes;
        public Pipe[] Pipes;
        private string[] flags;

        private bool inRoutine;
        private Collider detect;

        private Vector2 plateBoxPos;
        private Vector2 middlePos;
        public BatterySystem(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Depth = 1;
            flags = new string[4]
            {
                data.Attr("flag1"),data.Attr("flag2"),data.Attr("flag3"),data.Attr("flag4")
            };
            Collider = new Hitbox(Box.Width, Box.Height);
            Tag |= Tags.TransitionUpdate;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Nodes = new Node[4];
            Pipes = new Pipe[5];
            Vector2 pipeOffset = Position + new Vector2(Width, 3);
            for (int i = 0; i < 4; i++)
            {
                Pipes[i] = new Pipe(pipeOffset + (i * Vector2.UnitY * 10));
                Nodes[i] = new Node(i, flags[i]);
            }
            float pipeWidth = Pipes[0].Width;
            Pipes[4] = new Pipe(Pipes[3].Position + Vector2.UnitX * (pipeWidth + Middle.Width));
            middlePos = TopRight + Vector2.UnitX * pipeWidth;
            plateBoxPos = middlePos + new Vector2(Middle.Width + pipeWidth, Middle.Height - PlateBox.Height);
            plate = new Plate(plateBoxPos + Vector2.UnitY * (4 - PlateBox.Height));
            detect = new Hitbox(PlateTex.Width, PlateTex.Height, plate.X, plate.Y - PlateTex.Height + 8);

            Add(Nodes);
            scene.Add(Pipes);
            scene.Add(plate);
            for (int i = 0; i < 4; i++)
            {
                if (!string.IsNullOrEmpty(flags[i]) && (scene as Level).Session.GetFlag(flags[i]))
                {
                    Nodes[i].SlotIn(true);
                }
            }
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Draw.HollowRect(detect, Color.Cyan);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
        }
        private IEnumerator Routine(ForkAmpBattery battery, int pipeIndex)
        {
            if (Scene is Level level)
            {
                if (level.GetPlayer() is Player player)
                {
                    level.Session.SetFlag(battery.FlagOnFinish);
                    level.Session.DoNotLoad.Add(battery.EntityID);
                    player.StateMachine.State = Player.StDummy;
                    battery.Add(new Coroutine(battery.Approach(plate.CenterX)));
                    battery.Depth = plate.Depth;
                    yield return level.ZoomTo(middlePos + Middle.Center - level.Camera.Position, 1.5f, 1);
                    plate.Lower();
                    while (!plate.AtTarget)
                    {
                        yield return null;
                    }
                    battery.RemoveSelf();
                    yield return Pipes[4].Journey();
                    yield return (4 - pipeIndex) * 0.5f;
                    yield return Pipes[pipeIndex].Journey();
                    Nodes[pipeIndex].SlotIn();
                    yield return 0.3f;
                    plate.Raise();
                    while (!plate.AtTarget)
                    {
                        yield return null;
                    }
                    yield return level.ZoomBack(1);
                    player.StateMachine.State = Player.StNormal;
                }
            }
            yield return null;
        }
        public override void Update()
        {
            base.Update();
            if (Scene is Level level && !inRoutine)
            {
                foreach (ForkAmpBattery battery in level.Tracker.GetEntities<ForkAmpBattery>())
                {
                    if (!battery.Hold.IsHeld && battery.Speed.Y >= 0 && plate.CollideCheckOutside(battery, plate.TopCenter - Vector2.UnitY))
                    {
                        int index = -1;
                        for (int i = 0; i < 4; i++)
                        {
                            if (battery.FlagOnFinish == flags[i])
                            {
                                index = i;
                                break;
                            }
                        }
                        if (index > -1)
                        {
                            inRoutine = true;
                            Add(new Coroutine(Routine(battery, index)));
                        }

                    }
                }
            }

        }
        public override void Render()
        {
            Draw.SpriteBatch.Draw(Box.Texture.Texture_Safe, Position, Color.White);
            base.Render();
            Draw.SpriteBatch.Draw(Middle.Texture.Texture_Safe, middlePos, Color.White);
            Draw.SpriteBatch.Draw(PlateBox.Texture.Texture_Safe, plateBoxPos, Color.White);
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            scene.Remove(Pipes);
        }
        public class Plate : JumpThru
        {
            public bool InRoutine;
            private bool raising;
            private bool lowering;
            private Vector2 orig;
            private float targetY;
            public bool AtTarget => Position.Y == targetY;
            private bool hasRider, hadRider;
            public const float ApproachSpeed = 50f;
            private Wiggler wiggler;
            public Plate(Vector2 position) : base(position, PlateTex.Width, true)
            {
                Depth = 2;
                orig = position;
                targetY = position.Y;
                Add(wiggler = Wiggler.Create(0.6f, 3));
                wiggler.StartZero = true;
            }
            public override void Update()
            {
                base.Update();

                if (!InRoutine)
                {
                    hasRider = HasPlayerRider();

                    if (hasRider) //if actor on surface
                    {
                        wiggler.StopAndClear();
                        raising = false;
                        targetY = orig.Y + 4;
                    }
                    else if (hadRider) //if actor just left surface
                    {
                        raising = true;
                        targetY = orig.Y;
                    }
                    if (raising && Position.Y == orig.Y) //if back at original position
                    {
                        wiggler.Start();
                        raising = false;
                    }
                    hadRider = hasRider;
                }
                MoveTowardsY(targetY + wiggler.Value, (InRoutine ? 10f : ApproachSpeed) * Engine.DeltaTime);


            }
            public override void Render()
            {
                base.Render();
                Draw.SpriteBatch.Draw(PlateTex.Texture.Texture_Safe, Collider.AbsolutePosition, Color.White);
            }
            public void Lower()
            {
                InRoutine = true;
                targetY = Position.Y + PlateTex.Height;
            }
            public void Raise()
            {
                InRoutine = false;
                targetY = Position.Y - PlateTex.Height;
            }
        }
        public class Pipe : Entity
        {
            public Sprite Sprite;
            public Color Color = Color.White;
            public Pipe(Vector2 position) : base(position)
            {
                Depth = 2;
                Sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/forkAmp/");
                Sprite.CenterOrigin();
                Sprite.Position += new Vector2(Sprite.Width / 2, Sprite.Height / 2);
                Sprite.AddLoop("idle", "batteryPipe", 0.1f, 0);
                Sprite.Add("squeeze", "batteryPipe", 0.1f, "idle");
                Add(Sprite);
                Sprite.Play("idle");
                Tag |= Tags.TransitionUpdate;
                Collider = new Hitbox(Sprite.Width, Sprite.Height);
            }
            public IEnumerator Journey()
            {
                Sprite.Play("squeeze");
                while (Sprite.CurrentAnimationID == "squeeze")
                {
                    yield return null;
                }
                yield return null;
            }
            public override void Render()
            {
                base.Render();
            }
        }
        public class Node : Sprite
        {
            public bool Activated;
            public string Flag;
            private int index;
            public static MTexture Glass = GFX.Game["objects/PuzzleIslandHelper/forkAmp/batteryGlass"];
            public Node(int index, string flag) : base(GFX.Game, "objects/PuzzleIslandHelper/forkAmp/")
            {
                this.index = index;
                AddLoop("idleOff", "batterySlot", 0.1f, 0);
                AddLoop("idleOn", "batterySlot", 0.1f, 11);
                Add("flash", "batterySlot", 0.1f, "idleOn", 7, 8, 9, 10);
                Add("slotIn", "batterySlot", 0.1f, "flash", 1, 2, 3, 4, 5, 6);
                Play("idleOff");
                Position = new Vector2(3, index * Height + 1);
                Flag = flag;
            }
            public override void Added(Entity entity)
            {
                base.Added(entity);
                if (!string.IsNullOrEmpty(Flag) && SceneAs<Level>().Session.GetFlag(Flag))
                {
                    SlotIn(true);
                }
            }
            public void SlotIn(bool instant = false)
            {
                Play(instant ? "idleOn" : "slotIn");
            }
            public override void Update()
            {
                base.Update();
                Activated = !string.IsNullOrEmpty(Flag) && SceneAs<Level>().Session.GetFlag(Flag);
            }
            public override void Render()
            {
                base.Render();
                Draw.SpriteBatch.Draw(Glass.Texture.Texture_Safe, RenderPosition, Color);
            }
        }
    }

}
