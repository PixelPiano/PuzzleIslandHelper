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
using static Celeste.Mod.PuzzleIslandHelper.Entities.FrequencyEntities.FrequencyData;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.FrequencyEntities
{
    [Tracked]
    public class ForkAmpUI : Entity
    {
        [Tracked]
        public class UI : Entity
        {
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
                public bool InControl;
                public float Frequency;
                public override void Update()
                {
                    base.Update();

                    float prevRate = FrequencyData.GetPrevRate(Scene, Index, ID);
                    int dir = !UISelected ? 0 : Input.MenuUp ? -1 : Input.MenuDown ? 1 : 0;
                    Player player = Scene.GetPlayer();
                    /*                    InControl = true;
                                        if (player.CollideFirst<FrequencyDecal>() is FrequencyDecal decal && decal.Active && decal.targetFrequencies[Index] >= 0)
                                        {
                                            InControl = false;
                                        }*/
                    if (InControl)
                    {
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
                                Frequency = Calc.Clamp(Calc.Snap(prevRate, jump) - lastDir * jump, FrequencyData.Min, FrequencyData.Max);
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
                                    Frequency = Calc.Clamp(prevRate - dir, FrequencyData.Min, FrequencyData.Max);
                                }
                            }
                            lastDir = dir;
                        }
                        FrequencyData.SetRate(Scene, Index, Frequency, ID);
                    }
                    else
                    {
                        Frequency = FrequencyData.GetRate(Scene, Index, ID);
                    }
                }
                public void DrawAll(float frequency)
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
                    DrawDial(frequency);
                    if (!Dummy)
                    {
                        DrawMarker(frequency);
                    }
                }
                public void DrawDial(float frequency)
                {
                    float xOffset = Position.X + WIDTH;
                    float middle = Position.Y + HEIGHT / 2;
                    float alpha = Dummy ? DummyAlpha : SelectedAlpha;
                    for (float i = FrequencyData.Min; i <= FrequencyData.Max; i += ChannelSize)
                    {
                        float offset = WIDTH / 2 - 4;
                        float yoffset = middle - (i - frequency) * 6;

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
                public void DrawMarker(float frequency)
                {
                    float height = ActiveFont.BaseSize * (TextScale / 2);
                    string text = frequency.ToString("0");
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
            public static bool FromLeft;
            private osc[] Oscillators;
            private VirtualRenderTarget target;
            public string ID;
            public float[] Frequencies = new float[4];
            public float SelectTimer;
            public float SelectDelay = 0.3f;
            public float Alpha;
            public int CurrentSet;
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
                Add(new BeforeRenderHook(BeforeRender));
            }
            public override void Update()
            {
                base.Update();
                Level level = Scene as Level;
                for (int i = 0; i < 4; i++)
                {
                    Frequencies[i] = FrequencyData.GetRate(Scene, i, ID);
                }
                for (int i = 0; i < 4; i++)
                {
                    Oscillators[i].InControl = true;
                }
                Player player = level.GetPlayer();
                foreach(FrequencyMod mod in level.Tracker.GetComponents<FrequencyMod>())
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (mod.OscillatorsAffected[i])
                        {
                            Oscillators[i].InControl = false;
                        }
                    }
                }
                bool prevDisabled = MInput.Disabled;
                MInput.Disabled = false;
                bool pressed = PianoModule.Settings.FrequencyMachineButtonBinding.Pressed;
                MInput.Disabled = prevDisabled;
                if (pressed)
                {
                    PianoModule.Settings.FrequencyMachineButtonBinding.ConsumePress();
                    if (player.Dead)
                    {
                        UISelected = false;
                        return;
                    }
                    if (UISelected)
                    {
                        UISelected = false;
                        if (player.StateMachine.State == Player.StDummy)
                        {
                            player.StateMachine.State = Player.StNormal;
                        }
                    }
                    else if (player.StateMachine == Player.StNormal)
                    {
                        UISelected = true;
                        player.StateMachine.State = Player.StDummy;
                    }
                }
                DisplaysUnlocked = Oscillators?.Select(item => !item.Dummy).Count() ?? 0;
                if (DisplaysUnlocked == 0)
                {
                    if (CurrentIndex > 0)
                    {
                        CurrentIndex = 0;
                    }
                    return;
                }
                if (UISelected)
                {
                    if (SelectTimer <= 0)
                    {
                        if (Input.MoveX != 0 && Input.MoveX.Value != 0)
                        {
                            int nextIndex = Calc.Clamp(CurrentIndex + Input.MoveX, 0, Math.Max(DisplaysUnlocked - 1, 0));
                            if (CurrentIndex != nextIndex)
                            {
                                CurrentIndex = nextIndex;
                                SelectTimer = 0.16f;
                            }
                        }
                    }
                    else
                    {
                        SelectTimer -= Engine.DeltaTime;
                    }
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
            public void BeforeRender()
            {
                if (target != null && !target.IsDisposed)
                {
                    target.SetAsTarget(UISelected ? Color.Blue : Color.Red);
                    Draw.SpriteBatch.Begin();
                    for (int i = 0; i < Oscillators.Length; i++)
                    {
                        Oscillators[i].DrawAll(Frequencies[i]);
                    }
                    Draw.SpriteBatch.End();
                }
            }
            public IEnumerator OnBegin()
            {
                ForkAmpSound.Stop(true);
                Scene.Add(Oscillators);
                foreach (osc osc in Oscillators)
                {
                    osc.Dummy = true;
                }
                Alpha = 0;
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
            public IEnumerator OnEnd()
            {
                foreach (osc osc in Oscillators)
                {
                    osc.Dummy = true;
                }
                ForkAmpSound.Stop(true);
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
            public void OnEndInstant()
            {
                ForkAmpSound.Stop(true);
                Oscillators?.RemoveSelves();
                target?.Dispose();
                target = null;
                Alpha = 0;
                RemoveSelf();
            }
        }
        public bool Finished;
        public UI ui;
        public static bool UIActive;
        public static bool UISelected;
        public static bool ForceOff;
        public static IEnumerator EndingSequence;
        public static Action OnEndCallback;
        public ForkAmpUI(params FlagList[] flags) : base()
        {
            ui = new UI(flags);
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(ui);
            OnBegin(scene as Level);
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            UIActive = false;
            UISelected = false;
            ui?.RemoveSelf();
            EndingSequence = null;
        }
        public void OnBegin(Level level)
        {
            if (level.GetPlayer() is Player player)
            {
                //player.StateMachine.State = Player.StDummy;
                UIActive = true;
                Add(new Coroutine(Routine(player)));
            }
        }
        public void OnEnd(Level level)
        {
            UIActive = false;
            UISelected = false;
            ui?.OnEndInstant();
            Finished = true;
            level.EnableMovement();
            OnEndCallback?.Invoke();
        }
        public IEnumerator Routine(Player player)
        {
            yield return ui.OnBegin();
            while (true)
            {
                if (UISelected)
                {
                    if (Input.MenuCancel || ForceOff)
                    {
                        break;
                    }
                }
                else if (ForceOff)
                {
                    break;
                }
                yield return null;
            }
            if (UISelected) Input.MenuCancel.ConsumePress();
            ForceOff = false;
            yield return ui.OnEnd();
            UIActive = false;
            if (EndingSequence != null)
            {
                yield return new SwapImmediately(EndingSequence);
            }
            EndingSequence = null;
            OnEnd(SceneAs<Level>());
        }
        [OnUnload]
        public static void Unload()
        {
            UIActive = false;
            UISelected = false;
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