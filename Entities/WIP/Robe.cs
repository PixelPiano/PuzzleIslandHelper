using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
namespace Celeste.Mod.PuzzleIslandHelper.Entities.WIP
{
    [CustomEntity("PuzzleIslandHelper/Robe")]
    [Tracked]
    public class Robe : Actor
    {

        public const float WalkSpeed = 64f;
        public const float RunSpeed = 128f;
        public const float Gravity = 900f;
        public Sprite Sprite;
        public Vector2 Speed;
        public Facings Facing = Facings.Right;
        public Robe(EntityData data, Vector2 offset) : this(data.Position + offset) { }
        public Robe(Vector2 position) : base(position)
        {
            Depth = 1;
            Sprite = new Sprite(GFX.Game, "characters/PuzzleIslandHelper/Robe/");
            Sprite.AddLoop("idle", "idle", 0.2f);
            Sprite.AddLoop("walk", "walk", 0.1f);
            Sprite.AddLoop("run", "walk", 0.05f);
            Add(Sprite);
            Sprite.Play("idle");
            Collider = new Hitbox(13, 14, 11, 18);
            Position.Y -= 10;
            Position.X -= 6;
        }
        public void Run(int direction)
        {
            Speed.X = RunSpeed * Math.Sign(direction);
            Sprite.Play("run");
        }
        public void Stop(Facings facing = Facings.Left)
        {
            Facing = facing;
            Speed.X = 0;
            Sprite.Play("idle");
        }
        public void Walk(int direction)
        {
            Speed.X = WalkSpeed * Math.Sign(direction);
            Sprite.Play("walk");
        }
        public override void Update()
        {
            base.Update();
            Sprite.FlipX = Facing == Facings.Left;
            MoveH(Speed.X * Engine.DeltaTime);
            MoveV(Speed.Y * Engine.DeltaTime);
            Speed.Y = Calc.Approach(Speed.Y, Gravity, Gravity * Engine.DeltaTime);

        }

    }
}