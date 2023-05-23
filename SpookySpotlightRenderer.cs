using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;

namespace Celeste.Mod.PuzzleIslandHelper
{
    public class SpookySpotlightRenderer
    {
        public static bool Visible = false; // toggles effect visibility
        public static string ColorGradeName = "PianoBoy/inverted"; // colorgrade to use for the effect
        public static float Radius = 30; // radius of the center circle
        public static float BeamLength = 320;
        public static float BeamWidth = 5;
        public static float BeamCount = 4;
        public static float RotateRate = 0;
        public static float GapLength = 0;
        public static float offsetRate=0;
        public static bool hasGaps=true;
        public static bool offset=false;
        private static float oS=0;
        private static readonly string circlePath = "decals/PianoBoy/util/circle";

        private static MTexture filledCircle;
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

            filledCircle = GFX.Game[circlePath];
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
                cursor.EmitDelegate<Action<Level>>(RenderSpookySpotlight);
            }
        }

        private static void RenderSpookySpotlight(Level level)
        {
            if (!Visible)
            {
                return;
            }

            Player player = level.Tracker.GetEntity<Player>();

            // Do nothing if there's no player entity, instead of crashing
            if (player == null)
            {
                return;
            }

            if (stencil == null)
            {
                stencil = VirtualContent.CreateRenderTarget(
                    "spooky-spotlight",
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

            // Draw the circle
            Draw.SpriteBatch.Draw(
                filledCircle.Texture.Texture_Safe,
                new Rectangle(
                    (int)(playerCenter.X - Radius),
                    (int)(playerCenter.Y - Radius),
                    2 * (int)Radius,
                    2 * (int)Radius
                ),
                Color.White
            );

            // Draw the beams
            if (!hasGaps)
            {
                for (int i = 0; i < BeamCount; i++)
                {
                    Draw.LineAngle(
                        playerCenter,
                        ((float)Math.PI * 2f / BeamCount * i) + RotateRate,
                        BeamLength,
                        Color.White,
                        BeamWidth
                    );
                }
            }
            else
            {
                 float angle;
                 for (int i = 0; i < BeamCount; i++)
                {
                   float current=GapLength+oS;
                   for(int j=0;j<=320/(GapLength+BeamLength);j++){
                        angle= ((float)Math.PI * 2f / BeamCount * i)+RotateRate;
                         Draw.LineAngle(
                             playerCenter + 
                             (new Vector2((float)Math.Cos(angle),(float)Math.Sin(angle)))
                             *current,
                             angle,
                             BeamLength,
                             Color.White,
                             BeamWidth
                    );
                        if(!offset){
                            current += GapLength+BeamLength;
                        }else{
                            current = (current+GapLength+BeamLength) % 320;
                            oS += offsetRate * Engine.DeltaTime;
                            
                        }
                    }
                    
                }
            }

            Draw.SpriteBatch.End();

            // Draw the level buffer onto our stencil
            Draw.SpriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                null,
                stencilContent,
                null
            );
            Draw.SpriteBatch.Draw(GameplayBuffers.Level, Vector2.Zero, Color.White);
            Draw.SpriteBatch.End();

            // Start rendering to the level buffer again
            Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.Level);
            ColorGrade.Set(GFX.ColorGrades[ColorGradeName]);

            // Draw our stencil content over the level buffer, with the color grade
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
    }
}
