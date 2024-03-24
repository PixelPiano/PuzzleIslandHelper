using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.CommunalHelper;
using Celeste.Mod.PuzzleIslandHelper.Effects;
using Celeste.Mod.PuzzleIslandHelper.Entities.GameplayEntities;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
namespace Celeste.Mod.PuzzleIslandHelper.Cutscenes.Prologue
{
    public class PIPrologueEnding : CutsceneEntity
    {
        public class PrologueGlitchBlock : CustomFlagExitBlock
        {
            public PrologueGlitchBlock(Vector2 position, float width, float height, char tileType, string flag) : base(position, width, height, tileType, flag, false, true, true, glitchEvent, false)
            {
            }
            public PrologueGlitchBlock(PrologueBird bird) : this(bird.StartPosition - Vector2.One * 16, 32, 40, 'Q', "birdBlock")
            {

            }
            public PrologueGlitchBlock(Player player) : this(player.Position - Vector2.One * 22, 48, 56, 'Q', "playerBlock")
            {
            }
            public IEnumerator PrologueGlitchIncrement()
            {
                inRoutine = true;
                glitchLimit = 0;
                while (glitchLimit < max)
                {
                    glitchLimit += 1;
                    yield return null;
                }
                inRoutine = false;
                newTiles.Visible = true;
                Collidable = true;
            }
            public override void Update()
            {
                base_Update();
                timer += Engine.RawDeltaTime;
                seed = Calc.Random.NextFloat();
            }
            public override void Awake(Scene scene)
            {
                base.Awake(scene);
                Add(new Coroutine(PrologueGlitchIncrement(), true));
                timer += Engine.RawDeltaTime;
                seed = Calc.Random.NextFloat();
                Audio.Play(audio, Center);
                newCutout.Alpha = newTiles.Alpha = 1;
            }
        }
        public PrologueGlitchBlock[] Blocks = new PrologueGlitchBlock[2];
        private class EndingCutsceneDelay : Entity
        {
            public EndingCutsceneDelay()
            {
                Add(new Coroutine(Routine()));
            }

            private IEnumerator Routine()
            {
                yield return 3f;
                (Scene as Level).CompleteArea(spotlightWipe: false, false, false);
                InvertOverlay.playerTimeRate = 1;
            }
        }
        private Player player;

        private PrologueBird bird;

        private PrologueBridge bridge;

        private bool keyOffed;

        public const string glitchEvent = "event:/PianoBoy/invertGlitch2";
        public PIPrologueEnding(Player player, PrologueBird bird, PrologueBridge bridge)
            : base(fadeInOnSkip: false, endingChapterAfter: true)
        {
            Engine.TimeRate = 1;
            InvertOverlay.playerTimeRate = 1;
            this.player = player;
            this.bird = bird;
            this.bridge = bridge;
            if (bird is null || player is null || bird.nodes.Length == 0)
            {
                RemoveSelf();
            }

        }
        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            InvertOverlay.playerTimeRate = 1;
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            InvertOverlay.playerTimeRate = 1;
        }
        public override void OnBegin(Level level)
        {
            Add(new Coroutine(Cutscene(level)));
        }
        private IEnumerator Cutscene(Level level)
        {
            while (Engine.TimeRate > 0f)
            {
                yield return null;
                if (Engine.TimeRate < 0.5f && bridge != null)
                {
                    bridge.StopCollapseLoop();
                }
                level.StopShake();
                MInput.GamePads[Input.Gamepad].StopRumble();
                Engine.TimeRate -= Engine.RawDeltaTime * 2f;
                InvertOverlay.playerTimeRate = Engine.TimeRate;
            }
            Engine.TimeRate = 0f;
            InvertOverlay.playerTimeRate = 0;
            player.StateMachine.State = Player.StDummy;
            player.DummyAutoAnimate = false;

            player.Facing = Facings.Left;
            Vector2 blockPos = player.Position;
            yield return WaitFor(1f);
            EventInstance instance = Audio.Play("event:/game/general/bird_in", bird.Position);
            bird.Facing = Facings.Right;
            bird.Sprite.Play("fall");
            float percent = 0f;
            Vector2 from = bird.Position;
            Vector2 to = bird.StartPosition;
            while (percent < 1f)
            {
                bird.Position = from + (to - from) * Ease.QuadOut(percent);
                Audio.Position(instance, bird.Position);
                if (percent > 0.5f)
                {
                    bird.Sprite.Play("fly");
                }
                percent += Engine.RawDeltaTime * 0.5f;
                yield return null;
            }
            bird.Position = to;
            Audio.Play("event:/game/general/bird_land_dirt", bird.Position);
            Dust.Burst(bird.Position, -(float)Math.PI / 2f, 12, null);
            bird.Sprite.Play("idle");
            yield return WaitFor(0.5f);
            bird.Sprite.Play("peck");
            yield return WaitFor(1.1f);
            yield return bird.ShowTutorial(new BirdTutorialGui(bird, new Vector2(0f, -16f), Dialog.Clean("tutorial_dash"), new Vector2(-1f, -1f), "+", BirdTutorialGui.ButtonPrompt.Dash), caw: true);
            while (true)
            {
                Vector2 aimVector = Input.GetAimVector();
                if (aimVector.X < 0f && aimVector.Y < 0f && Input.Dash.Pressed)
                {
                    break;
                }
                yield return null;
            }
            Blocks[0] = new PrologueGlitchBlock(bird);
            Blocks[1] = new PrologueGlitchBlock(player);
            level.Add(Blocks[0]);
            bird.Add(new Coroutine(bird.HideTutorial()));
            yield return WaitFor(3);
            level.Add(Blocks[1]);
            while (Engine.TimeRate < 1f)
            {
                Engine.TimeRate += Engine.RawDeltaTime * 2f;
                yield return WaitFor(0.1f);
            }
            Engine.TimeRate = 1f;
            keyOffed = true;
            yield return 4f;
            EndCutscene(level);
        }

        private IEnumerator WaitFor(float time)
        {
            for (float t = 0f; t < time; t += Engine.RawDeltaTime)
            {
                yield return null;
            }
        }

        public override void OnEnd(Level level)
        {
            if (WasSkipped)
            {
                if (bird != null)
                {
                    bird.Visible = false;
                }

                if (!keyOffed)
                {
                    Audio.CurrentMusicEventInstance.triggerCue();
                }

            }
            //Engine.TimeRate = 1f;
            level.PauseLock = true;
            level.Entities.FindFirst<SpeedrunTimerDisplay>().CompleteTimer = 10f;
            level.Add(new EndingCutsceneDelay());
        }
    }
}
