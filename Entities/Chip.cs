using Celeste.Mod.Core;
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using YamlDotNet.Core;
using static Celeste.Mod.CommunalHelper.Entities.RedlessBerry;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked]
    //[CustomEntity("PuzzleIslandHelper/WipEntity")]
    public class Chip : Entity
    {
        public struct VertexInfo
        {
            public float ZAmount;
            public Vector2 Offset;
            public Color GetColor(Color baseColor)
            {
                return ZAmount switch
                {
                    > 0 => Color.Lerp(baseColor, Color.White, ZAmount),
                    < 0 => Color.Lerp(baseColor, Color.Black, ZAmount),
                    _ => baseColor
                };
            }
        }
        public class BinaryChar
        {
            public Vector2 Offset;
            public float Scale;
            public Color Color;
            public float Alpha = 1;
            public VertexInfo[] Info = new VertexInfo[4];
            public BinaryChar(Vector2 offset, float scale, Color color)
            {
                Offset = offset;
                Scale = scale;
                Color = color;
            }
            public Color GetColor(int index)
            {
                return Info[index].GetColor(Color) * Alpha;
            }
            public static string GetBinaryChar(char c)
            {
                if (char.IsWhiteSpace(c)) return "/";
                return Lookup[c].Substring(3);
            }
            public static string[] GetBinaryArray(string word)
            {
                string output = "";
                foreach (char c in word)
                {
                    output += GetBinaryChar(c) + " ";
                }
                output.Trim();

                return output.Split(' ');
            }
            public static string GetBinaryWord(string word)
            {
                string output = "";
                foreach (char c in word)
                {
                    output += GetBinaryChar(c) + " ";
                }
                output.Trim();
                return output;
            }
            public static List<BinaryChar> ParseWord(string word)
            {
                if (string.IsNullOrEmpty(word)) return null;
                List<BinaryChar> bits = new();

                float x = 0;
                float xSpacing = 2;
                float ySpacing = 2;
                float size = 4;
                string[] array = GetBinaryArray(word);
                foreach (string s in array)
                {
                    if (!(string.IsNullOrEmpty(s) || s == "/"))
                    {
                        float y = 0;
                        for (int i = 0; i < s.Length; i++)
                        {
                            if (s[i] != '0')
                            {
                                bits.Add(new BinaryChar(new Vector2(x, y), size, Color.Lime));
                            }
                            y += size + ySpacing;
                        }
                    }
                    x += size + xSpacing;
                }
                return bits;
            }
        }

        private static Vector2[] points = new Vector2[] { new(0), new(1, 0), new(0, 1), new(1) };
        public static string[] Lookup = new string[256];

        public VertexPositionColor[] Vertices;
        public float Alpha = 1;
        public int[] Indices;
        public string Word;
        public int Primitives;
        public bool Baked;
        public List<BinaryChar> Bins = new();
        public Chip(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Word = data.Attr("stringValue");
            Bins = BinaryChar.ParseWord(Word.ToLower());
            float right = int.MinValue, left = int.MaxValue, up = int.MaxValue, down = int.MinValue;
            foreach (BinaryChar c in Bins)
            {
                right = Math.Max(right, c.Offset.X);
                down = Math.Max(down, c.Offset.Y);
                left = Math.Min(left, c.Offset.X);
                up = Math.Min(up, c.Offset.Y);
            }
            Collider = new Hitbox(right - left + 4, down - up + 4, left, up);
            Bake();
        }
        public override void Update()
        {
            base.Update();
            UpdateVertices();
        }
        public void UpdateVertices()
        {
            if (!Baked) return;
            for (int i = 0; i < Bins.Count; i++)
            {
                BinaryChar info = Bins[i];
                for (int v = 0; v < 4; v++)
                {
                    Vertices[i * 4 + v].Position = new(Position + info.Offset + points[v] * info.Scale + info.Info[v].Offset, 0);
                    Vertices[i * 4 + v].Color = info.GetColor(v) * Alpha;
                }
            }
        }
        public void Bake()
        {
            if (Bins.Count == 0) return;
            List<VertexPositionColor> vertices = new();
            List<int> indices = new();
            foreach (BinaryChar info in Bins)
            {
                int c = vertices.Count;
                indices.Add(c + 0);
                indices.Add(c + 1);
                indices.Add(c + 2);
                indices.Add(c + 2);
                indices.Add(c + 1);
                indices.Add(c + 3);
                for (int i = 0; i < 4; i++)
                {
                    vertices.Add(new(new(Position + info.Offset + points[i] * info.Scale, 0), info.Color));
                }
                Primitives += 2;
            }
            Vertices = vertices.ToArray();
            Indices = indices.ToArray();
            Baked = true;
        }
        public override void Render()
        {
            base.Render();
            if (!Baked || Scene is not Level level) return;
            Draw.SpriteBatch.End();
            DrawVertices(level);
            GameplayRenderer.Begin();
        }
        public void DrawVertices(Level level)
        {
            GFX.DrawIndexedVertices(level.Camera.Matrix, Vertices, Vertices.Length, Indices, Primitives);
        }
        [OnInitialize]
        public static void Initialize()
        {
            //Thank you Rik on Stack Overflow
            //One of the not rude people there
            //https://stackoverflow.com/questions/26887113/get-8-digit-binary-from-char-in-c-sharp-win-form
            Lookup = new string[256];
            for (int i = 0; i < 256; i++)
            {
                Lookup[i] = Convert.ToString(i, 2).PadLeft(8, '0');
            }
        }
        [Command("get_binary", "get binary data from string")]
        public static void GetBin(string value)
        {
            if (Engine.Scene is not Level level) return;
            Engine.Commands.Log(BinaryChar.GetBinaryWord(value));
        }
    }
}
