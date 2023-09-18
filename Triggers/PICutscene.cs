using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Triggers
{

    [CustomEntity("PuzzleIslandHelper/PICutscene")]
    public class PICutscene : Trigger
    {
        private int CutsceneNum;
        private bool OncePerRoom;
        private bool OncePerPlaythrough;
        private bool ActivateOnTransition;
        private bool Entered;
        private bool InCutscene;

        public PICutscene(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            CutsceneNum = data.Int("cutscene");
            OncePerRoom = data.Bool("oncePerRoom");
            OncePerPlaythrough = data.Bool("oncePerPlaythrough");
            ActivateOnTransition = data.Bool("activateOnTransition");
        }
        private IEnumerator Cutscene(int cutscene, Player player)
        {
            switch (cutscene)
            {
                case 0:
                    break;
                #region 1
                case 1:

                    break;
                #endregion
                #region 2
                case 2:
                    break;
                #endregion
                #region 3
                case 3:
                    break;
                #endregion
                #region 4
                case 4:
                    break;
                #endregion
                #region 5
                case 5:
                    break;
                #endregion
                #region 6
                case 6:
                    break;
                    #endregion
            }
            yield return null;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);

            if (OncePerPlaythrough)
            {
                if (PianoModule.SaveData.UsedCutscenes.Contains(CutsceneNum))
                {
                    RemoveSelf();
                }
            }
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Player player = (scene as Level).Tracker.GetEntity<Player>();
            if (ActivateOnTransition && player is not null)
            {
                OnEnter(player);
                Entered = true;
            }
        }
        public override void OnEnter(Player player)
        {
            if (Entered)
            {
                return;
            }

            if (OncePerRoom || OncePerPlaythrough)
            {
                Entered = true;
            }

            if (!InCutscene)
            {
                Add(new Coroutine(Cutscene(CutsceneNum, player)));
            }

            if (OncePerPlaythrough)
            {
                PianoModule.SaveData.UsedCutscenes.Add(CutsceneNum);
            }
        }

        public override void OnLeave(Player player)
        {
        }
        public static void TeleportTo(Scene scene, Player player, string room, Player.IntroTypes introType = Player.IntroTypes.Transition, Vector2? nearestSpawn = null)
        {
            Level level = scene as Level;
            if (level != null)
            {
                level.OnEndOfFrame += delegate
                {
                    level.TeleportTo(player, room, introType, nearestSpawn);
                };
            }
        }
    }
}
