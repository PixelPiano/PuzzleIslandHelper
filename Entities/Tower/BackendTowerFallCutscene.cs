using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.WIP;
using Celeste.Mod.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static Celeste.Mod.PuzzleIslandHelper.Entities.Tower.Portal;


namespace Celeste.Mod.PuzzleIslandHelper.Entities.Tower
{
    [Tracked]
    public class BackendTowerFallCutscene : CutsceneEntity
    {
        private static bool overrideReflectionFall = false;
        private TowerSpiralBooster booster;
        public Player Player;
        public RedMemoryOrb Red;
        public RedOrbShield Shield;
        public Act Act;
        private float BlackFade = 1;
        public float CameraYOffset = -64;
        public BackendTowerFallCutscene() : base()
        {
            Depth = int.MinValue;

        }
        public override void Render()
        {
            if (BlackFade > 0)
            {
                Draw.Rect(SceneAs<Level>().Camera.Position, 320, 180, Color.Black * BlackFade);
            }
        }
        public override void OnBegin(Level level)
        {
            level.Session.SetFlag("inFallSequence");
            level.Session.SetFlag("FellDownBackendTower");
            Player = level.GetPlayer();
            Red = level.Tracker.GetEntity<RedMemoryOrb>();
            Add(orbCoroutine = new Coroutine(redPulse()));
            redAlphaAddTween = Tween.Create(Tween.TweenMode.Persist, Ease.SineOut, 0.7f, false);
            redAlphaAddTween.OnUpdate = t => redAlphaAdd = Calc.LerpClamp(0.07f, 0, t.Eased);
            redAlphaAddTween.OnComplete = t => redAlphaAdd = 0;
            Add(redAlphaAddTween);
            booster = level.Tracker.GetEntity<TowerSpiralBooster>();
            if (booster != null)
            {
                booster.Collidable = false;
            }
            Player.StateMachine.State = Player.StDummy;
            Player.DummyGravity = false;
            Player.Speed.Y = 0;
            if (Marker.TryFind("fallMarker", out Vector2 p))
            {
                Player.Position = p;
            }
            Level.Camera.Position = Player.CameraTarget;
            Add(new Coroutine(routine()));
        }
        public override void Update()
        {
            base.Update();
            Camera camera = Level.Camera;
            float min = Level.Bounds.Top;
            camera.Position = Player.CameraTarget;
            camera.Y = Math.Max(min, camera.Y + CameraYOffset);
        }
        private float timer;
        private static bool hitSomething;
        public void SetCamera(float amount)
        {
            CameraYOffset = -64 * (1 - amount);
        }
        private void cancelRedPulse()
        {
            orbCoroutine?.Cancel();
            redAlphaAddTween.Stop();
            Red.RedAlpha = RedMemoryOrb.HiddenAlpha;
            Red.Orb.Radius = MaxOrbRadius;
        }
        private Coroutine orbCoroutine;
        private float redAlphaAdd;
        private Tween redAlphaAddTween;
        private void instantRedPulse()
        {
            redAlphaAddTween.Start();
            orbCoroutine.Replace(redPulse());
            orbCoroutine.Update();
        }
        private IEnumerator redPulse()
        {
            while (true)
            {
                float from = MaxOrbRadius;
                yield return PianoUtils.Lerp(Ease.CubeIn, 0.6f, f =>
                {
                    Red.RedAlpha = Calc.LerpClamp(RedMemoryOrb.HiddenAlpha, RedMemoryOrb.HiddenAlpha * 3, f) + redAlphaAdd;
                    Red.Orb.Radius = Calc.LerpClamp(from, from + 4, f);
                });
                yield return PianoUtils.Lerp(Ease.SineInOut, 0.6f, f =>
                {
                    Red.RedAlpha = Calc.LerpClamp(RedMemoryOrb.HiddenAlpha * 3, RedMemoryOrb.HiddenAlpha, f) + redAlphaAdd;
                    Red.Orb.Radius = Calc.LerpClamp(from + 4, from, f);
                });
                yield return 0.7f;
            }
        }
        private IEnumerator routine()
        {
            //fade in
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                BlackFade = 1 - i;
                yield return null;
            }
            BlackFade = 0;
            Red.AutoFade = false;
            Red.State = MemoryOrb.StDummy;
            Red.DummyHover = false;
            //maddy falling
            overrideReflectionFall = true;
            Player.StateMachine.State = Player.StReflectionFall;
            //red orb catches up and creates protective bubble around her just before she hits the ground
            //maddy breaks through several layers of ground before falling in a pool of water
            //handled in ReflectionFallUpdate hook
            bool playerOnScreen = false;
            Shield = new RedOrbShield(Vector2.Zero, SetCamera);
            while (overrideReflectionFall)
            {
                if (Player.OnScreen())
                {
                    if (!hitSomething) timer += Engine.DeltaTime;
                    if (!playerOnScreen)
                    {
                        Scene.Add(Shield);
                        Shield.StateMachine.State = 1;
                    }
                    playerOnScreen = true;
                }
                yield return null;
            }

