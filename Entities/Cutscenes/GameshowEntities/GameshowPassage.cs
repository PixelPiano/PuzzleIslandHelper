using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Linq;
using static MonoMod.InlineRT.MonoModRule;

namespace Celeste.Mod.PuzzleIslandHelper.Cutscenes.GameshowEntities
{
    [CustomEntity("PuzzleIslandHelper/GameshowPassage")]
    [Tracked]
    public class GameshowPassage : Passage
    {
        public static string LastRoomVisited;
        private bool toSet;
        public GameshowPassage(EntityData data, Vector2 offset) : base(data.Position + offset, data.Attr("teleportTo"), data.Enum<Facings>("facing"), data.Float("fadeTime"), Color.Yellow, "objects/PuzzleIslandHelper/gameshow/gameshowPassage/")
        {
            toSet = data.Bool("returningToSet");
            if (toSet)
            {
                EndPlayerState = Player.StDummy;
            }
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (toSet)
            {
                OnTransition = delegate { LastRoomVisited = (scene as Level).Session.Level; };
            }
        }
    }
}
