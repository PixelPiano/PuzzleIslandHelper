using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora.Passengers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;


namespace Celeste.Mod.PuzzleIslandHelper.Entities.Cutscenes
{
    [Obsolete("Only used in a now scrapped interaction")]
    [TrackedAs(typeof(ShaderOverlay))]
    public class Aura : ShaderOverlay
    {
        public bool Strong;
        public float Random;
        private Player player;
        private bool forcedVisibleState;
        public Aura() : base("PuzzleIslandHelper/Shaders/glitchAura", "", true)
        {
        }
        public override void Update()
        {
            player = Scene.GetPlayer();
            if (player != null)
            {
                player.Visible = forcedVisibleState;
            }
            base.Update();
        }
        public override void BeforeApply()
        {
            base.BeforeApply();
            if (player is null) return;
            player.Visible = false;
        }
        public override void AfterApply()
        {
            base.AfterApply();
            if (player is null) return;
            Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.Level);
            GameplayRenderer.Begin();
            player.Render();
            Draw.SpriteBatch.End();
        }
        public override bool ShouldRender()
        {
            return Amplitude >= 0 && base.ShouldRender();
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            if (player != null)
            {
                player.Visible = true;
            }
        }

        public override void ApplyParameters(Level level)
        {
            base.ApplyParameters(level);
            Effect.Parameters["Strong"]?.SetValue(Strong);
            if (player != null)
            {
                Effect.Parameters["Center"]?.SetValue((player.Center - level.Camera.Position) / new Vector2(320, 180));
            }
            Effect.Parameters["Size"]?.SetValue(0.01f);
            Effect.Parameters["Random"]?.SetValue(Calc.Random.Range(1, 100f));
        }
    }
}
