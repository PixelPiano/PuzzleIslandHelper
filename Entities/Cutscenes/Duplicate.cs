using Celeste.Mod.Helpers;
using Celeste.Mod.Meta;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Cutscenes
{
    [Tracked]
    public class Duplicate : CutsceneEntity
    {
        private static VirtualRenderTarget _Target;
        public static VirtualRenderTarget Target => _Target ??= VirtualContent.CreateRenderTarget("DuplicateTarget", 320, 180);
        public Duplicate() : base()
        {
            Add(new BeforeRenderHook(BeforeRender));
            Depth = int.MinValue;
            Tag |= Tags.Global | Tags.Persistent | Tags.TransitionUpdate;
        }
        private void BeforeRender()
        {
            //if (Scene is not Level level || level.GetPlayer() is not Player Player) return;

        }
        public Scene ExtraScene;
        public void AddNewScene(string room)
        {
            ExtraScene = new Scene();
            Level.Add(ExtraScene);
            LoadLevel(ExtraScene as Level, Level.Session, Player.IntroTypes.None, false);
        }
        public void LoadLevel(Level level, Session session, Player.IntroTypes playerIntro, bool isFromLoader = false)
        {
            if (session.FirstLevel && session.StartedFromBeginning && session.JustStarted)
            {
                LevelLoader levelLoader = Engine.Scene as LevelLoader;
                if ((levelLoader == null || !levelLoader.PlayerIntroTypeOverride.HasValue) && session.Area.Mode == AreaMode.CSide)
                {
                    MapMeta mapMeta = AreaData.GetMode(session.Area)?.GetMapMeta();
                    if (mapMeta != null && mapMeta.OverrideASideMeta.GetValueOrDefault())
                    {
                        Player.IntroTypes? introType = mapMeta.IntroType;
                        if (introType.HasValue)
                        {
                            Player.IntroTypes valueOrDefault = introType.GetValueOrDefault();
                            playerIntro = valueOrDefault;
                        }
                    }
                }
            }

            try
            {
                Logger.Log(LogLevel.Verbose, "LoadLevel", "Loading room " + session.LevelData.Name + " of " + session.Area.GetSID());
                level.orig_LoadLevel(playerIntro, isFromLoader);
                if (Level.ShouldAutoPause)
                {
                    Level.ShouldAutoPause = false;
                    level.Pause();
                }
            }
            catch (Exception ex)
            {
                if (LevelEnter.ErrorMessage == null)
                {
                    if (ex is ArgumentOutOfRangeException && ex.MethodInStacktrace(typeof(Level), "get_DefaultSpawnPoint"))
                    {
                        LevelEnter.ErrorMessage = Dialog.Get("postcard_levelnospawn");
                    }
                    else
                    {
                        LevelEnter.ErrorMessage = Dialog.Get("postcard_levelloadfailed").Replace("((sid))", session.Area.GetSID());
                    }
                }

                Logger.Log(LogLevel.Warn, "LoadLevel", "Failed loading room " + session.Level + " of " + session.Area.GetSID());
                ex.LogDetailed();
                return;
            }

            Everest.Events.Level.LoadLevel(level, playerIntro, isFromLoader);
        }
        public override void Render()
        {
            base.Render();


        }
        public override void OnBegin(Level level)
        {
            Add(new Coroutine(Cutscene()));
        }

        public override void OnEnd(Level level)
        {

        }
        private IEnumerator Cutscene()
        {

            yield return TeleportAndDraw("digiD1", "digiA1");
        }
        private IEnumerator TeleportAndDraw(string roomOne, string roomTwo)
        {
            string thislevel = Level.Session.Level;
            AddNewScene(roomOne);
            yield return null;
            /*            Player.IntroTypes introType = Player.IntroTypes.None;

                        yield return TP(roomOne, introType);
                        yield return 1;
                        yield return TP(thislevel, introType);
                        yield return TP(roomOne, introType, Vector2.One * -16);
                        yield return 1;
                        yield return TP(thislevel, introType);
                        yield return null;*/

        }
        private IEnumerator TP(string room, Player.IntroTypes introType, Vector2? cameraOffset = null)
        {

            Teleport(Level, room, introType, cameraOffset);
            yield return null;
            Level = SceneAs<Level>();

        }

        public static void Teleport(Level level, string room, Player.IntroTypes introType = Player.IntroTypes.None, Vector2? cameraOffset = null)
        {
            Player player = level.GetPlayer();
            if (level == null || player is null || string.IsNullOrEmpty(room))
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
                level.LoadLevel(introType);
                Vector2 val4 = level.DefaultSpawnPoint - level.LevelOffset - val2;
                level.Camera.Position = level.LevelOffset + val3 + val4;
                level.Add(player);
                player.Position = session.RespawnPoint.HasValue ? session.RespawnPoint.Value : level.DefaultSpawnPoint;
                if (cameraOffset.HasValue) level.Camera.Position += cameraOffset.Value;
                player.Facing = facing;
                player.Hair.MoveHairBy(level.LevelOffset - levelOffset + val4);
                if (level.Wipe != null)
                {
                    level.Wipe.Cancel();
                }


            };
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            _Target?.Dispose();
            _Target = null;
        }
    }
}
