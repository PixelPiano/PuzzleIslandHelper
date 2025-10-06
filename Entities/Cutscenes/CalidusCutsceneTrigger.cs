using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
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
        private string endArgsData
        {
            get
            {
                if(CutsceneEntity != null)
                {
                    return CutsceneEntity.EndArgsData;
                }
                else
                {
                    return "";
                }
            }
        }
        private EntityID id;
        private bool oncePerInstance;
        private bool oncePerSession;
        private bool activated;
        public bool Talker;
        public DotX3 Talk;
        public CalidusCutsceneTrigger(EntityData data, Vector2 offset, EntityID id)
            : base(data, offset)
        {
            this.id = id;
            CutsceneID = data.Attr("cutscene");
            startArgs = data.Attr("startArgs");
            endArgs = data.Attr("endArgs");
            FlagList = new FlagList(data.Attr("flag"));
            Tag |= Tags.TransitionUpdate;
            Talker = data.Bool("talker");
            ActivateOnTransition = data.Bool("activateOnTransition");
            oncePerInstance = data.Bool("oncePerInstance");
            oncePerSession = data.Bool("oncePerSession");
            if (Talker)
            {
                Add(Talk = new DotX3(Collider, (player) =>
                {
                    Activate(player);
                }));
            }
        }
        public override void Update()
        {
            base.Update();
            if(Talk != null)
            {
                Talk.Enabled = FlagList;
            }
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (!CalidusCutsceneLoader.HasCutscene(CutsceneID))
            {
                RemoveSelf();
                return;
            }
            CutsceneEntity = CalidusCutsceneLoader.CreateCutscene(CutsceneID, null, null, startArgs, endArgs);
            if (CutsceneEntity == null)
            {
                RemoveSelf();
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
            if (!Talker)
            {
                Activate(player);
            }
        }
        public void Activate(Player player)
        {
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
        }
    }
}
