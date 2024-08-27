using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Cutscenes
{
    [Tracked]
    public class WarpToCalidus : CutsceneEntity
    {
        public WarpToCalidus() : base()
        {
            Tag |= Tags.TransitionUpdate;
        }

        public override void OnBegin(Level level)
        {
            Add(new Coroutine(GlitchIn()));
        }
        private IEnumerator StutterGlitch(int frames)
        {
            for (int i = 0; i < frames; i++)
            {
                if (Calc.Random.Chance(0.1f))
                {
                    int addframes = Calc.Random.Range(1, 4);
                    Glitch.Value = Calc.Random.Range(0.08f, 0.4f);
                    yield return Engine.DeltaTime * addframes;
                    i += addframes;
                }
                Glitch.Value = 0;
            }
        }
        private IEnumerator GlitchIn()
        {
            yield return StutterGlitch(60);
            for (float i = 0; i < 1; i += Engine.DeltaTime / 2)
            {
                Glitch.Value = i;
                yield return null;
            }
            Glitch.Value = 1;
            yield return 0.5f;
            Audio.SetMusicParam("fade", 1);
            int suffix = CalidusCutscene.GetCutsceneFlag(Scene, CalidusCutscene.Cutscenes.First) ? 1 :
                         CalidusCutscene.GetCutsceneFlag(Scene, CalidusCutscene.Cutscenes.Second) ? 2 :
                         3;
            string room = "digiCalidus" + suffix;
            InstantTeleport(Scene, Scene.GetPlayer(), room);
        }
        public bool DirectlyIntoDigiMeet;


        private bool panningOut;
        private IEnumerator WaitForPanOut()
        {
            while (panningOut)
            {
                yield return null;
            }
        }
        private IEnumerator PanOut()
        {
            Add(new Coroutine(ActuallyPanOut()));
            yield return null;
        }
        private IEnumerator ActuallyPanOut()
        {
            panningOut = true;
            yield return Level.ZoomBack(4.3f);
            panningOut = false;
            Level.ResetZoom();
        }
        public override void OnEnd(Level level)
        {
            /*            if (part == 2)
                        {
                            if (!DirectlyIntoDigiMeet)
                            {
                                if (WasSkipped)
                                {
                                    Level.ResetZoom();
                                }
                                if (level.GetPlayer() is Player player)
                                {
                                    player.StateMachine.State = Player.StNormal;
                                }
                            }
                            Glitch.Value = 0;
                        }*/
        }
        public static void InstantTeleport(Scene scene, Player player, string room)
        {
            Level level = scene as Level;
            if (level == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(room))
            {
                return;
            }
            level.OnEndOfFrame += delegate
            {
                Vector2 levelOffset = level.LevelOffset;
                Vector2 val2 = player.Position - level.LevelOffset;
                Vector2 val3 = level.Camera.Position - level.LevelOffset;
                Facings facing = player.Facing;
                level.Remove(player);
                level.UnloadLevel();
                level.Session.Level = room;
                Session session = level.Session;
                Level level2 = level;
                Rectangle bounds = level.Bounds;
                float num = bounds.Left;
                bounds = level.Bounds;
                session.RespawnPoint = level2.GetSpawnPoint(new Vector2(num, bounds.Top));
                level.Session.FirstLevel = false;
                level.LoadLevel(Player.IntroTypes.Transition);

                level.Camera.Position = level.LevelOffset + val3;
                level.Add(player);

                if (session.RespawnPoint.HasValue)
                {
                    player.Position = session.RespawnPoint.Value;
                }
                player.Facing = facing;
                player.Hair.MoveHairBy(level.LevelOffset - levelOffset);

                Vector2 newPos = level.LevelOffset;

                if (level.Wipe != null)
                {
                    level.Wipe.Cancel();
                }
            };
        }
        public static void InstantRelativeTeleport(Scene scene, string room, bool snapToSpawnPoint, int positionX = 0, int positionY = 0)
        {
            Level level = scene as Level;
            Player player = level.GetPlayer();
            if (level == null || player == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(room))
            {
                return;
            }
            level.OnEndOfFrame += delegate
            {
                Vector2 levelOffset = level.LevelOffset;
                Vector2 val2 = player.Position - levelOffset;
                Vector2 val3 = level.Camera.Position - levelOffset;
                Vector2 offset = new Vector2(positionY, positionX);
                Facings facing = player.Facing;
                level.Remove(player);
                level.UnloadLevel();
                level.Session.Level = room;
                Session session = level.Session;
                Level level2 = level;
                Rectangle bounds = level.Bounds;
                float num = bounds.Left;
                bounds = level.Bounds;
                session.RespawnPoint = level2.GetSpawnPoint(new Vector2(num, bounds.Top));
                level.Session.FirstLevel = false;
                level.LoadLevel(Player.IntroTypes.None);

                level.Camera.Position = level.LevelOffset + val3 + offset.Floor();
                if (snapToSpawnPoint && session.RespawnPoint.HasValue)
                {
                    player.Position = session.RespawnPoint.Value + offset.Floor();
                }
                else
                {
                    player.Position = level.LevelOffset + val2 + offset.Floor();
                }

                player.Facing = facing;
                player.Hair.MoveHairBy(level.LevelOffset - levelOffset + offset.Floor());
                if (level.Wipe != null)
                {
                    level.Wipe.Cancel();
                }
            };
        }
    }
}
