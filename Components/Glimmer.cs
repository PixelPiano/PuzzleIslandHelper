using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;

namespace Celeste.Mod.PuzzleIslandHelper.Components
{
    [Tracked]
    public class Glimmer : GraphicsComponent
    {
        private float flashTimer;
        private float blockMult = 1;
        public Color EdgeColor = Color.Transparent;
        public Color CenterColor
        {
            get => Color;
            set => Color = value;
        }
        public int Lines = 8;
        public bool Blocked;
        public bool FadeWhenBlocked = true;
        public float RotationRate;
        public float BaseAlpha = 1;
        public float FadeThresh;
        public float RotationInterval;
        public float FlashMult = 0;
        public float FadeMult = 1;
        public float LineOffset = 4;
        public int MinAngle = 0;
        public int MaxAngle = 360;
        public bool FadeX;
        public bool FadeY;
        public bool Flashes;
        public float FlashIntensity;
        public float FlashDelay;
        public float FlashAttack;
        public float FlashSustain;
        public float FlashRelease;
        public float FlashWait;
        public bool SolidColor = false;
        public int LineWidth = 1;
        public int LineWidthTarget = 1;
        public float Alpha => ((BaseAlpha * FadeMult) + (FlashIntensity * FlashMult)) * blockMult * AlphaMult;
        public float AlphaMult = 1;
        public FlashStages FlashStage
        {
            get => _stage;
            set
            {
                _stage = value;
                switch (value)
                {
                    case FlashStages.None:
                        Flashes = false;
                        FlashMult = 0;
                        break;
                    case FlashStages.Delay:
                        Flashes = true;
                        flashTimer = FlashDelay;
                        FlashMult = 0;
                        break;
                    case FlashStages.Attack:
                        Flashes = true;
                        flashTimer = FlashAttack;
                        FlashMult = 0;
                        break;
                    case FlashStages.Sustain:
                        Flashes = true;
                        flashTimer = FlashSustain;
                        FlashMult = 1;
                        break;
                    case FlashStages.Release:
                        Flashes = true;
                        flashTimer = FlashRelease;
                        FlashMult = 1;
                        break;
                    case FlashStages.Wait:
                        Flashes = true;
                        flashTimer = FlashWait;
                        FlashMult = 0;
                        break;
                }
            }
        }
        private FlashStages _stage = FlashStages.None;
        public enum FlashStages
        {
            None = -2,
            Delay = -1,
            Attack = 0,
            Sustain = 1,
            Release = 2,
            Wait = 3
        }

