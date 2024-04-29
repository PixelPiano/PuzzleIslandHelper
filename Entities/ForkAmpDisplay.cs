using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{

    public class ForkAmpDisplay : Entity
    {
        public const float WorldWidth = 12;
        public const float WorldHeight = 20;
        public float WIDTH = WorldWidth * 6;
        public float HEIGHT = WorldHeight * 6;
        public const float AbsoluteMin = 25f;
        public const float AbsoluteMax = 300f;
        public const float ChannelSize = 25f;


        public const int SegmentsPerChannel = 5;
        public const int ScaleTarget = 3;
        public const float SelectPadding = 4;
        public float Frequency;
        public float CurrentMin;
        public float CurrentMax;
        public bool Interacting = true;
        public float TextScale = 0.5f;

        public float TextOffset = 8;
        public bool Selected;
        public bool InPlay;

        public float ContentAlpha => InPlay ? 1 : 0.3f;
        public float FadeAmount;
        public bool Removing;
        public ForkAmpDisplay(Vector2 position, float min, float max) : base(position)
        {
            CurrentMin = Calc.Max(min, AbsoluteMin);
            CurrentMax = Calc.Min(max, AbsoluteMax);
            Frequency = CurrentMin;
            Collider = new Hitbox(WIDTH / 6, HEIGHT / 6);
            Tag = TagsExt.SubHUD;
        }
        public override void Update()
        {
            base.Update();
            FadeAmount = Calc.Clamp(FadeAmount + (Removing ? -Engine.DeltaTime : Engine.DeltaTime),0,1);
            if (Removing)
            {
                if(FadeAmount == 0)
                {
                    RemoveSelf();
                }
                return;
            }
            if(!Selected) return;
            if (Input.MoveY != 0)
            {
                Frequency += -Input.MoveY.Value;
            }
            Frequency = Calc.Clamp(Frequency, AbsoluteMin, AbsoluteMax);

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

        public override void Render()
        {
            base.Render();
            if (Selected)
            {
                Draw.Rect(Position.X - SelectPadding, Position.Y - SelectPadding, WIDTH + SelectPadding * 2, HEIGHT + SelectPadding * 2, Color.Yellow * FadeAmount);
                DrawMarkerBox(SelectPadding * 2, Color.Yellow * FadeAmount);
            }
            Draw.Rect(Position.X, Position.Y, WIDTH, HEIGHT, Color.Black * FadeAmount);
            DrawMarkerBox(4, Color.Black * FadeAmount);
            DrawDial();
            DrawMarker();
        }
        public void DrawDial()
        {
            float xOffset = Position.X + WIDTH;
            float middle = Position.Y + HEIGHT / 2;
            for (float i = AbsoluteMin; i <= AbsoluteMax; i += ChannelSize)
            {
                float offset = WIDTH/2 - 4;
                float yoffset = middle - (i - Frequency) * 6;
                bool big = true;
                for (int j = 0; j < SegmentsPerChannel; j++)
                {
                    if (i >= 300 && !big) break;
                    float y = yoffset;
                    float alpha = GetAlphaAt(y, HEIGHT) * ContentAlpha * FadeAmount;
                    if (alpha > 0)
                    {
                        Color color = (big ? Color.White : Color.Gray);
                        Draw.Line(xOffset - offset, y, Position.X + WIDTH, y, color * alpha, 6);
                        if (big)
                        {
                            float scale = TextScale * 0.8f;
                            string hz = i.ToString("0");
                            float height = ActiveFont.BaseSize * (scale / 2);
                            ActiveFont.Draw(hz, new Vector2(Position.X + 2, y - height), Vector2.Zero, Vector2.One * scale, Color.White * alpha);
                        }
                    }
                    yoffset -= ChannelSize / SegmentsPerChannel * 6;
                    offset = WIDTH/4 - 2;
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
            if(!InPlay) return;
            float height = ActiveFont.BaseSize * (TextScale / 2);
            string text = Frequency.ToString("0");
            Vector2 position = Position + new Vector2(WIDTH / 2 - GetTextArea(text) / 2, -height);
            ActiveFont.Draw(text, position, Vector2.Zero, Vector2.One * TextScale, Color.White * FadeAmount);
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Position -= (scene as Level).Camera.Position;
            Position *= 6;
            Collider.Position = (scene as Level).ScreenToWorld(Position) - Position;
        }
    }
}
