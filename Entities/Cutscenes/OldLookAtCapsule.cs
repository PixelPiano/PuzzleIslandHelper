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

    [CustomEvent("PuzzleIslandHelper/LookAtCapsuleOld")]
    public class OldLookAtCapsuleScene : CutsceneEntity
    {
        public OldLookAtCapsuleScene() : base()
        {

        }
        public override void OnBegin(Level level)
        {
            //"BeginningTalkCutsceneWatched"
            if (level.GetPlayer() is Player player)
            {
                player.DisableMovement();
                player.ForceCameraUpdate = true;
                Add(new Coroutine(cutscene(player)));
            }
        }

        private IEnumerator calidusToMarker(string s, float speed = 1f)
        {
            Calidus c = SceneAs<Level>().Tracker.GetEntity<Calidus>();
            WarpCapsule capsule = SceneAs<Level>().Tracker.GetEntity<WarpCapsule>();
            if (c != null && capsule != null && Marker.TryFind("c" + s, out Vector2 position))
            {
                c.StopFollowing();
                c.LookAt(capsule.Center);
                yield return new SwapImmediately(c.Float(position, speed));
            }
        }
        private IEnumerator calidusTo1a()
        {
            yield return calidusToMarker("1a", 0.5f);
        }
        //[Command("add_default_runes", "adds the default set of runes to the rune inventory")]
        public static void AddDefaultRunes()
        {
            WarpCapsule.ObtainedRunes.TryAddRuneRange(WarpCapsule.DefaultRunes);
        }
        private IEnumerator calidusBam()
        {
            yield return 0.8f;
            if (Level.Tracker.GetEntity<Calidus>() is not Calidus c || Level.Tracker.GetEntity<WarpCapsule>() is not WarpCapsule w) yield break;
            Vector2 pos = w.InputMachine.TopLeft + Vector2.One * 8;
            Vector2 from = c.BottomRight;
            yield return PianoUtils.LerpYoyo(Ease.Linear, 0.1f, f => c.BottomRight = Vector2.Lerp(from, pos, f), delegate {/*todo: play sound of calidus ramming into screen*/});
        }
        private IEnumerator screenOn()
        {
            WarpCapsule capsule = Level.Tracker.GetEntity<WarpCapsule>();
            //capsule.InputMachine.Fill();
            yield return null;
        }
        private IEnumerator calidusTo4()
        {
            yield return calidusToMarker("4");
        }
        private IEnumerator calidusTo1()
        {
            yield return lookAtTerminal();
            yield return calidusToMarker("1");
        }
        private IEnumerator calidusTo2()
        {
            yield return calidusToMarker("2", 0.3f);
            yield return 0.8f;
            Calidus c = Level.Tracker.GetEntity<Calidus>();
            if (c != null)
            {
                c.Emotion(Calidus.Mood.Stern);
            }
            yield return 1f;
        }
        private IEnumerator calidusTo3()
        {
            Calidus c = Level.Tracker.GetEntity<Calidus>();
            WarpCapsule w = Level.Tracker.GetEntity<WarpCapsule>();
            if (c != null && w != null)
            {
                c.LookAt(w);
            }
            yield return calidusToMarker("3");
        }
        private IEnumerator lookatplayer()
        {
            Calidus c = SceneAs<Level>().Tracker.GetEntity<Calidus>();
            if (c != null)
            {
                c.LookAt(Scene.GetPlayer());
            }
            yield return null;
        }
        private IEnumerator prep(Player player)
        {
            if (Marker.TryFind("playerWalkTo2", out var position))
            {
                Coroutine walk = new Coroutine(player.DummyWalkTo(position.X));
                Add(walk);
                while (!walk.Finished)
                {
                    yield return null;
                }
                Add(new Coroutine(Level.ZoomTo(new Vector2(160, 90), 1.5f, 1)));
            }
        }
        private IEnumerator cutscene(Player player)
        {
            Add(new Coroutine(prep(player)));
            Add(new Coroutine(calidusTo1()));
            yield return Textbox.Say("LookAtCapsule", calidusTo1, calidusTo2, calidusTo3, lookatplayer, lookAtTerminal, calidusBam, screenOn, calidusTo4, calidusTo1a);
            yield return Level.ZoomBack(1);
            EndCutscene(Level);
        }
        private IEnumerator lookAtTerminal()
        {
            if (Level.Tracker.GetEntity<Calidus>() is not Calidus c || Level.Tracker.GetEntity<WarpCapsule.Machine>() is not WarpCapsule.Machine w) yield break;
            c.LookAt(w);
        }
        public override void OnEnd(Level level)
        {
            Level.ResetZoom();
            if (level.GetPlayer() is Player player)
            {
                player.EnableMovement();
            }
            Calidus c = SceneAs<Level>().Tracker.GetEntity<Calidus>();
            if (c != null)
            {
                c.StartFollowing();
                c.Normal();
                if (WasSkipped)
                {
                    c.SnapToLeader();
                }
            };
            //Level.Tracker.GetEntity<WarpCapsule>()?.InputMachine?.Fill();
            AddDefaultRunes();
        }
    }

}
