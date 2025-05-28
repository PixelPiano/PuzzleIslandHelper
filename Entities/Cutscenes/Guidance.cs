using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora.Passengers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;


namespace Celeste.Mod.PuzzleIslandHelper.Entities.Cutscenes
{
    [CustomPassengerCutscene("PassengerGuidance")]
    [Tracked]
    public class Guidance : PassengerCutscene
    {
        public VertexPassenger VertexPassenger;
        public Func<IEnumerator>[] Events =>
            [
            walkToPrimitive, walkToPrimitiveBehind,
            primitiveTurnOn,
            appearObject,
            staticBegin,staticEnd,
            powerDown
            ];
        private IEnumerator staticEnd()
        {
            yield return null;
        }
        public Guidance(Passenger passenger, Player player) : base(passenger, player)
        {
            if (passenger is VertexPassenger)
            {
                VertexPassenger = passenger as VertexPassenger;
            }
            OncePerSession = false;
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);

        }
        public override void OnBegin(Level level)
        {
            if (VertexPassenger == null) return;
            Player.DisableMovement();
            Add(new Coroutine(Cutscene()));
        }
        public IEnumerator Cutscene()
        {
            if (Level.Session.GetFlag("MemoryBoxFirstEncounter"))
            {
                yield return Textbox.Say("PassengerGuidanceDialog", Events);
            }
            else
            {
                yield return Textbox.Say("PassengerNothing");
            }
            EndCutscene(Level);
        }
        public class ShineObject : Actor
        {
            public float Scale = 1;
            public float WhiteVal = 1;
            public Image Image;
            public ShineObject(MTexture texture, Vector2 position) : base(position)
            {
                Add(Image = new Image(texture));
                Image.CenterOrigin();
                Image.Position += Image.HalfSize();
                Image.Scale = Vector2.One;
            }
            public override void Render()
            {
                base.Render();
                Draw.SpriteBatch.End();
                BlendState state = new BlendState()
                {
                    BlendFactor = Color.Multiply(Color.White, 1 + 254 * Math.Min(WhiteVal, 1)),
                    ColorSourceBlend = Blend.SourceColor,
                    ColorDestinationBlend = Blend.BlendFactor,
                    ColorBlendFunction = BlendFunction.Add
                };
                Draw.SpriteBatch.StandardBegin(SceneAs<Level>().Camera.Matrix, state);
                Image.Render();
                Draw.SpriteBatch.End();
                GameplayRenderer.Begin();

            }
        }
        private IEnumerator appearObject()
        {
            MTexture t = WARP.InputMachine.FilledTex;
            ShineObject o = new ShineObject(t, new Vector2((Passenger.X - Player.Right) / 2 - t.Width / 2, Passenger.Y - t.Height / 2));
            Scene.Add(o);
            yield return null;
        }
        private IEnumerator staticBegin()
        {
            yield return null;
        }
        private IEnumerator powerDown()
        {
            yield return new SwapImmediately((VertexPassenger as TutorialPassenger).TurnOffRoutine());
        }
        private IEnumerator primitiveTurnOn()
        {
            if (VertexPassenger is TutorialPassenger)
            {
                yield return Player.DummyWalkTo(Passenger.Left);
                yield return 0.1f;
                yield return Player.Boop();

                IEnumerator routine()
                {
                    yield return 0.3f;
                    yield return Marker.WalkTo("playerWalkTo", null, true, 1.2f);
                }
                Add(new Coroutine(routine()));
                yield return new SwapImmediately((VertexPassenger as TutorialPassenger).TurnOnRoutine());
            }
        }
        private IEnumerator walkToPrimitive()
        {
            yield return new SwapImmediately(Player.DummyWalkTo(Passenger.X - 8));
            Player.Face(Passenger);
            yield return 0.6f;
        }
        private IEnumerator walkToPrimitiveBehind()
        {
            yield return new SwapImmediately(Player.DummyWalkTo(Passenger.Right + 8));
            Player.Face(Passenger);
            yield return 0.2f;
        }
        public override void OnEnd(Level level)
        {
            Player.EnableMovement();
            if (level.Session.GetFlag("MemoryBoxFirstEncounter"))
            {
                level.Session.SetFlag("PassengerGuidanceWatched");
            }
        }
    }
}
