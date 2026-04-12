using Celeste.Mod.PuzzleIslandHelper.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using Celeste.Mod.PuzzleIslandHelper.Entities.FrequencyEntities;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Components
{
    [Tracked]
    public class MonumentComponent : Component
    {
        public float Alpha = 1;
        public float WaveSpeed = 2;
        public float WaveAmplitude = 2;
        public int SliceSize = 1;
        public float SliceSinIncrement = 0.05f;
        public bool EaseDown;
        public float sineTimer = Calc.Random.NextFloat();
        public bool RetainCollision;
        public bool IsEnabled => monument != null && IDs != null && IDs.Contains(monument.MonumentComponentID) && monument.CompletedFlag;
        private bool wasEnabled;
        public string[] IDs;
        public float Width;
        public float Height;
        public Vector2 Offset;
        public int PadX;
        public int PadY;
        public float MOEPercent;
        private bool? lastVisibleState;
        private bool? lastCollidableState;

        private bool forceInvisible;
        private bool forceUncollidable;
        public int CellSize = 8;
        public bool useEntityWidth = true, useEntityHeight = true;
        public string Level;
        public Action<bool> OnEnableHook;
        public Action<bool> OnDisableHook;
        public Action EnableUpdate;
        public Action DisableUpdate;
        private bool initialized;
        private BeforeRenderHook beforeRender;
        private AfterRenderHook afterRender;
        private PreUpdateHook preUpdate;
        private PostUpdateHook postUpdate;
        private FrequencyMonument monument;
        public Action RenderCallback;
        public MonumentComponent() : base(true, true) { }
        public bool Check(Rectangle bounds)
        {
            if (MOEPercent >= 1) return true;
            Rectangle entityBounds = new Rectangle((int)(Entity.X + Offset.X - PadX), (int)(Entity.Y + Offset.Y - PadY), (int)Width + (PadX * 2), (int)Height + (PadY * 2));
            int cells = 0;
            int collisions = 0;
            int halfCell = CellSize / 2;
            for (int x = halfCell; x < bounds.Width; x += CellSize)
            {
                for (int y = halfCell; y < bounds.Height; y += CellSize)
                {
                    Rectangle cell = new Rectangle(bounds.X + x - halfCell, bounds.Y + y - halfCell, CellSize, CellSize);
                    if (cell.Colliding(entityBounds))
                    {
                        collisions++;
                    }
                    cells++;
                }
            }
            return collisions > 0 && (float)cells / collisions >= (1 - MOEPercent);

        }
        public override void EntityAdded(Scene scene)
        {
            base.EntityAdded(scene);
            RefreshSize(Entity);
            if (!initialized) initialize(Entity);
        }
        public void RefreshSize(Entity entity)
        {
            if (entity is Decal)
            {
                Decal decal = entity as Decal;
                MTexture texture = decal.textures[(int)decal.frame];
                if (useEntityWidth)
                {
                    Width = texture.Width;
                }
                if (useEntityHeight)
                {
                    Height = texture.Height;
                }
            }
            else
            {
                if (useEntityWidth)
                {
                    Width = entity.Width;
                }
                if (useEntityHeight)
                {
                    Height = entity.Height;
                }
            }
        }
        public override void EntityAwake()
        {
            base.EntityAwake();
            RefreshSize(Entity);
            monument = Scene.Tracker.GetEntity<FrequencyMonument>();
        }
        public void Enable(bool instant)
        {
            OnEnableHook?.Invoke(instant);
            wasEnabled = true;
        }
        public void Disable(bool instant)
        {
            OnDisableHook?.Invoke(instant);
            wasEnabled = false;
        }
        public override void Update()
        {
            base.Update();
            monument = Scene.Tracker.GetEntity<FrequencyMonument>();
            bool enabled = IsEnabled;
            if (enabled)
            {
                if (!wasEnabled)
                {
                    Enable(false);
                }
                EnableUpdate?.Invoke();
            }
            else
            {
                //if disabled
                if (wasEnabled)
                {
                    Disable(false);
                }
                DisableUpdate?.Invoke();
            }
            wasEnabled = enabled;
        }
        private void initialize(Entity entity)
        {
            initialized = true;
            entity.Add(preUpdate = new PreUpdateHook(() =>
            {
                if (lastVisibleState.HasValue)
                {
                    entity.Visible = lastVisibleState.Value;
                    lastVisibleState = null;
                }
                lastCollidableState = null;
                if (!IsEnabled)
                {
                    lastCollidableState = entity.Collidable;
                    entity.Collidable = RetainCollision;
                }
            }));
            entity.Add(postUpdate = new PostUpdateHook(() =>
            {
                if (lastCollidableState.HasValue)
                {
                    entity.Collidable = lastCollidableState.Value;
                    lastCollidableState = null;
                }
                lastVisibleState = null;
                if (!IsEnabled)
                {
                    lastVisibleState = entity.Visible;
                    entity.Visible = false;
                }
            }));
            wasEnabled = IsEnabled;
            if (wasEnabled)
            {
                Enable(true);
            }
            else
            {
                Disable(true);
            }
        }
        public override void Added(Entity entity)
        {
            base.Added(entity);
            if (!initialized) initialize(entity);
        }
        public override void Removed(Entity entity)
        {
            beforeRender?.RemoveSelf();
            afterRender?.RemoveSelf();
            preUpdate?.RemoveSelf();
            postUpdate?.RemoveSelf();
            base.Removed(entity);

        }
    }

    [ConstantEntity("PuzzleIslandHelper/MonumentRenderer")]
    [Tracked]
    public class MonumentManager : Entity
    {
        public static void ValueTo(List<MonumentRenderer> renderers, float value)
        {
            foreach (MonumentRenderer renderer in renderers)
            {
                renderer.Value = value;
            }
        }
        public static IEnumerator FadeInComponents(FrequencyMonument monument, MonumentManager manager)
        {
            Level level = monument.Scene as Level;
            if (Marker.TryFind(monument.MonumentComponentID + ":CameraTarget", out Vector2 position))
            {
                Rectangle bounds = level.Bounds;
                Vector2 camPosition = (position - new Vector2(160, 90)).Clamp(bounds);
                yield return CutsceneEntity.CameraTo(camPosition, 1, Ease.SineInOut, 0.6f);
            }
            yield return 0.6f;
            List<MonumentRenderer> renderers = manager.Renderers;
            for (int i = 0; i < 3; i++)
            {
                yield return Calc.Random.Range(5, 15) * Engine.DeltaTime;
                ValueTo(renderers, Calc.Random.Range(0.5f, 0.8f));
                EventInstance instance = Audio.Play("event:/PianoBoy/invertGlitch2");
                instance.setVolume(Calc.Random.Range(0.2f, 0.4f));
                instance.getPitch(out float pitch, out float finalpitch);
                instance.setPitch(pitch + 12);
                yield return Calc.Random.Range(4, 8) * Engine.DeltaTime;
                ValueTo(renderers, 0);
            }
            for (float i = 0; i < 1; i += Engine.DeltaTime / 3f)
            {
                Distort.Anxiety = Ease.CubeIn(i);
                float ease = Ease.CubeIn(i);
                foreach (MonumentRenderer renderer in manager.Renderers)
                {
                    renderer.Value = ease;
                }
                yield return null;
            }

            foreach (MonumentRenderer renderer in manager.Renderers)
            {
                renderer.Value = 1;
                renderer.Flash(1f, 2.4f);
            }

            Audio.Play("event:/PianoBoy/invertGlitch2");
            yield return 1;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 2.4f)
            {
                Distort.Anxiety = 1 - Ease.SineInOut(i);
                yield return null;
            }
            Distort.Anxiety = 0;
        }
        public static void EnableAll(Scene scene, FrequencyMonument monument)
        {
            foreach (MonumentComponent component in (scene as Level).Tracker.GetComponents<MonumentComponent>())
            {
                if (component.IDs.Contains(monument.MonumentComponentID) && !component.IsEnabled)
                {
                    component.Enable(true);
                }
            }
            if (scene.Tracker.GetEntity<MonumentManager>() is MonumentManager manager)
            {
                foreach (MonumentRenderer renderer in manager.Renderers)
                {
                    renderer.Value = 1;
                    renderer.FlashValue = 0;
                    renderer.Components.RemoveAll<Tween>();
                }
            }
            Distort.Anxiety = 0;
            monument.CompletedFlag.State = true;
        }

        public Dictionary<int, List<MonumentComponent>> ComponentsInScene = [];
        public List<MonumentRenderer> Renderers = [];
        public MonumentManager() : base()
        {
            TransitionListener l = new();
            l.OnOutBegin = () =>
            {
                ReloadRenderers(Scene);
            };

            Add(l);
            Tag |= Tags.Global | Tags.Persistent | Tags.TransitionUpdate;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            ReloadRenderers(scene);
        }
        public void ReloadRenderers(Scene scene)
        {
            foreach (var r in Renderers)
            {
                r.RemoveSelf();
            }
            Renderers.Clear();
            ComponentsInScene.Clear();
            foreach (MonumentComponent c in scene.Tracker.GetComponents<MonumentComponent>())
            {
                if (ComponentsInScene.TryGetValue(c.Entity.Depth, out List<MonumentComponent> value))
                {
                    value.Add(c);
                }
                else
                {
                    ComponentsInScene.Add(c.Entity.Depth, [c]);
                }
            }
            foreach (var pair in ComponentsInScene)
            {
                MonumentRenderer renderer = new MonumentRenderer() { Depth = pair.Key, MonumentComponents = pair.Value };
                Renderers.Add(renderer);
                scene.Add(renderer);
            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            foreach (var r in Renderers)
            {
                r.RemoveSelf();
            }
        }
    }
    public class MonumentRenderer : Entity
    {
        public List<MonumentComponent> MonumentComponents = [];
        public VirtualRenderTarget Target;
        public float Value = 0;
        public float FlashValue;
        public void Flash(float wait, float duration)
        {
            FlashValue = 1;
            Alarm.Set(this, wait, () =>
            {
                Tween.Set(this, Tween.TweenMode.Oneshot, duration, Ease.SineInOut, t =>
                {
                    FlashValue = 1 - t.Eased;
                }, t => FlashValue = 0);
            });
        }
        public MonumentRenderer() : base()
        {
            Tag |= Tags.TransitionUpdate | Tags.Persistent;
            Target = VirtualContent.CreateRenderTarget("MonumentRendererTarget", 320, 180);
            Add(new BeforeRenderHook(() =>
            {
                Target.SetAsTarget(true);
                Level level = Scene as Level;
                Camera cam = level.Camera;
                Vector2 pos = cam.Position;
                if (MonumentComponents.Count > 0)
                {
                    VirtualRenderTarget temp = GameplayBuffers.TempA;
                    temp.SetAsTarget(true);
                    Draw.SpriteBatch.StandardBegin();
                    foreach (MonumentComponent c in MonumentComponents)
                    {
                        c.Entity.Position -= pos;
                        c.Entity.Render();
                        c.Entity.Position += pos;
                    }
                    Draw.SpriteBatch.End();
                    Effect effect;
                    float alpha = 1;
                    if (FlashValue > 0)
                    {
                        effect = ShaderFX.WhiteOut;
                        effect.ApplyIdentityParameters(SceneAs<Level>(), FlashValue);
                    }
                    else
                    {
                        alpha = 0.5f + (1 - Value) * 0.2f;
                        effect = ShaderFX.Monument;
                        effect.ApplyIdentityParameters(SceneAs<Level>(), Value);
                    }
                    Target.SetAsTarget(true);
                    Draw.SpriteBatch.StandardBegin(Matrix.Identity, effect);
                    Draw.SpriteBatch.Draw(temp, Vector2.Zero, Color.White * alpha);
                    Draw.SpriteBatch.End();
                }
            }));
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Target?.Dispose();
        }
        public override void Render()
        {
            base.Render();
            Draw.SpriteBatch.Draw(Target, SceneAs<Level>().Camera.Position, Color.White);
        }
    }
}
