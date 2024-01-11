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
        public Action OnFinish;
        public DigiAccessWait(float time, Action onFinish) : base(Vector2.Zero)
        {
            Tag |= Tags.Global | Tags.Persistent;
            TimeLeft = time;
            OnFinish = onFinish;
        }
        public override void Update()
        {
            base.Update();
            TimeLeft -= Engine.DeltaTime;
            if (TimeLeft < 0 && OnFinish != null)
            {
                OnFinish.Invoke();
                RemoveSelf();
            }
        }
        private IEnumerator TransitionScene(string to)
        {
            if (Scene is not Level level)
            {
                yield break;
            }
            if (Interface.ActiveInstance != null && Interface.Interacting)
            {
                Interface.ActiveInstance.RemoveWindow();
                yield return 0.2f;
                yield return Interface.ActiveInstance.CloseInterface(false);
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
            RemoveSelf()
        }
    }
}
