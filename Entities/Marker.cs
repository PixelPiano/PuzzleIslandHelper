using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
// PuzzleIslandHelper.ArtifactSlot
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/Marker")]
    [Tracked]
    public class Marker : Entity
    {
        public string ID;
        public Dictionary<string, string> Args = [];
        public MarkerData? Data;
        public string ArgData;
        public string ArgInfo
        {
            get
            {
                if (Args.Count == 0) return "No arguments passed in.";
                string output = "\n---MARKER ARGUMENTS---\n";
                foreach (var pair in Args)
                {
                    output += '\t';
                    string v = string.IsNullOrEmpty(pair.Value) ? "" : pair.Value;
                    output += string.Format("({0}: {1})", pair.Key, v);
                    output += '\n';
                }
                output += "----------------------";
                return output;
            }
        }
        public static MTexture Texture => GFX.Game["objects/PuzzleIslandHelper/marker/lonn"];
        public Marker(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            ID = data.Attr("markerID");
            ArgData = data.Attr("args");
            Collider = new Hitbox(Texture.Width, Texture.Height);
            Tag |= Tags.TransitionUpdate;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (MarkerExt.TryGetData(ID, (scene as Level).Session.Level, scene, out var Data))
            {
                this.Data = Data;
            }
            if (!string.IsNullOrEmpty(ArgData))
            {
                foreach (string arg in ArgData.Split(','))
                {
                    string key = arg;
                    string value = "";
                    for (int i = 0; i < arg.Length; i++)
                    {
                        if (arg[i] == ':' || arg[i] == '=')
                        {
                            key = arg.Substring(0, i);
                            value = arg.Substring(i + 1);
                            break;
                        }
                    }
                    if (!string.IsNullOrEmpty(key))
                    {
                        Args.Add(key, value);
                    }
                }
            }
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
        public static IEnumerator ZoomTo(string name, float zoom, float duration, Vector2 offset = default)
        {
            if (Engine.Scene is Level level)
            {
                if (TryFind(name, out Vector2 pos))
                {
                    yield return level.ZoomTo(pos - level.Camera.Position + offset, zoom, duration);
                }
            }
        }
        public static IEnumerator CameraTo(string name, Ease.Easer ease, float duration, Vector2 offset = default)
        {
            if (Engine.Scene is Level level)
            {
                if (TryFind(name, out Vector2 pos))
                {
                    Vector2 from = level.Camera.Position;
                    yield return PianoUtils.Lerp(ease, duration, f => level.Camera.Position = Vector2.Lerp(from, pos + offset, f));
                }
            }
        }
        public static IEnumerator CameraToX(string name, Ease.Easer ease, float duration, float offset = 0)
        {
            if (Engine.Scene is Level level)
            {
                if (TryFind(name, out Vector2 pos))
                {
                    float from = level.Camera.X;
                    yield return PianoUtils.Lerp(ease, duration, f => level.Camera.X = Calc.LerpClamp(from, pos.X + offset, f));
                }
            }
        }
        public static IEnumerator CameraToY(string name, Ease.Easer ease, float duration, float offset = 0)
        {
            if (Engine.Scene is Level level)
            {
                if (TryFind(name, out Vector2 pos))
                {
                    float from = level.Camera.Y;
                    yield return PianoUtils.Lerp(ease, duration, f => level.Camera.Y = Calc.LerpClamp(from, pos.Y + offset, f));
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
        [Command("list_marker_ids", "")]
        public static void ListMarkerIDs(string room)
        {
            if (Engine.Scene is not null)
            {
                if (PianoMapDataProcessor.MarkerData.TryGetValue(Engine.Scene.GetAreaKey(), out var l))
                {
                    if (l.TryGetValue(room, out var l2))
                    {
                        foreach (var d in l2)
                        {
                            Engine.Commands.Log(d.ID);
                        }
                    }
                }
            }
        }
        public static List<MarkerData> RoomMarkers(string room)
        {
            if (Engine.Scene is not null)
            {
                if (PianoMapDataProcessor.MarkerData.TryGetValue(Engine.Scene.GetAreaKey(), out var l))
                {
                    if (l.TryGetValue(room, out var l2))
                    {
                        return l2;
                    }
                }
            }
            return [];
        }
        public static bool TryGetData(string id, string room, Scene scene, out MarkerData data)
        {
            data = default;
            if (PianoMapDataProcessor.MarkerData.TryGetValue(scene.GetAreaKey(), out var l))
            {
                if (l.TryGetValue(room, out List<MarkerData> value))
                {
                    foreach (MarkerData d in value)
                    {
                        if (d.ID == id)
                        {
                            data = d;
                            return true;
                        }
                    }
                }
            }

            return false;

        }
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