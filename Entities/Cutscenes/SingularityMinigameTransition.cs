using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.Singularity;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Cutscenes
{
    [Tracked]
    public class MinigameTransition : CutsceneEntity
    {
        //public Minigame Minigame;
        public SingularityActor Singularity;
        public enum Orbs
        {
            R, G, B
        }
        public Orbs Orb;
        public MinigameTransition(SingularityActor singularity, Orbs orb) : base()
        {
            Singularity = singularity;
            Orb = orb;
        }
        public override void OnBegin(Level level)
        {
            level.DisableMovement();
            Add(new Coroutine(Routine()));
        }

        public override void OnEnd(Level level)
        {
            level.DisableMovement();
/*            Minigame = Orb switch
            {
                Orbs.R => new MadelineMinigame(Singularity),
                Orbs.G => new CalidusMinigame(Singularity),
                _ => new RaniMinigame(Singularity)
            };
            Singularity.Active = false;
            Scene.Add(Minigame);*/
        }
        public IEnumerator Routine()
        {
            yield return CameraTo(Singularity.Position - new Vector2(160, 90), 1);
            //orb grow routine
            yield return null;
            EndCutscene(Level);
        }
    }
}
