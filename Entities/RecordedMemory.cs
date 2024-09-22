using Celeste.Mod.Core;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked]
    public class RecordedMemory : Entity
    {
        public VirtualRenderTarget[] Frames;
        public int RecordFrame;
        public int RenderFrame;
        public int TotalFrames;
        public bool Recording;
        public Color Color = Color.White;
        public float Alpha = 1;
        public RecordedMemory(int frames, bool persist) : base()
        {
            TotalFrames = (int)Calc.Max(frames, 0);
            if (TotalFrames > 0)
            {
                Frames = new VirtualRenderTarget[frames];
                for (int i = 0; i < frames; i++)
                {
                    Frames[i] = VirtualContent.CreateRenderTarget("recorded_memory_frame_" + i, 320, 180);
                }
                Recording = true;
                Add(new BeforeRenderHook(BeforeRender));
                if (persist)
                {
                    AddTag(Tags.Persistent);
                    PianoModule.Session.Memories.Add(this);
                }
            }
        }
        public static void ChanceAMemory(Level level, int frames, bool persist, float chance)
        {
            if (Calc.Random.Chance(chance))
            {
                level.Add(new RecordedMemory(frames, persist));
            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Dispose();
            if (PianoModule.Session.Memories.Contains(this))
            {
                PianoModule.Session.Memories.Remove(this);
            }
        }
        public void Dispose()
        {
            for (int i = 0; i < Frames.Length; i++)
            {
                Frames[i]?.Dispose();
                Frames[i] = null;
            }
        }
        public void BeforeRender()
        {
            if (Visible && Recording && Frames != null && Frames[RecordFrame] != null && Scene is Level level)
            {
                Frames[RecordFrame].SetRenderTarget(Color.Transparent);
                Draw.SpriteBatch.StandardBegin(null, Matrix.Identity);
                Draw.SpriteBatch.Draw(GameplayBuffers.Gameplay, Vector2.Zero, Color.White);
                Draw.SpriteBatch.End();
                RecordFrame++;
                if (RecordFrame >= TotalFrames)
                {
                    Recording = false;
                }
            }
        }
        public override void Render()
        {
            base.Render();
            if (!Recording && Frames != null && Frames[RenderFrame] != null && Scene is Level level)
            {
                Draw.SpriteBatch.Draw(Frames[RenderFrame], level.Camera.Position, Color * Alpha);
                RenderFrame++;
                RenderFrame %= TotalFrames;
            }
        }
    }
}