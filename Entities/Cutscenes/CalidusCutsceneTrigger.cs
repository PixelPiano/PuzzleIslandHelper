using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.Cutscenes;
using Celeste.Mod.PuzzleIslandHelper.Loaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Cutscenes
{

    [CustomEntity("PuzzleIslandHelper/CalidusCutsceneTrigger")]
    [Tracked]
    public class CalidusCutsceneTrigger : Trigger
    {
        private CalidusCutscene CutsceneEntity;
        public string CutsceneID;
        private bool ActivateOnTransition;
        private bool InCutscene;
        private FlagList FlagList;
        private string startArgs;
        private string endArgs;
        private EntityID id;
        private bool oncePerInstance;
        private bool oncePerSession;
        private bool activated;
        public CalidusCutsceneTrigger(EntityData data, Vector2 offset, EntityID id)
            : base(data, offset)
        {
            this.id = id;
            CutsceneID = data.Attr("cutscene");
            startArgs = data.Attr("startArgs");
            endArgs = data.Attr("endArgs");
            FlagList = new FlagList(data.Attr("flag"));
            Tag |= Tags.TransitionUpdate;
            ActivateOnTransition = data.Bool("activateOnTransition");
            oncePerInstance = data.Bool("oncePerInstance");
            oncePerSession = data.Bool("oncePerSession");
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (!CalidusCutsceneLoader.HasCutscene(CutsceneID))
            {
                Engine.Commands.Log("Could not find Cutscene with id:" + CutsceneID);
                RemoveSelf();
                return;
            }
            CutsceneEntity = CalidusCutsceneLoader.CreateCutscene(CutsceneID, null, null, startArgs, endArgs);
            if (CutsceneEntity == null)
            {
                RemoveSelf();
                Engine.Commands.Log("CutsceneEntity is null");
                return;
            }
            if (ActivateOnTransition && scene.GetPlayer() is Player player)
            {
                OnEnter(player);
            }
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if (!InCutscene && FlagList.State && !CutsceneEntity.GetFlag(Scene as Level))
            {
                if ((oncePerSession || oncePerInstance) && activated) return;
                Triggered = true;
                CutsceneEntity.Player = player;
                InCutscene = true;
                SceneAs<Level>().Add(CutsceneEntity);
                if (oncePerSession)
                {
                    SceneAs<Level>().Session.DoNotLoad.Add(id);
                }
                activated = true;
            }
            else
            {
                Engine.Commands.Log("CutsceneEntity did not activate.");
                Engine.Commands.Log("FlagState:" + (bool)FlagList);
            }
        }
    }
}
