using Celeste.Mod.Entities;
using Celeste.Mod.FancyTileEntities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System.Collections;
using System.Collections.Generic;
using VivHelper.Colliders;
using System.IO;
using System;
using System.Linq;
using IL.Celeste;

// PuzzleIslandHelper.LabFallingBlock
namespace Celeste.Mod.PuzzleIslandHelper.Entities.GameplayEntities
{
    [CustomEntity("PuzzleIslandHelper/LabFallingBlock")]
    [Tracked]
    public class LabFallingBlock : Solid
    {
        private char TileType;
        private List<string> RandomList = new List<string>();
        private float RenderRotation;

        private string filename;

        private Player player;

        private string flag;

        public int RealWidth;
        public int RealHeight;

        private float RotationMult = 2;

        private float Gravity = 30f;

        private float Rotation = 0;

        private float addLerp = 0;
        private Vector2 TopBound;
        private Vector2 BottomBound;
        public List<Vector2> orig_Vectors = new();
        private int buffer;
        private Level l;
        private float OffsetY;
        private Grid Grid;
        private string TilesData;
        private char[,] TilePlacements;
        public List<Vector2> Vectors = new();
        public VirtualRenderTarget Block;
        private List<Vector2> Unordered = new();
        private bool Falling;
        public Rectangle AABB;
        private FancySolidTiles FancyBlock;
        private bool BreakOnImpact;
        private ParticleSystem particles;
        private bool DashCollided;
        private bool FromController;
        private bool Continue;
        private Color color = Color.White;
        private ParticleType Dust = new ParticleType
        {
            Source = GFX.Game["objects/PuzzleIslandHelper/particles/line00"],
            Size = 1f,
            Color = Color.Gray * 0.4f,
            Color2 = Color.White * 0.4f,
            ColorMode = ParticleType.ColorModes.Choose,
            LifeMin = 0.5f,
            LifeMax = 1.2f,
            Direction = (float)(Math.PI / 2f) * 3,
            SpeedMin = 10f,
            SpeedMax = 20f,
            SpeedMultiplier = 6f,
            FadeMode = ParticleType.FadeModes.Linear,
            RotationMode = ParticleType.RotationModes.SameAsDirection
        };


