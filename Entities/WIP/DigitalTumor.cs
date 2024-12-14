using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;
namespace Celeste.Mod.PuzzleIslandHelper.Entities.WIP
{
    [CustomEntity("PuzzleIslandHelper/DigitalTumor")]
    [Tracked]
    public class DigiTumor : Entity
    {
        public List<Tumor> Tumors = new();
        public string Flag;
        public bool OnScreen;
        public bool CanRender;
        public DigiTumor(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Tag |= Tags.TransitionUpdate;
            Depth = -1;
            Collider = new Hitbox(data.Width, data.Height);
            Flag = data.Attr("flag");
            Color color = data.HexColor("color");
            for (int i = 0; i < Width; i += 8)
            {
                for (int j = 0; j < Height; j += 8)
                {
                    Tumor tumor = new Tumor(new Vector2(i, j), Vector2.One * 4, color);
                    Tumors.Add(tumor);
                    Add(tumor);
                }
            }
        }
        public override void Update()
        {
            base.Update();
            if (Scene is Level level)
            {
                OnScreen = Left - 20 < level.Camera.X + 320 && Right + 20 > level.Camera.X && Top - 20 < level.Camera.Y + 180 && Bottom + 20 > level.Camera.Y;
                CanRender = OnScreen && (string.IsNullOrEmpty(Flag) || level.Session.GetFlag(Flag));
            }
        }
        public override void Render()
        {
            if (CanRender)
            {
                foreach (Tumor t in Tumors)
                {
                    t.Render();
                }
            }
        }
        public class Tumor : Image
        {
            public Vector2 PositionRange;
            public float Alpha = 1;
            private float timeMult;
            public Vector2 _RenderPosition;
            private float Sine => SineHelper.Percent;
            private float sineOffset;
            public Tumor(Vector2 position, Vector2 positionRange, Color color) : base(GFX.Game["objects/PuzzleIslandHelper/digitalTumor/tumor"], true)
            {
                CenterOrigin();
                Position = position + new Vector2(Width / 2, Height / 2);
                PositionRange = positionRange;
                Color = color;
            }
            public override void Added(Entity entity)
            {
                base.Added(entity);
                Color = Color.Lerp(Color, Color.Black, Calc.Random.Range(0f, 0.6f));
                _RenderPosition = (PianoUtils.Random(-PositionRange, PositionRange) + RenderPosition).Floor();
            }
            public override void EntityAwake()
            {
                base.EntityAwake();
                timeMult = Calc.Random.Range(0.3f, 1);
                sineOffset = Calc.Random.Range(0.2f, 0.6f);
            }
            public override void Update()
            {
                base.Update();
                Scale = Vector2.One * (0.5f + (((Sine + sineOffset) % 1) * timeMult * 0.5f));
            }
            public override void Render()
            {
                if (Texture != null)
                {
                    Draw.SpriteBatch.Draw(Texture.Texture.Texture_Safe, _RenderPosition, null, Color, Rotation, Origin, Scale, Effects, 0);
                }
            }
        }
    }


}