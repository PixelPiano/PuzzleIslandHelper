using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.RuntimeDetour;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
namespace Celeste.Mod.PuzzleIslandHelper.Entities.Tower
{
    [CustomEntity("PuzzleIslandHelper/TowerBooster")]
    [Tracked]
    public class TowerSpiralBooster : Booster
    {
        public Vector2 Node;
        public bool Spins = true;
        public float CurrentZ;
        public float ShadeValue;
        public float BaseSpeed = -60;
        public float YSpeed
        {
            get => yspeed;
            set
            {
                if (value != yspeed)
                {
                    prevYSpeed = yspeed;
                }
                yspeed = value;
            }
        }
        private float speedDifference => yspeed - prevYSpeed;
        private float yspeed = -60;
        private float prevYSpeed;
        public float BaseWaveHeight = 20;
        private float nextYPosInCollider;
        public float WaveHeight
        {
            get => waveHeight;
            set
            {
                if (value != waveHeight)
                {
                    BaseAngle = CalculateNextBaseAngle(Radius, nextYPosInCollider, waveHeight, value, BaseAngle);
                }
                waveHeight = value;
            }
        }
        private float maxShade = 0.7f;
        private float waveHeight;
        public double BaseAngle;
        public double AngleOffset;
        public float BaseRadius = 32;
        public float Radius
        {
            get => radius;
            set
            {
                float prev = radius;
                radius = value;
                if (prev != radius)
                {
                    RecalculateBounds();
                }
            }
        }
        private float radius;
        public Rectangle Bounds;
        public bool Pause;
        public bool ReadyForTransition;
        public Vector2 PrevPosition;
        public bool UntetherCamera;
        public Coroutine SpinRoutine;
        public double AngleRate;
        public bool DisableDashing;
        private float additionalProgress;
        public DepthParticleSystem particleSystem;
        public CameraControlEntity CamControl;
        public bool PopOnContact;
        public TowerSpiralBooster(EntityData data, Vector2 offset) : this(data.Position + offset, data.NodesWithPosition(offset)[1], data.Float("baseRadius", 32), data.Float("baseSpeed", -60), data.Float("baseWaveHeight", 20))
        {
        }
        public TowerSpiralBooster(Vector2 position, Vector2 node, float radius, float speed, float waveHeight) : base(position, true)
        {
            BaseSpeed = YSpeed = speed;
            BaseWaveHeight = this.waveHeight = waveHeight;
            Node = node;
            BaseRadius = Radius = radius;
            AddTag(Tags.TransitionUpdate);
            Add(SpinRoutine = new Coroutine(false));
            SpinRoutine.Active = false;
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            ParticleSystem reference = (scene as Level).ParticlesBG;
            scene.Add(particleSystem = new DepthParticleSystem(reference.Depth, reference.particles.Length));
            CamControl = new CameraControlEntity();
            Scene.Add(CamControl);
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            particleSystem.RemoveSelf();
            CamControl.RemoveSelf();
        }
        public override void Update()
        {
            bool updateRoutine = SpinRoutine.Active;
            SpinRoutine.Active = false;
            base.Update();
            SpinRoutine.Active = updateRoutine;
            if (Scene.GetPlayer() is Player player)
            {
                nextYPosInCollider = Bounds.Height - Calc.Clamp(player.CenterY + YSpeed * Engine.DeltaTime - Bounds.Top, 0, Bounds.Height);
                if (BoostingPlayer)
                {
                    if (updateRoutine) SpinRoutine.Update();
                    if (!Pause)
                    {
                        PrevPosition = player.Center;
                        if (Spins && Radius > 0 && WaveHeight > 0)
                        {
                            CurrentZ = GetZ(Radius, nextYPosInCollider + additionalProgress, WaveHeight, BaseAngle + AngleOffset);
                            ShadeValue = maxShade - ((CurrentZ + Radius) / (Radius * 2)) * maxShade;
                            sprite.Scale = Vector2.One * Calc.LerpClamp(1, 0.7f, ShadeValue / maxShade);
                            player.MoveToX(GetX(Radius, nextYPosInCollider + additionalProgress, WaveHeight, BaseAngle + AngleOffset) + Bounds.CenterX());
                        }
                        else
                        {
                            player.MoveTowardsX(Bounds.CenterX() - player.Width / 2, 10 * Engine.DeltaTime);
                            ShadeValue = Calc.Approach(ShadeValue, 0, 10 * Engine.DeltaTime);
                            CurrentZ = Calc.Approach(CurrentZ, 0, 10 * Engine.DeltaTime);
                        }
                        player.MoveV(YSpeed * Engine.DeltaTime);
                        AngleOffset = (AngleOffset + AngleRate) % MathHelper.TwoPi;
                        sprite.RenderPosition += player.Center - PrevPosition;
                    }
                }
                else
                {
                    ShadeValue = Calc.Approach(ShadeValue, 0, 13 * Engine.DeltaTime);
                    PrevPosition = Center;
                }
            }
            else
            {
                PrevPosition = Center;
            }
            sprite.Color = Color.Lerp(Color.White, Color.Black, ShadeValue);
        }
        public IEnumerator Routine()
        {
            AngleRate = 0;
            Player player = Scene.GetPlayer();
            Radius = 0;
            YSpeed = -60;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 0.5f)
            {
                float ease = Ease.SineInOut(i);
                Radius = Calc.LerpClamp(0, 32, ease);
                yield return null;
            }
            yield return 0.4f;
            //move up, contracting while slowing down to a halt
            float prev = YSpeed;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 3f)
            {
                float ease = Ease.SineInOut(i);
                Radius = Calc.LerpClamp(32, 0, ease);
                YSpeed = Calc.LerpClamp(-60, 0, ease);
                additionalProgress += (speedDifference * Engine.DeltaTime);
                yield return null;
            }
            YSpeed = 0;
            Radius = 32;
            prev = YSpeed;
            //move down, squishing the height between waves
            for (float i = 0; i < 1; i += Engine.DeltaTime / 2f)
            {
                float ease = Ease.SineInOut(i);
                Radius = Calc.LerpClamp(0, 32, ease);
                YSpeed = Calc.LerpClamp(0, 60, ease);
                if (i > 0.5f) additionalProgress += (speedDifference * Engine.DeltaTime);
                WaveHeight = Calc.LerpClamp(20, 1, ease);
                yield return null;
            }
            prev = YSpeed;
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                float ease = Ease.SineInOut(i);
                YSpeed = Calc.LerpClamp(prev, 0, ease);
                if (i < 0.5f) additionalProgress += (speedDifference * Engine.DeltaTime);
                yield return null;
            }
            YSpeed = 0;
            //squish radius to zero
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                float ease = Ease.SineInOut(i);
                Radius = Calc.LerpClamp(32, 0, i);
                additionalProgress += (30 * Engine.DeltaTime * ease * (Radius / 32f));
                yield return null;
            }
            DisableDashing = true;
            //launch up
            YSpeed = -300;
            UntetherCamera = true;
            CamControl.Enabled = true;
            CamControl.SpeedY = -60;
            Tween.Set(this, Tween.TweenMode.Oneshot, 0.9f, Ease.SineOut, t =>
            {
                CamControl.SpeedY = Calc.LerpClamp(-60, 0, t.Eased);
            });
            WaveHeight = 40;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 0.4f)
            {
                Radius = Calc.LerpClamp(0, 40, i);
                yield return null;
            }
            yield return 0.5f;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 0.75f)
            {
                Radius = Calc.LerpClamp(40, 30, Ease.BackIn(i));
                yield return null;
            }
            for (float i = 0; i < 1; i += Engine.DeltaTime / 1.5f)
            {
                Radius = Calc.LerpClamp(30, 40, Ease.BackOut(i * 2));
                WaveHeight = Calc.LerpClamp(40, 120, Ease.BackOut(i));
                yield return null;
            }
        }
        public void RecalculateBounds()
        {
            Bounds = new Rectangle((int)(CenterX - Radius), (int)Node.Y, (int)Radius * 2, (int)(Bottom - Node.Y));
        }
        public Vector3 GetPoint(float radius, float y, float waveHeight, double angleOffset)
        {
            return new Vector3(GetX(radius, y, waveHeight, angleOffset), y, GetZ(radius, y, waveHeight, angleOffset));
        }
        public double CalculateNextBaseAngle(float r, float y, float w, float v, double m)
        {
            Vector3 P = new Vector3(GetX(r, y, w, m), y, GetZ(r, y, w, m));
            Vector3 B0 = new Vector3(GetX(r, y, v, m), y, GetZ(r, y, v, m));
            return m + Math.Atan2(Vector2.Dot(new Vector2(-P.Z, P.X), new Vector2(B0.X, B0.Z)),
                Vector2.Dot(new Vector2(P.X, P.Z), new Vector2(B0.X, B0.Z)));
        }
        public static float GetX(float radius, float y, double wavelength, double angleOffset = 0)
        {
            return (float)(double)((double)radius * Math.Sin(angleOffset + (double)y * Math.PI / wavelength));
        }
        public static float GetZ(float radius, float y, float wavelength, double angleOffset = 0)
        {
            return (float)(double)(radius * Math.Cos(angleOffset + y * Math.PI / wavelength));
        }
        private static IEnumerator Booster_BoostRoutine(On.Celeste.Booster.orig_BoostRoutine orig, Booster self, Player player, Vector2 dir)
        {
            float angle = (-dir).Angle();
            float origSize = self.particleType.Size;
            while ((player.StateMachine.State == 2 || player.StateMachine.State == 5) && self.BoostingPlayer)
            {
                self.sprite.RenderPosition = player.Center + playerOffset;
                self.loopingSfx.Position = self.sprite.Position;
                if (self.Scene.OnInterval(0.02f))
                {
                    if (self is TowerSpiralBooster t)
                    {
                        Vector2 newDir = Vector2.Normalize(player.Center - t.PrevPosition);
                        float newAngle = (-newDir).Angle();
                        self.particleType.Size = Calc.LerpClamp(origSize, origSize * 0.5f, t.ShadeValue) + 0.5f;
                        t.particleSystem.EmitDepth(self.particleType, 2, player.Center - newDir * 3f + new Vector2(0f, -2f), new Vector2(3f, 3f), Color.Lerp(self.particleType.Color, Color.Black, t.ShadeValue), newAngle, t.CurrentZ);
                        self.particleType.Size = origSize;

                    }
                    else
                    {
                        (self.Scene as Level).ParticlesBG.Emit(self.particleType, 2, player.Center - dir * 3f + new Vector2(0f, -2f), new Vector2(3f, 3f), angle);
                    }
                }

                yield return null;
                self.sprite.RenderPosition = player.Center + playerOffset;
            }

            self.PlayerReleased();
            if (player.StateMachine.State == 4)
            {
                self.sprite.Visible = false;
            }

            while (self.SceneAs<Level>().Transitioning)
            {
                yield return null;
            }

            self.Tag = 0;
        }

        private static void Booster_Respawn(On.Celeste.Booster.orig_Respawn orig, Booster self)
        {
            if (self is TowerSpiralBooster t)
            {
                t.SpinRoutine?.Cancel();
                t.YSpeed = t.BaseSpeed;
                t.WaveHeight = t.BaseWaveHeight;
                t.Radius = t.BaseRadius;
                t.CurrentZ = 0;
                t.BaseAngle = 0;
                t.sprite.Origin = Vector2.Zero;
                t.sprite.Position = Vector2.Zero;
                t.additionalProgress = 0;
                t.AngleRate = 0;
            }
            orig(self);
        }

        private static IEnumerator Player_RedDashCoroutine(On.Celeste.Player.orig_RedDashCoroutine orig, Player self)
        {
            if (self.CurrentBooster is TowerSpiralBooster t)
            {
                yield return null;
                t.sprite.CenterOrigin();
                t.sprite.Position += t.sprite.HalfSize();
                Vector2 aim = -Vector2.UnitY;
                self.gliderBoostDir = (self.DashDir = aim);
                self.SceneAs<Level>().DirectionalShake(aim, 0.2f);
                self.CallDashEvents();
                t.SpinRoutine.Cancel();
                t.SpinRoutine.Replace(t.Routine());
            }
            else
            {
                yield return orig(self);
            }
        }
        [OnLoad]
        public static void Load()
        {
            On.Celeste.Player.RedDashCoroutine += Player_RedDashCoroutine;
            On.Celeste.Player.BoostUpdate += Player_BoostUpdate;
            On.Celeste.PlayerSprite.Render += PlayerSprite_Render;
            On.Celeste.PlayerHair.Render += PlayerHair_Render;
            On.Celeste.Booster.BoostRoutine += Booster_BoostRoutine;
            On.Celeste.Booster.Respawn += Booster_Respawn;
        }
        private static int Player_BoostUpdate(On.Celeste.Player.orig_BoostUpdate orig, Player self)
        {
            if (self.CurrentBooster is TowerSpiralBooster o)
            {
                Vector2 vector = Vector2.UnitY * 3f;
                Vector2 vector2 = Calc.Approach(self.ExactPosition, self.boostTarget - self.Collider.Center + vector, 80f * Engine.DeltaTime);
                self.MoveToX(vector2.X);
                self.MoveToY(vector2.Y);
                if (o.DisableDashing)
                {
                    Input.Dash.ConsumePress();
                    Input.CrouchDash.ConsumePress();
                }
                else
                {
                    if (Input.DashPressed || Input.CrouchDashPressed)
                    {
                        self.demoDashed = Input.CrouchDashPressed;
                        Input.Dash.ConsumePress();
                        Input.CrouchDash.ConsumeBuffer();
                        if (self.boostRed)
                        {
                            return 5;
                        }

                        return 2;
                    }
                }
                return 4;
            }
            return orig(self);
        }
        private static void PlayerHair_Render(On.Celeste.PlayerHair.orig_Render orig, PlayerHair self)
        {
            if (self.Entity is Player player && player.LastBooster != null && player.LastBooster.BoostingPlayer && player.LastBooster is TowerSpiralBooster)
            {
                return;
            }
            orig(self);
        }
        private static void PlayerSprite_Render(On.Celeste.PlayerSprite.orig_Render orig, PlayerSprite self)
        {
            if (self.Entity is Player player && player.LastBooster != null && player.LastBooster.BoostingPlayer && player.LastBooster is TowerSpiralBooster)
            {
                return;
            }
            orig(self);
        }
        [OnUnload]
        public static void Unload()
        {
            On.Celeste.Player.RedDashCoroutine -= Player_RedDashCoroutine;
            On.Celeste.Player.BoostUpdate -= Player_BoostUpdate;
            On.Celeste.PlayerSprite.Render -= PlayerSprite_Render;
            On.Celeste.PlayerHair.Render -= PlayerHair_Render;
            On.Celeste.Booster.BoostRoutine -= Booster_BoostRoutine;
            On.Celeste.Booster.Respawn -= Booster_Respawn;
        }
        [CustomEntity("PuzzleIslandHelper/TowerTransitionTrigger")]
        [Tracked]
        public class TowerTransitionTrigger : Trigger
        {
            public class TransitionCutscene : CutsceneEntity
            {
                private Player player;
                private string room;
                private string marker;
                private float alpha;
                private TowerSpiralBooster towerBooster;
                private TowerSummitBooster towerSummitBooster;
                public class TowerSummitBooster : Booster
                {
                    public float InstanceTargetVolume;
                    private float volume;
                    public bool DisableDashing = true;
                    public TowerSummitBooster(Vector2 position) : base(position, true)
                    {
                    }
                    public override void Awake(Scene scene)
                    {
                        base.Awake(scene);
                        if (scene.GetPlayer() is Player player)
                        {
                            Input.Aim.Value = -Vector2.UnitY;
                            cannotUseTimer = 0.45f;
                            player.RedBoost(this);
                            wiggler.Start();
                            sprite.Play("inside");
                            sprite.FlipX = player.Facing == Facings.Left;
                        }
                    }
                    public override void Update()
                    {
                        base.Update();
                        if (loopingSfx != null && loopingSfx.InstancePlaying)
                        {
                            loopingSfx.instance.setVolume(volume = Calc.Approach(volume, 1, Engine.DeltaTime / 1.3f));
                        }
                    }
                    public int NewBoostUpdate(Player player)
                    {
                        if (DisableDashing)
                        {
                            Input.Dash.ConsumePress();
                            Input.CrouchDash.ConsumePress();
                        }
                        else
                        {
                            if (Input.DashPressed || Input.CrouchDashPressed)
                            {
                                player.demoDashed = Input.CrouchDashPressed;
                                Input.Dash.ConsumePress();
                                Input.CrouchDash.ConsumeBuffer();
                                return 5;
                            }
                        }
                        Vector2 vector = Vector2.UnitY * -3f;
                        Vector2 vector2 = Calc.Approach(player.ExactPosition, player.boostTarget - player.Collider.Center + vector, 80f * Engine.DeltaTime);
                        player.MoveToX(vector2.X);
                        player.MoveToY(vector2.Y);
                        return 4;
                    }
                    public void NewOnPlayer(Player player)
                    {

                        if (respawnTimer <= 0f && cannotUseTimer <= 0f && !BoostingPlayer)
                        {
                            Input.Aim.Value = -Vector2.UnitY;
                            cannotUseTimer = 0.45f;
                            player.RedBoost(this);
                            wiggler.Start();
                            sprite.Play("inside");
                            sprite.FlipX = player.Facing == Facings.Left;
                        }
                    }
                    public void NewPlayerBoosted(Player player, Vector2 direction)
                    {
                        if (red)
                        {
                            loopingSfx.Play("event:/game/05_mirror_temple/redbooster_move");
                            loopingSfx.DisposeOnTransition = false;
                            loopingSfx.instance.getVolume(out InstanceTargetVolume, out _);
                            loopingSfx.instance.setVolume(0);
                        }
                        BoostingPlayer = true;
                        Tag = (int)Tags.Persistent | (int)Tags.TransitionUpdate;
                        sprite.Play("spin");
                        sprite.FlipX = player.Facing == Facings.Left;
                        outline.Visible = true;
                        wiggler.Start();
                        dashRoutine.Replace(BoostRoutine(player, direction));
                    }
                    [OnLoad]
                    public static void Load()
                    {
                        On.Celeste.Booster.OnPlayer += Booster_OnPlayer;
                        On.Celeste.Booster.Respawn += Booster_Respawn1;
                        On.Celeste.Booster.PlayerBoosted += Booster_PlayerBoosted;
                        On.Celeste.Player.BoostUpdate += Player_BoostUpdate;
                        On.Celeste.Player.RedDashCoroutine += Player_RedDashCoroutine1;
                    }
                    public bool Uncollidable;
                    private static void Booster_OnPlayer(On.Celeste.Booster.orig_OnPlayer orig, Booster self, Player player)
                    {
                        if (self is TowerSummitBooster)
                        {
                            (self as TowerSummitBooster).NewOnPlayer(player);
                        }
                        else
                        {
                            orig(self, player);
                        }
                    }

                    [OnUnload]
                    public static void Unload()
                    {
                        On.Celeste.Booster.OnPlayer -= Booster_OnPlayer;
                        On.Celeste.Booster.Respawn -= Booster_Respawn1;
                        On.Celeste.Booster.PlayerBoosted -= Booster_PlayerBoosted;
                        On.Celeste.Player.BoostUpdate -= Player_BoostUpdate;
                        On.Celeste.Player.RedDashCoroutine -= Player_RedDashCoroutine1;
                    }
                    private static IEnumerator Player_RedDashCoroutine1(On.Celeste.Player.orig_RedDashCoroutine orig, Player self)
                    {
                        if (self.CurrentBooster is TowerSummitBooster)
                        {
                            yield return null;
                            self.Speed = -Vector2.UnitY * 240f;
                            self.gliderBoostDir = self.DashDir = -Vector2.UnitY;
                            self.SceneAs<Level>().DirectionalShake(-Vector2.UnitY, 0.2f);
                            self.CallDashEvents();
                        }
                        else
                        {
                            yield return new SwapImmediately(orig(self));
                        }
                    }
                    private static int Player_BoostUpdate(On.Celeste.Player.orig_BoostUpdate orig, Player self)
                    {
                        return self.CurrentBooster is TowerSummitBooster o ? o.NewBoostUpdate(self) : orig(self);
                    }
                    private static void Booster_PlayerBoosted(On.Celeste.Booster.orig_PlayerBoosted orig, Booster self, Player player, Vector2 direction)
                    {
                        if (self is TowerSummitBooster)
                        {
                            (self as TowerSummitBooster).NewPlayerBoosted(player, direction);
                        }
                        else
                        {
                            orig(self, player, direction);
                        }
                    }
                    private static void Booster_Respawn1(On.Celeste.Booster.orig_Respawn orig, Booster self)
                    {
                        if (self is TowerSummitBooster)
                        {
                            self.RemoveSelf();
                        }
                        else
                        {
                            orig(self);
                        }
                    }
                }
                private bool up;
                private Dictionary<Platform, bool> previousCollisions = [];
                private void turnOffPlatformCollisions()
                {
                    previousCollisions.Clear();
                    foreach (Platform p in Scene.Tracker.GetEntities<Platform>())
                    {
                        previousCollisions.Add(p, p.Collidable);
                        p.Collidable = false;
                    }
                }
                private void resetPlatformCollisions()
                {
                    foreach (KeyValuePair<Platform, bool> pair in previousCollisions)
                    {
                        pair.Key.Collidable = pair.Value;
                    }
                    previousCollisions.Clear();
                }
                public TransitionCutscene(TowerSpiralBooster booster, Player player, string room, string marker) : this(player, room, marker)
                {
                    towerBooster = booster;
                    up = true;
                }
                public TransitionCutscene(Player player, string room, string marker) : base()
                {
                    this.player = player;
                    this.room = room;
                    this.marker = marker;
                    Depth = int.MinValue;
                }

                private IEnumerator boosterRoutine(TowerSpiralBooster booster)
                {
                    yield return PianoUtils.Lerp(Ease.Linear, 1.4f, f =>
                    {
                        if (booster != null && booster.loopingSfx.InstancePlaying)
                        {
                            booster.loopingSfx.instance.getVolume(out float volume, out _);
                            booster.loopingSfx.instance.setVolume(volume * (1 - f));
                        }
                        alpha = f;
                    }, true);
                    AddTag(Tags.Global);
                    PianoUtils.InstantTeleportToMarker(Level, room, marker, OnBoosterTeleportEnd);
                    yield return null;
                    Level.Remove(booster);
                    RemoveTag(Tags.Global);
                    yield return 1;
                    yield return PianoUtils.Lerp(Ease.Linear, 1.4f, f =>
                    {
                        alpha = 1 - f;
                    }, true);
                    alpha = 0;
                    yield return 0.5f;

                    Rectangle bounds = Level.Bounds;
                    Vector2 point = player.BottomCenter;
                    Platform platform = Scene.CollideFirst<Platform>(point);
                    while (point.Y >= bounds.Top && platform == null)
                    {
                        platform = Scene.CollideFirst<Platform>(point);
                        point.Y--;
                    }
                    phasedPlatform = platform;
                    towerSummitBooster = new TowerSummitBooster(Vector2.Zero) { Center = player.Center };
                    Level.Add(towerSummitBooster);
                    if (phasedPlatform != null)
                    {
                        phasedPlatform.Collidable = false;
                        while (player.Bottom >= phasedPlatform.Top) yield return null;
                        phasedPlatform.Collidable = true;
                    }
                    towerSummitBooster.DisableDashing = false;
                    EndCutscene(Level);
                }
                private Platform phasedPlatform;
                private bool teleported;
                private IEnumerator fallRoutine()
                {
                    while (player.OnScreen(8)) yield return null;
                    player.StateMachine.State = Player.StDummy;
                    player.DummyGravity = false;
                    yield return PianoUtils.Lerp(Ease.Linear, 1.4f, f =>
                    {
                        alpha = f;
                    }, true);
                    AddTag(Tags.Global);
                    PianoUtils.InstantTeleportToMarker(Level, room, marker, OnFallTeleportEnd);
                    yield return null;
                    RemoveTag(Tags.Global);
                    yield return 1;
                    yield return PianoUtils.Lerp(Ease.Linear, 1.4f, f =>
                    {
                        alpha = 1 - f;
                    }, true);
                    alpha = 0;
                    yield return 0.5f;
                    player = Level.GetPlayer();
                    player.StateMachine.State = Player.StNormal;
                    EndCutscene(Level);
                }
                public override void Render()
                {
                    base.Render();
                    if (alpha > 0)
                    {
                        Draw.Rect(SceneAs<Level>().Camera.Position, 320, 180, Color.White * alpha);
                    }
                }

                public override void OnBegin(Level level)
                {
                    if (up)
                    {
                        Add(new Coroutine(boosterRoutine(towerBooster)));
                    }
                    else
                    {
                        Add(new Coroutine(fallRoutine()));
                    }
                }
                public void TrySetPlayerOnPlatformAbove(Level level, Player player)
                {
                    Vector2 orig = player.Position;
                    Rectangle r = level.Bounds;
                    bool found = false;

                    while (player.Y > r.Top)
                    {
                        if (player.CollideFirst<Platform>() is Platform p)
                        {
                            player.Bottom = p.Top;
                            found = true;
                            break;
                        }
                        else
                        {
                            player.Y--;
                        }
                    }
                    if (!found) player.Position = orig;
                    player.StateMachine.State = Player.StNormal;
                }
                public void OnBoosterTeleportEnd(Level level, Player player)
                {
                    teleported = true;
                    Level = level;
                    this.player = player;
                    if (WasSkipped)
                    {
                        TrySetPlayerOnPlatformAbove(level, player);
                    }
                    else
                    {
                        player.StateMachine.State = Player.StDummy;
                        player.DummyGravity = false;
                        player.DummyFriction = false;
                        player.Speed = Vector2.Zero;
                    }
                }
                public void OnFallTeleportEnd(Level level, Player player)
                {
                    teleported = true;
                    Level = level;
                    this.player = player;
                    if (WasSkipped)
                    {
                        player.StateMachine.State = Player.StNormal;
                    }
                    else
                    {
                        player.StateMachine.State = Player.StDummy;
                        player.DummyGravity = false;
                        player.Speed = Vector2.Zero;
                    }
                }
                public override void OnEnd(Level level)
                {
                    alpha = 0;
                    RemoveTag(Tags.Global);
                    if (phasedPlatform != null)
                    {
                        phasedPlatform.Collidable = true;
                    }
                    if (WasSkipped)
                    {
                        if (teleported)
                        {
                            if (up)
                            {
                                TrySetPlayerOnPlatformAbove(level, player);
                            }
                            player.StateMachine.State = Player.StNormal;
                        }
                        else
                        {
                            if (up)
                            {
                                PianoUtils.InstantTeleportToMarker(Level, room, marker, OnFallTeleportEnd);
                            }
                            else
                            {
                                PianoUtils.InstantTeleportToMarker(Level, room, marker, OnBoosterTeleportEnd);
                            }
                        }
                    }
                    else if (!up)
                    {
                        player.StateMachine.State = Player.StNormal;
                    }
                }
            }
            public bool TransitioningUp;
            public string Room;
            public string Marker;
            public bool Running;
            public TowerTransitionTrigger(EntityData data, Vector2 offset) : base(data, offset)
            {
                TransitioningUp = data.Bool("upTransition");
                Room = data.Attr("room");
                Marker = data.Attr("marker");
            }
            private void doUpTransition(Player player)
            {
                CameraControlEntity c = Scene.Tracker.GetEntity<CameraControlEntity>();
                if (c != null)
                {
                    c.Enabled = true;
                    c.OverrideY = player.CameraTarget.Y;
                    c.Offset = 0;
                    c.SpeedY = -200f;
                    Tween.Set(c, Tween.TweenMode.Oneshot, 1, Ease.CubeOut, t =>
                    {
                        c.SpeedY = Calc.LerpClamp(-200, 0, t.Eased);
                    });
                }
                Running = true;
                Scene.Add(new TransitionCutscene(player.CurrentBooster as TowerSpiralBooster, player, Room, Marker));
            }
            public override void OnEnter(Player player)
            {
                base.OnEnter(player);
                if (TransitioningUp)
                {
                    if (SceneAs<Level>().Tracker.GetEntity<TowerSpiralBooster>() is TowerSpiralBooster booster && booster.BoostingPlayer)
                    {
                        doUpTransition(player);
                    }
                }
                else
                {
                    Running = true;
                    Scene.Add(new TransitionCutscene(player, Room, Marker));
                }
            }
        }
        [Tracked]
        public class CameraControlEntity : Entity
        {
            public bool Enabled;
            public float? OverrideY;
            public float SpeedY;
            public float Offset;
            public CameraControlEntity() : base()
            {
            }
            public override void Update()
            {
                base.Update();
                if (Enabled)
                {
                    Offset += SpeedY * Engine.DeltaTime;
                }

            }
            private static Hook hookPlayerCameraTarget;
            [OnLoad]
            public static void Load()
            {
                hookPlayerCameraTarget = new Hook(
                    typeof(Player).GetMethod("get_CameraTarget"),
                    typeof(CameraControlEntity).GetMethod("modCameraTarget", BindingFlags.NonPublic | BindingFlags.Static));
            }
            [OnUnload]
            public static void Unload()
            {
                hookPlayerCameraTarget?.Dispose();
                hookPlayerCameraTarget = null;
            }


            private static Vector2 modCameraTarget(Func<Player, Vector2> orig, Player self)
            {
                if (self.Scene == null) return orig(self);

                Vector2 target = orig(self);
                foreach (CameraControlEntity entity in self.Scene.Tracker.GetEntities<CameraControlEntity>())
                {
                    if (entity.Enabled)
                    {
                        return new Vector2(target.X, (entity.OverrideY ?? target.Y) + entity.Offset);
                    }
                }
                return target;
            }
        }
    }
}