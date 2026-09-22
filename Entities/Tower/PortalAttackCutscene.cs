using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.WIP;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static Celeste.Mod.PuzzleIslandHelper.Entities.Tower.Portal;
using static Celeste.MoonGlitchBackgroundTrigger;


namespace Celeste.Mod.PuzzleIslandHelper.Entities.Tower
{
    [Tracked]
    public class PortalAttackCutscene : CutsceneEntity
    {
        public float BlackFade, WhiteFade;
        public BetterShaker CombineCenterShaker;
        public Vector2 CombineCenterOffsetTarget;
        public Vector2 CombineCenterOffset;
        public float CombineCenterShakeMult = 0;
        public Vector2 CombineOffset => CombineCenterOffset * CombineCenterShakeMult;
        public Vector2 RedPosition, GreenPosition, BluePosition;
        public Portal Portal;
        public TimeRateModifier TimeRateMod;
        public RedMemoryOrb Red;
        public GreenMemoryOrb Green;
        public BlueMemoryOrb Blue;
        public TowerBlock Block;
        public bool FromTransit;
        public bool RedBrokeFree;
        private Coroutine timeRateCoroutine;
        private Coroutine whiteCoroutine;
        private Coroutine combinePushCoroutine;
        private Tween playerYSpeedTween;
        private Tween jostleTween;
        public const int IntermediateBlasts = 4;
        public bool InSlowDown;
        private Player Player;
        private float combinePush;
        public bool HoldPlayerOffScreen;
        public bool OrbsToCenter;
        public float OrbsToCenterApproach;
        private float innerDuration = 0.5f;
        private float outerDuration = 0.5f;
        private float auraRate;
        public bool FinalBlastCollided;
        private float redAura;
        public ThickCirclePulse Aura;
        private bool teleported;
        public PortalAttackCutscene(Portal portal, bool fromTransit) : base()
        {
            Portal = portal;
            FromTransit = fromTransit;
            Add(TimeRateMod = new TimeRateModifier(1));
            Add(timeRateCoroutine = new Coroutine(false));
            timeRateCoroutine.UseRawDeltaTime = true;
            Add(CombineCenterShaker = new BetterShaker(v =>
            {
                CombineCenterOffsetTarget += v;
            }));
            CombineCenterShaker.ShakeFor(-1);
/*            KeyComponent.ForKey(this, Microsoft.Xna.Framework.Input.Keys.S, () =>
            {
                ThickCirclePulse.TestCenter = !ThickCirclePulse.TestCenter;
            });*/
        }
        public override void OnBegin(Level level)
        {
            Red = Scene.Tracker.GetEntity<RedMemoryOrb>();
            Green = Scene.Tracker.GetEntity<GreenMemoryOrb>();
            Blue = Scene.Tracker.GetEntity<BlueMemoryOrb>();
            Block = Scene.Tracker.GetEntity<TowerBlock>();
            Add(new Coroutine(routine(Portal, Scene.GetPlayer())) { UseRawDeltaTime = true });
        }
        private IEnumerator routine(Portal portal, Player player)
        {

            ///bugs:
            ///portal capsules stay filled even after snapped to States.TwoOrbs
            ///AfterImagesRate could be set higher
            yield return redAuraGrow();
            Player = player;
            player.DisableMovement();
            player.DummyGravity = false;
            yield return new SwapImmediately(playerEntersPortal(Portal, player));

            yield return new SwapImmediately(teleportTo("", player, Portal));
            Portal.StateTransitioning = true;
            while (Portal.StateTransitioning)
            {
                yield return null;
            }
            playerYSpeedTween = Tween.Set(this, Tween.TweenMode.Oneshot, 1f, Ease.SineIn, t => Player.Speed.Y = t.Eased * 20);
            yield return 0.7f;
            Red.ShakeFor(1);
            Green.ShakeFor(1);
            Blue.ShakeFor(1);
            Red.ShakeMultTo(0.6f, 2, 1);
            Green.ShakeMultTo(0.6f, 2, 1);
            Blue.ShakeMultTo(0.6f, 2, 1);
            yield return 1;
            Red.State = MemoryOrb.StDummy;
            Green.State = MemoryOrb.StDummy;
            Blue.State = MemoryOrb.StDummy;
            Red.DummyHover = Green.DummyHover = Blue.DummyHover = false;
            RedPosition = Red.Position;
            GreenPosition = Green.Position;
            BluePosition = Blue.Position;
            OrbsToCenter = true;
            float dist = Vector2.Distance(Portal.Center, RedPosition) - Red.Orb.Radius / 2;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 0.1f)
            {
                yield return null;
                OrbsToCenterApproach = dist * i;
            }
            JostleCombineCenter(2, 0, 0.5f);
            Red.CombineShake(-1);
            Green.CombineShake(-1);
            Blue.CombineShake(-1);
            //todo: make burst effect
            float ringDelay = 0.1f;
            float from = 3;
            float to = 1;
            float time = 0.8f;
            float innerTo = 90;
            float outerTo = 90;
            float innerDelay = 0.1f;
            JostleCombineCenter(2, 0, 0.6f);
            screenFlashTo(1, 60f, 0.7f);
            CombineIntensityLerp(from, to, time);
            DoBlast(1, innerTo, outerTo, innerDelay, ringDelay, false, OnPulseCollideA);
            while (!player.OnGround()) yield return null; //--todo: make player hit ground routine
            Block.PulseShake(0.3f);
            yield return 1.2f;
            JostleCombineCenter(3, 0.5f, 0.7f);
            screenFlashTo(1, 60f, 0.7f);
            CombineIntensityLerp(from, to, time);
            DoBlast(1, innerTo, outerTo, innerDelay, ringDelay, true, OnPulseCollideB, OnSubPulseCollide);
            yield return 0.8f;
            JostleCombineCenter(3.5f, 1, 0.7f);
            screenFlashTo(1, 60f, 0.7f);
            CombineIntensityLerp(from, to, time);
            DoBlast(1, innerTo, outerTo, innerDelay, ringDelay, true, OnPulseCollideB, OnSubPulseCollide);
            yield return 0.7f;
            JostleCombineCenter(4, 1, 0.8f);
            screenFlashTo(1, 60f, 0.7f);
            CombineIntensityLerp(from, to, time);
            DoBlast(1, innerTo, outerTo, innerDelay, ringDelay, true, OnPulseCollideB, OnSubPulseCollide);
            yield return 0.6f;
            JostleCombineCenter(4, 1, 0.8f);
            screenFlashTo(1, 60f, 0.7f);
            CombineIntensityLerp(from, to, time);
            DoBlast(1, innerTo, outerTo, innerDelay, ringDelay, true, OnPulseCollideB, OnSubPulseCollide);
            yield return 1;
            JostleCombineCenter(6, 2, 2);
            screenFlashTo(1, 60f, 0.7f);
            CombineIntensityLerp(5, 3, time * 2);
            ThickCirclePulse[] pulses = new ThickCirclePulse[3];
            innerDuration = 0.5f;
            outerDuration = 0.5f;
            innerTo = 200;
            outerTo = 220;
            pulses[0] = DoBlast(3, innerTo, outerTo, innerDelay, 0.01f, true, OnPulseCollideC)[0];
            pulses[1] = DoBlast(1, innerTo - 3, innerTo - 1, innerDelay * 0.05f, 0.1f)[0];
            pulses[2] = DoBlast(1, innerTo, outerTo, innerDelay, 0.1f)[0];
            pulses[1].Alpha = 0.5f;
            pulses[2].Alpha = 0.5f;
            foreach (var p in pulses)
            {
                p.FadeOut = false;
                p.RemoveOnEnd = true;
            }
            HoldPlayerOffScreen = true;

