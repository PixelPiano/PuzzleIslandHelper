using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/SteamEmitter")]
    [Tracked]
    public class SteamEmitter : Entity
    {
        public string Flag;
        private Image image;
        public enum Directions
        {
            Right, Up, Left, Down
        }
        public float Angle;
        private float delay;
        private Vector2 emitOffset;
        private float timer;
        private float interval;
        public SteamEmitter(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            
            Angle = (MathHelper.Pi / -2f) * (float)data.Enum<Directions>("direction");
            image = new Image(GFX.Game["objects/PuzzleIslandHelper/cap"]);
            image.Rotation = Angle;
            image.CenterOrigin();
            image.Position += image.HalfSize();
            Add(image);
            interval = data.Float("interval", 0.3f);
            Flag = data.Attr("flag");
            Collider = new Hitbox(8, 8);
            emitOffset = Calc.AngleToVector(Angle, 6);
            image.RenderPosition = Center + Calc.AngleToVector(Angle, 8);

        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            delay = Calc.Random.NextFloat();
            timer = delay + interval;
        }
        public override void Update()
        {
            base.Update();
            if (Scene is not Level level || !Scene.OnInterval(interval) || !Flag.GetFlag()) return;
            level.ParticlesFG.Emit(ParticleTypes.Steam, Center + emitOffset, Color.White * Calc.Random.Range(0.5f, 1.1f), Angle);
        }
    }
}
