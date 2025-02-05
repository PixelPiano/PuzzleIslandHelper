using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
// PuzzleIslandHelper.ArtifactSlot
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/ImpactSender")]
    [TrackedAs(typeof(DashBlock))]
    public class ImpactSender : DashBlock
    {
        public char Key;
        public ImpactSignaller Signaller;
        public ImpactSender(EntityData data, Vector2 offset, EntityID id) : base(data, offset, id)
        {
            Key = data.Char("key");
            OnDashCollide = NewOnDashed;
        }
        public DashCollisionResults NewOnDashed(Player player, Vector2 direction)
        {
            if (!canDash && player.StateMachine.State != 5 && player.StateMachine.State != 10)
            {
                return DashCollisionResults.NormalCollision;
            }
            Impact(player.Center, direction, true, true);
            return DashCollisionResults.Rebound;
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Signaller = scene.Tracker.GetEntity<ImpactSignaller>();
        }
        public void EmitKey()
        {
            Signaller.EmitKey(this);
        }
        public void Impact(Vector2 from, Vector2 direction, bool playSound = true, bool playDebrisSound = true)
        {
            StartShaking(0.3f);
            EmitKey();
            if (playSound)
            {
                if (tileType == '1')
                {
                    Audio.Play("event:/game/general/wall_break_dirt", Position);
                }
                else if (tileType == '3')
                {
                    Audio.Play("event:/game/general/wall_break_ice", Position);
                }
                else if (tileType == '9')
                {
                    Audio.Play("event:/game/general/wall_break_wood", Position);
                }
                else
                {
                    Audio.Play("event:/game/general/wall_break_stone", Position);
                }
            }
            if (from.Y <= Top)
            {
                for (int i = 0; i < Width / 8f; i++)
                {
                    Scene.Add(Engine.Pooler.Create<Debris>().Init(Position + new Vector2(4 + i * 8, -4), tileType, playDebrisSound).BlastFrom(from + Vector2.UnitY * 8));
                }
            }
            else if (from.Y >= Bottom)
            {
                for (int i = 0; i < Width / 8f; i++)
                {
                    Scene.Add(Engine.Pooler.Create<Debris>().Init(Position + new Vector2(4 + i * 8, Height + 4), tileType, playDebrisSound).BlastFrom(from + Vector2.UnitY * -8));
                }
            }
            else if (from.X <= Left)
            {
                for (int i = 0; i < Height / 8f; i++)
                {
                    Scene.Add(Engine.Pooler.Create<Debris>().Init(Position + new Vector2(-4, 4 + i * 8), tileType, playDebrisSound).BlastFrom(from + Vector2.UnitX * 8));
                }
            }
            else if (from.X >= Right)
            {
                for (int i = 0; i < Height / 8f; i++)
                {
                    Scene.Add(Engine.Pooler.Create<Debris>().Init(Position + new Vector2(4, 4 + i * 8), tileType, playDebrisSound).BlastFrom(from + Vector2.UnitX * -8));
                }
            }
            else
            {
                for (int i = 0; i < Width / 8f; i++)
                {
                    for (int j = 0; j < Height / 8f; j++)
                    {
                        Scene.Add(Engine.Pooler.Create<Debris>().Init(Position + new Vector2(4 + i * 8, 4 + j * 8), tileType, playDebrisSound).BlastFrom(from));
                    }
                }
                if (permanent)
                {
                    RemoveAndFlagAsGone();
                }
                else
                {
                    RemoveSelf();
                }
            }
        }

    }

}