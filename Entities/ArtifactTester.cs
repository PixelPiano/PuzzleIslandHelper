using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using MonoMod.Utils;
using Celeste.Mod.PuzzleIslandHelper.ModIntegration;
using System.Collections.Generic;
using ExtendedVariants.Variants;

// PuzzleIslandHelper.LabDoor
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/ArtifactTester")]
    [Tracked]
    public class ArtifactTester : Entity
    {
        private Sprite Border;
        private Sprite Monitor;
        private Sprite Glow;
        private Player player;
        private bool Stop;
        private Level level;
        private float Spacing = 8;
        private float SpaceProgress;
        private int Lines;
        private int Rate = 15;
        private int GlowBuffer = 4;
        private int glowBuf = 4;
        private bool[] flags = new bool[8];
        private Sprite Prompt;
        private bool GlitchPrompt;
        private float PromptOpacity = 1;
        private Sprite[] Boxes = new Sprite[8];
        private bool ShouldGlitch;
        private VirtualRenderTarget Target;
        private VirtualRenderTarget Mask;
        private VirtualRenderTarget ScreenContent;
        private int CodesUsed;
        private List<string> Anims = new();
        public static bool[] Verified = new bool[8];
        public static bool DontChange;
        public bool Completed
        {
            get
            {
                int loops = 0;
                for (int i = 0; i < 8; i++)
                {
                    if (Verified[i])
                    {
                        loops++;
                    }
                }
                return loops >= 8 || PianoModule.Session.HasArtifact;
            }
        }
        public ArtifactTester(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            // TODO: read properties from data
            Depth = 1;
            CreateSprites();
            Target = VirtualContent.CreateRenderTarget("MonitorEffects", 320, 180);
            Mask = VirtualContent.CreateRenderTarget("MonitorMask", 320, 180);
            ScreenContent = VirtualContent.CreateRenderTarget("ScreenContent", 320, 180);
            Add(new BeforeRenderHook(BeforeRender));
        }
        private void ChooseRandomPrompt()
        {
            if (Stop || Prompt is null)
            {
                return;
            }
            if (DontChange && !string.IsNullOrEmpty(PianoModule.Session.CurrentPrompt))
            {
                Prompt.Play(PianoModule.Session.CurrentPrompt);
                return;
            }
            Random r = new Random((int)level.TimeActive * 10);
            int index = 0;
            int loops = 0;
            while (true)
            {
                if (loops > 10)
                {
                    break;
                }
                for (int i = 0; i < 8; i++)
                {
                    level.Session.SetFlag("activationCode" + (i + 1), false);
                }
                index = r.Range(0, 8);
                if (Verified[index])
                {
                    loops++;
                    continue;
                }
                level.Session.SetFlag("activationCode" + (index + 1), true);
                break;
            }
            if (loops > 10)
            {
                Stop = true;
                if (Prompt is not null)
                {
                    Prompt.Stop();
                    Remove(Prompt);
                }
                return;
            }
            string animID = GetAnim(index + 1);
            Prompt.Play(animID);
            PianoModule.Session.CurrentPrompt = animID;
            DontChange = true;
        }
        private void CreateSprites()
        {
            Add(Border = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/artifactTester/"));
            Border.AddLoop("idle", "monitorBorder", 0.1f);
            Border.Play("idle");
            Add(Monitor = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/artifactTester/"));
            Monitor.AddLoop("idle", "monitorScreen", 0.1f);
            Monitor.AddLoop("white", "monitorWhite", 0.1f);
            Monitor.Play("white");

            Monitor.Visible = false;
            Lines = (int)(Monitor.Height / Spacing) + 2;
            Collider = new Hitbox(Border.Width, Border.Height);
            for (int i = 0; i < 8; i++)
            {
                Boxes[i] = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/artifactTester/");
                Boxes[i].AddLoop("empty", "boxEmpty", 0.1f);
                //Boxes[i].AddLoop("filled", "boxFill", 0.1f, 8);
                Boxes[i].AddLoop("closed", "boxClose", 0.1f, 3);
                Boxes[i].Add("close", "boxClose", 0.07f, "closed");
                Boxes[i].Add("fill", "boxFill", 0.05f, "close");
                Boxes[i].Add("shine", "boxShine", 0.07f, "fill");
                Boxes[i].Add("glitch", "boxGlitch", 0.05f, "shine");
                Boxes[i].Position.Y = 13;
                Boxes[i].Position.X = 7 + (14 * i);
                Boxes[i].Play("empty");
                Boxes[i].Visible = false;
            }
            Add(Boxes);

            Add(Glow = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/artifactTester/"));
            Glow.AddLoop("idle", "glow", 1f);
            Glow.Color = Color.Green * 0.5f;

            Glow.Play("idle");
            Glow.Visible = false;

            Add(Prompt = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/artifactTester/"));
            Prompt.AddLoop("dirt", "images/dirt", 0.1f);
            Prompt.AddLoop("water", "images/water", 0.1f);
            Prompt.AddLoop("spikeBlock", "images/spikeBlock", 0.1f);
            Prompt.AddLoop("glass", "images/glass", 0.1f);
            Prompt.AddLoop("layer", "images/layer", 0.1f);
            Prompt.AddLoop("spinner", "images/spinner", 0.1f);
            Prompt.AddLoop("stacking", "images/stacking", 0.1f);
            Prompt.AddLoop("bracket", "images/bracket", 0.1f);
            Prompt.Position.Y = 18;
            Prompt.Visible = false;
        }
        private IEnumerator Intermission()
        {
            yield return 2f;
            GlitchPrompt = true;
            for (int i = 0; i < 3; i++)
            {
                PromptOpacity = 0;
                yield return 0.1f;
                PromptOpacity = 1;
                yield return 0.1f;
            }
            DontChange = false;
            ChooseRandomPrompt();
            yield return 0.3f;
            GlitchPrompt = false;
            yield return null;
        }
        private void HandleGlow()
        {
            glowBuf--;
            if (glowBuf <= 0)
            {
                GlowBuffer = Calc.Random.Range(1, 5);
                glowBuf = GlowBuffer;
                Color color1 = Color.Green;
                Color color2 = Color.Lerp(Color.LightGreen, Color.DarkGreen, Calc.Random.Range(0f, 1f));
                float addition = ShouldGlitch ? 0.2f : 0;
                Glow.Color = Color.Lerp(color1, color2, Calc.Random.Range(0f, 1f)) * Calc.Random.Range(0.2f + addition, 0.5f + addition);
            }
        }

        private void BeforeRender()
        {
            EasyRendering.SetRenderMask(Mask, Monitor.Render, level);
            EasyRendering.DrawToObject(Target, DrawContent, level);
            EasyRendering.DrawToObject(ScreenContent, DrawScreenContent, level, true);
            if (ShouldGlitch)
            {
                EasyRendering.AddGlitch(Target, Calc.Random.Range(0.1f, 1f), Calc.Random.Range(1, 100f));
            }
            if (GlitchPrompt)
            {
                EasyRendering.AddGlitch(ScreenContent);
            }
            EasyRendering.MaskToObject(Target, Mask);
            EasyRendering.MaskToObject(ScreenContent, Mask);
            ShaderFX.Static.ApplyStandardParameters(level);
        }
     
        public override void Render()
        {
            base.Render();
            Draw.SpriteBatch.End();

            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone,
                ShaderFX.Static);

            Draw.SpriteBatch.Draw(Target, level.Camera.Position, Color.White);
            Draw.SpriteBatch.End();
            GameplayRenderer.Begin();
            Draw.SpriteBatch.Draw(ScreenContent, level.Camera.Position, Color.White);
            Glow.Render();
        }
          private void DrawScreenContent()
        {
            for (int i = 0; i < 8; i++)
            {
                Boxes[i].Render();
            }
            if (Prompt is not null && !Completed)
            {
                Prompt.Render();
            }
        }
        private void DrawContent()
        {
            Draw.Rect(level.Bounds, Color.Black);
            for (int i = 0; i < Lines; i++)
            {
                Vector2 offsetY = new Vector2(0, (Spacing * i) + SpaceProgress);
                Draw.Line(BottomLeft - offsetY, BottomRight - offsetY, Color.Green);
            }

        }
        private string GetAnim(int i)
        {
            return i switch
            {
                1 => "dirt",
                2 => "water",
                3 => "spikeBlock",
                4 => "stacking",
                5 => "spinner",
                6 => "glass",
                7 => "layer",
                8 => "bracket",
                _ => null
            };
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
            for (int i = 0; i < 8; i++)
            {
                if (Verified[i] || PianoModule.Session.HasArtifact)
                {
                    Boxes[i].Play("closed");
                }
            }
            if (Completed)
            {
                level.Session.SetFlag("doorCodeAll", true);
            }
            ChooseRandomPrompt();
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Add(new Coroutine(WaitThenGlitch()));
            player = Scene.Tracker.GetEntity<Player>();
            CodesUsed = CheckFlags();


        }
        private IEnumerator WaitThenGlitch()
        {
            while (true)
            {
                ShouldGlitch = false;
                yield return Calc.Random.Range(10f, 30f);
                ShouldGlitch = true;
                yield return Calc.Random.Range(0.1f, 2f);
            }
        }
        private void DebugFlags()
        {
            for (int i = 0; i < 8; i++)
            {

                level.Session.SetFlag("codeDoor" + (i + 1), false);
            }
        }
        private int CheckFlags()
        {
            if (Completed)
            {
                level.Session.SetFlag("doorCodeAll", true);
                return 8;
            }
            else
            {
                level.Session.SetFlag("doorCodeAll", false);
            }
            int amount = 0;
            bool[] copy = new bool[8];
            flags.CopyTo(copy, 0);
            for (int i = 0; i < 8; i++)
            {
                if (Verified[i])
                {
                    amount++;
                    continue;
                }
                flags[i] = level.Session.GetFlag("codeDoor" + (i + 1));
                if (flags[i])
                {
                    //amount++;
                    if (!copy[i])
                    {
                        Audio.Play("event:/game/09_core/frontdoor_heartfill", Position);
                        Boxes[i].Play("glitch");
                        Verified[i] = true;
                        Add(new Coroutine(Intermission()));
                    }
                }
            }
            return amount;
        }

        public override void Update()
        {
            base.Update();
            if (Prompt is not null)
            {
                Prompt.Color = Color.White * PromptOpacity;
            }
            SpaceProgress += Engine.DeltaTime * Rate;
            SpaceProgress %= Spacing;
            HandleGlow();
            CodesUsed = CheckFlags();
        }
    }
}
