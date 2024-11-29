using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using Celeste.Mod.PuzzleIslandHelper.Cutscenes;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.CustomCalidusEntities
{

    [Tracked]
    public class CalidusScane : Entity
    {
        public VirtualRenderTarget Buffer;
        public Effect Shader;
        public float Radius;
        public float Amplitude = 1;
        public CalidusScane(Vector2 position) : base(position)
        {
            Buffer = VirtualContent.CreateRenderTarget("calidusscantarget", 320, 180);
            Shader = ShaderHelper.TryGetEffect("CalidusGlow");
        }
        public void BeforeRender()
        {
            Buffer.SetAsTarget(Color.White);
        }
        public override void Render()
        {
            base.Render();
            if (Scene is not Level level) return;
            Shader.ApplyParameters(level, level.Camera.Matrix, Amplitude);
            Draw.SpriteBatch.End();
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap,
                DepthStencilState.None, RasterizerState.CullNone, Shader, level.Camera.Matrix);
            Draw.SpriteBatch.Draw(Buffer, level.Camera.Position, Color.White);
            Draw.SpriteBatch.End();
            GameplayRenderer.Begin();
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Buffer?.Dispose();
            Buffer = null;
            Shader?.Dispose();
            Shader = null;
        }
    }
}
