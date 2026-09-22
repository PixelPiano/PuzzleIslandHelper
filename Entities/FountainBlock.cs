using Celeste.Mod.CommunalHelper;
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/FountainBlock")]
    [Tracked]
    public class FountainBlock : Entity
    {
        public class CodeRule
        {
            public string Orig;
            public int Digit;
            public string Operator = "";
            public char[] Ends;
            public CodeRule(int digit, string op, params char[] ends)
            {
                Orig = digit.ToString();
                Orig += " " + op + " [";
                foreach (char c in ends)
                {
                    Orig += c + ", ";
                }
                Orig = Orig.TrimEnd(',') + "]";
                Digit = digit;
                Operator = op;
                Ends = ends;
            }
            public string Log()
            {
                string s = "";
                s += "Digit: " + Digit;
                s += "\nOperator: " + Operator;
                foreach (char c in Ends)
                {
                    s += "\n{ char: " + c;
                    bool isNumber = char.IsNumber(c);
                    bool isLetter = char.IsLetter(c);
                    s += "\n\t isNumber: " + isNumber;
                    s += "\n\t isLetter: " + isLetter;
                    if (isLetter)
                    {
                        s += "\n\t LetterNum: " + (c - 'A');
                    }
                    s += "\n}";
                }
                return s;
            }
            public string ValidLog = "";
            public bool RuleMet(string combo)
            {
                void vLog(string tag, object value = null)
                {
                    if (value == null)
                    {
                        ValidLog += "\n\t" + tag;
                    }
                    else
                    {
                        ValidLog += "\n\t" + tag + ": " + value.ToString();
                    }
                }
                int count = combo.Count(c2 => char.GetNumericValue(c2) == Digit);
                ValidLog = "Validity Log:\n{";
                vLog("count of digit", count);
                if (count == 1)
                {
                    int index = combo.IndexOf(char.Parse(Digit.ToString()));
                    vLog("index of digit", index);
                    switch (Operator)
                    {
                        case "<3":
                            vLog("operator", "<3");
                            foreach (char c in Ends)
                            {
                                vLog("checking char", c);
                                if (char.IsLetter(c))
                                {
                                    vLog("char is letter", c - 'A');
                                    int letterIndex = c - 'A';
                                    if (letterIndex != index + 1 && letterIndex != index - 1)
                                    {
                                        vLog("failed");
                                        return false;
                                    }
                                    vLog("passed");
                                }
                                else
                                {
                                    int nextIndex = combo.IndexOf(c);
                                    if (nextIndex >= 0)
                                    {
                                        vLog("charIndex is", nextIndex);
                                    }
                                    if (nextIndex != index + 1 && nextIndex != index - 1)
                                    {
                                        vLog("failed");
                                        return false;
                                    }
                                    vLog("passed");
                                }
                            }
                            break;
                        case "</3":
                            vLog("operator", "</3");
                            foreach (char c in Ends)
                            {
                                vLog("checking char", c);
                                if (char.IsLetter(c))
                                {
                                    vLog("char is letter", c - 'A');
                                    int letterIndex = c - 'A';
                                    if (letterIndex == index + 1 || letterIndex == index - 1)
                                    {
                                        vLog("failed");
                                        return false;
                                    }
                                    vLog("passed");
                                }
                                else
                                {
                                    int nextIndex = combo.IndexOf(c);
                                    if (nextIndex >= 0)
                                    {
                                        vLog("charIndex is", nextIndex);
                                    }
                                    if (nextIndex == index + 1 || nextIndex == index - 1)
                                    {
                                        vLog("failed");
                                        return false;
                                    }
                                    vLog("passed");
                                }
                            }
                            break;
                        default:
                            return false;
                    }
                    return true;
                }
                return false;
            }
        }
        private List<CodeRule> rules = [];
        public const int GeneratorsRequired = 5;
        public class Screen : Entity
        {
            public int ScreenFrame;
            public bool Shaking;
            private Vector2 offset;
            public Sprite sprite;
            public bool FullyRevealed;
            public MTexture Texture => GFX.Game["objects/PuzzleIslandHelper/fountainBlock/screen0" + ScreenFrame];
            public Screen(Vector2 position, float width) : base(position)
            {
                sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/fountainBlock/");
                sprite.AddLoop("idle", "screenInfo", 0.6f, 0, 1, 2, 3);
                sprite.Add("intoLoad", "screenInfo", 0.2f, "load", 4, 5, 6, 7);
                sprite.AddLoop("load", "screenInfo", 0.1f, 8, 9, 10, 11);
                sprite.Add("outLoad", "screenInfo", 0.2f, "intoText", 12, 13, 14, 15);
                sprite.Add("intoText", "screenInfo", 0.2f, "textLoad", 17, 18, 19, 20);
                sprite.Add("textLoad", "screenInfo", 0.1f, "textIdle", 20, 21, 22, 23, 24, 25, 26);
                sprite.AddLoop("textIdle", "screenInfo", 0.1f, 26);
                sprite.Add("intoIdle", "screenInfoReverse", 0.1f);
                sprite.AddLoop("off", "screenOff", 0.1f);
                sprite.Visible = false;
                Add(sprite);
                Depth = 8999;
                offset = new Vector2(width / 2 - Texture.Width / 2, -Texture.Height);
                sprite.Position = offset;
                AddTag(Tags.TransitionUpdate);
            }
            public override void Render()
            {
                base.Render();
                if (ScreenFrame > 0)
                {
                    Vector2 shake = Shaking ? Calc.Random.ShakeVector().XComp() : Vector2.Zero;
                    if (FullyRevealed)
                    {
                        sprite.Render();
                    }
                    else
                    {
                        Draw.SpriteBatch.Draw(Texture.Texture.Texture_Safe, Position + offset + shake, Color.White);
                    }
                }
            }
        }
        public class Fountain : Entity
        {
            public string Input;
            public Sprite Sprite;
            public Fountain(Vector2 position, float width) : base(position)
            {
                Depth = 9000;
                Sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/fountainBlock/");
                Sprite.Add("idle", "fountainBroken", 0.1f);
                Add(Sprite);
                Sprite.Play("idle");
                Sprite.Position = new Vector2(width / 2 - Sprite.Width / 2, -Sprite.Height);
                Collider = new Hitbox(Sprite.Width, Sprite.Height, Sprite.X, Sprite.Y);
                AddTag(Tags.TransitionUpdate);
            }

        }
        public enum FountainStates
        {
            Inactive,
            Screen,
            Solved
        }
        public FountainStates State;
        public class Light : Entity
        {
            private readonly MTexture Texture = GFX.Game["objects/PuzzleIslandHelper/fountainBlock/light"];
            private readonly MTexture BackTexture = GFX.Game["objects/PuzzleIslandHelper/fountainBlock/lightBack"];
            public ParticleType GlassShards = new ParticleType()
            {
                SpeedMax = 20f,
                SpeedMin = 5f,
                Direction = -Vector2.UnitY.Angle(),
                DirectionRange = 15f.ToRad(),
                LifeMin = 1,
                LifeMax = 3,
                FadeMode = ParticleType.FadeModes.None,
                RotationMode = ParticleType.RotationModes.SameAsDirection
            };
            private int CrackFrame = -1;
            private Color TexColor;
            private float BackOpacity;
            public Vector2 offset;
            public bool Broken;

            public Light(Vector2 position, float width) : base(position)
            {
                offset = new Vector2(width / 2 - Texture.Width / 2, 0);
                Depth = -13001;
                Tween colorTween = Tween.Create(Tween.TweenMode.YoyoLooping, Ease.SineInOut, 4, true);
                colorTween.OnUpdate = (t) =>
                {
                    TexColor = Color.Lerp(Color.White, Color.Black, t.Eased / 4);
                    BackOpacity = t.Eased / 2;
                };
                Collider = new Hitbox(width, BackTexture.Height);
                Add(colorTween);
                AddTag(Tags.TransitionUpdate);
            }
            public void EmitGlassShards()
            {
                if (Scene is not Level level) return;
                level.Shake(0.3f);
                ParticleSystem system = level.ParticlesBG;
                for (int i = 0; i < 12; i++)
                {
                    Vector2 pos = TopCenter + Vector2.UnitX * Calc.Random.Range(-8, 8);
                    Vector2 accell = new Vector2((pos.X - TopCenter.X) / 8, 1);
                    GlassShards.Acceleration = accell * 10;
                    system.Emit(GlassShards, pos);
                }
            }
            public override void Render()
            {
                base.Render();
                if (!Broken)
                {
                    Draw.SpriteBatch.Draw(BackTexture.Texture.Texture_Safe, Position + offset, Color.White * BackOpacity);
                }
                if (CrackFrame >= 0 && CrackFrame < 6)
                {
                    MTexture crack = GFX.Game["objects/PuzzleIslandHelper/fountainBlock/lightCrack0" + CrackFrame];
                    Color c = CrackFrame == 5 ? Color.Gray : Color.White;
                    Draw.SpriteBatch.Draw(crack.Texture.Texture_Safe, Position + offset, c);
                }
                else
                {
                    Draw.SpriteBatch.Draw(Texture.Texture.Texture_Safe, Position + offset, TexColor);
                }

            }
            public IEnumerator CrackLight()
            {
                float between = 0.05f;
                CrackFrame = 0;
                yield return 0.8f;
                CrackFrame = 1;
                yield return between;
                CrackFrame = 2;
                yield return 1f;
                CrackFrame = 3;
                yield return 0.2f + between;
                CrackFrame = 4;
                yield return 1;
                Break(false);
                yield return null;
            }
            public void Break(bool instant)
            {
                Broken = true;
                CrackFrame = 5;
                if (!instant)
                {
                    //play sound
                    EmitGlassShards();
                }
            }
            public override void Removed(Scene scene)
            {
                base.Removed(scene);
            }
        }
        private Fountain fountain;
        private Screen screen;
        public Light light;
        public DashListener Listener;
        public DotX3 Talk;
        public string InputCode = "";
        public FountainBlock(EntityData data, Vector2 offset)
          : base(data.Position + offset)
        {
            Depth = -13000;
            Collider = new Hitbox(data.Width, data.Height);
            fountain = new Fountain(Position, Width);
            light = new Light(Position, Width);
            screen = new Screen(Position, Width);
            AddTag(Tags.TransitionUpdate);
        }
        [Command("reveal_fountain", "skips the fountain puzzle")]
        public static void RevealFountain()
        {
            if (Engine.Scene is Level level)
            {
                foreach (FountainBlock b in level.Tracker.GetEntities<FountainBlock>())
                {
                    b.EnableScreen(true);
                }
            }
        }
        public override void Update()
        {
            base.Update();
            string output = "Rules:";
            foreach (CodeRule rule in rules)
            {
                rule.RuleMet(InputCode);
                output += "\n" + "{Rule: " + rule.Orig + ", Valid: " + rule.ValidLog + "}";
            }
            output += "\nCode: " + InputCode;
            fountain.Input = output;
        }
        private class RevealScreenCutscene : CutsceneEntity
        {
            private Screen screen;
            private Player player;
            private FountainBlock block;
            private Action onEnd;
            public RevealScreenCutscene(Player player, Screen screen, FountainBlock block, Action onEnd) : base()
            {
                this.screen = screen;
                this.player = player;
                this.block = block;
                this.onEnd = onEnd;
            }
            public override void OnBegin(Level level)
            {
                player.StateMachine.State = Player.StDummy;
                Add(new Coroutine(RevealScreen(level)));
            }
            public IEnumerator RevealScreen(Level level)
            {
                screen.Shaking = true;
                for (int i = 0; i < 6; i++)
                {
                    yield return 0.65f;
                    block.ScreenState(i);
                    //todo: play rock emerging/breaking/impact sound
                }
                screen.Shaking = false;
                yield return 0.5f;
                yield return level.ZoomBack(1);
                EndCutscene(level);
            }
            public override void OnEnd(Level level)
            {
                screen.FullyRevealed = true;
                screen.sprite.Play("idle");
                player.StateMachine.State = Player.StNormal;
                level.Session.SetFlag("FountainScreen");
                screen.Shaking = false;
                block.ScreenState(5);
                onEnd?.Invoke();
            }
        }
        private class ScreenCutscene : CutsceneEntity
        {
            private Screen screen;
            private Player player;
            private Action onEnd;
            private int generators
            {
                get
                {
                    int value = 0;
                    foreach (var a in PianoModule.Session.MiniGenStates.Values)
                    {
                        if (a)
                        {
                            value++;
                        }
                    }
                    return value;
                }
            }
            public ScreenCutscene(Player player, Screen screen, Action onEnd) : base()
            {
                this.screen = screen;
                this.player = player;
                this.onEnd = onEnd;
            }
            public override void OnBegin(Level level)
            {
                player.StateMachine.State = Player.StDummy;
                Add(new Coroutine(Cutscene(level)));
            }
            public IEnumerator Cutscene(Level level)
            {
                screen.FullyRevealed = true;

                yield return level.ZoomTo(level.Marker("fountainCamera", true) + Vector2.UnitY * 16, 1.7f, 1);
                screen.sprite.Play("intoLoad");
                yield return 3;
                screen.sprite.Play("outLoad");
                while (screen.sprite.CurrentAnimationID != "textIdle")
                {
                    yield return null;
                }
                yield return 0.5f;
                yield return PianoUtils.TextboxSayClean(Calc.Clamp(GeneratorsRequired - generators, 0, GeneratorsRequired) + " generators remain.");
                yield return 0.3f;
                screen.sprite.OnFinish = (string s) =>
                {
                    if (s == "intoIdle")
                    {
                        screen.sprite.Play(generators >= GeneratorsRequired ? "off" : "idle");
                    }
                };
                screen.sprite.Play("intoIdle");
                yield return 1f;
                yield return level.ZoomBack(1);
                EndCutscene(level);
            }
            public override void OnEnd(Level level)
            {
                if (WasSkipped)
                {
                    screen.sprite.Play(generators >= GeneratorsRequired ? "off" : "idle");
                    level.ResetZoom();
                    screen.sprite.OnFinish = delegate { };
                }
                player.StateMachine.State = Player.StNormal;
                if (generators >= GeneratorsRequired)
                {
                    onEnd?.Invoke();
                }
            }
        }
        private class InspectCutscene : CutsceneEntity
        {
            private float lerp;
            private bool inControl;
            private bool cancelled;
            private MTexture texture;
            private VirtualRenderTarget Target;
            private bool rendered;
            public InspectCutscene() : base()
            {
                Tag |= TagsExt.SubHUD;
                Target = VirtualContent.CreateRenderTarget("FountainInspectTarget", Engine.Width, Engine.Height);
                Add(new BeforeRenderHook(() =>
                {
                    if (!rendered)
                    {
                        Target.SetAsTarget(Color.Black * 0.7f);
                        Draw.SpriteBatch.Begin();
                        texture.DrawCentered(new Vector2(Engine.Width, Engine.Height) / 2);
                        Draw.SpriteBatch.End();
                        rendered = true;
                    }
                }));
            }
            public override void Removed(Scene scene)
            {
                base.Removed(scene);
                Target?.Dispose();
            }
            public override void Added(Scene scene)
            {
                base.Added(scene);
                texture = GFX.Game["objects/PuzzleIslandHelper/fountainBlock/inspect"];
            }
            public override void Update()
            {
                base.Update();
                if (inControl)
                {
                    if (Input.MenuCancel)
                    {
                        inControl = false;
                        cancelled = true;
                    }
                }
            }
            public override void OnBegin(Level level)
            {
                level.DisableMovement();
                Add(new Coroutine(routine()));
            }
            private IEnumerator routine()
            {
                for (float i = 0; i < 1; i += Engine.DeltaTime)
                {
                    float eased = Ease.SineInOut(i);
                    lerp = eased;
                    yield return null;
                }
                inControl = true;
                while (!cancelled) yield return null;
                inControl = false;
                for (float i = 1; i > 0; i -= Engine.DeltaTime)
                {
                    float eased = Ease.SineInOut(i);
                    lerp = eased;
                    yield return null;
                }
                lerp = 0;
                EndCutscene(Level);
            }
            public override void Render()
            {
                base.Render();
                Draw.SpriteBatch.Draw(Target, Vector2.Zero, Color.White * lerp);
            }
            public override void OnEnd(Level level)
            {
                level.EnableMovement();
            }
        }
        private class OpenCutscene : CutsceneEntity
        {
            private FountainBlock block;
            private Player player;
            public OpenCutscene(Player player, FountainBlock block) : base()
            {
                this.block = block;
                this.player = player;
            }
            public override void OnBegin(Level level)
            {
                player.StateMachine.State = Player.StDummy;
                Add(new Coroutine(OpenPassage(level)));
            }
            public IEnumerator OpenPassage(Level level)
            {
                yield return CameraToFountain(level, block);
                yield return 0.6f;
                yield return block.light.CrackLight();
                block.Open(false);
                yield return 1f;
                EndCutscene(level);
            }
            public override void OnEnd(Level level)
            {
                player.StateMachine.State = Player.StNormal;
                block.Open(true);
                level.Session.SetFlag("OpenedFountain");
            }

        }
        private void OnCode()
        {
            EnableScreen(false);
        }
        public void Interact(Player player)
        {
            if (State == FountainStates.Screen)
            {
                Scene.Add(new ScreenCutscene(player, screen, OnComplete));
            }
            else if (State == FountainStates.Inactive)
            {
                Scene.Add(new InspectCutscene());
            }
        }
        public void OnComplete()
        {
            if (Scene.GetPlayer() is Player player)
            {
                Remove(Talk);
                Scene.Add(new OpenCutscene(player, this));
            }
        }

        private void EnableScreen(bool instant)
        {
            State = FountainStates.Screen;
            if (instant)
            {
                Talk.Enabled = true;
                ScreenState(5);
                screen.FullyRevealed = true;
                screen.sprite.Play("idle");
            }
            else
            {
                if (Scene is not Level level || level.GetPlayer() is not Player player) return;
                Talk.Enabled = false;
                level.Add(new RevealScreenCutscene(player, screen, this, () => Talk.Enabled = true));
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(fountain, screen, light);
            Talk = new DotX3(fountain.Collider, Interact) { PlayerMustBeFacing = false };
            Talk.DrawAt.X++;
            fountain.Add(Talk);
            Level level = scene as Level;
            if (level.Session.GetFlag("OpenedFountain"))
            {
                Open(true);
            }
            else if (level.Session.GetFlag("FountainScreen"))
            {
                EnableScreen(true);
            }
            else
            {
                State = FountainStates.Inactive;
                rules.Add(new CodeRule(0, "<3", 'B'));
                rules.Add(new CodeRule(1, "<3", '3'));
                rules.Add(new CodeRule(2, "</3", '3'));
                rules.Add(new CodeRule(5, "</3", '6'));
                rules.Add(new CodeRule(4, "</3", '5', '7'));
                rules.Add(new CodeRule(3, "</3", 'B', 'G'));
                rules.Add(new CodeRule(5, "<3", 'D', 'F'));
                Add(Listener = new DashListener(v =>
                {
                    if (State == FountainStates.Inactive)
                    {
                        v = v.Round();
                        char c = v == -Vector2.UnitY ? '0'
                            : v == -Vector2.One ? '1'
                            : v == -Vector2.UnitX ? '2'
                            : v == new Vector2(-1, 1) ? '3'
                            : v == Vector2.UnitY ? '4'
                            : v == Vector2.One ? '5'
                            : v == Vector2.UnitX ? '6' : '7';
                        if (InputCode.Length < 8) InputCode += c;
                        else InputCode = InputCode[1..] + c;
                        if (InputCode.Length == 8)
                        {
                            foreach (CodeRule rule in rules)
                            {
                                if (!rule.RuleMet(InputCode)) return;
                            }
                            OnCode();
                        }
                    }
                }));
            }
        }
        public void ScreenState(int frame)
        {
            screen.ScreenFrame = frame;
        }
        public void Open(bool instant)
        {
            light.Break(instant);
            SceneAs<Level>().Session.SetFlag("OpenedFountain");
            ScreenState(5);
            State = FountainStates.Solved;
            Talk.Enabled = false;
        }
        public static IEnumerator CameraToFountain(Level level, FountainBlock block)
        {
            Camera camera = level.Camera;
            Vector2 target = level.Marker("fountainCamera") - new Vector2(160, 90);
            for (float i = 0; i < 1; i += Engine.DeltaTime / 1.5f)
            {
                camera.Approach(target, i);
                yield return null;
            }
        }
    }
}