using FrostHelper.ModIntegration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System.Collections.Generic;

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
                Frames[RecordFrame].SetAsTarget(Color.Transparent);
                Draw.SpriteBatch.StandardBegin(Matrix.Identity);
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


        //[Command("record_memory_and_save", "")]
        public static void Record(int frames, string name, float interval)
        {
            if (Engine.Scene is Level level)
            {
                level.Add(new MemoryRecorder(name, frames, interval));
            }
        }
        [Command("stop_active_recording", "")]
        public static void StopRecording()
        {
            if (Engine.Scene is Level level)
            {
                foreach (MemoryRecorder recording in level.Tracker.GetEntities<MemoryRecorder>())
                {
                    recording.Recording = false;
                }
            }
        }
        [Tracked]
        public class MemoryRecorder : Entity
        {
            public List<VirtualRenderTarget> Frames = [];
            public int RecordFrame;
            public int RenderFrame;
            public int TotalFrames;
            public bool Recording;
            public Color Color = Color.White;
            public float Alpha = 1;
            private float timer;
            private float interval;
            private string Name;
            public MemoryRecorder(string name, int frames = -1, float interval = -1) : base()
            {
                Tag |= Tags.TransitionUpdate | Tags.Global;
                Name = name;
                this.interval = interval;
                timer = interval;
                TotalFrames = frames;
                Recording = true;
                Add(new BeforeRenderHook(BeforeRender));
            }
            public override void Update()
            {
                base.Update();
                if (TotalFrames > 0 && RecordFrame >= TotalFrames)
                {
                    Recording = false;
                }
                if (Recording)
                {
                    if (timer <= 0)
                    {
                        timer = interval;
                        Visible = true;
                    }
                    else
                    {
                        Visible = false;
                        timer -= Engine.DeltaTime;
                    }
                }
                else
                {
                    int count = 0;
                    foreach (VirtualRenderTarget target in Frames)
                    {
                        if (target != null)
                        {
                            PianoUtils.SaveTargetAsPng((RenderTarget2D)target, "Screenshots/MemoryRecordings/" + Name + count.ToString() + ".png", 0, 0, 320, 180, 6);
                            count++;
                        }
                    }
                    RemoveSelf();
                }
            }
            public override void Removed(Scene scene)
            {
                base.Removed(scene);
                Dispose();
            }
            public void Dispose()
            {
                for (int i = 0; i < Frames.Count; i++)
                {
                    Frames[i]?.Dispose();
                    Frames[i] = null;
                }
            }
            public void BeforeRender()
            {
                if (Visible && Recording)
                {
                    if (Frames.Count <= RecordFrame)
                    {
                        Frames.Add(VirtualContent.CreateRenderTarget("recorded_memory_frame_" + RecordFrame, 320, 180));
                    }
                    var frame = Frames[RecordFrame];
                    frame.SetAsTarget(Color.Transparent);
                    Draw.SpriteBatch.StandardBegin(Matrix.Identity);
                    Draw.SpriteBatch.Draw(GameplayBuffers.Level, Vector2.Zero, Color.White);
                    Draw.SpriteBatch.End();
                    RecordFrame++;
                }
            }
            public override void Render()
            {
                base.Render();
            }
        }



        [Command("add_memory_playback", "")]
        public static void AddMemoryPlayback(string path, float delay = -1, string effect = "")
        {
            if (Engine.Scene is Level level)
            {
                level.Add(new MemoryPlayback(path, delay, effect));
            }
        }
        public class MemoryPlayback : Entity
        {
            public Sprite Sprite;
            public Effect Effect;
            private string effectName;
            public MemoryPlayback(EntityData data, Vector2 offset) : this(data.Attr("path"), data.Float("delay"), data.Attr("effect"))
            {
            }
            public override void Removed(Scene scene)
            {
                base.Removed(scene);
                Effect?.Dispose();
                Effect = null;
            }
            public MemoryPlayback(string path, float delay = -1, string effect = "") : base()
            {
                Tag |= TagsExt.SubHUD;
                Sprite = new Sprite(GFX.Game, "recordings/PuzzleIslandHelper/" + path + "/");
                delay = delay < 0 ? Engine.DeltaTime : delay;
                Sprite.AddLoop("idle", "memory", Calc.Max(Engine.DeltaTime, delay));
                Add(Sprite);
                Sprite.Play("idle");
                effectName = effect;
            }
            public override void Render()
            {
                if (!string.IsNullOrEmpty(effectName))
                {
                    Effect = ShaderHelperIntegration.TryGetEffect(effectName);
                    if (Effect != null)
                    {
                        Draw.SpriteBatch.End();
                        Effect.ApplyVectorZeroParams(Scene as Level);
                        Draw.SpriteBatch.StandardBegin(Matrix.Identity, Effect);
                        base.Render();
                        Draw.SpriteBatch.End();
                        GameplayRenderer.Begin();
                    }
                    else
                    {
                        base.Render();
                    }
                }
                else
                {
                    base.Render();
                }

            }
        }
    }
}