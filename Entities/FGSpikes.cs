using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.GameplayEntities
{
    [CustomEntity("PuzzleIslandHelper/FGSpikesUp = LoadUp", "PuzzleIslandHelper/FGSpikesDown = LoadDown", "PuzzleIslandHelper/FGSpikesLeft = LoadLeft", "PuzzleIslandHelper/FGSpikesRight = LoadRight")]
    [Tracked(false)]
    public class FGSpikes : Entity
    {
        public static Entity LoadUp(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
        {
            entityData.Values["type"] = entityData.Attr("type", "default");
            return new FGSpikes(entityData, offset, Directions.Up);
        }

        public static Entity LoadDown(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
        {
            entityData.Values["type"] = entityData.Attr("type", "default");
            return new FGSpikes(entityData, offset, Directions.Down);
        }

        public static Entity LoadLeft(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
        {
            entityData.Values["type"] = entityData.Attr("type", "default");
            return new FGSpikes(entityData, offset, Directions.Left);
        }

        public static Entity LoadRight(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
        {
            entityData.Values["type"] = entityData.Attr("type", "default");
            return new FGSpikes(entityData, offset, Directions.Right);
        }
        public enum Directions
        {
            Up,
            Down,
            Left,
            Right
        }
        private readonly Directions direction;
        private Vector2 imageOffset;
        private readonly int size;
        private readonly string overrideType;
        private string spikeType;
        public FGSpikes(Vector2 position, int size, Directions dir, string type) : base(position)
        {
            Depth = -10001;
            direction = dir;
            this.size = size;
            overrideType = type;
            switch (direction)
            {
                case Directions.Up:
                    Collider = new Hitbox(size, 3f, 0f, -3f);
                    Add(new LedgeBlocker((_) => CheckGravity(inverted: false)));
                    break;
                case Directions.Down:
                    Collider = new Hitbox(size, 3f);
                    Add(new LedgeBlocker((_) => CheckGravity(inverted: true)));
                    break;
                case Directions.Left:
                    Collider = new Hitbox(3f, size, -3f);
                    Add(new LedgeBlocker());
                    break;
                case Directions.Right:
                    Collider = new Hitbox(3f, size);
                    Add(new LedgeBlocker());
                    break;
            }
            Add(new PlayerCollider(OnCollide));
            Add(new StaticMover
            {
                OnShake = OnShake,
                SolidChecker = IsRiding,
                JumpThruChecker = IsRiding,
            });
        }
        private void AddTentacle(float i)
        {
            Sprite sprite = GFX.SpriteBank.Create("tentacles");
            sprite.Play(Calc.Random.Next(3).ToString(), restart: true, randomizeFrame: true);
            Sprite sprite2 = sprite;
            Directions directions = direction;
            bool flag = (uint)directions <= 1u;
            sprite2.Position = (flag ? Vector2.UnitX : Vector2.UnitY) * (i + 0.5f) * 16f;
            sprite.Scale.X = Calc.Random.Choose(-1, 1);
            sprite.SetAnimationFrame(Calc.Random.Next(sprite.CurrentAnimationTotalFrames));
            if (direction == Directions.Up)
            {
                sprite.Rotation = -(float)Math.PI / 2f;
                sprite.Y++;
            }
            else if (direction == Directions.Right)
            {
                sprite.Rotation = 0f;
                sprite.X--;
            }
            else if (direction == Directions.Left)
            {
                sprite.Rotation = (float)Math.PI;
                sprite.X++;
            }
            else if (direction == Directions.Down)
            {
                sprite.Rotation = (float)Math.PI / 2f;
                sprite.Y--;
            }
            sprite.Rotation += (float)Math.PI / 2f;
            Add(sprite);
        }

        private void OnShake(Vector2 amount)
        {
            imageOffset += amount;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            AreaData areaData = AreaData.Get(scene);
            spikeType = areaData.Spike;
            if (!string.IsNullOrEmpty(overrideType) && !overrideType.Equals("default"))
            {
                spikeType = overrideType;
            }
            Directions directions = direction;
            string text = directions.ToString().ToLower();
            if (spikeType == "tentacles")
            {
                for (int i = 0; i < size / 16; i++)
                {
                    AddTentacle(i);
                }
                if (size / 8 % 2 == 1)
                {
                    AddTentacle(size / 16 - 0.5f);
                    return;
                }
            }
            else
            {
                List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures("danger/spikes/" + spikeType + "_" + text);
                for (int j = 0; j < size / 8; j++)
                {
                    Image image = new Image(Calc.Random.Choose(atlasSubtextures));
                    switch (direction)
                    {
                        case Directions.Up:
                            image.JustifyOrigin(0.5f, 1f);
                            image.Position = Vector2.UnitX * (j + 0.5f) * 8f + Vector2.UnitY;
                            break;
                        case Directions.Down:
                            image.JustifyOrigin(0.5f, 0f);
                            image.Position = Vector2.UnitX * (j + 0.5f) * 8f - Vector2.UnitY;
                            break;
                        case Directions.Left:
                            image.JustifyOrigin(1f, 0.5f);
                            image.Position = Vector2.UnitY * (j + 0.5f) * 8f + Vector2.UnitX;
                            break;
                        case Directions.Right:
                            image.JustifyOrigin(0f, 0.5f);
                            image.Position = Vector2.UnitY * (j + 0.5f) * 8f - Vector2.UnitX;
                            break;
                    }
                    Add(image);
                }
            }
        }
        public override void Render()
        {
            Vector2 position = Position;
            Position += imageOffset;
            base.Render();
            Position = position;
        }

        public void SetOrigins(Vector2 origin)
        {
            foreach (Component component in Components)
            {
                if (component is Image image)
                {
                    Vector2 vector = origin - Position;
                    image.Origin = image.Origin + vector - image.Position;
                    image.Position = vector;
                }
            }
        }

        private void OnCollide(Player player)
        {
            bool flag = GravityHelperImports.IsPlayerInverted?.Invoke() ?? false;
            switch (direction)
            {
                case Directions.Up:
                    if (!flag && player.Speed.Y >= 0f && player.Bottom <= Bottom || flag && player.Speed.Y <= 0f)
                    {
                        player.Die(new Vector2(0f, -1f));
                    }
                    break;
                case Directions.Down:
                    if (!flag && player.Speed.Y <= 0f || flag && player.Speed.Y >= 0f && player.Top >= Top)
                    {
                        player.Die(new Vector2(0f, 1f));
                    }
                    break;
                case Directions.Left:
                    if (player.Speed.X >= 0f)
                    {
                        player.Die(new Vector2(-1f, 0f));
                    }
                    break;
                case Directions.Right:
                    if (player.Speed.X <= 0f)
                    {
                        player.Die(new Vector2(1f, 0f));
                    }
                    break;
            }
        }

        private static int GetSize(EntityData data, Directions dir)
        {
            return dir > Directions.Down ? data.Height : data.Width;
        }

        private bool IsRiding(Solid solid)
        {
            return direction switch
            {
                Directions.Up => CollideCheckOutside(solid, Position + Vector2.UnitY),
                Directions.Down => CollideCheckOutside(solid, Position - Vector2.UnitY),
                Directions.Left => CollideCheckOutside(solid, Position + Vector2.UnitX),
                Directions.Right => CollideCheckOutside(solid, Position - Vector2.UnitX),
                _ => false,
            };
        }

        private bool IsRiding(JumpThru jumpThru)
        {
            return direction == Directions.Up && CollideCheck(jumpThru, Position + Vector2.UnitY);
        }
        private static bool CheckGravity(bool inverted)
        {
            return GravityHelperImports.IsPlayerInverted != null && GravityHelperImports.IsPlayerInverted() == inverted;
        }
        public FGSpikes(EntityData data, Vector2 offset, Directions dir)
        : this(data.Position + offset, GetSize(data, dir), dir, data.Attr("type", "default"))
        {

        }
    }
}