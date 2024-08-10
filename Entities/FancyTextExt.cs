using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;


namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    /*
     * This literally only adds a Color parameter to the draw calls and nothing else. I'm not experienced enough to know how to do this better
     * 
     * just kidding this is now future me, i have added an offset feature because i am a gremlin HHEE HEE HOO HOO HA
     * 
     * future future me here, THE OFFSET FEATURE CAME IN CLUTCH LETS GO
     */
    public class FancyTextExt
    {
        public class Node
        {
        }
        public class Char : Node
        {
            public Vector2 Offset;

            public int Index;

            public Vector2 LastPosition;

            public int Character;

            public float Position;

            public int Line;

            public int Page;

            public float Delay;

            public float LineWidth;

            public Color Color;

            public float Scale;

            public float Rotation;

            public float YOffset;

            public float Fade;

            public bool Shake;

            public bool Wave;

            public bool Impact;

            public bool IsPunctuation;

            public float Width;

            public void Draw(PixelFont font, float baseSize, Vector2 position, Vector2 scale, float alpha, Color color)
            {
                Color _color = Color;
                if (57344 <= Character && Character <= Emoji.Last && !Emoji.IsMonochrome((char)Character))
                {
                    Color = new Color(Color.A, Color.A, Color.A, Color.A);
                }

                orig_Draw(font, baseSize, position, scale, alpha, color);
            }

            public void orig_Draw(PixelFont font, float baseSize, Vector2 position, Vector2 scale, float alpha, Color color)
            {

                float num = (Impact ? (2f - Fade) : 1f) * Scale;
                Vector2 zero = Vector2.Zero;
                Vector2 vector = scale * num;
                PixelFontSize pixelFontSize = font.Get(baseSize * Math.Max(vector.X, vector.Y));
                PixelFontCharacter pixelFontCharacter = pixelFontSize.Get(Character);
                vector *= baseSize / pixelFontSize.Size;
                position.X += (Position) * scale.X;
                position += Offset * scale;
                zero += (Shake ? (new Vector2(-1 + Calc.Random.Next(3), -1 + Calc.Random.Next(3)) * 2f) : Vector2.Zero);
                zero += (Wave ? new Vector2(0f, (float)Math.Sin((float)Index * 0.25f + Engine.Scene.RawTimeActive * 8f) * 4f) : Vector2.Zero);
                zero.X += pixelFontCharacter.XOffset;
                zero.Y += (float)pixelFontCharacter.YOffset + (-8f * (1f - Fade) + YOffset * Fade);
                pixelFontCharacter.Texture.Draw(position + zero * vector, Vector2.Zero, color * Fade * alpha, vector, Rotation);

                LastPosition = position + zero * vector;
            }
        }

        public class Portrait : Node
        {
            public int Side;

            public string Sprite;

            public string Animation;

            public bool UpsideDown;

            public bool Flipped;

            public bool Pop;

            public bool Glitchy;

            public string SfxEvent;

            public int SfxExpression = 1;

            public string SpriteId => "portrait_" + Sprite;

            public string BeginAnimation => "begin_" + Animation;

            public string IdleAnimation => "idle_" + Animation;

            public string TalkAnimation => "talk_" + Animation;
        }

        public class Wait : Node
        {
            public float Duration;
        }

        public class Trigger : Node
        {
            public int Index;

            public bool Silent;

            public string Label;
        }

        public class NewLine : Node
        {
        }

        public class NewPage : Node
        {
        }

        public class NewSegment : Node
        {
            public int Lines;
            public NewSegment(int newLines = 1)
            {
                Lines = newLines;
            }
        }

        public enum Anchors
        {
            Top,
            Middle,
            Bottom
        }

        public class Anchor : Node
        {
            public Anchors Position;
        }

        public class Text
        {

            public List<Node> Nodes;



            public int Lines;

            public int Pages;

            public PixelFont Font;

            public float BaseSize;

            public int Count => Nodes.Count;

            public Node this[int index] => Nodes[index];

            public int GetCharactersOnPage(int start)
            {
                int num = 0;

                for (int i = start; i < Count; i++)
                {
                    if (Nodes[i] is Char)
                    {
                        num++;
                    }
                    else if (Nodes[i] is NewPage)
                    {
                        break;
                    }
                }

                return num;
            }

            public int GetNextPageStart(int start)
            {
                for (int i = start; i < Count; i++)
                {
                    if (Nodes[i] is NewPage)
                    {
                        return i + 1;
                    }
                }

                return Nodes.Count;
            }

            public float WidestLine()
            {
                int num = 0;
                for (int i = 0; i < Nodes.Count; i++)
                {
                    if (Nodes[i] is Char)
                    {
                        num = Math.Max(num, (int)(Nodes[i] as Char).LineWidth);
                    }
                }

                return num;
            }

            public void Draw(Vector2 position, Vector2 justify, Vector2 scale, float alpha, Color color, int start = 0, int end = int.MaxValue)
            {
                int num = Math.Min(Nodes.Count, end);
                int num2 = 0;
                float num3 = 0f;
                float num4 = 0f;

                PixelFontSize pixelFontSize = Font.Get(BaseSize);
                for (int i = start; i < num; i++)
                {
                    if (Nodes[i] is NewLine || Nodes[i] is NewSegment)
                    {
                        if (num3 == 0f)
                        {
                            num3 = 1f;
                        }

                        num4 += num3;
                        num3 = 0f;
                    }
                    else if (Nodes[i] is Char)
                    {
                        num2 = Math.Max(num2, (int)(Nodes[i] as Char).LineWidth);
                        num3 = Math.Max(num3, (Nodes[i] as Char).Scale);
                    }
                    else if (Nodes[i] is NewPage)
                    {
                        break;
                    }
                }

                num4 += num3;
                position -= justify * new Vector2(num2, num4 * (float)pixelFontSize.LineHeight) * scale;
                num3 = 0f;
                for (int j = start; j < num && !(Nodes[j] is NewPage); j++)
                {
                    if (Nodes[j] is NewLine || Nodes[j] is NewSegment)
                    {
                        if (num3 == 0f)
                        {
                            num3 = 1f;
                        }

                        position.Y += (float)pixelFontSize.LineHeight * num3 * scale.Y;
                        num3 = 0f;
                    }
                    if (Nodes[j] is Char)
                    {
                        Char @char = Nodes[j] as Char;

                        @char.Draw(Font, BaseSize, position, scale, alpha, color);
                        num3 = Math.Max(num3, @char.Scale);
                    }
                }
            }

            public void DrawJustifyPerLine(Vector2 position, Vector2 justify, Vector2 scale, float alpha, Color color, Vector2 offset, int start = 0, int end = int.MaxValue)
            {
                int num = Math.Min(Nodes.Count, end);
                float num2 = 0f;
                float num3 = 0f;
                PixelFontSize pixelFontSize = Font.Get(BaseSize);
                for (int i = start; i < num; i++)
                {
                    if (Nodes[i] is NewLine || Nodes[i] is NewSegment)
                    {
                        if (num2 == 0f)
                        {
                            num2 = 1f;
                        }

                        num3 += num2;
                        num2 = 0f;
                    }
                    else if (Nodes[i] is Char)
                    {
                        num2 = Math.Max(num2, (Nodes[i] as Char).Scale);
                    }
                    else if (Nodes[i] is NewPage)
                    {
                        break;
                    }
                }

                num3 += num2;
                num2 = 0f;
                for (int j = start; j < num && !(Nodes[j] is NewPage); j++)
                {
                    if (Nodes[j] is NewLine || Nodes[j] is NewSegment)
                    {
                        if (num2 == 0f)
                        {
                            num2 = 1f;
                        }

                        position.Y += num2 * (float)pixelFontSize.LineHeight * scale.Y;
                        num2 = 0f;
                    }

                    if (Nodes[j] is Char)
                    {
                        Char @char = Nodes[j] as Char;
                        Vector2 vector = -justify * new Vector2(@char.LineWidth, num3 * (float)pixelFontSize.LineHeight) * scale;
                        @char.Draw(Font, BaseSize, position + vector + offset, scale, alpha, color);
                        num2 = Math.Max(num2, @char.Scale);
                    }
                }
            }
        }

        public static Color DefaultColor = Color.LightGray;

        public const float CharacterDelay = 0.01f;

        public const float PeriodDelay = 0.3f;

        public const float CommaDelay = 0.15f;

        public const float ShakeDistance = 2f;

        private Language language;

        private string text;

        private Text group = new Text();

        private int maxLineWidth;

        private int linesPerPage;

        private PixelFont font;

        public PixelFontSize size;

        private Color defaultColor;

        private float startFade;

        private int currentLine;

        private int currentPage;

        public float currentPosition;

        private Color currentColor;

        private float currentScale = 1f;

        private float currentDelay = 0.01f;

        private bool currentShake;

        private bool currentWave;

        private bool currentImpact;

        private bool currentMessedUp;

        private int currentCharIndex;

        private Vector2 offset;
        private Vector2 currentOffset = Vector2.Zero;
        private float tabOffset;
        public static Text Parse(string text, int maxLineWidth, int linesPerPage, Vector2 offset, float startFade = 1f, Color? defaultColor = null, Language language = null)
        {
            return new FancyTextExt(text, maxLineWidth, linesPerPage, offset, startFade, defaultColor.HasValue ? defaultColor.Value : DefaultColor, language).Parse();
        }

        private FancyTextExt(string text, int maxLineWidth, int linesPerPage, Vector2 offset, float startFade, Color defaultColor, Language language)
        {

            this.text = text;
            this.maxLineWidth = maxLineWidth;
            this.linesPerPage = ((linesPerPage < 0) ? int.MaxValue : linesPerPage);
            this.startFade = startFade;
            this.defaultColor = (currentColor = defaultColor);
            if (language == null)
            {
                language = Dialog.Language;
            }

            this.language = language;
            group.Nodes = new List<Node>();
            this.offset = offset;
            group.Font = (font = Fonts.Get(language.FontFace));
            group.BaseSize = language.FontFaceSize;
            size = font.Get(group.BaseSize);
        }
        private static bool EvaluateText(string text, List<string> list)
        {
            bool found = false;
            if (Engine.Scene is Level level && level.Tracker.GetEntity<Calidus>() is Calidus calidus)
            {
                if ((text[0] == 'c') && list.Count > 1)
                {
                    string output = "Found valid c command:";
                    for (int i = 0; i < list.Count; i++)
                    {
                        output += "\n" + i + ":" + list[i];
                    }
                    Console.WriteLine(output);
                    if (list[0].Equals("look"))
                    {
                        if (Enum.TryParse(list[1], true, out Calidus.Looking result))
                        {
                            Console.WriteLine("Found valid look command: " + result);
                            calidus.Look(result);
                            found = true;
                        }

                    }
                    else if (list[0].Equals("mood") && Enum.TryParse(list[1], true, out Calidus.Mood result2))
                    {
                        Console.WriteLine("Found valid mood command: " + list[1]);
                        calidus.Emotion(result2);
                        found = true;
                    }
                }
            }
            return found;
        }
        private Text Parse()
        {
            string[] array = Regex.Split(this.text, language.SplitRegex);
            string[] array2 = new string[array.Length];
            int num = 0;
            for (int i = 0; i < array.Length; i++)
            {
                if (!string.IsNullOrEmpty(array[i]))
                {
                    array2[num++] = array[i];
                }
            }

            Stack<Color> stack = new Stack<Color>();
            Portrait[] array3 = new Portrait[2];
            for (int j = 0; j < num; j++)
            {
                if (array2[j] == "{")
                {
                    j++;
                    string text = array2[j++];
                    List<string> list = new List<string>();
                    for (; j < array2.Length && array2[j] != "}"; j++)
                    {
                        if (!string.IsNullOrWhiteSpace(array2[j]))
                        {
                            list.Add(array2[j]);
                        }
                    }

                    float result = 0f;
                    if (float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
                    {
                        group.Nodes.Add(new Wait
                        {
                            Duration = result
                        });
                        continue;
                    }

                    if (text[0] == '#')
                    {
                        string text2 = "";
                        if (text.Length > 1)
                        {
                            text2 = text.Substring(1);
                        }
                        else if (list.Count > 0)
                        {
                            text2 = list[0];
                        }

                        if (string.IsNullOrEmpty(text2))
                        {
                            if (stack.Count > 0)
                            {
                                currentColor = stack.Pop();
                            }
                            else
                            {
                                currentColor = defaultColor;
                            }

                            continue;
                        }

                        stack.Push(currentColor);
                        switch (text2)
                        {
                            case "red":
                                currentColor = Color.Red;
                                break;
                            case "green":
                                currentColor = Color.Green;
                                break;
                            case "blue":
                                currentColor = Color.Blue;
                                break;
                            default:
                                currentColor = Calc.HexToColor(text2);
                                break;
                        }

                        continue;
                    }
                    if (text[0] == 'n' && text != "n")
                    {
                        int loops = text[1] - '0';
                        AddNewSegment(loops);
                        for (int i = 0; i < loops; i++)
                        {
                            AddNewLine();
                        }
                        continue;
                    }
                    switch (text)
                    {
                        case "break":
                            CalcLineWidth();
                            currentPage++;
                            group.Pages++;
                            currentLine = 0;
                            currentPosition = 0f;
                            group.Nodes.Add(new NewPage());
                            continue;
                        case "n":
                            AddNewLine();
                            continue;
                        case ">>":
                            {
                                if (list.Count > 0 && float.TryParse(list[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var result2))
                                {
                                    currentDelay = 0.01f / result2;
                                }
                                else
                                {
                                    currentDelay = 0.01f;
                                }

                                continue;
                            }
                    }
                    if (text.Equals("/>>"))
                    {
                        currentDelay = 0.01f;
                        continue;
                    }

                    if (text.Equals("anchor"))
                    {
                        if (Enum.TryParse<Anchors>(list[0], ignoreCase: true, out var result3))
                        {
                            group.Nodes.Add(new Anchor
                            {
                                Position = result3
                            });
                        }

                        continue;
                    }

                    if (text.Equals("portrait") || text.Equals("left") || text.Equals("right"))
                    {
                        Portrait item;
                        if (text.Equals("portrait") && list.Count > 0 && list[0].Equals("none"))
                        {
                            item = new Portrait();
                            group.Nodes.Add(item);
                            continue;
                        }

                        if (text.Equals("left"))
                        {
                            item = array3[0];
                        }
                        else if (text.Equals("right"))
                        {
                            item = array3[1];
                        }
                        else
                        {
                            item = new Portrait();
                            foreach (string item2 in list)
                            {
                                if (item2.Equals("upsidedown"))
                                {
                                    item.UpsideDown = true;
                                }
                                else if (item2.Equals("flip"))
                                {
                                    item.Flipped = true;
                                }
                                else if (item2.Equals("left"))
                                {
                                    item.Side = -1;
                                }
                                else if (item2.Equals("right"))
                                {
                                    item.Side = 1;
                                }
                                else if (item2.Equals("pop"))
                                {
                                    item.Pop = true;
                                }
                                else if (item.Sprite == null)
                                {
                                    item.Sprite = item2;
                                }
                                else
                                {
                                    item.Animation = item2;
                                }
                            }
    
                        }

                        if (GFX.PortraitsSpriteBank.Has(item.SpriteId))
                        {
                            List<SpriteDataSource> sources = GFX.PortraitsSpriteBank.SpriteData[item.SpriteId].Sources;
                            for (int num2 = sources.Count - 1; num2 >= 0; num2--)
                            {
                                XmlElement xML = sources[num2].XML;
                                if (xML != null)
                                {
                                    if (item.SfxEvent == null)
                                    {
                                        item.SfxEvent = "event:/char/dialogue/" + xML.Attr("sfx", "");
                                    }

                                    if (xML.HasAttr("glitchy"))
                                    {
                                        item.Glitchy = xML.AttrBool("glitchy", defaultValue: false);
                                    }

                                    if (xML.HasChild("sfxs") && item.SfxExpression == 1)
                                    {
                                        foreach (object item3 in xML["sfxs"])
                                        {
                                            XmlElement xmlElement = item3 as XmlElement;
                                            if (xmlElement != null && xmlElement.Name.Equals(item.Animation, StringComparison.InvariantCultureIgnoreCase))
                                            {
                                                item.SfxExpression = xmlElement.AttrInt("index");
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        group.Nodes.Add(item);
                        array3[(item.Side > 0) ? 1 : 0] = item;
                        continue;
                    }

                    if (text.Equals("trigger") || text.Equals("silent_trigger"))
                    {
                        string text3 = "";
                        for (int k = 1; k < list.Count; k++)
                        {
                            text3 = text3 + list[k] + " ";
                        }

                        if (int.TryParse(list[0], out var result4) && result4 >= 0)
                        {
                            group.Nodes.Add(new Trigger
                            {
                                Index = result4,
                                Silent = text.StartsWith("silent"),
                                Label = text3
                            });
                        }

                        continue;
                    }

                    if (text.Contains("offset:"))
                    {
                        bool uv = text.Contains("screen");
                        string[] values = text.Substring(text.IndexOf(':') + 1).Split(',');
                        float x = 0, y = 0;
                        if (values.Length > 0)
                        {
                            x = float.Parse(values[0], NumberStyles.Any);
                        }
                        if (values.Length > 1)
                        {
                            y = float.Parse(values[1], NumberStyles.Any);
                        }
                        float xMult = uv ? Engine.ViewWidth * 1.7f : 1;
                        float yMult = uv ? Engine.ViewHeight : 1;
                        currentOffset = new Vector2(x * xMult, y * yMult);
                        continue;
                    }
                    if (text.Equals("/offset"))
                    {
                        currentOffset = Vector2.Zero;
                        continue;
                    }
                    if (text.Equals("*"))
                    {
                        currentShake = true;
                        continue;
                    }

                    if (text.Equals("/*"))
                    {
                        currentShake = false;
                        continue;
                    }
                    if (text.Equals("tab"))
                    {
                        tabOffset += (int)(size.Size * 1.5f);
                        continue;
                    }
                    if (text.Equals("/tab"))
                    {
                        tabOffset = 0;
                        continue;
                    }
                    if (text.Equals("mtRight"))
                    {
                        currentOffset.X = offset.X;
                        continue;
                    }
                    if (text.Equals("/mtRight"))
                    {
                        currentOffset.X = 0;
                        continue;
                    }
                    if (text.Equals("~"))
                    {
                        currentWave = true;
                        continue;
                    }

                    if (text.Equals("/~"))
                    {
                        currentWave = false;
                        continue;
                    }

                    if (text.Equals("!"))
                    {
                        currentImpact = true;
                        continue;
                    }

                    if (text.Equals("/!"))
                    {
                        currentImpact = false;
                        continue;
                    }

                    if (text.Equals("%"))
                    {
                        currentMessedUp = true;
                        continue;
                    }

                    if (text.Equals("/%"))
                    {
                        currentMessedUp = false;
                        continue;
                    }

                    if (text.Equals("big"))
                    {
                        currentScale = 1.5f;
                        continue;
                    }

                    if (text.Equals("/big"))
                    {
                        currentScale = 1f;
                        continue;
                    }

                    if (text.Equals("s"))
                    {
                        int result5 = 1;
                        if (list.Count > 0)
                        {
                            int.TryParse(list[0], out result5);
                        }

                        currentPosition += 5 * result5;
                        continue;
                    }

                    if (!text.Equals("savedata"))
                    {
                        continue;
                    }

                    if (SaveData.Instance == null)
                    {
                        if (list[0].Equals("name", StringComparison.OrdinalIgnoreCase))
                        {
                            AddWord("Madeline");
                        }
                        else
                        {
                            AddWord("[SD:" + list[0] + "]");
                        }
                    }
                    else if (list[0].Equals("name", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!language.CanDisplay(SaveData.Instance.Name))
                        {
                            AddWord(Dialog.Clean("FILE_DEFAULT", language));
                        }
                        else
                        {
                            AddWord(SaveData.Instance.Name);
                        }
                    }
                    else
                    {
                        FieldInfo field = typeof(SaveData).GetField(list[0]);
                        AddWord(field.GetValue(SaveData.Instance).ToString());
                    }
                }
                else
                {
                    AddWord(array2[j]);
                }
            }

            CalcLineWidth();
            return group;
        }

        private void CalcLineWidth()
        {
            Char @char = null;
            int num = group.Nodes.Count - 1;
            while (num >= 0 && @char == null)
            {
                if (group.Nodes[num] is Char)
                {
                    @char = group.Nodes[num] as Char;
                }
                else if (group.Nodes[num] is NewLine || group.Nodes[num] is NewPage || group.Nodes[num] is NewSegment)
                {
                    return;
                }

                num--;
            }

            if (@char == null)
            {
                return;
            }

            float lineWidth = (@char.LineWidth = @char.Position + (float)size.Get(@char.Character).XAdvance * @char.Scale);
            while (num >= 0 && !(group.Nodes[num] is NewLine) && !(group.Nodes[num] is NewPage) && !(group.Nodes[num] is NewSegment))
            {
                if (group.Nodes[num] is Char)
                {
                    (group.Nodes[num] as Char).LineWidth = lineWidth;
                }

                num--;
            }
        }

        private void AddNewLine()
        {
            CalcLineWidth();
            currentLine++;
            currentPosition = 0f;
            group.Lines++;
            tabOffset = 0;
            if (currentLine > linesPerPage)
            {
                group.Pages++;
                currentPage++;
                currentLine = 0;
                group.Nodes.Add(new NewPage());
            }
            else
            {
                group.Nodes.Add(new NewLine());

            }
        }
        private void AddNewSegment(int lines = 1)
        {
            CalcLineWidth();
            currentLine += lines;
            currentPosition = 0f;
            group.Lines += lines;
            if (currentLine > linesPerPage)
            {
                group.Pages++;
                currentPage++;
                currentLine = 0;
                group.Nodes.Add(new NewPage());
            }
            else
            {
                group.Nodes.Add(new NewSegment(lines));
            }
        }
        private void AddWord(string word)
        {
            word = Emoji.Apply(word);
            orig_AddWord(word);
        }

        private bool Contains(string str, char character)
        {
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == character)
                {
                    return true;
                }
            }

            return false;
        }
        private void orig_AddWord(string word)
        {
            float num = size.Measure(word).X * currentScale;
            if (currentPosition + num > maxLineWidth)
            {
                AddNewLine();
            }

            for (int i = 0; i < word.Length; i++)
            {
                if ((currentPosition == 0f && word[i] == ' ') || word[i] == '\\')
                {
                    continue;
                }

                PixelFontCharacter pixelFontCharacter = size.Get(word[i]);
                if (pixelFontCharacter == null)
                {
                    continue;
                }

                float num2 = 0f;
                if (i == word.Length - 1 && (i == 0 || word[i - 1] != '\\'))
                {
                    if (Contains(language.CommaCharacters, word[i]))
                    {
                        num2 = 0.15f;
                    }
                    else if (Contains(language.PeriodCharacters, word[i]))
                    {
                        num2 = 0.3f;
                    }
                }

                group.Nodes.Add(new Char
                {
                    Index = currentCharIndex++,
                    Character = word[i],
                    Position = currentPosition,
                    Line = currentLine,
                    Page = currentPage,
                    Delay = (currentImpact ? 0.00349999988f : (currentDelay + num2)),
                    Color = currentColor,
                    Scale = currentScale,
                    Rotation = (currentMessedUp ? ((float)Calc.Random.Choose(-1, 1) * Calc.Random.Choose(0.17453292f, 0.34906584f)) : 0f),
                    YOffset = (currentMessedUp ? ((float)Calc.Random.Choose(-3, -6, 3, 6)) : 0f),
                    Fade = startFade,
                    Shake = currentShake,
                    Impact = currentImpact,
                    Wave = currentWave,
                    IsPunctuation = (Contains(language.CommaCharacters, word[i]) || Contains(language.PeriodCharacters, word[i])),
                    Offset = currentOffset + Vector2.UnitX * tabOffset
                });
                currentPosition += (float)pixelFontCharacter.XAdvance * currentScale;
                if (i < word.Length - 1 && pixelFontCharacter.Kerning.TryGetValue(word[i], out var value))
                {
                    currentPosition += (float)value * currentScale;
                }
            }
        }

    }
}

