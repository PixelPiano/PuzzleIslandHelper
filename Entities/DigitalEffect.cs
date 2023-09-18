using System;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/DigitalEffect")]
    [Tracked]
    public class DigitalEffect : Entity
    {
        #region Variables
        public static bool RenderCondition;

        private Player player;
        private Color[] HairColor = {Calc.HexToColor("0FD4F1"),
                                     Calc.HexToColor("00a807"),
                                     Calc.HexToColor("99ffd3"),
                                     Calc.HexToColor("7040FF"),
                                     Calc.HexToColor("CF40FF"),
                                     Calc.HexToColor("FF0080")};

        private Color background;

        private Color[] lines = new Color[4];

        private Color[] lineCache = new Color[4];

        private Color tempBack;

        private float[] addToY = new float[4];

        private float maxY; 

        private float rate = 0.2f;

        private int opacityCounter; 

        private readonly float lineOpacity = 0.5f; 

        private readonly float backOpacity = 0.9f; 
        private readonly float lineOpacity2 = 0.2f;

        private readonly float backOpacity2 = 0.8f;


        private bool backFlicker;

        private bool lineFlicker;

        private float currentLineOpacity;

        private float currentBackOpacity;

        private readonly int opacityBuffer = 8; 

        private int opacityBufferCounter;

        private int[] lineFrameOpacity = new int[] { 1, 0, 0, 1, 1, 0, 1, 0, 1, 1,
                                                1, 1, 0, 1, 1, 0, 0, 0, 1, 0,
                                                1, 1, 0, 1, 0, 1, 1, 1, 0, 1, 0, 1 };
  

        private int[] backgroundFrameOpacity = new int[] { 1, 1, 0, 1, 1, 1, 1, 0, 1, 1,
                                                      1, 1, 0, 1, 1, 1, 1, 1, 0, 1,
                                                      1, 1, 1, 1, 1, 1, 1, 0, 0, 1, 0, 1 };
    

        private bool start; 

        private static VirtualRenderTarget _MaskRenderTarget;
        private static VirtualRenderTarget _ObjectRenderTarget;

        public static VirtualRenderTarget MaskRenderTarget => _MaskRenderTarget ??=
                      VirtualContent.CreateRenderTarget("PlayerDigital", 320, 180);

        public static VirtualRenderTarget ObjectRenderTarget => _ObjectRenderTarget ??=
                      VirtualContent.CreateRenderTarget("DigitalObject", 320, 180);
        private bool LeaveOutHair;

        private EntityID id;

        public static readonly BlendState AlphaMaskBlendState = new()
        {
            ColorSourceBlend = Blend.Zero,
            ColorBlendFunction = BlendFunction.Add,
            ColorDestinationBlend = Blend.SourceColor,
            AlphaSourceBlend = Blend.Zero,
            AlphaBlendFunction = BlendFunction.Add,
            AlphaDestinationBlend = Blend.SourceColor
        };
        #endregion
        public static bool ForceStop;
        public DigitalEffect(EntityData data, Vector2 offset, EntityID id)
          : base(data.Position + offset)
        {
            Tag |= Tags.TransitionUpdate;
            this.id = id;
            LeaveOutHair = data.Bool("leaveOutHair", true);
            RenderCondition = true;
            background = Calc.HexToColor("008801");
            lines[0] = Calc.HexToColor("00FF00");
            lines[1] = Calc.HexToColor("00E800");
            lines[2] = Calc.HexToColor("00FF00");
            lines[3] = Calc.HexToColor("07ED07");
            backFlicker = data.Bool("backgroundFlicker", true);
            lineFlicker = data.Bool("lineFlicker", true);
            tempBack = background;
            lines.CopyTo(lineCache, 0);
            Depth = -1;
            start = true;
            maxY = 10;
            opacityCounter = 0;
            currentLineOpacity = 1f;
            currentBackOpacity = 1f;
            opacityBufferCounter = 1;
        }

        public override void Render()
        {
            base.Render();
            if (Scene is not Level l || ForceStop)
            {
                return;
            }

            Player player = Scene.Tracker.GetEntity<Player>();

            if (player == null) { return; }
            if (LeaveOutHair && !l.Transitioning)
            {
                RenderCondition = false;
            }
            float rectX = player.X - (player.Width / 2) - 18;
            float rectY = player.Y - player.Height - 22;
            float rectWidth = player.Width + 40;
            float rectHeight = player.Height + 32;

            if (start)
            {
                addToY[0] = 0;
                addToY[1] = -rectHeight / 2;
                addToY[2] = -rectHeight / 4;
                addToY[3] = -(rectHeight / 4) * 3;
                maxY = rectHeight;
            }
            start = false;
            Draw.SpriteBatch.End(); // stop drawing things like normal

            Engine.Graphics.GraphicsDevice.SetRenderTarget(MaskRenderTarget); // "when you draw, draw to my buffer"
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);// clear the buffer, since that doesn't get done automatically
            GameplayRenderer.Begin(); // setup drawing again with standard properties

            // ...
            // draw your mask (here, the player)
            player.Render();
            Draw.SpriteBatch.End();


            Engine.Graphics.GraphicsDevice.SetRenderTarget(ObjectRenderTarget);
            // note the custom AlphaMaskBlendState to ignore *source* alpha and *destination* colour

            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);

            GameplayRenderer.Begin();

            if (player.Facing == Facings.Right) { rectX -= 1; }

            // draw our overlay
            opacityCounter %= 32;
            opacityBufferCounter %= opacityBuffer;
            if (lineFlicker)
            {
                if (lineFrameOpacity[opacityCounter] == 0)
                {
                    Random ra = new Random();
                    int raInt = ra.Next(0, 100);
                    if (raInt > 50)
                    {
                        currentLineOpacity = lineOpacity;
                    }
                    else
                    {
                        currentLineOpacity = lineOpacity2;
                    }
                }
                else
                {
                    currentLineOpacity = 1f;
                }
            }

            if (backFlicker)
            {
                if (backgroundFrameOpacity[opacityCounter] == 0)
                {
                    Random r = new Random();
                    int rInt = r.Next(0, 100);
                    if (rInt > 50)
                    {
                        currentBackOpacity = backOpacity;
                    }
                    else
                    {
                        currentBackOpacity = backOpacity2;
                    }
                }
                else
                {
                    currentBackOpacity = 1f;
                }
            }
            Draw.Rect(rectX, rectY, rectWidth, rectHeight, background);

            float[] lineY = new float[4];

            for (int i = 0; i < 4; i++)
            {
                addToY[i] %= maxY;
                lineY[i] = rectY + rectHeight + addToY[i];
            }
            float endX = rectX + rectWidth;

            Draw.Line(new Vector2(rectX, lineY[0]), new Vector2(endX, lineY[0]), lines[0]);

            Draw.Line(new Vector2(rectX, lineY[1]), new Vector2(endX, lineY[1]), lines[1]);

            Draw.Line(new Vector2(rectX, lineY[2]), new Vector2(endX, lineY[2]), lines[2]);

            Draw.Line(new Vector2(rectX, lineY[3]), new Vector2(endX, lineY[3]), lines[3]);

            // back to normal

            Draw.SpriteBatch.End();

            Engine.Graphics.GraphicsDevice.SetRenderTarget(ObjectRenderTarget);

            Draw.SpriteBatch.Begin(
                SpriteSortMode.Deferred,
                AlphaMaskBlendState,
                SamplerState.PointClamp,
                DepthStencilState.Default,
                RasterizerState.CullNone,
                null, Matrix.Identity);

            //draw mask to object target
            Draw.SpriteBatch.Draw(MaskRenderTarget, Vector2.Zero, Color.White);

            GameplayRenderer.End();
            //switch to render to Gameplay
            Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.Gameplay);

            GameplayRenderer.Begin();
            // draw our masked overlay
            Draw.SpriteBatch.Draw(ObjectRenderTarget, l.Camera.Position, Color.White);
            RenderCondition = true;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            ForceStop = false;
        }
        public override void Update()
        {
            base.Update();
            if (Scene as Level is null)
            {
                return;
            }

            Visible = SceneAs<Level>().Session.Level.Contains("digi");
            background = Color.Lerp(Color.LightGreen, tempBack,currentBackOpacity);
            for (int i = 0; i < 4; i++)
            {
                lines[i] = lineCache[i] * currentLineOpacity;
                addToY[i] -= rate;
            }

            if (opacityBufferCounter == 1)
            {
                opacityCounter++;
            }
            opacityBufferCounter++;
        }
        public static void Unload()
        {
            _MaskRenderTarget?.Dispose();
            _ObjectRenderTarget?.Dispose();
        }
        public static void Load()
        {
            RenderCondition = true;
        }
    }
}
