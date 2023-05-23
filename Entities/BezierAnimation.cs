using System;
using System.Collections.Generic;
using Monocle;
using Microsoft.Xna.Framework;
//Coded by XMinty7#1871 (Discord) :>

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    public class BezierAnimationBuilder
    {
        List<BezierAnimation.Part> Parts = new List<BezierAnimation.Part>();

        readonly int Sampling;
        readonly double Tolerancy;
        readonly double Urgence;
        public BezierAnimationBuilder(int sampling = BezierCurve.DEFAULT_SAMPLING, double tolerancy = BezierCurve.DEFAULT_TOLERANCY, double urgence = BezierCurve.DEFAULT_URGENCE)
        {
            Sampling = sampling;
            Tolerancy = tolerancy;
            Urgence = urgence;
        }

        public void Add(BezierCurve curve, float length = 1f)
        {
            Parts.Add(new BezierAnimation.Part(curve, length));
        }

        public void Add(Vector2 c1, Vector2 c2, float length = 1f)
        {
            Parts.Add(new BezierAnimation.Part(new BezierCurve(c1, c2, Sampling, Tolerancy, Urgence), length));
        }

        public void Add(float c1x, float c1y, float c2x, float c2y, float length = 1f)
        {
            Parts.Add(new BezierAnimation.Part(new BezierCurve(c1x, c1y, c2x, c2y, Sampling, Tolerancy, Urgence), length));
        }

        public BezierAnimation Build()
        {
            var res = new BezierAnimation(Parts);
            Parts = null;
            return res;
        }
    }

    public class BezierAnimation : Component
    {
        public struct Part
        {
            public readonly BezierCurve Curve;
            public readonly float Length;

            public Part(BezierCurve curve, float length = 1f)
            {
                Curve = curve;
                Length = length;
            }

            public static IEnumerable<Part> GetParts(IEnumerable<BezierCurve> curves)
            {
                foreach (var curve in curves) yield return new Part(curve);
            }
        }

        readonly List<Part> Parts;
        readonly List<float> PartLengths;
        readonly float Length;
        public BezierAnimation(params BezierCurve[] curves) : this(Part.GetParts(curves)) { }

        public BezierAnimation(IEnumerable<Part> parts) : base(false, false)
        {
            Parts = new List<Part>(parts);
            PartLengths = new List<float>(Parts.Count + 1);
            foreach (var part in parts)
            {
                PartLengths.Add(Length);
                Length += part.Length;
            }
            PartLengths.Add(Length);
            PartLengths.Add(Length);

            for (int i = 0; i < PartLengths.Count; i++) PartLengths[i] /= Length;
        }

        public BezierAnimation(params Part[] parts) : this((IEnumerable<Part>)parts) { }

        double time = 0f;
        double duration = 1d;

        public void Start(float duration)
        {
            this.duration = duration;
            Active = true;
            OnStart?.Invoke(this);
        }

        public Action<BezierAnimation> OnStart;
        public Action<BezierAnimation> OnUpdate;
        public Action<BezierAnimation> OnEnd;

        public override void Update()
        {
            base.Update();

            time += Engine.DeltaTime;
            float t = (float)(time / duration);

            if (t >= 1d)
            {
                Active = false;
                time = 0d;
                OnEnd?.Invoke(this);
                return;
            }

            int i = 0;
            for (; i < PartLengths.Count; i++)
            {
                if (PartLengths[i] > t)
                {
                    i--;
                    break;
                }
            }

            var start = PartLengths[i];
            var end = PartLengths[i + 1];
            var len = end - start;
            var ct = (t - start) / len;

            PassedTime = t;
            PassedTimeRelative = ct;

            CurrentPart = i;

            Value = Parts[i].Curve.Anim(ct);

            OnUpdate(this);
        }

        public float Value { get; private set; }
        public float PassedTime { get; private set; }
        public float PassedTimeRelative { get; private set; }
        public int CurrentPart { get; private set; }

        public float Animate(float start, float end)
        {
            return MathHelper.Lerp(start, end, Value);
        }

        public Vector2 Animate(Vector2 start, Vector2 end)
        {
            return new Vector2(Animate(start.X, end.X), Animate(start.Y, end.Y));
        }
    }
}
