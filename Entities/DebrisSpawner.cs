using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/DebrisSpawner")]
    [Tracked]
    public class DebrisSpawner : Entity
    {
        private string flag;
        private bool inverted;
        private bool oncePerSession;
        private char tileType;
        private EntityID id;
        private bool State
        {
            get
            {
                if (string.IsNullOrEmpty(flag))
                {
                    return true;
                }
                bool flagState = SceneAs<Level>().Session.GetFlag(flag);
                return inverted ? !flagState : flagState;
            }
        }
        public DebrisSpawner(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
        {
            this.id = id;
            flag = data.Attr("flag");
            inverted = data.Bool("invertFlag");
            oncePerSession = data.Bool("onlyOncePerSession");
            tileType = data.Char("tileType");
            Collider = new Hitbox(data.Width, data.Height);
        }
        public override void Update()
        {
            base.Update();
            if (State)
            {
                EmitDebris();
            }
        }
        public static void SpawnDebrisBox(Scene scene, Vector2 position, Vector2 blastFrom, float width, float height, char tileType, bool playSound = true)
        {
            for (int i = 0; i < width / 8f; i++)
            {
                for (int j = 0; j < height / 8f; j++)
                {
                    SpawnDebrisSingle(scene, position + new Vector2(4 + i * 8, 4 + j * 8), blastFrom, tileType, playSound);
                }
            }
        }
        public static void SpawnDebrisSingle(Scene scene, Vector2 position, Vector2 blastFrom, char tileType, bool playSound = true)
        {
            scene.Add(Engine.Pooler.Create<Debris>().Init(position, tileType, playSound).BlastFrom(blastFrom));
        }
        private void EmitDebris()
        {
            SpawnDebrisBox(Scene, Position, Center, Width, Height, tileType, true);
            if (oncePerSession)
            {
                SceneAs<Level>().Session.DoNotLoad.Add(id);
                RemoveSelf();
            }
        }
    }
}