            while (!FinalBlastCollided) yield return null;
            yield return 1;
            yield return redAuraGrow(); //todo: make this
            //red aura grows around orbs until they seperate in a flash
            //flow of time restored
            yield return redOrbBreakFree();
            TimeRateTo(1, 0.7f, 0, Ease.SineIn);
            //red sparkles from red orb orig towards direction the player went in
            //blue and green orbs shake a bit, then combine.
            //blue/green combo bash self into floor before zooming off in frenzy
            //fade to black
            yield return 0.8f;
            yield return redOrbFlyAfterPlayer();
            yield return 1;
            yield return blueAndGreenOrbsCombine();
            yield return combinedOrbsExit();
            yield return blackFadeTo(1, 1);
            Teleport();
            EndCutscene(Level);
        }
        public void Teleport()
        {
            BlackFade = 1;
            teleported = true;
            PianoUtils.InstantTeleportToMarker(Scene, "0-TowerBackend", "fallMarker", (l, p) =>
            {
                Scene.Add(new BackendTowerFallCutscene());
            });
        }
        public override void Update()
        {
            base.Update();
            if (HoldPlayerOffScreen && Scene.GetPlayer() is Player player && !player.OnScreen(8))
            {
                player.Speed.Y = 0;
                player.DummyGravity = false;
            }
            if (TimeRateMod.Multiplier < 1)
            {
                Red.RubberbandShake = !RedBrokeFree;
                Green.RubberbandShake = Blue.RubberbandShake = true;
                CombineCenterOffset = CombineCenterOffset.RubberbandApproach(CombineCenterOffsetTarget, 0.05f, 0.000000007764825821);
            }
            else
            {
                Green.RubberbandShake = false;
                Blue.RubberbandShake = false;
                Red.RubberbandShake = false;
                CombineCenterOffset = CombineCenterOffsetTarget;
            }
            Vector2 offset = CombineOffset;

            if (!RedBrokeFree) Red.RenderOffset = offset;
            else if (Red.RenderOffset != Vector2.Zero)
            {
                Red.RenderOffset *= 0.4f;
                if (Red.RenderOffset.Length() < 1) Red.RenderOffset = Vector2.Zero;
            }
            Green.RenderOffset = offset;
            Green.RenderOffset = offset;
            Blue.RenderOffset = offset;
            if (OrbsToCenter)
            {
                if (!RedBrokeFree)
                {
                    Red.Position = Calc.Approach(RedPosition, Portal.Center, OrbsToCenterApproach);
                }
                Green.Position = Calc.Approach(GreenPosition, Portal.Center, OrbsToCenterApproach);
                Blue.Position = Calc.Approach(BluePosition, Portal.Center, OrbsToCenterApproach);
            }
            if (Aura != null)
            {
                Aura.Position = Red.Position + Red.RenderOffset;
                redAura += auraRate * Engine.DeltaTime / 10f;
                float lerp = Math.Min(1, redAura);
                float add = (float)(Math.Sin(redAura - lerp) + 1) / 2f;
                Aura.OuterRadius = Red.Orb.Radius * (1 + (Ease.SineInOut(lerp) * 2.2f + add));
                Aura.Alpha = Ease.CubeInOut(lerp);

                Aura.UpdateVertices();
            }
        }
        public override void OnEnd(Level level)
        {
            PianoModule.Session.OrbsMerged = true;
            BlackFade = 1;
            if (WasSkipped)
            {
                ResetTimeRate();
                if (!teleported)
                {
                    Teleport();
                }
            }
            Red.AutoFade = true;
        }