        public LabFallingBlock(Vector2 Position, Vector2 offset, string flag, char TileType, int Width, int Height, bool FromController)
           : base(Position + offset, Width, Height, false)
        {
            Collidable = false;
            this.FromController = FromController;
            filename = "ModFiles/PuzzleIslandHelper/RandomFancyBlocks";
            this.flag = flag;
            this.TileType = TileType;
            AddRandom();
            int randomIndex = new Random().Range(0, RandomList.Count);
            TilesData = RandomList[/*randomIndex*/8];
            EntityData BlockData = new EntityData
            {
                Name = "FancyTileEntities/FancySolidTiles",
                Position = Position,
            };
            BlockData.Values = new()
            {
                {"randomSeed", Calc.Random.Next()},
                {"blendEdges", true },
                {"width", Width },
                {"height", Height },
                {"tileData", TilesData }
            };
            FancyBlock = new FancySolidTiles(BlockData, offset, new EntityID());
            FancyBlock.Visible = false;
            Add(new LightOcclude());
            Add(new TileInterceptor(FancyBlock.Tiles, false));
            SurfaceSoundIndex = SurfaceIndex.TileToIndex[TileType];
            Collider = FancyBlock.Collider;
            FancyBlock.Collidable = false;
            Collidable = true;
            Add(new BeforeRenderHook(BeforeRender));
            OnDashCollide = OnDashed;
        }
        private void AppearParticles()
        {
            if (buffer <= 0)
            {
                particles.Emit(Dust, 1, FancyBlock.Center - new Vector2(0, Height / 2), new Vector2(Width / 2, Height / 4));
                buffer = 2;
            }
            buffer--;
        }
        private void GetCorners()
        {

            string[] strings = TilesData.Split('\n');
            int width = strings[0].Length - 1, height = strings.Length - 1;
            TilePlacements = new char[width, height];
            for (int y = 0; y < height; y++)
            {
                width = strings[y].Length - 1;
                for (int x = 0; x < width; x++)
                {
                    TilePlacements[x, y] = strings[y][x];
                }
            }
            bool[,] bools = new bool[TilePlacements.GetLength(0), TilePlacements.GetLength(1)];
            for (int i = 0; i < bools.GetLength(0); i++)
            {
                for (int j = 0; j < bools.GetLength(1); j++)
                {
                    bools[i, j] = TilePlacements[i, j] != '0';
                }
            }
            for (int i = 0; i < bools.GetLength(0); i++)
            {
                for (int j = 0; j < bools.GetLength(1); j++)
                {
                    SetCorner(bools, i, j);
                }
            }
            Vector2 sum = Vector2.Zero;
            foreach (Vector2 v in Unordered)
            {
                sum += v;
            }
            sum /= Unordered.Count;
            var angles = new Dictionary<Vector2, float>();
            foreach (Vector2 v in Unordered)
            {
                angles.Add(v, (float)((sum - v).Angle() + Math.PI * 2d - Math.PI * 2d));
            }

            SortedSet<Vector2> Ordered = new SortedSet<Vector2>(new AngleSorter(angles));
            for (int i = 0; i < Unordered.Count; i++)
            {
                Ordered.Add(Unordered[i]);
            }
            Vectors.AddRange(Ordered.ToList());
            orig_Vectors.AddRange(Vectors);
        }
        private void SetCorner(bool[,] bools, int x, int y)
        {
            int Cols = bools.GetLength(0);
            int Rows = bools.GetLength(1);
            bool XIsZero = x == 0;
            bool YIsZero = y == 0;
            bool CollideUp = YIsZero ? false : bools[x, y - 1];
            bool CollideDown = y == Rows - 1 ? false : bools[x, y + 1];
            bool CollideLeft = XIsZero ? false : bools[x - 1, y];
            bool CollideRight = x == Cols - 1 ? false : bools[x + 1, y];


            if (!bools[x, y])
            {
                return;
            }
            List<Vector2> Temp = new();

            if (!CollideUp)
            {
                Temp.Add(Position + new Vector2(x * 8, y * 8));

                Temp.Add(Position + new Vector2((x + 1) * 8, y * 8));
            }
            if (!CollideRight)
            {
                Temp.Add(Position + new Vector2((x + 1) * 8, y * 8));
                Temp.Add(Position + new Vector2((x + 1) * 8, (y + 1) * 8));
            }
            if (!CollideLeft)
            {
                Temp.Add(Position + new Vector2(x * 8, y * 8));

                Temp.Add(Position + new Vector2(x * 8, (y + 1) * 8));
            }
            if (!CollideDown)
            {
                Temp.Add(Position + new Vector2(x * 8, (y + 1) * 8));
                Temp.Add(Position + new Vector2((x + 1) * 8, (y + 1) * 8));
            }
            foreach (Vector2 v in Temp)
            {
                if (!Unordered.Contains(v))
                {
                    Unordered.Add(v);
                }
            }
        }
        private void SetAABB()
        {
            float Top = Vectors[0].Y, Bottom = Vectors[0].Y, Left = Vectors[0].X, Right = Vectors[0].X;
            foreach (Vector2 v in Vectors)
            {
                if (v.X < Left)
                {
                    Left = v.X;
                }
                if (v.X > Right)
                {
                    Right = v.X;
                }
                if (v.Y < Top)
                {
                    Top = v.Y;
                }
                if (v.Y > Bottom)
                {
                    Bottom = v.Y;
                }
            }
            AABB = new Rectangle((int)Left, (int)Top, (int)Right - (int)Left, (int)Bottom - (int)Top);
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Draw.HollowRect(AABB, Color.Yellow);

        }
        public static Vector2? DoRaycast(Grid grid, Vector2 start, Vector2 end)
        {

            start = (start - grid.AbsolutePosition) / new Vector2(grid.CellWidth, grid.CellHeight);
            end = (end - grid.AbsolutePosition) / new Vector2(grid.CellWidth, grid.CellHeight);
            Vector2 dir = Vector2.Normalize(end - start);
            int xDir = Math.Sign(end.X - start.X), yDir = Math.Sign(end.Y - start.Y);
            if (xDir == 0 && yDir == 0) return null;
            int gridX = (int)start.X, gridY = (int)start.Y;
            float nextX = xDir < 0 ? (float)Math.Ceiling(start.X) - 1 : xDir > 0 ? (float)Math.Floor(start.X) + 1 : float.PositiveInfinity;
            float nextY = yDir < 0 ? (float)Math.Ceiling(start.Y) - 1 : yDir > 0 ? (float)Math.Floor(start.Y) + 1 : float.PositiveInfinity;
            while (Math.Sign(end.X - start.X) != -xDir || Math.Sign(end.Y - start.Y) != -yDir)
            {
                if (grid[gridX, gridY])
                {
                    return grid.AbsolutePosition + start * new Vector2(grid.CellWidth, grid.CellHeight);
                }
                if (Math.Abs((nextX - start.X) * dir.Y) < Math.Abs((nextY - start.Y) * dir.X))
                {
                    start.Y += Math.Abs((nextX - start.X) / dir.X) * dir.Y;
                    start.X = nextX;
                    nextX += xDir;
                    gridX += xDir;
                }
                else
                {
                    start.X += Math.Abs((nextY - start.Y) / dir.Y) * dir.X;
                    start.Y = nextY;
                    nextY += yDir;
                    gridY += yDir;
                }
            }
            return null;
        }
        public override void Update()
        {

            base.Update();
            if (!Continue)
            {
                return;
            }
            if (Falling)
            {
                float Rate = Engine.DeltaTime * (Gravity * Calc.LerpClamp(0, 0.6f, addLerp));
                Gravity += 10;
                addLerp += Engine.DeltaTime;

                OffsetY += Rate;
                FancyBlock.Position.Y += Rate;

                Rotation %= MathHelper.TwoPi * 57f;
                RenderRotation %= MathHelper.TwoPi;
                Rotation += Engine.DeltaTime * 57f * RotationMult;

                RenderRotation += Engine.DeltaTime * RotationMult;
                for (int i = 0; i < Vectors.Count; i++)
                {
                    Vectors[i] = RotatePoint(orig_Vectors[i] + Vector2.UnitY * OffsetY, FancyBlock.Center, Rotation);
                }
                SetAABB();
                Collider = new PolygonCollider(Vectors.ToArray(), this, false);
                AppearParticles();
                if (Collide.Check(this, player) && player.StateMachine.State != Player.StDash && !DashCollided)
                {
                    player.Die(Vector2.Zero);
                }
                for (int i = 1; i < Vectors.Count; i++)
                {
                    Vector2 offset = new Vector2(0, -4);
                    Vector2? Ray = DoRaycast(Grid, Vectors[i - 1] + offset, Vectors[i] + offset);

                    if (Ray.HasValue)
                    {
                        Falling = false;
                        Break(false);
                        break;
                    }
                }
                if (AABB.Intersects(LabShutter.Bounds))
                {
                    Break(true);
                }
            }
            if (AABB.Y > SceneAs<Level>().Bounds.Bottom + RealHeight)
            {
                Remove();
            }

        }

