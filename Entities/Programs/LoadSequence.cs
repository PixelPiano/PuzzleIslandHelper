using Celeste.Mod.PuzzleIslandHelper.Entities.BetterInterfaceEntities;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Programs
{
    public class LoadSequence : Entity
    {
        public Sprite Sprite;
        public Entity loader;
        private bool Initialize = false;
        public bool ButtonPressed = false;
        public static float BarProgress = 0;
        private int RandRange = 20;
        private float RandProgress = 0;
        public static float BarWidth = 64;
        public static float BarHeight = 16;
        private int timer = 0;
        public static bool DoneLoading = false;
        public static bool HasArtifact;
        private Sprite Warning;
        public static bool Invalid = false;
        private int InvalidCount = 0;
        private Color SpriteColor = Color.Green;
        private float timer2 = 0.5f;
        private bool startTimer2 = false;
        public LoadSequence(int Depth, Vector2 Position)
        {
            this.Position = Position;
            this.Depth = Depth - 1;
            Add(Sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/interface/load/"));
            Add(Warning = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/interface/load/"));
            Warning.AddLoop("idle", "accessWarning", 0.1f);
            Sprite.AddLoop("accessIdle", "accessLoad", 1f, 76);
            Sprite.AddLoop("accessDennysIdle", "accessDennys", 1f, 5);
            Sprite.Add("accessLoad", "accessLoad", 0.1f, "accessIdle");
            Sprite.Add("accessDennys", "accessDennys", 0.1f, "accessDennysIdle");
            Sprite.Visible = false;
            Collider = new Hitbox(Sprite.Width, Sprite.Height);
            Warning.Position -= new Vector2(BetterWindow.CaseHeight * 0.66f, BetterWindow.CaseWidth * 0.33f);
            Add(new Coroutine(SpriteColorLerp()));
        }
        private IEnumerator SpriteColorLerp()
        {
            while (HasArtifact)
            {
                if (Interface.Loading)
                {
                    for (float i = 0; i < 1; i += Engine.DeltaTime)
                    {
                        SpriteColor = Color.Lerp(Color.Green, Color.DarkGreen, i);
                        yield return null;
                    }
                    yield return 0.1f;
                    for (float i = 0; i < 1; i += Engine.DeltaTime)
                    {
                        SpriteColor = Color.Lerp(Color.Green, Color.DarkGreen, 1 - i);
                        yield return null;
                    }
                }
                yield return null;
            }
        }
        public override void Update()
        {
            base.Update();
            HasArtifact = PianoModule.SaveData.HasArtifact;
            if (startTimer2)
            {
                timer2 -= Engine.DeltaTime;
            }
            Warning.Position = Sprite.Position - new Vector2(BetterWindow.CaseWidth * 0.7f, BetterWindow.CaseHeight * 0.7f);
            //Reset values if BetterWindow isn't being drawn
            if (!BetterWindow.Drawing)
            {
                Initialize = false;
                Sprite.Visible = false;
                Warning.Visible = false;
                Warning.Stop();
                Interface.Loading = false;
                ButtonPressed = false;
                return;
            }

            //Reset the Texture animation if button is pressed right after the BetterWindow is drawn
            if (!Initialize && ButtonPressed)
            {
                startTimer2 = false;
                timer2 = 0.5f;
                Sprite.Stop();
                if (!HasArtifact)
                {
                    Warning.Play("idle");
                }
                Sprite.Play(HasArtifact ? "accessLoad" : "accessDennys");
                Initialize = true;
            }

            //If the Interface initiates a loading sequence
            if (Interface.Loading)
            {
                bool haltLoading = Calc.Random.Range(0, 4) == 0;
                if (timer > 0)
                {
                    timer--;
                }

                //If player has item
                if (HasArtifact)
                {
                    Sprite.SetColor(SpriteColor);
                    if (!haltLoading)
                    {
                        BarProgress = BarProgress < 1 ? BarProgress + 0.001f + RandProgress : BarProgress;
                    }
                    DoneLoading = BarProgress >= 1;
                    RandProgress = timer == 0 ? Calc.Random.Range(0f, 0.005f) : RandProgress;
                    if (haltLoading && timer == 0)
                    {
                        RandProgress = 0;
                    }
                    if (timer == 0)
                    {
                        timer = 30;
                    }
                }

                //if player does not have item
                else
                {
                    startTimer2 = true;
                    if (timer2 <= 0)
                    {
                        Warning.Visible = true;
                    }
                    Sprite.Visible = true;
                    Sprite.SetColor(Color.Lerp(Color.Red, Color.White, 0.3f));
                    //"no artifact detected!!!!!
                    //Warning.Visible = timer < 30;
                }
            }
            else
            {
                timer = 0;
                BarProgress = 0;
                RandProgress = 0;
            }
            Sprite.Origin = new Vector2(Sprite.Width, Sprite.Height);
            Sprite.Scale = new Vector2(BetterWindow.CaseWidth / Sprite.Width, BetterWindow.CaseHeight / Sprite.Height);
            Position = new Vector2((int)BetterWindow.DrawPosition.X, (int)BetterWindow.DrawPosition.Y) + new Vector2(Sprite.Width, Sprite.Height) * Sprite.Scale;
        }
        public override void Render()
        {
            base.Render();
            if (BetterWindow.Drawing)
            {
                //Draw border again to cover up part of Texture
                Draw.HollowRect(BetterWindow.DrawPosition, (int)BetterWindow.CaseWidth, (int)BetterWindow.CaseHeight, Color.Gray);

                //If player has item and Interface is in loading sequence
                if (Interface.Loading && HasArtifact)
                {
                    Draw.Rect(-1 + BetterWindow.DrawPosition.X + BetterWindow.CaseWidth / 2 - BarWidth / 2, -1 + BetterWindow.DrawPosition.Y + BetterWindow.CaseHeight / 2 - BarHeight / 2, BarWidth + 2, BarHeight + 2, Color.Black);
                    if (HasArtifact)
                    {
                        Draw.Rect(BetterWindow.DrawPosition.X + BetterWindow.CaseWidth / 2 - BarWidth / 2, BetterWindow.DrawPosition.Y + BetterWindow.CaseHeight / 2 - BarHeight / 2, BarWidth * BarProgress, BarHeight, Color.LimeGreen);
                    }
                }
                else
                {
                    BarProgress = 0;
                }
            }
            else
            {
                BarProgress = 0;
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Initialize = false;
            DoneLoading = false;
        }

    }
}