using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components.Visualizers;
using Celeste.Mod.PuzzleIslandHelper.Components.Visualizers.DSPs;
using Celeste.Mod.PuzzleIslandHelper.Entities.Transitions;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VivHelper.Entities.Spinner2;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Cutscenes
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
            AudioEffectGlobal.AddEffect(d = new Distortion(1), Audio.currentMusicEvent);

            //EndCutscene(level);
        }
        public override void Update()
        {
            base.Update();
            if(d is null) return;
            d.Level += Engine.DeltaTime;
        }
        public override void OnEnd(Level level)
        {

        }
    }
}
