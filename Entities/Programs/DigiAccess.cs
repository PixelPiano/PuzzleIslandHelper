using Celeste.Mod.PuzzleIslandHelper.Entities.BetterInterfaceEntities;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using static Celeste.Mod.PuzzleIslandHelper.PuzzleData.AccessData;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Programs
{
    public class DigiAccess : WindowContent
    {
        private List<Link> Links => PianoModule.AccessData.Links;
        public DigiAccess(BetterWindow window) : base(window)
        {
            Name = "access";
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
        private IEnumerator FailedRoutine()
        {
            if (Scene is not Level level)
            {
                yield break;
            }
            yield return WaitAnimation(Calc.Random.Range(1f, 3f));
            yield return 0.05f;
        }
        private IEnumerator AccessRoutine(string pass)
        {
            if (Scene is not Level level)
            {
                yield break;
            }
            yield return WaitAnimation(Calc.Random.Range(1f, 3f));
            yield return 0.05f;

            Link link = PianoModule.AccessData.GetLink(pass);
            if (link != null)
            {
                string destination = link.Room;
                if (!string.IsNullOrEmpty(destination))
                {
                    if (level.Session.MapData.Levels.Find(item => item.Name.Equals(destination)) == null)
                    {
                        yield break;
                    }
                    if (link.Wait)
                    {

                    }
                    else
                    {
                        Interface.RemoveWindow();
                        yield return 0.2f;
                        yield return Interface.CloseInterface(false);
                        #region Transition
                        if (level.Session.MapData.Levels.Find(item => item.Name.Equals(destination)) != null)
                        {
                            level.Add(new TransitionManager(TransitionManager.Type.BeamMeUp, destination));
                            TransitionManager.Finished = false;
                            while (!TransitionManager.Finished)
                            {
                                yield return null;
                            }
                        }
                        #endregion
                    }

                }


            }
        }
        public void AddTransition(string to)
        {
            Add(new Coroutine(TransitionScene(to)));
        }
        private IEnumerator TransitionScene(string to)
        {
            if (Scene is not Level level)
            {
                yield break;
            }
            if (Interface.Interacting)
            {
                Interface.RemoveWindow();
                yield return 0.2f;
                yield return Interface.CloseInterface(false);
            }
            if (level.Session.MapData.Levels.Find(item => item.Name.Equals(to)) != null)
            {
                level.Add(new TransitionManager(TransitionManager.Type.BeamMeUp, to));
                TransitionManager.Finished = false;
                while (!TransitionManager.Finished)
                {
                    yield return null;
                }
            }
        }
        public bool CheckIfValidPass(string pass)
        {
            bool result = false;
            if (PianoModule.AccessData.HasID(pass))
            {
                result = true;
                Add(new Coroutine(AccessRoutine(pass)));
            }
            else
            {
                Add(new Coroutine(FailedRoutine()));
            }
            return result;
        }
        public override void Render()
        {
            base.Render();

        }

    }
}