            //moment maddy hits the water, act 3 title card appears
            yield return new SwapImmediately(Act3Begin());
            //end cutscene
            EndCutscene(Level);
        }
        [Command("add_portal", "")]
        public static void AddCutsceneAaaaa()
        {
            if (Engine.Scene.Tracker.GetEntity<BackendTowerFallCutscene>() is Entity e)
            {
                Engine.Scene.Remove(e);
            }
            Engine.Scene.Add(new BackendTowerFallCutscene());
        }
        [OnLoad]
        public static void Load()
        {
            hitSomething = false;
            overrideReflectionFall = false;
            On.Celeste.Player.ReflectionFallUpdate += Player_ReflectionFallUpdate;
            On.Celeste.Player.ReflectionFallEnd += Player_ReflectionFallEnd;
            Everest.Events.Level.OnAfterUpdate += Level_OnAfterUpdate;
        }

        private static void Level_OnAfterUpdate(Level obj)
        {
            if (obj.Session.GetFlag("inFallSequence"))
            {
                if (obj.Camera.Y < obj.Bounds.Top)
                {
                    obj.Camera.Y = obj.Bounds.Top;
                }
            }
            if (obj.Tracker.GetEntity<BackendTowerFallCutscene>() is BackendTowerFallCutscene cutscene)
            {
                if (cutscene.Red != null && cutscene.Shield != null)
                {
                    cutscene.Red.Position = cutscene.Shield.Center;
                }
            }
        }


