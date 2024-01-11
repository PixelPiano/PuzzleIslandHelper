using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.BetterInterfaceEntities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Programs
{
    [Tracked]
    public class DigiAccessWait : Entity
    {
        public float TimeLeft;
        public string To;
        public bool Running;
        public Interface Interface;
        public DigiAccessWait(Interface inter, float time,string to) : base(Vector2.Zero)
        {
            Interface = inter;
            Tag |= Tags.Global | Tags.Persistent;
            TimeLeft = time;
            To = to;
        }
        public override void Update()
        {
            base.Update();
            TimeLeft -= Engine.DeltaTime;
            if (TimeLeft < 0 && !string.IsNullOrEmpty(To) && !Running)
            {
                Add(new Coroutine(TransitionScene(To)));
            }
        }
        private IEnumerator TransitionScene(string to)
        {
            if (Scene is not Level level)
            {
                yield break;
            }
            Running = true;
            if (Interface != null && Interface.Interacting)
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
            RemoveSelf();
        }
    }
}
