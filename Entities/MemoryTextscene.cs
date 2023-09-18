using Celeste.Mod.Entities;
using ExtendedVariants.Variants.Vanilla;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/MemoryTextscene")]
    [Tracked]
    public class MemoryTextscene : Entity
    {
        private static readonly Dictionary<string, List<string>> fontPaths;
        static MemoryTextscene()
        {
            // Fonts.paths is private static and never instantiated besides in the static constructor, so we only need to get the reference to it once.
            fontPaths = (Dictionary<string, List<string>>)typeof(Fonts).GetField("paths", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
        }
        private string text;
        private Vector2 LastPosition;
        private bool Waiting;
        private bool Continue;
        private Entity ArrowEntity;
        private Sprite Arrow;
        private float ArrowColorLerp;
        private Level level;
        private float SolidOpacity;
        private Vector2 ArrowPosition;
        private float TextOpacity = 1;
        private FancyTextExt.Text activeText;
        private const string fontName = "alarm clock";
        private const int XOffset = 1920 / 16;
        private const int MaxLineWidth = 1920 - (XOffset * 2);

        private int CurrentNode;
        private readonly VirtualRenderTarget Target = VirtualContent.CreateRenderTarget("MemoryTextscene", 1920, 1080);
        public MemoryTextscene(Vector2 Position)
            : base(Position)
        {
            Tag |= TagsExt.SubHUD;
            Depth = -1000001;
            Arrow = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/memoryTextscene/");
            Arrow.AddLoop("idle", "arrow", 0.1f);
            Arrow.Play("idle");
            Arrow.Visible = false;
            Arrow.Color = Color.White * 0;

            Add(new BeforeRenderHook(BeforeRender));
        }

        public MemoryTextscene(EntityData data, Vector2 offset)
            : this(data.Position + offset)
        { }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            level = scene as Level;
            activeText = FancyTextExt.Parse(Dialog.Get("TEXT6"), MaxLineWidth, 16);
            Add(new Coroutine(Cutscene()));
        }

        public override void Update()
        {
            base.Update();
            if (Waiting)
            {
                if (ArrowColorLerp < 1)
                {
                    ArrowColorLerp += Engine.DeltaTime;
                }
                if (Input.DashPressed)
                {
                    Waiting = false;
                }
            }
            else if (ArrowColorLerp > 0)
            {
                ArrowColorLerp -= Engine.DeltaTime;
            }


            Arrow.Color *= ArrowColorLerp;
        }

        private IEnumerator Cutscene()
        {
            for (float i = 0; i < 1f; i += Engine.DeltaTime)
            {
                SolidOpacity = Calc.LerpClamp(0, 0.4f, i);
                yield return null;
            }

            while (CurrentNode < activeText.Nodes.Count)
            {

                for (int i = 0; i < activeText.Nodes.Count; i++)
                {
                    FancyTextExt.Node Node = activeText.Nodes[i];
                    CurrentNode = i + 1;
                    if (Node is FancyTextExt.Char)
                    {
                        LastPosition = (Node as FancyTextExt.Char).LastPosition;
                        yield return (Node as FancyTextExt.Char).Delay * 1.5f;
                    }
                    if (Node is FancyTextExt.NewSegment)
                    {
                        CurrentNode += (Node as FancyTextExt.NewSegment).Lines - 1;
                        level.Add(ArrowEntity = new Entity(LastPosition));
                        ArrowEntity.Add(Arrow);
                        
                        Arrow.Scale = Vector2.One * 6;
                        Tween ArrowTween = Tween.Create(Tween.TweenMode.YoyoLooping, Ease.SineOut,0.8f);
                        ArrowTween.OnUpdate = (Tween t) =>
                        {
                            ArrowEntity.Position.X = LastPosition.X + Arrow.Width + 4;
                            ArrowEntity.Position.Y = LastPosition.Y - 8 * t.Eased;
                        };
                        Add(ArrowTween);
                        ArrowTween.Start();
                        Waiting = true;

                        Arrow.Visible = true;
                        while (Waiting)
                        {
                            yield return null;
                        }
                        while(ArrowColorLerp > 0)
                        {
                            yield return null;
                        }
                        ArrowTween.Stop();
                        Continue = false;
                    }
                }
                yield return null;
            }
            yield return 3;
            for (float i = 0; i < 1f; i += Engine.DeltaTime)
            {
                SolidOpacity = Calc.LerpClamp(0.4f, 0, i);
                TextOpacity = Calc.LerpClamp(1, 0, i);
                yield return null;
            }
            SolidOpacity = 0;
            yield return null;
        }
        /*        static List<string> WrapText(string text, double pixels)
                {
                    string[] originalLines = text.Split(new string[] { " " },
                        StringSplitOptions.None);

                    List<string> wrappedLines = new List<string>();

                    StringBuilder actualLine = new StringBuilder();
                    double actualWidth = 0;

                    foreach (string item in originalLines)
                    {
                        actualLine.Append(item + " ");
                        actualWidth += ActiveFont.Measure(item).X * 2;

                        if (actualWidth > pixels || item == "{n}")
                        {
                            wrappedLines.Add(actualLine.ToString());
                            actualLine.Clear();
                            actualWidth = 0;
                        }
                    }

                    if (actualLine.Length > 0)
                        wrappedLines.Add(actualLine.ToString());

                    return wrappedLines;
                }*/

        private void BeforeRender()
        {
            EasyRendering.DrawToObject(Target, Drawing, level, true, true);
        }
        private void DrawOutline(Vector2 Position, int stroke, Vector2 Scale, float alpha = 1)
        {

            for (int i = 1; i <= stroke; i++)
            {
                Vector2 offset = Vector2.UnitX * i;

                activeText.Draw(Position + offset, Vector2.Zero, Scale, alpha, Color.Black, 0, CurrentNode);

                offset = -Vector2.UnitX * i;

                activeText.Draw(Position + offset, Vector2.Zero, Scale, alpha, Color.Black, 0, CurrentNode);

                offset = Vector2.UnitY * i;

                activeText.Draw(Position + offset, Vector2.Zero, Scale, alpha, Color.Black, 0, CurrentNode);

                offset = -Vector2.UnitY * i;

                activeText.Draw(Position + offset, Vector2.Zero, Scale, alpha, Color.Black, 0, CurrentNode);
            }

            activeText.Draw(Position, Vector2.Zero, Scale, 1, Color.White, 0, CurrentNode);
        }
        private void Drawing()
        {
            /*
                        List<string> lines = Regex.Split(Dialog.Clean("TEXT6"), "{n}", RegexOptions.None).ToList();
                        string text = Dialog.Clean("TEXT6");
                        List<string> lines = WrapText(text, MaxLineWidth);
                        for (int i = 0; i < lines.Count; i++)
                        {
                            Vector2 Position = new Vector2(0, XOffset + 48 * i);
                            ActiveFont.DrawOutline(lines[i], Position, Vector2.Zero, Vector2.One, Color.White, 1, Color.Black);
                        }*/
            Vector2 Position = new Vector2(XOffset, XOffset);
            DrawOutline(Position, 6, Vector2.One, 2);


        }
        public override void Render()
        {
            base.Render();
            Draw.Rect(0, 0, 1920, 1080, Color.Black * SolidOpacity);
            Draw.SpriteBatch.Draw(Target, Vector2.Zero, Color.White * TextOpacity);
        }

        private void ensureCustomFontIsLoaded()
        {
            if (Fonts.Get(fontName) == null)
            {
                // this is a font we need to load for the cutscene specifically!
                if (!fontPaths.ContainsKey(fontName))
                {
                    // the font isn't in the list... so we need to list fonts again first.
                    Logger.Log(LogLevel.Warn, "PuzzleIslandHelper/EscapeTimer", $"We need to list fonts again, {fontName} does not exist!");
                    Fonts.Prepare();
                }

                Fonts.Load(fontName);
                Engine.Scene.Add(new FontHolderEntity());
            }
        }
        // a small entity that just ensures the font loaded by the timer unloads upon leaving the map.
        private class FontHolderEntity : Entity
        {
            public FontHolderEntity()
            {
                Tag = Tags.Global;
            }

            public override void SceneEnd(Scene scene)
            {
                base.SceneEnd(scene);
                Fonts.Unload(fontName);
            }
        }
    }

}

