using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Flora
{
    [CustomEntity("PuzzleIslandHelper/Statid")]
    [Tracked]
    public class Statid : Entity
    {
        public class Data
        {
            public bool HasSap;
            public bool IsSapped;
        }
        public class Petal : Component
        {
            public Vector2[] PetalPoints = new Vector2[] { new(-0.15f, 0), new(0.15f, 0), new(-0.25f, -0.5f), new(0.25f, -0.5f), new(0, -1) };
            public int[] PetalIndices = new int[] { 1, 0, 2, 2, 3, 1, 4, 3, 2 };
            public float[] RotMult = new float[] { 0.2f, 0.2f, 0.5f, 0.5f, 1 };
            public VertexPositionColor[] Vertices;
            public Vector2 Scale;
            private float startRotation;
            public float Rotation;
            public Vector2 RenderPosition
            {
                get
                {
                    return Parent.StemConnect + Position;
                }
                set
                {
                    Position = value - Parent.StemConnect;
                }
            }
            public Vector2 Position;
            public float SwayAmount;
            public Statid Parent;
            public bool Digital => Parent.Digital;
            public Petal(Statid parent, float rotation, Vector2 scale) : base(true, true)
            {
                Parent = parent;
                startRotation = rotation;
                Scale = scale;
                Vertices = new VertexPositionColor[PetalPoints.Length];
                for (int i = 0; i < PetalPoints.Length; i++)
                {
                    Vertices[i] = new VertexPositionColor(new Vector3(PetalPoints[i] * Scale, 0), Color.White);
                }
            }
            public override void Added(Entity entity)
            {
                base.Added(entity);
            }
            public override void Update()
            {
                base.Update();
                UpdateVertices();
            }

            public void UpdateVertices()
            {
                if (Parent.Digital) return;
                Vector2 p = RenderPosition;
                for (int i = 0; i < Vertices.Length; i++)
                {
                    Vertices[i].Position = new Vector3(p + PianoUtils.RotateAroundDeg(PetalPoints[i] * Scale, Vector2.Zero, startRotation + Rotation), 0);
                }
            }
            public void DrawPetal(Matrix matrix)
            {
                GFX.DrawIndexedVertices(matrix, Vertices, 5, PetalIndices, 3);
            }
        }
        public Petal[] Petals;
        public bool Digital;
        public Data data;
        public const float BaseAngle = -90;
        public const float MaxAngleDiff = 35;
        public Color AliveColor;
        public bool Dead;

        public int PetalCount;
        public int Direction;
        private int scaleRange;
        private int bulbSize;
        public float Angle;
        public float AngleOffset;
        public float MaxAngleOffset;
        public float Mult;
        public float TargetMult;
        public bool CanProduceSap;
        public bool Sleeping;
        public bool IsInView;
        public Vector2 Orig;
        public Vector2 Ground;
        public Vector2 StemConnect;
        public Vector2 PetalScale;
        public EntityID ID;
        public int Thickness = 1;
        private VirtualRenderTarget PetalTarget;
        private Ease.Easer ease;
        public IEnumerable<Entity> CollidingActors;
        public Vector2 GroundOffset;
        public float HeightOffset;
        private float heightOffsetRate;
        private float heightOffsetTarget;
        public float bulbOffset;
        private float turnRate;
        private bool playerColliding;
        private bool playerWasColliding;
        private Color DeadColor;
        public Color Color;
        public bool HasSap
        {
            get => data.HasSap;
            set => data.HasSap = value;
        }
        public bool IsSapped
        {
            get => data.IsSapped;
            set => data.IsSapped = value;
        }
        public HashSet<Firfil> Firfils = [];
        public bool Single;
        public float PlayerCollidingTimer = 3f;
        public string ThoughtPath;
        public FlagList FlagOnSapped;
        public Statid(EntityData data, Vector2 offset, EntityID id) : this(data.Position + offset, data.Int("petals"), data.Bool("digital"), Vector2.One * 4, 0, id, data.Int("depth", 2), data.HexColor("color"), data.HexColor("shadeColor"), data.Bool("dead"))
        {
            Single = true;
            ThoughtPath = data.Attr("thoughtPath");
            FlagOnSapped = data.FlagList("flagOnSapped");
            CanProduceSap = data.Bool("canProduceSap");
        }
        public Statid(Vector2 position, int petals, bool digital, Vector2 petalScale, int scaleRange, EntityID id, int depth, Color aliveColor, Color shadeColor, bool dead, bool modifyColorBasedOnDepth = true) : base(position)
        {
            int y = (int)Y;
            int x = (int)X;
            if (x == 0) x = 1;
            if (y == 0) y = 1;
            Random rand = new Random(x + y + (y * x) - (x * x));
            Color.Lerp(Color.White, Color.Black, depth < 0 ? 0 : rand.Range(0.2f, 0.5f));
            Tag |= Tags.TransitionUpdate;
            ID = id;
            Digital = digital;
            Depth = depth;
            AliveColor = aliveColor;
            if (modifyColorBasedOnDepth)
            {
                AliveColor = Color.Lerp(aliveColor, shadeColor, depth < 0 ? 0 : rand.Range(0.2f, 0.5f));
            }
            Dead = dead;
            ease = Ease.Follow(Ease.SineIn, Ease.BackOut);
            PetalScale = petalScale;
            this.scaleRange = scaleRange;
            Orig = Position;
            PetalCount = petals;
            Add(new PlayerCollider(OnPlayer));
        }
        public void OnPlayer(Player player)
        {
            playerColliding = true;
            if (PlayerCollidingTimer > 0)
            {
                PlayerCollidingTimer -= Engine.DeltaTime;
            }
            if (!playerWasColliding)
            {
                float speed = (player.Speed.X) * Engine.DeltaTime * 0.06f;
                TargetMult = Calc.Min(TargetMult + speed, 1);
            }
        }
        public bool InView()
        {
            Camera camera = (Scene as Level).Camera;
            float xPad = Width;
            float yPad = Height;
            if (X < camera.X - xPad || Y < camera.Y - yPad || X > camera.X + 320f + xPad || Y > camera.Y + 180f + yPad)
            {
                return false;
            }
            return true;
        }
        public void ChanceEmit(Player player)
        {
            if (Calc.Random.Chance((int)(player.Speed.Length() / 40) * 0.025f))
            {
                Vector2 position = Ground + GroundOffset + Calc.AngleToVector(Angle, Height + HeightOffset + 2);
                float speedMult = Dead ? 0.4f : 1;
                float lifeMult = Dead ? 0.4f : 1;
                WavyParticle p = new WavyParticle(position)
                {
                    MinSpeed = 20f * speedMult,
                    MaxSpeed = 50f * speedMult,
                    MinOffset = 4f,
                    MaxOffset = 16f,
                    MinLife = 1f,
                    MaxLife = 5f * lifeMult,
                    Color = Dead ? Color.Gray.RandomShade(0.3f) : player.Hair.Color,
                    Color2 = Color.Black,
                    DownFirst = Calc.Random.Chance(0.5f),
                    Dir = -Vector2.UnitY,
                    Friction = 1f,
                    Acceleration = player.Speed * 0.005f,
                    Size = Calc.Random.Choose(1, 2),
                    OffsetMode = WavyParticle.OffsetModes.ChangeEveryHalfWave,
                    ColorMode = WavyParticle.ColorModes.FadeFromMiddle,
                    FadeMode = WavyParticle.FadeModes.Linear,

                };
                Scene.Add(p);
            }
        }
        public void InactiveUpdate()
        {
            bulbOffset = 0;
            HeightOffset = 0;
        }
        private class loneAscwiitCutscene : CutsceneEntity
        {
            private Statid statid;
            private Ascwiit bird;
            public loneAscwiitCutscene(Statid statid, Ascwiit bird) : base()
            {
                this.statid = statid;
                this.bird = bird;
            }
            public override void OnBegin(Level level)
            {
                Add(new Coroutine(ascwiitEatSequence()));
            }
            private IEnumerator ascwiitEatSequence()
            {
                Level.GetPlayer()?.DisableMovement();
                yield return bird.EatSap(statid);
                Camera camera = Level.Camera;
                float y = bird.Y;
                float camY = camera.Y;
                for (float i = 0; i < 1; i += Engine.DeltaTime / 6)
                {
                    camera.Y = Calc.LerpClamp(camY, Level.Bounds.Top, Ease.SineInOut(i));
                    yield return null;
                }
                yield return 4f;
                float from = camera.Y;
                for (float i = 0; i < 1; i += Engine.DeltaTime / 2)
                {
                    camera.Y = Calc.LerpClamp(from, camY, Ease.SineInOut(i));
                    yield return null;
                }
                yield return 0.5f;
                EndCutscene(Level);
            }
            public override void OnEnd(Level level)
            {
                level.GetPlayer()?.EnableMovement();
                if (WasSkipped)
                {
                    PianoModule.Session.AscwiitsWithFirfils.Add(bird.id);
                    bird.Gravity = Ascwiit.NormalGravity;
                    bird.FirfilEated = true;
                    bird.PathFlag.State = true;
                    statid.HasSap = false;
                    statid.IsSapped = true;
                    if (bird.Persistent)
                    {
                        Level.Session.DoNotLoad.Add(bird.id);
                    }
                    bird.RemoveSelf();
                }
            }
        }
        private class ascwiitSapCutscene : CutsceneEntity
        {
            public Ascwiit Bird;
            public Statid Flower;
            private Entity collisionChecker;
            private bool removeBird;
            public ascwiitSapCutscene(Ascwiit bird, Statid flower) : base()
            {
                Bird = bird;
                Flower = flower;
            }
            public override void Added(Scene scene)
            {
                base.Added(scene);
                scene.Add(collisionChecker = new Entity());
                collisionChecker.Collider = new Hitbox(1, 1);
            }
            public override void OnBegin(Level level)
            {
                if (Bird == null)
                {
                    removeBird = true;
                    Vector2 p = level.Camera.Position;
                    List<float> possibleSpawns = [];
                    for (int x = 0; x < 320; x++)
                    {
                        if (!Scene.CollideCheck<Solid>(p + Vector2.UnitX * x, Flower.TopCenter))
                        {
                            possibleSpawns.Add(p.X + x);
                        }

                    }
                    if (possibleSpawns.Count > 0)
                    {
                        float spawnX = possibleSpawns.Random();
                        float spawnY = level.Camera.Y - 8;
                        Bird = new Ascwiit(new Vector2(spawnX, spawnY), Ascwiit.StFlyTo);
                    }
                    else
                    {
                        RemoveSelf();
                    }
                }
                level.GetPlayer()?.DisableMovement();
                Add(new Coroutine(cutscene()));
            }
            private IEnumerator cutscene()
            {
                Vector2 position = Bird.Position;
                Bird.StateMachine.State = Ascwiit.StDummy;
                Bird.DummyGravity = false;
                Bird.DummyFlap = false;
                Bird.DummyPeck = true;
                yield return Bird.EatSap(Flower);
                yield return Bird.FlyToRoutine(position);
                if (removeBird)
                {
                    Bird.RemoveSelf();
                }
                EndCutscene(Level);
            }
            public override void OnEnd(Level level)
            {
                Bird.RemoveSelf();
                level.GetPlayer()?.EnableMovement();
            }
            public override void Removed(Scene scene)
            {
                base.Removed(scene);
                collisionChecker.RemoveSelf();
                if (removeBird)
                {
                    Bird?.RemoveSelf();
                }
            }
        }
        private IEnumerator ascwiitEatSequence(Ascwiit bird)
        {
            Vector2 position = bird.Position;
            int state = bird.State;
            yield return bird.EatSap(this);
            if (bird.PathFlag)
            {
                bird.StateMachine.State = Ascwiit.StPath;
            }
            else
            {
                yield return bird.FlyToRoutine(position, state);
            }
        }
        public bool Occupied;
        public override void Update()
        {
            Color = Dead ? DeadColor : AliveColor;
            if (HasSap)
            {
                Color = Color.Yellow;
            }
            if (Sleeping)
            {
                InactiveUpdate();
                return;
            }
            IsInView = InView();
            if (!IsInView)
            {
                InactiveUpdate();
                return;
            }

            if (Scene.GetPlayer() is Player player)
            {
                if (HasSap && !IsSapped && !Occupied)
                {
                    Camera camera = SceneAs<Level>().Camera;
                    bool foundBaby = false;
                    if (Left > camera.X + 30 && Right < camera.X + 290)
                    {
                        List<Entity> birds = Scene.Tracker.GetEntities<Ascwiit>();
                        if (birds.Count == 1 && (birds[0] as Ascwiit).Scale < 1 && !(birds[0] as Ascwiit).FirfilEated)
                        {
                            foundBaby = string.IsNullOrEmpty(ThoughtPath);
                        }
                        if (foundBaby)
                        {
                            Scene.Add(new loneAscwiitCutscene(this, birds[0] as Ascwiit));
                            Occupied = true;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(ThoughtPath))
                            {
                                Scene.Add(new ascwiitSapCutscene(null, this));
                                Occupied = true;
                            }
                        }
                        if (Occupied && !FlagOnSapped.Empty)
                        {
                            FlagOnSapped.State = true;
                        }
                    }
                }
                if (MathHelper.Distance(CenterY, player.CenterY) < Height * 1.5f)
                {
                    float dist = Vector2.DistanceSquared(player.Center + player.Speed * 2 * Engine.DeltaTime, Center);
                    if (dist < 1600)
                    {
                        if (dist < 64)
                        {
                            ChanceEmit(player);
                        }
                        if (!Dead)
                        {
                            bulbOffset = Calc.Approach(bulbOffset, 2 * Math.Sign(player.CenterX - CenterX), Engine.DeltaTime * turnRate);
                            HeightOffset = Calc.Approach(HeightOffset, heightOffsetTarget, heightOffsetRate);
                        }
                        else
                        {
                            bulbOffset = 0;
                            HeightOffset = 0;
                        }
                    }
                }
                else
                {
                    if (Dead)
                    {
                        bulbOffset = 0;
                        HeightOffset = 0;
                    }
                    else
                    {
                        bulbOffset = Calc.Approach(bulbOffset, 0, Engine.DeltaTime);
                        HeightOffset = Calc.Approach(HeightOffset, 0, heightOffsetRate);
                    }
                }
            }
            if (!Digital)
            {
                float ease = this.ease(Mult);
                AngleOffset = Calc.Approach(AngleOffset, MaxAngleDiff * ease, (5 + 15 * ease));
            }

            Mult = Calc.Approach(Mult, TargetMult, Engine.DeltaTime);
            Angle = (BaseAngle + AngleOffset).ToRad();
            TargetMult = Calc.Approach(TargetMult, 0, Engine.DeltaTime * 1.2f);
            StemConnect = Ground + Calc.AngleToVector(Angle, Height);

            playerWasColliding = playerColliding;
            if (!playerColliding)
            {
                PlayerCollidingTimer = 3f;
            }
            playerColliding = false;
            base.Update();

        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (Single)
            {
                if (PianoModule.Session.StatidPatchData.TryGetValue(ID, out var value))
                {
                    data = value[0];
                }
                else
                {
                    data = new Data()
                    {
                        HasSap = false,
                        IsSapped = false
                    };
                    PianoModule.Session.StatidPatchData.Add(ID, [data]);
                }
            }
            if (data == null)
            {
                RemoveSelf();
                return;
            }
            DeadColor = Color.Gray.RandomShade(0.4f);
            turnRate = Calc.Random.Range(5f, 7f);
            heightOffsetTarget = Calc.Random.Range(0, 1f) * 7;
            heightOffsetRate = Calc.Random.Range(4, 10) * Engine.DeltaTime;
            bulbSize = Calc.Random.Chance(0.1f) ? 0 : Calc.Random.Range(2, 4);
            Thickness = Calc.Random.Choose(1, 2);
            if (!this.HasGroundBelow(out Ground))
            {
                RemoveSelf();
                return;
            }
            Ground.Y++;
            Collider = new Hitbox(1, 1);
            if (!CollideCheck<Solid>(Ground + Vector2.One))
            {
                Ground.X -= 2;
                Position.X -= 2;
            }
            else if (!CollideCheck<Solid>(Ground + new Vector2(-1, 1)))
            {
                Ground.X += 2;
                Position.X += 2;
            }
            Collider = new Hitbox(8, Ground.Y - Orig.Y, -4);

            /*Petals = new Petal[PetalCount];

            for (int i = 0; i < PetalCount; i++)
            {
                Vector2 scale = new Vector2(4 + Calc.Random.Range(-scaleRange, scaleRange), 4 + Calc.Random.Range(-scaleRange, scaleRange));
                Petals[i] = new Petal(this, 360f / PetalCount * i, scale);
                Petals[i].Visible = false;
            }
            Add(Petals);

            float left = int.MaxValue;
            float right = int.MinValue;
            float top = int.MaxValue;
            float bottom = int.MinValue;
            foreach (Petal p in Petals)
            {
                foreach (VertexPositionColor vertice in p.Vertices)
                {
                    left = Calc.Min(vertice.Position.X, left);
                    right = Calc.Max(vertice.Position.X, right);
                    top = Calc.Min(vertice.Position.Y, top);
                    bottom = Calc.Max(vertice.Position.Y, bottom);
                }
            }
            int width = (int)(right - left);
            int height = (int)(bottom - top);
            PetalTarget = VirtualContent.CreateRenderTarget("StatidPetalTarget", width, height);*/
        }
        /*        private bool drewOnce;

                public void BeforeRender()
                {
                    if (drewOnce) return;
                    PetalTarget.SetRenderTarget(null);
                    foreach (Petal p in Petals)
                    {
                        p.DrawPetal(Matrix.Identity);
                    }

                    drewOnce = true;
                }*/
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            PetalTarget?.Dispose();
            PetalTarget = null;
        }
        public override void Render()
        {
            if (Sleeping || !IsInView) return;
            if (Digital)
            {
                Draw.LineAngle(Ground + GroundOffset, Angle, Height + HeightOffset, Color, Thickness);
                DrawPoint(Ground + GroundOffset + Calc.AngleToVector(Angle, Height + HeightOffset + 2) + Vector2.UnitX * (-bulbSize + bulbOffset), Color, bulbSize);
            }
            /*            else
                        {
                            Draw.LineAngle(Ground + GroundOffset, Angle, Height + HeightOffset, Color, Thickness);
                            //Draw.SpriteBatch.Draw(PetalTarget, Position, Color);
                        }*/
        }
        public void DrawPoint(Vector2 position, Color color, int size)
        {
            Draw.SpriteBatch.Draw(Draw.Pixel.Texture.Texture_Safe, position, Draw.Pixel.ClipRect, color, 0f, Vector2.Zero, size, SpriteEffects.None, 0f);
        }
    }
}
