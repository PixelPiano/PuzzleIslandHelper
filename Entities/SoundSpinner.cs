using Celeste.Mod.Entities;
using ExtendedVariants.Entities.ForMappers;
using FrostHelper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/SoundSpinner")]
    [TrackedAs(typeof(CustomSpinner))]
    public class SoundSpinner : CustomSpinner
    {
        public string ShatterFlag;
        public bool Shattered;
        public Vector2 ShakeVector;
        public float ShakeAmount;
        private Vector2 origPosition;
        private SineWave wave;
        private int sign;
        private float offset;
        private float xAmount;
        private float MaxOffset = 4;
        private Color origColor;
        private Color FlashColor;
        private Coroutine flashRoutine;
        public float[] Amps;
        private float AmpTimer;
        private float ShakeAddAmount;
        public const float AmpRange = 0.1f;
        public const float UserAmpLimit = 0.5f;
        public static EntityData CreateData(EntityData orig)
        {
            if (orig.Values.ContainsKey("bloomAlpha"))
            {
                orig.Values["bloomAlpha"] = Calc.Random.Range(0, 1f);
            }
            if (orig.Values.ContainsKey("bloomRadius"))
            {
                orig.Values["bloomRadius"] = Calc.Random.Chance(0.7f) ? Calc.Random.Range(8f, 32f) : 0;
            }
            return orig;
        }
        public SoundSpinner(EntityData data, Vector2 offset) : base(CreateData(data), offset, false, "danger/PuzzleIslandHelper/icecrystal", data.Attr("tint", "639bff"), false, data.Attr("tint", "ffffff"))
        {
            origPosition = Position;
            ShatterFlag = "sound_spinner_shatter_flag_" + data.Int("attachGroup");
            wave = new SineWave(1, Calc.Random.Range(-1f, 1));
            Add(wave);
            Active = true;
            origColor = Tint;
            FlashColor = Tint;
            Amps = new float[4]
            {
                data.Int("freq1"),data.Int("freq2"),data.Int("freq3"),data.Int("freq4")
            };
            this.offset = Calc.Random.NextFloat();
            foreach (BloomPoint point in Components.GetAll<BloomPoint>())
            {
                point.Alpha = data.Float("bloomAlpha") - Calc.Random.Range(0, 0.3f);
            }
        }
        public void SetAmplitude()
        {
            float[] rates = PianoModule.Session.ForkAmpState.Rates;
            if(rates is null || rates.Length <= 0) return;
            int count = 0;
            float amount = 0;
            float maxRange = 15;
            for (int i = 0; i < 4; i++)
            {
                if (Amps[i] < 0) continue;
                float dist = MathHelper.Distance(rates[i], Amps[i]);
                amount += UserAmpLimit - (Calc.Clamp(dist, 0, maxRange) / maxRange) * UserAmpLimit;
                count++;
            }
            ShakeAmount = (count == 0 ? 0 : amount / count) + ShakeAddAmount;
        }
        private IEnumerator ColorFlash()
        {
            float time = 0.2f;
            yield return Calc.Random.Range(0, 0.5f);
            while (true)
            {
                float limit = 0.25f;
                for (float i = 0; i < 1; i += (Engine.DeltaTime / time) * ShakeAmount)
                {
                    FlashColor = Color.Lerp(origColor, Color.White, i * limit);
                    yield return null;
                }
                for (float i = 0; i < 1; i += (Engine.DeltaTime / time) * ShakeAmount)
                {
                    FlashColor = Color.Lerp(origColor, Color.White, limit - i * limit);
                    yield return null;
                }
                yield return null;
            }
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            flashRoutine = new Coroutine(ColorFlash());
            Add(flashRoutine);
            sign = Calc.Random.Choose(-1, 1);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if ((scene as Level).Session.GetFlag(ShatterFlag))
            {
                RemoveSelf();
            }
        }
        public override void Update()
        {
            base.Update();
            DoCycle();
            SetAmplitude();
            if (MathHelper.Distance(ShakeAmount - ShakeAddAmount, UserAmpLimit) <= AmpRange)
            {
                ShakeAddAmount = Math.Min(ShakeAddAmount + Engine.DeltaTime / 3, (1 - UserAmpLimit) * 2);
                if (ShakeAddAmount >= (1 - UserAmpLimit) * 2)
                {
                    AmpTimer += Engine.DeltaTime;
                    if (AmpTimer > 1)
                    {
                        AmpDestroy();
                    }
                }
            }
            else
            {
                ShakeAddAmount = Math.Max(ShakeAddAmount - Engine.DeltaTime, 0);
                AmpTimer = 0;
            }

            flashRoutine.Update();
            Tint = Color.Lerp(origColor, FlashColor, ShakeAmount);
            foreach (Image image in Components.GetAll<Image>())
            {
                image.Color = Tint;
            }
            wave.Frequency = ShakeAmount * MaxOffset;
            wave.Update();
            xAmount = wave.Value * sign * ShakeAmount;

            Position = origPosition + (Vector2.UnitX * xAmount);
        }
        private void DoCycle()
        {
            if (HasCollider && Scene.OnInterval(0.05f, offset))
            {
                Player player = Scene.GetPlayer();
                if (player != null)
                {
                    Collidable = Math.Abs(player.X - base.X) < 128f && Math.Abs(player.Y - base.Y) < 128f;
                }
            }
        }
        public void AmpDestroy()
        {
            Destroy();
            (Scene as Level).Session.SetFlag(ShatterFlag);
            (Scene as Level).Shake();
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
            Celeste.Freeze(0.01f);
        }
    }
}