        public void ResetTimeRate()
        {
            timeRateCoroutine.Cancel();
            TimeRateMod.Multiplier = 1;
        }
        public void TimeRateTo(float mult, float time, float delay, Ease.Easer ease)
        {
            timeRateCoroutine.Replace(timeRateToRoutine(mult, time, delay, ease));
        }
        private IEnumerator timeRateToRoutine(float to, float time, float delay, Ease.Easer ease)
        {
            if (delay > 0) yield return delay;
            ease ??= Ease.Linear;
            float from = TimeRateMod.Multiplier;
            for (float i = 0; i < 1; i += Engine.RawDeltaTime / time)
            {
                TimeRateMod.Multiplier = Calc.LerpClamp(from, to, ease(i));
                yield return null;
            }
        }
        public void OnPulseCollideA(ThickCirclePulse pulse, Player p)
        {
            p.Speed.Y = 0;
            playerYSpeedTween?.RemoveSelf();
            playerYSpeedTween = null;
            p.DummyAutoAnimate = false;
            p.Sprite.Play(PlayerSprite.Tired);
            if (p.Top < pulse.Y + pulse.OuterRadius && !p.OnGround() && !p.CollideCheck<Platform>(p.Position + Vector2.UnitY))
            {
                float v = (pulse.Y + pulse.OuterRadius) - p.Top;
                p.MoveV(v);
            }
        }
        public void OnPulseCollideB(ThickCirclePulse pulse, Player player)
        {
            Block.PulseShake(0.3f);
            //big shake crack
        }
        public void OnPulseCollideC(ThickCirclePulse pulse, Player player)
        {
            FinalBlastCollided = true;
            TimeRateTo(0, 0.6f, 0, Ease.ExpoOut);
            Block.Break();
            player.Speed.Y = 200f;
            player.Sprite.Play(PlayerSprite.Fall);
            player.DummyGravity = true;
        }
        public void OnSubPulseCollide(ThickCirclePulse pulse, Player player)
        {
            //small shake crack
        }
        private IEnumerator redAuraGrow()
        {
            Aura = new ThickCirclePulse(Vector2.Zero, Red.Orb.Radius, Red.Orb.Radius, (int)Red.Orb.Radius * 4);
            Aura.InnerColor = Color.Red;
            Aura.OuterAlpha = 0;
            Aura.AutoUpdateVertices = false;
            Aura.BlendState = BlendState.Additive;
            Aura.Alpha = 0;
            Aura.BetweenLerp = 0.65f;
            Scene.Add(Aura);
            for (float i = 0; i < 1; i += Engine.RawDeltaTime / 2)
            {
                auraRate = i * 20f;
                yield return null;
            }
            redAura = 20f;
        }
        private IEnumerator redOrbBreakFree()
        {
            //todo: come up with red orb break free visuals
            //make them themeatic or something
            timeRateCoroutine.Cancel();
            TimeRateMod.Multiplier = 1;
            RedBrokeFree = true;
            Red.RubberbandShake = false;
            Red.StopShaking();
            Red.StopCombineShake();
            Red.Speed.Y = -100f;
            Red.Friction.Y = 150f;
            Red.UseRawDeltaTime = true;
            while (Red.Speed.Y != 0) yield return null;
            Red.Friction.Y = 0;
        }
        private IEnumerator redOrbFlyAfterPlayer()
        {
            //todo: tweak movement
            Vector2 a = Portal.Center - Vector2.UnitX * 30f;
            Vector2 b = Block.TopCenter - Vector2.UnitY * Red.Orb.Radius * 2;
            Vector2 from = Red.Position;
            for (float i = 0; i < 1; i += Engine.RawDeltaTime / 0.6f)
            {
                Red.Position = Vector2.Lerp(from, a, Ease.SineIn(i));
                yield return null;
            }
            for (float i = 0; i < 1; i += Engine.RawDeltaTime / 0.6f)
            {
                Red.Position.X = Calc.LerpClamp(a.X, b.X, Ease.SineIn(i));
                Red.Position.Y = Calc.LerpClamp(a.Y, b.Y, Ease.SineInOut(i));
                yield return null;
            }
            yield return 0.5f;
            from = Red.Position;
            for (float i = 0; i < 1; i += Engine.RawDeltaTime / 0.5f)
            {
                Red.Position.Y = Calc.LerpClamp(from.Y, from.Y - 8, Ease.CubeOut(i));
                yield return null;
            }
            yield return 0.1f;
            Red.Speed.Y = 200f;
            Red.SpeedMult = 1;
            Red.Friction.Y = 0;
            yield return 1;
            Red.Speed.Y = 0;
            yield return null;
        }
        private IEnumerator blueAndGreenOrbsCombine()
        {
            Green.CombineIntensityTo(6, 0.6f);
            Blue.CombineIntensityTo(6, 0.6f);
            yield return 0.6f;
            Green.CombineIntensityTo(0, 1);
            Blue.CombineIntensityTo(0, 1);
            Green.ShakeMultTo(0, 1);
            Blue.ShakeMultTo(0, 1);
            OrbsToCenter = false;
            Vector2 fromGreen = Green.Position;
            Vector2 fromBlue = Blue.Position;
            float combineCenterShakeMultFrom = CombineCenterShakeMult;
            //orbs combine in center as screen fades to white
            for (float i = 0; i < 1; i += Engine.DeltaTime / 2)
            {
                CombineCenterShakeMult = combineCenterShakeMultFrom * (1 - i);
                Green.Position = Vector2.Lerp(fromGreen, Portal.Center, i);
                Blue.Position = Vector2.Lerp(fromBlue, Portal.Center, i);
                WhiteFade = Ease.Linear(i);
                yield return null;
            }
            CombineCenterShakeMult = 0;
            WhiteFade = 1;
            //replace green and blue orbs with boss
            //alternatively, leave this part up to interpretation (aka exit this routine and transition to falling sequence immediately before fading the screen back)
        }
        private IEnumerator combinedOrbsExit()
        {
            yield return null;
        }
        private IEnumerator blackFadeTo(float time, float to)
        {
            float from = BlackFade;
            for (float i = 0; i < 1; i += Engine.RawDeltaTime / time)
            {
                BlackFade = Calc.LerpClamp(from, to, i);
                yield return null;
            }
            BlackFade = to;
            yield return null;
        }
        private IEnumerator combineBlastFinal()
        {
            //fade in to maddy falling
            //red orb catches up and creates protective bubble around her just before she hits the ground
            //maddy breaks through several layers of ground before falling in a pool of water
            //moment maddy hits the water, act 3 title card appears
            //end cutscene
            yield return null;
        }
        private IEnumerator playerEntersPortal(Portal portal, Player player)
        {
            float target = (MathF.PI / 180) * 9f;
            while (player.Center != portal.Center || portal.RotationRate != target)
            {
                player.Center = Calc.Approach(player.Center, portal.Center, Engine.DeltaTime * 40);
                portal.RotationRate = Calc.Approach(portal.RotationRate, target, Engine.DeltaTime * 2);
                yield return null;
            }
        }
        private IEnumerator teleportTo(string room, Player player, Portal portal)
        {
            yield return null;
        }
        public void OnSpeedToPoint(float lerp)
        {
            if (InSlowDown)
            {
                TimeRateMod.Multiplier = Calc.Approach(TimeRateMod.Multiplier, 0.1f, Engine.RawDeltaTime * 2);
            }
        }
        public void PortalRotationRateTo(float value, float time)
        {
            float from = Portal.RotationRate;
            Tween.Set(this, Tween.TweenMode.Oneshot, time, Ease.SineInOut, t =>
            {
                Portal.RotationRate = Calc.LerpClamp(from, value, t.Eased);
            }, t => Portal.RotationRate = value);
        }
        #region Visual Effects
        public void SetCombineIntensity(float intensity)
        {
            Red.CombineIntensity = Green.CombineIntensity = Blue.CombineIntensity = intensity;
        }
        public void CombineIntensityLerp(float from, float to, float time)
        {
            SetCombineIntensity(from);
            if (!Red.Combining) Red.CombineShake(-1);
            if (!Green.Combining) Green.CombineShake(-1);
            if (!Blue.Combining) Blue.CombineShake(-1);
            Red.CombineIntensityTo(to, time);
            Green.CombineIntensityTo(to, time);
            Blue.CombineIntensityTo(to, time);
        }
        public void CombineShakeBurst(float from, float to, float time, Ease.Easer ease)
        {
            Red.CombineIntensityTo(from, to, time, ease);
            Green.CombineIntensityTo(from, to, time, ease);
            Blue.CombineIntensityTo(from, to, time, ease);
        }
        public void JostleCombineCenter(float multFrom, float multTo, float time)
        {
            jostleTween?.RemoveSelf();
            jostleTween = Tween.Set(this, Tween.TweenMode.Oneshot, time, Ease.CubeOut, t =>
            {
                CombineCenterShakeMult = Calc.LerpClamp(multFrom, multTo, t.Eased);
            }, t => CombineCenterShakeMult = multTo);
        }

