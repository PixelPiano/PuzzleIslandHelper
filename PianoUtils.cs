// PuzzleIslandHelper.PuzzleIslandHelperCommands
using Celeste;
using Celeste.Mod;
using Celeste.Mod.CommunalHelper;
using Celeste.Mod.CommunalHelper.Utils;
using Celeste.Mod.FancyTileEntities;
using Celeste.Mod.PuzzleIslandHelper;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.WARP;
using Celeste.Mod.PuzzleIslandHelper.Entities.WIP;
using Celeste.Mod.XaphanHelper.Effects;
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
using static Celeste.Autotiler;
using static Celeste.ClutterBlock;
using static Celeste.Mod.PuzzleIslandHelper.Entities.Pulse;
using static Celeste.Player;
/// <summary>A collection of methods + extension methods used primarily in PuzzleIslandHelper.</summary>
public static class PianoUtils
{
    public static void Cycle<T>(this Tuple<T, T> tuple)
    {
        var item1 = tuple.Item1;
        var item2 = tuple.Item2;
        tuple = new Tuple<T, T>(item2, item1);
    }
    public static Hitbox Collider(this IEnumerable<Vector2> list)
    {
        float left = int.MaxValue, top = int.MaxValue, right = int.MinValue, bottom = int.MinValue;
        foreach (Vector2 v in list)
        {
            left = Math.Min(left, v.X);
            top = Math.Min(top, v.Y);
            right = Math.Max(right, v.X);
            bottom = Math.Max(bottom, v.Y);
        }
        return new Hitbox(right - left, bottom - top);
    }
    public static Vector2 Derivative(Vector2 start, Vector2 end, float percent, float deviation)
    {
        Vector2 point = Vector2.Lerp(start, end, percent);
        Vector2 point2 = Vector2.Lerp(start, end, percent + deviation);
        Vector2 d = new Vector2(point2.X - point.X, point2.Y - point.Y);
        return d / deviation;
    }
    public static void HollowRect(float x, float y, float width, float height, Color color, int thickness)
    {
        Draw.rect.X = (int)x;
        Draw.rect.Y = (int)y;
        Draw.rect.Width = (int)width;
        Draw.rect.Height = thickness;
        Draw.SpriteBatch.Draw(Draw.Pixel.Texture.Texture_Safe, Draw.rect, Draw.Pixel.ClipRect, color);
        Draw.rect.Y += (int)height - 1;
        Draw.SpriteBatch.Draw(Draw.Pixel.Texture.Texture_Safe, Draw.rect, Draw.Pixel.ClipRect, color);
        Draw.rect.Y -= (int)height - 1;
        Draw.rect.Width = thickness;
        Draw.rect.Height = (int)height;
        Draw.SpriteBatch.Draw(Draw.Pixel.Texture.Texture_Safe, Draw.rect, Draw.Pixel.ClipRect, color);
        Draw.rect.X += (int)width - 1;
        Draw.SpriteBatch.Draw(Draw.Pixel.Texture.Texture_Safe, Draw.rect, Draw.Pixel.ClipRect, color);
    }
    public static void HollowRect(Vector2 position, float width, float height, Color color, int thickness) => HollowRect(position.X, position.Y, width, height, color, thickness);
    public static void DrawOutline(this FancyText.Char c, PixelFont font, float baseSize, Vector2 position, Vector2 scale, float alpha)
    {
        float num = (c.Impact ? (2f - c.Fade) : 1f) * c.Scale;
        Vector2 zero = Vector2.Zero;
        Vector2 vector = scale * num;
        PixelFontSize pixelFontSize = font.Get(baseSize * Math.Max(vector.X, vector.Y));
        PixelFontCharacter pixelFontCharacter = pixelFontSize.Get(c.Character);
        vector *= baseSize / pixelFontSize.Size;
        position.X += c.Position * scale.X;
        zero += (c.Shake ? (new Vector2(-1 + Calc.Random.Next(3), -1 + Calc.Random.Next(3)) * 2f) : Vector2.Zero);
        zero += (c.Wave ? new Vector2(0f, (float)Math.Sin((float)c.Index * 0.25f + Engine.Scene.RawTimeActive * 8f) * 4f) : Vector2.Zero);
        zero.X += pixelFontCharacter.XOffset;
        zero.Y += (float)pixelFontCharacter.YOffset + (-8f * (1f - c.Fade) + c.YOffset * c.Fade);

        pixelFontCharacter.Texture.Draw(position + zero * vector + new Vector2(1, 1), Vector2.Zero, Color.Black * c.Fade * alpha, vector, c.Rotation);
        pixelFontCharacter.Texture.Draw(position + zero * vector + new Vector2(1, -1), Vector2.Zero, Color.Black * c.Fade * alpha, vector, c.Rotation);
        pixelFontCharacter.Texture.Draw(position + zero * vector + new Vector2(-1, 1), Vector2.Zero, Color.Black * c.Fade * alpha, vector, c.Rotation);
        pixelFontCharacter.Texture.Draw(position + zero * vector + new Vector2(-1, -1), Vector2.Zero, Color.Black * c.Fade * alpha, vector, c.Rotation);
        pixelFontCharacter.Texture.Draw(position + zero * vector, Vector2.Zero, c.Color * c.Fade * alpha, vector, c.Rotation);
    }
    public static void DrawOutlineJustifyPerLine(this FancyText.Text text, Vector2 position, Vector2 justify, Vector2 scale, float alpha, int start = 0, int end = int.MaxValue)
    {
        int num = Math.Min(text.Nodes.Count, end);
        float num2 = 0f;
        float num3 = 0f;
        PixelFontSize pixelFontSize = text.Font.Get(text.BaseSize);
        for (int i = start; i < num; i++)
        {
            if (text.Nodes[i] is FancyText.NewLine)
            {
                if (num2 == 0f)
                {
                    num2 = 1f;
                }

                num3 += num2;
                num2 = 0f;
            }
            else if (text.Nodes[i] is FancyText.Char)
            {
                num2 = Math.Max(num2, (text.Nodes[i] as FancyText.Char).Scale);
            }
            else if (text.Nodes[i] is FancyText.NewPage)
            {
                break;
            }
        }

        num3 += num2;
        num2 = 0f;
        for (int j = start; j < num && !(text.Nodes[j] is FancyText.NewPage); j++)
        {
            if (text.Nodes[j] is FancyText.NewLine)
            {
                if (num2 == 0f)
                {
                    num2 = 1f;
                }

                position.Y += num2 * (float)pixelFontSize.LineHeight * scale.Y;
                num2 = 0f;
            }

            if (text.Nodes[j] is FancyText.Char)
            {
                FancyText.Char @char = text.Nodes[j] as FancyText.Char;
                Vector2 vector = -justify * new Vector2(@char.LineWidth, num3 * (float)pixelFontSize.LineHeight) * scale;
                DrawOutline(@char, text.Font, text.BaseSize, position + vector, scale, alpha);
                num2 = Math.Max(num2, @char.Scale);
            }
        }
    }

