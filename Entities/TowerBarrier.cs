using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/TowerBarrier")]
    [Tracked]
    public class TowerBarrierBlock : Entity
    {
        public float SolidWidth;
        public float SolidHeight;
        [Tracked]
        public class TowerBarrierManager : Solid
        {
            public List<TowerBarrierBlock> Blocks = new();
            public TowerBarrierManager() : base(Vector2.Zero, 0, 0, false) { }
            public override void Awake(Scene scene)
            {
                base.Awake(scene);
                Vector2 topLeft = new Vector2(int.MaxValue, int.MaxValue);
                Collider[] colliders = new Collider[Blocks.Count];
                foreach (TowerBarrierBlock b in Blocks)
                {
                    topLeft.X = Math.Min(topLeft.X, b.Left);
                    topLeft.Y = Math.Min(topLeft.Y, b.Top);
                }
                for (int i = 0; i < Blocks.Count; i++)
                {
                    var b = Blocks[i];
                    colliders[i] = new Hitbox(b.SolidWidth, b.SolidHeight, b.X - X, b.Y - Y);
                }
                Collider = new ColliderList(colliders);
            }
            public override void Update()
            {
                base.Update();
                Collidable = false;
                foreach(TowerHead h in Scene.Tracker.GetEntities<TowerHead>())
                {
                    if (h.PlayerInside)
                    {
                        Collidable = true;
                        break;
                    }
                }
            }
        }
        public TowerBarrierBlock(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            SolidWidth = data.Width;
            SolidHeight = data.Height;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            TowerBarrierManager manager = PianoUtils.SeekController<TowerBarrierManager>(scene);
            if (manager == null)
            {
                scene.Add(manager = new TowerBarrierManager());
            }
            manager.Blocks.Add(this);
        }
    }
}