        private void screenFlashTo(float alpha, float rateIn, float timeOut)
        {
            if (Settings.Instance.DisableFlashes) return;
            if (whiteCoroutine == null)
            {
                Add(whiteCoroutine = new Coroutine(false));
            }
            whiteCoroutine.Replace(flashRoutine(WhiteFade, alpha, rateIn, timeOut));
            float from = WhiteFade;
        }
        private IEnumerator flashRoutine(float from, float to, float rateIn, float timeOut)
        {
            WhiteFade = from;
            while (WhiteFade != to)
            {
                WhiteFade = Calc.Approach(WhiteFade, to, rateIn * Engine.RawDeltaTime);
                yield return null;
            }
            for (float i = 0; i < 1; i += Engine.RawDeltaTime / timeOut)
            {
                WhiteFade = Calc.LerpClamp(to, 0, Ease.QuintOut(i));
                yield return null;
            }
            WhiteFade = 0;
        }
        public void CombinePush(float amount, float timeIn, float timeOut = -1)
        {
            if (combinePushCoroutine == null)
            {
                Add(combinePushCoroutine = new Coroutine(false));
            }
            combinePushCoroutine.Replace(combinePushRoutine(amount, timeIn, timeOut));
        }
        private IEnumerator combinePushRoutine(float amount, float timeIn, float timeOut = -1)
        {
            float from = combinePush;
            for (float i = 0; i < 1; i += Engine.DeltaTime / timeIn)
            {
                combinePush = Calc.LerpClamp(from, amount, Ease.CubeOut(i));
                yield return null;
            }
            if (timeOut > 0)
            {
                for (float i = 0; i < 1; i += Engine.DeltaTime / timeOut)
                {
                    combinePush = Calc.LerpClamp(amount, from, Ease.SineIn(i));
                }
            }
            if (timeOut >= 0)
            {
                combinePush = from;
            }
        }
        public ThickCirclePulse[] DoBlast(int rings, float innerTo, float outerTo, float innerDelay, float ringDelay, bool collideOnce = true, Action<ThickCirclePulse, Player> onFirstPulseCollide = null, Action<ThickCirclePulse, Player> onSubPulseCollide = null)
        {
            return BlastRings(rings, ringDelay, innerTo, outerTo, innerDelay, true, true, collideOnce, onFirstPulseCollide, onSubPulseCollide);
        }
        public ThickCirclePulse[] BlastRings(int rings, float delay = 0.1f, float innerTo = 90, float outerTo = 90, float innerDelay = 0.1f, bool fadeOut = true, bool removeOnEnd = true, bool collideOnce = true, Action<ThickCirclePulse, Player> onFirstPulseCollide = null, Action<ThickCirclePulse, Player> onSubPulseCollide = null)
        {
            if (rings <= 0) return null;
            ThickCirclePulse[] array = new ThickCirclePulse[rings];
            Level.Shake(0.3f + (rings - 1) * delay);
            array[0] = BlastRingVisual(innerTo, outerTo, innerDelay, fadeOut, removeOnEnd, onFirstPulseCollide, 1, collideOnce);
            for (int i = 1; i < rings; i++)
            {
                float alpha = Math.Max(0.4f, 0.8f - (0.1f * i));
                array[i] = BlastRingVisual(innerTo, outerTo, innerDelay, fadeOut, removeOnEnd, onSubPulseCollide, alpha, collideOnce, i * delay);
            }
            return array;
        }

