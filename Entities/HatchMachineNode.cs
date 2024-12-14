using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/HatchMachineNode")]
    [Tracked]
    public class HatchMachineNode : Entity
    {
        private const string path = "objects/PuzzleIslandHelper/machines/hatchMachine/";
        public class Light : GraphicsComponent
        {
            private MTexture glass => GFX.Game[path + "nodeGlass"];
            private MTexture fill => GFX.Game[path + "nodeFill"];
            public Light(Vector2 position) : base(true)
            {
                Position = position;
            }
            public void SetColor(Color color)
            {
                Color = color;
            }
            public override void Render()
            {
                Vector2 p = RenderPosition;
                fill?.Draw(p, Vector2.Zero, Color, Scale, Rotation, Effects);
                glass?.Draw(p, Vector2.Zero, Color.White, Scale, 0, Effects);
            }
        }
        private Light light;
        private Image node;
        private int index;
        private string flag => "HatchMachineLights" + index;
        private char tiletype;
        private Block block;
        private float shakeTimer;
        private Vector2 shakeVector;
        [TrackedAs(typeof(DashBlock))]
        public class Block : DashBlock
        {
            public HatchMachineNode Parent;
            public Block(HatchMachineNode parent, Vector2 position, char tiletype) : base(position, tiletype, 16, 16, false, true, true, new EntityID(Guid.NewGuid().ToString(), 0))
            {
                Parent = parent;
                Tag |= Tags.TransitionUpdate;
            }
            public void ActivateParent()
            {
                Parent?.Activate();
            }
            [OnLoad]
            public static void Load()
            {
                On.Celeste.DashBlock.OnDashed += DashBlock_OnDashed;
            }
            [OnUnload]
            public static void Unload()
            {
                On.Celeste.DashBlock.OnDashed -= DashBlock_OnDashed;
            }
            private static DashCollisionResults DashBlock_OnDashed(On.Celeste.DashBlock.orig_OnDashed orig, DashBlock self, Player player, Vector2 direction)
            {
                if (self is Block block)
                {
                    block.ActivateParent();
                }
                return orig(self, player, direction);
            }
        }
        public HatchMachineNode(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Depth = 1;
            index = data.Int("index");
            tiletype = data.Char("tiletype");
            Tag |= Tags.TransitionUpdate;
        }
        public void Activate()
        {
            flag.SetFlag(true);
            shakeTimer = 0.3f;
        }
        public void Deactivate()
        {
            flag.SetFlag(false);
            shakeTimer = 0;
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Add(node = new Image(GFX.Game[path + "node"]));
            Add(light = new Light(new Vector2(8, 2)));
            Position.X += (node.Width - 2);
            float snapped = this.Snapped<Solid>(-Vector2.UnitY).Y - 8 - Y;
            Position.X -= (node.Width - 2);
            Collider = node.Collider();

            MTexture wire = GFX.Game[path + "wire00"];
            float ox = (int)Width - (wire.Width * 2 + 1);
            float y = 0;
            while (y != snapped - 8)
            {
                for (int x = 0; x < 2; x++)
                {
                    Image newWire = new Image(wire);
                    newWire.Position = new Vector2(ox + x * 3, y - 8 + x);
                    Add(newWire);
                }
                y = Calc.Approach(y, snapped - 8, 8);
            }
            if (!flag.GetFlag())
            {
                scene.Add(block = new Block(this, Position + new Vector2(3, 11), tiletype));
            }
        }
        public override void Update()
        {
            light.SetColor(flag.GetFlag() ? Color.Lime : Color.Red);
            shakeVector = Vector2.Zero;
            if(shakeTimer > 0)
            {
                shakeVector = Calc.Random.ShakeVector();
                shakeTimer -= Engine.DeltaTime;
            }
            base.Update();
        }
        public override void Render()
        {
            Position += shakeVector;
            base.Render();
            Position -= shakeVector;
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            block?.RemoveSelf();
        }
    }
}