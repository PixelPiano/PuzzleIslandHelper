using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using System.Collections;
using Celeste.Mod.PuzzleIslandHelper.Effects;
using Celeste.Mod.PuzzleIslandHelper.Components;
using System.Linq;
using System;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{

    [CustomEntity("PuzzleIslandHelper/MiniGenerator")]
    [Tracked]
    public class MiniGenerator : Entity
    {
        public static bool FillsWithStool = true;
        public static float FillIncrement = 100f;
        public EntityID ID;
        public bool Activated;
        public bool Registered => PianoModule.Session.MiniGenStates.ContainsKey(ID);
        public bool RegistryState => Registered && PianoModule.Session.MiniGenStates[ID];
        public float FillAmount;
        public float FillSpeed;
        private float flashLerp;
        private float colorLerp;
        private float endFill = 16;
        private float speedTarget = -30f;
        private static Color StreakDefaultColor => Calc.HexToColor("203747");
        private static Color SymbolDefaultColor => Calc.HexToColor("020812");
        private static Color StreakTargetColor => Calc.HexToColor("3CFFF6");
        private static Color SymbolTargetColor => Calc.HexToColor("AA3CFF");
        public MTexture Machine = GFX.Game["objects/PuzzleIslandHelper/miniGenerator/machine"];
        private Coroutine flashCoroutine, colorCoroutine;
        public MiniGenerator(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
        {
            Depth = 1;
            ID = id;
            Collider = new Hitbox(Machine.Width, Machine.Height);
            Add(colorCoroutine = new Coroutine(false));
            Add(flashCoroutine = new Coroutine(false));
            if (FillsWithStool)
            {
                Add(new StoolListener(OnStoolRaised));
            }
            else
            {
                Add(new DashListener(OnDash));
            }
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (!Registered)
            {
                Register();
            }
            else if (RegistryState)
            {
                Activate(false);
            }
        }
        public override void Update()
        {
            base.Update();
            if (!Activated)
            {
                if (FillAmount != endFill)
                {
                    FillAmount = Calc.Clamp(FillAmount + FillSpeed * Engine.DeltaTime, 0, endFill);
                }
                FillSpeed = Calc.Approach(FillSpeed, speedTarget, Engine.DeltaTime * 30f) * 0.98f;
                if (FillAmount >= endFill)
                {
                    Activate(true);
                }
            }
            else
            {
                FillAmount = endFill;
                FillSpeed = speedTarget;
            }

        }
        public override void Render()
        {
            base.Render();
            Color streakColor = Color.Lerp(Color.Lerp(StreakDefaultColor, StreakTargetColor, colorLerp), Color.White, flashLerp);
            Color symbolColor = Color.Lerp(Color.Lerp(SymbolDefaultColor, SymbolTargetColor, colorLerp), Color.White, flashLerp);
            Draw.Rect(Position + new Vector2(19, 8), 2, 26, streakColor);
            Draw.Rect(Position + new Vector2(35, 8), 2, 25, streakColor);
            Draw.Rect(Position + new Vector2(25, 12), 7, 16, symbolColor);
            if (!Activated)
            {
                Draw.Rect(Position + new Vector2(25, 28 - FillAmount), 7, FillAmount, Color.White);
            }

            Draw.SpriteBatch.Draw(Machine.Texture.Texture_Safe, Position, Color.White);
        }
        private void OnDash(Vector2 dir)
        {
            if (dir == Vector2.UnitY && FillAmount < endFill && CollideCheck<Player>())
            {
                FillSpeed += FillIncrement;
            }
        }
        private void OnStoolRaised(Stool stool)
        {
            if (FillAmount < 1 && stool.DashesHeld > 1 && CollideCheck(stool))
            {
                FillSpeed += FillIncrement * (stool.DashesHeld - 1);
            }
        }
        public void Activate(bool flash)
        {
            Activated = true;
            UpdateRegistry(true); //set dictionary value to true
            colorCoroutine.Replace(ColorLerp());
            colorLerp = 0;
            flashLerp = 1;
            FillAmount = endFill;
            if (flash)
            {
                flashCoroutine.Replace(FlashRoutine());
            }
        }
        public void Deactivate()
        {
            if (!Registered)
            {
                Register();
            }
            Activated = false;
            flashCoroutine.Cancel();
            colorCoroutine.Cancel();
            FillAmount = 0;
            FillSpeed = -50f;
            colorLerp = 0;
            flashLerp = 0;
            UpdateRegistry(false);
        }
        public void Register()
        {
            if (!Registered)
            {
                PianoModule.Session.MiniGenStates.Add(ID, Activated);
            }
        }
        public void UpdateRegistry(bool state)
        {
            if (Registered)
            {
                PianoModule.Session.MiniGenStates[ID] = state;
            }
        }
        private IEnumerator FlashRoutine()
        {
            yield return 0.4f;
            yield return PianoUtils.Lerp(Ease.SineIn, 1, (f) => flashLerp = 1 - f, true);
        }
        private IEnumerator ColorLerp()
        {
            float duration = 3;
            yield return PianoUtils.Lerp(Ease.SineInOut, duration, f => colorLerp = f, true);
            while (Activated)
            {
                yield return PianoUtils.LerpYoyo(Ease.SineInOut, 0.5f, f => colorLerp = 1 - 0.3f * f, delegate { colorLerp = 0.7f; });
                yield return PianoUtils.Lerp(Ease.SineInOut, duration, f => colorLerp = 1 - 0.7f * f, true);
                yield return PianoUtils.Lerp(Ease.SineInOut, duration, f => colorLerp = 0.3f + 0.7f * f, true);
            }

        }
        [Command("change_fill_speed", "")]
        public static void ChangeFillSpeed(float speed = 15f)
        {
            FillIncrement = speed;
        }
        [Command("disable_minigen", "disables the nearest mini generator")]
        public static void DisableNearest()
        {
            if (Engine.Scene is Level level && level.GetPlayer() is Player player)
            {
                if (level.Tracker.GetNearestEntity<MiniGenerator>(player.Center) is var gen)
                {
                    gen.Deactivate();
                }
            }
        }
    }
}