        public ThickCirclePulse BlastRingVisual(float innerTo, float outerTo, float innerDelay, bool fadeOut, bool removeOnEnd, Action<ThickCirclePulse, Player> onPulseCollide, float alpha = 1, bool collideOnce = true, float delay = 0)
        {
            var e = new ThickCirclePulse(Portal.Center, 0, 0, (int)outerTo, onPulseCollide, collideOnce, delay);
            e.RemoveOnEnd = removeOnEnd;
            e.FadeOut = fadeOut;
            e.Alpha = alpha;
            Scene.Add(e);
            e.StartOuter(outerTo, outerDuration, 0f, 0.5f, Ease.CubeOut);
            e.StartInner(innerTo, innerDuration, innerDelay, 0.5f, Ease.CubeOut);
            return e;
        }
        #endregion
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Aura?.RemoveSelf();
        }
        public class ThickCirclePulse : Entity
        {
            public const int HitboxExtend = 8;
            public VertexPositionColor[] Vertices;
            private int[] indices;
            private int[] indicesWithCenter;
            public Circle Circle;
            public bool OnlyCollideOnce;
            private bool collided;
            private Action<ThickCirclePulse, Player> collideCallback;
            public float OuterRadius;
            public float InnerRadius;
            public float OuterAlpha = 1;
            public float InnerAlpha = 1;
            public float BetweenLerp;
            public float BetweenAlpha
            {
                get => MergeBetweenAndInnerColor ? InnerAlpha : betweenAlpha;
                set
                {
                    if (MergeBetweenAndInnerColor) InnerAlpha = value;
                    else betweenAlpha = value;
                }
            }
            private float betweenAlpha = 1;
            private int corners;
            public float Alpha = 1;
            public bool UseCenter;
            public Vector2 RenderOffset;
            public Func<Effect> GetEffect;
            public Func<EffectParameterCollection, EffectParameterCollection> AdjustParameters;

            private float delayTimer;
            public Color InnerColor = Color.White;
            public Color OuterColor = Color.White;
            public Color BetweenColor
            {
                get => MergeBetweenAndInnerColor ? InnerColor : betweenColor;
                set
                {
                    if (MergeBetweenAndInnerColor) InnerColor = value;
                    else betweenColor = value;
                }
            }
            private Color betweenColor = Color.White;
            public bool MergeBetweenAndInnerColor = true;
            public Entity Track;
            public Effect Effect;
            public BlendState BlendState;
            public bool DisposeEffectOnRemoved = true;
            public bool AutoUpdateVertices = true;
            public static bool TestCenter = false;
            public ThickCirclePulse(Vector2 position, float innerRadius, float outerRadius, int corners, Action<ThickCirclePulse, Player> onCollide = null, bool onlyCollideOnce = false, float delay = 0) : base(position)
            {
                InnerRadius = innerRadius;
                OuterRadius = outerRadius;
                delayTimer = delay;
                Add(new PlayerCollider(p =>
                {
                    if (!OnlyCollideOnce || !collided)
                    {
                        collideCallback?.Invoke(this, p);
                        collided = true;
                    }
                }));
                Add(innerCoroutine = new(false));
                Add(outerCoroutine = new(false));
                Depth = 1;
                OnlyCollideOnce = onlyCollideOnce;
                Collider = Circle = new Circle(outerRadius + HitboxExtend);
                collideCallback = onCollide;
                this.corners = corners;
                Vertices = new VertexPositionColor[corners * 3 + 1];
                int[] ring1Indices = RingIndices(corners, 0);
                int[] ring2Indices = RingIndices(corners, corners);
                int[] centerIndices = CircleIndices(corners, corners * 2, Vertices.Length - 1);
                indices = new int[ring1Indices.Length + ring2Indices.Length];
                indicesWithCenter = new int[ring1Indices.Length + ring2Indices.Length + centerIndices.Length];
                ring1Indices.CopyTo(indices, 0);
                ring2Indices.CopyTo(indices, ring1Indices.Length);

                ring1Indices.CopyTo(indicesWithCenter, 0);
                ring2Indices.CopyTo(indicesWithCenter, ring1Indices.Length);
                centerIndices.CopyTo(indicesWithCenter, ring1Indices.Length + ring2Indices.Length);
            }
            public static int[] CircleIndices(int corners, int firstCornerIndex, int centerIndex)
            {
                int[] array = new int[corners * 3];
                int ind = 0;
                for (int i = 0; i < corners - 1; i++)
                {
                    array[ind++] = centerIndex;
                    array[ind++] = i + firstCornerIndex;
                    array[ind++] = i + 1 + firstCornerIndex;
                }
                array[ind++] = centerIndex;
                array[ind++] = corners - 1 + firstCornerIndex;
                array[ind++] = firstCornerIndex;
                return array;
            }
            public static int[] RingIndices(int corners, int offset)
            {
                int ind = 0;
                int[] array = new int[corners * 6];
                for (int i = 0; i < corners - 1; i++)
                {
                    array[ind++] = i + offset;
                    array[ind++] = i + corners + offset;
                    array[ind++] = i + corners + 1 + offset;
                    array[ind++] = i + offset;
                    array[ind++] = i + corners + 1 + offset;
                    array[ind++] = i + 1 + offset;
                }
                array[ind++] = corners - 1 + offset;
                array[ind++] = corners * 2 - 1 + offset;
                array[ind++] = corners + offset;
                array[ind++] = corners - 1 + offset;
                array[ind++] = corners + offset;
                array[ind++] = offset;
                return array;
            }
            public override void Awake(Scene scene)
            {
                base.Awake(scene);
                if (AutoUpdateVertices) UpdateVertices();
            }
            public void UpdateVertices()
            {
                Vector2 p;
                if (Track == null || Track.Scene == null)
                {
                    Collider.Position = Vector2.Zero;
                    p = Position + RenderOffset;
                }
                else
                {
                    Collider.Position = Track.Position - Position;
                    p = Track.Position + RenderOffset;
                }

                Color betweenCol = BetweenColor;
                float betweenAlpha = BetweenAlpha;
                for (int i = 0; i < corners; i++)
                {
                    float angle = i * (MathHelper.TwoPi / corners);
                    //first ring
                    Vertices[i].Position = new Vector3(p + Calc.AngleToVector(angle, InnerRadius), 0);
                    //second ring
                    Vertices[i + corners].Position = new Vector3(p + Calc.AngleToVector(angle, Calc.LerpClamp(InnerRadius, OuterRadius, BetweenLerp)), 0);
                    //third ring
                    Vertices[i + corners * 2].Position = new Vector3(p + Calc.AngleToVector(angle, OuterRadius), 0);
                    Vertices[i].Color = InnerColor * InnerAlpha * Alpha;
                    Vertices[i + corners].Color = betweenCol * betweenAlpha * Alpha;
                    Vertices[i + corners * 2].Color = OuterColor * OuterAlpha * Alpha;
                }
                Vertices[^1].Position = p.ToVec3();
                Vertices[^1].Color = InnerColor * InnerAlpha * Alpha;
            }
            public bool RemoveOnEnd = true;
            public bool FadeOut = true;
            public override void Update()
            {
                if (delayTimer > 0)
                {
                    delayTimer -= Engine.DeltaTime;
                    return;
                }
                Circle.Radius = OuterRadius + HitboxExtend;
                base.Update();
                if (AutoUpdateVertices) UpdateVertices();
                if (RemoveOnEnd && innerStartedOnce && outerStartedOnce && !innerCoroutine.Active && !outerCoroutine.Active)
                {
                    RemoveSelf();
                }
            }

            private Coroutine innerCoroutine, outerCoroutine;
            private bool innerStartedOnce, outerStartedOnce;
            public void StartInner(float radiusTo, float time, float delay, float fadeThresh, Ease.Easer ease) => StartInner(InnerRadius, radiusTo, InnerAlpha, time, delay, fadeThresh, ease);
            public void StartOuter(float radiusTo, float time, float delay, float fadeThresh, Ease.Easer ease) => StartOuter(OuterRadius, radiusTo, OuterAlpha, time, delay, fadeThresh, ease);

            public void StartInner(float radiusFrom, float radiusTo, float fromAlpha, float time, float delay, float fadeThresh, Ease.Easer ease)
            {
                innerStartedOnce = true;
                innerCoroutine.Replace(routine(radiusFrom, radiusTo, fromAlpha, 0, delay + delayTimer, time, fadeThresh, ease, (f) => InnerRadius = f, f => InnerAlpha = f));
            }
            public void StartOuter(float radiusFrom, float radiusTo, float fromAlpha, float time, float delay, float fadeThresh, Ease.Easer ease)
            {
                outerStartedOnce = true;
                outerCoroutine.Replace(routine(radiusFrom, radiusTo, fromAlpha, 0, delay + delayTimer, time, fadeThresh, ease, (f) => OuterRadius = f, f => OuterAlpha = f));
            }
            private IEnumerator routine(float radiusFrom, float radiusTo, float alphaFrom, float alphaTo, float delay, float time, float fadeThresh, Ease.Easer ease, Action<float> onRadiusUpdate, Action<float> onAlphaUpdate = null)
            {
                if (delay > 0) yield return delay;
                for (float i = 0; i < 1; i += Engine.DeltaTime / time)
                {
                    onRadiusUpdate?.Invoke(Calc.LerpClamp(radiusFrom, radiusTo, ease(i)));
                    if (FadeOut && i > fadeThresh && fadeThresh >= 0 && onAlphaUpdate != null)
                    {
                        onAlphaUpdate.Invoke(Calc.LerpClamp(alphaFrom, alphaTo, ease((i - fadeThresh) / (1 - fadeThresh))));
                    }
                    yield return null;
                }
                onRadiusUpdate?.Invoke(radiusTo);
                onAlphaUpdate?.Invoke(alphaTo);
            }
            public override void Removed(Scene scene)
            {
                base.Removed(scene);
                if (DisposeEffectOnRemoved)
                {
                    Effect?.Dispose();
                }
                BlendState?.Dispose();
            }
            public override void Render()
            {
                if (delayTimer > 0) return;
                base.Render();
                Draw.SpriteBatch.End();
                if (Effect != null && AdjustParameters != null)
                {
                    AdjustParameters.Invoke(Effect.Parameters);
                }
                if (TestCenter)
                {
                    GFX.DrawIndexedVertices(SceneAs<Level>().Camera.Matrix, Vertices, Vertices.Length, indicesWithCenter, indicesWithCenter.Length / 3, Effect, BlendState);
                }
                else
                {
                    GFX.DrawIndexedVertices(SceneAs<Level>().Camera.Matrix, Vertices, Vertices.Length - 1, indices, indices.Length / 3, Effect, BlendState);
                }
                GameplayRenderer.Begin();
            }
        }
    }

}
