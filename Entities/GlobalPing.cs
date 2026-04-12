using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.FrequencyEntities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/Ping")]
    [Tracked]
    public class Ping : Entity
    {
        public bool ClampLeft, ClampRight, ClampUp, ClampDown;
        public FlagList Flag;
        public Color Color;
        public float Delay;
        public float Wait;
        public float Duration;
        public float BaseAlpha;
        public float BaseRadius, TargetRadius;
        public float TargetAlpha;
        public float Alpha;
        public string EventPath;
        private float radius;
        public Vector2 Target;
        private static Vector2[] points;
        private VertexPositionColor[] vertices;
        private static int[] indices;
        private const int PointsCount = 360;
        [OnLoad]
        public static void Load()
        {
            points = new Vector2[PointsCount];
            List<int> ind = [];
            points[0] = Calc.AngleToVector(0, 1);
            for (int i = 1; i < PointsCount; i++)
            {
                points[i] = Calc.AngleToVector(MathHelper.TwoPi / PointsCount * i, 1);
                ind.Add(i - 1);
                ind.Add(i);
                ind.Add(PointsCount + 1);
            }
            indices = [.. ind];
        }
        public Ping(Vector2 position, Color color, float baseRadius, float targetRadius, bool clampLeft, bool clampRight, bool clampUp, bool clampDown, float baseAlpha, float targetAlpha, float duration = 0.5f, float wait = 0, float delay = 0, FlagList flag = default, string eventPath = null) : base(position)
        {
            Color = color;
            ClampLeft = clampLeft;
            ClampRight = clampRight;
            ClampUp = clampUp;
            ClampDown = clampDown;
            EventPath = eventPath;
            Wait = wait;
            Duration = duration;
            Delay = delay;
            EventPath = eventPath;
            Flag = flag;
            Target = position;
            BaseAlpha = baseAlpha;
            TargetAlpha = targetAlpha;
            Alpha = BaseAlpha;
            radius = BaseRadius = baseRadius;
            TargetRadius = targetRadius;
        }
        public Ping(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.HexColor("color", Color.White), data.Float("baseRadius"), data.Float("targetRadius"),
                  data.Bool("clampLeft"), data.Bool("clampRight"), data.Bool("clampUp"), data.Bool("clampDown"),
                  data.Float("baseAlpha", 0), data.Float("targetAlpha", 1), data.Float("duration", 0.5f), data.Float("wait", 0), data.Float("delay", 0), data.FlagList("flag"), data.Attr("audioEvent"))
        {
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Add(new Coroutine(routine()));
            vertices = new VertexPositionColor[PointsCount + 1];
        }
        public void UpdateVertices(Vector2 position)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector2 pos = position + points[i] * radius;
                vertices[i].Position = new Vector3(pos, 0);
                vertices[i].Color = Color.Transparent;
            }
            vertices[^1].Position = new Vector3(position, 0);
            vertices[^1].Color = Color;
        }
        private IEnumerator routine()
        {
            if (Delay > 0) yield return Delay;
            while (true)
            {
                float inDuration = Duration * 0.2f;
                float outDuration = Duration * 0.8f;
                for (float i = 0; i < 1; i += Engine.DeltaTime / inDuration)
                {
                    float eased = Ease.CubeOut(i);
                    radius = Calc.LerpClamp(BaseRadius, TargetRadius, eased);
                    Alpha = Calc.LerpClamp(BaseAlpha, TargetAlpha, eased);
                    yield return null;
                }
                radius = TargetRadius;
                Alpha = TargetAlpha;
                for (float i = 0; i < 1; i += Engine.DeltaTime / outDuration)
                {
                    float eased = Ease.SineInOut(i);
                    radius = Calc.LerpClamp(TargetRadius, BaseRadius, eased);
                    Alpha = Calc.LerpClamp(TargetAlpha, BaseAlpha, eased);
                    yield return null;
                }
                radius = BaseRadius;
                Alpha = BaseAlpha;
                if (Wait > 0) yield return Wait;
            }
        }
        private const int pad = 16;
        public override void Update()
        {
            base.Update();
            Vector2 target = Position;
            Camera camera = SceneAs<Level>().Camera;
            if (ClampUp) target.Y = Math.Max(target.Y, camera.Top - pad);
            if (ClampDown) target.Y = Math.Min(target.Y, camera.Bottom + pad);
            if (ClampLeft) target.X = Math.Max(target.X, camera.Left - pad);
            if (ClampRight) target.X = Math.Min(target.X, camera.Right + pad);
            Target = target;
            UpdateVertices(Target);
        }
        public override void Render()
        {
            base.Render();
            GameplayRenderer.End();
            GFX.DrawIndexedVertices(Matrix.Identity, vertices, vertices.Length, indices, PointsCount);
            GameplayRenderer.Begin();
        }
    }
}
