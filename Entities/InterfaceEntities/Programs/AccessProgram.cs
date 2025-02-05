using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.Transitions;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.Programs
{
    [Tracked]
    [CustomProgram("Access")]
    public class AccessProgram : WindowContent
    {
        public static bool AccessTeleporting;
        public List<SegmentBox> Boxes = [];
        public List<TextHelper.Snippet> Snippets = [];
        public AccessProgram(Window window) : base(window)
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
            SetBoxPositions();
        }
        public bool IsValidCharacter(char c)
        {
            return char.IsNumber(c);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            SegmentBox box1 = new SegmentBox(Window, 3, 2, CheckInput, IsValidCharacter);
            SegmentBox box2 = new SegmentBox(Window, 2, 2, CheckInput, IsValidCharacter);
            SegmentBox box3 = new SegmentBox(Window, 2, 2, CheckInput, IsValidCharacter);
            SegmentBox box4 = new SegmentBox(Window, 1, 2, CheckInput, IsValidCharacter);
            Boxes = [box1, box2, box3, box4];
            for (int i = 0; i < Boxes.Count; i++)
            {
                SegmentBox b = Boxes[i];
                ProgramComponents.Add(b);
            }
        }
        public bool CheckInput(string i)
        {
            return CheckAllInput();
        }
        public bool CheckAllInput()
        {
            return false;
        }
        public void SetBoxPositions()
        {
            float width = 0;
            for (int i = 0; i < Boxes.Count; i++)
            {
                if(i < Boxes.Count - 1) Snippets[i].Offset.X = Boxes[i].Width + Boxes[i].CellWidth / 2 - ActiveFont.Measure(".").X / 6 / 2;
                width += Boxes[i].Width + Boxes[i].CellWidth;
            }
            float x = Window.CaseWidth / 2 - width / 2;
            for (int i = 0; i < Boxes.Count; i++)
            {
                Boxes[i].Position.X = x;
                x += Boxes[i].Width + Boxes[i].CellWidth;
            }
        }
        public override void OnOpened(Window window)
        {
            base.OnOpened(window);
            for (int i = 0; i < Boxes.Count; i++)
            {
                if (i < Boxes.Count - 1)
                {
                    Snippets.Add(Boxes[i].Helper.AddSnippet(".", new Vector2(0, Boxes[i].Height - ActiveFont.Measure(".").Y / 6f)));
                }
            }
            SetBoxPositions();
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
            WarpCapsuleData data = PianoMapDataProcessor.WarpLinks[pass];
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
            return PianoMapDataProcessor.WarpLinks.ContainsKey(id);
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
        public override void WindowRender()
        {
            base.WindowRender();
        }

    }
    public class TransitionEndingHelper : Entity
    {
        public float Time;
        public bool InRoutine;
        public string Room;
        public TransitionEndingHelper(string room, float time = 0) : base(Vector2.Zero)
        {
            Room = room;
            Time = time;
            Tag |= Tags.Global | Tags.Persistent | TagsExt.SubHUD;
            Depth = -1000011;
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Add(new Coroutine(TimedTransition()));
        }
        private IEnumerator TimedTransition()
        {
            for (float i = Time; i > 0; i -= Engine.DeltaTime)
            {
                Time = i;
                yield return null;
            }
            yield return AccessProgram.ForceTransition(Room, true, AccessProgram.AccessTeleporting);
            RemoveSelf();
        }
        public override void Render()
        {
            base.Render();
            ActiveFont.DrawOutline(Time.ToString(),/*(float)Math.Round(Time, 2)).ToString(),*/ Vector2.Zero, Vector2.Zero, Vector2.One, Color.White, 8, Color.Black);
        }
    }
}