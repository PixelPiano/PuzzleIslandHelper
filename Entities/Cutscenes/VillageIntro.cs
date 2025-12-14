using Celeste.Mod.Entities;
using Celeste.Mod.LuaCutscenes;
using Celeste.Mod.PuzzleIslandHelper.Cutscenes.GameshowEntities;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora.Passengers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Cutscenes
{
    [CustomEvent("PuzzleIslandHelper/VillageIntro")]
    [Tracked]
    public class VillageIntroCutscene : CutsceneEntity
    {
        public TallPassenger Elder;
        private Player player;
        public override void OnBegin(Level level)
        {
            player = level.GetPlayer();
            Elder = level.Tracker.GetEntity<TallPassenger>();
            Add(new Coroutine(cutscene()));
        }
        private IEnumerator elderTurn()
        {
            Elder?.FacePlayer(player);
            yield return null;
        }
        private IEnumerator cutscene()
        {
            player.DisableMovement();
            player.Face(Elder);
            if (Marker.TryFind("camera", out Vector2 camPos))
            {
                Add(new Coroutine(CameraTo(camPos - new Vector2(160, 90), 1, Ease.SineInOut)));
            }
            yield return Textbox.Say("VillageIntro", elderTurn);
            EndCutscene(Level);
        }
        public override void OnEnd(Level level)
        {
            level.GetPlayer()?.EnableMovement();
            level.Session.SetFlag("TalkedToElder");
        }
    }
}
