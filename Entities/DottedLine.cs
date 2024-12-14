using Celeste.Mod.CommunalHelper;
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using ExtendedVariants.Variants;
using FrostHelper.ModIntegration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Monocle;
using System;
using System.Collections.Generic;
using VivHelper.Entities;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked]
    public class DottedLine : Entity
    {
        public Rectangle Bounds;
        public bool OnScreen;
        public List<FancyLineAngle> Lines = [];
        public FancyLine.ColorModes ColorMode;
        public float LineLength;
        public float ColorInterval;
        public float Spacing;
        public float MoveRate;
        public float Thickness;
        public Color Color1;
        public Color Color2;
        public Vector2 Start, End;
        public FancyLineAngle Building;
        public float Angle;
        public Entity Track;
        public Vector2 Offset;
        public float OffsetMult = 1;
        public VirtualRenderTarget Target;
        public DottedLine(EntityData data, Vector2 offset) : this(data.Position + offset, data.NodesOffset(offset)[0], data.Float("thickness"), data.Enum<FancyLine.ColorModes>("colodMode"), data.Float("lineLength"), data.Float("spacing"), data.Float("colorInterval"), data.HexColor("color1"), data.HexColor("color2"), data.Float("moveRate"))
        {
        }
        public DottedLine(Vector2 start, Vector2 end, float thickness, FancyLine.ColorModes colorMode, float lineLength, float spacing, float colorInterval, Color color, Color color2, float moveRate) : base()
        {
            Add(new BeforeRenderHook(BeforeRender));
            Bounds = new Rectangle();
            Bounds.X = (int)Math.Min(start.X - thickness, end.X - thickness);
            Bounds.Y = (int)Math.Min(start.Y - thickness, end.Y - thickness);
            Bounds.Width = (int)Math.Max(start.X + thickness, end.X + thickness) - Bounds.X;
            Bounds.Height = (int)Math.Max(start.Y + thickness, end.Y + thickness) - Bounds.Y;
            Position = start;
            Target = VirtualContent.CreateRenderTarget("akfhgkdjghks", 320, 180);
            MoveRate = moveRate;
            Angle = Calc.Angle(start, end);
            Thickness = thickness;
            Start = start;
            End = end;
            ColorMode = colorMode;
            LineLength = lineLength;
            Spacing = spacing;
            ColorInterval = colorInterval;
            Color1 = color;
            Color2 = color2;
            Tag |= Tags.TransitionUpdate;
            StaticMover a = new StaticMover();
            a.SolidChecker = CheckForSolid;
            Add(a);
        }

        private bool CheckForSolid(Solid solid)
        {
            return solid.CollideLine(Start, End);
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Target.Dispose();
        }
        public List<FancyLineAngle> toRemove = new();
        private float buildLength;
        public virtual bool IsOnScreen()
        {
            Rectangle bounds = SceneAs<Level>().Camera.GetBounds().Pad(8);
            return Collide.RectToLine(bounds, Start, End);
        }
        private void orig_Update()
        {
            OnScreen = IsOnScreen();
            if (!OnScreen) return;
            base.Update();
            if (MoveRate != 0)
            {
                foreach (FancyLineAngle line in Lines)
                {
                    line.MoveBy(Calc.AngleToVector(line.Angle, MoveRate * Engine.DeltaTime));
                }
                if (Building == null)
                {
                    Building = new FancyLineAngle(Start, 0, Angle, Color1, Thickness, Color2, ColorMode, ColorInterval);
                    Add(Building);
                }
                else
                {
                    if (Building.Length >= LineLength)
                    {
                        buildLength = 0;
                        FancyLineAngle copy = new FancyLineAngle(Start, LineLength, Angle, Color1, Thickness, Color2, ColorMode, ColorInterval);
                        copy.RandomizeOnAdd = false;
                        copy.ColorTimer = Building.ColorTimer;
                        Lines.Add(copy);
                        Add(copy);
                        Remove(Building);
                        Building = null;
                    }
                    else
                    {
                        buildLength = Calc.Approach(buildLength, LineLength + Spacing, MoveRate * Engine.DeltaTime);
                        Building.Length = Calc.Max(0, buildLength - Spacing);
                    }
                }
                if (Track != null)
                {
                    foreach (FancyLineAngle line in Lines)
                    {
                        line.Offset = Track.Position;
                    }
                }
                CleanUpLines();
                foreach (FancyLineAngle line in toRender)
                {
                    if (Calc.Random.Chance(0.01f))
                    {
                        float amount = Calc.Random.Range(-2f, 2f);
                        Vector2 offset = Calc.AngleToVector(line.Angle + MathHelper.PiOver2, amount);
                        line.Offset += offset;
                        Alarm.Set(this, Calc.Random.Range(3, 15) * Engine.DeltaTime, delegate { line.Offset -= offset; });
                    }
                }
            }
        }
        public List<FancyLineAngle> toRender = [];
        public virtual void CleanUpLines()
        {
            toRender.Clear();
            Rectangle level = SceneAs<Level>().Bounds;
            Rectangle cam = SceneAs<Level>().Camera.GetBounds();
            foreach (FancyLineAngle line in Lines)
            {
                Vector2 s = line.RenderStart, e = s + line.EndOffset;
                if (!Collide.RectToLine(cam, s, e))
                {
                    if (!Collide.RectToLine(level, s, e))
                    {
                        toRemove.Add(line);
                    }
                }
                else
                {
                    toRender.Add(line);
                }
            }
            if (toRemove.Count > 0)
            {
                foreach (FancyLineAngle line in toRemove)
                {
                    line.RemoveSelf();
                }
                toRemove.Clear();
            }
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Draw.Line(Start, End, Color.Red);
            Draw.LineAngle(Start, Angle, 50, Color.Green);
            Draw.HollowRect(Bounds, Color.Yellow);
        }
        private void BeforeRender()
        {
            Target.SetAsTarget(true);
        }
        public override void Render()
        {
            if (Scene is not Level level) return;
            Effect shader = ShaderHelperIntegration.TryGetEffect("PuzzleIslandHelper/Shaders/dottedLineShader");
            if (shader != null)
            {
                shader.ApplyParameters(level, level.Camera.Matrix);
                shader.Parameters["Pixel"]?.SetValue(1 / 180f);
                shader.Parameters["Start"]?.SetValue(Start);
                shader.Parameters["End"]?.SetValue(End);
                shader.Parameters["Length"]?.SetValue(LineLength);
                shader.Parameters["Space"]?.SetValue(Spacing);
                shader.Parameters["Color"]?.SetValue(Color1.ToVector4());
                shader.Parameters["Thickness"]?.SetValue(Thickness);
                shader.Parameters["Interval"]?.SetValue(ColorInterval);
                shader.Parameters["Rate"]?.SetValue(MoveRate);
                shader.Parameters["Offset"]?.SetValue(Offset);
                shader.Parameters["OffsetMult"]?.SetValue(OffsetMult);
            }
            Draw.SpriteBatch.End();
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap,
                DepthStencilState.None, RasterizerState.CullNone, shader, level.Camera.Matrix);
            Draw.SpriteBatch.Draw(Target, level.Camera.Position, Color.White);
            Draw.SpriteBatch.End();
            GameplayRenderer.Begin();
        }
    }
}