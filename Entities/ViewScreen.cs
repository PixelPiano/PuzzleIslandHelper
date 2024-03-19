using System;
using System.Collections.Generic;
using Monocle;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using System.Collections;
using FrostHelper;
using FrostHelper.ModIntegration;
using Celeste.Mod.PuzzleIslandHelper.Effects;

namespace Celeste.Mod.PuzzleIslandHelper.Helpers
{
    [Tracked]
    public class ViewScreen : TalkComponent
    {
        //TalkComponent for the screen entity
        public Image Image;
        public TalkComponent Talk;
        public ViewContent Content;
        private Rectangle screen;
        public string[] Dialogs;
        public void Interact(Player player)
        {
            Scene?.Add(Content = new ViewContent((Scene as Level).Camera.Position.ToInt(), screen, Dialogs, true));
            //player.StateMachine.FlagState = 11;
        }
        public ViewScreen(float width, float height, Rectangle screen, string[] dialogs) : base(new Rectangle(0, 0, (int)width, (int)height), Vector2.UnitX * width / 2, null)
        {
            Dialogs = dialogs;
            OnTalk = Interact;
            this.screen = screen;
        }
        public override void Removed(Entity entity)
        {
            base.Removed(entity);
            if (Content != null)
            {
                Scene?.Remove(Content);
            }
            Content = null;
        }
    }
    [Tracked]
    public class ViewContent : Entity
    {
        //Background and screen surface rendering
        private Image image = new Image(GFX.Game["objects/PuzzleIslandHelper/TEST/curveTest"]);
        private float alpha;
        public bool FullyIn;
        public bool FullyOut;

        public bool Closing;
        public Vector2 Size;
        public int why = 1;
        public Vector2 Dimensions => new Vector2(Screen.Width, Screen.Height);
        public Vector2 Offset => new Vector2(Screen.X, Screen.Y);
        public Rectangle Screen;
        public ViewContentText Text;
        private static VirtualRenderTarget _Target;
        private static VirtualRenderTarget _Target2;
        private static VirtualRenderTarget _Mask;
        public static VirtualRenderTarget Target => _Target ??=
                      VirtualContent.CreateRenderTarget("ViewContentTarget", 320, 180);
        public static VirtualRenderTarget Target2 => _Target2 ??=
              VirtualContent.CreateRenderTarget("ViewContentTarget2", 320, 180);
        public static VirtualRenderTarget Mask => _Mask ??= VirtualContent.CreateRenderTarget("ViewContentMask", 320, 180);
        public class ViewContentText : Entity //TextHelper renderer
        {

            public bool Finished;
            private int CurrentLine = 1;
            private int CurrentNode;
            private int CurrentID;
            private int MaxLineWidth;
            private int StartNode;

            private float TextOpacity = 1;
            private float LineSpace;

            private List<string> DialogIDs = new();

