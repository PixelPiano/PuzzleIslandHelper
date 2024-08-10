using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.MenuElements;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/GlobalTimer")]
    [Tracked]
    public class GlobalTimer : Entity
    {
        public static float Time;
        public const float AmnesiaTime = 1296000f;
        private EntityID id;
        public GlobalTimer(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
        {
            this.id = id;
            Tag = Tags.TransitionUpdate | Tags.Global | Tags.Persistent;
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            (scene as Level).Session.DoNotLoad.Add(id);
        }
        public override void Update()
        {
            base.Update();
            Time += Engine.DeltaTime * 1000;
            OuiFileFader.FadeAmount = Time / AmnesiaTime;
        }
    }
}
