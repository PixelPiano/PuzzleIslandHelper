using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Components.Visualizers.DSPs;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using Celeste.Mod.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Monocle;
using System;
using System.Collections;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.FrequencyEntities
{
    [Tracked]
    public class ForkAmpUI : CutsceneEntity
    {
        [Tracked]
        public class UI : Entity
        {
            public string ID;
            public class osc : Entity
            {
                public bool Enabled => Flag;
                public FlagList Flag;
                public bool Finished;
                public const int WorldWidth = 12;
                public const int WorldHeight = 20;
                public const float ChannelSize = 12f;
                public const int SegmentsPerChannel = 4;
                public const int ScaleTarget = 3;
                public const float SelectPadding = 4;

                public float WIDTH = WorldWidth * 6;
                public float HEIGHT = WorldHeight * 6;
                public float Frequency;
                public float CurrentMin = FrequencyData.Min;
                public float CurrentMax = FrequencyData.Max;
                public bool Interacting = true;
                public float TextScale = 0.5f;

                public float TextOffset = 8;
                public bool Selected;
                public bool Dummy = true;
                public readonly float DummyAlpha = 0.3f;
                public readonly float SelectedAlpha = 1;
                public float Alpha;
                public bool Removing;
                public float Rate;
                public float TargetRate;
                public const float TargetRange = 15;
                public float Effectiveness => 1 - Calc.Clamp(MathHelper.Distance(Rate, TargetRate), 0, TargetRange) / TargetRange;
                public bool OnLeft;
                public int Index;
                public string ID => ui.ID;
                public UI ui;
                public osc(UI ui, Vector2 position, int index, FlagList flag = default) : base(position)
                {
                    this.ui = ui;
                    Flag = flag;
                    Collider = new Hitbox(WIDTH / 6, HEIGHT / 6);
                    Index = index;
                    Visible = false;

                }
                public override void Added(Scene scene)
                {
                    base.Added(scene);
                    Frequency = FrequencyData.GetRate(scene, Index, ID);
                }
                public enum TapGaps
                {
                    Tick,
                    SubChannel,
                    Channel
                }
                public TapGaps TapGap = TapGaps.Tick;
                private int lastDir;
                private float snapTimer;
                public override void Update()
                {
                    base.Update();
                    float prevRate = FrequencyData.GetRate(Scene, Index, ID);
                    int dir = Input.MenuUp ? -1 : Input.MenuDown ? 1 : 0;
                    if (snapTimer > 0)
                    {
                        snapTimer -= Engine.DeltaTime;
                        if (dir == 0)
                        {
                            float jump = TapGap switch
                            {
                                TapGaps.SubChannel => ChannelSize / SegmentsPerChannel,
                                TapGaps.Channel => ChannelSize,
                                _ => 1
                            };
                            Frequency = Calc.Clamp(Calc.Snap(Frequency, jump) - lastDir * jump, FrequencyData.Min, FrequencyData.Max);
                            FrequencyData.SetRate(Scene, Index, Frequency, ID);
                            //if the player taps the button briefly, snap the frequency to a customizable interval
                        }
                    }
                    if (Selected && !Dummy)
                    {
                        if (dir != 0)
                        {
                            if (lastDir == 0)
                            {
                                snapTimer = Engine.DeltaTime * 5;
                                //start the snap window
                            }
                            if (snapTimer <= 0)
                            {
                                Frequency = Calc.Clamp(Frequency - dir, FrequencyData.Min, FrequencyData.Max);
                                FrequencyData.SetRate(Scene, Index, Frequency, ID);
                            }
                        }
                        lastDir = dir;
                    }
                    if (Selected)
                    {
                        if (prevRate != Frequency)
                        {
                            foreach (FrequencyComponent c in Scene.Tracker.GetComponents<FrequencyComponent>())
                            {
                                c.OnFrequencyChanged?.Invoke(Index, prevRate, Frequency, ID);
                            }
                        }
                        else
                        {
                            foreach (FrequencyComponent c in Scene.Tracker.GetComponents<FrequencyComponent>())
                            {
                                c.OnFrequencyStay?.Invoke(Index, true, Frequency, ID);
                            }
                        }
                    }
                    else
                    {
                        foreach (FrequencyComponent c in Scene.Tracker.GetComponents<FrequencyComponent>())
                        {
                            c.OnFrequencyStay?.Invoke(Index, false, Frequency, ID);
                        }
                    }
                }
                public void DrawAll()
                {
                    if (Selected && !Dummy)
                    {
                        Draw.Rect(Position.X - SelectPadding, Position.Y - SelectPadding, WIDTH + SelectPadding * 2, HEIGHT + SelectPadding * 2, Color.Yellow);
                        DrawMarkerBox(SelectPadding * 2, Color.Yellow);
                    }
                    else
                    {
                        Draw.Rect(Position.X - SelectPadding, Position.Y - SelectPadding, WIDTH + SelectPadding * 2, HEIGHT + SelectPadding * 2, Dummy ? Color.Red : Color.White);
                    }
                    Draw.Rect(Position.X, Position.Y, WIDTH, HEIGHT, Color.Black);
                    DrawMarkerBox(4, Color.Black);
                    DrawDial();
                    if (!Dummy)
                    {
                        DrawMarker();
                    }
                }
                public void DrawDial()
                {
                    float xOffset = Position.X + WIDTH;
                    float middle = Position.Y + HEIGHT / 2;
                    float alpha = Dummy ? DummyAlpha : SelectedAlpha;
                    for (float i = FrequencyData.Min; i <= FrequencyData.Max; i += ChannelSize)
                    {
                        float offset = WIDTH / 2 - 4;
                        float yoffset = middle - (i - Frequency) * 6;

                        bool big = true;
                        float scale = TextScale * 0.8f;
                        float height = ActiveFont.BaseSize * (scale / 2);
                        for (int j = 0; j < SegmentsPerChannel; j++)
                        {
                            if (i >= FrequencyData.Max && !big) break;
                            float textAlpha = GetAlphaAt(yoffset, HEIGHT) * alpha;
                            if (textAlpha > 0)
                            {
                                Color color = big ? Color.White : Color.Gray;
                                Draw.Line(xOffset - offset, yoffset, Position.X + WIDTH, yoffset, color * textAlpha, 6);
                                if (big)
                                {
                                    string hz = i.ToString("0");
                                    ActiveFont.Draw(
                                        hz,
                                        new Vector2(Position.X + 2, yoffset - height),
                                        Vector2.Zero,
                                        Vector2.One * scale,
                                        Color.White * textAlpha);
                                }
                            }
                            yoffset -= ChannelSize / SegmentsPerChannel * 6;
                            offset = WIDTH / 4 - 2;
                            big = false;
                        }
                    }
                }
                public float GetTextArea(string text)
                {
                    return ActiveFont.Measure(text).X * TextScale;
                }
                public void DrawMarkerBox(float padding, Color color)
                {
                    float toX = Position.X + WIDTH * 0.2f;
                    float width = WIDTH * 0.6f;
                    float height = ActiveFont.BaseSize * (TextScale / 2);
                    Draw.Rect(toX - padding, Position.Y - height - padding, width + padding * 2, height + padding * 2, color);
                }
                public void DrawMarker()
                {
                    float height = ActiveFont.BaseSize * (TextScale / 2);
                    string text = Frequency.ToString("0");
                    Vector2 position = Position + new Vector2(WIDTH / 2 - GetTextArea(text) / 2, -height);
                    ActiveFont.Draw(text, position, Vector2.Zero, Vector2.One * TextScale, Color.White);
                }
                public float GetAlphaAt(float y, float height)
                {
                    float posY = Position.Y;
                    float fadeHeight = height / 3;
                    if (y < posY || y > posY + height) return 0;
                    else if (y < posY + fadeHeight) return MathHelper.Distance(y, posY) / fadeHeight;
                    else if (y > posY + height - fadeHeight) return MathHelper.Distance(y, posY + height) / fadeHeight;
                    else return 1;
                }
                public void SetSelected(int currentIndex)
                {
                    Selected = Index == currentIndex;
                }
                public override void Removed(Scene scene)
                {
                    base.Removed(scene);
                    Finished = true;
                }
            }
            private bool ended;
            private osc[] Oscillators;
            public float SelectTimer;
            public float SelectDelay = 0.3f;
            private int previousIndex;
            public int CurrentIndex
            {
                get => currentIndex;
                set
                {
                    currentIndex = value;
                    if (Oscillators != null)
                    {
                        foreach (osc o in Oscillators)
                        {
                            o.SetSelected(currentIndex);
                        }
                    }
                }
            }
            private int currentIndex;
            public int DisplaysUnlocked
            {
                get => FrequencyData.GetUnlocked(Scene);
                set => FrequencyData.SetUnlocked(Scene, value);
            }
            private VirtualRenderTarget target;
            public float Alpha;
            public UI(params FlagList[] flags) : base()
            {
                Collider = new Hitbox(100, 100);
                Tag |= TagsExt.SubHUD;
                osc[] oscillators = new osc[4];
                float space = 16;
                float y = 16;
                float right = 0;
                for (int i = 0; i < 4; i++)
                {
                    Vector2 position = Vector2.UnitX * space * 2.5f;
                    position.X += i / 4f * (space * 4);
                    position.Y = y;
                    right = position.X;
                    position *= 6;
                    oscillators[i] = new(this, position, i);
                    oscillators[i].Flag = flags != null && flags.Length > 0 && flags.Length > i ? flags[i] : default;
                }
                Oscillators = oscillators;

                target = VirtualContent.CreateRenderTarget("fork-amp-ui", (int)(right + space) * 6, (int)(y * 2 + osc.WorldHeight) * 6);
                Add(new BeforeRenderHook(() =>
                {
                    if (target != null && !target.IsDisposed)
                    {
                        target.SetAsTarget(Color.Blue);
                        Draw.SpriteBatch.Begin();
                        foreach (osc o in Oscillators)
                        {
                            o.DrawAll();
                        }
                        if (GameplayRenderer.RenderDebug || Engine.Commands.Open)
                        {
                            float y = 1080 - ActiveFont.LineHeight * 4;
                            ActiveFont.DrawOutline("Current Index: " + CurrentIndex, Vector2.UnitY * y, Vector2.Zero, Vector2.One, Color.White, 5, Color.Black);
                            y += ActiveFont.LineHeight;
                            ActiveFont.DrawOutline("Displays Unlocked: " + DisplaysUnlocked, Vector2.UnitY * y, Vector2.Zero, Vector2.One, Color.White, 5, Color.Black);
                            y += ActiveFont.LineHeight;
                            ActiveFont.DrawOutline("Select Timer: " + SelectTimer, Vector2.UnitY * y, Vector2.Zero, Vector2.One, Color.White, 5, Color.Black);
                            /*
                                                        if (Scene.Tracker.GetEntity<ForkAmpSound>() is ForkAmpSound sound)
                                                        {
                                                            sound.DrawVolumesAndDucks(new Vector2(1920, 1080));
                                                        }*/
                        }
                        Draw.SpriteBatch.End();
                    }
                }));
            }
            public override void Update()
            {
                base.Update();
                DisplaysUnlocked = Oscillators?.Select(item => !item.Dummy).Count() ?? 0;
                if (DisplaysUnlocked == 0)
                {
                    if (CurrentIndex > 0)
                    {
                        CurrentIndex = 0;
                    }
                    return;
                }
                if (SelectTimer <= 0)
                {
                    if (Input.MoveX != 0 && Input.MoveX.Value != 0)
                    {
                        int nextIndex = Calc.Clamp(CurrentIndex + Input.MoveX, 0, Math.Max(DisplaysUnlocked - 1, 0));
                        if (CurrentIndex != nextIndex)
                        {
                            CurrentIndex = nextIndex;
                            SelectTimer = 0.16f;
                            foreach (FrequencyComponent c in Scene.Tracker.GetComponents<FrequencyComponent>())
                            {
                                c.OnSelectedChanged?.Invoke(currentIndex, ID);
                            }
                        }
                    }
                }
                else
                {
                    SelectTimer -= Engine.DeltaTime;
                }
            }
            public override void Render()
            {
                base.Render();
                if (target != null && !target.IsDisposed)
                {
                    Draw.SpriteBatch.Draw(target, Position, Color.White * Alpha);
                }
            }
            public static bool FromLeft;
            private Tween tween;
            public bool TransitioningIn;
            public bool TransitioningOut;
            public IEnumerator OnBegin()
            {
                Scene.Add(Oscillators);
                foreach (osc osc in Oscillators)
                {
                    osc.Dummy = true;
                }
                Alpha = 0;
                foreach (FrequencyComponent c in Scene.Tracker.GetComponents<FrequencyComponent>())
                {
                    c.OnUIStart?.Invoke(ID);
                }
                float xFrom = FromLeft ? -target.Width : 1920;
                float xTarget = FromLeft ? 0 : 1920 - target.Width;
                for (float i = 0; i < 1; i += Engine.DeltaTime / 1.3f)
                {
                    float ease = Ease.SineInOut(i);
                    Position.X = Calc.LerpClamp(xFrom, xTarget, ease);
                    Alpha = ease;
                    yield return null;
                }
                Position.X = xTarget;
                Alpha = 1;

                foreach (osc osc in Oscillators)
                {
                    osc.Dummy = !osc.Flag;
                }
                if (!Oscillators[0].Dummy)
                {
                    Oscillators[0].SetSelected(0);
                }
                Active = true;
                ForkAmpSound.Start();
            }
            public void OnEndInstant()
            {
                if (ForkAmpSound.GlobalPlaying)
                {
                    foreach (FrequencyComponent c in Scene.Tracker.GetComponents<FrequencyComponent>())
                    {
                        c.OnUIEnd?.Invoke(true, ID);
                    }
                }
                ForkAmpSound.Stop(true);
                Oscillators?.RemoveSelves();
                target?.Dispose();
                target = null;
                Alpha = 0;
                RemoveSelf();
            }
            public IEnumerator OnEnd()
            {
                foreach (osc osc in Oscillators)
                {
                    osc.Dummy = true;
                }
                ForkAmpSound.Stop(true);
                foreach (FrequencyComponent c in Scene.Tracker.GetComponents<FrequencyComponent>())
                {
                    c.OnUIEnd?.Invoke(false, ID);
                }
                float xFrom = Position.X;
                float xTarget = FromLeft ? -target.Width : 1920;
                for (float i = 0; i < 1; i += Engine.DeltaTime / 1.3f)
                {
                    float ease = Ease.SineInOut(i);
                    Position.X = Calc.LerpClamp(xFrom, xTarget, ease);
                    Alpha = 1 - ease;
                    yield return null;
                }
                Oscillators.RemoveSelves();
                target?.Dispose();
                target = null;
                Alpha = 0;
            }
        }
        public bool Finished;
        public UI ui;
        public static bool UIActive;
        public ForkAmpUI(params FlagList[] flags) : base()
        {
            ui = new UI(flags);
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(ui);
        }
        public override void OnBegin(Level level)
        {
            if (level.GetPlayer() is Player player)
            {
                player.StateMachine.State = Player.StDummy;
                Add(new Coroutine(Routine(player)));
            }
        }
        public override void OnEnd(Level level)
        {
            ui?.OnEndInstant();
            Finished = true;
            level.EnableMovement();
        }
        public IEnumerator Routine(Player player)
        {
            UIActive = true;
            yield return ui.OnBegin();
            while (!Input.MenuCancel)
            {
                yield return null;
            }
            Input.Dash.ConsumePress();
            yield return ui.OnEnd();
            UIActive = false;
            EndCutscene(Level, true);
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            UIActive = false;
        }
        [OnUnload]
        public static void Unload()
        {
            UIActive = false;
        }
        private IEnumerator CameraScroll(Vector2 amount, float time)
        {
            if (Scene is not Level level) yield break;
            Vector2 orig = level.Camera.Position;
            for (float i = 0; i < 1; i += Engine.DeltaTime / time)
            {
                level.Camera.Position = orig + amount * Ease.CubeOut(i);
                yield return null;
            }
            level.Camera.Position = orig + amount;
        }
    }
}