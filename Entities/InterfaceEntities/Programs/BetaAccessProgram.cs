using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.Transitions;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using Celeste.Mod.PuzzleIslandHelper.Entities.WARP;
using static MonoMod.InlineRT.MonoModRule;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.Programs
{
    [Tracked]
    [CustomProgram("Access")]
    //[Obsolete("Use AccessProgram.cs")]
    public class BetaAccessProgram : WindowContent
    {
        public static bool AccessTeleporting;
        public InputBox Box;
        private Button placeholderButton;
        public BetaAccessProgram(Window window) : base(window)
        {
            Name = "Access";
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
            //ProgramComponents.Add(Box = new InputBox(Window, TryStartWarpRoutine));
            Circle circle = new Circle(27 / 2f);
            ProgramComponents.Add(placeholderButton = new Button(Window, circle, "greenCircle", OnClicked));
            placeholderButton.Position = new Vector2(Window.WindowWidth / 2, Window.WindowHeight / 2) - new Vector2(placeholderButton.Width / 2, placeholderButton.Height / 2);
        }
        public void OnClicked()
        {
            Scene.Add(new cutscene(this));
        }

        private class cutscene : CutsceneEntity
        {
            public BetaAccessProgram Program;
            public cutscene(BetaAccessProgram program) : base()
            {
                Program = program;
            }
            public override void OnBegin(Level level)
            {
                Add(new Coroutine(routine()));
            }
            private IEnumerator routine()
            {
                yield return Program.AccessRoutine("digiCalidus1",true);
                EndCutscene(Level);
            }
            public override void OnEnd(Level level)
            {
                if (WasSkipped)
                {
                    Program.Interface.CloseInterface(true);
                    level.GetPlayer()?.EnableMovement();
                    if(level.Tracker.GetEntity<WarpCapsuleBeta>() is var machine)
                    {
                        machine.RoomName = "digiCalidus1";
                    }
                }
            }
        }
        public override void OnOpened(Window window)
        {
            base.OnOpened(window);
        }
        public IEnumerator WaitAnimation(float time)
        {
            Interface.Buffering = true;
            yield return time;
            Interface.Buffering = false;
        }
        public IEnumerator AccessRoutine(string room, bool success)
        {
            yield return WaitAnimation(Calc.Random.Range(1f, 3f));
            yield return 0.05f;
            if (!success)
            {
                yield break;
            }
            yield return TransitionRoutine(room);
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
        public IEnumerator TransitionRoutine(string room, bool instant = false, bool buggy = false)
        {
            if (Engine.Scene is not Level level) yield break;
            WarpCapsuleBeta machine = level.Tracker.GetEntity<WarpCapsuleBeta>();
            if (PianoModule.Session.Interface != null && PianoModule.Session.Interface.Interacting)
            {
                yield return PianoModule.Session.Interface.ShutDown(true, machine != null);
            }
            if (machine != null)
            {
                machine.LeftDoor.MoveToBg();
                machine.RightDoor.MoveToBg();
                machine.RoomName = room;
                machine.LockPlayerState = true;
                level.GetPlayer().StateMachine.State = Player.StNormal;
            }
        }

        //leftover from ip address puzzle
        public static PuzzleData.AccessData.Link GetLink(string id)
        {
            if (PianoModule.AccessData.HasID(id))
            {
                return PianoModule.AccessData.GetLink(id);
            }
            return null;
        }
        //ditto
        public bool TryStartWarpRoutine(string pass)
        {
            if (GetLink(pass) is var data)
            {
                Add(new Coroutine(AccessRoutine(data.Room, true)));
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