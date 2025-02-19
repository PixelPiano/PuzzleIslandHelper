using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.Transitions;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.Programs
{
    [Tracked]
    //[CustomProgram("Access")]
    [Obsolete("Use AccessProgram.cs")]
    public class BetaAccessProgram : WindowContent
    {
        public static bool AccessTeleporting;
        public InputBox Box;
        public BetaAccessProgram(Window window) : base(window)
        {
            Name = "BetaAccess";
        }
        [OnLoad]
        public static void Load()
        {
            AccessTeleporting = false;
        }
        [OnUnload]
        public static void Unload()
        {
            AccessTeleporting = false;
        }
        public override void Update()
        {
            base.Update();
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            ProgramComponents.Add(Box = new InputBox(Window, TryStartWarpRoutine));
        }
        public override void OnOpened(Window window)
        {
            base.OnOpened(window);
        }
        private IEnumerator WaitAnimation(float time)
        {
            Interface.Buffering = true;
            yield return time;
            Interface.Buffering = false;
        }
        public IEnumerator AccessRoutine(string pass, bool success)
        {
            yield return WaitAnimation(Calc.Random.Range(1f, 3f));
            yield return 0.05f;

            if (!success)
            {
                yield break;
            }
            yield return TransitionRoutine(pass);
        }
        public static IEnumerator ForceTransition(string room, bool instant = false, bool buggy = false)
        {
            if (Engine.Scene is not Level level) yield break;
            if (PianoModule.Session.Interface != null && PianoModule.Session.Interface.Interacting)
            {
                yield return PianoModule.Session.Interface.ShutDown(instant);
            }

            level.Add(new BeamMeUp(room, AccessTeleporting));
        }
        public static IEnumerator TransitionRoutine(string pass, bool instant = false, bool buggy = false)
        {
            if (Engine.Scene is not Level level) yield break;
            WarpCapsuleData data = null;//PianoMapDataProcessor.WarpLinks[pass];
            WarpCapsule machine = level.Tracker.GetEntity<WarpCapsule>();
            if (PianoModule.Session.Interface != null && PianoModule.Session.Interface.Interacting)
            {
                yield return PianoModule.Session.Interface.ShutDown(instant, machine != null);
            }
            if (machine != null)
            {
                //machine.SetWarpTarget(pass);
                machine.LeftDoor.MoveToBg();
                machine.RightDoor.MoveToBg();
                yield return machine.MoveTo(1, 0, 0.7f, null);
                machine.Enabled = true;
                level.GetPlayer().StateMachine.State = Player.StNormal;
            }
        }
        private static bool TransitionValid(string id)
        {
            return false;
            //return PianoMapDataProcessor.WarpLinks.ContainsKey(id);
        }
        public bool TryStartWarpRoutine(string pass)
        {
            if (TransitionValid(pass))
            {
                Add(new Coroutine(AccessRoutine(pass, true)));
                return true;
            }
            return false;
        }
        public override void Render()
        {
            base.Render();
        }

    }
}