            private FancyTextExt.Text FText;
            public Vector2 Scale;
            public float Size;
            public int MaxLines;
            public float LineYOffset;
            public Rectangle Screen;
            private static VirtualRenderTarget _TextTarget;
            public static VirtualRenderTarget TextTarget => _TextTarget ??=
                          VirtualContent.CreateRenderTarget("ViewContentTextTarget", 320, 180);
            public ViewContentText(Vector2 RenderPosition, string[] dialogs, float textSize, int maxLineWidth, int maxLines, Rectangle screen)
            {
                Screen = screen;
                Position = RenderPosition;
                Size = textSize;
                Tag = TagsExt.SubHUD;
                MaxLineWidth = maxLineWidth;
                MaxLines = maxLines;
                DialogIDs.AddRange(dialogs);
                Add(new BeforeRenderHook(BeforeRender));

            }
            #region Rendering
            public void ApplyParameters()
            {
                if (Scene is not Level level)
                {
                    return;
                }
                var parameters = ShaderFX.CurvedScreen.Parameters;
                Matrix? camera = level.Camera.Matrix;
                parameters["DeltaTime"]?.SetValue(Engine.DeltaTime);
                parameters["Time"]?.SetValue(Engine.Scene.TimeActive);
                parameters["CamPos"]?.SetValue(level.Camera.Position);
                parameters["Dimensions"]?.SetValue(new Vector2(Screen.Width * 6, Screen.Height * 6));
                parameters["Offset"]?.SetValue(new Vector2(Screen.X * 6, Screen.Y * 6));
                parameters["ColdCoreMode"]?.SetValue(level.CoreMode == Session.CoreModes.Cold);

                Viewport viewport = Engine.Graphics.GraphicsDevice.Viewport;

                Matrix projection = Matrix.CreateOrthographicOffCenter(0, viewport.Width, viewport.Height, 0, 0, 1);
                // from communal helper
                Matrix halfPixelOffset = Matrix.Identity;

                parameters["TransformMatrix"]?.SetValue(halfPixelOffset * projection);

                parameters["ViewMatrix"]?.SetValue(camera ?? Matrix.Identity);
            }
            public void BeforeRender()
            {
                if (Scene is not Level level)
                {
                    return;
                }
                //ApplyParameters();
                TextTarget.DrawToObject(DrawText, Matrix.Identity, true, ShaderFX.CurvedScreen);
            }
            public void DrawText()
            {
                FText.Draw(Position - Vector2.UnitY * (CurrentLine * LineSpace), Vector2.Zero, Size * Vector2.One, 1, Color.White * TextOpacity, StartNode, CurrentNode);
            }
            public override void Render()
            {
                base.Render();
                Draw.SpriteBatch.Draw(TextTarget, Vector2.Zero, Color.White);
            }
            #endregion
            private void LoadText(int maxLineWidth, int linesPerPage, Vector2 offset)
            {
                FText = FancyTextExt.Parse(Dialog.Get(DialogIDs[CurrentID]), maxLineWidth, linesPerPage, offset);
                LineSpace = FText.BaseSize * Size;
            }
            public override void Awake(Scene scene)
            {
                base.Awake(scene);
                LoadText(MaxLineWidth, MaxLines, Vector2.UnitX * MaxLineWidth);
                FText.BaseSize = Size;
                Add(new Coroutine(Cutscene()));
            }

            public override void Update()
            {
                base.Update();
                LineYOffset = CurrentLine * LineSpace * 6;
            }
            #region Routines
            private void Advance(int next)
            {
                StartNode = 0;
                CurrentNode = 0;
                CurrentLine = 0;
                FText = FancyTextExt.Parse(Dialog.Get(DialogIDs[next]), MaxLineWidth, 16, Vector2.UnitX * MaxLineWidth);
            }
            private IEnumerator Cutscene()
            {
                float delayScalar = 1f;
                int charSkip = 5;
                //start scrolling text
                while (true)
                {
                    for (int k = 0; k < DialogIDs.Count; k++)
                    {
                        CurrentLine = 0;
                        while (CurrentNode < FText.Nodes.Count)
                        {
                            int charSkipCount = 0;
                            bool hitChar = false;
                            for (int i = 0; i < FText.Nodes.Count; i++)
                            {
                                FancyTextExt.Node Node = FText.Nodes[i];

                                CurrentNode = i + 1;

                                if (Node is FancyTextExt.Char c)
                                {

                                    if (charSkipCount < charSkip)
                                    {
                                        charSkipCount++;
                                    }
                                    else
                                    {
                                        hitChar = true;
                                        yield return c.Delay * 1.5f * delayScalar;
                                        charSkipCount = 0;
                                    }
                                }
                                if (Node is FancyTextExt.NewLine)
                                {
                                    CurrentLine++;
                                }
                            }
                            if (!hitChar)
                            {
                                yield return null;
                            }
                        }

                        if (k + 1 < DialogIDs.Count)
                        {
                            Advance(k + 1);
                        }
                    }
                }
            }
            #endregion
        }
        public string[] Dialogs;
        public ViewContent(Vector2 position, Rectangle screen, string[] dialogs, bool startActive = false) : base(position)
        {
            Add(image);
            image.Visible = false;
            Depth = -10000;
            Dialogs = dialogs;
            Screen = screen;
            if (startActive)
            {
                alpha = 1;
                FullyIn = true;
                FullyOut = false;
            }
            else
            {
                FullyOut = true;
                FullyIn = false;
                Fade(true, null, 1);
            }
            Add(new BeforeRenderHook(BeforeRender));
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            //TextHelper = new ViewContentText(Position + Offset, Dialogs, 35f, (int)Dimensions.X, 0, Screen);
        }
        public void Fade(bool fadeIn, Ease.Easer ease = null, float duration = 1)
        {
            if (ease is null)
            {
                ease = Ease.Linear;
            }
            Tween t = Tween.Create(Tween.TweenMode.Oneshot, ease, duration);
            t.OnUpdate = (Tween t) =>
            {
                FullyIn = false;
                FullyOut = false;
                alpha = fadeIn ? t.Eased : 1 - t.Eased;
            };
            t.OnComplete = (Tween t) =>
            {
                alpha = fadeIn ? 1 : 0;
                if (fadeIn)
                {
                    FullyIn = true;
                    FullyOut = false;
                }
                else
                {
                    FullyOut = true;
                    FullyIn = false;
                }
            };
            Add(t);
            t.Start();
        }

