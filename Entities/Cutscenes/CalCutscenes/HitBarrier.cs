using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora.Passengers;
using Celeste.Mod.PuzzleIslandHelper.Entities.WARP;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using static Celeste.Mod.PuzzleIslandHelper.Entities.Calidus;


namespace Celeste.Mod.PuzzleIslandHelper.Entities.Cutscenes
{
    [CalidusCutscene("HitBarrier")]
    public class HitBarrier : CalidusCutscene
    {
        private class calidusTeleporter : Trigger
        {
            public bool triggered;
            private Coroutine glitchRoutine;
            private string markerName;
            public calidusTeleporter(EntityData data, Vector2 offset) : base(data, offset)
            {
                markerName = data.Attr("marker");
                Add(glitchRoutine = new Coroutine(false));
                Add(new CalidusCollider(OnCollide));
            }
            private void OnCollide(Calidus c)
            {
                triggered = true;
                glitchRoutine.Replace(glitch());
                QuickGlitch.Create(c, new NumRange2(2, 5), Vector2.One, 0.2f, 6, 0.5f);
                if(Marker.TryFind(markerName, out Vector2 position))
                {
                    c.MoveTo(position);
                }
            }
            public override void Removed(Scene scene)
            {
                base.Removed(scene);
                Glitch.Value = 0;
            }
            private IEnumerator glitch()
            {
                Glitch.Value = 0.01f;
                yield return 0.2f;
                Glitch.Value = 0;
            }
        }
        private bool calidusMoving;
        public HitBarrier(Player player = null, Calidus calidus = null, Arguments start = null, Arguments end = null) : base(player, calidus, start, end)
        {

        }
        public override IEnumerator Cutscene(Level level)
        {
            //StoolPickupBarrier barrier = level.Tracker.GetEntity<StoolPickupBarrier>();
            //Add(new Coroutine(Player.DummyWalkTo(barrier.X - 16)));
            yield return Textbox.Say("CalidusHitBarrier", PlayerLookRight, cNormal,startMoving,stopMoving);
        }
        private IEnumerator startMoving()
        {
            calidusMoving = true;
            Calidus.StopFollowing();
            while (calidusMoving)
            {
                Calidus.MoveH(-20 * Engine.DeltaTime);
                yield return null;
            }
        }
        private IEnumerator stopMoving()
        {
            calidusMoving = false;
            yield return null;
        }
        private IEnumerator cNormal()
        {
            Calidus.Normal();
            yield return null;
        }
    }
}
