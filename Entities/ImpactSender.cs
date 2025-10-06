using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked]
    public class ImpactSignalComponent : Component
    {
        public Action<ImpactSender> Action;
        public ImpactSignalComponent(Action<ImpactSender> action) : base(true, false)
        {
            Action = action;
        }
    }
    [CustomEntity("PuzzleIslandHelper/ImpactSender")]
    [TrackedAs(typeof(DashBlock))]
    public class ImpactSender : DashBlock
    {
        public char Key;
        public ImpactSignaller Signaller;
        private float pulseDuration;
        private Vector2 pulsePosition;
        private Color pulseColor;
        private bool shakes;
        private TileGrid tiles;
        private Color tileColor = Color.White;
        private float colorLerp;
        public ImpactSender(EntityData data, Vector2 offset, EntityID id) : base(data, offset, id)
        {
            Key = data.Char("key");
            OnDashCollide = NewOnDashed;
            pulseDuration = data.Float("pulseDuration");
            pulsePosition = data.NodesOffset(offset)[0];
            pulseColor = data.HexColor("pulseColor");
            shakes = data.Bool("shakes");

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
        public override void OnShake(Vector2 amount)
        {
            base.OnShake(amount);
            if (tiles != null)
            {
                tiles.Position += amount;
            }
        }
        public override void Update()
        {
            base.Update();
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Signaller = scene.Tracker.GetEntity<ImpactSignaller>();
            tiles = Components.Get<TileGrid>();
        }
        public void EmitKey()
        {
            Signaller.EmitKey(this);
        }

        public void Impact(Vector2 from, Vector2 direction, bool playSound = true, bool playDebrisSound = true)
        {
            if (shakes)
            {
                StartShaking(0.3f);
            }
            EmitKey();
            PulseEntity.Circle(pulsePosition, Depth + 1, Pulse.Fade.InAndOut, Pulse.Mode.Oneshot,0, Width, pulseDuration,true,pulseColor,pulseColor,null,Ease.CubeIn);
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