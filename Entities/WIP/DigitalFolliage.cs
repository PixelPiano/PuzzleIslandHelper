using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;
namespace Celeste.Mod.PuzzleIslandHelper.Entities.WIP
{
    [CustomEntity("PuzzleIslandHelper/DigitalFolliage")]
    [Tracked]
    public class DigiFolliage : Entity
    {
        public List<FgFolliage> FGFolliage = new();
        public List<BgFolliage> BGFolliage = new();
        public string Flag;
        public DigiFolliage(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Tag |= Tags.TransitionUpdate;
            Collider = new Hitbox(data.Width, data.Height);
            Flag = data.Attr("flag");
            Fg = data.Bool("foreground");
            Depth = data.Int("depth", -1);
            Color color = data.HexColor("color");
            for (int i = 0; i < Width; i += 8)
            {
                for (int j = 0; j < Height; j += 8)
                {
                    FgFolliage tumor = new FgFolliage(new Vector2(i, j), Vector2.One * 2, color);
                    FGFolliage.Add(tumor);
                    BgFolliage bg = new BgFolliage(new Vector2(i, j), Vector2.One * 2, color);
                    BGFolliage.Add(bg);

                }
            }
            foreach (BgFolliage t in BGFolliage)
            {
                Add(t);
            }
            foreach (FgFolliage t in FGFolliage)
            {
                Add(t);
            }
        }
        public bool OnScreen;
        public bool CanRender;
        public bool Fg;
        public override void Update()
        {
            base.Update();
            if (Scene is Level level)
            {
                OnScreen = Left - 20 < level.Camera.X + 320 && Right + 20 > level.Camera.X && Top - 20 < level.Camera.Y + 180 && Bottom + 20 > level.Camera.Y;
                CanRender = OnScreen && (string.IsNullOrEmpty(Flag) || level.Session.GetFlag(Flag));
            }
        }
        public void RenderBackground()
        {
            if (CanRender)
            {
                foreach (BgFolliage t in BGFolliage)
                {
                    t.Render();
                }
            }
        }
        public override void Render()
        {
            if (CanRender)
            {
                if (Fg)
                {
                    foreach(BgFolliage t in BGFolliage)
                    {
                        t.Render();
                    }
                }
                foreach (FgFolliage t in FGFolliage)
                {
                    t.Render();
                }
            }
        }
        [Tracked]
        public class BgFolliageRenderer : Entity
        {
            public static float Sine;
            public static void Load()
            {
                Everest.Events.LevelLoader.OnLoadingThread += LevelLoader_OnLoadingThread;
            }
            public static void Unload()
            {
                Everest.Events.LevelLoader.OnLoadingThread -= LevelLoader_OnLoadingThread;
            }
            private static void LevelLoader_OnLoadingThread(Level level)
            {
                level.Add(new BgFolliageRenderer());
            }

            public BgFolliageRenderer() : base()
            {
                Depth = 1;
                Tag |= Tags.TransitionUpdate | Tags.Persistent | Tags.Global;
            }
            public override void Update()
            {
                base.Update();
                Sine = (float)(Math.Sin(Scene.TimeActive) + 1) / 2f;
            }
            public override void Render()
            {
                base.Render();
                foreach (DigiFolliage folliage in Scene.Tracker.GetEntities<DigiFolliage>())
                {
                    if(folliage.Fg) continue;
                    folliage.RenderBackground();
                }
            }
        }
        public class BgFolliage : Image
        {
            public Vector2 PositionRange;
            public float Alpha = 1;
            public float RotationRate;
            public BgFolliage(Vector2 position, Vector2 positionRange, Color color) : base(GFX.Game["objects/PuzzleIslandHelper/digiFolliage/background00"], true)
            {
                CenterOrigin();
                Position = position + new Vector2(Width / 2, Height / 2);
                PositionRange = positionRange;
                Color = color;
                Scale = Vector2.One * 1.6f;
            }
            private float addRotation;
            private int rotationMult;
            private float timeMult;
            public override void Added(Entity entity)
            {
                base.Added(entity);
                RotationRate = Calc.Random.Range(5f, 30f).ToRad();
                rotationMult = Calc.Random.Choose(-1, 1);
                timeMult = Calc.Random.Range(0.3f, 1);
                Rotation = Calc.Random.Range(0, 360f).ToRad();
                Color = Color.Lerp(Color, Color.Black, Calc.Random.Range(0.5f, 0.7f));
            }
            public override void Update()
            {
                base.Update();
                addRotation = BgFolliageRenderer.Sine * timeMult * RotationRate * rotationMult;
            }
            public override void Render()
            {
                if (Texture != null)
                {
                    Draw.SpriteBatch.Draw(Texture.Texture.Texture_Safe, RenderPosition.Floor(), null, Color, Rotation + addRotation, Origin, Scale, Effects, 0);
                }
            }
        }
        public class FgFolliage : Image
        {
            public float Timer;
            public Vector2 PositionRange;
            public float TimeMult;
            public float Delay = 0.1f;
            private float lerpAmount;
            private float lerpTimer;
            public float Alpha = 1;
            private float lerpAdd;
            private float lerpTime;
            public static string path = "objects/PuzzleIslandHelper/digiFolliage/flower0";
            private Color trueColor;
            public static MTexture[] Frames = new MTexture[4]
            {
                GFX.Game[path + '0'],
                GFX.Game[path + '1'],
                GFX.Game[path + '2'],
                GFX.Game[path + '3'],
            };
            public Texture2D Tex;
            public FgFolliage(Vector2 position, Vector2 positionRange, Color color) : base(GFX.Game["objects/PuzzleIslandHelper/digiFolliage/flower00"], true)
            {
                Tex = Frames[0].Texture.Texture_Safe;
                CenterOrigin();
                Position = position + new Vector2(Width / 2, Height / 2);
                PositionRange = positionRange;
                Color = color;
            }
            public Vector2 _RenderPosition;
            public override void Added(Entity entity)
            {
                base.Added(entity);
                lerpTime = Calc.Random.Range(0.2f, 2);
                lerpAdd = Calc.Random.Range(0.3f, 0.6f);
                Rotation = Calc.Random.Range(0, 360f).ToRad();
                Scale = Vector2.One * (float)Math.Round(Calc.Random.Range(0.5f, 1.2f), 1);
                Color = Color.Lerp(Color, Color.Black, Calc.Random.Range(0, 0.3f));
                Timer = Calc.Random.Range(0, 2f);
                TimeMult = Calc.Random.Range(0.1f, 1f);
                Position += PianoUtils.Random(-PositionRange, PositionRange);
                _RenderPosition = RenderPosition.Floor();
            }
            public override void Render()
            {
                Draw.SpriteBatch.Draw(Tex, _RenderPosition, null, trueColor, Rotation, Origin, Scale, Effects, 0);
            }
            public override void Update()
            {
                trueColor = Color.Lerp(Color, Color.Black, lerpAmount) * Alpha;
                lerpTimer -= Engine.DeltaTime;
                if (lerpTimer < 0)
                {
                    lerpTimer = 0;
                    lerpAmount = 0;
                }
                if (Calc.Random.Chance(0.04f))
                {
                    lerpTimer = lerpTime;
                    lerpAmount = lerpAdd;
                    Alpha = Calc.Random.Range(0.5f, 1.5f);
                }
                Tex = Frames[(int)(BgFolliageRenderer.Sine * TimeMult * Frames.Length)].Texture.Texture_Safe;
            }
        }
    }
}