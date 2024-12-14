using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
// PuzzleIslandHelper.ArtifactSlot
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/JumpBoostBlock")]
    [Tracked]
    public class JumpBoostBlock : Solid
    {
        private EntityID id;
        private char tileType;
        private float amount;
        private float target;
        private float speed;
        private float cooldownTimer;
        private float cooldownTime = 1.1f;
        private float jumpWindow;
        public JumpBoostBlock(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset, data.Width, data.Height, true)
        {
            tileType = data.Char("tiletype", '3');
            Depth = -12999;
            this.id = id;
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            TileGrid tileGrid;
            tileGrid = GFX.FGAutotiler.GenerateBox(tileType, (int)Width / 8, (int)Height / 8).TileGrid;
            Add(new LightOcclude());
            Add(tileGrid);
            Add(new TileInterceptor(tileGrid, highPriority: true));
            if (CollideCheck<Player>())
            {
                RemoveSelf();
            }
        }
        public override void Update()
        {
            base.Update();
            Player player = GetPlayerOnTop();
            if(jumpWindow > 0)
            {
                jumpWindow -= Engine.DeltaTime;
                if(player != null && player.InControl && Input.Jump.Pressed)
                {
                    player.Speed.Y -= 70f;
                }
            }
            else
            {
                jumpWindow = 0;
            }
            if (cooldownTimer > 0)
            {
                cooldownTimer -= Engine.DeltaTime;
                amount = Calc.LerpClamp(0f, 1f, cooldownTimer / cooldownTime);

            }
            else
            {
                cooldownTimer = 0;
                target = player != null ? 7f : 5f;
                amount = Calc.LerpClamp(0, 1f, speed * Engine.DeltaTime);
                speed = Calc.Approach(speed, target, 5f * Engine.DeltaTime);
                if (amount >= 1)
                {
                    cooldownTimer = cooldownTime;
                    jumpWindow = 0.2f;
                }
            }

        }
        public override void Render()
        {
            base.Render();
            Draw.Rect(Position + Vector2.UnitY * (1 - amount) * Height, Width, amount * Height, Color.Lime);
        }
    }
}