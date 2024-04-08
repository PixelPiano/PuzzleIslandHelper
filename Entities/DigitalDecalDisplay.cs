using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;

// PuzzleIslandHelper.DigitalDecalDisplay
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/DigitalDecalDisplay")]
    public class DigitalDecalDisplay : Entity
    {

        private string flag;
        private string folder = "objects/PuzzleIslandHelper/speechBubble/";

        private float amount;
        private float timer;
        private float amplitude;
        private float seed;
        private float amplitudeTemp;
        private Vector2 spriteOffset;
        private Vector2 activateDistance;

        private Effect effect = GFX.FxGaussianBlur;//GFX.FxGaussianBlur;
        private Sprite sprite;
        private Sprite bubble;
        private Sprite box;
        private Image fill;
        private Color color;

        private bool inverted;
        private bool activated;

        private bool shouldScale;
        private bool shouldGlitch;
        private bool inRoutine = false;
        private bool blur = false;
        private bool playerNear = false;
        private bool stayIdle = false;
        private bool showSprite = true;
        private bool inSpriteRoutine = false;

        private static VirtualRenderTarget _BubbleMask;
        private static VirtualRenderTarget _BubbleObject;
        private static VirtualRenderTarget _BubbleOutline;
        private static VirtualRenderTarget _BoxTarget;

        private static VirtualRenderTarget _LineRenderTarget;

        public static VirtualRenderTarget LineRenderTarget => _LineRenderTarget ??=
                      VirtualContent.CreateRenderTarget("Lines", 320, 180);
        public static VirtualRenderTarget BubbleMask => _BubbleMask ??=
                      VirtualContent.CreateRenderTarget("BubbleMask", 320, 180);

        public static VirtualRenderTarget BoxTarget => _BoxTarget ??=
              VirtualContent.CreateRenderTarget("BoxTarget", 320, 180);
        public static VirtualRenderTarget BubbleOutline => _BubbleOutline ??=
              VirtualContent.CreateRenderTarget("BubbleOutline", 320, 180);
        public static VirtualRenderTarget BubbleObject => _BubbleObject ??=
                      VirtualContent.CreateRenderTarget("BubbleObject", 320, 180);

        public static readonly BlendState AlphaMaskBlendState = new()
        {
            ColorSourceBlend = Blend.Zero,
            ColorBlendFunction = BlendFunction.Add,
            ColorDestinationBlend = Blend.SourceColor,
            AlphaSourceBlend = Blend.Zero,
            AlphaBlendFunction = BlendFunction.Add,
            AlphaDestinationBlend = Blend.SourceColor
        };
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            _BubbleMask?.Dispose();
            _BubbleObject?.Dispose();
            _BubbleOutline?.Dispose();
            _BoxTarget?.Dispose();
            _LineRenderTarget?.Dispose();
            _BubbleMask = null;
            _BubbleObject = null;
            _BubbleOutline = null;
            _BoxTarget = null;
            _LineRenderTarget = null;
        }
        public DigitalDecalDisplay(EntityData data, Vector2 offset)
        : base(data.Position + offset)
        {
            flag = data.Attr("flag");
            inverted = data.Bool("invertFlag");
            Depth = data.Int("nDepth",1);
            float delay = 1f / data.Float("fps");
            sprite = new Sprite(GFX.Game, "decals/");
            sprite.AddLoop("idle", data.Attr("decalPath"), delay);
            Add(sprite);
            sprite.Visible = false;
            spriteOffset = new Vector2(data.Float("offsetX"), data.Float("offsetY"));
            sprite.Position += spriteOffset;
            shouldScale = data.Bool("shouldScale");
            shouldGlitch = data.Bool("glitch");
            activateDistance = new(data.Float("playerDetectX"), data.Float("playerDetectY"));
            color = data.HexColor("color", Color.SpringGreen);
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Add(bubble = new Sprite(GFX.Game, folder));
            bubble.AddLoop("idle", "sB", 0.08f, 0);
            bubble.Add("open", "sBOpen", 0.08f, "idle");
            bubble.Add("close", "sBClose", 0.08f);

            fill = new Image(GFX.Game[folder + "fill"]);
            Add(box = new Sprite(GFX.Game, folder));
            box.AddLoop("idle", "miniBox", 0.08f);
            box.Add("open", "miniBoxOpen", 0.08f, "idle");
            box.Add("close", "miniBoxClose", 0.08f);

            if (shouldScale)
            {
                float scaleX = bubble.Width / sprite.Width, scaleY = bubble.Height / sprite.Height;
                float scale = Math.Min(scaleX, scaleY);
                sprite.Scale = new Vector2(scale, scale);
            }

            sprite.Justify = new Vector2(0f, 0.1f);
            bubble.Visible = false;
            box.Visible = false;
            box.Position += new Vector2(-5, bubble.Height-8);
            box.Play("idle");
            playerNear = false;
        }
        public override void Render()
        {
            base.Render();
            if (Scene is not Level l)
            {
                return;
            }
            #region MainRender
            Draw.SpriteBatch.End();

            #region BubbleMask
            Engine.Graphics.GraphicsDevice.SetRenderTarget(BubbleMask);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            GameplayRenderer.Begin();
            fill.RenderPosition = bubble.RenderPosition;
            fill.Render();
            Draw.SpriteBatch.End();
            #endregion

            Engine.Graphics.GraphicsDevice.SetRenderTarget(BoxTarget);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, effect, l.Camera.Matrix);
            box.Render();
            Draw.SpriteBatch.End();

            #region Sprite
            Engine.Graphics.GraphicsDevice.SetRenderTarget(BubbleObject);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, effect, l.Camera.Matrix);
            if (showSprite)
            {
                sprite.Render();
            }
            Draw.SpriteBatch.End();
            #endregion

            #region BubbleOutline
            Engine.Graphics.GraphicsDevice.SetRenderTarget(BubbleOutline);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, effect, l.Camera.Matrix);
            bubble.Render();
            Draw.SpriteBatch.End();
            #endregion

            #region BubbleMask => Sprite
            Engine.Graphics.GraphicsDevice.SetRenderTarget(BubbleObject);
            Draw.SpriteBatch.Begin(
                SpriteSortMode.Deferred,
                AlphaMaskBlendState,
                SamplerState.PointClamp,
                DepthStencilState.Default,
                RasterizerState.CullNone,
                effect, Matrix.Identity);

            Draw.SpriteBatch.Draw(BubbleMask, Vector2.Zero, Color.White);
            GameplayRenderer.End();
            #endregion

            #region Glitch Config
            if (shouldGlitch)
            {
                var glitchSave = Glitch.Value;
                Glitch.Value = amount;
                Glitch.Apply(BubbleObject, timer, seed, amplitude);
                Glitch.Apply(BubbleOutline, timer, seed, amplitude);
                Glitch.Value = glitchSave;
            }
            #endregion

            #region Everything => Gameplay Buffer
            Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.Gameplay);
            GameplayRenderer.Begin();

            Draw.SpriteBatch.Draw(BubbleOutline, l.Camera.Position, color);
            Draw.SpriteBatch.Draw(BoxTarget, l.Camera.Position, color);
            Draw.SpriteBatch.Draw(BubbleObject, l.Camera.Position, Color.White);
            #endregion
            #endregion

        }

        public override void Update()
        {
            base.Update();
            if (Scene as Level == null)
            {
                return;
            }
            amplitudeTemp = Calc.Random.Range(15f, 30f);

            Level scene = Scene as Level;
            Player player = Scene.Tracker.GetEntity<Player>();
            if(player is null)
            {
                return;
            }
            playerNear = ((player.Position.X < Position.X + bubble.Width + activateDistance.X
            && player.Position.X > Position.X - activateDistance.X) || activateDistance.X < 0)
            && ((player.Position.Y < Position.Y + bubble.Height + activateDistance.Y
            && player.Position.Y > Position.Y - activateDistance.Y) || activateDistance.Y < 0);

            effect = blur ? GFX.FxGaussianBlur: null;
            timer += Engine.DeltaTime * 2;
            seed = scene.OnInterval(2 / 60f) ? Calc.Random.NextFloat() : seed;
            if (!inSpriteRoutine)
            {
                if (!stayIdle && playerNear)
                {
                    Add(new Coroutine(ChangeState(true), true));
                }
                if (stayIdle && !playerNear)
                {
                    Add(new Coroutine(ChangeState(false), true));
                }
            }

            if (!inRoutine && stayIdle) 
            {
                Add(new Coroutine(WaitInterval(), true));
            }
        }
        private IEnumerator ChangeState(bool state)
        {
            inSpriteRoutine = true;
            float duration = 0.05f;
            int counter = 0;
            if (state)
            {
                
                box.Play("close");
                while (box.CurrentAnimationFrame < 7)
                {
                    counter++;
                    if(counter == 5)
                    {
                        bubble.Play("open");
                    }
                    yield return null;
                }
                stayIdle = true;
                showSprite = false;
                sprite.Play("idle");
                while (bubble.CurrentAnimationID == "open")
                {
                    yield return null;
                }
                showSprite = true;
                yield return duration;
                showSprite = false;
                yield return duration;
                showSprite = true;
                yield return duration;
                showSprite = false;
                yield return duration;
                showSprite = true;
            }
            else
            {
                showSprite = false;
                yield return duration;
                showSprite = true;
                yield return duration;
                showSprite = false;
                yield return duration;
                showSprite = true;
                yield return duration;
                showSprite = false;
                stayIdle = false;
                bubble.Play("close");
                while (bubble.Animating)
                {
                    counter++;
                    if(counter == 5)
                    {
                        box.Play("open");
                    }
                    yield return null;
                }
            }
            inSpriteRoutine = false;
        }
        private IEnumerator WaitInterval()
        {
            inRoutine = true;
            yield return Calc.Random.Range(1f, 6f);
            float amountTemp = Calc.Random.Range(0.06f, 0.1f);
            float amountInit = amount;
            float amplitudeInit = amplitude;

            for (float i = 0; i < 1; i += 0.02f)
            {
                amount = Calc.Approach(amountInit, amountTemp, amountTemp*i);
                amplitude = Calc.Approach(amplitudeInit, amplitudeTemp, amplitudeTemp*i);
                yield return null;
            }
            //blur = true;
            yield return Calc.Random.Range(1f, 3f);
            for(float i = 1; i>0; i -= 0.01f)
            {
                amount = amountTemp * i;
                amplitude = amplitudeTemp * i;
                yield return null;
            }
            amount = 0;
            amplitude = 0;
            inRoutine = false;
        }
    }
}