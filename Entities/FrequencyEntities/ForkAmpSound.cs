using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Components.Visualizers.DSPs;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using Celeste.Mod.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Monocle;
using System;
using System.Collections;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.FrequencyEntities
{
    [Tracked(false)]
    public class ForkAmpSound : Entity
    {
        [Command("test_sound", "")]
        public static ForkAmpSound AddLocalSound(float a, float b, float c, float d, string id)
        {
            ForkAmpSound entity = new ForkAmpSound(false, true) { ID = id };
            Engine.Scene.Add(entity);
            return entity;
        }

        public static ForkAmpSound GlobalSound;
        public static void Start(bool fadeIn = true)
        {
            if (GlobalSound == null)
            {
                Engine.Scene.Add(GlobalSound = new ForkAmpSound(true, true));
            }
            else
            {
                GlobalSound.StartAllSources(true);
            }
            GlobalPlaying = true;
        }
        public static void Stop(bool fadeOut = true)
        {
            GlobalSound?.StopAllSources(fadeOut);
            GlobalPlaying = false;
        }
        public static void StopAll(bool fadeOut = true, Scene scene = null)
        {
            scene ??= Engine.Scene;
            if(scene == null) return;
            foreach (ForkAmpSound s in scene.Tracker.GetEntities<ForkAmpSound>())
            {
                s.StopAllSources(fadeOut);
            }
            GlobalPlaying = false;
        }
        public static bool GlobalPlaying { get; private set; }
        protected SoundSource[] Sources;
        public float Volume
        {
            get => volume;
            private set
            {
                float finalValue = Math.Clamp(value, 0, 1);
                if (volume != finalValue)
                {
                    UpdateVolumes(finalValue, ducks);
                    volume = finalValue;
                }
            }
        }
        private float volume = 1;
        private float[] ducks = new float[4];
        private float duckArea = 6f;
        private float duckMult = 1f;
        private Tween fadeTween;
        public string ID;
        public bool Playing;
        private bool startImmediately;
        [OnLoad]
        public static void Load()
        {
            Everest.Events.Level.OnLoadLevel += Level_OnLoadLevel;
        }

        [OnUnload]
        public static void Unload()
        {
            Everest.Events.Level.OnLoadLevel -= Level_OnLoadLevel;
        }
        private static void Level_OnLoadLevel(Level level, Player.IntroTypes playerIntro, bool isFromLoader)
        {
            if (playerIntro != Player.IntroTypes.Transition || !isFromLoader)
            {
                StopAll(false, level);
            }
        }

        public ForkAmpSound(bool global = true, bool start = false) : base()
        {
            Depth = int.MinValue;
            startImmediately = start;
            if (global)
            {
                Tag |= Tags.Global;
                GlobalSound = this;
                Add(new LevelEndingHook(() =>
                {
                    StopAll();
                    RemoveGlobal();
                }
                ));
            }
            Tag |= Tags.TransitionUpdate;
            Sources = new SoundSource[4];
            for (int i = 0; i < 4; i++)
            {
                Add(Sources[i] = new SoundSource());
            }
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            if (GlobalSound == this)
            {
                if (GlobalPlaying)
                {
                    Draw.HollowRect(camera.Position, 6, 3, Color.Magenta);
                    for (int i = 0; i < Sources.Length; i++)
                    {
                        SoundSource source = Sources[i];
                        Color color;
                        if (source.Playing) color = Color.Lime;
                        else if (source.InstancePlaying) color = Color.Cyan;
                        else if (source.instance != null) color = Color.White;
                        else color = Color.Red;
                        Draw.Point(camera.Position + new Vector2(1 + i, 1), color);
                    }
                }
                else
                {
                    Draw.HollowRect(camera.Position, 6, 3, Color.DarkRed);
                }
            }
            else if (!TagCheck(Tags.Global))
            {
                Draw.HollowRect(camera.Position + Vector2.UnitY * 3, 4, 3, Playing ? Color.Lime : Color.DarkGreen);
            }
        }
        public void Silence()
        {
            Volume = 0;
            StopAllSources(false);
        }
        public static void SilenceGlobal()
        {
            GlobalSound?.Silence();
        }
        public static void RemoveGlobal()
        {
            SilenceGlobal();
            GlobalSound?.RemoveSelf();
            GlobalSound = null;
        }
        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            if (GlobalSound == this)
            {
                RemoveGlobal();
            }
            else
            {
                Silence();
            }
        }
        public override void SceneBegin(Scene scene)
        {
            base.SceneBegin(scene);
            Silence();
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (startImmediately)
            {
                if (GlobalSound == this)
                {
                    GlobalPlaying = true;
                }
                StartAllSources(true);
            }
        }
        public override void Update()
        {
            base.Update();
            Vector2 position = Scene.GetPlayer()?.Center ?? Vector2.Zero;
            bool anyPlaying = false;
            foreach (SoundSource source in Sources)
            {
                if (source.Playing || source.InstancePlaying)
                {
                    anyPlaying = true;
                    break;
                }
            }
            if (anyPlaying)
            {
                float[] rates = FrequencyData.GetRates(Scene, ID);
                float[] prevRates = FrequencyData.GetPrevRates(Scene, ID);
                for (int i = 0; i < rates.Length; i++)
                {
                    SoundSource source = Sources[i];
                    if (source.Playing || source.InstancePlaying)
                    {
                        source.Position = position;
                        source.UpdateSfxPosition();
                        source.Param("Pitch", (rates[i] / FrequencyData.Max) * 240f);
                    }
                    if (i != 0)
                    {
                        float duckTarget = 0;
                        if (prevRates[i] == rates[i]) //if rate not being changed
                        {
                            for (int r = i - 1; r >= 0; r--)
                            {
                                if (r != i)
                                {
                                    float dist = MathHelper.Distance(rates[i], rates[r]);
                                    if (dist < duckArea)
                                    {
                                        duckTarget = Math.Max(duckTarget, (duckArea - dist) / duckArea);
                                    }
                                }
                            }
                        }
                        ducks[i] = Calc.Approach(ducks[i], duckTarget, Engine.DeltaTime * 3);
                    }
                }
                UpdateVolumes(Volume, ducks);
            }
        }
        public void UpdateVolumes(float volume, float[] ducks)
        {
            int count = 0;
            foreach (SoundSource source in Sources)
            {
                float duck = 0;
                if (ducks.Length > count)
                {
                    duck = ducks[count];
                }
                if (source.Playing || source.InstancePlaying)
                {
                    source.Param("Vol", Math.Clamp(volume - duck * duckMult, 0, 1));
                }
                count++;
            }
        }
        public void StopAllSources(bool fadeOut = true)
        {
            fadeTween?.RemoveSelf();
            if (fadeOut)
            {
                fadeTween = Tween.Set(this, Tween.TweenMode.Oneshot, 1, Ease.Linear, t =>
                {
                    Volume = Calc.LerpClamp(1, 0, t.Eased);
                },
                t =>
                {
                    Volume = 0;
                    StopAllSources(false);
                });
            }
            else
            {
                Volume = 0;
                foreach (SoundSource source in Sources)
                {
                    if (source.instance != null)
                    {
                        source.Stop(fadeOut);
                    }
                }
                Playing = false;
            }
        }
        public void StartAllSources(bool fadeIn = true)
        {
            Playing = true;
            foreach (SoundSource source in Sources)
            {
                source.Play("event:/PianoBoy/Soundwaves/tuningForkLoop2");
            }
            fadeTween?.RemoveSelf();
            if (fadeIn)
            {
                Volume = 0;
                fadeTween = Tween.Set(this, Tween.TweenMode.Oneshot, 1, Ease.Linear, t =>
                {
                    Volume = Calc.LerpClamp(0, 1, t.Eased);
                },
                t =>
                {
                    Volume = 1;
                });
            }
            else
            {
                Volume = 1;
            }
        }
        public void DrawVolumesAndDucks(Vector2 bottomRight)
        {
            ActiveFont.DrawOutline("Volume: " + Volume, bottomRight - Vector2.UnitY * ActiveFont.LineHeight * 5, new Vector2(1, 1), Vector2.One, Color.White, 5, Color.Black);
            for (int i = 0; i < ducks.Length; i++)
            {
                ActiveFont.DrawOutline("Duck " + (i + 1) + ": " + (ducks[i] * duckMult).ToString("#.##"), bottomRight - Vector2.UnitY * ActiveFont.LineHeight * (5 - (i + 1)), new Vector2(1, 1), Vector2.One, Color.White, 5, Color.Black);
            }
        }
    }
}