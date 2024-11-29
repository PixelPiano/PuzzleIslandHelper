using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Structs;
using Microsoft.Xna.Framework;
using Monocle;
namespace Celeste.Mod.PuzzleIslandHelper.Entities.Flora
{
    [CustomEntity("PuzzleIslandHelper/EssenceEmitter")]
    [Tracked]
    public class EssenceEmitter : Entity
    {
        public float Rate = 0.5f;
        public float Timer;
        public EssenceEmitter(Vector2 position) : base(position)
        {

        }
        public EssenceEmitter(EntityData data, Vector2 offset) : this(data.Position + offset)
        {

        }
        public override void Update()
        {
            base.Update();
            Timer -= Engine.DeltaTime;
            if (Timer < 0)
            {
                Timer = Calc.Random.Range(0.3f, 0.6f);
                EssenceRenderer.Add(Position, Calc.Random.NextAngle(), 0, Calc.Random.Range(10f, 20f), Vector2.UnitY * Calc.Random.Range(-0.5f, 0.5f), 0, new Range(2f, 4f), new Range(5f, 15f));

            }
        }


    }

}
