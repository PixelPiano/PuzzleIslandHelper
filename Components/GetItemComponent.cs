using Celeste.Mod.PuzzleIslandHelper.Entities;
using Celeste.Mod.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Components
{
    [TrackedAs(typeof(PlayerCollider))]
    public class GetItemComponent : PlayerCollider
    {
        public Glimmer Glimmer;
        public Vector2 GlimmerOffset;
        public Vector2 EntityOffset;
        public bool WaitForInput = true;
        public string Text = "";
        public string Subtext = "";
        public Action<Player> OnCollect;
        private string flag;
        private bool removeEntity;
        private Vector2? prevPosition;
        public bool RevertPlayerState;
        private int prevState;
        public bool Running;
        private Vector2? prevSpeed;
        private int? prevDepth;
        private renderer text;
        private Entity fakeEntity;
        public bool Initialized;
        public GetItemComponent(Action<Player> onCollect, string flag, bool removeEntity, string text = "", string subText = "") : base(null)
        {
            this.removeEntity = removeEntity;
            this.flag = flag;
            OnCollide = Activate;
            Glimmer = new Glimmer(Vector2.Zero, Color.White, 10, 8, 2, 3)
            {
                LineWidthTarget = 4,
                LineWidth = 1
            };
            Glimmer.AlphaMult = 0.2f;
            OnCollect = onCollect;
            Text = text;
            Subtext = subText;
        }
        public override void Update()
        {
            base.Update();
            if (Running)
            {
                Glimmer.AlphaMult = Calc.Approach(Glimmer.AlphaMult, 1, Engine.DeltaTime * 5);
                Glimmer.Size = Calc.Approach(Glimmer.Size, 20, Engine.DeltaTime * 10);
                if (Glimmer.LineWidth < 3) Glimmer.LineWidth++;
                if (Glimmer.LineWidthTarget < 8) Glimmer.LineWidthTarget++;
                fakeEntity.Position = Entity.Position;
            }
        }
        public override void Added(Entity entity)
        {
            base.Added(entity);
            if(entity != null && entity.Scene != null && !Initialized)
            {
                Initialize(entity);
            }
        }
        public override void EntityAdded(Scene scene)
        {
            base.EntityAdded(scene);
            if (!Initialized)
            {
                Initialize(Entity);
            }
        }
        public override void EntityAwake()
        {
            base.EntityAwake();
            if (!Initialized)
            {
                Initialize(Entity);
            }
        }
        public void Initialize(Entity entity)
        {
            Initialized = true;
            fakeEntity = new Entity(entity.Position);
            fakeEntity.Depth = entity.Depth + 1;
            fakeEntity.Add(Glimmer);
            entity.Scene.Add(fakeEntity);
            if (entity.Collider != null)
            {
                Glimmer.Position = entity.Collider.HalfSize;
            }
        }
        public override void Removed(Entity entity)
        {
            fakeEntity?.RemoveSelf();
            if (Running)
            {
                End(Scene.GetPlayer());
            }
            Running = false;
            Initialized = false;
            base.Removed(entity);
        }
        public void End(Player player)
        {
            player.DummyAutoAnimate = true;
            player.DummyGravity = true;
            player.EnableMovement();
            if (prevDepth.HasValue)
            {
                Entity.Depth = prevDepth.Value;
            }
            if (player != null)
            {
                if (RevertPlayerState)
                {
                    player.StateMachine.State = prevState;
                }
                if (prevSpeed.HasValue)
                {
                    player.Speed = prevSpeed.Value;
                }
            }
            if (prevPosition.HasValue)
            {
                Entity.Position = prevPosition.Value;
            }
            fakeEntity.RemoveSelf();
            if (removeEntity)
            {
                Entity.RemoveSelf();
            }
            Running = false;
            RemoveSelf();
        }
        public void Activate(Player player)
        {
            prevDepth = Entity.Depth;
            Entity.Depth = int.MinValue;
            Scene.Add(text = new renderer(Text, Subtext, -15f.ToRad()));
            Running = true;
            prevState = player.StateMachine.State;
            if (!string.IsNullOrEmpty(flag))
            {
                SceneAs<Level>().Session.SetFlag(flag);
            }
            prevPosition = Entity.Position;
            player.DisableMovement();
            player.DummyAutoAnimate = false;
            player.DummyGravity = false;
            prevSpeed = player.Speed;
            player.Speed.Y = 0;
            player.Speed.X = 0;
            player.Sprite.Play("pickup");
            Entity.Center = player.TopCenter - Vector2.UnitY * Entity.Height + EntityOffset;
            Audio.Play("event:/game/general/secret_revealed", player.Center);

            OnCollect?.Invoke(player);
            fakeEntity.Add(new Coroutine(cutscene(player)));
        }
        private IEnumerator cutscene(Player player)
        {
            yield return text.cutscene();
            End(player);
        }
        private class renderer : Entity
        {
            [Command("create_text_bg", "")]
            public static void CreateTextBg()
            {
                if (Engine.Scene is Level level)
                {
                    level.Add(new renderer("this is test text aaaaaaaaaaaaa", "this is test subtext aaaaaaaaaaa", -15f.ToRad()));
                }
            }
            private class textBackground : Component
            {
                public Vector2[] Points = new Vector2[4];
                public VertexPositionColor[] Vertices = new VertexPositionColor[4];
                private float Height;
                private float Width;
                public float WidthLerp;
                public Vector2 Offset;
                private Vector2 pad;
                public Color color = Color.Black;
                public Color colorLight;
                public Color colorDark;
                private bool state;
                private static int[] indices = [0, 1, 2, 1, 3, 2];
                private float[] offsets = [5.4f, 34.1f, 12.774f, 0f];
                private float delayTimer;
                public bool AtTarget;
                public textBackground(Vector2 offset, float width, float height, Vector2 pad, Color color, float delay) : base(true, true)
                {
                    delayTimer = delay;
                    Offset = offset;
                    Width = width;
                    Height = height;
                    this.color = color;
                    this.pad = pad;
                    colorLight = Color.Lerp(Color.Lerp(color, Color.White, 0.8f), Color.Gold, 0.2f);
                    colorDark = Color.Lerp(Color.Lerp(color, Color.Black, 0.8f), Color.DarkMagenta, 0.2f);
                }
                public override void Added(Entity entity)
                {
                    base.Added(entity);
                    UpdateVertices();
                }
                public void UpdateVertices()
                {
                    float lerp = Ease.SineInOut(WidthLerp);
                    Rectangle rect = new Rectangle((int)Offset.X, (int)Offset.Y, (int)Width, (int)Height);
                    Vector2 bottomLeft = rect.BottomLeft() - pad.XComp();
                    Vector2 topLeft = rect.TopLeft() - pad.YComp();//bottomLeft + Calc.AngleToVector(Angle, height);
                    Vector2 bottomRight = rect.BottomRight() + pad.YComp();//bottomLeft + Vector2.UnitX * width;
                    Vector2 topRight = rect.TopRight() + pad.XComp();//topLeft + Vector2.UnitX * width;
                    Vertices[0].Position = new(bottomLeft, 0);
                    Vertices[1].Position = new(topLeft, 0);
                    Vertices[2].Position = new(Vector2.Lerp(bottomLeft, bottomRight, lerp), 0);
                    Vertices[3].Position = new(Vector2.Lerp(topLeft, topRight, lerp), 0);
                    if (Engine.Scene != null)
                    {
                        Vertices[0].Color = Color.Lerp(color, colorLight, ((float)Math.Sin(Engine.Scene.TimeActive + offsets[0]) + 1) / 2 * 0.9f);
                        Vertices[1].Color = Color.Lerp(color, colorLight, 0.7f + (float)Math.Sin(Engine.Scene.TimeActive + offsets[1]) * 0.2f);
                        Vertices[2].Color = Color.Lerp(color, colorDark, 0.7f + (float)Math.Sin(Engine.Scene.TimeActive + offsets[2]) * 0.2f);
                        Vertices[3].Color = Color.Lerp(color, colorDark, 0.2f + (float)Math.Sin(Engine.Scene.TimeActive + offsets[3]) * 0.1f);
                    }
                }
                public override void Render()
                {
                    base.Render();
                    SubHudRenderer.EndRender();
                    DrawBanner(Matrix.Identity);
                    SubHudRenderer.BeginRender();
                }
                public void DrawOutline(int outline, Color color, Matrix matrix)
                {
                    VertexPositionColor a = Vertices[0], b = Vertices[1], c = Vertices[2], d = Vertices[3];
                    Vertices[0].Position += new Vector3(-outline, outline, 0);
                    Vertices[1].Position += new Vector3(-outline, -outline, 0);
                    Vertices[2].Position += new Vector3(outline, outline, 0);
                    Vertices[3].Position += new Vector3(outline, -outline, 0);
                    Vertices[0].Color = Vertices[1].Color = Vertices[2].Color = Vertices[3].Color = color;
                    DrawBanner(matrix);
                    Vertices[0] = a;
                    Vertices[1] = b;
                    Vertices[2] = c;
                    Vertices[3] = d;
                }
                public void DrawOffset(Vector2 offset, Matrix matrix, Color? color = null)
                {
                    VertexPositionColor a = Vertices[0], b = Vertices[1], c = Vertices[2], d = Vertices[3];
                    Vector3 o = new Vector3(offset, 0);
                    Vertices[0].Position += o;
                    Vertices[1].Position += o;
                    Vertices[2].Position += o;
                    Vertices[3].Position += o;
                    if (color.HasValue)
                    {
                        Vertices[0].Color = Vertices[1].Color = Vertices[2].Color = Vertices[3].Color = color.Value;
                    }
                    DrawBanner(matrix);
                    Vertices[0] = a;
                    Vertices[1] = b;
                    Vertices[2] = c;
                    Vertices[3] = d;
                }
                public void DrawBanner(Matrix matrix)
                {
                    GFX.DrawIndexedVertices(matrix, Vertices, 4, indices, 2);
                }
                public void Start(bool reversed = false, float delay = 0)
                {
                    AtTarget = false;
                    delayTimer = delay;
                    state = true;
                    Reverse = reversed;
                    WidthLerp = Reverse ? 1 : 0;
                }
                public void Stop()
                {
                    state = false;
                }

                public bool Reverse;
                public override void Update()
                {
                    base.Update();
                    if (state)
                    {
                        if (delayTimer > 0)
                        {
                            delayTimer -= Engine.DeltaTime;
                        }
                        else
                        {
                            if (Reverse)
                            {
                                WidthLerp -= WidthLerp * (1f - (float)Math.Pow(0.0099999997764825821, Engine.DeltaTime));
                                AtTarget = WidthLerp < 0.05f;
                            }
                            else
                            {
                                WidthLerp += (1 - WidthLerp) * (1f - (float)Math.Pow(0.0099999997764825821, Engine.DeltaTime));
                                AtTarget = WidthLerp > 0.95f;
                            }
                        }
                    }

                    UpdateVertices();
                }
            }
            public FancyText.Text Text, Sub;
            public int TextEnd;
            public int SubEnd;
            private textBackground textBg, subBg;
            public float SubtextScale = 0.7f;
            public Vector2 TextCenter => new Vector2(160, 30) * 6;
            public Vector2 TextPosition => TextCenter - Vector2.UnitX * (TextWidth / 2);
            public Vector2 SubtextCenter => TextCenter + Vector2.UnitY * Text.BaseSize * 1.2f;
            public Vector2 SubtextPosition => SubtextCenter - Vector2.UnitX * (SubtextWidth / 2);
            public float TextWidth => Text.WidestLine();
            public float SubtextWidth => Sub.WidestLine() * SubtextScale;
            public float Alpha;
            public renderer(string text, string subtext, float angle) : this(Parse(text, 320 * 6, Color.White), Parse(subtext, 240 * 6, Color.LightGray), angle)
            {
            }
            public static FancyText.Text Parse(string text, int maxLineWidth, Color color)
            {
                return FancyText.Parse(text, maxLineWidth, 10, 1, color);
            }
            public renderer(FancyText.Text text, FancyText.Text subtext, float angle) : base()
            {
                Tag |= TagsExt.SubHUD;
                Text = text;
                Sub = subtext;
            }
            public IEnumerator cutscene()
            {
                if (Sub != null)
                {
                    subBg = new textBackground(SubtextPosition, SubtextWidth, (Sub.Lines + 1) * Sub.BaseSize * SubtextScale, new Vector2(8, 4) * 6, Color.Blue, 1);
                    Add(subBg);
                    subBg.Start();
                }
                if (Text != null)
                {
                    textBg = new textBackground(TextPosition, TextWidth, (Text.Lines + 1) * Text.BaseSize, new Vector2(8, 4) * 6, Color.Magenta, 0);
                    Add(textBg);
                    textBg.Start();
                }
                if (Text != null)
                {
                    while (TextEnd < Text.Count)
                    {
                        var node = Text[TextEnd];
                        TextEnd++;
                        if (node is FancyText.Char)
                        {
                            yield return (node as FancyText.Char).Delay;
                        }
                        else if (node is FancyText.Wait)
                        {
                            yield return (node as FancyText.Wait).Duration;
                        }
                        if (Input.MenuConfirm.Pressed)
                        {
                            TextEnd = Text.Count - 1;
                            break;
                        }
                    }
                    yield return 0.1f;
                }
                if (Sub != null)
                {
                    while (SubEnd < Sub.Count)
                    {
                        var node = Sub[SubEnd];
                        SubEnd++;
                        if (node is FancyText.Char)
                        {
                            yield return (node as FancyText.Char).Delay;
                        }
                        else if (node is FancyText.Wait)
                        {
                            yield return (node as FancyText.Wait).Duration;
                        }
                        if (Input.MenuConfirm.Pressed)
                        {
                            SubEnd = Sub.Count - 1;
                            break;
                        }
                    }
                }
                yield return 0.6f;
                while (!Input.MenuConfirm.Pressed)
                {
                    yield return null;
                }
                yield return 0.8f;
                IEnumerator fadeOutSub()
                {
                    int dec = 2;
                    int count = 0;
                    for (int i = Sub.Count - 1; i > -2; i -= dec)
                    {
                        SubEnd = i;
                        if (count > 10)
                        {
                            count = 0;
                            dec++;
                        }
                        yield return null;
                    }
                    SubEnd = -1;
                    subBg.Start(true, 0f);
                    while (!subBg.AtTarget) yield return null;
                }
                IEnumerator fadeOutMain()
                {
                    int dec = 2;
                    int count = 0;
                    for (int i = Text.Count - 1; i > -2; i -= dec)
                    {
                        TextEnd = i;
                        if (count > 10)
                        {
                            count = 0;
                            dec++;
                        }
                        yield return null;
                    }
                    TextEnd = -1;
                    textBg.Start(true);
                    while (!textBg.AtTarget) yield return null;
                }
                Coroutine main = new Coroutine(fadeOutMain());
                Coroutine sub = new Coroutine(fadeOutSub());
                Add(sub);
                yield return 0.7f;
                Add(main);
                while (!main.Finished || !sub.Finished)
                {
                    yield return null;
                }
                Collidable = false;
                RemoveSelf();
            }
            public override void Render()
            {
                if (subBg != null)
                {
                    subBg.DrawOffset(new Vector2(-40, 20), Matrix.Identity, Color.Black);
                    if (subBg.AtTarget)
                    {
                        Draw.Rect(Vector2.Zero, 40, 40, Color.Red);
                    }
                }
                if (textBg != null)
                {
                    textBg.DrawOffset(new Vector2(-40, 20), Matrix.Identity, Color.Black);
                    if (textBg.AtTarget)
                    {
                        Draw.Rect(Vector2.One * 40, 40, 40, Color.Lime);
                    }
                }
                if (Sub != null && subBg != null)
                {
                    subBg.Render();
                    if (SubEnd > -1)
                    {
                        Sub.DrawOutlineJustifyPerLine(SubtextCenter, new Vector2(0.5f, 0), Vector2.One * SubtextScale, 1, 0, SubEnd);
                    }
                }
                if (Text != null && textBg != null)
                {
                    textBg.Render();
                    if (TextEnd > -1)
                    {
                        Text.DrawOutlineJustifyPerLine(TextCenter, new Vector2(0.5f, 0), Vector2.One, 1, 0, TextEnd);
                    }
                }
            }
        }
    }
}
