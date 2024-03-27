using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
//Coded by XMinty7#1871 (Discord) :>

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    public class BezierCurve
    {
        public const int DEFAULT_SAMPLING = 60 * 100;
        public const double DEFAULT_TOLERANCY = 0.05d;
        public const double DEFAULT_URGENCE = 0.55d;

        double tolerate;
        Vector2 Control1, Control2;
        List<Vector2> Samples;

        // Sampling: Number of samples taken
        // Tolerancy:
        //   The ratio of tolerated distance error between samples to the ideal distance
        //   1 is full tolerancy - any value is accepted
        //   0 is no tolerancy - only the exact ideal sample value is accepted
        // Urgence:
        //   How fast will the sampler try to go from one sample to the next
        //   0 is infinitely thorough (never finishes)
        //   1 is very impatient (tries to make a single jump)
        public BezierCurve(Vector2 c1, Vector2 c2, int sampling = DEFAULT_SAMPLING, double tolerancy = DEFAULT_TOLERANCY, double urgence = DEFAULT_URGENCE)
        {
            Control1 = c1;
            Control2 = c2;

            // Prepare samples list
            Samples = new List<Vector2>(sampling);
            Samples.Add(Vector2.Zero);

            // Sampling state variables
            ulong count = 0;
            double t = 0d, x = 0d, cx = 0d;
            // Distance between each sample
            double size = 1d / sampling;
            // Tolerated distance
            double tolerate = tolerancy * size;
            this.tolerate = tolerate * 3d;
            while (true)
            {
                // Try to take evenly spaced samples
                count++;
                x = size * count;
                if (x >= 1f) break;

                // Too far from target sample X
                while (Math.Abs(x - cx) > tolerate)
                {
                    // Use the derivative to make an educated jump towards the next sample based on "urgence"
                    // Calculate next guess for t the acceleration parameter / x is always increasing
                    t += (x - cx) * urgence / dx(t);
                    // Calculate respective x value of t
                    cx = this.x(t) * Math.Sign(x - cx);
                }

                // Add the sample
                Samples.Add(new Vector2((float)x, (float)y(t)));
            }

            Samples.Add(Vector2.One);
        }

        public BezierCurve(float c1x, float c1y, float c2x, float c2y, int sampling = DEFAULT_SAMPLING, double tolerancy = DEFAULT_TOLERANCY, double urgence = DEFAULT_URGENCE) : this(new Vector2(c1x, c1y), new Vector2(c2x, c2y), sampling, tolerancy, urgence) { }

        public static BezierCurve Create(string preset, int sampling = DEFAULT_SAMPLING, double tolerancy = DEFAULT_TOLERANCY, double urgence = DEFAULT_URGENCE)
        {
            const int DEL_STR = 13;
            const int DEL_END = 1;
            preset = preset.Substring(0, preset.Length - DEL_END).Substring(DEL_STR);
            var strs = preset.Split(',');
            var factors = new float[strs.Length];
            for (int i = 0; i < strs.Length; i++) factors[i] = float.Parse(strs[i]);
            return new BezierCurve(factors[0], factors[1], factors[2], factors[3], sampling, tolerancy, urgence);
        }

        public double x(double t)
        {
            double tc = t * t * t;
            double ts = t * t;
            double f1 = 3 * tc - 6 * ts + 3 * t;
            double f2 = -3 * tc + 3 * ts;
            return Control1.X * f1 + Control2.X * f2 + tc;
        }

        public double y(double t)
        {
            double tc = t * t * t;
            double ts = t * t;
            double f1 = 3 * tc - 6 * ts + 3 * t;
            double f2 = -3 * tc + 3 * ts;
            return Control1.Y * f1 + Control2.Y * f2 + tc;
        }

        public Vector2 Point(double t)
        {
            return new Vector2((float)x(t), (float)y(t));
        }

        private double dx(double t)
        {
            double ts = t * t;
            double f1 = 9 * ts - 12 * t + 3;
            double f2 = -9 * ts + 6 * t;
            double f3 = 3 * ts;
            return Control1.X * f1 + Control2.X * f2 + f3;
        }

        private double dy(double t)
        {
            double ts = t * t;
            double f1 = 9 * ts - 12 * t + 3;
            double f2 = -9 * ts + 6 * t;
            double f3 = 3 * ts;
            return Control1.Y * f1 + Control2.Y * f2 + f3;
        }

        public Vector2 Derivative(double t)
        {
            return new Vector2((float)dx(t), (float)dy(t));
        }

        public float Anim(float t)
        {
            Vector2 closest = Vector2.Zero;
            float closestDist = t;
            int i = (int)Math.Max(0, Math.Floor((t - tolerate) * Samples.Count));
            for (; true; i++)
            {
                if (i >= Samples.Count)
                {
                    closest = Samples[Samples.Count - 1];
                    break;
                }
                var val = Samples[i];
                var dist = Math.Abs(t - val.X);
                if (dist > closestDist) break;
                closestDist = dist;
                closest = val;
            }
            return closest.Y;
        }
    }
}
