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
    [CustomPassengerCutscene("BeginningTalkOld")]
    [Tracked]
    public class OldBeginningTalkCutscene : PassengerCutscene
    {
        public CalidusButton Button;
        public VertexPassenger VertexPassenger;
        public Calidus Calidus;
        public Func<IEnumerator>[] OldEvents => [
            walkToPrimitive,
            walkToPrimitiveBack,
            primitiveTurnOn,
            playerLookAroundLoop,
            approachChip,
            divide,
            powerDown,
            pressButton,
            lookAtPlayer,
            maddyAngry,
            lookUpLeft,
            playerLookRight,
            lookAtPrimitive,
            floatToPrimitive,
            awkwardPause,
            calidusStutter,
            calidusCloseEye,
            calidusYouWhat,
            Wait1,
            stopLookLoop,
            passengerMoveBack
            ];
        private bool playerLookingAround;
        private float passengerFrom;
        public OldBeginningTalkCutscene(Passenger passenger, Player player) : base(passenger, player)
        {
            if (passenger is VertexPassenger)
            {
                VertexPassenger = passenger as VertexPassenger;
            }
            OncePerSession = true;
        }
        public override void OnBegin(Level level)
        {
            if (VertexPassenger == null) return;
            Calidus = level.Tracker.GetEntity<Calidus>();
            Button = level.Tracker.GetEntity<CalidusButton>();
            if (Calidus == null)
            {
                Calidus = new Calidus(Vector2.Zero, new EntityID(Guid.NewGuid().ToString(), 0));
                level.Add(Calidus);
            }
            Calidus.Visible = Calidus.Active = false;
            Player.DisableMovement();
            Add(new Coroutine(Cutscene()));
        }
        private IEnumerator divide()
        {
            if (Passenger is TutorialPassenger)
            {
                (Passenger as TutorialPassenger).DivideInstruction();
            }
            yield return 1;

            Vector2 pos = Player.TopCenter + Vector2.UnitY * 4;
            Button = new CalidusButton(pos);
            Scene.Add(Button);
            float playerfrom = Player.Position.Y;
            Player.DummyGravity = false;
            while (Player.Position.Y > playerfrom - 32)
            {
                Player.MoveTowardsY(playerfrom - 32, 20 * Engine.DeltaTime);
                yield return null;
            }
            playerLookingAround = false;
            Player.Facing = Facings.Right;
            yield return 0.5f;
            Button.Reveal();
            float from = Button.X;
            playerfrom = Player.Position.X;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 1)
            {
                Button.Position.X = Calc.LerpClamp(from, from - 16, Ease.SineOut(i));
                yield return null;
            }
            yield return 0.5f;
            if (Passenger is TutorialPassenger)
            {
                (Passenger as TutorialPassenger).DivideInstructionEnd();
            }
            yield return 0.9f;

            Player.DummyGravity = true;
            Button.Drop();
            while (!Player.OnGround())
            {
                yield return null;
            }
            yield return null;

            Player.Facing = Facings.Left;
        }
        public IEnumerator Cutscene()
        {
            Player.ForceCameraUpdate = false;
            Add(new Coroutine(Marker.ZoomTo("cam", 1.4f, 1)));
            yield return Marker.WalkTo("playerWalkTo", Facings.Right);
            yield return Textbox.Say("WarpCapsuleFirstNew", OldEvents);
            Calidus?.StartFollowing();
            yield return Level.ZoomBack(1);
            EndCutscene(Level);
        }
        private IEnumerator stopLookLoop()
        {
            playerLookingAround = false;
            yield return null;
        }
        private IEnumerator approachChip()
        {
            passengerFrom = Passenger.X;
            yield return Passenger.MoveToX(Player.Right, 0.8f);
            yield return null;
        }
        private IEnumerator passengerMoveBack()
        {
            yield return Passenger.MoveToX(passengerFrom, 0.8f);
            yield return null;
        }
        /*        private IEnumerator turnOnChip()
                {
                    Color[] colors = [Color.Lime, CalidusButton.PinColor];
                    for (int i = 0; i < 3; i++)
                    {
                        for (int c = 0; c < 2; c++)
                        {
                            CalidusButton.PinColor = colors[c];
                            yield return Engine.DeltaTime * 2;
                        }
                    }
                    CalidusButton.PinColor = colors[0];
                    yield return null;
                }*/
        private IEnumerator powerDown()
        {
            yield return new SwapImmediately((VertexPassenger as TutorialPassenger).TurnOffRoutine());
            yield return null;
        }
        private IEnumerator calidusCloseEye()
        {
            Calidus.CloseEye();
            yield return null;
        }
        private IEnumerator calidusYouWhat()
        {
            Calidus.Normal();
            Calidus?.LookAt(Player);
            yield return null;
        }
        private IEnumerator calidusStutter()
        {
            Calidus?.Look(Calidus.Looking.Left);
            Add(new Coroutine(Calidus.FloatXNaive(-36, 1.3f)));
            yield return null;
        }
        private IEnumerator awkwardPause()
        {
            Calidus?.Look(Calidus.Looking.UpLeft);
            yield return 2f;
            Calidus?.LookAt(Player);
        }
        private IEnumerator floatToPrimitive()
        {
            float fromY = Calidus.Y;
            float fromX = Calidus.X;
            yield return this.MultiRoutine(
                PianoUtils.Lerp(Ease.CubeOut, 0.7f, f => Calidus.Y = Calc.LerpClamp(fromY, Passenger.Y - 4, f)),
                PianoUtils.Lerp(Ease.SineOut, 0.65f, f => Calidus.X = Calc.LerpClamp(fromX, Passenger.X - Calidus.Width - 24, f)));
        }
        private IEnumerator waitThree()
        {
            yield return 3;
        }
        private IEnumerator walkToPrimitive()
        {
            yield return new SwapImmediately(Player.DummyWalkTo(Passenger.X - 8));
            Player.Face(Passenger);
            yield return 0.6f;
        }
        private IEnumerator walkToPrimitiveBack()
        {
            yield return new SwapImmediately(Player.DummyWalkTo(Passenger.Right + 8));
            Player.Face(Passenger);
            yield return 0.2f;
        }
        public override void OnEnd(Level level)
        {
            if (VertexPassenger is TutorialPassenger t)
            {
                t.TurnOff();
                t.Position.X = t.prevPosition;
            }
            playerLookingAround = false;
            Calidus.Active = Calidus.Visible = true;
            Calidus.StartFollowing(Calidus.Looking.Player);
            Calidus.Normal();
            if (WasSkipped)
            {
                Calidus.Position = Player.TopLeft - Vector2.UnitX * 16;
            }
            level.ResetZoom();
            Player.EnableMovement();
            level.Session.SetFlag("BeginningTalkCutsceneWatched");
        }
        private IEnumerator lookAtPrimitive()
        {
            Calidus.LookAt(Passenger);
            yield return null;
        }
        private IEnumerator lookUpLeft()
        {
            Calidus.Look(Calidus.Looking.UpLeft);
            yield return null;
        }
        private IEnumerator maddyAngry()
        {
            yield return null;
        }
        private IEnumerator lookAtPlayer()
        {
            Calidus.LookAt(Player);
            yield return null;
        }
        private void CalidusStartFollowing()
        {
            Calidus.StartFollowing();
        }
        private IEnumerator pressButton()
        {
            /* if (Marker.TryFind("cam", out Vector2 pos))
             {
                 Add(new Coroutine(Level.ZoomAcross(pos - Level.Camera.Position, 1.4f, 1)));
             }
             yield return Player.DummyWalkTo(Button.Right + 2);
             yield return 0.1f;
             yield return Player.Boop(-1);*/
            Player.Duck();
            Button.Press();
            while (Button.Loading)
            {
                yield return null;
            }
            Button.FadeOut();
            yield return null;
        }
        private IEnumerator playerLookLeft()
        {
            Player.Facing = Facings.Left;
            yield return null;
        }
        private IEnumerator playerLookRight()
        {
            Player.Facing = Facings.Right;
            yield return null;
        }
        private IEnumerator playerLookAroundLoop()
        {
            IEnumerator routine()
            {
                playerLookingAround = true;
                while (playerLookingAround)
                {
                    Player.Facing = (Facings)(-(int)Player.Facing);
                    yield return 0.3f;
                }
            }
            Add(new Coroutine(routine()));
            yield return null;
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
                    if (Marker.TryFind("playerWalkTo", out Vector2 pos2))
                    {
                        yield return Player.DummyWalkTo(pos2.X, true, 1.2f);
                    }
                }
                Add(new Coroutine(routine()));
                yield return new SwapImmediately((VertexPassenger as TutorialPassenger).TurnOnRoutine());
            }
        }

    }
}
