using Celeste.Mod.PuzzleIslandHelper.Entities.BetterInterfaceEntities;
using Celeste.Mod.PuzzleIslandHelper.Entities.Transitions;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using static Celeste.Mod.PuzzleIslandHelper.PuzzleData.AccessData;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Programs
{
    public class DigiAccess : WindowContent
    {
        public static bool AccessTeleporting;
        public DigiAccess(BetterWindow window) : base(window)
        {
            Name = "access";
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
                Console.WriteLine("Unsuccessful");
                yield break;
            }
            yield return TransitionRoutine(pass);
        }
        public static IEnumerator ForceTransition(string room, bool instant = false, bool buggy = false)
        {
            if (Engine.Scene is not Level level) yield break;
            if (PianoModule.Session.Interface != null && PianoModule.Session.Interface.Interacting)
            {
                yield return PianoModule.Session.Interface.CloseInterface(instant);
            }

            level.Add(new BeamMeUp(room, AccessTeleporting));
        }
        public static IEnumerator TransitionRoutine(string pass, bool instant = false, bool buggy = false)
        {
            Link link = PianoModule.AccessData.GetLink(pass);

            //this massize if statement is my child it is my beloved baby and i will fight anybody who threatens its existence
            if (link is null || link.Room is not string destination || string.IsNullOrEmpty(destination) ||
                PianoModule.Session.Interface == null || Engine.Scene is not Level level ||
                level.Session.MapData.Levels.Find(item => item.Name.Equals(destination)) == null) yield break;

            if (link.Wait && !instant)
            {
                level.Add(new TransitionEndingHelper(link.Room, 25));
                yield break;
            }
            if (PianoModule.Session.Interface != null && PianoModule.Session.Interface.Interacting)
            {
                yield return PianoModule.Session.Interface.CloseInterface(instant);
            }
            level.Add(new BeamMeUp(link.Room, AccessTeleporting));
        }

        public bool CheckIfValidPass(string pass)
        {
            if (PianoModule.AccessData is null)
            {
                Console.WriteLine("Access data is null");
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
            if (Scene is not Level level) yield break;
            yield return DigiAccess.ForceTransition(Room, true, DigiAccess.AccessTeleporting);
            RemoveSelf();
        }
        public override void Render()
        {
            base.Render();
            ActiveFont.DrawOutline(Time.ToString(), Vector2.Zero, Vector2.One / 2, Vector2.One, Color.White, 8, Color.Black);
        }
    }
}