    public enum TransitionDirection
    {
        None = 0,
        Right = 1,
        Down = 2,
        Left = 3,
        Up = 4
    }
    public static TransitionDirection GetTransitionDirection(this Player player)
    {
        MapData mapData = player.SceneAs<Level>().Session.MapData;
        Vector2 center = player.Center;
        const int TileSize = 8;

        if (mapData.GetAt(center + Vector2.UnitX * TileSize) is not null)
            return TransitionDirection.Right;
        if (mapData.GetAt(center + Vector2.UnitY * (TileSize + TileSize / 2)) is not null)
            return TransitionDirection.Down;
        if (mapData.GetAt(center - Vector2.UnitX * TileSize) is not null)
            return TransitionDirection.Left;
        if (mapData.GetAt(center - Vector2.UnitY * (TileSize + TileSize / 2)) is not null)
            return TransitionDirection.Up;
        return TransitionDirection.None;
    }
    public static void MagicGlowRender(Texture2D texture, Vector2 position, Color color, float noiseEase, float direction, Matrix matrix)
    {
        GFX.FxMagicGlow.Parameters["alpha"].SetValue(0.5f);
        GFX.FxMagicGlow.Parameters["pixel"].SetValue(new Vector2(1f / (float)texture.Width, 1f / (float)texture.Height) * 3f);
        GFX.FxMagicGlow.Parameters["noiseSample"].SetValue(new Vector2(1f, 0.5f));
        GFX.FxMagicGlow.Parameters["noiseDistort"].SetValue(new Vector2(1f, 1f));
        GFX.FxMagicGlow.Parameters["noiseEase"].SetValue(noiseEase * 0.05f);
        GFX.FxMagicGlow.Parameters["direction"].SetValue(0f - direction);
        Engine.Graphics.GraphicsDevice.Textures[1] = GFX.MagicGlowNoise.Texture_Safe;
        Engine.Graphics.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, GFX.FxMagicGlow, matrix);
        Draw.SpriteBatch.Draw(texture, position, color);
        Draw.SpriteBatch.End();
    }
    public static void MagicGlowRender(List<Texture2D> textures, Vector2 position, Color color, float noiseEase, float direction, Matrix matrix)
    {
        GFX.FxMagicGlow.Parameters["alpha"].SetValue(0.5f);
        GFX.FxMagicGlow.Parameters["pixel"].SetValue(new Vector2(1f / (float)textures[0].Width, 1f / (float)textures[0].Height) * 3f);
        GFX.FxMagicGlow.Parameters["noiseSample"].SetValue(new Vector2(1f, 0.5f));
        GFX.FxMagicGlow.Parameters["noiseDistort"].SetValue(new Vector2(1f, 1f));
        GFX.FxMagicGlow.Parameters["noiseEase"].SetValue(noiseEase * 0.05f);
        GFX.FxMagicGlow.Parameters["direction"].SetValue(0f - direction);
        Engine.Graphics.GraphicsDevice.Textures[1] = GFX.MagicGlowNoise.Texture_Safe;
        Engine.Graphics.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, GFX.FxMagicGlow, matrix);
        foreach (var t in textures)
        {
            Draw.SpriteBatch.Draw(t, position, color);
        }
        Draw.SpriteBatch.End();
    }
    public static void MagicGlowRender(List<Image> textures, Vector2 offset, float noiseEase, float direction, Matrix matrix)
    {
        GFX.FxMagicGlow.Parameters["alpha"].SetValue(0.5f);
        GFX.FxMagicGlow.Parameters["pixel"].SetValue(new Vector2(1f / (float)textures[0].Width, 1f / (float)textures[0].Height) * 3f);
        GFX.FxMagicGlow.Parameters["noiseSample"].SetValue(new Vector2(1f, 0.5f));
        GFX.FxMagicGlow.Parameters["noiseDistort"].SetValue(new Vector2(1f, 1f));
        GFX.FxMagicGlow.Parameters["noiseEase"].SetValue(noiseEase * 0.05f);
        GFX.FxMagicGlow.Parameters["direction"].SetValue(0f - direction);
        Engine.Graphics.GraphicsDevice.Textures[1] = GFX.MagicGlowNoise.Texture_Safe;
        Engine.Graphics.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, GFX.FxMagicGlow, matrix);
        foreach (var t in textures)
        {
            Draw.SpriteBatch.Draw(t.Texture.Texture.Texture_Safe, t.RenderPosition + offset, t.Color);
        }
        Draw.SpriteBatch.End();
    }
    public static IEnumerator CutAtEnd(string dialog, params Func<IEnumerator>[] events)
    {
        bool close = false;
        IEnumerator stop() { close = true; yield break; }
        Textbox textbox = new Textbox(dialog, [.. events.Prepend(stop)]);
        Engine.Scene.Add(textbox);
        while (!close)
        {
            yield return null;
        }
        yield return textbox.EaseClose(true);
        textbox.Close();
    }
    public static EntityID GenerateRandomID() => new EntityID(Guid.NewGuid().ToString(), 0);
    public static List<Image> NineSlice(this MTexture texture, int width, int height, int segmentWidth = 8, int segmentHeight = 8)
    {
        List<Image> list = [];
        if (width < segmentWidth || height < segmentHeight) return list;
        for (int x = 0; x < width; x += segmentWidth)
        {
            for (int y = 0; y < height; y += segmentHeight)
            {
                int quadX = x < segmentWidth ? 0 : x >= width - segmentWidth ? width - segmentWidth : segmentWidth;
                int quadY = y < segmentHeight ? 0 : y >= height - segmentHeight ? height - segmentHeight : segmentHeight;
                Image image = new Image(texture.GetSubtexture(quadX, quadY, segmentWidth, segmentHeight));
                image.Position = new Vector2(x, y);
                list.Add(image);
            }
        }
        return list;
    }

    public static List<Rectangle> Split(this Rectangle rect, int chunkWidth, int chunkHeight, int skipX = 0, int skipY = 0)
    {
        List<Rectangle> list = [];
        int startX = 0;
        int startY = 0;
        while (true)
        {
            int w = Math.Clamp(chunkWidth, 0, Math.Max(0, rect.Width - startX));
            int h = Math.Clamp(chunkHeight, 0, Math.Max(0, rect.Height - startY));
            if (w == 0)
            {
                if (h == 0)
                {
                    break;
                }
                else
                {
                    startX = 0;
                    startY += h + skipY;
                }
            }
            list.Add(new Rectangle(startX, startY, w, h));
            startX += w + skipX;
        }
        return list;
    }

    public static void LogValue<T>(this Monocle.Commands commands, T value)
    {
        commands.Log($"{{{nameof(value)}:{value.ToString()}}}");
    }
    public static Vector2 Size(this VirtualRenderTarget target) => new Vector2(target.Width, target.Height);
    public static Vector2 HalfSize(this VirtualRenderTarget target) => new Vector2(target.Width, target.Height) / 2f;
    public static FlagData GetFlagData(this BinaryPacker.Element element, string flagname = "flag", string invertedname = "inverted")
        => (new FlagData(element.Attr(flagname), element.AttrBool(invertedname)));
    public static FlagData Flag(this EntityData data, string flagname, bool inverted)
    => new FlagData(data.Attr(flagname), inverted);
    public static FlagData Flag(this EntityData data, string flagname = "flag", string invertedname = "inverted")
        => new FlagData(data.Attr(flagname), data.Bool(invertedname));
    public static FlagList FlagList(this EntityData data, string flagname = "flag", string invertedname = "inverted") => new FlagList(data.Attr(flagname), data.Bool(invertedname));
    public static Vector2 Position(this BinaryPacker.Element element) => new Vector2(element.AttrFloat("x"), element.AttrFloat("y"));
    public static IEnumerator TextboxSayClean(string text, params Func<IEnumerator>[] events)
    {
        Textbox textbox = new Textbox("", events);
        textbox.text = FancyText.Parse(text, (int)textbox.maxLineWidth, textbox.linesPerPage, 0f, null, Dialog.Language);
        Engine.Scene.Add(textbox);
        while (textbox.Opened)
        {
            yield return null;
        }
    }
    public static string GetAreaKey(this Entity entity) => GetAreaKey(entity.Scene);
    public static string GetAreaKey(this Scene scene) => (scene as Level).Session.Area.GetFullID();
    public static string GetFullID(this AreaKey key) => key.SID + key.Mode.ToString();
    public static TileGrid GetTileGridOverlay(Scene scene, float x, float y, float width, float height, char tile) => GetTileOverlay(scene, x, y, width, height, tile).TileGrid;
    public static Generated GetTileOverlay(Scene scene, float x, float y, float width, float height, char tile)
    {
        Level level = scene as Level; ;
        Rectangle tileBounds = level.Session.MapData.TileBounds;
        VirtualMap<char> solidsData = level.SolidsData;
        x = (int)(x / 8f) - tileBounds.Left;
        y = (int)(y / 8f) - tileBounds.Top;
        int tilesX = (int)width / 8;
        int tilesY = (int)height / 8;
        return GFX.FGAutotiler.GenerateOverlay(tile, (int)x, (int)y, tilesX, tilesY, solidsData);
    }
    public static TileGrid GetTileGridBox(float width, float height, char tile) => GetTileBox(width, height, tile).TileGrid;
    public static Generated GetTileBox(float width, float height, char tile)
    {
        int tilesX = (int)width / 8;
        int tilesY = (int)height / 8;
        return GFX.FGAutotiler.GenerateBox(tile, tilesX, tilesY);
    }
    public static string ReplaceAt(this string input, int index, char newChar)
    {
        if (input == null)
        {
            throw new ArgumentNullException("Input is null!");
        }
        char[] chars = input.ToCharArray();
        chars[index] = newChar;
        return new string(chars);
    }
    public static bool CheckAll(this List<(string, bool)> flags, bool inverted = false)
    {
        return !flags.Exists(item => !item.Item1.GetFlag(!item.Item2) != inverted);
    }
    /// <summary>Returns a texture with expanded edges as a <see cref="VirtualTexture" />.</summary>
    /// <param name="input">The <see cref="MTexture" /> to expand.</param>
    /// <param name="xPad">The units to extend the left and right edges by.</param>
    /// <param name="yPad">The units to extend the top and bottom edges by.</param>
    /// <param name="color">The color of the newly added units.</param>
    /// <returns> A <see cref="VirtualTexture" /> including the <paramref name="input"/> with expanded edges.</returns>
    public static VirtualTexture PadTexture(this MTexture input, int xPad, int yPad, Color color)
    {
        int w = input.Width;
        int h = input.Height;
        int nw = w + xPad * 2;
        int nh = h + yPad * 2;
        Color[] data = new Color[w * h];
        input.Texture.Texture.GetData(data, 0, w * h);
        List<Color> newData = [];
        void add(int count)
        {
            for (int i = 0; i < count; i++)
            {
                newData.Add(color);
            }
        }
        add(nw * yPad);
        for (int y = 0; y < h; y++)
        {
            add(xPad);
            for (int x = 0; x < w; x++)
            {
                newData.Add(data[x + (y * w)]); //
            }
            add(xPad);
        }
        add(nw * yPad);
        var padded = VirtualContent.CreateTexture("paddedtex", nw, nh, color);
        padded.Texture.SetData([.. newData], 0, newData.Count);
        return padded;

    }

    /// <summary>Returns an array of randomly shuffled enum values.</summary>
    /// <typeparam name="T">The type of the enum to use.</typeparam>
    /// <returns> A <see cref="List{T}"/> of randomly shuffled <typeparamref name="T"/> values.</returns>
    /// <exception cref="ArgumentException"><typeparam name="T"/> must be of Enum type.</exception>
    public static List<T> RandomCombo<T>() where T : struct
    {
        Type type = typeof(T);
        if (!type.IsEnum)
        {
            throw new ArgumentException(typeof(T).Name + " must be of Enum type", type.ToString());
        }
        List<T> array = [];
        foreach (T value in type.GetEnumValues())
        {
            array.Add(value);
        }
        array.Shuffle();
        List<T> output = default;
        int rand = Calc.Random.Range(0, array.Count);
        for (int i = 0; i < rand; i++)
        {
            output.Add(array[i]);
        }
        return output;

    }

    /// <summary>Shortcut function to check if <paramref name="entity"/> is on screen.</summary>
    /// <param name="entity">The <see cref="Entity" /> to check.</param>
    /// <param name="pad">The number of units to extend the left, right, top and bottom edges of the camera bounds by.</param>
    /// <returns> <see langword="true"/> if <paramref name="entity"/>'s Collider exists and is within the currently active level's camera bounds (expanded by <paramref name="pad"/> units). <para/><see langword="false"/> if <paramref name="entity"/>'s Collider does not exists or is outside the currently active level's camera bounds (expanded by <paramref name="pad"/> units).</returns>
    public static bool OnScreen(this Entity entity, float pad = 0)
    {
        return entity.Collider != null && entity.Collider.OnScreen(Engine.Scene as Level ?? entity.SceneAs<Level>(), pad);
    }

    public static bool OnScreen(this Vector2 v, float pad = 0)
    {
        Collider c = new Hitbox(1, 1, v.X, v.Y);
        return c.OnScreen(Engine.Scene as Level, pad);
    }
    /// <summary>Gets all entities currently on screen.</summary>
    /// <param name="level">The <see cref="Level" /> to search.</param>
    /// <param name="pad">The number of units to extend the left, right, top and bottom edges of the camera bounds by.</param>
    /// <returns> A List of entities that are in the bounds of <paramref name="level"/>'s camera extended by <paramref name="pad"/> units.</returns>
    public static List<Entity> EntitiesOnScreen(this Level level, float pad = 0)
    {
        return [.. level.Entities.Where(item => item.OnScreen(pad))];
    }
    /// <summary>Exports the contents of <paramref name="from"/> as a png file.</summary>
    /// <param name="from">The <see cref="RenderTarget2D"/> to convert.</param>
    /// <param name="path">The location the resulting png will be saved to, appended to Celeste's root folder.</param>
    /// <param name="x">The left bound of the area to export.</param>
    /// <param name="y">The top bound of the area to export.</param>
    /// <param name="w">The width of the area to export.</param>
    /// <param name="h">The height of the area to export.</param>
    /// <param name="scale">The scale factor of the resulting png.</param>
    public static void SaveTargetAsPng(RenderTarget2D from, string path, int x, int y, int w, int h, int scale = 1)
    {
        if (!path.EndsWith(".png"))
        {
            path += ".png";
        }
        Rectangle value = new Rectangle(x, y, w, h);
        Color[] data = new Color[w * h];
        from.GetData(0, value, data, 0, w * h);
        using Texture2D texture2D = new Texture2D(Engine.Graphics.GraphicsDevice, w, h);
        texture2D.SetData(data);
        using RenderTarget2D renderTarget2D = new RenderTarget2D(Engine.Graphics.GraphicsDevice, w * scale, h * scale);
        Engine.Instance.GraphicsDevice.SetRenderTarget(renderTarget2D);
        Engine.Instance.GraphicsDevice.Clear(Color.Transparent);
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);
        Draw.SpriteBatch.Draw(texture2D, new Rectangle(0, 0, renderTarget2D.Width, renderTarget2D.Height), Color.White);
        Draw.SpriteBatch.End();
        Engine.Instance.GraphicsDevice.SetRenderTarget(null);
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        using Stream stream = File.OpenWrite(path);
        renderTarget2D.SaveAsPng(stream, renderTarget2D.Width, renderTarget2D.Height);
    }
    private class spriteSplitEntity : Entity
    {
        private string path, anim;
        private int w, h;
        private MTexture[] textures;
        private List<string> prefixesUsed = [];
        private List<char> prefixesAllowed = ['a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z'];
        private string[] names;
        private int frames;
        private VirtualRenderTarget[] targets;
        private bool ready;
        public spriteSplitEntity(string path, string anim, int width, int height) : base()
        {
            names = "Sa,Sb,UDUa,UDUb,UDUc,UDUd,Sc,Sd,UDMb,UDMc,UDMd,IUDL,IUDR,UDDa,UDDb,UDDc,UDDd,ULa,ULb,URa,URb,ULc,ULd,Ua,Ub,Uc,Ud,URc,URd,LRLa,LRMa,LRRa,La,Ma,Mb,Mc,Md,Ra,LRLb,LRMb,LRRb,Lb,Me,Mf,Mg,Mh,Rb,LRLc,LRMc,LRRc,Lc,Mi,Mj,Mba,Mbb,Rc,LRLd,LRMd,LRRd,Ld,Mbc,Mbd,Mb4,Mb5,Rd,DLa,DLb,Da,Db,Dc,Dd,DRa,DRb,DLc,DLd,IDL,IDR,DRc,DRd".Split(',');
            this.path = path;
            this.anim = anim;
            w = width;
            h = height;
            textures = GFX.Game.GetAtlasSubtextures(path + anim).ToArray();
            frames = textures.Length;
            targets = new VirtualRenderTarget[frames];
            for (int i = 0; i < targets.Length; i++)
            {
                targets[i] = VirtualContent.CreateRenderTarget("bleh", textures[0].Width, textures[0].Height);
            }
            Add(new BeforeRenderHook(BeforeRender));
        }
        public override void Update()
        {
            base.Update();
            if (ready)
            {
                int width = textures[0].Width;
                int height = textures[0].Height;
                int nameindex = 0;
                string output = "";
                for (int y = 0; y < height; y += h)
                {
                    for (int x = 0; x < width; x += w)
                    {
                        bool success = true;
                        for (int i = 0; i < frames; i++)
                        {
                            string name = names[nameindex];
                            name += "0" + i;

                            if (!SaveTargetAsPng(targets[i], "Split/" + name, x, y, w, h))
                            {
                                success = false;
                                break;
                            }
                        }
                        if (success)
                        {
                            output += string.Format("<sprite name=\"{0}\" path=\"animatedTiles/PianoBoy/digitalTest/{0}\" delay=\"0.1\" posX=\"0\" posY=\"0\"/>", names[nameindex]) + '\n';
                            nameindex++;
                        }
                    }
                }
                using (StreamWriter stream = File.CreateText("Split/text.txt"))
                {
                    stream.WriteLine(output);
                }
                RemoveSelf();
            }
        }
        public bool SaveTargetAsPng(RenderTarget2D from, string path, int x, int y, int w, int h, int scale = 1)
        {
            if (!path.EndsWith(".png"))
            {
                path += ".png";
            }
            Rectangle value = new Rectangle(x, y, w, h);
            Color[] data = new Color[w * h];
            from.GetData(0, value, data, 0, w * h);
            if (data[0] == Color.Transparent) return false;
            using Texture2D texture2D = new Texture2D(Engine.Graphics.GraphicsDevice, w, h);
            texture2D.SetData(data);
            using RenderTarget2D renderTarget2D = new RenderTarget2D(Engine.Graphics.GraphicsDevice, w * scale, h * scale);
            Engine.Instance.GraphicsDevice.SetRenderTarget(renderTarget2D);
            Engine.Instance.GraphicsDevice.Clear(Color.Transparent);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);
            Draw.SpriteBatch.Draw(texture2D, new Rectangle(0, 0, renderTarget2D.Width, renderTarget2D.Height), Color.White);
            Draw.SpriteBatch.End();
            Engine.Instance.GraphicsDevice.SetRenderTarget(null);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            using Stream stream = File.OpenWrite(path);
            renderTarget2D.SaveAsPng(stream, renderTarget2D.Width, renderTarget2D.Height);
            return true;
        }
        public void BeforeRender()
        {
            if (ready) return;
            for (int i = 0; i < targets.Length; i++)
            {
                targets[i].SetAsTarget();
                Draw.SpriteBatch.Begin();
                textures[i].Draw(Vector2.Zero);
                Draw.SpriteBatch.End();
            }
            ready = true;

        }
        public override void Render()
        {
            base.Render();
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            foreach (var v in targets)
            {
                v?.Dispose();
            }
        }
    }
    [Command("temp_split", "")]
    public static void SplitAnimationIntoFrames(string path, string anim, int width, int height)
    {
        if (Engine.Scene is Level level)
        {
            level.Add(new spriteSplitEntity(path, anim, width, height));
        }
    }
    /// <summary>Splits a string into segments of size <paramref name="count"/>.</summary>
    /// <param name="str">The <see cref="string"/> to split.</param>
    /// <param name="count">The maximum size of each segment.</param>
    /// <param name="includeLeftover">Whether to include the last segment if it's length is less than the maximum segment size.</param>
    /// <remarks>Aka System.Linq.Chunk but better</remarks>
    public static List<string> Segment(this string str, int count, bool includeLeftover)
    {
        List<string> list = [];
        string add = "";
        int c = 0;
        for (int i = 0; i < str.Length; i++)
        {
            add += str[i];
            c++;
            if (c >= count)
            {
                list.Add(add);
                add = "";
                c = 0;
            }
        }
        if (includeLeftover && c > 0)
        {
            list.Add(add);
        }
        return list;
    }
    public static bool TryGetAreaKey(out AreaKey key)
    {
        if (Engine.Scene is Level level)
        {
            key = level.Session.Area;
            return true;
        }
        key = default;
        return false;
    }
    /// <summary>Adds a <see cref="Rune"/> to the provided List if it doesn't already contain a matching <see cref="Rune"/>.</summary>
    /// <param name="list">The <see cref="List{}"/> to add the rune to.</param>
    /// <param name="rune">The <see cref="Rune"/> to check.</param>
    /// <returns> <see langword="true"/> if <paramref name="rune"/> was successfully added to <paramref name="list"/>.<para/> <see langword="false"/> if <paramref name="rune"/> matches another <see cref="Rune"/> in <paramref name="list"/>.</returns>
    public static bool TryAddRune(this HashSet<WarpRune> list, WarpRune rune)
    {
        foreach (WarpRune r in list)
        {
            if (r.Match(rune))
            {
                return false;
            }
        }
        list.Add(rune);
        return true;
    }
    public static bool TryAddRuneRange(this HashSet<WarpRune> list, IEnumerable<WarpRune> runes)
    {
        string current = "CURRENT RUNES IN LIST:\n";
        string adding = "RUNES TO BE CHECKED:\n";
        string added = "RUNES ADDED:\n";
        foreach (WarpRune r in list)
        {
            current += "\t" + r.ToString() + "\n";
        }
        foreach (WarpRune r in runes)
        {
            adding += "\t" + r.ToString() + "\n";
        }
        bool failed = false;
        foreach (WarpRune r in runes)
        {
            if (!list.TryAddRune(r))
            {
                failed = true;
            }
            else
            {
                added += "\t" + r.ToString() + "\n";
            }
        }
        /*        Engine.Commands.Log(current);
                Engine.Commands.Log(adding);
                Engine.Commands.Log(added);*/
        return !failed;
    }
    public static int Sign(this Random random, bool allowZero = false)
    {
        return allowZero ? random.Choose(-1, 0, 1) : random.Choose(-1, 1);
    }
    public static Vector2 Random(this Rectangle rectangle)
    {
        return new Vector2(Calc.Random.Range(rectangle.Left, rectangle.Right), Calc.Random.Range(rectangle.Top, rectangle.Bottom));
    }
    public static Vector2 Random(this Collider collider)
    {
        return collider.Bounds.Random();
    }
    public static Leader Reset(this Leader leader, Vector2 newPosition)
    {
        leader.PastPoints.Clear();
        for (int i = 0; i < 6; i++)
        {
            leader.PastPoints.Add(newPosition);
        }
        return leader;
    }
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
    public static VertexPositionColor[] CreateVertices(this Vector2[] points, Vector2 scale, out int[] basicIndices, params Color[] colors)
    {
        Vector3[] newPoints = new Vector3[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            newPoints[i] = new Vector3(points[i], 0);
        }
        return CreateVertices(newPoints, new Vector3(scale, 0), out basicIndices, colors);
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
    public static bool Contains(this Rectangle rect, Vector2 point)
    {
        return rect.Contains((int)point.X, (int)point.Y);
    }
    public static Rectangle Pad(this Rectangle rect, int pad)
    {
        return rect.Pad(pad, pad);
    }
    public static Rectangle Pad(this Rectangle rect, Vector2 pad)
    {
        return rect.Pad((int)pad.X, (int)pad.Y);
    }
    public static Rectangle Pad(this Rectangle rect, int xPad, int yPad)
    {
        return new Rectangle(rect.X - xPad, rect.Y - yPad, rect.Width + xPad * 2, rect.Height + yPad * 2);
    }
    public static bool Contains(this Rectangle rect, Vector2 point, int pad)
    {
        Rectangle rect2 = rect.Pad(pad);
        return rect2.Left <= point.X && rect2.Right >= point.X && rect2.Top <= point.Y && rect2.Bottom >= point.Y;
    }
    public static bool GetFlag(this string str, bool inverted = false)
        => string.IsNullOrEmpty(str) ? !inverted : Engine.Scene is Level level && level.Session.GetFlag(str) != inverted;
    public static bool GetFlag(this string str, Level level, bool inverted = false)
    {
        return string.IsNullOrEmpty(str) ? !inverted : level.Session.GetFlag(str) != inverted;
    }
    public static bool TryGetFlag(this string str, out bool result, bool inverted = false)
    {
        if (Engine.Scene is Level level && !string.IsNullOrEmpty(str))
        {
            result = level.Session.GetFlag(str) != inverted;
            return true;
        }
        else
        {
            result = false;
            return false;
        }
    }
    public static void SetFlag(this string str, bool value = true)
    {
        if (Engine.Scene is Level level && !string.IsNullOrEmpty(str))
        {
            level.Session.SetFlag(str, value);
        }
    }
    public static void InvertFlag(this string str, bool inverted = false)
    {
        if (Engine.Scene is Level level && !string.IsNullOrEmpty(str))
        {
            //level.Session.SetFlag(str, !str.GetFlag(inverted));
            level.Session.SetFlag(str, !level.Session.GetFlag(str));
        }
    }
    public static int GetCounter(this string str)
    {
        if (Engine.Scene is Level level && !string.IsNullOrEmpty(str))
        {
            return level.Session.GetCounter(str);
        }
        return 0;
    }
    public static void SetCounter(this string str, int value)
    {
        if (Engine.Scene is Level level && !string.IsNullOrEmpty(str))
        {
            level.Session.SetCounter(str, value);
        }
    }
    public static int IncrementCounter(this string str, int? mod = null)
    {
        if (Engine.Scene is Level level && !string.IsNullOrEmpty(str))
        {
            int num = level.Session.GetCounter(str) + 1;
            if (mod.HasValue) num %= mod.Value;
            level.Session.SetCounter(str, num);
            return num;
        }
        return 0;
    }
    public static int DecrementCounter(this string str, int? mod = null)
    {
        if (Engine.Scene is Level level && !string.IsNullOrEmpty(str))
        {
            int num = level.Session.GetCounter(str) - 1;
            if (mod.HasValue) num %= mod.Value;
            level.Session.SetCounter(str, num);
            return num;
        }
        return 0;
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
        public Vector2[] FlattenedPoints;
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
    public static Color RandomColorAlpha(bool red = true, bool green = true, bool blue = true, bool alpha = false, int min = 0, int max = 256)
    {
        int r = Calc.Random.Range(min, max);
        int g = Calc.Random.Range(min, max);
        int b = Calc.Random.Range(min, max);
        int a = Calc.Random.Range(min, max);
        if (!red) r *= 0;
        if (!green) g *= 0;
        if (!blue) b *= 0;
        if (!alpha) a *= 0;
        return new Color(r, g, b, a);
    }
    public static Color RandomColor(bool red = true, bool green = true, bool blue = true, int min = 0, int max = 256)
    {
        int r = Calc.Random.Range(min, max);
        int g = Calc.Random.Range(min, max);
        int b = Calc.Random.Range(min, max);
        if (!red) r *= 0;
        if (!green) g *= 0;
        if (!blue) b *= 0;
        return new Color(r, g, b);
    }
    public static Color RandomColorMix(this Color a, Color b)
    {
        return Color.Lerp(a, b, Calc.Random.Range(0f, 1));
    }
    public static Color Shade(this Color a, float value, float range = 1)
    {
        if (value < 0)
        {
            return Color.Lerp(a, Color.Black, Math.Abs(value) / range);
        }
        return Color.Lerp(a, Color.White, Math.Abs(value) / range);
    }
    public static Color RandomShade(this Color a, float range = 1)
    {
        if (range == 0) return a;
        float abs = Math.Abs(range);
        float value = Calc.Random.Range(-abs, abs);
        if (value < 0)
        {
            return Color.Lerp(a, Color.Black, Math.Abs(value));
        }
        else
        {
            return Color.Lerp(a, Color.White, value);
        }
    }
    public static Mesh<T> CreateTriWallMesh<T>(Vector3 position, float areaWidth, float areaHeight, float triWidth, float triHeight, int extend, Func<Vector2, Color> getColor, Func<Vector3, Color, T> createVertex, out TriWall wall) where T : struct, IVertexType
    {
        Mesh<T> mesh = new Mesh<T>();
        int exL = extend + 1;
        int exR = extend + 2;
        int exT = extend;
        int exB = extend + 1;

        wall = CreateTriWall(areaWidth, areaHeight, triWidth, triHeight, exL, exR, exT, exB, null, out int rows, out int cols);
        int count = rows * cols;
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                Vector2 uv = new Vector2(wall[c, r].X / areaWidth, wall[c, r].Y / areaHeight);
                T vertex = createVertex(position + new Vector3(wall[c, r], 0), getColor(uv));
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
    public static List<int> CountArray(int count, bool ascending)
    {
        List<int> output = [];
        for (int i = 0; i < count; i++)
        {
            output.Add(ascending ? i : count - i);
        }
        return output;
    }
    public static LineMesh<T> CreateTriLineWallMesh<T>(Vector3 position, float areaWidth, float areaHeight, float triWidth, float triHeight, int extend, Func<Vector2, Color> getColor, Func<int, int, Vector2> getOffset, Func<Vector3, Color, T> createVertex, out TriWall triWall) where T : struct, IVertexType
    {
        getOffset ??= new Func<int, int, Vector2>(delegate { return Vector2.Zero; });
        LineMesh<T> mesh = new LineMesh<T>();
        int exL = extend + 1;
        int exR = extend + 2;
        int exT = extend;
        int exB = extend + 1;

        triWall = CreateTriWall(areaWidth, areaHeight, triWidth, triHeight, exL, exR, exT, exB, getOffset, out int rows, out int cols);

        int count = rows * cols;
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                Vector2 uv = new Vector2(triWall[c, r].X / areaWidth, triWall[c, r].Y / areaHeight);
                T vertex = createVertex(position + new Vector3(triWall[c, r], 0), getColor(uv));
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
    public static Mesh<T> CreateTriWallMesh<T>(Vector3 position, float areaWidth, float areaHeight, float triWidth, float triHeight, int extend, Color color, Func<Vector3, Color, T> createVertex, out TriWall wall) where T : struct, IVertexType
    {
        Func<Vector2, Color> func = delegate { return color; };
        return CreateTriWallMesh<T>(position, areaWidth, areaHeight, triWidth, triHeight, extend, func, createVertex, out wall);
    }
    public static TriWall CreateTriWall(float areaWidth, float areaHeight, float triHeight, float triWidth, int leftExtend, int rightExtend, int topExtend, int bottomExtend, Func<int, int, Vector2> getOffset, out int rows, out int cols)
    {

        getOffset ??= new Func<int, int, Vector2>(delegate { return Vector2.Zero; });
        float top = topExtend * -triHeight;
        float left = leftExtend * -triWidth;
        float right = areaWidth + rightExtend * triWidth;
        float bottom = areaHeight + bottomExtend * triHeight;
        rows = (int)((bottom - top) / triHeight);
        cols = (int)((right - left) / triWidth);
        float xOffset = triWidth / 2f;
        Vector2[,] grid = new Vector2[cols, rows];
        Vector2[] flattened = new Vector2[cols * rows];
        int count = 0;
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                Vector2 v = new Vector2(left + c * triWidth + (r % 2 != 0 ? xOffset : 0), top + r * triHeight) + getOffset(c, r);
                grid[c, r] = v;
                flattened[count] = v;
                count++;
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
            TriangleHeight = triHeight,
            FlattenedPoints = flattened
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

    public static Vector2 NaiveMax(this IEnumerable<Vector2> list)
    {
        float right = int.MinValue, down = int.MinValue;
        foreach (Vector2 v in list)
        {
            right = Math.Max(right, v.X);
            down = Math.Max(down, v.Y);
        }
        return new Vector2(right, down);
    }
    public static Vector2 NaiveMin(this IEnumerable<Vector2> list)
    {
        float left = int.MaxValue, up = int.MaxValue;
        foreach (Vector2 v in list)
        {
            left = Math.Min(left, v.X);
            up = Math.Min(up, v.Y);
        }
        return new Vector2(left, up);
    }
    public static List<(string, bool)> ParseFlagsFromString(string flagList)
    {
        List<(string, bool)> list = [];
        if (!string.IsNullOrEmpty(flagList))
        {
            string[] array = flagList.Replace(" ", "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var item in array)
            {
                if (item[0] == '!' && item.Length > 1)
                {
                    list.Add((item.Substring(1), false));
                }
                else
                {
                    list.Add((item, true));
                }
            }
        }
        return list;

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
    public static List<T> GetFollowerEntities<T>(this Leader leader) where T : Entity
    {
        List<T> list = new();
        if (leader.HasFollower<T>())
        {
            foreach (Follower f in leader.Followers)
            {
                if (f.Entity is T)
                {
                    list.Add(f.Entity as T);
                }
            }
        }
        return list;
    }
    public static List<Follower> GetFollowers<T>(this Leader leader) where T : Entity
    {
        List<Follower> list = new();
        if (leader.HasFollower<T>())
        {
            foreach (Follower f in leader.Followers)
            {
                if (f.Entity is T)
                {
                    list.Add(f);
                }
            }
        }
        return list;
    }
    public static void Face(this Player player, Entity entity)
    {
        if (entity != null)
        {
            if (entity.CenterX > player.CenterX)
            {
                player.Facing = Facings.Right;
            }
            else
            {
                player.Facing = Facings.Left;
            }
        }
    }
    public static IEnumerator Boop(this Player player, int dir = 1, float dist = 7)
    {
        float from = player.X;
        player.Facing = (Facings)dir;
        yield return LerpYoyo(Ease.CubeIn, 0.1f, f => player.X = Calc.LerpClamp(from, from + dist * dir, f));
    }
    public static IEnumerator Boop(this Player player, Entity booped, float additionalBoopage = 0)
    {
        yield return new SwapImmediately(player.Boop(Math.Sign(booped.X - player.X), MathHelper.Distance(booped.X, player.X) + additionalBoopage));
    }
    public static IEnumerator ZoomToWorld(this Level level, Vector2 worldPos, float zoom, float duration)
    {
        yield return new SwapImmediately(level.ZoomTo(worldPos - level.Camera.Position, zoom, duration));
    }
    public static IEnumerator ZoomAcrossWorld(this Level level, Vector2 worldPos, float zoom, float duration)
    {
        yield return new SwapImmediately(level.ZoomAcross(worldPos - level.Camera.Position, zoom, duration));
    }
    public static void Ground<T>(IEnumerable<T> list, bool includeJumpThrus = false) where T : FallingBlock
    {
        foreach (FallingBlock block in list.OrderByDescending(item => item.Bottom))
        {
            block.Ground();
        }
    }

    /// <summary>Wraps an integer to the bounds of min (inclusive) and max (inclusive)</summary>
    public static int Wrap(this int i, int min, int max, int move)
    {
        int dir = Math.Sign(move);
        for (int j = 0; j < Math.Abs(move); j++)
        {
            i += dir;
            if (i < min) i = max;
            if (i > max) i = min;
        }
        return i;
    }
    public static int Wrap<T>(this int i, T[] array, int move) => array == null ? i : i.Wrap(0, Math.Max(array.Length - 1, 0), move);
    public static int Wrap<T>(this int i, ICollection<T> array, int move) => array == null ? i : i.Wrap(0, Math.Max(array.Count - 1, 0), move);
    public static int Wrap<T>(this int i, ISet<T> array, int move) => array == null ? i : i.Wrap(0, Math.Max(array.Count - 1, 0), move);
    [Command("groundplayer", "")]
    public static void GroundPlayer(bool jumpthru = false, bool snapup = true)
    {
        if (Engine.Scene.GetPlayer() is Player player)
        {
            player.Ground(jumpthru, snapup);
        }
    }
    public static void Ground(this Entity entity, bool includeJumpThrus = false, bool snapToTop = true)
    {
        if (entity is FallingBlock block)
        {
            Ground(block, includeJumpThrus);
            return;
        }
        if (entity.Scene is not Level level) return;
        Vector2 prev = entity.Position;
        if (snapToTop)
        {
            if (includeJumpThrus && entity.CollideFirst<JumpThru>() is JumpThru jt)
            {
                entity.Bottom = jt.Top;
                return;
            }
            while (entity.CollideCheck<Solid>())
            {
                entity.Y--;
                if (entity.Y < level.Bounds.Top)
                {
                    entity.Y = prev.Y;
                    break;
                }
            }
        }

        while (entity.Y < level.Bounds.Bottom)
        {
            Vector2 p = entity.Position + Vector2.UnitY;
            if (includeJumpThrus && entity.CollideCheck<JumpThru>(p)) break;
            if (entity.CollideCheck<Solid>(p)) break;
            entity.Y++;
        }
        if (entity is Player player)
        {
            level.Camera.Position = player.CameraTarget;
        }

    }
    public static void Ground(this FallingBlock block, bool includeJumpThrus = false)
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
            if (block.MoveVCollideSolids(speed * Engine.DeltaTime, thruDashBlocks: !includeJumpThrus))
            {
                IEnumerator makeLame()
                {
                    while (true)
                    {
                        block.FallDelay = 10;
                        yield return null;
                    }
                }
                ;
                block.Add(new Coroutine(makeLame()));
                block.Add(new KeepGrounded());
                break;
            }
            Vector2 p = block.Position + Vector2.UnitY;
            if (block.Top > (level.Bounds.Bottom + 16) || (block.Top > (level.Bounds.Bottom - 1)
                && ((includeJumpThrus && block.CollideCheck<JumpThru>(p)) || block.CollideCheck<Solid>(p))))
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
    public static Vector2 Snapped<T>(this Entity entity, Vector2 dir, float limit = -1, float step = 1, Collider collider = null) where T : Entity
    {
        dir = Calc.Sign(dir);
        if (step <= 0 || (dir.X == 0 && dir.Y == 0) || entity.Scene is not Level level) return entity.Position;
        if (limit < 0)
        {
            if (dir.X < 0) limit = level.Bounds.Left;
            else if (dir.X > 0) limit = level.Bounds.Right;
            else if (dir.Y < 0) limit = level.Bounds.Top;
            else if (dir.Y > 0) limit = level.Bounds.Bottom;
        }
        collider ??= entity.Collider ?? new Hitbox(1, 1);
        Collider prevCollider = entity.Collider;
        entity.Collider = collider;
        Vector2 prev = entity.Position;
        Vector2 pos = entity.Position;
        float maxDist = MathHelper.Distance(dir.X != 0 ? pos.X : pos.Y, limit);
        float dist = 0;
        while (!entity.CollideCheck<T>() && dist < maxDist)
        {
            entity.Position += dir * step;
            dist += step;
        }
        Vector2 result = prev + (dir * Math.Min(limit, dist));
        entity.Position = prev;
        entity.Collider = prevCollider;
        return result;
    }
    public static Entity PushOutOfSolids(this Entity entity, Vector2 step)
    {
        while (entity.CollideCheck<Solid>())
        {
            entity.Position += step;
        }
        return entity;
    }
    public static Vector2 NearestSnap<T>(this Entity entity, float step = 1, Collider collider = null) where T : Entity
    {
        Vector2[] snapped = new Vector2[4]
        {
            entity.Snapped<T>(-Vector2.UnitX, -1, step, collider),
             entity.Snapped<T>(Vector2.UnitX, -1, step, collider),
            entity.Snapped<T>(-Vector2.UnitY, -1, step, collider),
             entity.Snapped<T>(Vector2.UnitY, -1, step, collider)
        };
        Vector2 max = Vector2.Zero;
        float maxDist = 0;
        for (int i = 0; i < snapped.Length; i++)
        {
            float dist = Vector2.DistanceSquared(entity.Position, snapped[i]);
            if (dist > maxDist)
            {
                max = snapped[i];
            }
        }

        return max;

    }
    public static bool HasGroundBelow(this Entity entity, out Vector2 groundPosition)
    {
        Level level = entity.Scene as Level;
        Vector2 pos = groundPosition = entity.Position;
        Collider c = entity.Collider;
        entity.Collider ??= new Hitbox(1, 1);

        while (!entity.CollideCheck<Solid>(pos))
        {
            if (pos.Y > level.Bounds.Bottom)
            {
                entity.Collider = c;
                return false;
            }
            pos.Y++;
        }
        entity.Collider = c;
        groundPosition = pos;
        return true;
    }
    public static bool OnGround(this Entity entity, int downCheck = 1, bool ignoreJumpThrus = false)
    {
        if (!entity.CollideCheck<Solid>(entity.Position + Vector2.UnitY * downCheck))
        {
            if (!ignoreJumpThrus)
            {
                return entity.CollideCheckOutside<JumpThru>(entity.Position + Vector2.UnitY * downCheck);
            }

            return false;
        }
        return true;
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
    public static Hitbox Collider(this EntityData data) => new Hitbox(data.Width, data.Height);
    public static Hitbox ColliderCentered(this EntityData data) => new Hitbox(data.Width, data.Height, data.Width / 2, data.Height / 2);
    public static Hitbox ColliderCentered(this MTexture texture) => new Hitbox(texture.Width, texture.Height, -texture.Width / 2, -texture.Height / 2);
    public static Hitbox ColliderCentered(this Image texture) => texture.Texture.ColliderCentered();
    public static Hitbox Collider(this MTexture texture) => new Hitbox(texture.Width, texture.Height);
    public static Hitbox Collider(this Image texture) => texture.Texture.Collider();
    public static Hitbox Collider(this Sprite sprite) => new Hitbox(sprite.Width, sprite.Height, sprite.X, sprite.Y);
    public static IEnumerator Lerp(this Ease.Easer ease, float time, Action<float> action, bool actionOnEnd = false)
    {
        ease ??= Ease.Linear;
        for (float i = 0; i < 1; i += Engine.DeltaTime / time)
        {
            action?.Invoke(ease(i));
            yield return null;
        }
        if (actionOnEnd)
        {
            action?.Invoke(1);
        }
    }
    public static IEnumerator LerpYoyo(this Ease.Easer ease, float halfTime, Action<float> action, Action onHalf = null)
    {
        ease ??= Ease.Linear;
        yield return ease.Lerp(halfTime, action);
        onHalf?.Invoke();
        yield return Ease.Invert(ease).ReverseLerp(halfTime, action);
    }
    public static IEnumerator ReverseLerp(this Ease.Easer ease, float time, Action<float> action, bool actionOnEnd = false)
    {
        ease ??= Ease.Linear;
        for (float i = 0; i < 1; i += Engine.DeltaTime / time)
        {
            action?.Invoke(ease(1 - i));
            yield return null;
        }
        if (actionOnEnd)
        {
            action?.Invoke(0);
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

    public static float RenderLeft(this Image image) => image.RenderPosition.X;
    public static float RenderRight(this Image image) => image.RenderPosition.X + image.Width;
    public static float RenderTop(this Image image) => image.RenderPosition.Y;
    public static float RenderBottom(this Image image) => image.RenderPosition.Y + image.Height;
    public static float RenderCenterX(this Image image) => image.RenderPosition.X + image.HalfSize().X;
    public static float RenderCenterY(this Image image) => image.RenderPosition.Y + image.HalfSize().Y;
    public static Vector2 RenderTopRight(this Image image) => image.RenderPosition + Vector2.UnitX * image.Width;
    public static Vector2 RenderTopLeft(this Image image) => image.RenderPosition;
    public static Vector2 RenderBottomRight(this Image image) => image.RenderPosition + image.Size();
    public static Vector2 RenderBottomLeft(this Image image) => image.RenderPosition + Vector2.UnitY * image.Height;
    public static Vector2 RenderCenterLeft(this Image image) => image.RenderPosition + Vector2.UnitY * image.Height / 2;
    public static Vector2 RenderCenterRight(this Image image) => image.RenderPosition + new Vector2(image.Width, image.Height / 2);
    public static Vector2 RenderTopCenter(this Image image) => image.RenderPosition + Vector2.UnitX * image.Width / 2;
    public static Vector2 RenderBottomCenter(this Image image) => image.RenderPosition + new Vector2(image.Width / 2, image.Height);
    public static Vector2 RenderCenter(this Image image) => image.RenderPosition + image.HalfSize();
    public static Rectangle Bounds(this Image image) => new((int)image.X, (int)image.Y, (int)image.Width, (int)image.Height);
    public static Rectangle RenderBounds(this Image image) =>
        new((int)image.RenderPosition.X, (int)image.RenderPosition.Y, (int)image.Width, (int)image.Height);


    public static Rectangle SetPos(this Rectangle rect, Vector2 position)
    {
        rect.X = (int)position.X;
        rect.Y = (int)position.Y;
        return rect;
    }
    public static Rectangle SetPos(this Rectangle rect, float x, float y)
    {
        rect.X = (int)x;
        rect.Y = (int)y;
        return rect;
    }
    public static Rectangle SetSize(this Rectangle rect, float width, float height)
    {
        rect.Width = (int)width;
        rect.Height = (int)height;
        return rect;
    }
    public static Rectangle SetSize(this Rectangle rect, Vector2 size)
    {
        return rect.SetSize(size.X, size.Y);
    }
    public static Rectangle Set(this Rectangle rect, Vector2 position, float width, float height)
    {
        return rect.Set(position.X, position.Y, width, height);
    }
    public static Rectangle Set(this Rectangle rect, Vector2 position, Vector2 size)
    {
        return rect.Set(position.X, position.Y, size.X, size.Y);
    }
    public static Rectangle Set(this Rectangle rect, float x, float y, float width, float height)
    {
        rect.SetPos(x, y);
        rect.SetSize(width, height);
        return rect;
    }
    public static bool OnScreen(this Collider collider, Level level, float pad = 0)
    {
        return collider.Bounds.OnScreen(level, pad);
    }
    public static bool OnScreen(this Rectangle rect, Level level, float pad = 0)
    {
        return rect.Colliding(level.Camera.GetBounds(), pad);
    }
    public static bool Colliding(this Rectangle rect, Rectangle check)
    {
        return Colliding(rect, check, 0);
    }
    public static bool Colliding(this Rectangle rect, Rectangle check, float padding)
    {
        return Colliding(rect, check, Vector2.One * padding);
    }
    public static bool Colliding(this Rectangle rect, Rectangle check, Vector2 padding)
    {
        if (padding != Vector2.Zero)
        {
            check = check.Pad(padding);
        }

        return check.Right > rect.Left && check.Bottom > rect.Top && check.Left < rect.Right && check.Top < rect.Bottom;
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
    public static void DrawOutlineOnly(this MTexture texture, Vector2 position, Vector2 origin, Color color, float scale, float rotation)
    {
        float scaleFix = texture.ScaleFix;
        scale *= scaleFix;
        Rectangle clipRect = texture.ClipRect;
        Vector2 origin2 = (origin - texture.DrawOffset) / scaleFix;
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i != 0 || j != 0)
                {
                    Draw.SpriteBatch.Draw(texture.Texture.Texture_Safe, position + new Vector2(i, j), clipRect, color, rotation, origin2, scale, SpriteEffects.None, 0f);
                }
            }
        }
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
    public static void StandardBegin(this SpriteBatch spriteBatch, Matrix matrix, BlendState blend)
    {
        spriteBatch.Begin(SpriteSortMode.Deferred, blend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, matrix);
    }
    public static void StandardBegin(this SpriteBatch spriteBatch, BlendState blend, Effect shader)
    {
        spriteBatch.Begin(SpriteSortMode.Deferred, blend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, shader, Matrix.Identity);
    }
    public static void StandardBegin(this SpriteBatch spriteBatch, Matrix matrix, BlendState blend, Effect shader)
    {
        spriteBatch.Begin(SpriteSortMode.Deferred, blend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, shader, matrix);
    }
    public static void StandardBegin(this SpriteBatch spriteBatch, BlendState blend)
    {
        spriteBatch.Begin(SpriteSortMode.Deferred, blend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
    }
    public static void StandardBegin(this SpriteBatch spriteBatch, Matrix matrix, Effect effect)
    {
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, effect, matrix);
    }

    public static void SetAsTarget(this VirtualRenderTarget source, Color clear)
    {
        Engine.Graphics.GraphicsDevice.SetRenderTarget(source);
        Engine.Graphics.GraphicsDevice.Clear(clear);
    }

    public static void SetAsTarget(this VirtualRenderTarget source, bool clear)
    {
        Engine.Graphics.GraphicsDevice.SetRenderTarget(source);
        if (clear)
        {
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
        }
    }
    public static void SetAsTarget(this VirtualRenderTarget source)
    {
        Engine.Graphics.GraphicsDevice.SetRenderTarget(source);
    }
    public static VirtualRenderTarget Apply(this VirtualRenderTarget source, VirtualRenderTarget bufferSameSizeAsSource, Effect effect, Vector2 position = default)
    {
        bufferSameSizeAsSource.SetAsTarget(Color.Transparent);
        Draw.SpriteBatch.StandardBegin(effect);
        Draw.SpriteBatch.Draw((RenderTarget2D)source, position, Color.White);
        Draw.SpriteBatch.End();
        source.SetAsTarget(Color.Transparent);
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
    public static void RemoveSelves(this IEnumerable<Entity> entities)
    {
        foreach (Entity e in entities)
        {
            e.RemoveSelf();
        }
    }
    public static Collider Collider(this IEnumerable<Vector2> positions, Vector2 offset = default, int mult = 1)
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
    public static Rectangle Bounds(this IEnumerable<Vector2> positions, Vector2 offset = default, float mult = 1)
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
        return new Rectangle((int)((right - left) * mult), (int)((bottom - top) * mult), (int)((left * mult) + offset.X), (int)((top * mult) + offset.Y));
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

    public static Color Brighten(this Color color, float amount)
    {
        return new Color(color.R + amount, color.G + amount, color.B + amount);
    }
    public static Color Darken(this Color color, float amount)
    {
        return new Color(color.R - amount, color.G - amount, color.B - amount);
    }
    public static void DisableMovement(this Player player)
    {
        player.StateMachine.State = Player.StDummy;
    }
    public static void EnableMovement(this Player player)
    {
        player.StateMachine.State = Player.StNormal;
    }
    public static Vector2 RandomFrom(this Vector2 vec, float xMin, float xMax, float yMin, float yMax)
    {
        vec = Random(xMin, xMax, yMin, yMax) + vec;
        return vec;
    }
    public static Ease.Easer RandomEaser()
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

    public static T Random<T>(this List<T> array)
    {
        int min = 0;
        int max = array.Count - 1;
        if (max <= 1) return array[0];
        return array[Calc.Random.Range(min, max)];
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
    public static T Random<T>(this T[] array, int limit = -1) => array.Random(Calc.Random, limit);
    public static T Random<T>(this T[] array, Random random, int limit = -1)
    {
        if (limit < 0 || limit >= array.Length)
        {
            limit = array.Length;
        }
        return array[random.Range(0, limit)];
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
    public static void TeleportTo(Scene scene, Player player, string room, IntroTypes introType = IntroTypes.Transition, Vector2? nearestSpawn = null, Action<Level, Player> onEnd = null)
    {
        if (scene is Level level)
        {
            level.OnEndOfFrame += delegate
            {
                level.TeleportTo(player, room, introType, nearestSpawn);
                onEnd?.Invoke(level, player);
            };
        }
    }
    public static void InstantRelativeTeleport(Scene scene, string room, bool snapToSpawnPoint, Vector2 offset, Action<Level, Player> onEnd = null)
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
            onEnd?.Invoke(level, player);
        };
    }
    public static void InstantRelativeTeleport(Scene scene, string room, bool snapToSpawnPoint, Action<Level, Player> onEnd = null)
    {
        InstantRelativeTeleport(scene, room, snapToSpawnPoint, Vector2.Zero, onEnd);
    }
    public static void InstantTeleport(Scene scene, string room, Vector2 newPosition, Action<Level, Player> onEnd = null)
    {
        InstantTeleport(scene, room, newPosition.X, newPosition.Y, onEnd);
    }
    public static void InstantTeleportToMarker(Scene scene, string room, string markerName, Action<Level, Player> onEnd = null)
    {
        foreach (MarkerData d in PianoMapDataProcessor.MarkerData[scene.GetAreaKey()][room])
        {
            if (d.ID == markerName)
            {
                Vector2 pos = d.WorldPosition + new Vector2(4, 5);
                InstantTeleport(scene, room, pos, onEnd);
                return;
            }
        }
    }
    public static void InstantTeleport(Scene scene, string room, float positionX, float positionY, Action<Level, Player> onEnd = null)
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
            level.Wipe?.Cancel();
            onEnd?.Invoke(level, player);
        };
    }
    public static void InsertPoint(this Leader leader)
    {
        Vector2 vector = leader.Entity.Position + leader.Position;
        leader.InsertPoint(vector);
    }
    public static void InsertPoint(this Leader leader, Vector2 point)
    {
        if (leader.PastPoints.Count != 0)
        {
            leader.PastPoints.Insert(0, point);
            if (leader.PastPoints.Count > 350)
            {
                leader.PastPoints.RemoveAt(leader.PastPoints.Count - 1);
            }
        }
        else
        {
            leader.PastPoints.Add(point);
        }
    }
    public static List<Vector2> GetGridPoints(float width, float height, float cellWidth, float cellHeight, Func<int, int, Vector2> getCellOffset = null)
    {
        getCellOffset ??= new Func<int, int, Vector2>(delegate { return Vector2.Zero; });
        List<Vector2> points = [];
        int rows = (int)(height / cellHeight);
        int cols = (int)(width / cellWidth);
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                Vector2 offset = getCellOffset(x, y);
                points.Add(new Vector2(x * cellWidth, y * cellHeight) + offset);
            }
        }
        return points;
    }
    public static List<List<Vector2>> GetGridPointsMultiList(float width, float height, float cellWidth, float cellHeight, bool orderByRow, Func<int, int, Vector2> getCellOffset = null)
    {
        getCellOffset ??= new Func<int, int, Vector2>(delegate { return Vector2.Zero; });
        List<List<Vector2>> points = [];
        int rows = (int)(height / cellHeight);
        int cols = (int)(width / cellWidth);
        if (orderByRow)
        {
            for (int y = 0; y < rows; y++)
            {
                List<Vector2> row = [];
                for (int x = 0; x < cols; x++)
                {
                    Vector2 offset = getCellOffset(x, y);
                    row.Add(new Vector2(x * cellWidth, y * cellHeight) + offset);
                }
                points.Add(row);
            }
        }
        else
        {
            for (int x = 0; x < cols; x++)
            {
                List<Vector2> col = [];
                for (int y = 0; y < rows; y++)
                {
                    Vector2 offset = getCellOffset(x, y);
                    col.Add(new Vector2(x * cellWidth, y * cellHeight) + offset);
                }
                points.Add(col);
            }
        }
        return points;
    }
    public static bool TryAddRange<T>(this List<T> list, params T[] values)
    {
        bool clean = true;
        foreach (T value in values)
        {
            if (!list.Contains(value))
            {
                list.Add(value);
            }
            else
            {
                clean = false;
            }
        }
        return clean;
    }
    public static bool TryAdd<T>(this List<T> list, T value, bool allowNull = true)
    {
        if (!allowNull && value == null) return false;
        if (!list.Contains(value))
        {
            list.Add(value);
            return true;
        }
        return false;
    }

    public static IEnumerator MultiRoutine(this Entity entity, params IEnumerator[] functionCalls)
    {
        Coroutine[] routines = new Coroutine[functionCalls.Length];
        for (int i = 0; i < functionCalls.Length; i++)
        {
            routines[i] = new Coroutine(functionCalls[i]);
            entity.Add(routines[i]);
        }
        foreach (Coroutine routine in routines)
        {
            while (!routine.Finished) yield return null;
        }
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

    public static Vector2 RotateAroundDeg(this Vector2 pointToRotate, Vector2 centerPoint, double angleInDegrees, bool floor = false)
    {
        double angleInRadians = angleInDegrees * (Math.PI / 180);
        return RotateAroundRad(pointToRotate, centerPoint, angleInRadians, floor);
    }
    public static Vector2 RotateAroundRad(this Vector2 pointToRotate, Vector2 centerPoint, double angleInRadians, bool floor = false)
    {
        double cosTheta = Math.Cos(angleInRadians);
        double sinTheta = Math.Sin(angleInRadians);
        double x = (cosTheta * (pointToRotate.X - centerPoint.X) -
        sinTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.X);
        double y = (sinTheta * (pointToRotate.X - centerPoint.X) +
                cosTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.Y);
        if (floor)
        {
            return new Vector2((int)x, (int)y);
        }
        else
        {
            return new Vector2((float)x, (float)y);
        }
    }

    public static void RenderAt(this TileGrid grid, Vector2 position, int xPad, int yPad)
    {
        if (grid.Alpha <= 0f)
        {
            return;
        }

        if (grid.ClipCamera == null && grid.Scene is Level level)
        {
            grid.ClipCamera = level.Camera;
        }

        Rectangle clippedRenderTiles = grid.GetClippedRenderTiles();
        int tileWidth = grid.TileWidth;
        int tileHeight = grid.TileHeight;
        Color color = grid.Color * grid.Alpha;
        Vector2 position2 = new Vector2(position.X + xPad * tileWidth + (float)(clippedRenderTiles.Left * tileWidth), position.Y + yPad * tileHeight + (float)(clippedRenderTiles.Top * tileHeight));
        for (int i = clippedRenderTiles.Left + xPad; i < clippedRenderTiles.Right - xPad; i++)
        {
            for (int j = clippedRenderTiles.Top + yPad; j < clippedRenderTiles.Bottom - yPad; j++)
            {
                MTexture mTexture = grid.Tiles[i, j];
                if (mTexture != null)
                {
                    Draw.SpriteBatch.Draw(mTexture.Texture.Texture_Safe, position2, mTexture.ClipRect, color);
                }

                position2.Y += tileHeight;
            }
            position2.X += tileWidth;
            position2.Y = position.Y + (float)(clippedRenderTiles.Top * tileHeight) + yPad * tileHeight;
        }
    }
    public static void RenderAt(this AnimatedTiles grid, Vector2 position, int xPad, int yPad)
    {
        Rectangle clippedRenderTiles = grid.GetClippedRenderTiles(1);
        Color color = grid.Color * grid.Alpha;
        for (int i = clippedRenderTiles.Left + xPad; i < clippedRenderTiles.Right - xPad; i++)
        {
            for (int j = clippedRenderTiles.Top + yPad; j < clippedRenderTiles.Bottom - yPad; j++)
            {
                List<AnimatedTiles.Tile> list = grid.tiles[i, j];
                if (list != null)
                {
                    for (int k = 0; k < list.Count; k++)
                    {
                        AnimatedTiles.Tile tile = list[k];
                        AnimatedTilesBank.Animation animation = grid.Bank.Animations[tile.AnimationID];
                        animation.Frames[(int)tile.Frame % animation.Frames.Length].Draw(position + animation.Offset + new Vector2((float)i + 0.5f, (float)j + 0.5f) * 8f, animation.Origin, color, tile.Scale);
                    }
                }
            }
        }
    }
    public static Player GetPlayer(this Level level) => level.Tracker.GetEntity<Player>();
    public static Player GetPlayer(this Scene scene) => (scene as Level).GetPlayer();

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
