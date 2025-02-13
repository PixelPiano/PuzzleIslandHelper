using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Cutscenes
{

    [CustomEntity("PuzzleIslandHelper/CalidusCutsceneTrigger")]
    [Tracked]
    public class CalidusCutsceneTrigger : Trigger
    {
        public CalidusCutscene.Cutscenes Progress => PianoModule.Session.CalidusCutscene;
        private CutsceneEntity CutsceneEntity;
        public CalidusCutscene.Cutscenes Cutscene;
        private bool ActivateOnTransition;
        private bool InCutscene;
        //private List<string> flags = new();
        private List<(string, bool)> flags = new();
        public bool FlagState => PianoUtils.CheckAll(flags);
        public CalidusCutsceneTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            flags = PianoUtils.ParseFlagsFromString(data.Attr("flag"));
            Tag |= Tags.TransitionUpdate;
            Cutscene = data.Enum<CalidusCutscene.Cutscenes>("cutscene");
            CutsceneEntity = new CalidusCutscene(Cutscene);
            ActivateOnTransition = data.Bool("activateOnTransition");
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            (scene as Level).InCutscene = false;
            if (ActivateOnTransition && scene.GetPlayer() is Player player)
            {
                if (!FlagState)
                {
                    RemoveSelf();
                }
                else if (player is not null)
                {
                    OnEnter(player);
                }
            }
        }
        public override void OnEnter(Player player)
        {
            if (Progress == Cutscene && !InCutscene && FlagState)
            {
                InCutscene = true;
                SceneAs<Level>().Add(CutsceneEntity);
            }
        }
    }
}
