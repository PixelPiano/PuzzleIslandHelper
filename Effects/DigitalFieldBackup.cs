using Celeste.Mod.Backdrops;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.PuzzleIslandHelper.Effects
{
    [CustomBackdrop("PuzzleIslandHelper/DigitalFieldBackup")]
    public class DigitalFieldBackup : Backdrop
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

        public DigitalFieldBackup(BinaryPacker.Element data) : base()
        {
            Spacing = new Vector2(data.AttrFloat("xSpacing"), data.AttrFloat("ySpacing"));
            BackColor = Calc.HexToColor(data.Attr("backColor"));
            FrontColor = Calc.HexToColor(data.Attr("frontColor"));
            Layers = data.AttrInt("layers", 4);
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
        public override void BeforeRender(Scene scene)
        {
            base.BeforeRender(scene);
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
        public override void Render(Scene scene)
        {
            base.Render(scene);
            for (int l = 0; l < Layers; l++)
            {
                float amount = (float)l / Layers;
                Color color = Color.Lerp(BackColor, FrontColor, amount);
                int size =(int)Calc.LerpClamp(MinSize, MaxSize, (float)l / Layers);
                for (float i = 0; i < 320; i += Spacing.X)
                {
                    for (float j = 0; j < 180; j += Spacing.Y)
                    {
                        Vector2 p = new Vector2(i, j);
                        float angle = GetAngle(p);
                        Vector2 offset = Calc.AngleToVector(angle, Calc.LerpClamp(MinDepth, MaxDepth, Equation(scene, p) * (1 - amount)));
                        Draw.Rect(p + offset - Vector2.One * (size / 2), size, size, color);
                    }
                }
            }
        }
    }
}

