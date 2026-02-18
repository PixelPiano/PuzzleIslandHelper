using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.FrequencyEntities;
using Microsoft.Xna.Framework;
using Monocle;
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/CodeWire")]
    public class CodeWire : Wire
    {
        public float At;
        public MTexture Texture;
        private float[] Rates;
        private FrequencyCodeComponent code;
        private Envelope envelope;
        private float topBound = 1;
        private float bottomBound = 0;
        private Color textureColor = Color.White;
        public CodeWire(EntityData data, Vector2 offset) : base(data, offset)
        {
            Color = data.HexColor("color", Calc.HexToColor("595866"));
            At = data.Float("percent");
            Rates = [data.Float("rateA"), data.Float("rateB"), data.Float("rateC"), data.Float("rateD")];
            code = new FrequencyCodeComponent(Rates);
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
            if(PianoModule.Session.CompletedFrequencyCodeIDs.Contains("wire") && !envelope.Active)
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
            base.Render();
            if (Texture != null && At >= 0)
            {
                float percent = Calc.Snap(At, 1 / 16f);
                Texture.DrawCentered(Curve.GetPoint(percent), textureColor);
            }
        }
    }
}