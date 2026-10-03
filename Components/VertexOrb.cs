using Celeste.Mod.PuzzleIslandHelper.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using static Celeste.Mod.PuzzleIslandHelper.Entities.FlowTexture;
using static Celeste.Mod.PuzzleIslandHelper.Entities.Tower.Portal;

namespace Celeste.Mod.PuzzleIslandHelper.Components
{
    [Tracked]
    public class VertexOrb : GraphicsComponent
    {
        public const float WobbleRotationRate = (MathF.PI / 180f);
        public bool Wobble = true;
        public float WobbleMult = 1;
        private float wobbleOffset;
        public bool UseRawDeltaTime;
        public float Delta => UseRawDeltaTime ? Engine.RawDeltaTime : Engine.DeltaTime;
        public static BlendState SquashAlphaBlend;
        [OnLoad]
        public static void Load()
        {
            SquashAlphaBlend = new BlendState()
            {
                Name = "AfterImageBlendState",
                ColorSourceBlend = Blend.SourceAlpha,
                AlphaSourceBlend = Blend.One,
                ColorDestinationBlend = Blend.One,
                AlphaDestinationBlend = Blend.One,
                AlphaBlendFunction = BlendFunction.Max,
                ColorBlendFunction = BlendFunction.Max
            };
        }
        [OnUnload]
        public static void Unload()
        {
            SquashAlphaBlend?.Dispose();
            SquashAlphaBlend = null;
        }
        [Tracked]
        public class AfterImage : VertexOrb
        {
            public Vector2 Speed;
            private Vector2 speedVector;
            private VertexOrb From;
            private List<AfterImage> trackedList;
            private float fillAlphaFadeRate, edgeAlphaFadeRate;
            public float ScaleSpeed;
            public bool DependsOnVisibilityOfBaseCircle;
            private Vector2 origPosition;
            public float ScaleFriction;
            public AfterImage(VertexOrb from, float fillAlphaFadeRate, float edgeAlphaFadeRate, float scaleSpeed, List<AfterImage> trackedList = null) : base(from)
            {
                origPosition = from.RenderPosition;
                From = from;
                this.fillAlphaFadeRate = fillAlphaFadeRate;
                this.edgeAlphaFadeRate = edgeAlphaFadeRate;
                ScaleSpeed = scaleSpeed;
                this.trackedList = trackedList;

            }
            public override void Added(Entity entity)
            {
                base.Added(entity);
                if (trackedList != null)
                {
                    trackedList.Add(this);
                }
            }
            public override void Removed(Entity entity)
            {
                base.Removed(entity);
                if (trackedList != null)
                {
                    trackedList.Remove(this);
                }
            }
            public override void Update()
            {
                speedVector += Speed * Engine.DeltaTime;
                RenderPosition = origPosition + speedVector;
                base.Update();
                float delta = Delta;
                Scale = Vector2.Max(Scale + Vector2.One * ScaleSpeed * delta, Vector2.Zero);
                ScaleSpeed = Calc.Approach(ScaleSpeed, 0, ScaleFriction * delta);
                if (FillAlpha > 0)
                {
                    FillAlpha = Calc.Approach(FillAlpha, 0, fillAlphaFadeRate * delta);
                }
                if (EdgeAlpha > 0)
                {
                    EdgeAlpha = Calc.Approach(EdgeAlpha, 0, edgeAlphaFadeRate * delta);
                }
                if ((FillAlpha == 0 && EdgeAlpha == 0) || Scale == Vector2.Zero) RemoveSelf();
            }
            public override void DirectRenderVertices()
            {
                if (!DependsOnVisibilityOfBaseCircle || (From != null && From.Visible))
                {
                    var blend = Engine.Graphics.GraphicsDevice.BlendState;
                    Engine.Graphics.GraphicsDevice.BlendState = SquashAlphaBlend;
                    base.DirectRenderVertices();
                    Engine.Graphics.GraphicsDevice.BlendState = blend;
                }
            }
        }
        public enum VertexType
        {
            Fill,
            Edge,
            Center
        }
        public Vector2 RenderOffset;
        /// <summary>
        /// <para>VertexType - The type of vertex.</para>
        /// <para>int - The corner index of the vertex. The center point will always have an index of '0'. The corners are also indexed starting at '0'.</para>
        /// <para>float - The angle between the vertex and the center of the circle.</para>
        /// <para>Vector2 - The returned position offset of the vertex.</para>
        /// </summary>
        public Func<VertexType, int, float, Vector2> VertexOffset;
        public VertexPositionColor[] Vertices;
        public VertexPositionColor[] EdgeVertices;
        public float EdgeRadiusOffset;
        public float EdgeRotationOffset;
        public float FillRadiusOffset;
        public float FillRotationOffset;
        public BetterShaker Shaker;
        public Vector2 Shake;
        public Vector2 ShakeMult = Vector2.One;
        public int IgnoredLines
        {
            get => ignoredLines;
            set
            {
                int prev = ignoredLines;
                ignoredLines = Math.Clamp(value, 0, Corners);
                if (prev != ignoredLines)
                {
                    ReassignLineIndices();
                    UpdateVertices();
                }
            }
        }
        public int ignoredLines = 0;
        public int[] Indices;
        public int[] LineIndices;
        public int Corners
        {
            get => corners;
            set
            {
                int prev = corners;
                corners = value;
                if (prev != value && corners > 1) RecalculateVertices();
            }
        }
        private int corners;
        public float Radius;
        public float RotationRate;
        public float RotationRateMult = 1;
        public Color EdgeColor = Color.Black;
        public Color ColorB;
        public Color EdgeColorB;
        public Color? CenterColor;
        public Color? CenterColorB;
        public float FillColorLerp, EdgeColorLerp, CenterColorLerp;
        public float FillAlpha = 1;
        public float EdgeAlpha = 1;
        public float Alpha = 1;
        public float ogAlpha = 1;
        public bool DrawEdge = true;
        public bool DrawFill = true;
        public float CenterAlpha = 1;
        public float ogCenterAlpha;
        public bool TreatCenterAsFill = true;
        public bool ogTreatCenterAsFill = true;
        public bool Baked;
        public bool ResetOnRemoved;
        private int totalLines;
        private Vector2 ogPosition;
        private float ogRadius, ogEdgeRadiusOffset, ogFillRadiusOffset;
        private Color ogColor, ogEdgeColor, ogColorB, ogEdgeColorB;
        private int ogCorners;
        private bool ogDrawFill, ogDrawEdge;
        private float ogRotation, ogRotationRate, ogEdgeRotationOffset, ogFillRotationOffset;
        private int ogIgnoredLines;
        private float ogFillAlpha, ogEdgeAlpha;
        private float ogFillColorLerp, ogEdgeColorLerp, ogCenterColorLerp;
        private Color? ogCenterColor, ogCenterColorB;
        private Vector2 ogRenderOffset;
        private bool ogWobble;
        private float ogWobbleMult = 1;
        public Func<VertexType, int, float, Vector2> ogVertexOffset;
        public List<Func<Color, int, Color>> CustomGetFillColor = [];
        public List<Func<Color, int, Color>> CustomGetEdgeColor = [];
        public List<Func<Color, Color>> CustomGetCenterColor = [];
        public List<Func<Color, int, Color>> ogCustomGetFillColor = [];
        public List<Func<Color, int, Color>> ogCustomGetEdgeColor = [];
        public List<Func<Color, Color>> ogCustomGetCenterColor = [];