        [OnUnload]
        public static void Unload()
        {
            overrideReflectionFall = false;
            On.Celeste.Player.ReflectionFallUpdate -= Player_ReflectionFallUpdate;
            On.Celeste.Player.ReflectionFallEnd -= Player_ReflectionFallEnd;
            Everest.Events.Level.OnAfterUpdate -= Level_OnAfterUpdate;
        }
        private static void Player_ReflectionFallEnd(On.Celeste.Player.orig_ReflectionFallEnd orig, Player self)
        {
            overrideReflectionFall = false;
            orig(self);
        }
        private static int Player_ReflectionFallUpdate(On.Celeste.Player.orig_ReflectionFallUpdate orig, Player self)
        {
            int v = orig(self);
            if (overrideReflectionFall)
            {
                CustomFallUpdate(self);
            }
            return v;
        }
        public static void CustomFallUpdate(Player player)
        {
            BackendTowerFallCutscene cutscene = player.Scene.Tracker.GetEntity<BackendTowerFallCutscene>();
            if (cutscene == null) return;
            if (player.CollideCheck<Water>())
            {
                overrideReflectionFall = false;
                return;
            }
            Rectangle bounds = new Rectangle((int)player.X - 6, (int)player.Y, 12, 12);
            DashBlock dashBlock = player.Scene.CollideFirst<DashBlock>(bounds);
            if (dashBlock != null)
            {
                hitSomething = true;
                dashBlock.Break(dashBlock.TopCenter, Vector2.UnitY, true);
                player.level.Shake();
                cutscene.OnHitObject();
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                Celeste.Freeze(0.1f);
            }
            MiniHeartChecker miniHeartChecker = player.Scene.CollideFirst<MiniHeartChecker>(bounds);
            if (miniHeartChecker != null)
            {
                hitSomething = true;
                miniHeartChecker.Break();
                player.level.Shake();
                cutscene.OnHitObject();
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                Celeste.Freeze(0.1f);
            }
            TowerSpiralBooster booster = player.Scene.Tracker.GetEntity<TowerSpiralBooster>();
            if (booster != null && booster.respawnTimer == 0 && player.CollideRect(booster.Collider.Bounds))
            {
                hitSomething = true;
                booster.PlayerReleased();
                booster.respawnTimer = 5;
            }
        }
        private void snapPlayerToWater()
        {
            if (Marker.TryFind("fallMarker", out Vector2 p) && Marker.TryFind("waterbedMarker", out Vector2 p2))
            {
                Player.Position = p;
                while (Player.Y < Level.Bounds.Bottom && !Player.CollideCheck<Water>())
                {
                    Player.Y++;
                    if (Player.CollideFirst<DashBlock>() is DashBlock dashBlock)
                    {
                        dashBlock.RemoveAndFlagAsGone();
                    }
                    if (Player.CollideFirst<MiniHeartChecker>() is MiniHeartChecker miniHeartChecker)
                    {
                        miniHeartChecker.RemoveAndFlagAsGone();
                    }
                }
                Player.Y = p2.Y;
                Level.Camera.Position = Player.CameraTarget;
            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Act?.RemoveSelf();
        }
        public void OnHitObject()
        {
            instantRedPulse();
            ShieldFlare();
        }
        public void ShieldFlare()
        {
            //todo: make shield flare up a little bit
        }
        public override void OnEnd(Level level)
        {
            PianoModule.Session.OrbsMerged = true;
            MInput.Disabled = false;
            cancelRedPulse();
            level.Session.SetFlag("inFallSequence", false);
            BlackFade = 0;
            if (WasSkipped)
            {
                snapPlayerToWater();
            }
            Player.StateMachine.State = Player.StSwim;
            CameraYOffset = 0;
            Shield?.RemoveSelf();
            Act?.RemoveSelf();
            overrideReflectionFall = false;
            if (booster != null)
            {
                booster.Collidable = true;
            }
        }
        private class Act3 : Act
        {
            private ParticleSystem system;
            private class Bubble : Sprite
            {
                public float Speed;
                public float Alpha = 1;
                public float ColorLerp;
                private bool spawnParticles = true;
                private ParticleType particle = new ParticleType()
                {
                    Color = Color.Cyan,
                    Color2 = Color.Blue,
                    ColorMode = ParticleType.ColorModes.Choose,
                    Direction = -MathHelper.PiOver2,
                    DirectionRange = MathHelper.PiOver4 / 2,
                    LifeMin = 0.6f,
                    LifeMax = 0.9f,
                    FadeMode = ParticleType.FadeModes.Linear,
                    Size = 6
                };
                private ParticleSystem system;
                public Bubble(ParticleSystem system, float scale, float x, float ySpeed) : base(GFX.Game, "objects/PuzzleIslandHelper/")
                {
                    this.system = system;

                    Scale = scale * Vector2.One;
                    Position.Y = 1080;
                    Position.X = x;
                    Speed = ySpeed * 6;
                }
                public override void Added(Entity entity)
                {
                    base.Added(entity);
                    AddLoop("idle", "actBubble", 0.2f);
                    Play("idle", false, true);
                    Position.Y += Height * Scale.Y;

                }
                public override void Render()
                {
                    base.Render();
                }
                public override void Update()
                {
                    base.Update();
                    Position.Y += Speed * Engine.DeltaTime;

                    ColorLerp = 1 - (Math.Max(0, Position.Y - 320) / (1080 - 320));
                    Speed = Calc.Approach(Speed, 0, 400f * Engine.DeltaTime);
                    if (Math.Abs(Speed) < 50f) spawnParticles = false;
                    Color = Color.Lerp(Color.White, Color.Red, ColorLerp * 0.7f) * Alpha;
                    if (spawnParticles && Scene.OnInterval(0.1f))
                    {
                        EmitParticle();
                    }
                }
                public void EmitParticle()
                {
                    Vector2 pos = Position + new Vector2((int)((Width * Scale.X) / 2 + (Width * Scale.X) / 2 * Calc.Random.Range(-1, 1f)), (int)(Height * Scale.Y + Calc.Random.Range(1, 6)));
                    particle.Color = Color.Lerp(Color.Cyan, Color.DeepPink, ColorLerp);
                    particle.Color2 = Color.Lerp(Color.Blue, Color.Red, ColorLerp);
                    particle.Direction = MathHelper.PiOver2;
                    system.Emit(particle, pos);
                }
            }
            private float vertexLerp;
            private VertexPositionColor[] vertices;
            private int[] indices = [0, 1, 2, 1, 2, 3];
            public Act3() : base("Act 3: Red")
            {
                vertices = new VertexPositionColor[4];
                vertices[1].Position = new Vector3(-30, -30, 0);
                vertices[0].Position = vertices[0].Position;
                vertices[2].Position = new Vector3(1950, -30, 0);
                vertices[3].Position = vertices[2].Position;
                vertices[1].Color = vertices[2].Color = Color.Red;
            }
            public override void Render()
            {
                float width = 1920;
                float height = 1080;
                Vector2 p = new Vector2(width / 2, height / 2);
                int size = (int)(height / 5f);
                Draw.Rect(-20, -20, width + 40, height + 40, Color.Black * BGAlpha);
                Components.Render();
                system.Render();
                SubHudRenderer.EndRender();
                GFX.DrawIndexedVertices(Matrix.Identity, vertices, 4, indices, 2);
                SubHudRenderer.BeginRender();
                ActiveFont.Draw(Text, p, Vector2.One / 2, Vector2.One * size / ActiveFont.BaseSize, Color.White * TextAlpha);
            }
            public override void Update()
            {
                base.Update();
                vertices[0].Position.Y = vertices[3].Position.Y = -30 + 1110 * vertexLerp;
                vertices[1].Color = vertices[2].Color = Color.Red * TextAlpha;

            }
            public override void SetTextAlpha(float alpha)
            {
                base.SetTextAlpha(alpha);
                foreach (Bubble b in Components.GetAll<Bubble>())
                {
                    b.Alpha = alpha;
                }
            }
            public override void Added(Scene scene)
            {
                base.Added(scene);
                Tween.Set(this, Tween.TweenMode.Oneshot, 1, Ease.CubeOut, t => vertexLerp = t.Eased);
                system = new ParticleSystem(int.MinValue, 200);
                system.Visible = false;
                scene.Add(system);

                bool high = false;
                for (float i = 0; i < 1840; i += 1840 / 14f)
                {
                    float ySpeed = high ? -Calc.Random.Range(140f, 155f) : -Calc.Random.Range(80f, 100f);
                    high = !high;
                    Bubble bubble = new Bubble(system, Calc.Random.Range(1, 1.5f), 30 + i, ySpeed);
                    Add(bubble);
                }
            }
            public override void Removed(Scene scene)
            {
                base.Removed(scene);
                system.RemoveSelf();
            }
        }
        public IEnumerator Act3Begin()
        {
            Act = new Act3();
            Level.Add(Act);
            Act.OnTextFadeStart = () =>
            {
                if (Marker.TryFind("waterbedMarker", out Vector2 p))
                {
                    Player.Position = p;
                }
                Player.StateMachine.State = Player.StDummy;
                Player.Speed.Y = 0;
                Player.DummyGravity = false;
            };
            Act.OnBGFadeStart = () =>
            {
                Player.StateMachine.State = Player.StSwim;
                MInput.Disabled = true;
            };
            Coroutine coroutine = new Coroutine(Act.Routine(Player));
            Add(coroutine);
            yield return 2;
            Shield.RemoveSelf();
            Act.CanContinue = true;
            Act.PlayerCanPress = true;
            while (!coroutine.Finished) yield return null;
            yield return 0.3f;
        }
        public class RedOrbShield : Entity
        {
            public const float WaveRadius = 80;
            public const float DistanceFromPlayer = 140;
            public float DistanceFromPlayerMult = 1;
            public float WaveRadiusMult = 1;
            public float FormAmount = 0;
            public StateMachine StateMachine;
            private Player player;
            private Wiggler wiggler;
            private float wiggle;
            public float ShieldAmount = 1;
            public Vector2 ShieldScale = Vector2.One;
            public float ShieldWidth = 32, ShieldHeight = 32;
            public VertexPositionColor[] Vertices;
            public int[] Indices;
            public const int HalfRes = 20;
            public Vector2 Scale => Vector2.One + pulseScale;
            public VertexOffsetComponent[] RingOffsets;
            public Color InnerColor = Color.Red;
            public Color OuterColor = Color.DarkRed;
            private float tailYOffsetAmount;
            private float maxTailYOffset = -10;
            public List<Vector2> OrigOuterPoints = [];
            public List<TailParticle> tailParticles = [];
            private Vector2 pulseScale;
            private BetterShaker shaker;
            private Vector2 shake;
            private FlashStack stack;
            private float shakeMult = 1;
            private Tween shakeMultTween;
            private float shakeMultTo;
            public float TimeActiveMult = 3;
            private Wiggler pulseWiggler;
            private int colorIndex;
            private Color[] colorSequence = [Color.White, Color.Yellow, Color.Orange, Color.Beige];
            private float[] tailOffsets = [-8, -4, -24, -8, -4];
            private static Vector2 fallSpriteOffset = new Vector2(3, -8);
            private Action<float> setCamera;
            private float sparkleMult = 0;
            public RedOrbShield(Vector2 position, Action<float> setCamera) : base(position)
            {
                this.setCamera = setCamera;
                Add(stack = new FlashStack());
                Add(shaker = new(v => shake += v));
                shaker.Interval *= 2;
                Collider = new Hitbox(32, 32, 0, 0);
                Add(StateMachine = new StateMachine(3));
                StateMachine.SetCallbacks(0, IdleUpdate);
                StateMachine.SetCallbacks(1, ShimmerUpdate, ShimmerRoutine, ShimmerBegin, ShimmerEnd);
                StateMachine.SetCallbacks(2, ShieldUpdate, null, ShieldBegin);
                StateMachine.State = 0;
                pulseWiggler = Wiggler.Create(0.25f, 1, f =>
                {
                    pulseScale = Vector2.One * (f * 0.1f);
                }, false, false);
                pulseWiggler.StartZero = true;
                Add(pulseWiggler);
                Depth = -1;
                shakeMultTween = Tween.Create(Tween.TweenMode.Persist, Ease.Linear, 1, false);
                float from = 0;
                shakeMultTween.OnStart = t => from = shakeMult;
                shakeMultTween.OnUpdate = t => shakeMult = Calc.LerpClamp(from, shakeMultTo, t.Eased);
                shakeMultTween.OnComplete = t => shakeMult = shakeMultTo;
                Add(shakeMultTween);
                int size = HalfRes * 4 + 2;
                Vertices = new VertexPositionColor[size + 2];
                RingOffsets = new VertexOffsetComponent[size - 4];
                for (int i = 0; i < RingOffsets.Length; i++)
                {
                    int min, max;
                    int speed;
                    if (int.IsEvenInteger(i))
                    {
                        min = -1; max = 0;
                        speed = 4;
                    }
                    else
                    {
                        min = 0; max = 4;
                        speed = 16;
                    }
                    RingOffsets[i] = new VertexOffsetComponent(max, min, 0.4f, VertexOffsetComponent.ApproachModes.ConstantApproach, true, null)
                    {
                        ConstantApproachSpeed = speed,
                        WaitForTimer = false
                    };

                }
                Add(RingOffsets);
                UpdateVertices();
                List<int> indices = [];
                for (int i = 2; i < size; i++)
                {
                    indices.AddRange([i - 2, i - 1, i]);
                }
                indices.AddRange([size, 0, 1, size + 1, size - 2, size - 1]);
                Indices = [.. indices];
                Add(new Coroutine(particleRoutine()));
            }
            public int IdleUpdate()
            {
                ShieldUpdate();
                return 0;
            }
            public void ShimmerBegin()
            {
                sparkleMult = 1;
                TimeActiveMult = 3;
                DistanceFromPlayerMult = 1;
                WaveRadiusMult = 1;
                FormAmount = 0;
                ShieldAmount = 0;
            }
            public void ShimmerEnd()
            {
                sparkleMult = 1;
                TimeActiveMult = 0;
                DistanceFromPlayerMult = 0;
                WaveRadiusMult = 0;
                FormAmount = 1;
                ShieldAmount = 1;
            }
            public IEnumerator ShimmerRoutine()
            {
                float waitTime = 0.7f;
                float flyTimeA = 1;
                float flyTimeB = 1;
                float waitTime2 = 0.4f; //3.1
                float formTime = 1; //4.1
                float meteorTimeA = 1f;//5.1f
                float meteorWait = 0.2f; //5.3f
                float meteorTimeB = 0.9f; //6.2f


                yield return waitTime;

                float timeActiveMultFrom = TimeActiveMult;
                for (float i = 0; i < 1; i += Engine.DeltaTime / (flyTimeA + flyTimeB))
                {
                    WaveRadiusMult = Calc.LerpClamp(1f, 0, Ease.SineOut(i));
                    DistanceFromPlayerMult = Calc.LerpClamp(1f, 0, Ease.SineInOut(i));
                    TimeActiveMult = Calc.LerpClamp(timeActiveMultFrom, 0, Ease.SineOut(i));
                    yield return null;
                }
                DistanceFromPlayerMult = 0;
                WaveRadiusMult = 0;
                Tween.Set(this, Tween.TweenMode.Oneshot, waitTime2 + formTime + meteorTimeA, Ease.SineInOut, t =>
                {
                    setCamera?.Invoke(t.Eased);
                }, t => setCamera?.Invoke(1));
                yield return waitTime2;
                yield return PianoUtils.Lerp(Ease.CubeInOut, formTime, f => FormAmount = f, true);
                yield return PianoUtils.Lerp(Ease.SineOut, meteorTimeA, f => ShieldAmount = f * 0.3f);
                yield return meteorWait;
                yield return PianoUtils.Lerp(Ease.BackIn, meteorTimeB, f => ShieldAmount = 0.3f + 0.7f * f, true);
                StateMachine.State = 2;
            }
            public int ShimmerUpdate()
            {
                float sin = (float)Math.Sin(Scene.TimeActive * TimeActiveMult);
                float x = player.CenterX + sin * WaveRadius * WaveRadiusMult;
                float y = player.CenterY - DistanceFromPlayer * DistanceFromPlayerMult;

                Center = new Vector2(x, y) + fallSpriteOffset;
                return 1;
            }
            public int ShieldUpdate()
            {
                if (sparkleMult > 0)
                {
                    sparkleMult = Calc.Approach(sparkleMult, 0, Engine.DeltaTime);
                }
                Center = player.Center + fallSpriteOffset;
                if (Scene.OnInterval(0.3f))
                {
                    Add(new Pulse(1f, Ease.Linear, MathHelper.PiOver4 / 2, 0, colorSequence[colorIndex], null) { LerpMult = 0.2f });
                    colorIndex = (colorIndex + 1) % colorSequence.Length;
                }
                return 2;
            }
            public void ShieldBegin()
            {
                //shieldBeginTween.Start();
            }
            public void ShakeMultTo(float to, float duration, Ease.Easer ease)
            {
                shakeMultTween.Easer = ease;
                shakeMultTween.Start(duration);
            }
            public void StopShaking()
            {
                shaker.StopShaking();
                shakeMultTween.Stop();
                shakeMult = 1;
            }
            public void StartShaking(float time = -1)
            {
                shaker.ShakeFor(time);
            }
            private void doMainPulse(float duration)
            {
                Add(new Pulse(duration, Ease.Linear, MathHelper.PiOver2, MathHelper.PiOver4 / 2, Color.Orange, () =>
                {
                    if (StateMachine.State != 1)
                    {
                        stack.AddFlash(Color.White, 0.00125f, 0.3f);
                        stack.AddFlash(Color.Black, 0.025f, 0.3f);
                        pulseWiggler.Start();
                        SpawnTailParticle();
                        doMainPulse(duration);
                    }
                })
                { LerpMult = 0.63f });
            }
            private IEnumerator particleRoutine()
            {
                float duration = 0.25f;
                doMainPulse(duration / 2);
                while (true)
                {
                    yield return PianoUtils.Lerp(Ease.Linear, duration, f => tailYOffsetAmount = f, true);
                    yield return PianoUtils.Lerp(Ease.ExpoOut, duration, f => tailYOffsetAmount = 1 - f, true);
                }
            }
            public void SpawnTailParticle()
            {
                int size = HalfRes * 4 + 2;
                int[] ind = [size, 0, 1, size + 1, size - 2, size - 1];
                VertexPositionColor[] array = [Vertices[ind[0]], Vertices[ind[1]], Vertices[ind[2]]];
                VertexPositionColor[] array2 = [Vertices[ind[3]], Vertices[ind[4]], Vertices[ind[5]]];
                array[1].Position.Y -= Calc.Random.Range(2, 6);
                array2[2].Position.Y -= Calc.Random.Range(2, 6);

                float rotationRate = 1.5f.ToRad();
                float ySpeed = -50f;
                float xSpeed = 10f;
                TailParticle particleA = null;
                particleA = new TailParticle(array, -rotationRate, ySpeed, -xSpeed, 1f, () => tailParticles.Remove(particleA));
                TailParticle particleB = null;
                particleB = new TailParticle(array, rotationRate, ySpeed, xSpeed, 1f, () => tailParticles.Remove(particleB));
                TailParticle particleC = null;
                particleC = new TailParticle(array2, -rotationRate, ySpeed, -xSpeed, 1f, () => tailParticles.Remove(particleC));
                TailParticle particleD = null;
                particleD = new TailParticle(array2, rotationRate, ySpeed, xSpeed, 1f, () => tailParticles.Remove(particleD));
                particleA.Alpha = particleB.Alpha = particleC.Alpha = particleD.Alpha = Math.Min(1, Scale.X);
                Add(particleA, particleB, particleC, particleD);
                tailParticles.Add(particleA);
                tailParticles.Add(particleB);
                tailParticles.Add(particleC);
                tailParticles.Add(particleD);
            }
            public void UpdateVertices()
            {
                bool initializePoints = OrigOuterPoints.Count == 0;
                float width = ShieldWidth * ShieldScale.X * Scale.X;
                float height = ShieldHeight * ShieldScale.Y * Scale.Y;

                Ease.Easer xApproachEase = Ease.SineIn;
                Ease.Easer yApproachEase = Ease.SineIn;
                float maxApproach = 3;
                float minApproach = 1;
                float angle = MathHelper.Pi;
                int index = 0;
                float lerp;
                Vector2 size = new Vector2(width / 2, height / 2);
                while (angle <= MathHelper.TwoPi)
                {
                    lerp = Calc.AngleDiff(angle, MathHelper.Pi + MathHelper.PiOver2) / MathHelper.PiOver2;
                    Vector2 v = Calc.AngleToVector(-angle, 1) * size;
                    if (initializePoints) OrigOuterPoints.Add(v);
                    Vertices[index].Position = new(v + Center, 0);
                    Vertices[index].Color = OuterColor;
                    float approachX = minApproach + xApproachEase(lerp) * (maxApproach - minApproach);
                    float approachY = minApproach + yApproachEase(lerp) * (maxApproach - minApproach);
                    v.Y = Calc.Approach(v.Y, 0, approachY);
                    v.X = Calc.Approach(v.X, 0, approachX);
                    Vertices[index + 1].Position = new(v + Center, 0);
                    Vertices[index + 1].Color = InnerColor;
                    index += 2;
                    if (angle == MathHelper.TwoPi) break;
                    angle = Calc.Approach(angle, MathHelper.TwoPi, MathHelper.Pi / (HalfRes * 2));
                }
                Vector3 tailOffset = Vector3.UnitY * (-8 + maxTailYOffset * tailYOffsetAmount) * ShieldAmount;
                Vertices[index].Position = Vertices[0].Position + tailOffset;
                Vertices[index + 1].Position = Vertices[index - 2].Position + tailOffset;
                Vertices[index].Color = Vertices[index + 1].Color = Color.OrangeRed;
                Vector3 center = Center.ToVec3();
                for (int i = 0; i < RingOffsets.Length; i++)
                {
                    Vector3 p = Vertices[i + 2].Position;
                    Vector3 newP = Calc.Approach(p, center, RingOffsets[i].FloatOffset);
                    Vector3 next = p + (newP - p) * ShieldAmount;
                    Vertices[i + 2].Position = next;

                    Color color = Vertices[i + 2].Color;
                    Color nextColor = Color.Lerp(color, int.IsEvenInteger(i) ? Color.Black : Color.White, 0.4f + Calc.Random.Range(-0.3f, 0.3f));
                    float colorlerp = Math.Abs((RingOffsets[i].FloatOffset - RingOffsets[i].MinOffset) / (RingOffsets[i].MaxOffset - RingOffsets[i].MinOffset));
                    Vertices[i + 2].Color = Color.Lerp(color, nextColor, colorlerp);
                }
                foreach (Pulse p in Components.GetAll<Pulse>())
                {
                    AddFade(p.Eased, p.FadeAngleRegion, p.SolidAngleRegion, p.Color, p.LerpMult);
                }
                if (!stack.Empty)
                {
                    for (int i = 0; i < Vertices.Length; i++)
                    {
                        Vertices[i].Color = Color.Lerp(Vertices[i].Color, stack.Color, stack.Lerp);
                    }
                }
                if (ShieldAmount < 1)
                {
                    float fadeArea = MathHelper.PiOver4 * (1 - ShieldAmount);
                    if (ShieldAmount < 0.05f) fadeArea *= (ShieldAmount / 0.05f);
                    int index2 = 0;

                    float globalDiff = ShieldAmount * (MathHelper.PiOver2 + 2f.ToRad());
                    float fadeDiff = globalDiff + fadeArea;
                    Color c = Color.Transparent;
                    for (int i = 0; i < OrigOuterPoints.Count; i++)
                    {
                        float vAngle = OrigOuterPoints[i].Angle();
                        float localDiff = Calc.AbsAngleDiff(vAngle, -MathHelper.PiOver2 * 3);
                        float l = 1;

                        if (localDiff < fadeDiff && localDiff > globalDiff)
                        {
                            //fade
                            l = (localDiff - globalDiff) / (fadeDiff - globalDiff);
                        }
                        else if (localDiff <= globalDiff)
                        {
                            //show
                            l = 0;
                        }
                        Vertices[index2].Color = Color.Lerp(Vertices[index2].Color, c, l);
                        Vertices[index2 + 1].Color = Color.Lerp(Vertices[index2 + 1].Color, c, l);
                        if (i == 0)
                        {
                            /*                            Vertices[^2].Color = Vertices[^4].Color = Vertices[^1].Color = Color.Lerp(Vertices[^2].Color, c, l);*/
                        }
                        index2 += 2;
                    }
                }
                Vertices[^1].Color = Vertices[^4].Color;
                Vertices[^2].Color = Vertices[0].Color;
            }
            public void AddFade(float value, float fadeAngleRegion, float solidAngleRegion, Color color, float lerpMult = 1)
            {
                float fadeArea = fadeAngleRegion * (1 - value);
                int index2 = 0;
                float globalDiff = value * (MathHelper.PiOver2 + 2f.ToRad());
                float fadeDiff = globalDiff + fadeArea;
                for (int i = 0; i < OrigOuterPoints.Count; i++)
                {
                    float vAngle = OrigOuterPoints[i].Angle();
                    float localDiff = Calc.AbsAngleDiff(vAngle, -MathHelper.PiOver2 * 3);
                    if (localDiff > globalDiff - (fadeArea + solidAngleRegion / 2) && localDiff < globalDiff + (fadeArea + solidAngleRegion / 2))
                    {
                        float l = 0;
                        if (localDiff > globalDiff - (solidAngleRegion / 2) && localDiff < globalDiff + (solidAngleRegion / 2))
                        {
                            l = 1;
                        }
                        else
                        {
                            l = 1 - Math.Abs(localDiff - globalDiff) / fadeArea;
                        }
                        l *= lerpMult;
                        Vertices[index2].Color = Color.Lerp(Vertices[index2].Color, color, l);
                        Vertices[index2 + 1].Color = Color.Lerp(Vertices[index2 + 1].Color, color, l);
                        if (i == 0)
                        {
                            Vertices[^2].Color = Vertices[^4].Color = Vertices[^1].Color = Color.Lerp(Vertices[^2].Color, color, l);
                        }
                    }
                    index2 += 2;
                }
            }
            public override void Awake(Scene scene)
            {
                base.Awake(scene);
                player = scene.GetPlayer();
            }
            public override void Update()
            {
                Position += shake;
                base.Update();
                UpdateVertices();
                if (sparkleMult > 0 && Scene.OnInterval(0.1f))
                {
                    for (int i = 0; i < 2; i++)
                    {
                        SpawnSparkle(sparkleMult);
                    }
                }

                Position -= shake;
            }
            private void SpawnSparkle(float mult)
            {
                ParticleSystem system = SceneAs<Level>().ParticlesBG;
                PSparkle.Size = 0.5f + Calc.Random.Range(0, 0.5f * mult);
                system.Emit(PSparkle, Center + Calc.AngleToVector(Calc.Random.NextAngle(), Calc.Random.Range(0, Width / 2 * (0.5f + mult * 0.5f))), Color.Red.RandomShade(0.3f));
            }
            //private Tween shieldBeginTween;
            public override void Render()
            {
                base.Render();
                if (ShieldAmount * ShieldScale.X * ShieldScale.Y * Scale.X * Scale.Y != 0)
                {
                    Draw.SpriteBatch.End();
                    PianoUtils.DrawUserPrimitives<VertexPositionColor>(SceneAs<Level>().Camera.Matrix, DirectRenderVertices);
                    GameplayRenderer.Begin();
                }
            }
            public void DirectRenderVertices(EffectPass pass)
            {
                foreach (TailParticle p in tailParticles)
                {
                    p.DirectRenderVertices(pass);
                }
                Engine.Instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, Vertices, 0, Vertices.Length, Indices, 0, Indices.Length / 3);
            }
            public ParticleType PSparkle = new ParticleType()
            {
                LifeMin = 0.7f,
                LifeMax = 0.8f,
                Size = 1,
                SpeedMin = 1,
                SpeedMax = 5,
                Friction = 10,
                FadeMode = ParticleType.FadeModes.Linear,
                Direction = -MathHelper.PiOver2,
                Acceleration = Vector2.UnitY * 20,
                SpinMin = MathHelper.TwoPi * 0.01f,
                SpinMax = MathHelper.TwoPi * 0.07f,
                SpinFlippedChance = true,
                ScaleOut = true,
                Source = GFX.Game["objects/PuzzleIslandHelper/shieldSparkle"],
                RotationMode = ParticleType.RotationModes.SameAsDirection
            };
            public class TailParticle : GraphicsComponent
            {
                private float intervalOffset;
                public ParticleType Particle = new ParticleType()
                {
                    Direction = -MathHelper.PiOver2,
                    DirectionRange = MathHelper.PiOver4,
                    FadeMode = ParticleType.FadeModes.Late,
                    LifeMin = 0.6f,
                    LifeMax = 1,
                    Size = 1,
                    SpeedMin = 5,
                    SpeedMax = 20,
                    Friction = 5,
                };
                public float RotationRate;
                public float YSpeed;
                public float XSpeed;
                public float FadeTime;
                public float AirFriction = 30f;
                public float Friction = 30f;
                public VertexPositionColor[] Vertices;
                public Vector2[] Offsets;
                public Color[] OrigColors;
                public Color[] ParticleColors = [Color.Red, Color.Orange, Color.DarkRed, Color.Pink, Color.Beige, Color.OrangeRed, Color.White];
                public float Alpha = 1;
                public float AlphaMult = 0.7f;
                private float origAlpha;
                private float timer;
                private Action onRemoved;
                public Vector2 OrigPosition;
                public TailParticle(VertexPositionColor[] vertices, float rotationRate, float ySpeed, float xSpeed, float fadeTime, Action onRemoved = null) : base(true)
                {
                    this.onRemoved = onRemoved;
                    RotationRate = rotationRate;
                    YSpeed = ySpeed;
                    XSpeed = xSpeed;
                    FadeTime = fadeTime;
                    timer = fadeTime;
                    Vertices = new VertexPositionColor[3];
                    Array.Copy(vertices, Vertices, 3);
                    Offsets = new Vector2[3];
                    OrigColors = new Color[3];
                    Visible = true;
                    for (int i = 0; i < vertices.Length; i++)
                    {
                        Alpha += vertices[i].Color.A;
                    }
                    Alpha /= vertices.Length;
                }
                public override void Added(Entity entity)
                {
                    base.Added(entity);
                    Vector2 center = Vertices.Center();
                    RenderPosition = center;
                    OrigPosition = center;
                    for (int i = 0; i < 3; i++)
                    {
                        Offsets[i] = Vertices[i].Position.XY() - center;
                        OrigColors[i] = Vertices[i].Color;
                    }
                    origAlpha = Alpha;
                    intervalOffset = Calc.Random.Range(0, 0.4f);
                }
                public Vector2 SpeedOffset;
                public override void Update()
                {
                    base.Update();
                    XSpeed = Calc.Approach(XSpeed, 0, Friction * Engine.DeltaTime);
                    YSpeed = Calc.Approach(YSpeed, 0, AirFriction * Engine.DeltaTime);
                    SpeedOffset += new Vector2(XSpeed * Engine.DeltaTime, YSpeed * Engine.DeltaTime);
                    Rotation += RotationRate;
                    Vector2 p = OrigPosition + SpeedOffset;
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 r = Calc.Rotate(Offsets[i], Rotation) * Scale;
                        Vertices[i].Position = new(p + r, 0);
                        Vertices[i].Color = OrigColors[i] * Alpha * AlphaMult;
                    }
                    if (Scene.OnInterval(0.4f, intervalOffset))
                    {
                        float alpha = 0;
                        foreach (var v in Vertices)
                        {
                            alpha += v.Color.A;
                        }
                        alpha /= Vertices.Length;
                        alpha *= Alpha * AlphaMult;
                        if (alpha > 0) SpawnSubParticle(alpha);
                    }
                    if (timer > 0)
                    {
                        timer -= Engine.DeltaTime;
                        if (timer <= 0) RemoveSelf();
                        else
                        {
                            Scale = Vector2.One * (timer / FadeTime);
                            Alpha = Calc.LerpClamp(0, origAlpha, timer / FadeTime);
                        }
                    }
                }
                private int side = 1;
                public void SpawnSubParticle(float alpha)
                {
                    Vector2 position = OrigPosition + SpeedOffset + Calc.Random.Range(3, 6) * Vector2.UnitX * side;
                    SceneAs<Level>().ParticlesBG.Emit(Particle, position, Calc.Random.Choose(ParticleColors) * alpha);
                    side *= -1;
                }
                public void DirectRenderVertices(EffectPass pass)
                {
                    Engine.Instance.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, Vertices, 0, 1);
                }
                public override void Removed(Entity entity)
                {
                    base.Removed(entity);
                    onRemoved?.Invoke();
                }
            }
            private class FlashStack : Component
            {
                private class flash
                {
                    public Color Color;
                    public float Timer;
                    public float Lerp;
                }
                public bool Empty => flashes.Count == 0;
                public Color Color { get; private set; }
                public float Lerp { get; private set; }
                private Stack<flash> flashes = [];
                public FlashStack() : base(true, false)
                {

                }
                public void AddFlash(Color color, float duration, float lerp)
                {
                    flashes.Push(new flash() { Color = color, Lerp = lerp, Timer = duration });
                }
                public override void Update()
                {
                    base.Update();
                    if (flashes.Count > 0)
                    {
                        flash atTop = flashes.Peek();
                        atTop.Timer -= Engine.DeltaTime;
                        if (atTop.Timer <= 0)
                        {
                            flashes.Pop();
                            Color = Color.Transparent;
                            Lerp = 0;
                        }
                        else
                        {
                            Color = atTop.Color;
                            Lerp = atTop.Lerp;
                        }
                    }

                }
            }
            private class Pulse : Component
            {
                public float LerpMult = 1;
                public float Duration;
                public float Intensity;
                public Ease.Easer Ease;
                public float Eased;
                private float timer;
                private Action onEnd;
                public Color Color;
                public float FadeAngleRegion;
                public float SolidAngleRegion;
                public Pulse(float duration, Ease.Easer ease, float fadeAreaRegion, float solidAreaRegion, Color color, Action onEnd) : base(false, false)
                {
                    FadeAngleRegion = fadeAreaRegion;
                    SolidAngleRegion = solidAreaRegion;
                    Color = color;
                    this.onEnd = onEnd;
                    Duration = duration;
                    Ease = ease;
                    Start();
                }
                public void Start()
                {
                    timer = Duration;
                    Active = true;
                    Eased = 0;
                }
                public void Stop()
                {
                    Active = false;
                }
                public override void Update()
                {
                    base.Update();
                    if (timer > 0)
                    {
                        Eased = Ease(1 - timer / Duration);
                        timer -= Engine.DeltaTime;
                        if (timer <= 0)
                        {
                            Eased = 1;
                            if (onEnd != null)
                            {
                                onEnd.Invoke();
                            }
                            RemoveSelf();
                        }
                    }
                }
            }
        }
    }

}
