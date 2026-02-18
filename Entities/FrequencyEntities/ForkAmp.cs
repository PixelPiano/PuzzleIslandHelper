using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Components.Visualizers.DSPs;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.FrequencyEntities
{
    [Tracked]
    public class FrequencyComponent : Component
    {
        public Action<int, string> OnSelectedChanged;
        public Action<int, float, float, string> OnFrequencyChanged;
        public Action<int, bool, float, string> OnFrequencyStay;
        public Action<string> OnUIStart;
        public Action<bool, string> OnUIEnd;
        public FrequencyComponent() : base(false, false)
        {

        }
    }
    public static class FrequencyData
    {
        public const float Interval = 24;
        public const float Max = 96;
        public const float Min = 0;
        public static bool Enabled;
        public static bool PuzzleSolved(string id)
        {
            return string.IsNullOrEmpty(id) || id == "ovrd" || PianoModule.Session.CompletedFrequencyCodeIDs.Contains(id);
        }
        public static bool TryGetIndex(Scene scene, out int index)
        {
            if (scene == null || scene.Tracker == null)
            {
                index = -1;
                return false;
            }
            if (scene.Tracker.GetEntity<ForkAmpUI.UI>() is ForkAmpUI.UI ui)
            {
                index = ui.CurrentIndex;
                return true;
            }
            index = -1;
            return false;
        }
        public static bool RateChanged(Scene scene, int index)
        {
            return GetRate(scene, index) != GetPrevRate(scene, index);
        }
        public static float RateDiff(Scene scene, int index)
        {
            return GetRate(scene, index) - GetPrevRate(scene, index);
        }
        public static int GetUnlocked(Scene scene)
        {
            if (scene is not Level level)
            {
                throw new Exception("Current scene is not a level.");
            }
            return level.Session.GetCounter("ForkAmpUnlocks");
        }
        public static void SetUnlocked(Scene scene, int value, string id = null)
        {
            if (scene is not Level level)
            {
                throw new Exception("Current scene is not a level.");
            }
            level.Session.SetCounter(id ?? "ForkAmpUnlocks", value);
        }
        public static float[] GetRates(Scene scene, string id = null)
        {
            if (scene is not Level level)
            {
                throw new Exception("Current scene is not a level.");
            }
            float[] rates = new float[4];
            for (int i = 0; i < 4; i++)
            {
                rates[i] = level.Session.GetSlider((id ?? "") + "ForkAmpRate" + i);
            }
            return rates;
        }
        public static float[] GetPrevRates(Scene scene, string id = null)
        {
            if (scene is not Level level)
            {
                throw new Exception("Current scene is not a level.");
            }
            float[] rates = new float[4];
            for (int i = 0; i < 4; i++)
            {
                rates[i] = level.Session.GetSlider((id ?? "") + "PrevForkAmpRate" + i);
            }
            return rates;
        }
        public static float GetRate(Scene scene, int index, string id = null)
        {
            if (scene is not Level level)
            {
                throw new Exception("Current scene is not a level.");
            }
            return level.Session.GetSlider((id ?? "") + "ForkAmpRate" + index);
        }
        public static float GetPrevRate(Scene scene, int index, string id = null)
        {
            if (scene is not Level level)
            {
                throw new Exception("Current scene is not a level.");
            }
            return level.Session.GetSlider((id ?? "") + "PrevForkAmpRate" + index);
        }
        public static void SetRate(Scene scene, int index, float rate, string id = null)
        {
            if (scene is not Level level)
            {
                throw new Exception("Current scene is not a level.");
            }
            level.Session.SetSlider((id ?? "") + "PrevForkAmpRate" + index, GetRate(scene, index, id));
            level.Session.SetSlider((id ?? "") + "ForkAmpRate" + index, rate);
        }
    }
    [Tracked]
    public class FrequencyCodeComponent : Component
    {
        public bool Instant;
        public bool RequiresAudibleSound;
        public bool StopAtFullPower = true;
        public int MarginOfError = 0;
        public float ApproachMult = 1;
        public readonly float[] Rates;
        public float[] RatePower;
        public float Radius = -1;
        public float Duration = -1;
        public Action<float> OnStayInside;
        public Action OnExit;
        public Action OnEnter;
        public Action OnStayOutside;
        public Action<float, float> OnPowerChange;
        public Action OnFullPower;
        public Action<float> PowerUpdate;
        public bool InRange { get; private set; }
        private bool wasInRange;
        public float Power { get; private set; }
        private float prevPower;
        public FrequencyCodeComponent(params float[] rates) : base(true, false)
        {
            Rates = rates;
            RatePower = new float[rates.Length];
        }
        public static float CalculatePercent(Scene scene, float[] rates, float moe = 1)
        {
            float[] realRates = FrequencyData.GetRates(scene);
            int count = 0;
            int valid = 0;
            for (int i = 0; i < 4; i++)
            {
                float dist = MathHelper.Distance(rates[i], realRates[i]);
                if (dist <= moe)
                {
                    valid++;
                }
                count++;
            }
            if (count > 0)
            {
                return (float)valid / count;
            }
            return 0;
        }
        public override void Update()
        {
            base.Update();
            float mult = 1;
            if (Radius > 0)
            {
                if (Scene.GetPlayer() is Player player)
                {
                    float playerDist = Vector2.DistanceSquared(player.Center, Entity.Center);
                    mult = playerDist / (Radius * Radius);
                    bool prev = InRange;
                    InRange = playerDist < Radius * Radius;
                    if (wasInRange != InRange)
                    {
                        if (wasInRange) OnExit?.Invoke();
                        else OnEnter?.Invoke();
                    }
                    else
                    {
                        if (InRange) OnStayInside?.Invoke(mult);
                        else OnStayOutside?.Invoke();
                    }
                }
            }
            Power = 0;
            float[] rates = FrequencyData.GetRates(Scene);
            if (rates is null || rates.Length <= 0) return;
            if (RequiresAudibleSound && !ForkAmpSound.GlobalPlaying)
            {
                Power = 0;
            }
            else
            {
                int count = 0;
                for (int i = 0; i < 4; i++)
                {
                    if (Rates[i] < 0) continue;
                    float dist = MathHelper.Distance(rates[i], Rates[i]);
                    if (dist <= MarginOfError)
                    {
                        RatePower[i] = Instant ? 1 : Calc.Approach(RatePower[i], 1, Engine.DeltaTime * mult * ApproachMult);
                    }
                    else
                    {
                        RatePower[i] = Instant ? 0 : Calc.Approach(RatePower[i], 0, Engine.DeltaTime * mult * ApproachMult);
                    }
                    Power += RatePower[i];
                    count++;
                }
                if (count > 0)
                {
                    Power /= count;
                }
            }
            if (Power != prevPower)
            {
                OnPowerChange?.Invoke(prevPower, Power);
            }
            if (Power == 1)
            {
                if (StopAtFullPower)
                {
                    Active = false;
                }
                OnFullPower?.Invoke();
            }
            PowerUpdate?.Invoke(Power);
            wasInRange = InRange;
            prevPower = Power;
        }
    }

    [CustomEntity("PuzzleIslandHelper/ForkAmp")]
    [Tracked]
    public class ForkAmp : Entity
    {
        public Sprite Sprite;
        public DotX3 Talk;
        private FlagList[] flags;
        private ForkAmpUI ui;
        public ForkAmp(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Depth = 1;
            Sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/forkAmp/");
            Sprite.AddLoop("idle", "texture", 0.1f);
            Sprite.Play("idle");
            Add(Sprite);
            Collider = new Hitbox(Sprite.Width, Sprite.Height);
            Position -= new Vector2(Sprite.Width / 2, Sprite.Height / 2);
            Add(Talk = new DotX3(Collider, Interact));
            flags = new FlagList[4];
            flags[0] = new FlagList(data.Attr("firstFlag"));
            flags[1] = new FlagList(data.Attr("secondFlag"));
            flags[2] = new FlagList(data.Attr("thirdFlag"));
            flags[3] = new FlagList(data.Attr("fourthFlag"));
        }
        public void Interact(Player player)
        {
            Scene.Add(ui = new ForkAmpUI(flags));
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            ui?.RemoveSelf();
        }
    }

}