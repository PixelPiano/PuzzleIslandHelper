using Celeste.Mod.Backdrops;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using FrostHelper;
using FrostHelper.ModIntegration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Security.Cryptography;

namespace Celeste.Mod.PuzzleIslandHelper.Effects
{
    [CustomBackdrop("PuzzleIslandHelper/MemoryGrid")]
    public class MemoryGrid : Backdrop
    {
        public class Block
        {
            public int X;
            public int Y;
            public int Z;
            public Random Random;
            public float Speed;
            public Vector3 NextDirection;
            public Vector3 Direction;
            public Vector3 Position;
            public float Timer;
            public Vector3 MaxDist = new Vector3(2, 2, 1);
            public Block(int x, int y, int z)
            {
                X = x;
                Y = y;
                Z = z;
                Random = new Random((x * 7) + (y * 22) + (z * 1053));
            }
            public Vector3 GetRandomSign()
            {
                Vector3 r = new Vector3
                {
                    X = Random.Choose(-1, 1),
                    Y = Random.Choose(-1, 1),
                    Z = Random.Choose(-1, 1)
                };
                Vector3 ab = new Vector3(Math.Abs(Position.X), Math.Abs(Position.Y), Math.Abs(Position.Z));
                if (ab.X > MaxDist.X && ab.X + r.X > ab.X) r.X *= -1;
                if (ab.Y > MaxDist.Y && ab.Y + r.Y > ab.Y) r.Y *= -1;
                if (ab.Z > MaxDist.Z && ab.Z + r.Z > ab.Z) r.Z *= -1;
                Timer = Random.Range(0.3f, 1);
                return r;
            }
            public void Update()
            {
                if (Timer > 0)
                {
                    Timer -= Engine.DeltaTime;
                }
                else
                {
                    NextDirection = GetRandomSign();
                }
                Direction = Calc.Approach(Direction, NextDirection, Engine.DeltaTime);
                Position += Direction * 0.1f;

            }
        }
        public readonly string Path = "PuzzleIslandHelper/Shaders/memoryGrid";
        public Vector3[] Offsets = new Vector3[27];
        public Block[,,] Blocks = new Block[3, 3, 3];
        private static VirtualRenderTarget _target;
        public static VirtualRenderTarget Target => _target ??= VirtualContent.CreateRenderTarget("memory_grid", 320, 180);
        public MemoryGrid(BinaryPacker.Element data) : base()
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        Blocks[i,j,k] = new Block(i, j, k);
                    }
                }
            }
        }
        public Effect ApplyParameters(Effect effect, Level level)
        {
            if (effect == null) return null;
            effect.ApplyParameters(level, Matrix.Identity, 0);

            effect.Parameters["Offsets"]?.SetValue(Offsets);
            return effect;
        }
        public override void Ended(Scene scene)
        {
            base.Ended(scene);
            _target?.Dispose();
            _target = null;
        }
        public override void Update(Scene scene)
        {
            base.Update(scene);
            float sin = (float)(Math.Sin(scene.TimeActive) + 1) / 2f;
            for(int i = 0; i<3; i++)
            {
                for(int j = 0; j<3; j++)
                {
                    for(int k = 0; k<3; k++)
                    {
                        Blocks[i,j,k].Update();
                        Offsets[i + 3 * j + 3 * 3 * k] = Blocks[i,j,k].Position;
                    }
                }
            }
        }

        public override void BeforeRender(Scene scene)
        {
            base.BeforeRender(scene);
            Target.SetAsTarget(Color.White);
        }
        public override void Render(Scene scene)
        {
            base.Render(scene);
            if (scene is not Level level) return;
            Effect effect = ShaderHelperIntegration.GetEffect(Path);
            effect = ApplyParameters(effect, level);
            Draw.SpriteBatch.End();
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap,
                DepthStencilState.None, RasterizerState.CullNone, effect, Matrix.Identity);
            Draw.SpriteBatch.Draw(Target, Vector2.Zero, Color.White);
            Draw.SpriteBatch.End();
            GameplayRenderer.Begin();
        }
    }
}

