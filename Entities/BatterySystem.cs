using Celeste.Mod.Entities;
using Celeste.Mod.Meta;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Components.Visualizers;
using Celeste.Mod.PuzzleIslandHelper.Components.Visualizers.DSPs;
using Celeste.Mod.PuzzleIslandHelper.Cutscenes.GameshowEntities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System.Collections;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/BatterySystem")]
    [Tracked]
    public class BatterySystem : Entity
    {
        public static MTexture Box = GFX.Game["objects/PuzzleIslandHelper/forkAmp/batteryContainer"];
        public static MTexture Middle = GFX.Game["objects/PuzzleIslandHelper/forkAmp/middleMachine"];
        public static MTexture PlateBox = GFX.Game["objects/PuzzleIslandHelper/forkAmp/plateBox"];
        public MTexture Plate => GFX.Game["objects/PuzzleIslandHelper/forkAmp/plate" + (lowering ? "01" : "00")];
        private bool lowering;

        public Node[] Nodes;
        public Pipe[] Pipes;
        private string[] flags;
        private int unlocked;
        private bool inRoutine;
        private Collider detect;

        private Vector2 plateBoxPos;
        private Vector2 middlePos;
        private Vector2 platePos;
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
                Nodes[i] = new Node(i);
            }
            float pipeWidth = Pipes[0].Width;
            Pipes[4] = new Pipe(Pipes[3].Position + Vector2.UnitX * (pipeWidth + Middle.Width));
            middlePos = TopRight + Vector2.UnitX * pipeWidth;
            plateBoxPos = middlePos + new Vector2(Middle.Width + pipeWidth, Middle.Height - PlateBox.Height);
            platePos = plateBoxPos - new Vector2(1, PlateBox.Height);
            detect = new Hitbox(Plate.Width, Plate.Height, platePos.X, platePos.Y - Plate.Height + 8);

            Add(Nodes);
            scene.Add(Pipes);

            for (int i = 0; i < 4; i++)
            {
                if (!string.IsNullOrEmpty(flags[i]) && (scene as Level).Session.GetFlag(flags[i]))
                {
                    Nodes[i].SlotIn(true);
                    unlocked++;
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
                    level.Session.SetFlag(battery.FlagOnSet);
                    //level.Session.DoNotLoad.Add(battery.EntityID);
                    battery.RemoveSelf();
                    lowering = true;
                    player.StateMachine.State = Player.StDummy;
                    yield return level.ZoomTo(middlePos + Middle.Center - level.Camera.Position, 1.5f, 1);
                    Vector2 from = platePos;
                    for (float i = 0; i < 1; i += Engine.DeltaTime)
                    {
                        platePos.Y = Calc.LerpClamp(from.Y, from.Y + Plate.Height, i);
                        yield return null;
                    }
                    platePos.Y = from.Y + Plate.Height;
                    yield return Pipes[4].Journey();
                    yield return (4 - pipeIndex) * 0.5f;
                    yield return Pipes[pipeIndex].Journey();
                    Nodes[pipeIndex].SlotIn();
                    yield return 0.3f;
                    lowering = false;
                    for (float i = 0; i < 1; i += Engine.DeltaTime)
                    {
                        platePos.Y = Calc.LerpClamp(from.Y + Plate.Height, from.Y, i);
                        yield return null;
                    }
                    platePos.Y = from.Y;
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
                    if (!battery.Hold.IsHeld && battery.Speed.Y > 0 && detect.Collide(battery))
                    {
                        int index = -1;
                        for (int i = 0; i < 4; i++)
                        {
                            if (battery.FlagOnSet == flags[i])
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

            Draw.SpriteBatch.Draw(Plate.Texture.Texture_Safe, platePos, Color.White);
            Draw.SpriteBatch.Draw(PlateBox.Texture.Texture_Safe, plateBoxPos, Color.White);
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            scene.Remove(Pipes);
        }
        public class Pipe : Entity
        {
            public Sprite Sprite;
            public Color Color = Color.White;
            public Pipe(Vector2 position) : base(position)
            {
                Depth = 2;
                Sprite = new Sprite(GFX.Game,"objects/PuzzleIslandHelper/forkAmp/");
                Sprite.CenterOrigin();
                Sprite.Position += new Vector2(Sprite.Width / 2, Sprite.Height / 2);
                Sprite.AddLoop("idle","batteryPipe",0.1f,0);
                Sprite.Add("squeeze","batteryPipe",0.1f,"idle");
                Add(Sprite);
                Sprite.Play("idle");
                Tag |= Tags.TransitionUpdate;
                Collider = new Hitbox(Sprite.Width, Sprite.Height);
            }
            public IEnumerator Journey()
            {
                Sprite.Play("squeeze");
                while(Sprite.CurrentAnimationID == "squeeze")
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
            private int index;
            public static MTexture Glass = GFX.Game["objects/PuzzleIslandHelper/forkAmp/batteryGlass"];
            public Node(int index) : base(GFX.Game, "objects/PuzzleIslandHelper/forkAmp/")
            {
                this.index = index;
                AddLoop("idleOff", "batterySlot", 0.1f, 0);
                AddLoop("idleOn", "batterySlot", 0.1f, 11);
                Add("flash", "batterySlot", 0.1f, "idleOn", 7, 8, 9, 10);
                Add("slotIn", "batterySlot", 0.1f, "flash", 1, 2, 3, 4, 5, 6);
                Play("idleOff");
                Position = new Vector2(3, index * Height + 1);
            }
            public void SlotIn(bool instant = false)
            {
                Play(instant ? "idleOn" : "slotIn");
                Activated = true;
            }
            public override void Render()
            {
                base.Render();
                Draw.SpriteBatch.Draw(Glass.Texture.Texture_Safe, RenderPosition, Color);
            }
        }
    }

}
