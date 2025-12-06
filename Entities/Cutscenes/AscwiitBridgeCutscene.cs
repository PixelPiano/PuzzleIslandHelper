using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora.Passengers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;


namespace Celeste.Mod.PuzzleIslandHelper.Entities.Cutscenes
{
    [Tracked]
    [CustomEvent("PuzzleIslandHelper/AscwiitBridgeCutscene")]
    public class AscwiitBridgeCutscene : CutsceneEntity
    {
        public List<Ascwiit> Birds = [];
        public override void OnBegin(Level level)
        {
            level.GetPlayer()?.DisableMovement();
            foreach (Ascwiit bird in Level.Tracker.GetEntities<Ascwiit>())
            {
                Birds.Add(bird);
            }
            Add(new Coroutine(cutscene()));

        }

        private IEnumerator cutscene()
        {

            yield return 2f;
            foreach (Ascwiit bird in Birds)
            {
                bird.Squawk();
            }
            yield return 0.5f;
            Level.Session.SetFlag("ascwiitBridge:" + Level.Session.Level);
            yield return 1f;
            foreach (Ascwiit bird in Birds)
            {
                bird.PathID = "bridgeEscape";
            }
            EndCutscene(Level);
        }
        public override void OnEnd(Level level)
        {
            level.GetPlayer()?.EnableMovement();
            foreach (Ascwiit bird in Birds)
            {
                level.Session.DoNotLoad.Add(bird.id);
            }
            Level.Session.SetFlag("ascwiitBridgeActive:" + Level.Session.Level);
            level.Session.SetFlag("BridgeCutsceneWatched:" + Level.Session.Level);
        }
    }
}
