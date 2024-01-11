using Celeste.Mod.Entities;
using FMOD;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod.Utils;
using System.Collections;
using System.Collections.Generic;

// PuzzleIslandHelper.DecalEffects
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/DecalEffectController")]
    public class DecalEffectController : Entity
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
        private float blendAmount;
        private float fadeDuration;
        private bool fading = false;
        private bool outlineSprites;

        private string effectString = "";
        private string audio = "";
        private string id = "";

        private Color color;
        private Color color2;
        private Color customColor;
        private Color flashColor;

        private bool inFlashRoutine = false;
        private bool shouldGlitch = false;
        private bool inRoutine = false;
        private bool playerTouching = false;
        private bool flashedOnce = false;
        private bool shouldFlash = false;
        private bool speaking = false;
        private bool inSpeakingRoutine = false;
        private bool usesAudio = true;
        private bool fadeInOut;

        private Level l;
        private Player player;
        private Effect effect;
        private EventInstance sfx;
        private static VirtualRenderTarget _SpriteTarget;
        private static VirtualRenderTarget _FlashMaskTarget;
        private static VirtualRenderTarget _FlashObject;
        private static VirtualRenderTarget _ColorFix;

        private List<Entity> entityList = new();
        public static VirtualRenderTarget FlashMaskTarget => _FlashMaskTarget ??= VirtualContent.CreateRenderTarget("FlashMaskTarget", 320, 180);
        public static VirtualRenderTarget FlashObject => _FlashObject ??= VirtualContent.CreateRenderTarget("FlashObject", 320, 180);
        public static VirtualRenderTarget SpriteTarget => _SpriteTarget ??= VirtualContent.CreateRenderTarget("SpriteTarget", 320, 180);
        public static VirtualRenderTarget ColorFix => _ColorFix ??= VirtualContent.CreateRenderTarget("SpriteTarget", 320, 180);

        public static readonly BlendState AlphaMaskBlendState = new()
        {
            ColorSourceBlend = Blend.Zero,
            ColorBlendFunction = BlendFunction.Add,
            ColorDestinationBlend = Blend.SourceAlpha,
            AlphaSourceBlend = Blend.Zero,
            AlphaBlendFunction = BlendFunction.Add,
            AlphaDestinationBlend = Blend.SourceAlpha
        };

        public DecalEffectController(EntityData data, Vector2 offset)
        : base(data.Position + offset)
        {
            outlineSprites = data.Bool("outlineSprites");
            fadeDuration = data.Float("colorFadeDuration", 1);
            fadeInOut = data.Bool("colorFadeInOut");
            blendAmount = data.Float("colorBlendAmount", 1);
            id = data.Attr("targetGroupID");
            effectString = data.Attr("gfxEffect");
            shouldFlash = data.Bool("flashOnCollide");
            color = data.HexColor("color", Color.White);
            color2 = data.HexColor("color2", Color.Green);
            customColor = color;
            flashColor = data.HexColor("flashColor", Color.White);
            flashMax = data.Float("flashLimit", 0.5f);
            shouldGlitch = data.Bool("glitch");
            audio = data.Attr("event", "event:/new_content/game/10_farewell/glitch_short");
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            entityList = scene.Tracker.GetEntities<DecalEffectTarget>();

            foreach (DecalEffectTarget entity in entityList)
            {
                if (entity.id == id)
                {
                    //Add(entity.Texture);
                    entity.sprite.Visible = false;
                    entity.sprite.Play("idle");
                }
            }
            if (fadeInOut)
            {
                Tween colorTween = Tween.Create(Tween.TweenMode.YoyoLooping, Ease.SineInOut, fadeDuration);
                colorTween.OnUpdate = (Tween t) =>
                {
                    customColor = Color.Lerp(color, color2, blendAmount * t.Eased);
                };
                Add(colorTween);
                colorTween.Start();
            }
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            effect = EffectFromString(effectString);
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
            foreach (DecalEffectTarget entity in entityList)
            {
                if (entity.id == id)
                {
                    Depth = entity.Depth;
                    entity.sprite.RenderPosition = entity.Position;
                    if (outlineSprites)
                    {
                        entity.sprite.DrawSimpleOutline();
                    }
                    entity.sprite.Render();
                }
            }
            Draw.SpriteBatch.End();
            #endregion

            #region Sprite
            Engine.Graphics.GraphicsDevice.SetRenderTarget(SpriteTarget);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, effect, l.Camera.Matrix);
            foreach (DecalEffectTarget entity in entityList)
            {
                if (entity.id == id)
                {
                    Depth = entity.Depth;
                    entity.sprite.RenderPosition = entity.Position;
                    if (outlineSprites)
                    {
                        entity.sprite.DrawSimpleOutline();
                    }
                    entity.sprite.Render();
                }
            }
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
            Draw.SpriteBatch.Draw(SpriteTarget, l.Camera.Position, customColor * fadeOpacity);
            if (fading || !inFlashRoutine)
            {
                Draw.SpriteBatch.Draw(ColorFix, l.Camera.Position, customColor * 0.25f);
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

            if (!string.IsNullOrEmpty(audio) && speaking && !inSpeakingRoutine)
            {
                Add(new Coroutine(SoundWaves(), true));
            }
            amplitudeTemp = Calc.Random.Range(15f, 30f);
            player = Scene.Tracker.GetEntity<Player>();

            //check if the player is touching any of the target decals' bounds
            if (player is not null)
            {
                foreach (DecalEffectTarget entity in entityList)
                {
                    if (entity.id == id)
                    {
                        if (player.CollideRect(entity.bounds))
                        {
                            playerTouching = true;
                            break;
                        }
                        else
                        {
                            playerTouching = false;
                        }
                    }
                }
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

        private IEnumerator SoundWaves()
        {
            inSpeakingRoutine = true;

            while (speaking)
            {
                foreach (DecalEffectTarget entity in entityList)
                {
                    if (entity.id == id)
                    {
                        SceneAs<Level>().Displacement.AddBurst(entity.Center, 0.8f, Width / 4, Width / 4 + 32f, 0.2f);
                    }
                }
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

                sfx = Audio.Play(audio, player.Center);
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
