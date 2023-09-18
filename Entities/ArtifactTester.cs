using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using MonoMod.Utils;
using Celeste.Mod.PuzzleIslandHelper.ModIntegration;

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
        private Level level;
        private float Spacing = 8;
        private float SpaceProgress;
        private int Lines;
        private int Rate = 15;
        private Effect Shader;
        private int GlowBuffer = 4;
        private int glowBuf = 4;
        private float MinGlow = 0.2f;
        private float MaxGlow = 0.5f;
        private bool[] flags = new bool[8];

        private Sprite[] Boxes = new Sprite[8];
        private bool ShouldGlitch;
        private VirtualRenderTarget Target;
        private VirtualRenderTarget Mask;
        private int CodesUsed;
        public ArtifactTester(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            // TODO: read properties from data
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
                Boxes[i].Add("glitch","boxGlitch",0.05f,"shine");
                Boxes[i].Position.Y = 26;
                Boxes[i].Position.X = 7 + (14 * i);
                Boxes[i].Play("empty");
                Boxes[i].Visible = false;
            }
            Add(Boxes);

            Add(Glow = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/artifactTester/"));
            Glow.Position -= new Vector2(24, 24);
            Glow.AddLoop("idle", "glow", 1f);
            Glow.Color = Color.Green * 0.5f;

            Glow.Play("idle");
            Glow.Visible = false;
            Target = VirtualContent.CreateRenderTarget("MonitorEffects", 320, 180);
            Mask = VirtualContent.CreateRenderTarget("MonitorMask", 320, 180);
            Add(new BeforeRenderHook(BeforeRender));
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
                float addition = ShouldGlitch? 0.4f : 0;
                Glow.Color = Color.Lerp(color1, color2, Calc.Random.Range(0f, 1f)) * Calc.Random.Range(0.2f+addition, 0.5f+addition);
            }
        }
        private Effect? TryGetEffect(string id)
        {
            id = id.Replace('\\', '/');

            if (Everest.Content.TryGet($"Effects/PuzzleIslandHelper/Shaders/{id}.cso", out var effectAsset, true))
            {
                try
                {
                    Effect effect = new Effect(Engine.Graphics.GraphicsDevice, effectAsset.Data);
                    return effect;
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, "PuzzleIslandHelper", "Failed to load the shader " + id);
                    Logger.Log(LogLevel.Error, "PuzzleIslandHelper", "Exception: \n" + ex.ToString());
                }
            }

            return null;
        }


        private void BeforeRender()
        {
            EasyRendering.SetRenderMask(Mask, Monitor.Render, level);
            EasyRendering.DrawToObject(Target, DrawContent, level);
            if (ShouldGlitch)
            {
                EasyRendering.AddGlitch(Target, Calc.Random.Range(0.1f, 1f), Calc.Random.Range(1, 100f));
            }

            EasyRendering.MaskToObject(Target, Mask);
            /*            if (ShouldGlitch)
                        {*/
            // }
            Shader.ApplyStandardParameters(level);
        }
        public override void Render()
        {
            base.Render();
            Draw.SpriteBatch.End();

            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone,
                Shader);

            Draw.SpriteBatch.Draw(Target, level.Camera.Position, Color.White);
            Draw.SpriteBatch.End();
            GameplayRenderer.Begin();
            for (int i = 0; i < 8; i++)
            {
                Boxes[i].Render();
            }
            Glow.Render();
        }
        private void DrawContent()
        {
            //Draw.Rect(Collider, Color.Black);
            Draw.Rect(level.Bounds, Color.Black);
            for (int i = 0; i < Lines; i++)
            {
                Vector2 offsetY = new Vector2(0, (Spacing * i) + SpaceProgress);
                Draw.Line(BottomLeft - offsetY, BottomRight - offsetY, Color.Green);
            }

        }
        public override void Added(Scene scene)
        {
            base.Added(scene);

        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            level = scene as Level;
            Shader = TryGetEffect("static");
            Add(new Coroutine(WaitThenGlitch()));
            DebugFlags(); //TODO: remove
            player = Scene.Tracker.GetEntity<Player>();
            CodesUsed = CheckFlags();
            for (int i = 0; i < 8; i++)
            {
                if (flags[i])
                {
                    Boxes[i].Play("filled");
                }
            }

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
            int amount = 0;
            bool[] copy = new bool[8];
            flags.CopyTo(copy, 0);
            for (int i = 0; i < 8; i++)
            {
                flags[i] = level.Session.GetFlag("codeDoor" + (i + 1));
                if (flags[i])
                {
                    amount++;
                    if (!copy[i])
                    {
                        Audio.Play("event:/game/09_core/frontdoor_heartfill", Position);
                        Boxes[i].Play("glitch");
                        //Boxes[i].Position.X--;
                    }
                }
            }
            return amount;
        }

        public override void Update()
        {
            base.Update();
            SpaceProgress += Engine.DeltaTime * Rate;
            SpaceProgress %= Spacing;
            HandleGlow();
            CodesUsed = CheckFlags();
            if (CodesUsed >= 8)
            {
                level.Session.SetFlag("codeDoorAll");
            }
        }

    }
    public static class Ext
    {
        public static Effect ApplyStandardParameters(this Effect effect, Level level)
        {
            var parameters = effect.Parameters;
            Matrix? camera = level.Camera.Matrix;
            parameters["DeltaTime"]?.SetValue(Engine.DeltaTime);
            parameters["Time"]?.SetValue(Engine.Scene.TimeActive);
            parameters["CamPos"]?.SetValue(level.Camera.Position);
            parameters["Dimensions"]?.SetValue(new Vector2(320, 180) * (GameplayBuffers.Gameplay.Width / 320));
            parameters["ColdCoreMode"]?.SetValue(level.CoreMode == Session.CoreModes.Cold);

            Viewport viewport = Engine.Graphics.GraphicsDevice.Viewport;

            Matrix projection = Matrix.CreateOrthographicOffCenter(0, viewport.Width, viewport.Height, 0, 0, 1);
            // from communal helper
            Matrix halfPixelOffset = Matrix.Identity;

            parameters["TransformMatrix"]?.SetValue(halfPixelOffset * projection);

            parameters["ViewMatrix"]?.SetValue(camera ?? Matrix.Identity);

            return effect;
        }
    }
}
