using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Cutscenes.GameshowEntities
{
    [CustomEntity("PuzzleIslandHelper/GameshowLift")]
    [Tracked]
    public class Lift : Solid
    {
        public static MTexture liftTex = GFX.Game["objects/PuzzleIslandHelper/gameshow/lift/lift"];
        public Image lift;
        public Button button;
        public bool Rising;
        private float ySpeed;
        private string teleportTo;
        private Fader fader;
        private Vector2 liftFrom;
        private Vector2 playerFrom;
        public class Button : Entity
        {
            public Sprite button;
            public bool Pressed;
            private bool interacted;
            public Button(Vector2 position) : base(position)
            {
                Add(button = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/gameshow/lift/"));
                button.AddLoop("up", "switch", 0.1f, 0);
                button.AddLoop("down", "switch", 0.1f, 3);
                button.Add("press", "switch", 0.08f, "down");
                Add(new TalkComponent(new Rectangle(-8, 0, 10, 16), Vector2.UnitX * 5, Interact));
                VertexLight light = new VertexLight(Color.White, 1, (int)button.Width + 4, 30);
                light.Position = new Vector2(button.Width / 2, button.Height / 2);
                Add(light);
                button.Play("up");
            }
            public override void Update()
            {
                base.Update();
                if (button.CurrentAnimationID == "down")
                {
                    Pressed = true;
                }
            }
            public override void Render()
            {
                button.DrawSimpleOutline();
                base.Render();
            }
            private void Interact(Player player)
            {
                if (interacted) return;
                interacted = true;
                button.Play("press");
                player.StateMachine.State = Player.StDummy;

            }
        }
        public Lift(EntityData data, Vector2 offset) : base(data.Position + offset, liftTex.Width, liftTex.Height, true)
        {
            Add(lift = new Image(liftTex));
            teleportTo = data.Attr("teleportTo");
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(fader = new Fader());
            Entity helper = new Entity(new Vector2(X, (scene as Level).LevelOffset.Y))
            {
                new Image(GFX.Game["objects/PuzzleIslandHelper/gameshow/lift/cords"])
            };
            scene.Add(helper);
            helper.Depth = 3;
            button = new Button(Position + new Vector2(Width - 8, -16));
            scene.Add(button);
        }
        public class Fader : Entity
        {
            public float Alpha;
            private float timer;
            private bool timing;
            public Fader() : base()
            {
                Depth = -100001;
            }
            public override void Update()
            {
                base.Update();
                if (timing)
                {
                    timer += Engine.DeltaTime;
                    if (timer >= 1)
                    {
                        RemoveSelf();
                    }
                }
            }
            public void StartGlobalTimer()
            {
                AddTag(Tags.Global);
                AddTag(Tags.TransitionUpdate);
                timing = true;
                timer = 0;
            }
            public override void Render()
            {
                base.Render();
                if (Scene is Level level && Alpha > 0)
                {
                    Draw.Rect(level.Camera.Position, 320, 180, Color.Black * Alpha);
                }
            }
        }
        public override void Update()
        {
            base.Update();
            if (!shaking)
            {
                lift.Position = Vector2.Zero;
            }
            if (button.Pressed && !Rising)
            {
                Rising = true;
                Scene.Add(new LiftCutscene(this));
            }
            if (Scene is Level level && HasPlayerOnTop() && Bottom < level.LevelOffset.Y + 48)
            {
                if (Bottom < level.LevelOffset.Y) ySpeed = 0;
                fader.Alpha = Calc.Min(fader.Alpha + Engine.DeltaTime, 1);
                if (level.GetPlayer() is Player player)
                {
                    player.Light.Alpha = 1 - fader.Alpha;
                }
                if (fader.Alpha == 1)
                {
                    Teleport();
                }
            }
            if (Rising)
            {
                MoveV(ySpeed);
            }
        }
        public bool Teleported;
        public void Teleport()
        {
            if(Teleported) return;
            Teleported = true;
            SceneAs<Level>().Lighting.Alpha = 1;
            fader.StartGlobalTimer();
            PassageTransition.InstantRelativeTeleport(Scene, teleportTo, true);
        }

        public class LiftCutscene : CutsceneEntity
        {
            public Lift Lift;
            public LiftCutscene(Lift lift) : base()
            {
                Lift = lift;
            }

            public override void OnBegin(Level level)
            {
                Add(new Coroutine(routine()));
            }
            private IEnumerator routine()
            {
                if (SceneAs<Level>().GetPlayer() is not Player player) yield break;
                Lift.playerFrom = player.Position;
                Lift.liftFrom = Lift.Position;
                Lift.StartShaking(0.3f);
                yield return 0.3f;
                Lift.Position = Lift.liftFrom;
                player.Position = Lift.playerFrom;
                for (float i = 0.3f; i < 1; i += Engine.DeltaTime / 2)
                {
                    Lift.ySpeed = Calc.LerpClamp(0, -40 * Engine.DeltaTime, Ease.BackIn(i));
                    yield return null;
                }
                while (Lift.fader.Alpha < 1)
                {
                    yield return null;
                }
                Lift.Teleport();
                EndCutscene(Level);
            }
            public override void OnEnd(Level level)
            {
                Lift.fader.Alpha = 1;
                Lift.Teleport();
            }
        }

        public override void OnShake(Vector2 amount)
        {
            base.OnShake(amount);
            Position = liftFrom + amount;
            if (SceneAs<Level>().GetPlayer() is Player player)
            {
                player.Position = playerFrom + amount;
            }
        }

    }
}
