using Celeste.Mod.Entities;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System.Collections;

// PuzzleIslandHelper.DecalEffects
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/DecalEffects")]
    public class DecalEffects : Entity
    {

        private float amount;
        private float timer;
        private float amplitude;
        private float seed;
        private float amplitudeTemp;
        private float glitchSave;
        private float flashMax;
        private float opacity = 0f;
        private float fadeOpacity = 1f;
        private int fadeDistance = 45;
        private bool fading = false;
        private bool addLight = true;

        private string effectString = "";
        private string audio = "";

        private Rectangle bounds;
        Rectangle leftSide;
        Rectangle rightSide;
        private Color color;
        private Color flashColor;

        private bool inFlashRoutine = false;
        private bool shouldGlitch = false;
        private bool inRoutine = false;
        private bool playerTouching = false;
        private bool flashedOnce = false;
        private bool shouldFlash = false;
        private bool atEdge = false;
        private bool shouldFade = false;
        private bool speaking = false;
        private bool inSpeakingRoutine = false;
        private bool usesAudio = true;

        private int side = 0;
        private Level l;
        private Player player;
        private Effect effect;
        private Sprite sprite;
        private EventInstance sfx;
        private bool doesBurst = true;
        private static VirtualRenderTarget _SpriteTarget;
        private static VirtualRenderTarget _FlashMaskTarget;
        private static VirtualRenderTarget _FlashObject;
        private static VirtualRenderTarget _ColorFix;
        public static VirtualRenderTarget FlashMaskTarget => _FlashMaskTarget ??= VirtualContent.CreateRenderTarget("FlashMaskTarget", 320, 180);
        public static VirtualRenderTarget FlashObject => _FlashObject ??= VirtualContent.CreateRenderTarget("FlashObject", 320, 180);
        public static VirtualRenderTarget SpriteTarget => _SpriteTarget ??= VirtualContent.CreateRenderTarget("SpriteTarget", 320, 180);

        public static VirtualRenderTarget ColorFix => _ColorFix ??= VirtualContent.CreateRenderTarget("ColorFix", 320, 180);

        public static readonly BlendState AlphaMaskBlendState = new()
        {
            ColorSourceBlend = Blend.Zero,
            ColorBlendFunction = BlendFunction.Add,
            ColorDestinationBlend = Blend.SourceAlpha,
            AlphaSourceBlend = Blend.Zero,
            AlphaBlendFunction = BlendFunction.Add,
            AlphaDestinationBlend = Blend.SourceAlpha
        };
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
             _SpriteTarget?.Dispose();
             _FlashMaskTarget?.Dispose();
             _FlashObject?.Dispose();
             _ColorFix?.Dispose();
            _SpriteTarget = null;
            _FlashMaskTarget = null;
            _FlashObject = null;
            _ColorFix = null;
        }
        public DecalEffects(EntityData data, Vector2 offset)
        : base(data.Position + offset)
        {
            //Flag = data.Attr("Flag");
            Tag |= Tags.TransitionUpdate;
            shouldFade = data.Bool("cameraFade");
            Depth = data.Int("depth", 1);
            float delay = 1f / data.Float("fps");
            sprite = new Sprite(GFX.Game, "decals/");
            effectString = data.Attr("gfxEffect");
            sprite.AddLoop("idle", data.Attr("decalPath"), delay);
            Add(sprite);
            sprite.Visible = false;
            shouldGlitch = data.Bool("glitch");
            audio = data.Attr("event", "event:/new_content/game/10_farewell/glitch_short");
            Collider = new Hitbox(sprite.Width, sprite.Height);
            bounds = new Rectangle((int)Position.X, (int)Position.Y, (int)sprite.Width, (int)sprite.Height);
            shouldFlash = data.Bool("flashOnCollide");
            color = data.HexColor("color", Color.SpringGreen);
            flashColor = data.HexColor("flashColor", Color.White);
            flashMax = data.Float("flashLimit", 0.5f);
            if (data.Bool("addLight", true))
            {
                Add(new VertexLight(Center, color, 1, (int)Width, (int)Width + 4));
            }
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            sprite.Play("idle");
            effect = EffectFromString(effectString);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            player = (scene as Level).Tracker.GetEntity<Player>();
        }
        private Effect EffectFromString(string str)
        {
            return str == "None" ? null :
                   str == "Blur" ? GFX.FxGaussianBlur :
                   str == "Distortion" ? GFX.FxDistort :
                   str == "Glitch" ? GFX.FxGlitch : null; ;
        }
        public override void Render()
        {
            base.Render();
            if (Scene is not Level)
            {
                return;
            }
            l = Scene as Level;
            Draw.SpriteBatch.End();
            #region Flash Mask
            Engine.Graphics.GraphicsDevice.SetRenderTarget(FlashMaskTarget);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, effect, l.Camera.Matrix);
            sprite.Render();
            Draw.SpriteBatch.End();
            #endregion

            #region Sprite
            Engine.Graphics.GraphicsDevice.SetRenderTarget(SpriteTarget);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, effect, l.Camera.Matrix);
            sprite.Render();
            Draw.SpriteBatch.End();
            #endregion

            #region FlashMask => FlashObject
            Engine.Graphics.GraphicsDevice.SetRenderTarget(FlashObject);
            Engine.Graphics.GraphicsDevice.Clear(flashColor);
            Draw.SpriteBatch.Begin(
                SpriteSortMode.Deferred,
                AlphaMaskBlendState,
                SamplerState.PointClamp,
                DepthStencilState.Default,
                RasterizerState.CullNone,
                effect, Matrix.Identity);
            Draw.SpriteBatch.Draw(FlashMaskTarget, Vector2.Zero, Color.White);
            GameplayRenderer.End();
            #endregion

            Engine.Graphics.GraphicsDevice.SetRenderTarget(ColorFix);
            Engine.Graphics.GraphicsDevice.Clear(color);
            Draw.SpriteBatch.Begin(
                SpriteSortMode.Deferred,
                AlphaMaskBlendState,
                SamplerState.PointClamp,
                DepthStencilState.Default,
                RasterizerState.CullNone,
                effect, Matrix.Identity);
            Draw.SpriteBatch.Draw(FlashMaskTarget, Vector2.Zero, Color.White);
            GameplayRenderer.End();

            #region Glitch Config
            if (shouldGlitch)
            {
                glitchSave = Glitch.Value;
                Glitch.Value = amount;
                Glitch.Apply(SpriteTarget, timer, seed, amplitude);
                Glitch.Apply(FlashObject, timer, seed, amplitude);
                Glitch.Apply(ColorFix, timer, seed, amplitude);
                Glitch.Value = glitchSave;
            }
            #endregion

            Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.Gameplay);
            GameplayRenderer.Begin();
            Draw.SpriteBatch.Draw(SpriteTarget, l.Camera.Position, color * fadeOpacity);
            if (fading || !inFlashRoutine)
            {
                Draw.SpriteBatch.Draw(ColorFix, l.Camera.Position, color * 0.25f);
            }
            if (inFlashRoutine)
            {
                Draw.SpriteBatch.Draw(FlashObject, l.Camera.Position, Color.White * opacity);
            }
        }

        public override void Update()
        {
            base.Update();
            if (Scene as Level == null)
            {
                return;
            }
            l = Scene as Level;
            if (shouldFade)
            {
                leftSide = new Rectangle((int)l.Camera.Left, (int)l.Camera.Top, fadeDistance, l.Bounds.Height);
                rightSide = new Rectangle((int)l.Camera.Right - fadeDistance, (int)l.Camera.Top, fadeDistance, l.Bounds.Height);
                side = CollideRect(leftSide) ? 1 : CollideRect(rightSide) ? 2 : 0;
                atEdge = side == 0 ? false : true;
                if (atEdge)
                {
                    FadeOpacity(l, side);
                }
                else
                {
                    fadeOpacity = 1;
                }
            }
            if (!string.IsNullOrEmpty(audio) && speaking && !inSpeakingRoutine && doesBurst)
            {
                Add(new Coroutine(SoundWaves(), true));
            }
            amplitudeTemp = Calc.Random.Range(15f, 30f);
            player = Scene.Tracker.GetEntity<Player>();
            if (player != null)
            {
                playerTouching = player.CollideRect(bounds);
            }
            timer += shouldGlitch ? Engine.DeltaTime * 2 : 0;
            seed = Scene.OnInterval(2 / 60f) ? Calc.Random.NextFloat() : seed;
            if (!inRoutine && shouldGlitch)
            {
                Add(new Coroutine(WaitInterval(), true));
            }
            if (playerTouching && shouldFlash)
            {
                Coroutine coroutine = usesAudio ? new Coroutine(FlashDecal(), true) : new Coroutine(FlashDecalNoAudio(), true);
                Add(coroutine);
            }
            else
            {
                flashedOnce = false;
            }
        }
        private void FadeOpacity(Level l, int side)
        {
            switch (side)
            {
                case 0: fadeOpacity = 1; break;
                case 1: fadeOpacity = Calc.Clamp((Center.X - l.Camera.Left) / fadeDistance, 0.2f, 1); break;
                case 2: fadeOpacity = Calc.Clamp((l.Camera.Right - Center.X) / fadeDistance, 0.2f, 1); break;
            }
        }

        private IEnumerator SoundWaves()
        {
            inSpeakingRoutine = true;

            while (speaking)
            {
                SceneAs<Level>().Displacement.AddBurst(Center, 0.8f, Width/2, Width, 0.2f);
                yield return 0.5f;
            }
            inSpeakingRoutine = false;
        }
        private IEnumerator WaitInterval()
        {
            inRoutine = true;
            float amountTemp = Calc.Random.Range(0.06f, 0.1f);
            float amountInit = amount;
            float amplitudeInit = amplitude;
            yield return Calc.Random.Range(0.2f, 6f);
            for (float i = 0; i < 1; i += 0.02f)
            {
                amount = Calc.Approach(amountInit, amountTemp, amountTemp * i);
                amplitude = Calc.Approach(amplitudeInit, amplitudeTemp, amplitudeTemp * i);
                yield return null;
            }
            yield return Calc.Random.Range(0.2f, 6f);
            for (float i = 1; i > 0; i -= 0.01f)
            {
                amount = amountTemp * i;
                amplitude = amplitudeTemp * i;
                yield return null;
            }
            amount = 0;
            amplitude = 0;

            inRoutine = false;
        }
        private IEnumerator FlashDecal()
        {
            float rate = 0.12f;
            if (!inFlashRoutine && !flashedOnce)
            {
                inFlashRoutine = true;
                sfx = Audio.Play(audio, Center);

                for (float i = 0; i < 1; i += rate * 2)
                {
                    opacity = Calc.Approach(0f, flashMax, i);
                    yield return Engine.DeltaTime;
                }
                sfx.getPlaybackState(out PLAYBACK_STATE state);

                bool opacityState = true;
                float _opacity = opacity;
                speaking = true;
                while (true)
                {
                    if (state == PLAYBACK_STATE.STOPPING) { break; }
                    else { sfx.getPlaybackState(out state); }

                    opacity = opacityState ? 0.1f : _opacity;
                    opacityState = !opacityState;
                    yield return null;
                }
                speaking = false;
                opacity = _opacity;
                yield return 0.1f;
                fading = true;
                for (float i = 0; i < flashMax; i += rate)
                {
                    opacity = Calc.Approach(flashMax, 0f, i);
                    yield return Engine.DeltaTime;
                }
                fading = false;
                opacity = 0;
                flashedOnce = true;
                inFlashRoutine = false;
                yield return null;
            }
        }
        private IEnumerator FlashDecalNoAudio()
        {
            float rate = 0.12f;
            if (!inFlashRoutine && !flashedOnce)
            {
                inFlashRoutine = true;

                for (float i = 0; i < 1; i += rate * 2)
                {
                    opacity = Calc.Approach(0f, flashMax, i);
                    yield return Engine.DeltaTime;
                }
                yield return 0.1f;
                fading = true;
                for (float i = 0; i < flashMax; i += rate)
                {
                    opacity = Calc.Approach(flashMax, 0f, i);
                    yield return Engine.DeltaTime;
                }
                fading = false;
                opacity = 0;
                flashedOnce = true;
                inFlashRoutine = false;
                yield return null;
            }
        }
    }
}
