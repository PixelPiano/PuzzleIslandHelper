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
    [CustomEvent("PuzzleIslandHelper/FindBlock")]
    public class FindBlockCutscene : CutsceneEntity
    {
        private Player player;
        private Calidus calidus;
        private Vector2 calidusTo;
        private Vector2 playerTo;
        private Coroutine calidusToRoutine;
        private Coroutine playerToRoutine;
        private Vector2? lookAt;
        public override void OnBegin(Level level)
        {
            if (Marker.TryFind("calidusTo", out calidusTo) && Marker.TryFind("playerTo", out playerTo))
            {
                if (level.GetPlayer() is not Player player) return;
                this.player = player;
                if (level.Tracker.GetEntity<Calidus>() is Calidus c)
                {
                    calidus = c;
                    c.StopFollowing();
                }
                else
                {
                    Vector2 position = player.Position;
                    if (Marker.TryFind("calidusFrom", out Vector2 marker))
                    {
                        position = marker;
                    }
                    calidus = Calidus.Create(position, false, true);
                }
                if (Marker.TryFind("look", out Vector2 lookTarget))
                {
                    lookAt = lookTarget;
                    calidus.LookAt(lookTarget);
                }
                Add(new Coroutine(Cutscene()));
            }
        }
        private IEnumerator Cutscene()
        {
            calidusToRoutine = new Coroutine(calidus.FloatTo(calidusTo));
            playerToRoutine = new Coroutine(player.DummyWalkTo(playerTo.X));
            Coroutine zoomRoutine = new Coroutine(Marker.ZoomTo("zoom", 1.5f, 1));
            Add(calidusToRoutine, playerToRoutine, zoomRoutine);
            while (!calidusToRoutine.Finished || !playerToRoutine.Finished)
            {
                yield return null;
            }
            player.Facing = Facings.Left;
            yield return Textbox.Say("CalidusFindBlock");
            yield return null;
            EndCutscene(Level);
        }
        private IEnumerator calidusInspect()
        {
            if (lookAt.HasValue)
            {
                Vector2 center = lookAt.Value;
                Vector2 offset = center - calidus.Position;
                float radius = offset.Length();
                float angle = offset.Angle();
                yield return calidus.RotateAroundApproach(center, radius, angle, angle + 135f.ToRad(), 1.2f, 1, Ease.CubeIn);
                yield return 0.5f;
                yield return calidus.RotateAroundApproach(center, radius, angle, angle + 135f.ToRad(), 1.2f, 1, Ease.CubeIn);
            }
            yield return null;
        }
        private IEnumerator boxFlash()
        {
            Level.Flash(Color.White); //temporary
            yield return null;
        }
        private IEnumerator nudgeBox()
        {
            yield return null;
        }
        private IEnumerator immediatelyStop()
        {
            yield return null;
        }
        private IEnumerator memoryDustFloats()
        {
            yield return null;
        }
        public override void OnEnd(Level level)
        {
            if (WasSkipped)
            {
                level.ResetZoom();
            }
            calidusToRoutine.Cancel();
            playerToRoutine.Cancel();
        }
    }
}
