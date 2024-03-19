using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using Celeste.Mod.Entities;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{

    [CustomEntity("PuzzleIslandHelper/GearMover")]
    [TrackedAs(typeof(GearHolder))]
    public class GearMover : GearHolder
    {
        private Vector2 start;
        private Vector2 end;
        private Vector2 target;
        private float MaxSpeed;
        private float acceleration;
        public GearMover(EntityData data, Vector2 offset) : base(data.Position + offset - new Vector2(8),  data.Bool("onlyOnce"), Color.Blue)
        {
            MaxSpeed = data.Float("maxSpeed", 50f);
            acceleration = data.Float("acceleration");
            start = Position;
            end = data.NodesWithPosition(offset)[1] - new Vector2(8);
            target = end;
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            RenderAt(end, Color.Red);
        }
        public void RenderAt(Vector2 position, Color color)
        {
            MTexture tex = GFX.Game["objects/PuzzleIslandHelper/Gear/holder"];
            Vector2 offset = new Vector2(tex.Width / 2, tex.Height / 2);
            Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position + offset, null, color, Rotation.ToRad(), offset, 1, SpriteEffects.None, 0);
        }
        public override void StopSpinning(bool drop = true)
        {
            base.StopSpinning(drop);
            target = target == start ? end : start;
            SpinDirection = -SpinDirection;
        }
        public override IEnumerator WhileSpinning(Gear gear)
        {
            float speedLerp = 0;
            while (Position != target)
            {
                if (!gear.InSlot)
                {
                    break;
                }
                Position = Calc.Approach(Position, target, MaxSpeed * Engine.DeltaTime * speedLerp);
                speedLerp = Calc.Min(1, speedLerp + Engine.DeltaTime * acceleration);
                yield return null;
            }
            StopSpinning();
            yield return base.WhileSpinning(gear);
        }
    }
}