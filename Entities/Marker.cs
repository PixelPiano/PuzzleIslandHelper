using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
// PuzzleIslandHelper.ArtifactSlot
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/Marker")]
    [Tracked]
    public class Marker : Entity
    {
        public string ID;
        public static MTexture Texture => GFX.Game["objects/PuzzleIslandHelper/marker/lonn"];
        public Marker(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            ID = data.Attr("markerID");
            Collider = new Hitbox(Texture.Width, Texture.Height);
        }
        public override void DebugRender(Camera camera)
        {
            Texture.Draw(Position);
        }
        public static bool TryFind(string name, out Vector2 position)
        {
            position = Vector2.Zero;
            Marker marker = Find(name);
            if (marker != null)
            {
                position = marker.Position;
                return true;
            }
            return false;
        }
        public static IEnumerator ZoomTo(string name, float zoom, float duration)
        {
            if (Engine.Scene is Level level)
            {
                if (TryFind(name, out Vector2 pos))
                {
                    yield return level.ZoomTo(pos - level.Camera.Position, zoom, duration);
                }
            }
        }
        public static IEnumerator CameraTo(string name, Ease.Easer ease, float duration, bool x, bool y)
        {
            if (Engine.Scene is Level level)
            {
                if (TryFind(name, out Vector2 pos))
                {
                    Vector2 from = level.Camera.Position;
                    if (x && y)
                    {
                        yield return PianoUtils.Lerp(ease, duration, f => level.Camera.Position = Vector2.Lerp(from, pos, f));
                    }
                    else if (x)
                    {
                        yield return PianoUtils.Lerp(ease, duration, f => level.Camera.Position = Vector2.Lerp(from, new Vector2(pos.X, from.Y), f));
                    }
                    else if (y)
                    {
                        yield return PianoUtils.Lerp(ease, duration, f => level.Camera.Position = Vector2.Lerp(from, new Vector2(from.X, pos.Y), f));
                    }
                }
            }
        }
        public static IEnumerator WalkTo(string name, Facings? endFacing = null, bool walkBackwards = false, float speedMult = 1, bool keepWalkingIntoWalls = false)
        {
            if (Engine.Scene is Level level && level.GetPlayer() is Player player && TryFind(name, out Vector2 pos))
            {
                yield return player.DummyWalkTo(pos.X, walkBackwards, speedMult, keepWalkingIntoWalls);
                if (endFacing.HasValue)
                {
                    player.Facing = endFacing.Value;
                }
            }
        }
        public static Marker Find(string name)
        {
            if (Engine.Scene is Level level)
            {
                foreach (Marker marker in level.Tracker.GetEntities<Marker>())
                {
                    if (marker.ID.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return marker;
                    }
                }
            }
            return null;
        }
    }
    public static class MarkerExt
    {
        public static void ToMarker(this Camera entity, string marker)
        {
            if (Marker.TryFind(marker, out Vector2 position))
            {
                entity.Position = position;
            }
        }
        public static void ToMarkerX(this Camera entity, string marker)
        {
            if (Marker.TryFind(marker, out Vector2 position))
            {
                entity.Position = new Vector2(position.X, entity.Position.Y);
            }
        }
        public static void ToMarkerY(this Camera entity, string marker)
        {
            if (Marker.TryFind(marker, out Vector2 position))
            {
                entity.Position = new Vector2(entity.Position.X, position.Y);
            }
        }
        public static void ToMarker(this Entity entity, string marker)
        {
            if (Marker.TryFind(marker, out Vector2 position))
            {
                entity.Position = position;
            }
        }
        public static void ToMarkerX(this Entity entity, string marker)
        {
            if (Marker.TryFind(marker, out Vector2 position))
            {
                entity.Position.X = position.X;
            }
        }
        public static void ToMarkerY(this Entity entity, string marker)
        {
            if (Marker.TryFind(marker, out Vector2 position))
            {
                entity.Position.Y = position.Y;
            }
        }
        public static void ToMarker(this Actor entity, string marker, Collision onCollideH = null, Collision onCollideV = null)
        {
            if (Marker.TryFind(marker, out Vector2 position))
            {
                entity.MoveToX(position.X, onCollideH);
                entity.MoveToY(position.Y, onCollideV);
            }
        }
        public static void ToMarkerX(this Actor entity, string marker, Collision onCollideH = null)
        {
            if (Marker.TryFind(marker, out Vector2 position))
            {
                entity.MoveToX(position.X, onCollideH);
            }
        }
        public static void ToMarkerY(this Actor entity, string marker, Collision onCollideV = null)
        {
            if (Marker.TryFind(marker, out Vector2 position))
            {
                entity.MoveToY(position.Y, onCollideV);
            }
        }

    }
}