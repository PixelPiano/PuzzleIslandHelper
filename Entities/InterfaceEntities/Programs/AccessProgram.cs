using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.Transitions;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

using static Celeste.Mod.PuzzleIslandHelper.PuzzleData.AccessData;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.Programs
{
    [Tracked]
    [CustomProgram("Access")]
    public class AccessProgram : WindowContent
    {
        public static bool AccessTeleporting;
        public InputBox Box;
        public AccessProgram(Window window) : base(window)
        {
            Name = "Access";
        }

        public static void Load()
        {
            AccessTeleporting = false;
        }
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
            ProgramComponents.Add(Box = new InputBox(Window, CheckIfValidPass));
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
                yield return PianoModule.Session.Interface.CloseInterfaceRoutine(instant);
            }

            level.Add(new BeamMeUp(room, AccessTeleporting));
        }
        public static IEnumerator TransitionRoutine(string pass, bool instant = false, bool buggy = false)
        {
            if (Engine.Scene is not Level level) yield break;
            Link link = PianoModule.AccessData.GetLink(pass);
            if (!TransitionValid(level, link)) yield break;

            if (link.Wait > 0 && !instant)
            {
                level.Add(new TransitionEndingHelper(link.Room, link.Wait));
                yield break;
            }
            if (PianoModule.Session.Interface != null && PianoModule.Session.Interface.Interacting)
            {
                yield return PianoModule.Session.Interface.CloseInterfaceRoutine(instant);
            }
            level.Add(new BeamMeUp(link.Room, AccessTeleporting));
        }
        private static bool TransitionValid(Scene scene, Link link)
        {
            if (link is null)
            {
                return false;
            }
            if (link.Room is not string destination || string.IsNullOrEmpty(destination))
            {
                return false;
            }
            destination = destination.Trim();
            if (PianoModule.Session.Interface is null)
            {
                return false;
            }
            if (scene is null || scene is not Level level)
            {
                return false;
            }
            if (level.Session.MapData.Levels.Find(item => item.Name.ToLower().Equals(destination.ToLower())) == null)
            {
                return false;
            }
            return true;
        }
        public bool CheckIfValidPass(string pass)
        {
            if (PianoModule.AccessData is null)
            {
                return false;
            }
            bool valid = PianoModule.AccessData.HasID(pass);
            Add(new Coroutine(AccessRoutine(pass, valid)));
            return valid;
        }
        public override void Render()
        {
            base.Render();
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