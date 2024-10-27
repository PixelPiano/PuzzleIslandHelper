// PuzzleIslandHelper.PuzzleIslandHelperCommands
using Celeste;
using Celeste.Mod;
using Celeste.Mod.CommunalHelper;
using Celeste.Mod.CommunalHelper.Utils;
using Celeste.Mod.FancyTileEntities;
using Celeste.Mod.PuzzleIslandHelper;
using Celeste.Mod.PuzzleIslandHelper.Cutscenes.GameshowEntities;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.WIP;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using static Celeste.Mod.PuzzleIslandHelper.Entities.ArtifactSlot;
using static Celeste.Player;

public static class PianoUtils
{
    public static T[] Shift<T>(this T[] array, int shift)
    {
        T[] array2 = array;
        for (int s = 0; s < shift; s++)
        {
            array2 = array;
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = array2[(i + 1) % array2.Length];
            }
        }
        return array2;
    }
    public static T[] ShiftReverse<T>(this T[] array, int shift)
    {
        T[] array2 = array;
        for (int s = 0; s < shift; s++)
        {
            array2 = array;
            for (int i = 0; i < array.Length; i++)
            {
                int index = i - 1;
                if (index < 0) index = array2.Length;
                array[i] = array2[index];
            }
        }
        return array2;
    }
    public static Tween SetTo(this Tween tween, float amount)
    {
        tween.TimeLeft = tween.Duration - tween.Duration * amount;
        tween.TimeLeft += (tween.UseRawDeltaTime ? Engine.RawDeltaTime : Engine.DeltaTime);
        return tween;
    }
    public static Tween Randomize(this Tween tween)
    {
        return tween.SetTo(Calc.Random.Range(0, 1f));
    }
    public static VertexPositionColor[] CreateVertices(this Vector2[] points, int[] indices, Vector2 scale, params Color[] colors)
    {
        Vector3[] newPoints = new Vector3[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            newPoints[i] = new Vector3(points[i], 0);
        }
        return CreateVertices(newPoints, indices, new Vector3(scale, 0), colors);
    }
    public static VertexPositionColor[] CreateVertices(this Vector2[] points, Vector2 scale, out int[] indices, params Color[] colors)
    {
        Vector3[] newPoints = new Vector3[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            newPoints[i] = new Vector3(points[i], 0);
        }
        return CreateVertices(newPoints, new Vector3(scale, 0), out indices, colors);
    }
    public static VertexPositionColor[] CreateVertices(this Vector3[] points, int[] indices, Vector3 scale, params Color[] colors)
    {
        if (indices == null)
        {
            indices = new int[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                indices[i] = i;
            }
        }
        VertexPositionColor[] vertices = new VertexPositionColor[points.Length];
        Color color = Color.White;
        for (int i = 0; i < points.Length; i++)
        {
            Color c = colors != null && colors.Length > i ? colors[i] : color;
            vertices[i] = new VertexPositionColor(points[i] * scale, c);
            color = c;
        }
        return vertices;
    }
    public static VertexPositionColor[] CreateVertices(this Vector3[] points, Vector3 scale, out int[] indices, params Color[] colors)
    {
        indices = new int[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            indices[i] = i;
        }
        VertexPositionColor[] vertices = new VertexPositionColor[points.Length];
        Color color = Color.White;
        for (int i = 0; i < points.Length; i++)
        {
            Color c = colors != null && colors.Length > i ? colors[i] : color;
            vertices[i] = new VertexPositionColor(points[i] * scale, c);
            color = c;
        }
        return vertices;
    }
    internal static void InvokeAllWithAttribute(Type attributeType)
    {
        Type attributeType2 = attributeType;
        Type[] typesSafe = typeof(PianoModule).Assembly.GetTypesSafe();
        for (int i = 0; i < typesSafe.Length; i++)
        {
            checkType(typesSafe[i]);
        }

        void checkType(Type type)
        {
            MethodInfo[] methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public);
            foreach (MethodInfo methodInfo in methods)
            {
                foreach (CustomAttributeData customAttribute in methodInfo.CustomAttributes)
                {
                    if (customAttribute.AttributeType == attributeType2)
                    {
                        methodInfo.Invoke(null, null);
                        return;
                    }
                }
            }
        }
    }
    public struct TriWall
    {
        public float Width;
        public float Height;
        public float TriangleWidth;
        public float TriangleHeight;
        public int ExtendL;
        public int ExtendR;
        public int ExtendU;
        public int ExtendD;
        public int Rows;
        public int Columns;
        public Vector2[,] Points;
        public Vector2 this[int x, int y]
        {
            get
            {
                if (x >= 0 && y >= 0 && x < Columns && y < Rows)
                {
                    return Points[x, y];
                }

                return Vector2.Zero;
            }
            set
            {
                Points[x, y] = value;
            }
        }
    }
    [Tracked]
    public class KeepGrounded : Monocle.Component
    {
        public KeepGrounded() : base(true, false) { }
        public override void Update()
        {
            if (Entity is FallingBlock)
            {
                (Entity as FallingBlock).FallDelay = 10;
            }
        }
    }
    public static Mesh<VertexPositionColor> CreateTriWallMesh(Vector3 position, float areaWidth, float areaHeight, float triWidth, float triHeight, int extend, Func<Vector2, Color> getColor)
    {
        Mesh<VertexPositionColor> mesh = new Mesh<VertexPositionColor>();
        int exL = extend + 1;
        int exR = extend + 2;
        int exT = extend;
        int exB = extend + 1;

        TriWall meshPoints = CreateTriWall(areaWidth, areaHeight, triWidth, triHeight, exL, exR, exT, exB, out int rows, out int cols);
        int count = rows * cols;
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                Vector2 uv = new Vector2(meshPoints[c, r].X / areaWidth, meshPoints[c, r].Y / areaHeight);
                VertexPositionColor vertex = new(position + new Vector3(meshPoints[c, r], 0), getColor(uv));
                mesh.AddVertex(vertex);
            }
        }
        for (int i = 0; i < count; i++)
        {
            int a1 = i;
            int b1 = i + 1;
            int c1 = i + cols;
            if (a1 / cols == b1 / cols)
            {
                if (c1 / cols % 2 == 0)
                {
                    c1++;
                }
                if (c1 < count)
                {
                    mesh.AddTriangle(a1, b1, c1);
                }
            }
            int a2 = c1;
            int c2 = c1 + 1;
            if (c1 / cols == c2 / cols && c2 < count)
            {
                mesh.AddTriangle(a2, b1, c2);
            }
        }
        return mesh;
    }

    public static LineMesh<VertexPositionColor> CreateTriLineWallMesh(Vector3 position, float areaWidth, float areaHeight, float triWidth, float triHeight, int extend, Func<Vector2, Color> getColor)
    {
        LineMesh<VertexPositionColor> mesh = new LineMesh<VertexPositionColor>();
        int exL = extend + 1;
        int exR = extend + 2;
        int exT = extend;
        int exB = extend + 1;

        TriWall meshPoints = CreateTriWall(areaWidth, areaHeight, triWidth, triHeight, exL, exR, exT, exB, out int rows, out int cols);
        int count = rows * cols;
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                Vector2 uv = new Vector2(meshPoints[c, r].X / areaWidth, meshPoints[c, r].Y / areaHeight);
                VertexPositionColor vertex = new(position + new Vector3(meshPoints[c, r], 0), getColor(uv));
                mesh.AddVertex(vertex);
            }
        }
        for (int i = 0; i < count; i++)
        {
            int o = i;
            int d1 = i + 1;
            int d2 = i + cols - 1;

            int div = o / cols;
            if (div == d1 / cols && d1 < count)
            {
                mesh.AddLine(o, d1);
            }
            if (div != d2 / cols)
            {
                if (d2 / cols % 2 == 0)
                {
                    d2++;
                }
                if (d2 < count)
                {
                    mesh.AddLine(o, d2);
                }
            }
            int d3 = d2 + 1;
            if (div != d3 / cols && d3 / cols == d2 / cols && d3 < count)
            {
                mesh.AddLine(o, d3);
            }

        }
        return mesh;
    }
    public static Mesh<VertexPositionColor> CreateTriWallMesh(Vector3 position, float areaWidth, float areaHeight, float triWidth, float triHeight, int extend, Color color)
    {
        Func<Vector2, Color> func = delegate { return color; };
        return CreateTriWallMesh(position, areaWidth, areaHeight, triWidth, triHeight, extend, func);
    }
    public static TriWall CreateTriWall(float areaWidth, float areaHeight, float triHeight, float triWidth, int leftExtend, int rightExtend, int topExtend, int bottomExtend, out int rows, out int cols)
    {

        float top = topExtend * -triHeight;
        float left = leftExtend * -triWidth;
        float right = areaWidth + rightExtend * triWidth;
        float bottom = areaHeight + bottomExtend * triHeight;
        rows = (int)((bottom - top) / triHeight);
        cols = (int)((right - left) / triWidth);
        float xOffset = triWidth / 2f;
        Vector2[,] grid = new Vector2[cols, rows];
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                grid[c, r] = new Vector2(left + c * triWidth + (r % 2 != 0 ? xOffset : 0), top + r * triHeight);
            }
        }
        TriWall wall = new()
        {
            Points = grid,
            ExtendD = bottomExtend,
            ExtendU = topExtend,
            ExtendL = leftExtend,
            ExtendR = rightExtend,
            Rows = rows,
            Columns = cols,
            Width = areaWidth,
            Height = areaHeight,
            TriangleWidth = triWidth,
            TriangleHeight = triHeight
        };
        return wall;
    }
    public static List<Vector2?> GetTriWallPoints(float areaWidth, float areaHeight, float triHeight, float triWidth, int extend, bool itsnew)
    {
        List<Vector2?> screenpoints = new List<Vector2?>();

        float top = extend * -triHeight;
        float left = extend * -triWidth;
        float right = areaWidth + extend * triWidth;
        float bottom = areaHeight + extend * triHeight;
        bool offsetHeight = false;
        bool offsetRow = false;
        float adjustY = triHeight / 2;
        float adjustX = 0;
        int r = (int)((bottom - top) / triHeight);
        int c = (int)((right - left) / triWidth);
        float xOffset = triWidth / 2;
        Vector2[,] grid = new Vector2[c, r];
        for (int i = 0; i < r; i++)
        {
            for (int j = 0; j < c; j++)
            {
                grid[j, i] = new Vector2(left + j * triWidth + (j % 2 == 0 ? xOffset : 0), top + r * triHeight);
            }
        }

        for (float j = top; j < bottom; j += triHeight)
        {
            for (float i = left; i < right; i += triWidth / 2)
            {
                Vector2 pos = new Vector2(i + adjustX, (offsetHeight ? -1 : 1) * adjustY + j);
                screenpoints.Add(pos);
                offsetHeight = !offsetHeight;
            }

            offsetHeight = false;
            adjustX = offsetRow ? triWidth / 2 : 0;
            offsetRow = !offsetRow;
            screenpoints.Add(null);
        }
        return screenpoints;
    }


    public static void ParseFlagsFromString(string flagList, out string[] flags, out bool[] states)
    {
        flags = null;
        states = null;
        if (!string.IsNullOrEmpty(flagList))
        {
            string[] array = flagList.Replace(" ", "").Split(',');
            states = new bool[array.Length];
            flags = new string[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].Length > 0)
                {
                    if (array[i][0] == '!')
                    {
                        flags[i] = array[i].Substring(1);
                        states[i] = false;
                    }
                    else
                    {
                        flags[i] = array[i];
                        states[i] = true;
                    }

                }
            }

        }
    }
    public static void RenderAt(this Image image, Vector2 at)
    {
        Vector2 orig = image.RenderPosition;
        image.RenderPosition = at;
        image.Render();
        image.RenderPosition = orig;
    }
    public static void RenderOffset(this Image image, Vector2 offset)
    {
        image.Position += offset;
        image.Render();
        image.Position -= offset;
    }

    public static void Ground<T>(IEnumerable<T> list) where T : FallingBlock
    {
        foreach (FallingBlock block in list.OrderByDescending(item => item.Bottom))
        {
            block.Ground();
        }
    }
    public static void Ground(this FallingBlock block)
    {

        Level level = block.Scene as Level;
        foreach (Monocle.Component c in block.Components)
        {
            if (c is KeepGrounded) return;
        }
        Vector2 pos = block.Position;
        float maxSpeed = (block.finalBoss ? 130f : 160f);
        float speed = 0f;
        while (true)
        {
            speed = Calc.Approach(speed, maxSpeed, 500f * Engine.DeltaTime);
            if (block.MoveVCollideSolids(speed * Engine.DeltaTime, thruDashBlocks: true))
            {
                IEnumerator makeLame()
                {
                    while (true)
                    {
                        block.FallDelay = 10;
                        yield return null;
                    }
                };
                block.Add(new Coroutine(makeLame()));
                block.Add(new KeepGrounded());
                break;
            }
            if (block.Top > (float)(level.Bounds.Bottom + 16) || (block.Top > (float)(level.Bounds.Bottom - 1) && block.CollideCheck<Solid>(block.Position + new Vector2(0f, 1f))))
            {
                block.Collidable = (block.Visible = false);
                block.RemoveSelf();
                block.DestroyStaticMovers();
                break;
            }
        }
    }
    public static bool OnGround(this Solid solid)
    {
        Level level = solid.Scene as Level;
        return solid.CollideCheck<Solid>(solid.Position + Vector2.UnitY);
    }
    public static Vector2 GroundedPosition(this Entity entity)
    {
        Level level = entity.Scene as Level;
        Vector2 pos = entity.Position;
        Collider c = entity.Collider;
        entity.Collider ??= new Hitbox(1, 1);

        while (pos.Y < level.Bounds.Bottom && !entity.CollideCheck<Solid>(pos))
        {
            pos.Y++;
        }
        entity.Collider = c;

        return pos;
    }

    public static void Timer(this Scene scene, float time, Action onEnd)
    {
        scene.Add(new SceneTimer(time, onEnd));
    }

    public static Hitbox ColliderCentered(this MTexture texture)
    {
        return new Hitbox(texture.Width, texture.Height, -texture.Width / 2, -texture.Height / 2);
    }
    public static Hitbox ColliderCentered(this Image texture)
    {
        return texture.Texture.ColliderCentered();
    }
    public static Hitbox Collider(this MTexture texture)
    {
        return new Hitbox(texture.Width, texture.Height);
    }
    public static Hitbox Collider(this Image texture)
    {
        return texture.Texture.Collider();
    }

    public static IEnumerator Lerp(this Ease.Easer ease, float time, Action<float> action)
    {
        ease ??= Ease.Linear;
        for (float i = 0; i < 1; i += Engine.DeltaTime / time)
        {
            action?.Invoke(ease(i));
            yield return null;
        }
    }
    public static IEnumerator LerpYoyo(this Ease.Easer ease, float halfTime, Action<float> action)
    {
        ease ??= Ease.Linear;
        yield return ease.Lerp(halfTime, action);
        yield return Ease.Invert(ease).ReverseLerp(halfTime, action);
    }
    public static IEnumerator ReverseLerp(this Ease.Easer ease, float time, Action<float> action)
    {
        ease ??= Ease.Linear;
        for (float i = 0; i < 1; i += Engine.DeltaTime / time)
        {
            action?.Invoke(ease(1 - i));
            yield return null;
        }
    }

    public static Vector2 TopLeft(this Rectangle rect)
    {
        return new Vector2(rect.Left, rect.Top);
    }
    public static Vector2 TopRight(this Rectangle rect)
    {
        return new Vector2(rect.Right, rect.Top);
    }
    public static Vector2 BottomLeft(this Rectangle rect)
    {
        return new Vector2(rect.Left, rect.Bottom);
    }
    public static Vector2 BottomRight(this Rectangle rect)
    {
        return new Vector2(rect.Right, rect.Bottom);
    }
    public static Vector2 Center(this Rectangle rect)
    {
        return new Vector2((int)(rect.Left + rect.Width / 2f), (int)(rect.Top + rect.Height / 2f));
    }
    public static Vector2 TopCenter(this Rectangle rect)
    {
        return new Vector2((int)(rect.Left + rect.Width / 2f), rect.Top);
    }
    public static Vector2 BottomCenter(this Rectangle rect)
    {
        return new Vector2((int)(rect.Left + rect.Width / 2f), rect.Bottom);
    }
    public static Vector2 CenterLeft(this Rectangle rect)
    {
        return new Vector2(rect.Left, (int)(rect.Top + rect.Height / 2f));
    }
    public static Vector2 CenterRight(this Rectangle rect)
    {
        return new Vector2(rect.Right, (int)(rect.Top + rect.Height / 2f));
    }

    public static bool Colliding(this Rectangle rect, Rectangle check)
    {
        return !(check.Left >= rect.Right) && !(check.Right <= rect.Left) && !(check.Top >= rect.Bottom) && !(check.Bottom <= rect.Top);
    }

    public static Vector2 HalfSize(this MTexture texture)
    {
        return new Vector2(texture.Width / 2, texture.Height / 2);
    }
    public static Vector2 HalfSize(this Image image)
    {
        return image.Texture.HalfSize();
    }
    public static Vector2 Size(this MTexture texture)
    {
        return new Vector2(texture.Width, texture.Height);
    }
    public static Vector2 Size(this Image image)
    {
        return image.Texture.Size();
    }

    public static int Modulas(int input, int divisor)
    {
        return (input % divisor + divisor) % divisor;
    }
    public static Vector2 Mod(this Vector2 vec, float mod)
    {
        return new Vector2(vec.X - vec.X % mod, vec.Y - vec.Y % mod);
    }
    public static Vector2 Mod(this Vector2 vec, int mod)
    {
        return new Vector2(vec.X - vec.X % mod, vec.Y - vec.Y % mod);
    }
    public static Vector2 Mod(this Vector2 vec, Vector2 mod)
    {
        return new Vector2(vec.X - vec.X % mod.X, vec.Y - vec.Y % mod.Y);
    }

    public static void StandardBegin(this SpriteBatch spriteBatch)
    {
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone);
    }
    public static void StandardBegin(this SpriteBatch spriteBatch, Effect effect)
    {
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, effect);
    }
    public static void StandardBegin(this SpriteBatch spriteBatch, Matrix matrix)
    {
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, matrix);
    }
    public static void StandardBegin(this SpriteBatch spriteBatch, BlendState blend, Matrix matrix)
    {
        spriteBatch.Begin(SpriteSortMode.Deferred, blend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, matrix);
    }
    public static void StandardBegin(this SpriteBatch spriteBatch, BlendState blend, Effect shader)
    {
        spriteBatch.Begin(SpriteSortMode.Deferred, blend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, shader, Matrix.Identity);
    }
    public static void StandardBegin(this SpriteBatch spriteBatch, BlendState blend, Effect shader, Matrix matrix)
    {
        spriteBatch.Begin(SpriteSortMode.Deferred, blend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, shader, matrix);
    }
    public static void StandardBegin(this SpriteBatch spriteBatch, BlendState blend)
    {
        spriteBatch.Begin(SpriteSortMode.Deferred, blend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
    }
    public static void StandardBegin(this SpriteBatch spriteBatch, Effect effect, Matrix matrix)
    {
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, effect, matrix);
    }

    public static void SetRenderTarget(this VirtualRenderTarget source, Color? clear = null)
    {
        Engine.Graphics.GraphicsDevice.SetRenderTarget(source);
        if (clear.HasValue)
        {
            Engine.Graphics.GraphicsDevice.Clear(clear.Value);
        }
    }

    public static VirtualRenderTarget Apply(this VirtualRenderTarget source, VirtualRenderTarget bufferSameSizeAsSource, Effect effect, Vector2 position = default)
    {
        bufferSameSizeAsSource.SetRenderTarget(Color.Transparent);
        Draw.SpriteBatch.StandardBegin(effect);
        Draw.SpriteBatch.Draw((RenderTarget2D)source, position, Color.White);
        Draw.SpriteBatch.End();
        source.SetRenderTarget(Color.Transparent);
        Draw.SpriteBatch.StandardBegin(effect);
        Draw.SpriteBatch.Draw((RenderTarget2D)bufferSameSizeAsSource, position, Color.White);
        Draw.SpriteBatch.End();
        return source;

    }
    public static VirtualRenderTarget PrepareRenderTarget(VirtualRenderTarget target, string name, int width = 320, int height = 180)
    {
        if (target == null || target.IsDisposed) target = VirtualContent.CreateRenderTarget(name, width, height, false);
        return target;
    }
    public static VirtualRenderTarget DrawBounds(this VirtualRenderTarget target, Vector2 position, Color color, bool hollow)
    {
        if (hollow) Draw.HollowRect(position.X, position.Y, target.Width, target.Height, color);
        else Draw.Rect(position.X, position.Y, target.Width, target.Height, color);

        return target;
    }

    public static void DrawSimpleOutlines(this IEnumerable<GraphicsComponent> components)
    {
        foreach (GraphicsComponent g in components)
        {
            g.DrawSimpleOutline();
        }
    }
    public static void DrawOutlines(this IEnumerable<GraphicsComponent> components, int offset = 1)
    {
        foreach (GraphicsComponent g in components)
        {
            g.DrawOutline(offset);
        }
    }
    public static void DrawOutlines(this IEnumerable<GraphicsComponent> components, Color color, int offset = 1)
    {
        foreach (GraphicsComponent g in components)
        {
            g.DrawOutline(color, offset);
        }
    }
    public static void DrawSimpleOutline(this VirtualRenderTarget target, Vector2 position, Color color)
    {
        Draw.SpriteBatch.Draw(target, position + new Vector2(-1f, 0f), color);
        Draw.SpriteBatch.Draw(target, position + new Vector2(0f, -1f), color);
        Draw.SpriteBatch.Draw(target, position + new Vector2(1f, 0f), color);
        Draw.SpriteBatch.Draw(target, position + new Vector2(0f, 1f), color);
    }

    public static float ClampRange(float value, float range)
    {
        float min = Calc.Min(range, -range);
        float max = Calc.Max(range, -range);
        return Calc.Clamp(value, min, max);
    }
    public static Vector2 ClampRange(Vector2 value, Vector2 range)
    {
        return new Vector2(ClampRange(value.X, range.X), ClampRange(value.Y, range.Y));
    }
    public static Vector2 Clamp(this Vector2 vector, Rectangle rect)
    {
        return new Vector2(Calc.Clamp(vector.X, rect.Left, rect.Right), Calc.Clamp(vector.Y, rect.Top, rect.Bottom));
    }

    public static Rectangle Create(this Rectangle rect, float x, float y, float width, float height)
    {
        rect = new Rectangle((int)x, (int)y, (int)width, (int)height);
        return rect;
    }
    public static Rectangle CreateRectangle(float x, float y, float width, float height)
    {
        return new Rectangle((int)x, (int)y, (int)width, (int)height);
    }
    public static Rectangle CreateRectangle(Vector2 topLeft, Vector2 bottomRight)
    {
        return new Rectangle((int)topLeft.X, (int)topLeft.Y, (int)bottomRight.X - (int)topLeft.X, (int)bottomRight.Y - (int)topLeft.Y);
    }
    public static Rectangle CloneRectangle(Rectangle from)
    {
        return new Rectangle(from.Left, from.Top, from.Width, from.Height);
    }

    public static Vector2[] GetNodes(BinaryPacker.Element element, bool withPosition = false, Vector2 offset = default)
    {
        List<Vector2> output = new();
        if (withPosition)
        {
            output.Add(Vector2.Zero);
        }
        for (int i = 0; i < (element.Children != null ? element.Children.Count : 0); i++)
        {
            Vector2 nodePosition = Vector2.Zero;
            foreach (KeyValuePair<string, object> attribute2 in element.Children[i].Attributes)
            {
                if (attribute2.Key == "x")
                {
                    nodePosition.X = Convert.ToSingle(attribute2.Value, CultureInfo.InvariantCulture);
                }
                else if (attribute2.Key == "y")
                {
                    nodePosition.Y = Convert.ToSingle(attribute2.Value, CultureInfo.InvariantCulture);
                }
            }
            output.Add(nodePosition);
        }
        for (int i = 0; i < output.Count; i++)
        {
            output[i] += offset;
        }

        return output.ToArray();
    }

    public static T[] Initialize<T>(Func<T> setValue, int length)
    {
        T[] array = new T[length];
        if (setValue is null || setValue.Invoke() == null) return null;
        for (int i = 0; i < length; i++)
        {
            array[i] = setValue.Invoke();
        }
        return array;
    }
    public static T[] Initialize<T>(T value, int length)
    {
        T[] array = new T[length];
        if (value is null) return null;
        for (int i = 0; i < length; i++)
        {
            array[i] = value;
        }
        return array;
    }

    public static Vector2 Center(this IEnumerable<VertexPositionColor> list)
    {
        Vector2 center = Vector2.Zero;
        foreach (VertexPositionColor v in list)
        {
            center += new Vector2(v.Position.X, v.Position.Y);
        }
        center /= list.Count();
        return center;
    }
    public static Vector2 Sum(this IEnumerable<Vector2> positions)
    {
        Vector2 val = Vector2.Zero;
        foreach (Vector2 v in positions)
        {
            val += v;
        }
        return val;
    }
    public static VertexPositionColor Create(Vector2 position, Color color)
    {
        return new VertexPositionColor(new Vector3(position, 0), color);
    }

    public static Collider Boundaries(this IEnumerable<Vector2> positions, Vector2 offset, int mult = 1)
    {
        float left, top, right, bottom;
        left = top = float.MaxValue;
        right = bottom = float.MinValue;
        foreach (Vector2 vector in positions)
        {
            left = Math.Min(vector.X, left);
            right = Math.Max(vector.X, right);
            top = Math.Min(vector.Y, top);
            bottom = Math.Max(vector.Y, bottom);
        }
        return new Hitbox((right - left) * mult, (bottom - top) * mult, (left * mult) + offset.X, (top * mult) + offset.Y);
    }

    public static FancySolidTiles Create(Vector2 position, float width, float height, string tileData, bool blendEdges)
    {
        EntityData BlockData = new EntityData
        {
            Name = "FancyTileEntities/FancySolidTiles",
            Position = position,
        };
        BlockData.Values = new()
            {
                {"randomSeed", Calc.Random.Next()},
                {"blendEdges", blendEdges },
                {"width", width },
                {"height", height },
                {"tileData", tileData }
            };
        return new FancySolidTiles(BlockData, Vector2.Zero, new EntityID());
    }

    public static Vector2 RandomFrom(this Vector2 vec, float xMin, float xMax, float yMin, float yMax)
    {
        vec = Random(xMin, xMax, yMin, yMax) + vec;
        return vec;
    }
    public static Ease.Easer Random()
    {
        return Calc.Random.Choose(Ease.BackIn, Ease.BackInOut, Ease.BackOut, Ease.BigBackIn, Ease.BigBackInOut, Ease.BigBackOut,
            Ease.BounceIn, Ease.BounceOut, Ease.BounceInOut, Ease.CubeIn, Ease.CubeOut, Ease.CubeInOut, Ease.ElasticIn, Ease.ElasticInOut,
            Ease.ElasticOut, Ease.ExpoIn, Ease.ExpoOut, Ease.ExpoInOut, Ease.Linear, Ease.QuadIn, Ease.QuadOut, Ease.QuadInOut, Ease.QuintIn,
            Ease.QuintOut, Ease.QuintInOut, Ease.SineIn, Ease.SineOut, Ease.SineInOut); //smile :)
    }
    public static Vector2 Random(float minX, float maxX, float minY, float maxY)
    {
        return new Vector2(Calc.Random.Range(minX, maxX), Calc.Random.Range(minY, maxY));
    }
    public static Vector2 Random(Vector2 min, Vector2 max)
    {
        return Random(min.X, max.X, min.Y, max.Y);
    }
    public static Color Random(this Color color, bool r, bool g, bool b, bool a)
    {
        byte red, green, blue, alpha;
        red = r ? (byte)Calc.Random.Range(0, 256) : color.R;
        green = g ? (byte)Calc.Random.Range(0, 256) : color.G;
        blue = b ? (byte)Calc.Random.Range(0, 256) : color.B;
        alpha = a ? (byte)Calc.Random.Range(0, 256) : color.A;
        color = new Color(red, green, blue, alpha);
        return color;
    }
    public static Color RandomColor(bool r, bool g, bool b, bool a)
    {
        float red, green, blue, alpha;
        red = r ? Calc.Random.Range(0, 256) : 0;
        green = g ? Calc.Random.Range(0, 256) : 0;
        blue = b ? Calc.Random.Range(0, 256) : 0;
        alpha = a ? Calc.Random.Range(0, 256) : 256;
        return new Color(red, green, blue, alpha);
    }
    public static T Random<T>(this T[] array, int limit = -1)
    {
        if (limit < 0 || limit >= array.Length)
        {
            limit = array.Length;
        }
        return array[Calc.Random.Range(0, limit)];
    }

    public static string GetDescription<T>(this T enumerationValue) where T : struct
    {
        Type type = enumerationValue.GetType();
        if (!type.IsEnum)
        {
            throw new ArgumentException("EnumerationValue must be of Enum type", "enumerationValue");
        }

        //Tries to find a DescriptionAttribute for a potential friendly name
        //for the enum
        MemberInfo[] memberInfo = type.GetMember(enumerationValue.ToString());
        if (memberInfo != null && memberInfo.Length > 0)
        {
            object[] attrs = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attrs != null && attrs.Length > 0)
            {
                //Pull out the description value
                return ((DescriptionAttribute)attrs[0]).Description;
            }
        }
        //If we have no description attribute, just return the ToString of the enum
        return enumerationValue.ToString();
    }

    public static void SeamlessTeleport(Scene scene, Player player, string nextLevel, bool relative)
    {

        //Todo: transfer tileseed
        //Todo: affect backgrounds
        //Todo: affect particles
        //Todo: affect dash snapshots
        Level level = scene as Level;
        level.OnEndOfFrame += delegate
        {
            LevelData nextData = level.Session.MapData.Get(nextLevel);
            if (nextData == null) return;

            Vector2 levelOffset = level.LevelOffset;
            Vector2 playerPositionInLevel = player.Position - levelOffset;
            Vector2 cameraPositionInLevel = level.Camera.Position - levelOffset;
            Facings facing = player.Facing;

            List<TrailManager.Snapshot> snapshots = new();
            List<Vector2> shotPositions = new();
            foreach (TrailManager.Snapshot shot in level.Tracker.GetEntities<TrailManager.Snapshot>())
            {
                snapshots.Add(shot);
                shotPositions.Add(shot.Position - levelOffset);
            }

            level.Remove(player);
            level.Displacement.Clear();
            level.UnloadLevel();
            level.Session.Level = nextLevel;

            Session session = level.Session;
            Level level2 = level;
            session.RespawnPoint = session.MapData.Get(nextLevel) is LevelData data && relative ? data.Position + playerPositionInLevel : level.Bounds.TopLeft();

            level.Session.FirstLevel = false;
            level.Add(player);
            level.LoadLevel(IntroTypes.Transition);
            level.Camera.Position = level.LevelOffset + cameraPositionInLevel;
            level.Wipe?.Cancel();

            player.Hair.MoveHairBy(level.LevelOffset - levelOffset);
            player.Position = level.LevelOffset + playerPositionInLevel;
            player.Facing = facing;
            /*
                        for (int i = 0; i < snapshots.Count; i++)
                        {
                            snapshots[i].Position = level.LevelOffset + shotPositions[i];
                        }*/
        };
    }
    public static void TeleportTo(Scene scene, Player player, string room, Player.IntroTypes introType = Player.IntroTypes.Transition, Vector2? nearestSpawn = null)
    {
        if (scene is Level level)
        {
            level.OnEndOfFrame += delegate
            {
                level.TeleportTo(player, room, introType, nearestSpawn);
            };
        }
    }
    public static void InstantRelativeTeleport(Scene scene, string room, bool snapToSpawnPoint, int positionX = 0, int positionY = 0)
    {
        Level level = scene as Level;
        Player player = level.GetPlayer();
        if (level == null || player == null)
        {
            return;
        }
        if (string.IsNullOrEmpty(room))
        {
            return;
        }
        level.OnEndOfFrame += delegate
        {
            Vector2 levelOffset = level.LevelOffset;
            Vector2 val2 = player.Position - levelOffset;
            Vector2 val3 = level.Camera.Position - levelOffset;
            Vector2 offset = new Vector2(positionY, positionX);
            Facings facing = player.Facing;
            level.Remove(player);
            level.UnloadLevel();
            level.Session.Level = room;
            Session session = level.Session;
            Level level2 = level;
            Rectangle bounds = level.Bounds;
            float num = bounds.Left;
            bounds = level.Bounds;
            session.RespawnPoint = level2.GetSpawnPoint(new Vector2(num, bounds.Top));
            level.Session.FirstLevel = false;
            level.LoadLevel(Player.IntroTypes.None);

            level.Camera.Position = level.LevelOffset + val3 + offset.Floor();
            level.Add(player);
            if (snapToSpawnPoint && session.RespawnPoint.HasValue)
            {
                player.Position = session.RespawnPoint.Value + offset.Floor();
            }
            else
            {
                player.Position = level.LevelOffset + val2 + offset.Floor();
            }

            player.Facing = facing;
            player.Hair.MoveHairBy(level.LevelOffset - levelOffset + offset.Floor());
            if (level.Wipe != null)
            {
                level.Wipe.Cancel();
            }
        };
    }
    public static void InstantRelativeTeleport(Scene scene, string room, bool snapToSpawnPoint)
    {
        InstantRelativeTeleport(scene, room, snapToSpawnPoint, 0, 0);
    }
    public static void InstantTeleport(Scene scene, string room, Vector2 newPosition)
    {
        InstantTeleport(scene, room, newPosition.X, newPosition.Y);
    }
    public static void InstantTeleport(Scene scene, string room, float positionX, float positionY)
    {
        Level level = scene as Level;
        Player player = level.GetPlayer();
        if (level == null || player == null)
        {
            return;
        }
        if (string.IsNullOrEmpty(room))
        {
            Vector2 val = new Vector2(positionX, positionY) - player.Position;
            player.Position = new Vector2(positionX, positionY);
            Camera camera = level.Camera;
            camera.Position += val;
            player.Hair.MoveHairBy(val);
            return;
        }
        level.OnEndOfFrame += delegate
        {
            Vector2 levelOffset = level.LevelOffset;
            Vector2 val2 = player.Position - level.LevelOffset;
            Vector2 val3 = level.Camera.Position - level.LevelOffset;
            Facings facing = player.Facing;
            level.Remove(player);
            level.UnloadLevel();
            level.Session.Level = room;
            Session session = level.Session;
            Level level2 = level;
            Rectangle bounds = level.Bounds;
            float num = bounds.Left;
            bounds = level.Bounds;
            session.RespawnPoint = level2.GetSpawnPoint(new Vector2(num, bounds.Top));
            level.Session.FirstLevel = false;
            level.LoadLevel(Player.IntroTypes.Transition);

            Vector2 val4 = new Vector2(positionX, positionY) - level.LevelOffset - val2;
            level.Camera.Position = level.LevelOffset + val3 + val4;
            level.Add(player);
            player.Position = new Vector2(positionX, positionY);
            player.Facing = facing;
            player.Hair.MoveHairBy(level.LevelOffset - levelOffset + val4);

            if (level.Wipe != null)
            {
                level.Wipe.Cancel();
            }
        };
    }

    public static List<T> CheckAdd<T>(this List<T> list, T value)
    {
        if (!list.Contains(value))
        {
            list.Add(value);
        }
        return list;
    }

    public static Entity MakeGlobal(this Entity entity)
    {
        entity.AddTag(Tags.Global);
        entity.AddTag(Tags.Persistent);
        return entity;
    }
    public static Entity MakeLocal(this Entity entity)
    {
        entity.RemoveTag(Tags.Global);
        entity.RemoveTag(Tags.Persistent);
        return entity;
    }

    public static Vector2 Marker(this Scene scene, string id, bool screenSpace = false)
    {
        List<Entity> markers = (scene as Level).Tracker.GetEntities<Marker>();
        if (markers != null && markers.Count > 0)
        {
            foreach (Marker m in markers)
            {
                if (m.ID == id)
                {
                    return screenSpace ? (scene as Level).Camera.CameraToScreen(m.Center) : m.Center;
                }
            }
        }
        return Vector2.Zero;
    }
    public static Vector2 MarkerCentered(this Scene scene, string id, bool screenSpace = false)
    {
        return scene.Marker(id, screenSpace) - new Vector2(160, 90);
    }

    public static Facings Flip(this Facings facing)
    {
        return (Facings)(-(int)facing);
    }

    public static IEnumerator AutoTraverseRelative(this Player player, float distance)
    {
        yield return player.AutoTraverse(player.X + distance);
    }
    public static IEnumerator AutoPlay(this Player player, int xDir, bool walkBackwards = false, float speedMultiplier = 1f)
    {

        int sign = Math.Sign(xDir);
        player.StateMachine.State = 11;
        if (!player.Dead)
        {
            player.DummyMoving = true;
            if (walkBackwards)
            {
                player.Sprite.Rate = -1f;
                player.Facing = (Facings)sign;
            }
            else
            {
                player.Facing = (Facings)(-sign);
            }

            while (player != null && !player.Dead && player.Scene != null)
            {
                Vector2 referencePoint = sign == 1 ? player.BottomRight : player.BottomLeft;
                if (player.OnGround())
                {
                    player.AutoJump = false;
                    player.AutoJumpTimer = 0;
                    Vector2 inFrontPosition = referencePoint + (Vector2.UnitX * sign * 8);
                    bool wallInFront = player.CollideCheck<Solid>(inFrontPosition);
                    bool gapInFront = !player.CollideCheck<Solid>(referencePoint + new Vector2(sign * 8, 8));
                    bool foundLedge = false;
                    for (int i = 8; i < 17; i += 8)
                    {
                        if (!player.CollideCheck<Solid>(inFrontPosition - (Vector2.UnitY * i)))
                        {
                            foundLedge = true;
                        }
                    }
                    bool shouldJump = gapInFront || (foundLedge && wallInFront);
                    if (wallInFront && !foundLedge)
                    {
                        player.Facing.Flip();
                        sign = -sign;
                    }
                    if (shouldJump)
                    {
                        player.Jump();
                        player.AutoJump = true;
                        player.AutoJumpTimer = 2;
                    }
                }
                player.Speed.X = Calc.Approach(player.Speed.X, sign * 64f * speedMultiplier, 1000f * Engine.DeltaTime);
                yield return null;
            }

            player.Sprite.Rate = 1f;
            player.Sprite.Play("idle");
            player.DummyMoving = false;
        }
        yield return null;
    }
    public static IEnumerator AutoTraverse(this Player player, float x, bool walkBackwards = false, float speedMultiplier = 1f)
    {
        int sign = Math.Sign(x - player.X);
        player.StateMachine.State = 11;
        if (Math.Abs(player.X - x) > 4f && !player.Dead)
        {
            player.DummyMoving = true;
            if (walkBackwards)
            {
                player.Sprite.Rate = -1f;
                player.Facing = (Facings)Math.Sign(player.X - x);
            }
            else
            {
                player.Facing = (Facings)Math.Sign(x - player.X);
            }

            while (Math.Abs(x - player.X) > 4f && player.Scene != null)
            {
                Vector2 referencePoint = sign == 1 ? player.BottomRight : player.BottomLeft;
                if (player.OnGround())
                {
                    player.AutoJump = false;
                    player.AutoJumpTimer = 0;
                    Vector2 inFrontPosition = referencePoint + (Vector2.UnitX * sign * 8);
                    bool wallInFront = player.CollideCheck<Solid>(inFrontPosition);
                    bool gapInFront = !player.CollideCheck<Solid>(referencePoint + new Vector2(sign * 8, 8));
                    bool foundJumpThru = false;
                    if (gapInFront)
                    {
                        foundJumpThru = player.CollideCheck<JumpThru>(inFrontPosition + Vector2.UnitY * 2);
                    }
                    bool foundLedge = false;
                    for (int i = 8; i < 17; i += 4)
                    {
                        if (!player.CollideCheck<Solid>(inFrontPosition - (Vector2.UnitY * i)))
                        {
                            foundLedge = true;
                        }
                    }

                    bool shouldJump = (gapInFront && !foundJumpThru) || (foundLedge && wallInFront);
                    if (shouldJump)
                    {
                        player.Jump();
                        player.AutoJump = true;
                        player.AutoJumpTimer = 2;
                    }
                }
                player.Speed.X = Calc.Approach(player.Speed.X, (float)Math.Sign(x - player.X) * 64f * speedMultiplier, 1000f * Engine.DeltaTime);
                yield return null;
            }

            player.Sprite.Rate = 1f;
            player.Sprite.Play("idle");
            player.DummyMoving = false;
        }
        yield return null;
    }

    public static Vector2 RotateAround(this Vector2 pointToRotate, Vector2 centerPoint, double angleInDegrees)
    {
        double angleInRadians = angleInDegrees * (Math.PI / 180);
        double cosTheta = Math.Cos(angleInRadians);
        double sinTheta = Math.Sin(angleInRadians);
        return new Vector2
        {
            X =
                (int)
                (cosTheta * (pointToRotate.X - centerPoint.X) -
                sinTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.X),
            Y =
                (int)
                (sinTheta * (pointToRotate.X - centerPoint.X) +
                cosTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.Y)
        };
    }
    public static Vector2 RotatePoint(Vector2 pointToRotate, Vector2 centerPoint, double angleInDegrees)
    {
        double angleInRadians = angleInDegrees * (Math.PI / 180);
        double cosTheta = Math.Cos(angleInRadians);
        double sinTheta = Math.Sin(angleInRadians);
        return new Vector2
        {
            X =
                (int)
                (cosTheta * (pointToRotate.X - centerPoint.X) -
                sinTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.X),
            Y =
                (int)
                (sinTheta * (pointToRotate.X - centerPoint.X) +
                cosTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.Y)
        };
    }

    public static Player GetPlayer(this Level level)
    {
        if (level is null)
        {
            return null;
        }
        return level.Tracker.GetEntity<Player>();
    }
    public static Player GetPlayer(this Scene scene)
    {
        return (scene as Level).GetPlayer();
    }

    public static Vector2? DoRaycast(Scene scene, Vector2 start, Vector2 end)
    => DoRaycast(scene.Tracker.GetEntities<Solid>().Select(s => s.Collider), start, end);
    public static Vector2? DoRaycast(IEnumerable<Collider> cols, Vector2 start, Vector2 end)
    {
        Vector2? curPoint = null;
        float curDst = float.PositiveInfinity;
        foreach (Collider c in cols)
        {
            if (DoRaycast(c, start, end) is not Vector2 intersectionPoint) continue;
            float dst = Vector2.DistanceSquared(start, intersectionPoint);
            if (dst < curDst)
            {
                curPoint = intersectionPoint;
                curDst = dst;
            }
        }
        return curPoint;
    }
    public static Vector2? DoRaycast(Collider col, Vector2 start, Vector2 end) => col switch
    {
        Hitbox hbox => DoRaycast(hbox, start, end),
        Grid grid => DoRaycast(grid, start, end),
        ColliderList colList => DoRaycast(colList.colliders, start, end),
        _ => null //Unknown collider type
    };
    public static Vector2? DoRaycast(Hitbox hbox, Vector2 start, Vector2 end)
    {
        start -= hbox.AbsolutePosition;
        end -= hbox.AbsolutePosition;

        Vector2 dir = Vector2.Normalize(end - start);
        float tmin = float.NegativeInfinity, tmax = float.PositiveInfinity;

        if (dir.X != 0)
        {
            float tx1 = (hbox.Left - start.X) / dir.X, tx2 = (hbox.Right - start.X) / dir.X;
            tmin = Math.Max(tmin, Math.Min(tx1, tx2));
            tmax = Math.Min(tmax, Math.Max(tx1, tx2));
        }
        else if (start.X < hbox.Left || start.X > hbox.Right) return null;

        if (dir.Y != 0)
        {
            float ty1 = (hbox.Top - start.Y) / dir.Y, ty2 = (hbox.Bottom - start.Y) / dir.Y;
            tmin = Math.Max(tmin, Math.Min(ty1, ty2));
            tmax = Math.Min(tmax, Math.Max(ty1, ty2));
        }
        else if (start.Y < hbox.Top || start.Y > hbox.Bottom) return null;

        return (0 <= tmin && tmin <= tmax && tmin * tmin <= Vector2.DistanceSquared(start, end)) ? hbox.AbsolutePosition + start + tmin * dir : null;
    }
    public static Vector2? DoRaycast(Grid grid, Vector2 start, Vector2 end)
    {
        start = (start - grid.AbsolutePosition) / new Vector2(grid.CellWidth, grid.CellHeight);
        end = (end - grid.AbsolutePosition) / new Vector2(grid.CellWidth, grid.CellHeight);
        Vector2 dir = Vector2.Normalize(end - start);
        int xDir = Math.Sign(end.X - start.X), yDir = Math.Sign(end.Y - start.Y);
        if (xDir == 0 && yDir == 0) return null;
        int gridX = (int)start.X, gridY = (int)start.Y;
        float nextX = xDir < 0 ? (float)Math.Ceiling(start.X) - 1 : xDir > 0 ? (float)Math.Floor(start.X) + 1 : float.PositiveInfinity;
        float nextY = yDir < 0 ? (float)Math.Ceiling(start.Y) - 1 : yDir > 0 ? (float)Math.Floor(start.Y) + 1 : float.PositiveInfinity;
        while (Math.Sign(end.X - start.X) != -xDir || Math.Sign(end.Y - start.Y) != -yDir)
        {
            if (grid[gridX, gridY])
            {
                return grid.AbsolutePosition + start * new Vector2(grid.CellWidth, grid.CellHeight);
            }
            if (Math.Abs((nextX - start.X) * dir.Y) < Math.Abs((nextY - start.Y) * dir.X))
            {
                start.Y += Math.Abs((nextX - start.X) / dir.X) * dir.Y;
                start.X = nextX;
                nextX += xDir;
                gridX += xDir;
            }
            else
            {
                start.X += Math.Abs((nextY - start.Y) / dir.Y) * dir.X;
                start.Y = nextY;
                nextY += yDir;
                gridY += yDir;
            }
        }
        return null;
    }

    public static bool CheckSolidsGrid(Vector2 at)
    {
        if (Engine.Scene is not Level level) return false;
        Grid grid = level.SolidTiles.Grid;
        at = (at - grid.AbsolutePosition) / new Vector2(grid.CellWidth, grid.CellHeight);
        return grid[(int)at.X, (int)at.Y];
    }
    public static LevelData SwitchedData(this LevelData toSwitch, LevelData switchWith)
    {
        toSwitch.Bg = switchWith.Bg;
        toSwitch.Solids = switchWith.Solids;
        return toSwitch;
    }

    public static T SeekController<T>(Scene scene, Func<T> factory = null) where T : Entity
    {
        T controller = scene.Tracker.GetEntity<T>();

        if (controller is not null)
        {
            return controller;
        }

        foreach (Entity entity in scene.Entities.ToAdd)
        {
            if (entity is T t)
            {
                return t;
            }
        }

        if (factory is null)
        {
            return null;
        }

        scene.Add(controller = factory());
        return controller;
    }

    public static string ReadModAsset(string filename)
    {
        return Everest.Content.TryGet(filename, out var asset) ? ReadModAsset(asset) : null;
    }
    public static string ReadModAsset(ModAsset asset)
    {
        using var reader = new StreamReader(asset.Stream);

        return reader.ReadToEnd();
    }
}
