using Celeste.Mod.CommunalHelper.Entities.StrawberryJam;
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Cutscenes.Prologue;
using Celeste.Mod.PuzzleIslandHelper.Entities.Cutscenes.Prologue;
using Celeste.Mod.PuzzleIslandHelper.Triggers;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Cutscenes
{
    [Tracked]
    public class WarpToCalidus : CutsceneEntity
    {
        private int part;
        public WarpToCalidus(int part) : base()
        {
            this.part = part;
            Tag |= Tags.TransitionUpdate;
        }

        public override void OnBegin(Level level)
        {
            if (part == 1)
            {
                Add(new Coroutine(GlitchIn()));
            }
            else if (!PianoModule.Session.MetWithCalidusFirstTime)
            {
                if (level.GetPlayer() is Player player)
                {
                    player.StateMachine.State = Player.StDummy;
                    level.ZoomSnap(player.Center - Level.Camera.Position - Vector2.UnitY * 24, 1.7f);
                }
                Add(new Coroutine(GlitchOut()));
            }
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
            InstantRelativeTeleport(Scene, "digiCalidus", true);
        }
        private IEnumerator GlitchOut()
        {
            PianoModule.Session.MetWithCalidusFirstTime = true;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 2)
            {
                Glitch.Value = 1 - i;
                yield return null;
            }
            Glitch.Value = 0;
            yield return 0.5f;
            Add(new Coroutine(StutterGlitch(20)));
            Player player = Level.GetPlayer();
            Vector2 pos = player.Center - Level.Camera.Position - Vector2.UnitY * 24;
            yield return Textbox.Say("wtc1", PanOut, WaitForPanOut);
            yield return Level.ZoomBack(0.8f);
            EndCutscene(Level);
        }
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
            Level.ResetZoom();
            if (part == 2)
            {
                if (level.GetPlayer() is Player player)
                {
                    player.StateMachine.State = Player.StNormal;
                }
                Glitch.Value = 0;
            }
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
                //level.Add(player);
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
