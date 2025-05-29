using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora.Passengers;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Cutscenes
{
    [CustomPassengerCutscene("GetPoster")]
    [Tracked]
    public class GetPosterCutscene : PassengerCutscene
    {
        public CalidusButton Button;
        public VertexPassenger VertexPassenger;
        public Calidus Calidus;
        public GetPosterCutscene(Passenger passenger, Player player) : base(passenger, player)
        {
            Depth = -1000000;
            if (passenger is VertexPassenger)
            {
                VertexPassenger = passenger as VertexPassenger;
            }
            OncePerSession = true;
        }
        public override void OnBegin(Level level)
        {
            if (VertexPassenger == null) return;
            Calidus = level.Tracker.GetEntity<Calidus>() ?? Calidus.Create();
            Calidus.Visible = Calidus.Active = false;
            Player.DisableMovement();
            Add(new Coroutine(Cutscene()));
        }
        public IEnumerator Cutscene()
        {
            ZoomTo(VertexPassenger.TopCenter - Vector2.UnitY * 32, 1.5f, 1);
            yield return VertexPassenger.PlayerStepBack(Player);
            VertexPassenger.FacePlayer(Player);
            yield return Textbox.Say("GetPoster", walkToPlayer, walkBack, wait1, walkAway, maddyStopAndTurn, passengerLeave, maddyWalkAway);
            yield return PianoUtils.Lerp(Ease.Linear, 1, f => alpha = f);
            Reset();
            yield return 0.1f;
            yield return PianoUtils.Lerp(Ease.Linear, 1, f => alpha = 1 - f);
            alpha = 0;
            EndCutscene(Level);
            yield return null;
        }
        public void Reset()
        {
            //set player position
            //set calidus position
            Calidus.StartFollowing();
            Calidus.Emotion(Calidus.Mood.Normal);
            Calidus.LookAt(Player);
            Player.Facing = Facings.Left;
            Level.ResetZoom();
            Level.Camera.Position = Player.CameraTarget;
        }
        private float passengerXOrig;
        private IEnumerator walkToPlayer()
        {
            passengerXOrig = VertexPassenger.X;
            yield return VertexPassenger.WalkToX(Player.X - 8);
        }
        private IEnumerator walkBack()
        {
            yield return VertexPassenger.WalkToX(passengerXOrig, 1, true);
        }
        private IEnumerator wait1()
        {
            yield return 1;
        }
        private bool maddyWalking;
        private IEnumerator walkAway()
        {
            Add(new Coroutine(Calidus.FloatTo(Calidus.Position - Vector2.UnitX * 50)));
            float from = Player.X;
            float to = from - 40;
            Add(new Coroutine(Player.DummyWalkTo(to)));
            while ((Player.X - to) / (from - to) > 0.3f)
            {
                yield return null;
            }
        }
        private IEnumerator maddyStopAndTurn()
        {
            yield return 0.6f;
            Player.Facing = Facings.Right;
        }
        private IEnumerator passengerLeave()
        {
            yield return VertexPassenger.WalkX(40);
            VertexPassenger.RemoveSelf();
            yield return null;
        }
        private IEnumerator maddyWalkAway()
        {
            yield return 0.7f;
            yield return Player.DummyWalkTo(Player.X + 30);
        }
        private float alpha;
        public override void Render()
        {
            base.Render();
            if (alpha > 0) Draw.Rect(Level.Camera.Position, 320, 180, Color.Black * alpha);
        }
        public override void OnEnd(Level level)
        {
            if (WasSkipped)
            {
                VertexPassenger.RemoveSelf();
                Calidus.StartFollowing(Calidus.Looking.Player);
                Calidus.Normal();
                level.ResetZoom();
            }
            Player.EnableMovement();
        }

    }
}
