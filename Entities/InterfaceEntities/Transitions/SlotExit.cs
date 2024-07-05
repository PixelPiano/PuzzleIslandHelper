using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.Transitions
{
    [Tracked]
    public class SlotExit : Entity
    {
        private Image reflection;
        public class SlotShader : ShaderOverlay
        {
            public float YScale;
            public float XScale;
            public float funEyeTimer;
            public float timer;
            public const float MaxZoomAdd = 0.5f;
            public Vector2[] Positions = { new(1, 1), new(1, -1), new(-1, 1), new(-1, -1) };
            public Vector2 RandomPosition;
            public Vector2 lastPosition;
            public bool startShaking;
            public SlotShader() : base("PuzzleIslandHelper/Shaders/slotExit")
            {
                UseRawDeltaTime = true;

            }
            public override void ApplyParameters()
            {
                base.ApplyParameters();
                Effect?.Parameters["YScale"]?.SetValue(YScale);
                Effect?.Parameters["XScale"]?.SetValue(XScale);
                Effect?.Parameters["EyeTimer"]?.SetValue(funEyeTimer);
                Effect?.Parameters["RandomPosition"]?.SetValue(RandomPosition);
            }
        }
        public SlotShader SShader;
        public bool DrawOnce;
        public bool Drawn;
        private string room;
        private float prevLightAlpha;
        private bool start;

        private static VirtualRenderTarget _Target;
        public static VirtualRenderTarget Target => _Target ??= VirtualContent.CreateRenderTarget("SlotExit", 320, 180);
        private static VirtualRenderTarget _Screenshot;
        public static VirtualRenderTarget Screenshot => _Screenshot ??= VirtualContent.CreateRenderTarget("SlotExitScreenshot", 320, 180);
        public SlotExit(bool start, string room) : base(Vector2.Zero)
        {
            Add(new BeforeRenderHook(BeforeRender));
            reflection = new Image(GFX.Game["objects/PuzzleIslandHelper/access/tvReflection"]);
            Add(reflection);
            reflection.SetColor(Color.White * 0);
            Depth = int.MinValue;
            Tag |= Tags.Global | Tags.Persistent | Tags.TransitionUpdate;
            SShader = new();
            this.start = start;
            this.room = room;
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (start)
            {
                Add(new Coroutine(Cutscene(scene.GetPlayer(), 1, 5, 4))
                {
                    UseRawDeltaTime = true
                });
            }
        }
        public override void Render()
        {
            base.Render();
            if (Drawn)
            {
                Draw.SpriteBatch.Draw(Target, SceneAs<Level>().Camera.Position, Color.White);
            }
        }
        public override void Update()
        {
            base.Update();
            if (SceneAs<Level>().GetPlayer() is not Player player)
            {
                return;
            }
            if (player != null && player.Dead)
            {
                RemoveSelf();
            }
        }
        public void BeforeRender()
        {
            if (DrawOnce && !Drawn)
            {
                Screenshot.DrawToObject(DrawLevel, Matrix.Identity, true);
                DrawOnce = false;
                Drawn = true;
            }
            if (Drawn)
            {
                ShaderOverlay.Apply(Screenshot, Target, SShader, true);
            }
        }
        private IEnumerator LerpAmplitude(float to, float time)
        {
            float prevAmp = SShader.Amplitude;
            for (float i = 0; i < 1; i += Engine.RawDeltaTime / time)
            {
                reflection.SetColor(Color.White * (i / 2));
                SShader.Amplitude = Calc.LerpClamp(prevAmp, to, i);
                yield return null;
            }
        }
        private IEnumerator CRTTurnOff(float time)
        {
            if (Scene is not Level level)
            {
                SShader.YScale = 1;
                SShader.XScale = -1;
                yield break;
            }
            for (float i = 0; i < 1f; i += Engine.RawDeltaTime / time)
            {
                SShader.YScale = i;
                SShader.XScale = i;
                yield return null;
            }
            SShader.YScale = 1;
            SShader.XScale = 1;
            for (float i = 0; i < 1; i += Engine.RawDeltaTime / 0.1f)
            {
                yield return null;
            }
            level.Lighting.Alpha = prevLightAlpha;
            for (float i = 0; i < 1; i += Engine.RawDeltaTime)
            {
                SShader.XScale = Calc.LerpClamp(1, -1, i);
                yield return null;
            }
            SShader.XScale = -1;
            yield return null;
        }
        private IEnumerator EyeThing()
        {
            while (SShader.timer < 10)
            {
                SShader.timer += Engine.RawDeltaTime;
                yield return null;
            }
            SShader.startShaking = true;
            float lerp = 0;

            bool blinking = false;
            float wait = 0.2f;
            float pause = 2;
            float pauseTimer = 0;
            float blinkTimer = 0;
            float moveTimer = 0;
            while (true)
            {
                if (lerp < 1)
                {
                    SShader.funEyeTimer = Calc.LerpClamp(0, 1, Ease.SineIn(lerp));
                    lerp += Engine.RawDeltaTime * 4;
                }
                else if (pauseTimer < pause)
                {
                    SShader.funEyeTimer = 1;
                    pauseTimer += Engine.RawDeltaTime;
                }
                else
                {
                    if (blinkTimer < wait)
                    {
                        blinkTimer += Engine.RawDeltaTime;
                    }
                    else
                    {
                        blinkTimer = 0;
                        SShader.funEyeTimer = blinking ? 1 : 0;
                        wait = blinking ? 0.5f : 0.1f;
                        blinking = !blinking;
                    }
                }
                if (moveTimer < Engine.RawDeltaTime)
                {
                    moveTimer += Engine.RawDeltaTime;
                }
                else
                {
                    while (SShader.lastPosition == SShader.RandomPosition)
                    {
                        SShader.RandomPosition = Calc.Random.Choose(SShader.Positions);
                    }
                    SShader.lastPosition = SShader.RandomPosition;
                    moveTimer = 0;
                }

                yield return Engine.RawDeltaTime;
            }
        }
        private IEnumerator Cutscene(Player player, float amplitudeTo, float amplitudeTime, float waitTime)
        {
            if (player is null || player.Scene is not Level level)
            {
                yield break;
            }
            player.StateMachine.State = Player.StDummy;
            level.ResetZoom();

            prevLightAlpha = level.Lighting.Alpha;
            SShader.ForceLevelRender = true;
            float timerate = Engine.TimeRate;

            for (float i = 0; i < 1; i += Engine.RawDeltaTime / 5)
            {
                Engine.TimeRate = Calc.LerpClamp(timerate, 0, i);
                level.Lighting.Alpha = Calc.LerpClamp(prevLightAlpha, 0, i);
                SetLights(level, 1 - i);
                yield return null;
            }
            if (!Drawn)
            {
                DrawOnce = true;
            }
            SShader.ForceLevelRender = false;
            yield return LerpAmplitude(amplitudeTo, amplitudeTime);

            yield return 0.5f;
            PianoUtils.InstantRelativeTeleport(level, room, true);
            Engine.TimeRate = 0;
            yield return null;
            level = Engine.Scene as Level;
            level.Lighting.Alpha = 0;
            SetLights(level, 0);
            player = level.GetPlayer();
            player.StateMachine.State = Player.StDummy;
            yield return waitTime;
            yield return CRTTurnOff(0.25f);
            Add(new Coroutine(level.ZoomBack(1 / Engine.RawDeltaTime / 3)));
            for (float i = 0; i < 1; i += Engine.RawDeltaTime / 3)
            {
                Engine.TimeRate = Calc.LerpClamp(0, timerate, i);
                yield return null;
            }
            player = level.GetPlayer();
            player.StateMachine.State = Player.StNormal;
            yield return null;
            RemoveSelf();
            yield return null;
        }
        public void SetLights(Level level, float alpha)
        {
            if (level is null)
            {
                return;
            }
            List<Component> lights = level.Tracker.GetComponents<VertexLight>();
            for (int i = 0; i < lights.Count; i++)
            {
                (lights[i] as VertexLight).Alpha = alpha;
            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Target?.Dispose();
            _Target?.Dispose();
            Screenshot?.Dispose();
            _Screenshot?.Dispose();
        }
        private void DrawLevel()
        {
            Draw.SpriteBatch.Draw(GameplayBuffers.Level, Vector2.Zero, Color.White);
            reflection.RenderPosition = Vector2.Zero;
            reflection.Render();
        }
    }
}
