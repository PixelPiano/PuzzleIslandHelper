using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/HousePart")]
    [Tracked]
    public class HousePart : Entity
    {
        public Vector2 Offset;
        private float mult;
        public float Rotation;
        public Color Color = Color.Lime;
        public SineWave Sine;
        public Vector2 Scale;
        public Sprite Sprite;
        public bool Outline;
        private float rotationRate;
        private string path;
        public HousePart(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Color = data.HexColor("color");
            Depth = data.Int("depth");
            Rotation = data.Float("rotation");
            rotationRate = data.Float("rotationRate");
            Outline = data.Bool("outline");
            Scale = new Vector2(data.Float("scaleX"), data.Float("scaleY"));

            Sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/houseParts/");
            path = data.Attr("decalPath");
            Sprite.AddLoop("idle", path, 0.1f);
            Sprite.CenterOrigin();
            Collider = new Hitbox(Sprite.Width, Sprite.Height);
            Sine = new SineWave()
            {
                OnUpdate = f => Offset.Y = f * mult
            };
            Sprite.Play("idle");
            Add(Sprite, Sine);
            Position -= Sprite.HalfSize();
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Sine.Frequency = Calc.Random.Range(0.05f, 0.5f);
            Sine.Counter = Calc.Random.Range(0, 25.132741928100586f);
            mult = Calc.Random.Range(1f, 4f);
            updateSprite();
            /*            Position.X += random.Range(-8f, 8f);
                        Position.Y += random.Range(-8f, 8f);*/
        }
        public override void Update()
        {
            base.Update();
            Rotation = (Rotation + rotationRate) % 360;
            updateSprite();
        }
        private void updateSprite()
        {
            Sprite.Position = Sprite.HalfSize() + Offset;
            Sprite.Color = Color;
            Sprite.Rotation = Rotation.ToRad();
            Sprite.Scale = Scale;
        }
        public override void Render()
        {
            if (Outline)
            {
                Sprite.DrawSimpleOutline();
            }
            base.Render();
        }
    }
}