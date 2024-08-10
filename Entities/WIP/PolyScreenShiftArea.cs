using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;
using Celeste.Mod.PuzzleIslandHelper.Entities.Cutscenes.Prologue;
using Celeste.Mod.PuzzleIslandHelper.Components;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.WIP
{
    [CustomEntity("PuzzleIslandHelper/PolyScreenShiftArea")]
    [Tracked]
    public class PolyScreenShiftArea : ShiftArea
    {
        public UnbornHusk Husk;
        public bool Repaired;
        public bool Touched;
        private float lineOffset;
        private int expandLinesCount = 5;
        private const float LineRate = 1f;// 10f;
        public List<GlitchSquare> RepairSquares = new();
        public bool LinesOutsideBounds;
        public bool StartedRepairing;
        public List<int> OOB = new();
        public int Thickness => LineThickness;
        public PolygonScreen Screen;
        public PolyScreenShiftArea(EntityData data, Vector2 offset)
            : this(data.Position, offset, data.NodesWithPosition(Vector2.Zero))
        {
        }
        public PolyScreenShiftArea(Vector2 position, Vector2 offset, Vector2[] nodes) : base(position, offset, '0', '0', '0', '0', nodes, new int[] { 0, 1, 2 })
        {
            Alpha = 1;
            SetVertexAlpha(0);
            Glitchy = true;
            Collider = new Hitbox(8, 8);
            AreaDepth = -2;
        }
        public void Start()
        {
            Add(new Coroutine(FadeIn()));
        }
        private IEnumerator FadeIn()
        {
            float prevGlitch = GlitchAmount;
            float prevAmp = GlitchAmp;
            GlitchAmount = 0;
            GlitchAmp = 0;
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                SetVertexAlpha(Calc.LerpClamp(0, 0.2f, i));
                yield return null;
            }
            SetVertexAlpha(1);
            for (float i = 0; i < 0.4f; i += Engine.DeltaTime)
            {
                float lerp = Calc.Random.Range(0.7f, 1);
                GlitchAmount = Calc.LerpClamp(30, 60f, lerp);
                GlitchAmp = 1;
                yield return null;
            }
            GlitchAmount = prevGlitch;
            GlitchAmp = prevAmp;
        }
        private IEnumerator RepairRoutine()
        {
            StartedRepairing = true;
            float prevGlitch = GlitchAmount;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 0.8f)
            {
                GlitchAmount = Calc.LerpClamp(prevGlitch, 0, Ease.CubeIn(i));
                yield return null;
            }
            GlitchAmount = 0;
            float prevAmp = GlitchAmp;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 0.6f)
            {
                GlitchAmp = Calc.LerpClamp(prevAmp, 0, Ease.CubeIn(i));
                yield return null;
            }
            OnTouched();
            Touched = true;
            Glitchy = false;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (PianoUtils.SeekController<PolygonScreen>(scene) == null)
            {
                scene.Add(Screen = new PolygonScreen());
            }
            else
            {
                Screen = PianoUtils.SeekController<PolygonScreen>(scene);
            }
        }
        public override void Awake(Scene scene)
        {
            if (Screen.IndexedAreas > Screen.BgCache.Count)
            {
                RemoveSelf();
            }
            PolygonScreenIndex = Screen.IndexedAreas;
            Screen.IndexedAreas++;

            BgTo = Screen.BgCache[PolygonScreenIndex];

            base.Awake(scene);
            Husk = scene.Tracker.GetEntity<UnbornHusk>();
        }
        public override void Update()
        {
            base.Update();
            if (Scene is not Level level) return;
            if (Husk is null || Screen is null)
            {
                Husk = level.Tracker.GetEntity<UnbornHusk>();
                Screen = level.Tracker.GetEntity<PolygonScreen>();
                return;
            }
            if (Box.Collide(Husk) && Input.DashPressed)
            {
                if (!StartedRepairing)
                {
                    Add(new Coroutine(RepairRoutine()));
                }
            }
        }
        public override void AfterRender(Level level)
        {
            base.AfterRender(level);
            if (!Touched) return;
            lineOffset += LineRate;
            for (int i = 0; i < expandLinesCount; i++)
            {
                float offset = Calc.Max(lineOffset - i * 20, 0);
                float alpha = 1 - Calc.Clamp(offset / 150f, 0, 1);
                ExpandLinesOnScreen = alpha > 0;
                if (!ExpandLinesOnScreen) continue;
                Vector2 start = Vertices[^1].Position.XY();
                Vector2 center = Vertices.Center();
                float startAngle = Calc.Angle(center, start);
                start += Calc.AngleToVector(startAngle, offset);

                Vector2 camOffset = Position - level.LevelOffset - (level.Camera.Position - level.LevelOffset);
                foreach (VertexPositionColor v in Vertices)
                {
                    Vector2 vec = v.Position.XY();
                    float angle = Calc.Angle(center, vec);
                    Vector2 end = vec + Calc.AngleToVector(angle, offset);
                    start += camOffset; end += camOffset;
                    Draw.Line(start, end, Color.White * alpha, i + 1);
                    start -= camOffset; end -= camOffset;
                    start = end;
                }
            }
        }

    }
}
