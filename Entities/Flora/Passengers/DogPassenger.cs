using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Flora.Passengers
{
    [CustomEntity("PuzzleIslandHelper/Passengers/Dog")]
    [Tracked]
    public class DogPassenger : VertexPassenger
    {
        private Collider playerBox;
        private float origBreathDuration;
        private float pantingDuration = 0.3f;
        public DogPassenger(EntityData data, Vector2 offset, EntityID id) : base(data, offset, id, 14, 10, new(14, 10), new(-1, 1), 0.7f)
        {
            origBreathDuration = 0.7f;
            MinWiggleTime = 0.7f;
            MaxWiggleTime = 1.1f;
            AddTriangle(new(0.1f, 0), new(1, 0), new(0, 0.6f), 1, Vector2.One, new(1, Ease.Linear, Color.Green, Color.DarkGreen, Color.LawnGreen));
            AddTriangle(new(1f, 0.1f), new(1, 0.7f), new(0, 0.7f), 1, Vector2.One, new(1, Ease.Linear, Color.Green, Color.DarkGreen, Color.LawnGreen));
            AddTriangle(new(0.2f, 0.7f), new(0.3f, 1), new(0.1f, 1), 0, Vector2.One, new(0.9f, Ease.Linear, Color.Green, Color.DarkGreen, Color.LawnGreen));
            AddTriangle(new(0.8f, 0.7f), new(0.9f, 1), new(0.7f, 1), 0, Vector2.One, new(0.9f, Ease.Linear, Color.Green, Color.DarkGreen, Color.LawnGreen));

            AddTriangle(new(1f, 0.1f), new(1.5f, 0.4f), new(1f, 0.5f), 1, Vector2.One, new(0.9f, Ease.Linear, Color.Green, Color.DarkGreen, Color.LawnGreen));

            AddTriangle(new(0.1f, 0f), new(0.5f, 0f), new(0.25f, 0.5f), 1, Vector2.One, new(0.9f, Ease.Linear, Color.White, Color.LightGreen, Color.LightGreen));
            playerBox = new Hitbox(Width + 32, Height + 32, -16, -16);
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Bake();
        }
        public override void Update()
        {
            base.Update();
            if (Baked)
            {
                Collider prev = Collider;
                Collider = playerBox;
                if (CollideFirst<Player>() is Player player)
                {
                    BreathDuration = Calc.Approach(BreathDuration, pantingDuration, Engine.DeltaTime);
                    MinWiggleTime = Calc.Approach(MinWiggleTime, 0.5f, Engine.DeltaTime);
                    MaxWiggleTime = Calc.Approach(MaxWiggleTime, 0.8f, Engine.DeltaTime);
                    VertexList[4].WiggleMult = Calc.Approach(VertexList[4].WiggleMult, 2, Engine.DeltaTime);
                }
                else
                {
                    BreathDuration = Calc.Approach(BreathDuration, origBreathDuration, Engine.DeltaTime);
                    MinWiggleTime = Calc.Approach(MinWiggleTime, 0.7f, Engine.DeltaTime);
                    MaxWiggleTime = Calc.Approach(MaxWiggleTime, 1.1f, Engine.DeltaTime);
                    VertexList[4].WiggleMult = Calc.Approach(VertexList[4].WiggleMult, 1, Engine.DeltaTime);
                }
                Collider = prev;
            }
        }

    }
}