        public float Size = 8;
        public void ResetFlash()
        {
            if (Flashes)
            {
                SnapFlash(FlashStages.Delay);
            }
        }
        public void SnapFlash(FlashStages stage)
        {
            FlashStage = stage;
        }
        public void AdvanceFlash()
        {
            //This method is run in Update() only if Flashes is true. Otherwise, if called elsewhere, Flashes is set to true.
            if (flashTimer <= 0)
            {
                //Delay is never repeated unless FlashStage is specifically set to that
                //None (-2) -> Delay (-1)
                //Delay (-1) -> Attack (0)
                //Wait (3) -> Attack(0)
                SnapFlash((FlashStages)(((int)FlashStage + 1) % 4));
            }
            switch (FlashStage)
            {
                case FlashStages.Delay:
                    FlashMult = 0;
                    break;
                case FlashStages.Attack:
                    if (FlashAttack != 0)
                    {
                        FlashMult = 1 - (flashTimer / FlashAttack);
                    }
                    break;
                case FlashStages.Sustain:
                    FlashMult = 1;
                    break;
                case FlashStages.Release:
                    if (FlashRelease != 0)
                    {
                        FlashMult = flashTimer / FlashRelease;
                    }
                    break;
                case FlashStages.Wait:
                    FlashMult = 0;
                    break;
            }
            flashTimer -= Engine.DeltaTime;
        }
        public Glimmer(Color color, Color color2) : base(true)
        {
            CenterColor = color;
            EdgeColor = color2;
        }
        public Glimmer(Vector2 position, Color color, float size, int lines, float lineOffset, float rotateRate) : this(position, color, Color.Transparent, size, lines, lineOffset, rotateRate)
        {
        }
        public Glimmer(Vector2 position, Color color, Color color2, float size, int lines, float lineOffset, float rotateRate) : this(color, color2)
        {
            Position = position;
            Size = size;
            Lines = lines;
            LineOffset = lineOffset;
            RotationRate = rotateRate;
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            if (Alpha > 0)
            {
                Draw.Point(RenderPosition, Color.Cyan);
            }
            else
            {
                Draw.Point(RenderPosition, Color.Red);
            }
        }
        public override void Update()
        {
            base.Update();
            if (Flashes)
            {
                AdvanceFlash();
            }
            Vector2 position = RenderPosition;
            if (FadeX || FadeY)
            {
                if (Scene.GetPlayer() is Player player)
                {
                    bool fade = false;
                    if (FadeX && FadeY)
                    {
                        fade = Vector2.Distance(player.Center, position) >= FadeThresh;
                    }
                    else if (FadeY)
                    {
                        fade = Math.Abs(player.CenterY - Y + position.Y) >= FadeThresh;
                    }
                    else if (FadeX)
                    {
                        fade = Math.Abs(player.CenterX - X + position.X) >= FadeThresh;
                    }
                    FadeMult = Calc.Approach(FadeMult, fade ? 0 : 1, Engine.DeltaTime);
                }
            }
            if (RotationInterval <= 0 || Scene.OnInterval(RotationInterval * Engine.DeltaTime))
            {
                Rotation += RotationRate;
                Rotation %= 360;
            }
            Blocked = false;
            if (FadeWhenBlocked)
            {
                foreach (BlockerComponent blocker in Scene.Tracker.GetComponents<BlockerComponent>())
                {
                    if (blocker.Check(RenderPosition))
                    {
                        Blocked = true;
                        break;
                    }
                }
                blockMult = Calc.Approach(blockMult, Blocked ? 0 : 1, 5 * Engine.DeltaTime);
            }
            else
            {
                blockMult = 1;
            }
        }
        public override void Render()
        {
            base.Render();
            Render(1);
        }
        public void Render(float lerp)
        {
            if (Alpha > 0)
            {
                //no point in rendering if the color is transparent
                Vector2 center = RenderPosition;
                for (int i = 0; i < Lines; i++)
                {
                    float deg = ((360f / Lines * i) + Rotation) % 360;
                    if (deg >= MinAngle && deg <= MaxAngle)
                    {
                        float angle = deg.ToRad();
                        float size = Size;
                        if (i % 2 == 0) size += LineOffset;
                        Vector2 start = center;
                        Vector2 end = center + Calc.AngleToVector(angle, size) * Scale;
                        if (SolidColor)
                        {
                            Color color = Color.Lerp(CenterColor, EdgeColor, lerp) * Alpha;
                            if (color != Color.Transparent)
                            {
                                Vector2 s = start - Vector2.One * Math.Max(1, LineWidth / 2);
                                Draw.SpriteBatch.Draw(Draw.Pixel.Texture.Texture_Safe, start, Draw.Pixel.ClipRect,
                                Color.Lerp(EdgeColor, CenterColor, lerp) * Alpha, Calc.Angle(start, end), Vector2.Zero, new Vector2(Vector2.Distance(start, end), LineWidth), SpriteEffects.None, 0f);

                            }
                        }
                        else
                        {
                            float length = (end - center).Length();
                            for (int j = 0; j < length; j++)
                            {
                                float lineLerp = (1f / length * j);
                                float pointAlpha = lerp * (1 - lineLerp);
                                Color color = Color.Lerp(EdgeColor, CenterColor, pointAlpha) * Alpha;
                                //see above
                                if (color != Color.Transparent)
                                {
                                    float scale = Calc.LerpClamp(LineWidth, LineWidthTarget, lineLerp);
                                    Vector2 offset = Vector2.One * scale / 2;
                                    Draw.SpriteBatch.Draw(Draw.Pixel.Texture.Texture_Safe, Vector2.Lerp(start, end, lineLerp) - offset,
                                        Draw.Pixel.ClipRect, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                                }
                            }
                        }
                    }
                }
            }

        }
    }
}
