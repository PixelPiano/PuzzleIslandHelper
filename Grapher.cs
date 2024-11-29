using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;

namespace Celeste.Mod.PuzzleIslandHelper
{
    public class Grapher
    {
        public static float timeMod = 0.2f;
        public static float piMod = 32;
        public static float step = (float)Math.PI / piMod; //Change this value
        public static bool Visible = false; // toggles effect visibility
        public static string ColorGradeName = "PianoBoy/invertFlag"; // colorgrade to use for the effect
        public static float k = 0;
        public static float lineWidth = 1f;
        public static float size = 10f;
        public static bool sizeMult = true;
        public static string luaColor = "ffffff";
        public static float colorAlpha = 1.0f;

        private static VirtualRenderTarget stencil;
        private static AlphaTestEffect alphaTestEffect;

        private static readonly DepthStencilState stencilMask = new DepthStencilState
        {
            StencilEnable = true,
            StencilFunction = CompareFunction.Always,
            StencilPass = StencilOperation.Replace,
            ReferenceStencil = 1,
            DepthBufferEnable = false,
        };

        private static readonly DepthStencilState stencilContent = new DepthStencilState
        {
            StencilEnable = true,
            StencilFunction = CompareFunction.LessEqual,
            StencilPass = StencilOperation.Keep,
            ReferenceStencil = 1,
            DepthBufferEnable = false,
        };

        public static void Load()
        {
            IL.Celeste.Level.Render += LevelRender;
        }

        public static void Unload()
        {
            IL.Celeste.Level.Render -= LevelRender;
        }

        public static void Initialize()
        {
            // Can't set these up in the static constructor because...
            // Load() is called before the GraphicsDevice and GFX atlas exist.
            // derpeline
            alphaTestEffect = new AlphaTestEffect(Engine.Graphics.GraphicsDevice)
            {
                VertexColorEnabled = true,
                DiffuseColor = Color.White.ToVector3(),
                AlphaFunction = CompareFunction.GreaterEqual,
                ReferenceAlpha = 1,
                World = Matrix.Identity,
                View = Matrix.Identity,
                Projection = Matrix.CreateOrthographicOffCenter(0, 320, 180, 0, 0, 1)
            };
        }

        private static void LevelRender(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            if (
                cursor.TryGotoNext(
                    MoveType.Before,
                    instr => instr.MatchLdnull(),
                    instr => instr.MatchCallvirt<GraphicsDevice>("SetRenderTarget")
                )
            )
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Action<Level>>(RenderGrapher);
            }
        }

        private static void RenderGrapher(Level level)
        {
            if (!Visible)
            {
                return;
            }

            Player player = level.Tracker.GetEntity<Player>();

            // Do nothing if there's no Player entity, instead of crashing
            if (player == null)
            {
                return;
            }

            if (stencil == null)
            {
                stencil = VirtualContent.CreateRenderTarget(
                    "graph",
                    320,
                    180,
                    depth: true
                );
            }

            // jank!!!
            Vector2 playerCenter =
                player.Position - level.Camera.Position - new Vector2(0, player.Height / 2f);

            // Start rendering to our stencil
            Engine.Graphics.GraphicsDevice.SetRenderTarget(stencil);
            Engine.Graphics.GraphicsDevice.Clear(
                ClearOptions.Target | ClearOptions.Stencil,
                Color.Transparent,
                0,
                0
            );

            // Start the stencil spritebatch
            Draw.SpriteBatch.Begin(
                SpriteSortMode.Immediate,
                BlendState.AlphaBlend,
                null,
                stencilMask,
                null,
                alphaTestEffect,
                level.Camera.Matrix
            );

            /////////////////////////////////////////////////////////////////////////////////////
            /////////////////////////////////////////////////////////////////////////////////////
            k += Engine.DeltaTime * timeMod; 
            Vector2 initialValue = player.Position;
            Vector2 prevValue = Vector2.Zero;
            float theta = step;
            Vector2 currentValue = Vector2.Zero;

            Vector2 st = prevValue + playerCenter;
            Vector2 end = currentValue + playerCenter;


            /*for (; theta <= (float)Math.PI * 4f; theta += step)
            {
                yieldPointFromFunction(theta, k, ref currentValue);
                st = prevValue + playerCenter;
                CurrentNode = currentValue + playerCenter;
                Draw.Line(st, CurrentNode, From.White, lineWidth);
                prevValue = currentValue;
            }*/
            Vector2 distance = new Vector2(playerCenter.X+20, playerCenter.Y+20);
            Draw.Line(playerCenter, distance , Color.White, lineWidth);
            Draw.SpriteBatch.End();
            /////////////////////////////////////////////////////////////////////////////////////
            /////////////////////////////////////////////////////////////////////////////////////


            Draw.SpriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                null,
                stencilContent,
                null
            );
            Draw.SpriteBatch.Draw(GameplayBuffers.Level, Vector2.Zero, Calc.HexToColor(luaColor)*colorAlpha);
            Draw.SpriteBatch.End();

            // Start rendering to the level again
            Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.Level);
            ColorGrade.Set(GFX.ColorGrades[ColorGradeName]);
            // Draw our stencil content over the level, with the Color grade
            Draw.SpriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                null,
                stencilContent,
                null,
                ColorGrade.Effect
            );
            Draw.SpriteBatch.Draw(stencil, Vector2.Zero, Color.White);
            Draw.SpriteBatch.End();
        }
        private static void yieldPointFromFunction(float theta, float k, ref Vector2 currentValue)
        {
            if (size <= 10 && sizeMult)
            {
                size = size * 10f;
                sizeMult = false;
            }
            currentValue.X = (float)(Math.Sin(k * theta) * Math.Cos(theta)) * size;
            currentValue.Y = (float)(Math.Sin(k * theta) * Math.Sin(theta)) * size;
        }

    }
}