        public override void Render()
        {
            base.Render();
            Draw.SpriteBatch.Draw(Target2, Position, Color.White);
        }
        private void CoolStuff()
        {
            Draw.Rect(Vector2.Zero, 320, 180, Color.White);
            image.RenderPosition = Vector2.Zero;
            image.Render();
            int lines = 4;
            for (int i = 0; i < lines; i++)
            {
                float yOffset = 180 / lines * i;
                Draw.Line(Vector2.UnitY * yOffset, new Vector2(320, yOffset), Color.Green, 3);
            }

        }
        public void BeforeRender()
        {
            /*How to render stuff with shader effects:
             * 1. You need two render targets.
             * 2. Render the stuff you want affected by the shader to RT 1 (in this case, whatever is in "CoolStuff()".
             * 3. Draw the contents of RT 1 to RT 2, but this acceleration using the ShaderFX.CurvedScreen.
             * 4. In Render(), draw RT 2.
             * Note: ID avoid inconsistent render positions, use Matrix.Identity/Vector2.Zero in BeforeRender(), and then render to whatever position you like in Render().
             * Also stop pestering rendering pros about your jank stuff ok
             */
            if (Scene is not Level level) return;
            ShaderFX.CurvedScreen.ApplyStandardParameters();
            Target.DrawToObject(CoolStuff, Matrix.Identity, true);

            Engine.Instance.GraphicsDevice.SetRenderTarget(Target2);
            Engine.Instance.GraphicsDevice.Clear(Color.Transparent);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, ShaderFX.CurvedScreen, Matrix.Identity);
            Draw.SpriteBatch.Draw(Target, Vector2.Zero, Color.White);
            Draw.SpriteBatch.End();
            //Target.MaskToObject(Mask);
        }

        public void ApplyParameters(Level level)
        {
            var parameters = ShaderFX.CurvedScreen.Parameters;
            Viewport viewport = Engine.Graphics.GraphicsDevice.Viewport;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, viewport.Width, viewport.Height, 0, 0, 1);
            parameters["DeltaTime"]?.SetValue(Engine.DeltaTime);
            parameters["Time"]?.SetValue(Engine.Scene.TimeActive);
            parameters["CamPos"]?.SetValue(level.Camera.Position);
            parameters["Dimensions"]?.SetValue(new Vector2(320, 180));// * (GameplayBuffers.Gameplay.Width / 320));
            //parameters["Offset"]?.SetValue(Offset);
            parameters["ColdCoreMode"]?.SetValue(level.CoreMode == Session.CoreModes.Cold);
            parameters["TransformMatrix"]?.SetValue(projection);
            parameters["ViewMatrix"]?.SetValue(level.Camera.Matrix);
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            if (Text != null)
            {
                scene.Remove(Text);
                Text = null;
            }
            _Target?.Dispose();
            _Mask?.Dispose();
            _Mask = null;
            _Target = null;
        }
        public override void Update()
        {
            base.Update();
            if (Closing)
            {
                if (FullyOut)
                {
                    RemoveSelf();
                }
            }
        }
    }
}
