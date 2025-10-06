using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities.Programs;
using Celeste.Mod.PuzzleIslandHelper.Entities.WIP;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using static Celeste.Overworld;
using static MonoMod.InlineRT.MonoModRule;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/RuneDisplay")]
    [Tracked]
    public class RuneDisplay : Entity
    {
        public FlagList Flags;
        public List<(int a, int b)> Lines = [];
        private readonly string rune;
        private const float xFactor = 0.16666666666666666f;
        private const float yFactor = 0.5f;
        private Vector2 scale;
        public bool DrawBounds = true;
        public bool DrawAllPoints = true;
        private readonly bool fromId;
        private static Vector2[] indexOffsets = [new(1, 0), new(3, 0), new(5, 0), new(0, 1), new(2, 1), new(4, 1), new(6, 1), new(1, 2), new(3, 2), new(5, 2)];

        public RuneDisplay(EntityData data, Vector2 offset) : this(data.Position + offset, data.Width, data.Height,
            data.Attr("mode") == "From ID" ? data.Attr("runeId") : data.Attr("rune"), data.Attr("mode") == "From ID", data.Attr("flag"),
            data.Int("depth", 1), data.Bool("drawBounds", true), data.Bool("drawAllPoints", true))
        {
        }
        public RuneDisplay(Vector2 position, int width, int height, string rune = "", bool fromid = false, string flag = "", int depth = 1,
            bool drawBounds = true, bool drawAllPoints = true) : base(position)
        {
            Collider = new Hitbox(width, height);
            this.rune = rune;
            this.fromId = fromid;
            scale = new Vector2(xFactor * Width, yFactor * Height);
            Flags = new FlagList(flag);
            Depth = depth;
            DrawBounds = drawBounds;
            DrawAllPoints = drawAllPoints;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            PopulateList(rune, fromId);
        }
        public void PopulateList(string rune, bool isId = false)
        {
            Lines.Clear();
            if (isId)
            {
                var ak = Scene.GetAreaKey();
                if (PianoMapDataProcessor.WarpCapsules.TryGetValue(ak, out CapsuleList list))
                {
                    foreach (var w in list.AllRunes)
                    {
                        if (w.ID.Equals(rune, StringComparison.OrdinalIgnoreCase))
                        {
                            Lines = w.Rune.Segments;
                        }
                    }
                }

            }
            else
            {
                foreach (string s in rune.Split(' ').Where(item => item.Length == 2))
                {
                    if (int.TryParse(s[0].ToString(), out int result) && int.TryParse(s[1].ToString(), out int result2))
                    {
                        Lines.Add(new(result, result2));
                    }
                }
            }
        }
        public override void Render()
        {
            base.Render();
            if (Flags)
            {
                if (DrawBounds)
                {
                    Draw.Rect(Collider, Color.Black);
                    Draw.HollowRect(Collider, Color.White);
                }
                if (DrawAllPoints)
                {
                    foreach (var i in indexOffsets)
                    {
                        Draw.Rect(Position - Vector2.One + i * scale, 3, 3, Color.Magenta);
                    }
                }
                foreach (var l in Lines)
                {
                    Vector2 start = indexOffsets[l.a] * scale;
                    Vector2 end = indexOffsets[l.b] * scale;
                    Draw.Line(Position + start, Position + end, Color.Red);
                }
            }
        }
    }
}