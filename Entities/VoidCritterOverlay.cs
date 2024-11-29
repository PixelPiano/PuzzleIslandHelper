using Celeste.Mod.Entities;
using Celeste.Mod.Meta;
using Celeste.Mod.PuzzleIslandHelper.Triggers;
using FrostHelper.ModIntegration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod.Utils;
using System.Collections;
using System.Collections.Generic;
using static MonoMod.InlineRT.MonoModRule;

// PuzzleIslandHelper.VoidCritters
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{

    [Tracked]
    public class VoidCritterOverlay : Entity
    {
        public float Amplitude;
        public bool RenderTest;
        public bool TransitioningIn;
        public bool Fading;
        public VoidCritters CrittersInLevel;
        public VirtualRenderTarget Target;
        public VoidCritterOverlay() : base()
        {
            Tag |= Tags.TransitionUpdate | Tags.Global;
            TransitionListener listener = new();
            listener.OnOut = (float f) =>
            {
                if (Fading)
                {
                    Amplitude = TransitioningIn ? f : 1 - f;
                }
            };
            Add(listener);
            Depth = -10000;
            Target = VirtualContent.CreateRenderTarget("voidCritterOverlayTarget", 320, 180);
            Add(new BeforeRenderHook(BeforeRender));

        }
        public void BeforeRender()
        {
            Target.SetAsTarget(Color.Black);
        }
        public override void Render()
        {
            base.Render();
            if (Scene is not Level level || Amplitude <= 0) return;
            Effect effect = ShaderHelperIntegration.GetEffect("PuzzleIslandHelper/Shaders/critterOverlay");
            if (effect != null)
            {
                Draw.SpriteBatch.End();
                effect.ApplyCameraParams(level);
                effect.Parameters["Amplitude"]?.SetValue(Ease.CubeOut(Amplitude));
                Draw.SpriteBatch.StandardBegin(level.Camera.Matrix,effect);
                Draw.SpriteBatch.Draw(Target, level.Camera.Position, Color.White);
                Draw.SpriteBatch.End();
                GameplayRenderer.Begin();
            }
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            CrittersInLevel = scene.Tracker.GetEntity<VoidCritters>();
            TransitioningIn = CrittersInLevel != null;
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Target?.Dispose();
            Target = null;
        }
        public override void Update()
        {
            base.Update();
            if (Scene is not Level level || level.GetPlayer() is not Player player || player.Dead) return;
            if (!level.Transitioning)
            {
                Fading = false;
                CrittersInLevel = Scene.Tracker.GetEntity<VoidCritters>();

                if (CrittersInLevel != null)
                {
                    TransitioningIn = CrittersInLevel.FlagState;
                    Amplitude = Calc.Approach(Amplitude, CrittersInLevel.FlagState ? 0 : 1, Engine.DeltaTime);
                }
                else
                {
                    TransitioningIn = false;
                    Amplitude = 0;
                }
            }
        }
        public void Transition(bool from, bool to)
        {
            if (from && !to)
            {
                Fading = true;
                TransitioningIn = false;
            }
            else if (to && !from)
            {
                Fading = true;
                TransitioningIn = true;
            }
            else
            {
                Fading = false;
            }
        }
        [OnLoad]
        public static void Load()
        {
            Everest.Events.Level.OnTransitionTo += Level_OnTransitionTo;
            Everest.Events.LevelLoader.OnLoadingThread += LevelLoader_OnLoadingThread;
        }

        private static void Level_OnTransitionTo(Level level, LevelData next, Vector2 direction)
        {
            VoidCritterOverlay overlay = level.Tracker.GetEntity<VoidCritterOverlay>();
            if (overlay != null)
            {
                VoidCritters thisCritters = level.Tracker.GetEntity<VoidCritters>();
                EntityData nextCritters = next.Entities.Find(item => item.Name == "PuzzleIslandHelper/VoidCritters");
                bool toHas = nextCritters != null;
                bool fromHas = thisCritters != null;
                bool fromFlag = fromHas && !VoidCritters.GetDisperseFlag(level, thisCritters);
                bool toFlag = toHas && !VoidCritters.GetDisperseFlag(level, nextCritters.Attr("flag"), nextCritters.Bool("inverted"));
                if (fromFlag != toFlag)
                {
                    overlay.Transition(fromFlag, toFlag);
                }
            }
        }

        private static void LevelLoader_OnLoadingThread(Level level)
        {
            level.Add(new VoidCritterOverlay());
        }

        [OnUnload]
        public static void Unload()
        {
            Everest.Events.Level.OnTransitionTo -= Level_OnTransitionTo;
            Everest.Events.LevelLoader.OnLoadingThread -= LevelLoader_OnLoadingThread;
        }
    }
}
