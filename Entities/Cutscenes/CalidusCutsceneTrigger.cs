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
        private CutsceneEntity Cutscene;
        private bool ActivateOnTransition;
        private bool InCutscene;
        private List<string> flags = new();
        public bool FlagState
        {
            get
            {
                if (Scene is not Level level) return false;
                List<bool> bools = new();
                foreach (string s in flags)
                {

                    if (string.IsNullOrEmpty(s)) continue;
                    bool inverted = s[0] == '!';
                    bool flagState = level.Session.GetFlag(s);
                    bools.Add(flagState || !flagState && inverted);
                }
                foreach (bool b in bools)
                {
                    if (!b) return false;
                }
                return true;
            }
        }
        public CalidusCutsceneTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            flags = data.Attr("flag").Replace(" ", "").Split(',').ToList();
            
            Tag |= Tags.TransitionUpdate;
            Cutscene = new CalidusCutscene(data.Enum<CalidusCutscene.Cutscenes>("cutscene"));
            ActivateOnTransition = data.Bool("activateOnTransition");
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            (scene as Level).InCutscene = false;
            if (!FlagState)
            {
                RemoveSelf();
            }
            else
            {
                Player player = (scene as Level).Tracker.GetEntity<Player>();
                if (ActivateOnTransition && player is not null)
                {
                    OnEnter(player);
                }
            }
        }
        public override void OnEnter(Player player)
        {
            if (!InCutscene && FlagState)
            {
                InCutscene = true;
                SceneAs<Level>().Add(Cutscene);
            }
        }
    }
}
