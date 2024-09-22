using Celeste.Mod.Backdrops;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using FrostHelper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;

namespace Celeste.Mod.PuzzleIslandHelper.Effects
{
    [CustomBackdrop("PuzzleIslandHelper/DigitalField")]
    public class DigitalField : Backdrop
    {
        public Vector2 Spacing;
        public Color BackColor;
        public Color FrontColor;
        public int Layers;
        public float OffsetMult;
        public float Amplitude;
        public Vector2 PlayerPos;
        public bool FollowPlayer;
        public float MinDepth = 0;
        public float MaxDepth = 12;
        public int MinSize = 1;
        public int MaxSize = 4;
        public enum Presets
        {
            Sine,
            Cube,
            Player,
            Custom
        }
        public void ApplyParameters(Level level)
        {
            Shader.ApplyParameters(level, Matrix.Identity, Amplitude);
            Shader.Parameters["BackColor"]?.SetValue(BackColor.ToVector4());
            Shader.Parameters["FrontColor"]?.SetValue(FrontColor.ToVector4());
            Shader.Parameters["Layers"]?.SetValue(Layers);
            Shader.Parameters["MinSize"]?.SetValue(MinSize);
            Shader.Parameters["MaxSize"]?.SetValue(MaxSize);
            Shader.Parameters["MinDepth"]?.SetValue(MinDepth);
            Shader.Parameters["MaxDepth"]?.SetValue(MaxDepth);
            Shader.Parameters["Spacing"]?.SetValue(Spacing / new Vector2(320, 180));
        }
        private static VirtualRenderTarget _target;
        public static VirtualRenderTarget Target => _target ??= VirtualContent.CreateRenderTarget("digital_field", 320, 180);
        public Effect Shader;
        public DigitalField(BinaryPacker.Element data) : base()
        {
            Spacing = new Vector2(data.AttrFloat("xSpacing"), data.AttrFloat("ySpacing"));
            BackColor = Calc.HexToColor(data.Attr("backColor"));
            FrontColor = Calc.HexToColor(data.Attr("frontColor"));
            Layers = data.AttrInt("layers", 4);
            Shader = ShaderHelper.TryGetEffect("DigitalField");
        }
        
        public override void Ended(Scene scene)
        {
            base.Ended(scene);
            Shader?.Dispose();
            Shader = null;
            _target?.Dispose();
            _target = null;
        }
        public override void Update(Scene scene)
        {
            base.Update(scene);
            float sin = (float)(Math.Sin(scene.TimeActive) + 1) / 2f;
            Amplitude = Calc.LerpClamp(0.4f, 1, sin);
            if (scene.GetPlayer() is Player player)
            {
                PlayerPos = player.Center - (scene as Level).Camera.Position;
            }
        }

        public float Equation(Scene scene, Vector2 p)
        {
            return Eased(scene, Ease.BounceInOut, p.X, 4);
        }
        public float EaseToPlayer(Vector2 p, int size)
        {
            int total = size * size;
            return Calc.Max(Vector2.DistanceSquared(p, PlayerPos), total) / total;
        }
        public float Eased(Scene scene, Ease.Easer ease, float x, int waves)
        {
            return Eased(ease, ((x % waves) / 360f + scene.TimeActive) % 2);
        }
        public float Eased(Ease.Easer ease, float amount)
        {
            if (amount < 1)
            {
                return ease(amount);
            }
            else
            {
                return ease(1 - (amount - 1));
            }
        }
        public float GetAngle(Vector2 p)
        {
            return (p - new Vector2(160, 90)).Angle();
        }
        public override void BeforeRender(Scene scene)
        {
            base.BeforeRender(scene);
            Target.SetRenderTarget(Color.White);
        }
        public override void Render(Scene scene)
        {
            base.Render(scene);
            if (scene is not Level level || Shader is null) return;
            ApplyParameters(level);
            Draw.SpriteBatch.End();
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap,
                DepthStencilState.None, RasterizerState.CullNone, Shader, Matrix.Identity);
            Draw.SpriteBatch.Draw(Target, Vector2.Zero, Color.White);
            Draw.SpriteBatch.End();
            GameplayRenderer.Begin();
        }
    }
}

