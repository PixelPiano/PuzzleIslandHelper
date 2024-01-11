using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using Celeste.Mod.Backdrops;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using FrostHelper;
namespace Celeste.Mod.PuzzleIslandHelper.Effects
{
    [CustomBackdrop("PuzzleIslandHelper/BlockGlitch")]
    public class BlockGlitch : Backdrop
    {

        private Level level;
        public static Effect Shader;
        public static int Blocks = 7;
        public static float Buffer = 10;
        public static float BlendTime;
        public static float CurrentBlendTime;
        public static float CurrentBufferTime;
        public static bool Blending;

        private float seed;
        private static VirtualRenderTarget _Target;
        public static VirtualRenderTarget Target => _Target ??=
                      VirtualContent.CreateRenderTarget("GlitchBlockTarget", 320, 180);
        private static VirtualRenderTarget _BgTarget;
        public static VirtualRenderTarget BgTarget => _BgTarget ??=
                      VirtualContent.CreateRenderTarget("BgGlitchBlockTarget", 320, 180);
        private static VirtualRenderTarget _Blend;
        public static VirtualRenderTarget Blend => _Blend ??=
              VirtualContent.CreateRenderTarget("GlitchBlockBlend", 320, 180);

        [OnLoad]
        public static void Load()
        {
            Everest.Content.OnUpdate += Content_OnUpdate;
            IL.Celeste.Level.Render += VeryFunny;

        }
        public static void Unload()
        {
            Shader?.Dispose();
            IL.Celeste.Level.Render -= VeryFunny;
            Everest.Content.OnUpdate -= Content_OnUpdate;
        }

        private void ResetBuffers()
        {
            Buffer = Calc.Random.Range(3, 30);
            BlendTime = Calc.Random.Range(0.5f, 5);
            CurrentBlendTime = 0;
            CurrentBufferTime = 0;
        }
        public BlockGlitch()
        {
            ResetBuffers();
        }
        private static void VeryFunny(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(
                    MoveType.After,
                    instr => instr.Match(OpCodes.Ldsfld),
                    instr => instr.Match(OpCodes.Call),
                    instr => instr.Match(OpCodes.Callvirt)
                    ))
            {
                ILLabel label = cursor.DefineLabel();
                cursor.EmitDelegate(SkipClear);
                cursor.Emit(OpCodes.Brtrue, label);
                if (cursor.TryGotoNext(MoveType.Before, instr => instr.Match(OpCodes.Ldarg_0)))
                {
                    cursor.MarkLabel(label);
                }
            }
        }

        private static bool SkipClear()
        {
            return Blending;
        }
        private static void Content_OnUpdate(ModAsset from, ModAsset to)
        {
            if (to.Format == "cso" || to.Format == ".cso")
            {
                try
                {
                    AssetReloadHelper.Do("Reloading Shader", () =>
                    {
                        var effectName = to.PathVirtual.Substring("Effects/".Length, to.PathVirtual.Length - ".cso".Length - "Effects/".Length);

                        if (Shader is not null)
                        {
                            if (!Shader.IsDisposed)
                                Shader.Dispose();
                        }
                        Shader = ShaderHelper.TryGetEffect("jitter");
                    }, () =>
                    {
                        (Engine.Scene as Level)?.Reload();
                    });

                }
                catch (Exception e)
                {
                    // there's a catch-all filter on Content.OnUpdate that completely ignores the exception,
                    // would nice to actually see it though
                    Logger.LogDetailed(e);
                }

            }
        }
        public override void BeforeRender(Scene scene)
        {
            base.BeforeRender(scene);
            level = scene as Level;
            if (!IsVisible(scene as Level))
            {
                return;
            }
            Shader.ApplyScreenSpaceParameters(level);


            Engine.Graphics.GraphicsDevice.SetRenderTarget(Target);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);

            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointWrap, null, null, Shader);

            List<BlockGlitchArea> areas = level.Tracker.GetEntities<BlockGlitchArea>().Cast<BlockGlitchArea>().ToList();

            for (int i = 0; i < Blocks / 2; i++)
            {
                if (i < areas.Count)
                {
                    Draw.SpriteBatch.Draw(GameplayBuffers.Level, areas[i].Position, areas[i].Bounds, areas[i].Color);
                }
            }

            Draw.SpriteBatch.End();
            Engine.Graphics.GraphicsDevice.SetRenderTarget(BgTarget);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);

            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointWrap, null, null, Shader);

            for (int i = Blocks / 2; i < Blocks; i++)
            {
                if (i < areas.Count)
                {
                    Draw.SpriteBatch.Draw(GameplayBuffers.Level, areas[i].Position, areas[i].Bounds, areas[i].Color);
                }
            }
            Draw.SpriteBatch.End();
            float val = Glitch.Value;
            Glitch.Value = Calc.Random.Range(0.02f, 0.07f);

            Glitch.Apply(Target, scene.TimeActive, seed, Calc.Random.Range(1, 10));
            Glitch.Apply(BgTarget, scene.TimeActive, seed, Calc.Random.Range(1, 10));
            Glitch.Value = val;
        }

        public override void Render(Scene scene)
        {
            base.Render(scene);

            if (IsVisible(scene as Level))
            {
                Draw.SpriteBatch.Draw(BgTarget, Vector2.Zero, Color.Lerp(Color.White, Color.Black, 0.6f));

                Draw.SpriteBatch.Draw(GameplayBuffers.Gameplay, Vector2.Zero, Color.White);

                Draw.SpriteBatch.Draw(Target, Vector2.Zero, Color.White);
            }
        }
        public override void Update(Scene scene)
        {
            base.Update(scene);
            level = scene as Level;
            if (level is not null)
            {
                if (IsVisible(level))
                {
                    if (!Blending)
                    {
                        CurrentBufferTime += Engine.DeltaTime;
                        if (CurrentBufferTime > Buffer)
                        {
                            Blending = true;
                        }
                    }
                    else
                    {
                        CurrentBlendTime += Engine.DeltaTime;
                        if (CurrentBlendTime > BlendTime)
                        {
                            Blending = false;
                            ResetBuffers();
                        }
                    }
                    if (scene.OnInterval(2 / 60f))
                    {
                        seed = Calc.Random.NextFloat();
                    }
                    if (level.Tracker.GetEntities<BlockGlitchArea>().Count < Blocks)
                    {
                        level.Add(new BlockGlitchArea());
                    }
                }
            }
        }
    }

}
