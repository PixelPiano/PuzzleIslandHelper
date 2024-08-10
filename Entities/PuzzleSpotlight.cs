using System;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/PuzzleSpotlight")]
    [Tracked]
    public class PuzzleSpotlight : Entity
    {
        public bool State = false; // toggles effect visibility
        public string ColorGradeName = "PianoBoy/invertFlag"; // colorgrade to use for the effect
        public float Radius = 30; // radius of the center circle
        private string flag;
        private static bool flagValue;
        private static float _Radius;
        public float BeamLength = 320;
        public float BeamWidth = 5;
        public float BeamCount = 4;
        public float RotateRate = 0;
        public float GapLength = 0;
        public float offsetRate=0;
        public bool hasGaps=true;
        public bool offset=false;
        private static float oS=0;
        private static readonly string circlePath = "decals/PianoBoy/util/circle";
        private static bool second = false;
        private static float rotateAd = 0f;
        private static MTexture filledCircle;
        private static VirtualRenderTarget stencil;
        private static AlphaTestEffect alphaTestEffect;
        
        private static readonly DepthStencilState stencilMask = new()
        {
            StencilEnable = true,
            StencilFunction = CompareFunction.Always,
            StencilPass = StencilOperation.Replace,
            ReferenceStencil = 1,
            DepthBufferEnable = false,
        };

        private static readonly DepthStencilState stencilContent = new ()
        {
            StencilEnable = true,
            StencilFunction = CompareFunction.LessEqual,
            StencilPass = StencilOperation.Keep,
            ReferenceStencil = 1,
            DepthBufferEnable = false,
        };
        public PuzzleSpotlight(Vector2 position)
         : this(position,false,"PianoBoy/Inverted",0,"spotlightFlag",0,0,0,0,0,0,false,false)
        {
        }
        public PuzzleSpotlight(Vector2 position, bool state, string colorGradeName, float radius, string flag, float beamLength, float beamWidth, float beamCount, float rotateRate, float gapLength, float offsetRate, bool hasGaps, bool offset, bool secondDesign = false)
        : base(position)
        {
            State = state;
            ColorGradeName = colorGradeName;
            Radius = radius;
            this.flag = flag;
            BeamLength = beamLength;
            BeamWidth = beamWidth;
            BeamCount = beamCount;
            RotateRate = rotateRate;
            GapLength = gapLength;
            this.offsetRate = offsetRate;
            this.hasGaps = hasGaps;
            this.offset = offset;
            second = secondDesign;
            _Radius = Radius;
            if (second)
            {
                ColorGradeName = "PuzzleIslandHelper/test";
            }
        }

        public PuzzleSpotlight(EntityData data, Vector2 offset)
          : this(data.Position + offset,
                data.Bool("startingState"),
                data.Attr("colorgrade"),
                data.Float("centerRadius", 30),
                data.Attr("flag", "spotlightFlag"),
                data.Float("beamLength", 320),
                data.Float("beamWidth", 5),
                data.Int("beams", 4),
                data.Float("rotationRate", 0),
                data.Float("gapLength", 0),
                data.Float("offsetRate", 0),
                data.Bool("segmentedBeams", true),
                data.Bool("hasOffset", false),
                data.Bool("secondDesign", false))
        {
        }
        public override void Update()
        {
            base.Update();
            flagValue = SceneAs<Level>().Session.GetFlag(flag);
            if (offset)
            {
                oS += offsetRate * Engine.DeltaTime;
            }
            rotateAd += DigitalCircle.rotationAdd/15;
            if (second)
            {
                BeamLength = DigitalCircle.beamLength;
                Radius = DigitalCircle.radius;
            }
            if(SceneAs<Level>().Session.GetFlag("colorCodeGrade") && second)
            {
                RemoveSelf();
            }
        }
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
            ILCursor cursor = new(il);
            if (
                cursor.TryGotoNext(
                    MoveType.Before,
                    instr => instr.MatchLdnull(),
                    instr => instr.MatchCallvirt<GraphicsDevice>("SetRenderTarget")
                )
            )
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Action<Level>>(RenderPuzzleSpotlight);
            }
        }
        private static void RenderPuzzleSpotlight(Level level)
        {
            PuzzleSpotlight renderer = level.Tracker.GetEntity<PuzzleSpotlight>();
            if (renderer == null || !renderer.State || !flagValue)
            {
                return;
            }

            Player player = level.Tracker.GetEntity<Player>();

            // Do nothing if there's no Player entity, instead of crashing
            if (player == null)
            {
                return;
            }

            stencil ??= VirtualContent.CreateRenderTarget(
                    "spooky-spotlight",
                    320,
                    180,
                    depth: true
                );

            // jank!!!
            Vector2 playerCenter =
                player.Position - level.Camera.Position - new Vector2(0, player.Height / 2f);
            if (second)
            {
                playerCenter = new Vector2(24*8 - _Radius + 5, 14.5f*8 - _Radius + 13);
            }
            // Start rendering to our stencil
            Engine.Graphics.GraphicsDevice.SetRenderTarget(stencil);
            Engine.Graphics.GraphicsDevice.Clear( ClearOptions.Target | ClearOptions.Stencil ,Color.Transparent, 0, 0);

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
                    (int)(playerCenter.X - renderer.Radius),
                    (int)(playerCenter.Y - renderer.Radius),
                    2 * (int)renderer.Radius,
                    2 * (int)renderer.Radius
                ),
                Color.White
            );

            // Draw the beams
            if (second)
            {
                for(int i = 0; i<renderer.BeamCount; i++)
                {
                    for(float k = 0; k<28; k++)
                    {
                        Draw.LineAngle(
                            playerCenter,
                            ((float)Math.PI * 2f / renderer.BeamCount * i)-0.5f + renderer.RotateRate+(k/180f)+rotateAd,
                            renderer.BeamLength,
                            Color.White,
                            renderer.BeamWidth
                            );
                    }
                }
            }
            else if (!renderer.hasGaps)
            {
                for (int i = 0; i < renderer.BeamCount; i++)
                {
                    Draw.LineAngle(
                        playerCenter,
                        ((float)Math.PI * 2f / renderer.BeamCount * i) + renderer.RotateRate,
                        renderer.BeamLength,
                        Color.White,
                        renderer.BeamWidth
                    );
                }
            }
            else
            {
                 float angle;
                 for (int i = 0; i < renderer.BeamCount; i++)
                {
                   float current= renderer.GapLength +oS;
                   for(int j=0;j<=320/(renderer.GapLength + renderer.BeamLength);j++){
                        angle= ((float)Math.PI * 2f / renderer.BeamCount * i)+renderer.RotateRate;
                         Draw.LineAngle(
                             playerCenter + 
                             (new Vector2((float)Math.Cos(angle),(float)Math.Sin(angle)))
                             *current,
                             angle,
                             renderer.BeamLength,
                             Color.White,
                             renderer.BeamWidth
                    );
                        if(!renderer.offset){
                            current += renderer.GapLength + renderer.BeamLength;
                        }else{
                            current = (current+ renderer.GapLength + renderer.BeamLength) % 320;
                            //oS += renderer.offsetRate * Engine.DeltaTime;
                            
                        }
                    }
                    
                }
            }

            Draw.SpriteBatch.End();

            // Draw the level heeheeBuffer onto our stencil
            Draw.SpriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                null,
                stencilContent,
                null
            );
            float opacity = second ? 0.8f : 1;
            Draw.SpriteBatch.Draw(GameplayBuffers.Level, Vector2.Zero, Color.White*opacity);
            Draw.SpriteBatch.End();

            // Start rendering to the level heeheeBuffer again
            Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.Level);
            ColorGrade.Set(GFX.ColorGrades[renderer.ColorGradeName]);

            // Draw our stencil content over the level heeheeBuffer, with the Color grade
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
