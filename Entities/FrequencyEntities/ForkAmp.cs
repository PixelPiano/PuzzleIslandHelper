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
    public static class FrequencyData
    {
        public static float[] TempleRates = [10, 20, 30, 40];
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
                rates[i] = GetRate(scene, i, id);
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
                rates[i] = GetPrevRate(scene, i, id);
            }
            return rates;
        }
        [Tracked]
        public class FrequencyMod : Component
        {
            public bool Enabled = true;
            public Func<float, int, float> ModRate;
            public bool[] OscillatorsAffected = [true, true, true, true];
            public FrequencyMod(Func<float, int, float> modRate) : base(true, false)
            {
                ModRate = modRate;
            }
        }
        public static float GetRate(Scene scene, int index, string id = null)
        {
            index = Math.Clamp(index, 0, 4);
            if (scene is not Level level)
            {
                throw new Exception("Current scene is not a level.");
            }
            return ApplyMods(scene, index, level.Session.GetSlider((id ?? "") + "ForkAmpRate" + index));
        }
        public static float ApplyMods(Scene scene, int index, float rate)
        {
            foreach (FrequencyMod mod in scene.Tracker.GetComponents<FrequencyMod>())
            {
                if (mod.Active && mod.OscillatorsAffected[index])
                {
                    rate = mod.ModRate.Invoke(rate, index);
                }
            }
            return rate;
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
            float prev = level.Session.GetSlider((id ?? "") + "ForkAmpRate" + index);
            level.Session.SetSlider((id ?? "") + "PrevForkAmpRate" + index, prev);
            level.Session.SetSlider((id ?? "") + "ForkAmpRate" + index, rate);
        }
    }
    [Tracked]
    public class FrequencySender : Component
    {
        public float[] Rates;
        public enum SendModes
        {
            Manual,
            Always,
            Interval
        }
        public SendModes SendMode;
        public float Interval;
        public FrequencySender(float radius, Vector2 position = default, params float[] rates) : base(true, false)
        {
            Rates = new float[4];
            if (rates != null)
            {
                for (int i = 0; i < 4 && i < rates.Length; i++)
                {
                    Rates[i] = rates[i];
                }
            }
        }
        public override void Update()
        {
            base.Update();

            switch (SendMode)
            {
                case SendModes.Always:
                    Send();
                    break;
                case SendModes.Interval:
                    if (Scene.OnInterval(Interval))
                    {
                        Send();
                    }
                    break;
            }
        }
        public static bool Collide(FrequencySender sender, FrequencyReceiver receiver)
        {
            return sender.Entity.CollideCheck(receiver.Entity);
            /*
                        Collider sPrev = sender.Entity.Collider;
                        Collider rPrev = receiver.Entity.Collider;
                        sender.Entity.Collider = sender.Collider ?? sender.Entity.Collider;
                        receiver.Entity.Collider = receiver.Collider ?? receiver.Entity.Collider;
                        bool colliding = sender.Entity.CollideCheck(receiver.Entity);
                        sender.Entity.Collider = sPrev;
                        receiver.Entity.Collider = rPrev;
                        return colliding;*/
        }
        public void Send()
        {
            foreach (FrequencyReceiver f in Scene.Tracker.GetComponents<FrequencyReceiver>())
            {
                if ((f.CustomCollideFunction != null && f.CustomCollideFunction.Invoke(this)) || Collide(this, f))
                {
                    f.ReceiveSignal(this);
                }
            }
        }
    }
    [Tracked(false)]
    public class FrequencyReceiver : Component
    {
        public int MarginOfError = 0;
        public float[] Rates;
        public bool Colliding { get; private set; }
        public bool wasColliding;
        public Func<FrequencySender, bool> CustomCollideFunction;
        public Action<FrequencySender> OnReceiveSignal;
        public FrequencyReceiver(params float[] rates) : base(true, false)
        {
            //Collider = new Hitbox(radius * 2, radius * 2, -radius, -radius);
            Rates = rates;
        }

        public override void Update()
        {
            base.Update();

            wasColliding = Colliding;
            Colliding = Entity.CollideCheck<Player>();
        }
        public static float CalculatePercent(float[] rates, float[] compare, float moe = 1)
        {
            int count = 0;
            int valid = 0;
            for (int i = 0; i < compare.Length; i++)
            {
                float a = compare[i];
                float b = i >= rates.Length ? 0 : rates[i];
                float dist = MathHelper.Distance(a, b);
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
        public virtual void ReceiveSignal(FrequencySender sender)
        {
            OnReceiveSignal?.Invoke(sender);
        }
    }
    [Tracked]
    public class GlobalFrequencyReceiver : FrequencyReceiver
    {
        public bool Instant;
        public bool RequiresAudibleSound;
        public bool StopAtFullPower = true;
        public bool StopAtZeroPower = false;
        public float ApproachMult = 1;
        public float ApproachUpMult = 1;
        public float ApproachDownMult = 1;
        public float[] RatePower;
        public float Duration = -1;
        public Action<float> OnStayInside;
        public Action OnExit;
        public Action OnEnter;
        public Action OnStayOutside;
        public Action<float, float> OnPowerChange;
        public Action OnFullPower;
        public Action OnZeroPower;
        public Action<float> PowerUpdate;
        public float Power { get; internal set; }
        private float prevPower;
        public float Delay;
        public bool Inside { get; private set; }
        public bool OnlyIfInside;
        public bool Disabled;
        public GlobalFrequencyReceiver(params float[] rates) : base(rates)
        {
            RatePower = new float[rates.Length];
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            if (Entity != null && Entity.Collider != null)
            {
                Draw.HollowRect(Entity.Collider, Colliding ? Color.Lime : Color.Red);
            }
        }
        public void Reset(bool on, bool start = false)
        {
            Active = start;
            float target = on ? 1 : 0;
            if (Power != target)
            {
                OnPowerChange?.Invoke(Power, target);
            }
            Power = target;
            if (on)
            {
                if (StopAtFullPower)
                {
                    Active = false;
                }
                FullPower();
            }
            else
            {
                if (StopAtZeroPower)
                {
                    Active = false;
                }
                ZeroPower();
            }
        }
        protected virtual void OnPlayerEnter(Player player)
        {
            OnEnter?.Invoke();
        }
        protected virtual void OnPlayerExit(Player player)
        {
            OnExit?.Invoke();
        }
        protected virtual void OnPlayerStayOutside(Player player)
        {
            OnStayOutside?.Invoke();
        }
        protected virtual void OnPlayerStayInside(Player player, float mult)
        {
            OnStayInside?.Invoke(mult);
        }
        public override void Update()
        {
            base.Update();
            if (Disabled) return;
            float mult = 1;
            /*            if (Collider != null || Entity.Collider != null)
                        {
                            if (Scene.GetPlayer() is Player player)
                            {
                                if (wasColliding != Colliding)
                                {
                                    if (wasColliding) OnPlayerExit(player);
                                    else OnPlayerEnter(player);
                                }
                                else
                                {
                                    if (Colliding) OnPlayerStayInside(player, 1);
                                    else OnPlayerStayOutside(player);
                                }
                            }
                        }
                        else
                        {*/
            //Inside = true;
            //}
            if (!OnlyIfInside || Colliding)
            {
                if (Delay > 0)
                {
                    Delay -= Engine.DeltaTime;
                }
                Power = 0;
                if (Delay <= 0 && (!RequiresAudibleSound || ForkAmpSound.GlobalPlaying))
                {
                    float[] rates = FrequencyData.GetRates(Scene);
                    if (rates is null || rates.Length <= 0) return;
                    int count = 0;
                    for (int i = 0; i < 4; i++)
                    {
                        if (Rates[i] < 0) continue;
                        float dist = MathHelper.Distance(rates[i], Rates[i]);
                        if (dist <= MarginOfError)
                        {
                            RatePower[i] = Instant ? 1 : Calc.Approach(RatePower[i], 1, Engine.DeltaTime * mult * ApproachUpMult * ApproachMult);
                        }
                        else
                        {
                            RatePower[i] = Instant ? 0 : Calc.Approach(RatePower[i], 0, Engine.DeltaTime * mult * ApproachDownMult * ApproachMult);
                        }
                        Power += RatePower[i];
                        count++;
                    }
                    if (count > 0)
                    {
                        Power /= count;
                    }
                }
            }
            else
            {
                Power = Calc.Approach(Power, 0, Engine.DeltaTime * ApproachDownMult * ApproachMult);
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
                FullPower();
            }
            else if (Power == 0)
            {
                if (StopAtZeroPower)
                {
                    Active = false;
                }
                ZeroPower();
            }
            PowerUpdate?.Invoke(Power);
            wasColliding = Colliding;
            prevPower = Power;
        }
        protected virtual void FullPower()
        {
            OnFullPower?.Invoke();
        }
        protected virtual void ZeroPower()
        {
            OnZeroPower?.Invoke();
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