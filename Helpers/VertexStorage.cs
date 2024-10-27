using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Celeste.Mod.CommunalHelper.Utils;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Helpers;

public class VertexStorage
{
    public static Dictionary<string, VertexPositionColor[]> Dictionary = new();
    public static void Store(string id, VertexPositionColor[] vertices)
    {
        float left = int.MaxValue;
        float right = int.MinValue;
        float up = int.MaxValue;
        float down = int.MinValue;
        for (int i = 0; i < vertices.Length; i++)
        {
            left = Calc.Min(left, vertices[i].Position.X);
            right = Calc.Max(right, vertices[i].Position.X);
            up = Calc.Min(up, vertices[i].Position.Y);
            down = Calc.Max(down, vertices[i].Position.Y);
        }
        Vector2 position = new Vector2(left, up);
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].Position -= new Vector3(position, 0);
        }
        if (!Dictionary.ContainsKey(id))
        {
            Dictionary.Add(id, vertices);
        }
        else
        {
            Dictionary[id] = vertices;
        }
    }
    public static VertexPositionColor[] Retrieve(string id)
    {
        return Dictionary[id];
    }
}
