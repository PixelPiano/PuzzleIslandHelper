using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/Slope")]
    [Tracked]
    public class Slope : Entity
    {
        [TrackedAs(typeof(JumpThru))]
        public class SlopePlatform : JumpThru
        {
            public SlopePlatform(Vector2 position, int width, bool safe) : base(position, width, safe)
            {
            }
        }

        public Vector2 Lower;
        public Vector2 Upper;
        public float Percent;
        public SlopePlatform Platform;
        private float platformCollideTimer;

        public Slope(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            List<Vector2> array = [.. data.NodesWithPosition(offset).OrderByDescending(item => item.Y)];
            Lower = array[0];
            Upper = array[1];
            float left = Math.Min(Lower.X, Upper.X);
            float right = Math.Max(Lower.X, Upper.X);
            float top = Math.Min(Lower.Y, Upper.Y);
            float bottom = Math.Max(Lower.Y, Upper.Y);
            float width = right - left;
            float height = bottom - top;
            Position = new Vector2(left, top);
            Collider = new Hitbox(width, height);
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Platform = new SlopePlatform(Lower, 16, true);
            scene.Add(Platform);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if(scene.GetPlayer() is Player player)
            {
                if(player.OnGround() && player != Platform.GetPlayerRider())
                {
                    platformCollideTimer = 0.5f;
                }
            }
        }
        public override void Update()
        {
            base.Update();
            if (Scene is not Level level || level.GetPlayer() is not Player player) return;
            if (Platform.GetPlayerRider() == player)
            {
                if (Input.MoveY.Value > 0.9f && Math.Abs(Input.MoveX.Value) < 0.2f && Input.Jump.Pressed)
                {
                    Input.Jump.ConsumePress();
                    platformCollideTimer = 0.3f;
                }
            }
            if (platformCollideTimer > 0)
            {
                Platform.Collidable = false;
                platformCollideTimer -= Engine.DeltaTime;
                if (platformCollideTimer <= 0)
                {
                    platformCollideTimer = 0;
                    Platform.Collidable = true;
                }
            }
            float px = player.CenterX;
            Vector2 from = Lower;
            Vector2 to = Upper;
            float left = Math.Min(from.X, to.X);
            float right = Math.Max(from.X, to.X);
            float top = Math.Min(from.Y, to.Y);
            float bottom = Math.Max(from.Y, to.Y);
            float width = right - left;
            Vector2 p = new Vector2(left, top);
            Platform.X = Calc.Clamp(px, left, right) - 8;
            if (width != 0)
            {
                Percent = (Platform.X + 8 - p.X) / width;
            }
            if (from.X < to.X)
            {
                Platform.MoveToY(Calc.LerpClamp(bottom, top, Percent), 0);
            }
            else
            {
                Platform.MoveToY(Calc.LerpClamp(top, bottom, Percent), 0);
            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Platform?.RemoveSelf();
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Draw.Line(Platform.Position, Platform.Position + Vector2.UnitX * Platform.Width, Platform.Collidable ? Color.Lime : Color.DarkGreen, 3);
        }
        public override void Render()
        {
            base.Render();
            Draw.Line(Lower, Upper, Color.White, 2);
        }
    }
}