        public void Remove()
        {
            FancyBlock.RemoveSelf();
            LFBController.CurrentBlockCount--;
            RemoveSelf();
        }
        private IEnumerator BreakRoutine(bool shouldBreak)
        {
            ImpactSfx();
            Falling = false;
            EmitDebris();
            if (shouldBreak)
            {

            }
            foreach (LabFallingBlock block in SceneAs<Level>().Tracker.GetEntities<LabFallingBlock>())
            {
                if (block != this)
                {
                    if (AABB.Intersects(block.AABB))
                    {
                        AddSmallDebris(5, true);
                        block.AddSmallDebris(5, true);
                        block.Remove();
                        Remove();
                    }
                }
            }
            /*            if (BreakOnImpact)
                        {
                            AddSmallDebris(7, true);
                            Remove();
                        }
                        else
                        {*/
            AddSmallDebris(3, true);
            StartShaking(0.2f);
            Add(new Coroutine(ColorLerp()));
            Depth = 1;
            Collidable = false;

            yield return null;
        }

        public LabFallingBlock(EntityData data, Vector2 offset)
          : this(data.Position, offset, data.Attr("flag"), data.Char("tiletype", '3'), data.Width, data.Height, false)
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            LFBController.CurrentBlockCount++;
            BreakOnImpact = new Random().Chance(0.5f);
            RotationMult = new Random().Range(-3f, 4f);
            Falling = true;
            scene.Add(particles = new ParticleSystem(0, 40));
            FancyBlock.Center = Center;
            scene.Add(FancyBlock);
            GetCorners();
            SetAABB();
            Grid = (scene as Level).SolidTiles.Grid;
            if (FromController)
            {
                Add(new Coroutine(SprinkleRocks(scene)));
            }
            else
            {
                Continue = true;
            }

        }
        private IEnumerator SprinkleRocks(Scene scene)
        {
            Level level = scene as Level;
            float W = AABB.Width;
            Vector2 InitialPosition = new Vector2(AABB.Left, level.Bounds.Top);
            for (int i = 0; i < 4; i++)
            {
                float xOffset = Calc.Random.Range(0, AABB.Width);
                level.Add(new GravityParticle(InitialPosition + new Vector2(xOffset, 0), Vector2.UnitY, Color.Lerp(Color.White, Color.Black, 0.7f)));
                int loops = Calc.Random.Range(0, 3);
                yield return Engine.DeltaTime * loops;
            }
            yield return 1;
            Continue = true;
            yield return null;
        }
        private void EmitDebris()
        {
            Vector2 Default = player is null ? Center : player.Center;
            Vector2 BlastLocation = Falling ? new Vector2(Center.X - Width, AABB.Y - Height) : Default;
            for (int i = 0; i < Width / 16f; i++)
            {
                for (int j = 0; j < Height / 16f; j++)
                {
                    Scene.Add(Engine.Pooler.Create<Debris>().Init(AABB.Center.ToVector2() - (Vector2.UnitX * 8 + Vector2.UnitX * AABB.Width / 2) + new Vector2(4 + i * 8, 4 + j * 8) - Vector2.UnitY * 16, TileType, true).BlastFrom(BlastLocation));
                }
            }
        }
        public class AngleSorter : IComparer<Vector2>
        {
            public Dictionary<Vector2, float> AnglesDict;

