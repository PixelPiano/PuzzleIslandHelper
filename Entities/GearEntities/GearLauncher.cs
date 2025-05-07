using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using Celeste.Mod.Entities;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.GearEntities
{

    [CustomEntity("PuzzleIslandHelper/GearLauncher")]
    [TrackedAs(typeof(GearHolder))]
    public class GearLauncher : GearHolder
    {
        private float Force;
        private Vector2 direction;
        private float chargeTime;
        public enum Direction
        {
            Up, Down, Left, Right, UpLeft, UpRight, DownLeft, DownRight
        }
        public GearLauncher(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset - Vector2.One * 8, data.Bool("onlyOnce"), Color.Orange, id)
        {
            Force = data.Float("force", 50f);
            direction = data.Enum<Direction>("direction") switch
            {
                Direction.Up => -Vector2.UnitY,
                Direction.Down => Vector2.UnitY,
                Direction.Left => -Vector2.UnitX,
                Direction.Right => Vector2.UnitX,
                Direction.UpLeft => -Vector2.One,
                Direction.UpRight => new Vector2(1, -1),
                Direction.DownLeft => new Vector2(-1, 1),
                Direction.DownRight => Vector2.One,
                _ => Vector2.Zero
            };
            chargeTime = data.Float("chargeTime", 1);
            DropGear = false;
        }
        public override void Update()
        {
            base.Update();
        }
        public void Launch(Gear gear)
        {
            gear?.Launch(direction, Force * 6);
            StartShaking(0.2f);
        }
        public override void OnWindBack(Gear gear, bool drop)
        {
            HasGear = false;
            PreventRegrab(0.5f);
        }
        public override IEnumerator WhileSpinning(Gear gear)
        {
            rate = targetRotateRate;
            for (float i = 0; i < 1; i += Engine.DeltaTime / chargeTime)
            {
                if (gear is null || !gear.InSlot)
                {
                    break;
                }
                rate = Calc.LerpClamp(rate, Force / 5, Ease.SineIn(i));
                if (rate > 6)
                {
                    EmitSparks(rate / 2, 1);
                }
                yield return null;
            }
            float speed = GearSparks.SpeedMax;
            yield return Engine.DeltaTime * 2;
            Launch(gear);
            Rotation %= 360;
            
            for (float i = 0; i < 1; i += Engine.DeltaTime / 3)
            {
                rate = Calc.LerpClamp(rate, targetRotateRate, Ease.CubeOut(i));
                yield return null;
            }
            StopSpinning(true);
            yield return base.WhileSpinning(gear);
        }
    }
}