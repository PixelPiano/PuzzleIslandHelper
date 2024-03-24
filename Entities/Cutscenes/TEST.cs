using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components.Visualizers;
using Celeste.Mod.PuzzleIslandHelper.Components.Visualizers.DSPs;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.Transitions;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Cutscenes
{
    [Tracked]
    public class TEST : CutsceneEntity
    {
        public Chorus Chorus;
        public Distortion d;
        public TEST() : base()
        {

        }
        public override void OnBegin(Level level)
        {
            Add(new Coroutine(cut()));
            //EndCutscene(level);
        }
        private IEnumerator cut()
        {
            Level.Add(new BeamMeUp("digiD1"));
            Level.Add(new BeamMeUp("digiA1", true));
            yield return null;
        }
        public override void Update()
        {
            base.Update();
            if (d is null) return;
            d.Level += Engine.DeltaTime;
        }
        public override void OnEnd(Level level)
        {

        }
    }
}