            public AngleSorter(Dictionary<Vector2, float> angles)
            {
                AnglesDict = angles;
            }

            int IComparer<Vector2>.Compare(Vector2 x, Vector2 y)
            {
                return Math.Sign(AnglesDict[y] - AnglesDict[x]);
            }
        }
        public override void Render()
        {
            base.Render();
            if (player is null || !Continue)
            {
                return;
            }
            Draw.SpriteBatch.Draw(
                Block, FancyBlock.Center + FancyBlock.Tiles.Position, null,
                color, RenderRotation,
                new Vector2(FancyBlock.Width / 2, FancyBlock.Height / 2),
                1, SpriteEffects.None, 0);
        }
        public void Break(bool shouldBreak)
        {
            Add(new Coroutine(BreakRoutine(shouldBreak)));
        }
        private void ImpactSfx()
        {
            if (TileType == '3')
            {
                Audio.Play("event:/game/01_forsaken_city/fallblock_ice_impact", new Vector2(AABB.Center.X, AABB.Top));
            }
            else if (TileType == '9')
            {
                Audio.Play("event:/game/03_resort/fallblock_wood_impact", new Vector2(AABB.Center.X, AABB.Top));
            }
            else if (TileType == 'g')
            {
                Audio.Play("event:/game/06_reflection/fallblock_boss_impact", new Vector2(AABB.Center.X, AABB.Top));
            }
            else
            {
                Audio.Play("event:/game/general/fallblock_impact", new Vector2(AABB.Center.X, AABB.Top));
            }
        }
        public void RenderAt(Vector2 position)
        {
            if (FancyBlock.Tiles.Alpha <= 0f)
            {
                return;
            }
            int tileWidth = FancyBlock.Tiles.TileWidth;
            int tileHeight = FancyBlock.Tiles.TileHeight;
            Color color = FancyBlock.Tiles.Color * FancyBlock.Tiles.Alpha;
            Vector2 position2 = new Vector2(position.X, position.Y);
            for (int i = 0; i < FancyBlock.Tiles.Tiles.Columns; i++)
            {
                for (int j = 0; j < FancyBlock.Tiles.Tiles.Rows; j++)
                {
                    MTexture mTexture = FancyBlock.Tiles.Tiles[i, j];
                    if (mTexture != null)
                    {
                        Draw.SpriteBatch.Draw(mTexture.Texture.Texture_Safe, position2, mTexture.ClipRect, color);
                    }

                    position2.Y += tileHeight;
                }

                position2.X += tileWidth;
                position2.Y = position.Y;
            }
        }
        private IEnumerator ColorLerp()
        {
            for (float i = 0; i < 0.3f; i += Engine.DeltaTime)
            {
                color = Color.Lerp(Color.White, Color.Black, i);
                yield return null;
            }
            yield return null;
        }
        public override void OnShake(Vector2 amount)
        {
            base.OnShake(amount);
            FancyBlock.Tiles.Position += amount;
        }
        private void BeforeRender()
        {
            if (Scene as Level is null)
            {
                return;
            }
            l = Scene as Level;
            EasyRendering.DrawToObject(Block, () => RenderAt(FancyBlock.Tiles.Position), l, clear: true, useIdentity: true);
        }
        public void AddSmallDebris(int amount, bool HighYSpeed, Vector2? DashDir = null)
        {
            Vector2 SpeedMult = new Vector2(4, 0.5f);
            for (int i = 0; i < amount; i++)
            {
                float SpeedX = Calc.Random.Range(-50, 50);
                float SpeedY = Calc.Random.Range(-100, -50);
                float RandX = Calc.Random.Range(-Width / 2, Width / 2);
                float RandY = Calc.Random.Range(8, 24);
                if (HighYSpeed)
                {
                    SpeedY += 70;
                }
                if (DashDir.HasValue)
                {
                    if (DashDir.Value.X != 0)
                    {
                        SpeedX *= SpeedMult.X * DashDir.Value.X;
                    }
                    if (DashDir.Value.Y != 0)
                    {
                        SpeedY *= SpeedMult.Y * DashDir.Value.Y;
                    }
                }
                Scene.Add(new GravityParticle(FancyBlock.Center - new Vector2(RandX, RandY), new Vector2(SpeedX, SpeedY), Color.Gray));
            }
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            player = SceneAs<Level>().Tracker.GetEntity<Player>();
            Block = VirtualContent.CreateRenderTarget("FancyBlock", (int)FancyBlock.Width, (int)FancyBlock.Height);
        }
        private DashCollisionResults OnDashed(Player player, Vector2 direction)
        {
            DashCollided = true;
            EmitDebris();
            ImpactSfx();
            AddSmallDebris(10, false, new Vector2?(direction));
            Remove();
            return DashCollisionResults.Rebound;
        }
        private void AddRandom()
        {
            string content = Everest.Content.TryGet(filename, out var asset) ? ReadModAsset(asset) : null;
            string[] array = content.Split('\n');
            string toAdd = "";
            foreach (string s in array)
            {
                if (string.IsNullOrWhiteSpace(s))
                {
                    if (!string.IsNullOrWhiteSpace(toAdd))
                    {
                        toAdd = toAdd.Replace('1', TileType);
                        RandomList.Add(toAdd);
                    }
                    toAdd = "";
                    continue;
                }
                RealWidth = s.Length * 8;
                toAdd += s + '\n';
            }
            RealHeight = array.Length * 8;
        }
        public static string ReadModAsset(string filename)
        {
            return Everest.Content.TryGet(filename, out var asset) ? ReadModAsset(asset) : null;
        }
        public static string ReadModAsset(ModAsset asset)
        {
            using var reader = new StreamReader(asset.Stream);

            return reader.ReadToEnd();
        }
        static Vector2 RotatePoint(Vector2 pointToRotate, Vector2 centerPoint, double angleInDegrees)
        {
            double angleInRadians = angleInDegrees * (Math.PI / 180);
            double cosTheta = Math.Cos(angleInRadians);
            double sinTheta = Math.Sin(angleInRadians);
            return new Vector2
            {
                X =
                    (int)
                    (cosTheta * (pointToRotate.X - centerPoint.X) -
                    sinTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.X),
                Y =
                    (int)
                    (sinTheta * (pointToRotate.X - centerPoint.X) +
                    cosTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.Y)
            };
        }
    }
}