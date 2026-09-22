using Celeste.Mod.Entities;
using Celeste.Mod.Helpers;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using static Celeste.Mod.PuzzleIslandHelper.Components.VertexOrb;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Singularity
{
    [CustomEntity("PuzzleIslandHelper/AscendPattern")]
    [Tracked]
    public class AscendPattern : Entity
    {
        public class Triangle
        {
            public Vector2 Offset;
            public float Distance;
            public float RotationRate;
            public float Rotation;
            private VertexPositionColor[] vertices = new VertexPositionColor[3];
            private int[] indices = [0, 1, 2];
            public void Update()
            {
                Rotation += RotationRate * Engine.DeltaTime;
                float angleOffset = MathHelper.TwoPi / 3;
                for (int i = 0; i < 3; i++)
                {
                    vertices[i].Position = new Vector3(Offset + Calc.AngleToVector(angleOffset * i + Rotation, Distance), 0);
                    vertices[i].Color = Color.White;
                }
            }
            public void DirectRenderVertices()
            {
                Engine.Graphics.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, vertices, 0, 3, indices, 0, 1);
            }
        }
        
        public VirtualRenderTarget Target;
        public int StreakHeight = 4;
        public int ColumnPad = 4;
        public char TileA = 'Y', TileB = 'R';
        private TileGrid grid;
        private bool renderedToTarget;
        private bool export;
        public Vector2 OrigPosition;
        private float clipYOffset;
        private MTexture texture;
        public float TotalDistance;
        public enum ClipModes
        {
            Camera,
            Speed
        }
        public ClipModes ClipMode = ClipModes.Camera;
        public AscendPattern() : this(Vector2.Zero) { }
        public AscendPattern(EntityData data, Vector2 offset) : this(data.Position + offset) { }
        public AscendPattern(Vector2 position) : base(position)
        {
            Depth = 3;
            Collider = new Hitbox(320, 184);
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            OrigPosition = Position;
            texture = GFX.Game["objects/PuzzleIslandHelper/ascendPattern"];
        }
        public void SwapClipMode()
        {
            ClipMode = ClipModes.Speed;
        }
        public override void Render()
        {
            base.Render();
            Rectangle clip = texture.ClipRect;
            Camera c = SceneAs<Level>().Camera;
            switch (ClipMode)
            {
                case ClipModes.Speed:
                    clip.Y = (int)(clipYOffset + TotalDistance);
                    break;
                case ClipModes.Camera:
                    clip.Y = -Math.Max(0, (int)(OrigPosition.Y - (c.Y + SceneAs<Level>().CameraOffset.Y)));
                    clipYOffset = clip.Y;
                    break;
            }

            float renderY = Math.Min(Y, c.Y);
            Draw.SpriteBatch.Draw(texture.Texture.Texture_Safe, new Vector2(X, renderY), clip, Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);
        }
        [Command("generate_ascend", "")]
        public static void GenerateAscend(int height, int pad, string tileA, string tileB, string name)
        {
            if (Engine.Scene.Tracker.GetEntity<AscendPattern>() is AscendPattern p)
            {
                p.StreakHeight = height;
                p.ColumnPad = pad;
                p.TileA = tileA[0];
                p.TileB = tileB[0];
                p.grid = null;
                p.Generate();
            }
        }
        public void Generate()
        {
            int halfcolumns = 320 / 16 - ColumnPad;
            int columns = 320 / 8;
            int rows = 184 / 8;
            VirtualMap<char> tiles = new VirtualMap<char>(columns, rows, default);
            (char a, char b) = (TileA, TileB);
            List<char> columnTilesList = [];
            int listStartIndex = 0;
            int offset = 0;
            for (int i = 0; i < rows; i++)
            {
                columnTilesList.Add(a);
                offset++;
                if (offset >= StreakHeight)
                {
                    (a, b) = (b, a);
                    offset = 0;
                }
            }

            for (int c = 0; c <= halfcolumns; c++)
            {
                for (int r = 0; r < rows; r++)
                {
                    int index = (listStartIndex + r) % columnTilesList.Count;
                    int index2 = Math.Max(0, (listStartIndex - 1 + r)) % columnTilesList.Count;
                    char tile = columnTilesList[index];
                    char tileB = columnTilesList[index2] == TileA ? TileB : TileA;
                    if (c != halfcolumns) tiles[c, r] = tile;
                    tiles[columns - c, r] = tileB;
                }
                listStartIndex++;
            }
            grid = GFX.BGAutotiler.GenerateMap(tiles, false).TileGrid;
            grid.VisualExtend = 1;
            renderedToTarget = false;
            export = false;
        }
        public override void Update()
        {
            base.Update();
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Target?.Dispose();
        }
    }
}