        public Matrix RotationMatrix;
        public float Yaw, Pitch, Roll;
        public float YawRate, PitchRate, RollRate;
        public VertexOrb(Vector2 position, float radius, int corners, Color color, Color edgeColor = default, float edgeRadiusOffset = 0) : base(true)
        {
            Position = position;
            Radius = radius;
            Corners = corners;
            Color = color;
            EdgeColor = edgeColor;
            EdgeRadiusOffset = edgeRadiusOffset;
            Shaker = new BetterShaker(OnShake);
        }
        public void ShakeFor(float time = -1)
        {
            Shaker.ShakeFor(time);
        }
        public void StopShaking()
        {
            Shaker.StopShaking();
        }
        public void OnShake(Vector2 amount)
        {
            Shake += amount;
        }
        public void CreateAfterImage(List<AfterImage> tracker, float fillMult = 1, float edgeMult = 1, float fillFadeRate = 1, float edgeFadeRate = 1, float scaleSpeed = 0, Vector2 speed = default)
        {
            AfterImage afterImage = new(this, fillFadeRate, edgeFadeRate, scaleSpeed, tracker);
            afterImage.FillAlpha *= fillMult;
            afterImage.EdgeAlpha *= edgeMult;
            afterImage.Speed = speed;
            Entity.Add(afterImage);
        }
        public VertexOrb(VertexOrb copyFrom, bool resetPosition = true) : this(resetPosition ? Vector2.Zero : copyFrom.Position, copyFrom.Radius, copyFrom.Corners, copyFrom.Color, copyFrom.EdgeColor, copyFrom.EdgeRadiusOffset)
        {
            Wobble = copyFrom.Wobble;
            Indices = new int[copyFrom.Indices.Length];
            Vertices = new VertexPositionColor[copyFrom.Vertices.Length];
            EdgeVertices = new VertexPositionColor[copyFrom.EdgeVertices.Length];
            Array.Copy(copyFrom.Indices, Indices, copyFrom.Indices.Length);
            Array.Copy(copyFrom.Vertices, Vertices, copyFrom.Vertices.Length);
            Array.Copy(copyFrom.EdgeVertices, EdgeVertices, copyFrom.EdgeVertices.Length);
            DrawFill = copyFrom.DrawFill;
            DrawEdge = copyFrom.DrawEdge;
            Rotation = copyFrom.Rotation;
            RotationRate = copyFrom.RotationRate;
            ignoredLines = copyFrom.ignoredLines;
            FillRadiusOffset = copyFrom.FillRadiusOffset;
            FillRotationOffset = copyFrom.FillRotationOffset;
            CenterColor = copyFrom.CenterColor;
            CenterColorB = copyFrom.CenterColorB;
            CenterColorLerp = copyFrom.CenterColorLerp;
            EdgeColorLerp = copyFrom.EdgeColorLerp;
            FillColorLerp = copyFrom.FillColorLerp;
            RenderOffset = copyFrom.RenderOffset;
            VertexOffset = copyFrom.VertexOffset;
            Alpha = copyFrom.Alpha;
            CenterAlpha = copyFrom.CenterAlpha;
            CustomGetCenterColor = copyFrom.CustomGetCenterColor;
            CustomGetEdgeColor = copyFrom.CustomGetEdgeColor;
            CustomGetFillColor = copyFrom.CustomGetFillColor;
            TreatCenterAsFill = copyFrom.TreatCenterAsFill;
            WobbleMult = copyFrom.WobbleMult;
            UseRawDeltaTime = copyFrom.UseRawDeltaTime;
        }
        public void JustifyOrigin(float x, float y)
        {
            Origin = new Vector2(x * Radius * 2, y * Radius * 2);
        }
        public IEnumerator GrowReveal(float duration, float delay, float radius, int? corners)
        {
            if (delay > 0) yield return delay;
            RotationRate = -2 * (MathF.PI / 180f);
            for (float i = 0; i < 1; i += Delta / duration)
            {
                float ease = Ease.QuintOut(i);
                Radius = Calc.LerpClamp(0, radius * 1.3f, ease);
                RotationRate = Calc.LerpClamp(-2 * (MathF.PI / 180f), 0, ease);
                if (corners.HasValue)
                {
                    Corners = (int)Calc.LerpClamp(0, corners.Value * 2, ease);
                }
                yield return null;
            }
            Radius = radius * 1.3f;
            RotationRate = 0;
            if (corners.HasValue) Corners = corners.Value * 2;
            yield return null;
            for (float i = 0; i < 1; i += Delta / duration)
            {
                float ease = Ease.SineInOut(i);
                Radius = Calc.LerpClamp(radius * 1.3f, radius, ease);
                RotationRate = (MathF.PI / 180f) * ease;
                yield return null;
            }
            Radius = radius;
            RotationRate = 0;//(MathF.PI / 180f);
            if (corners.HasValue) Corners = corners.Value;
        }
        private bool ogUseRawDeltaTime;
        public void Reset(bool visible = true)
        {
            Shivers.Clear();
            ColorMods.Clear();
            UseRawDeltaTime = ogUseRawDeltaTime;
            WobbleMult = ogWobbleMult;
            Wobble = ogWobble;
            Alpha = ogAlpha;
            CenterAlpha = ogCenterAlpha;
            CustomGetCenterColor = ogCustomGetCenterColor;
            CustomGetEdgeColor = ogCustomGetEdgeColor;
            CustomGetFillColor = ogCustomGetFillColor;
            TreatCenterAsFill = ogTreatCenterAsFill;
            Position = ogPosition;
            Radius = ogRadius;
            EdgeRadiusOffset = ogEdgeRadiusOffset;
            Color = ogColor;
            EdgeColor = ogEdgeColor;
            ColorB = ogColorB;
            EdgeColorB = ogEdgeColorB;
            CenterColor = ogCenterColor;
            CenterColorB = ogCenterColorB;
            FillColorLerp = ogFillColorLerp;
            EdgeColorLerp = ogEdgeColorLerp;
            CenterColorLerp = ogCenterColorLerp;
            DrawFill = ogDrawFill;
            DrawEdge = ogDrawEdge;
            Rotation = ogRotation;
            RotationRate = ogRotationRate;
            EdgeRotationOffset = ogEdgeRotationOffset;
            ignoredLines = ogIgnoredLines;
            FillAlpha = ogFillAlpha;
            EdgeAlpha = ogEdgeAlpha;
            corners = ogCorners;
            CenterColor = ogCenterColor;
            FillRotationOffset = ogFillRotationOffset;
            FillRadiusOffset = ogFillRadiusOffset;
            VertexOffset = ogVertexOffset;
            RenderOffset = ogRenderOffset;
            RecalculateVertices();
            Visible = visible;
        }
        public override void Added(Entity entity)
        {
            base.Added(entity);
            SaveAsBase();
            VertexOffset = GetShiverOffset;
        }
        public override void Removed(Entity entity)
        {
            base.Removed(entity);
            if (ResetOnRemoved)
            {
                Reset(false);
            }
        }
        public void SaveAsBase()
        {
            ogUseRawDeltaTime = UseRawDeltaTime;
            ogWobbleMult = WobbleMult;
            ogWobble = Wobble;
            ogAlpha = Alpha;
            ogCenterAlpha = CenterAlpha;
            ogCustomGetCenterColor = CustomGetCenterColor;
            ogCustomGetEdgeColor = CustomGetEdgeColor;
            ogCustomGetFillColor = CustomGetFillColor;
            ogTreatCenterAsFill = TreatCenterAsFill;
            ogPosition = Position;
            ogRadius = Radius;
            ogEdgeRadiusOffset = EdgeRadiusOffset;
            ogColor = Color;
            ogEdgeColor = EdgeColor;
            ogColorB = ColorB;
            ogEdgeColorB = EdgeColorB;
            ogFillColorLerp = FillColorLerp;
            ogEdgeColorLerp = EdgeColorLerp;
            ogCenterColorLerp = CenterColorLerp;
            ogCenterColor = CenterColor;
            ogCenterColorB = CenterColorB;
            ogCorners = Corners;
            ogDrawFill = DrawFill;
            ogDrawEdge = DrawEdge;
            ogRotation = Rotation;
            ogRotationRate = RotationRate;
            ogEdgeRotationOffset = EdgeRotationOffset;
            ogIgnoredLines = ignoredLines;
            ogFillAlpha = FillAlpha;
            ogEdgeAlpha = EdgeAlpha;
            ogFillRadiusOffset = FillRadiusOffset;
            ogFillRotationOffset = FillRotationOffset;
            ogVertexOffset = VertexOffset;
            ogRenderOffset = RenderOffset;
            RecalculateVertices();
        }
        private bool debug;
        public override void Update()
        {
            base.Update();
            debug = false;
            foreach (Shiver shiver in Shivers)
            {
                if (shiver.Active)
                {
                    shiver.Update();
                    debug = true;
                }
            }
            foreach (ColorMod mod in ColorMods)
            {
                if (mod.Active) mod.Update();
            }
            if (ShiversToRemove.Count > 0)
            {
                foreach (Shiver shiver in ShiversToRemove)
                {
                    Shivers.Remove(shiver);
                    shiver.Removed();
                }
                ShiversToRemove.Clear();
            }
            if (ColorModsToRemove.Count > 0)
            {
                foreach (ColorMod mod in ColorModsToRemove)
                {
                    ColorMods.Remove(mod);
                    mod.Removed();
                }
                ColorModsToRemove.Clear();
            }

            wobbleOffset = Wobble ? WobbleRotationRate * WobbleMult : 0;
            float rate = RotationRate;
            Rotation = (Rotation + rate) % MathHelper.TwoPi;
            if (AutoUpdateVertices)
            {
                UpdateVertices();
            }
        }
        public bool AutoUpdateVertices = true;
        public virtual Color GetEdgeColor(Color baseColor, int index)
        {
            if (CustomGetEdgeColor != null)
            {
                foreach (var item in CustomGetEdgeColor)
                {
                    baseColor = item.Invoke(baseColor, index);
                }
            }
            return baseColor * EdgeAlpha * Alpha;
        }
        public virtual Color GetFillColor(Color baseColor, int index)
        {
            if (CustomGetFillColor != null)
            {
                foreach (var item in CustomGetFillColor)
                {
                    baseColor = item.Invoke(baseColor, index);
                }
            }
            return baseColor * FillAlpha * Alpha;
        }
        public virtual Color GetCenterColor(Color baseColor)
        {
            if (CustomGetCenterColor != null)
            {
                foreach (var item in CustomGetCenterColor)
                {
                    baseColor = item.Invoke(baseColor);
                }
            }
            return baseColor * FillAlpha * Alpha;
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            if (debug)
            {
                Draw.Rect(Position, 32, 32, Color.Yellow);
            }
            if (Radius == 0 || Corners == 0 || Entity == null || !Entity.Collidable)
            {
                Draw.Circle(RenderPosition - Origin, Radius, Color.Red * 0.5f, 8);
            }
            else
            {
                Draw.Circle(RenderPosition - Origin, Radius, Color.Orange * 0.5f, 8);
            }
        }
        private Vector2 getVertexOffset(VertexType type, int index, float angle)
        {
            return VertexOffset != null ? VertexOffset(type, index, angle) - Origin : -Origin;
        }
        //DO NOT CHANGE ANYTHING ABOUT THE LOOP LOGIC
        //DO NOT CHANGE ANYTHING INVOLVING Corners OR IgnoredLines
        //PLEASE DON'T, JUST LET IT BE
        public void UpdateVertices() => UpdateVertices(RenderPosition + RenderOffset + Shake * ShakeMult, Rotation + wobbleOffset, Radius);
        public void OffsetVertices(Vector2 offset)
        {
            Vector3 offset3 = offset.ToVec3();
            for (int i = 0; i < Vertices.Length; i++)
            {
                Vertices[i].Position += offset3;
            }
            for (int i = 0; i < EdgeVertices.Length; i++)
            {
                EdgeVertices[i].Position += offset3;
            }

        }
        public void UpdateVertices(Vector2 renderPosition, float rotation, float radius)
        {
            if (Baked)
            {
                Matrix rotationMatrix = Matrix.CreateFromYawPitchRoll(Yaw, Pitch, Roll);
                Color fillColor = Color.Lerp(Color, ColorB, FillColorLerp);
                Color edgeColor = Color.Lerp(EdgeColor, EdgeColorB, EdgeColorLerp);
                Vector2 transformedOffset = (getVertexOffset(VertexType.Center, 0, 0) * Scale).Transform(0, rotationMatrix);
                Vertices[0].Position = (renderPosition + transformedOffset).ToVec3();
                if (CenterColor.HasValue)
                {
                    Color centerColor = Color.Lerp(CenterColor.Value, CenterColorB ?? Color.Transparent, CenterColorLerp);
                    Vertices[0].Color = GetCenterColor(centerColor);
                }
                else
                {
                    Vertices[0].Color = GetFillColor(fillColor, 0);
                }
                float angle = rotation;
                float angleStep = MathHelper.TwoPi / Corners;
                float edgeRadiusOffset = Math.Clamp(Math.Abs(radius), 0, 1) * EdgeRadiusOffset * Math.Sign(radius);
                float fillRadiusOffset = Math.Clamp(Math.Abs(radius), 0, 1) * FillRadiusOffset * Math.Sign(radius);
                for (int i = 0; i < Corners; i++, angle += angleStep)
                {
                    Vector2 angleOffsetA = Calc.AngleToVector(angle + FillRotationOffset, radius + fillRadiusOffset);
                    Vector2 angleOffsetB = Calc.AngleToVector(angle + EdgeRotationOffset, radius + edgeRadiusOffset);
                    Vector2 vertexOffsetA = getVertexOffset(VertexType.Fill, i, angle + FillRotationOffset);
                    Vector2 vertexOffsetB = getVertexOffset(VertexType.Edge, i, angle + EdgeRotationOffset);
                    Vector2 transformedOffsetA = ((vertexOffsetA + angleOffsetA) * Scale).Transform(0, rotationMatrix);
                    Vector2 transformedOffsetB = ((vertexOffsetB + angleOffsetB) * Scale).Transform(0, rotationMatrix);


                    Vertices[i + 1].Position = (renderPosition + transformedOffsetA).Round().ToVec3();
                    EdgeVertices[i].Position = (renderPosition + transformedOffsetB).Round().ToVec3();
                    Vertices[i + 1].Color = GetFillColor(fillColor, i + 1);
                    EdgeVertices[i].Color = GetEdgeColor(edgeColor, i);
                }
            }
        }
        public void ReassignLineIndices()
        {
            LineIndices = null;
            totalLines = 0;
            List<int> lineIndices = [];
            for (int i = 0; i < Corners - IgnoredLines; i++)
            {
                lineIndices.AddRange([i, (i + 1) % Corners]);
                totalLines++;
            }
            LineIndices = [.. lineIndices];
        }
        public void ReassignCircleIndices()
        {
            Indices = null;
            List<int> indices = [];

            for (int i = 0; i < Corners; i++)
            {
                if (i < Corners - 1) indices.AddRange([0, i + 1, i + 2]);
                else indices.AddRange([0, Corners, 1]);
            }
            Indices = [.. indices];
        }
        public void RecalculateVertices()
        {
            Baked = false;
            Vertices = new VertexPositionColor[Corners + 1];
            EdgeVertices = new VertexPositionColor[Corners];
            ReassignCircleIndices();
            ReassignLineIndices();
            Baked = true;
            UpdateVertices();
        }
        public virtual void DirectRenderOutlineVertices()
        {
            if (Radius + EdgeRadiusOffset != 0 && Corners - IgnoredLines > 0 && IgnoredLines < Corners)
            {
                Engine.Instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.LineList, EdgeVertices, 0, EdgeVertices.Length, LineIndices, 0, totalLines);
            }
        }
        public virtual void DirectRenderFillVertices()
        {
            if (Radius != 0 && Corners > 0)
            {
                Engine.Instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, Vertices, 0, Vertices.Length, Indices, 0, Corners);
            }
        }
        public virtual void DirectRenderVertices()
        {
            if (DrawFill) DirectRenderFillVertices();
            if (DrawEdge) DirectRenderOutlineVertices();
        }
        public virtual void RenderFillVertices(Matrix matrix)
        {
            GFX.DrawIndexedVertices(matrix, Vertices, Vertices.Length, Indices, Corners);
        }
        public virtual void RenderOutlineVertices(Matrix matrix)
        {
            PianoUtils.DrawUserPrimitives(PrimitiveType.LineStrip, matrix, EdgeVertices, Corners);
        }
        public virtual void RenderVertices()
        {
            if (Baked && Radius != 0 && Corners > 1 && (DrawFill || DrawEdge))
            {
                Matrix matrix = Entity.SceneAs<Level>().Camera.Matrix;
                if (DrawFill) RenderFillVertices(matrix);
                if (DrawEdge) RenderOutlineVertices(matrix);
            }
        }
        public float ShiverMult = 1;
        private Ease.Easer expoOutIn = Ease.Follow(Ease.ExpoOut, Ease.ExpoIn);
        public Vector2 GetShiverOffset(VertexType type, int index, float angle)
        {
            Vector2 offset = Vector2.Zero;
            if (type != VertexType.Center)
            {
                if (ShiverMult > 0)
                {
                    foreach (Shiver shiver in Shivers)
                    {
                        double diff = (angle - shiver.CurrentAngle + MathHelper.Pi) % MathHelper.TwoPi - MathHelper.Pi;
                        if (diff < -MathHelper.Pi) diff += MathHelper.TwoPi;
                        diff = Math.Abs(diff);
                        float lerp = 0;
                        float indexLerp = Math.Abs(index % 2 * 0.5f);
                        if (diff < shiver.Area / 2)
                        {
                            Ease.Easer ease = shiver.VertexEaser ?? Ease.Linear;
                            lerp = 1 - expoOutIn((float)diff / (shiver.Area / 2));
                        }
                        offset += ShiverMult * lerp * Calc.AngleToVector(angle, Calc.LerpClamp(shiver.CurrentMinIntensity, shiver.CurrentMaxIntensity, indexLerp));
                    }
                }
            }
            return offset;
        }
        #region Visual Manipulation
        public List<ColorMod> ColorMods = [];
        public List<Shiver> Shivers = [];
        public List<Shiver> ShiversToRemove = [];
        public List<ColorMod> ColorModsToRemove = [];

        public void RemoveAllInverts()
        {
            ColorMods.RemoveAll(t => t is InvertMod);
        }
        public void RemoveAllColorMods()
        {
            ColorMods.Clear();
        }
        public InvertMod Invert(float duration, int loops, bool fade, float invertAdjust = 0)
        {
            return InvertMod.Create(this, duration, loops, fade, invertAdjust);
        }
        public class InvertMod : ColorMod
        {
            public static InvertMod Create(VertexOrb orb, float duration, int loops, bool fade, float invertAdjust = 0)
            {
                InvertMod mod = new InvertMod(orb, duration, loops, fade, invertAdjust);
                orb.ColorMods.Add(mod);
                return mod;
            }
            public bool InvertActive;
            private float invertAdjust;
            public bool Fade;
            public InvertMod(VertexOrb parent, float duration, int loops, bool fade, float invertAdjust = 0) : base(parent, duration, Ease.Linear, duration, Ease.Linear, null, loops)
            {
                this.invertAdjust = invertAdjust;
                Fade = fade;
                GetColor mod = (c, i, f) => !InvertActive ? c : Fade ? Color.Lerp(c, c.Invert(), f) : c.Invert();
                CenterColorFunction = mod;
                FillColorFunction = mod;
                EdgeColorFunction = mod;
                OnComplete = (t) =>
                {
                    InvertActive = !InvertActive;
                    if (t.Reverse)
                    {
                        YoyoDuration += invertAdjust;
                    }
                    else
                    {
                        Duration += invertAdjust;
                    }
                };
            }
        }
        public ColorMod FlashColorMod(Color to, float flashDuration, float loopDelay, int flashes)
        {
            ColorMod mod = null;
            return mod = ColorMod.CreateYoyo(this, flashDuration, Ease.Linear, loopDelay, Ease.Linear, flashes, (c, i, f) =>
            {
                return mod.Tween.Reverse ? c : to;
            });
        }
        public ColorMod FlashColorMod(Color centerTo, Color fillTo, Color edgeTo, float flashDuration, float loopDelay, int flashes)
        {
            ColorMod mod = null;
            return mod = ColorMod.CreateYoyo(this, flashDuration, Ease.Linear, loopDelay, Ease.Linear,
                (c, i, f) =>
            {
                return mod.Tween.Reverse ? c : centerTo;
            },
                (c, i, f) =>
            {
                return mod.Tween.Reverse ? c : fillTo;
            },
                (c, i, f) =>
            {
                return mod.Tween.Reverse ? c : edgeTo;
            }, flashes);
        }
        public ColorMod FlashColorMod(float flashDuration, float loopDelay, int flashes, ColorMod.GetColor mod)
        {
            return ColorMod.CreateYoyo(this, flashDuration, Ease.Linear, loopDelay, Ease.Linear, flashes, mod);
        }
        public ColorMod FlashColorMod(float flashDuration, float loopDelay, int flashes, ColorMod.GetColor centerMod, ColorMod.GetColor fillMod, ColorMod.GetColor edgeMod)
        {
            return ColorMod.CreateYoyo(this, flashDuration, Ease.Linear, loopDelay, Ease.Linear, centerMod, fillMod, edgeMod, flashes);
        }
        public ColorMod PulseColorMod(float inDuration, float outDuration, Ease.Easer inEase, Ease.Easer outEase, int loops, ColorMod.GetColor mod)
        {
            return ColorMod.CreateYoyo(this, inDuration, inEase, outDuration, outEase, loops, mod);
        }
        public ColorMod PulseColorMod(float inDuration, float outDuration, Ease.Easer inEase, Ease.Easer outEase, int loops, ColorMod.GetColor centerMod, ColorMod.GetColor fillMod, ColorMod.GetColor edgeMod)
        {
            return ColorMod.CreateYoyo(this, inDuration, inEase, outDuration, outEase, centerMod, fillMod, edgeMod, loops);
        }
        public class ColorMod
        {
            public delegate Color GetColor(Color c, int i, float f);
            public static ColorMod CreateYoyo(VertexOrb entity, float duration, Ease.Easer ease, float yoyoDuration, Ease.Easer yoyoEase, GetColor center = null, GetColor fill = null, GetColor edge = null, int loops = 0)
            {
                ColorMod colorMod = new ColorMod(entity, duration, ease, yoyoDuration, yoyoEase, center, fill, edge, loops);
                entity.ColorMods.Add(colorMod);
                return colorMod;
            }
            public static ColorMod CreateYoyo(VertexOrb entity, float duration, Ease.Easer ease, float yoyoDuration, Ease.Easer yoyoEase, int loops, GetColor mod)
            {
                ColorMod colorMod = new ColorMod(entity, duration, ease, yoyoDuration, yoyoEase, mod, loops);
                entity.ColorMods.Add(colorMod);
                return colorMod;
            }
            public static ColorMod Create(VertexOrb entity, float duration, Ease.Easer ease, int loops, GetColor center = null, GetColor fill = null, GetColor edge = null)
            {
                ColorMod colorMod = new ColorMod(entity, duration, ease, center, fill, edge, loops);
                entity.ColorMods.Add(colorMod);
                return colorMod;
            }
            public static ColorMod Create(VertexOrb entity, float duration, Ease.Easer ease, int loops, GetColor mod)
            {
                ColorMod colorMod = new ColorMod(entity, duration, ease, mod, loops);
                entity.ColorMods.Add(colorMod);
                return colorMod;
            }
            public Tween Tween { get; private set; }
            private GetColor _c, _f, _e;
            public GetColor CenterColorFunction
            {
                set
                {
                    if (value != null) cFunc = (c) => value.Invoke(c, 0, this.value);
                    _c = value;
                }
                get => _c;
            }
            public GetColor FillColorFunction
            {
                set
                {
                    if (value != null) fFunc = (c, i) => value.Invoke(c, i, this.value);
                    _f = value;
                }
                get => _f;
            }
            public GetColor EdgeColorFunction
            {
                set
                {
                    if (value != null) eFunc = (c, i) => value.Invoke(c, i, this.value);
                    _e = value;
                }
                get => _e;
            }
            private Func<Color, Color> cFunc;
            private Func<Color, int, Color> fFunc;
            private Func<Color, int, Color> eFunc;
            public Action<Tween> OnComplete;
            private float value => Tween.Eased;
            public int Loops;
            public int Loop;
            private bool cAdded, fAdded, eAdded;
            public float Duration;
            public float YoyoDuration = -1;
            public Ease.Easer YoyoEaser;
            public Ease.Easer Easer;
            public VertexOrb Parent;
            public bool Active;
            public ColorMod(VertexOrb orb, float duration, Ease.Easer ease, float yoyoDuration, Ease.Easer yoyoEase, GetColor mod, int loops = 0) : this(orb, duration, ease, mod, mod, mod, yoyoDuration, yoyoEase, loops) { }
            public ColorMod(VertexOrb orb, float duration, Ease.Easer ease, GetColor mod, int loops = 0) : this(orb, duration, ease, mod, mod, mod, loops) { }
            public ColorMod(VertexOrb orb, float duration, Ease.Easer ease, float yoyoDuration, Ease.Easer yoyoEase, GetColor center = null, GetColor fill = null, GetColor edge = null, int loops = 0) : this(orb, duration, ease, center, fill, edge, yoyoDuration, yoyoEase, loops) { }
            public ColorMod(VertexOrb orb, float duration, Ease.Easer ease, GetColor center = null, GetColor fill = null, GetColor edge = null, int loops = 0) : this(orb, duration, ease, center, fill, edge, -1, null, loops) { }
            private ColorMod(VertexOrb orb, float duration, Ease.Easer ease, GetColor center = null, GetColor fill = null, GetColor edge = null, float yoyoDuration = -1, Ease.Easer yoyoEase = null, int loops = 0)
            {
                Parent = orb;
                Duration = duration;
                YoyoDuration = yoyoDuration;
                Loops = loops;
                bool yoyo = yoyoDuration > 0;
                if (yoyo) Loops *= 2;
                Easer = ease ?? Ease.Linear;
                YoyoEaser = yoyoEase ?? Easer;
                Tween = Tween.Create(yoyo ? Tween.TweenMode.YoyoLooping : Tween.TweenMode.Looping, Easer, Duration, false);
                Tween.OnComplete = (t) =>
                {
                    OnComplete?.Invoke(t);
                    if (yoyo)
                    {
                        if (t.Reverse)
                        {
                            Loop++;
                            t.Easer = Easer;
                            t.Duration = Duration;
                            if ((Loops > 0 && Loop > Loops) || Loops == 0) RemoveSelf();
                        }
                        else
                        {
                            t.Easer = YoyoEaser;
                            t.Duration = YoyoDuration;
                        }
                    }
                    else
                    {
                        Loop++;
                        if ((Loops > 0 && Loop > Loops) || Loops == 0) RemoveSelf();
                    }
                };
                CenterColorFunction = center;
                FillColorFunction = fill;
                EdgeColorFunction = edge;
                Start();
            }
            public void AddDelegates()
            {
                if (cFunc != null && !cAdded)
                {
                    Parent.CustomGetCenterColor.Add(cFunc);
                    cAdded = true;
                }
                if (fFunc != null && !fAdded)
                {
                    Parent.CustomGetFillColor.Add(fFunc);
                    fAdded = true;
                }
                if (eFunc != null && !eAdded)
                {
                    Parent.CustomGetEdgeColor.Add(eFunc);
                    eAdded = true;
                }
            }
            public void RemoveDelegates()
            {
                if (cFunc != null && cAdded)
                {
                    Parent.CustomGetCenterColor.Remove(cFunc);
                    cAdded = false;
                }
                if (fFunc != null && fAdded)
                {
                    Parent.CustomGetFillColor.Remove(fFunc);
                    fAdded = false;
                }
                if (eFunc != null && eAdded)
                {
                    Parent.CustomGetEdgeColor.Remove(eFunc);
                    eAdded = false;
                }
            }
            public void Start(bool addDelegates = true)
            {
                Active = true;
                Tween.Start();
                if (addDelegates) AddDelegates();
            }
            public void Stop(bool removeDelegates = false)
            {
                Active = false;
                Tween.Stop();
                if (removeDelegates) RemoveDelegates();
            }
            public void RemoveSelf()
            {
                Parent.ColorModsToRemove.Add(this);
            }
            public void Removed()
            {
                RemoveDelegates();
                Tween = null;
            }
            public void Update()
            {
                Tween?.Update();
            }
        }
        public class Shiver
        {
            public bool Active;
            public VertexOrb Parent;
            public Action<Shiver> OnEnd;
            public Action<Shiver> OnFadeOutEnd;
            public Coroutine IntensityRoutine = new Coroutine(false);
            public Coroutine Routine = new Coroutine(false);
            public float StartAngle;
            public float Duration;
            public float TargetMaxIntensity;
            public float TargetMinIntensity;
            public float CurrentAngle;
            public float CurrentMaxIntensity;
            public float CurrentMinIntensity;
            public float Area;
            public float IntensityMult;
            public float IntensityLerpDuration;
            public Ease.Easer IntensityLerpEase;
            public Ease.Easer VertexEaser;
            public Ease.Easer Easer;
            private Ease.Easer currentEase;
            public Ease.Easer AfterLoopEaser;
            public float Speed { private set; get; }
            public float Eased => currentEase(Value);
            public float Value;
            public bool StartFading;
            public Tween.TweenMode Mode;
            public bool Started;
            public Shiver(VertexOrb parent)
            {
                Parent = parent;
                IntensityLerpEase ??= Ease.SineInOut;
                Easer ??= Ease.Linear;
                currentEase = Easer;
            }
            public void Start(bool restart = true)
            {
                Started = true;
                Active = true;
                if (restart)
                {
                    IntensityRoutine.Replace(intensityFadeRoutine(IntensityLerpDuration, 0, 1));
                    Routine.Replace(routine());
                }
            }
            public void Stop()
            {
                Started = false;
                Active = false;
            }
            private IEnumerator routine()
            {
                bool repeat;
                bool reverse = false;
                while (true)
                {
                    for (float i = 0; i < 1; i += Parent.Delta / Duration)
                    {
                        Value = reverse ? 1 - i : i;
                        yield return null;
                    }
                    Value = reverse ? 0 : 1;
                    switch (Mode)
                    {
                        case Tween.TweenMode.Looping:
                            currentEase = AfterLoopEaser ?? Easer ?? Ease.Linear;
                            Value = 0;
                            repeat = true;
                            break;
                        case Tween.TweenMode.YoyoOneshot:
                            repeat = !reverse;
                            reverse = true;
                            break;
                        case Tween.TweenMode.YoyoLooping:
                            currentEase = AfterLoopEaser ?? Easer ?? Ease.Linear;
                            repeat = true;
                            reverse = !reverse;
                            break;
                        default:
                            repeat = false;
                            break;
                    }
                    if (!repeat)
                    {
                        Mode = Tween.TweenMode.Looping;
                        StartFading = true;
                        currentEase = AfterLoopEaser ?? Easer ?? Ease.Linear;
                        Value = 0;
                    }
                }
            }
            public void Update()
            {
                IntensityRoutine.Update();
                Routine.Update();
                CurrentAngle = (StartAngle + Eased * MathHelper.TwoPi) % MathHelper.TwoPi;
                CurrentMinIntensity = TargetMinIntensity * IntensityMult;
                CurrentMaxIntensity = TargetMaxIntensity * IntensityMult;
            }
            public void FadeOut()
            {
                StartFading = true;
            }
            private IEnumerator intensityFadeRoutine(float duration, float from, float to)
            {
                IntensityLerpEase ??= Ease.Linear;
                for (float i = 0; i < 1; i += Parent.Delta / duration)
                {
                    if (StartFading) break;
                    IntensityMult = Calc.LerpClamp(from, to, IntensityLerpEase(i));
                    yield return null;
                }
                if (!StartFading)
                {
                    IntensityMult = to;
                }
                to = IntensityMult;
                while (!StartFading)
                {
                    yield return null;
                }
                for (float i = 0; i < 1; i += Parent.Delta / duration)
                {
                    IntensityMult = Calc.LerpClamp(to, from, IntensityLerpEase(i));
                    yield return null;
                }
                RemoveSelf();
            }
            public void Removed()
            {
                IntensityRoutine = null;
                Routine = null;
            }
            public void RemoveSelf()
            {
                Parent.ShiversToRemove.Add(this);
            }

        }
        public Shiver AddShiver(float duration, float startAngle, float maxIntensity, float minIntensity, float angleArea, Ease.Easer ease, Tween.TweenMode tweenMode, float intensityLerpDuration = 0, Ease.Easer vertexEase = null, Ease.Easer intensityLerpEase = null, Action<Shiver> onEnd = null, Ease.Easer afterLoopEase = null)
        {
            Shiver shiver = new Shiver(this)
            {
                Duration = duration,
                OnEnd = onEnd,
                StartAngle = startAngle,
                CurrentAngle = startAngle,
                CurrentMaxIntensity = 0,
                CurrentMinIntensity = 0,
                TargetMaxIntensity = maxIntensity,
                TargetMinIntensity = minIntensity,
                Area = angleArea,
                VertexEaser = vertexEase,
                IntensityLerpDuration = intensityLerpDuration,
                Mode = tweenMode,
                Easer = ease,
                AfterLoopEaser = afterLoopEase
            };
            if (intensityLerpDuration <= 0) shiver.IntensityMult = 1;
            AddShiver(shiver);
            return shiver;
        }
        public void AddShiver(Shiver shiver, bool start = true)
        {
            Shivers.Add(shiver);
            if (start) shiver.Start();
        }
        public void RemoveShivers()
        {
            Shivers.Clear();
        }
        public void FadeShiver(Shiver shiver, float duration, bool removeOnComplete = true)
        {
            shiver.StartFading = true;
        }
        public void FadeAllShivers(float duration, bool removeOnComplete = true)
        {
            foreach (Shiver s in Shivers) FadeShiver(s, duration, removeOnComplete);
        }
        #endregion
    }
}