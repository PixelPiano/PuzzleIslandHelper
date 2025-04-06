using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked(false)]
    [CustomEntity("PuzzleIslandHelper/RandomTeleportArea")]
    public class RandomTeleportArea : Entity
    {
        public bool WaitUntilLeft;
        private bool playerIsInside;
        private bool playerWasInside;
        private Vector2[] Positions;
        private Collider[] Colliders;
        private Collider lastCollided;
        private int lastIndex = 0;
        private bool randomized;
        private bool disabled;
        public RandomTeleportArea(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            randomized = data.Bool("randomTeleport");
            Collider = new Hitbox(data.Width, data.Height);
            Positions = data.NodesWithPosition(offset);
            Colliders = new Collider[Positions.Length];
            for (int i = 0; i < Colliders.Length; i++)
            {
                Colliders[i] = new Hitbox(data.Width, data.Height, Positions[i].X, Positions[i].Y);
            }
            WaitUntilLeft = data.Bool("waitUntilLeave");
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            for (int i = 1; i < Colliders.Length; i++)
            {
                Draw.HollowRect(Colliders[i], Color.Orange);
            }
        }
        private void teleport()
        {
            if (Scene.GetPlayer() is Player player)
            {
                Collider current = Colliders[lastIndex];
                Collider target = randomized ? Colliders.Where(item => item != current).ToList().Random() : Colliders[(lastIndex + 1) % Colliders.Length];
                player.Position = target.Position + (current == null ? Vector2.Zero : player.Position - current.Position);

                Add(new Coroutine(checkRoutine()));
                Add(new Coroutine(playerCheck(player)));
            }
        }
        private IEnumerator playerCheck(Player player)
        {
            player.DisableMovement();
            yield return null;
            player.EnableMovement();
        }
        private IEnumerator checkRoutine()
        {
            disabled = true;
            while (playerIsInside) yield return null;
            disabled = false;
        }
        public override void Update()
        {
            base.Update();
            playerWasInside = playerIsInside;
            for (int i = 0; i < Colliders.Length; i++)
            {
                if (Scene.CollideCheck<Player>(Colliders[i].Bounds))
                {
                    lastIndex = i;
                    lastCollided = Colliders[i];
                    playerIsInside = true;
                    break;
                }
                playerIsInside = false;
                lastCollided = null;
            }
            if (!disabled && playerIsInside && (!WaitUntilLeft || (playerIsInside && !playerWasInside)))
            {
                teleport();
            }
        }
    }
}