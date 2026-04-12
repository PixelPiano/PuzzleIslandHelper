using Celeste.Mod.Entities;
using Celeste.Mod.Helpers;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.FrequencyEntities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/CodeWire")]
    public class CodeWire : Wire
    {
        public float At;
        public MTexture Texture;
        private float[] Rates;
        private GlobalFrequencyReceiver code;
        private Envelope envelope;
        private float topBound = 1;
        private float bottomBound = 0;
        private Color textureColor = Color.White;
        public CodeWire(EntityData data, Vector2 offset) : base(data, offset)
        {
            Color = data.HexColor("color", Calc.HexToColor("595866"));
            At = data.Float("percent");
            Rates = [data.Float("rateA"), data.Float("rateB"), data.Float("rateC"), data.Float("rateD")];
            code = new GlobalFrequencyReceiver(Rates);
            code.RequiresAudibleSound = true;
            code.StopAtFullPower = true;
            Add(code);
            code.OnFullPower = () =>
            {
                PianoModule.Session.CompletedFrequencyCodeIDs.Add("wire");
                envelope.Start();
            };
            envelope = new Envelope(1, 0.2f, 0.5f, 0, false, false, Envelope.Modes.Looping);
            envelope.OnUpdate = (e) =>
            {
                textureColor = Color.Lerp(Color.White, Color, Calc.LerpClamp(bottomBound, topBound, e.Eased));
            };
            envelope.OnStateChanged = (e) =>
            {
                if (e.State == Envelope.States.Sustain)
                {
                    bottomBound = bottomBound == 0.5f ? 0.3f : 0.5f;
                }
            };
            Add(envelope);
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Texture = GFX.Game["objects/PuzzleIslandHelper/wireNode"];
            if (PianoModule.Session.CompletedFrequencyCodeIDs.Contains("wire"))
            {
                code.Active = false;
                envelope.Start();
            }
        }
        public override void Update()
        {
            base.Update();
            if (PianoModule.Session.CompletedFrequencyCodeIDs.Contains("wire") && !envelope.Active)
            {
                envelope.Start();
            }
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            foreach (FrequencyCode codeEntity in scene.Tracker.GetEntities<FrequencyCode>())
            {
                if (codeEntity.CodeID == "wire")
                {
                    code.Active = false;
                    break;
                }
            }
        }
        public override void Render()
        {
            Level level = SceneAs<Level>();
            Vector2 vector = new Vector2((float)Math.Sin(sineX + level.WindSineTimer * 2f), (float)Math.Sin(sineY + level.WindSineTimer * 2.8f)) * 8f;
            float num = level.VisualWind / 100f;
            Vector2 vector2 = vector * num;
            Curve.Control = (Curve.Begin + Curve.End) / 2f + new Vector2(0f, 24f) + vector2;
            if (CullHelper.IsCurveVisible(Curve, 2f))
            {
                DrawCurve(-Vector2.UnitY, Curve, Color.Black, 16);
                DrawCurve(Vector2.UnitY, Curve, Color.Black, 16);
                DrawCurve(Vector2.Zero, Curve, Color, 16);
            }
            if (Texture != null && At >= 0)
            {
                float percent = (int)Math.Round(At / (1 / 16f)) * (1f / 16f);
                Texture.DrawCentered(Curve.GetPoint(percent), textureColor);
            }
        }
        public static void DrawCurve(Vector2 offset, SimpleCurve curve, Color color, int points)
        {
            Vector2 start = curve.Begin + offset;
            for (int i = 1; i <= points; i++)
            {
                float percent = (float)i / points;
                Vector2 point = curve.GetPoint(percent) + offset;
                Draw.Line(start, point, color);
                start = point;
            }
        }
    }
}