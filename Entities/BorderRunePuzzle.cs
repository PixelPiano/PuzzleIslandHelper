using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/BorderRunePuzzle")]
    public class BorderRunePuzzle : Entity
    {
        public bool[] Lights = new bool[4];
        public MTexture BaseTexture => GFX.Game["objects/PuzzleIslandHelper/borderRune/borderRunePuzzle"];
        public MTexture LightTexture => GFX.Game["objects/PuzzleIslandHelper/borderRune/borderRuneLight"];
        public BorderRunePuzzle(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            MTexture t = BaseTexture;
            Collider = new Hitbox(t.Width, t.Height);
            Tag |= Tags.TransitionUpdate;
        }
        public override void Render()
        {
            base.Render();
            Draw.SpriteBatch.Draw(BaseTexture.Texture.Texture_Safe, Position, Color.White);
            for (int i = 0; i < 4; i++)
            {
                Vector2 pos = Center + GetAngleOffset(i, i % 2 == 0 ? 17 : 14);
                Draw.SpriteBatch.Draw(LightTexture.Texture.Texture_Safe, pos - LightTexture.HalfSize(), Lights[i] ? Color.Lime : Color.Gray);
            }
        }
        public float GetAngle(int index)
        {
            return index * MathHelper.TwoPi / 4;
        }
        public Vector2 GetAngleOffset(int index, float length)
        {
            float theta = GetAngle(index);
            return new Vector2(x: (float)Math.Cos(theta) * length, y: (float)Math.Sin(theta) * length);
        }
        public override void Update()
        {
            base.Update();
            GetFlags();
        }
        public void GetFlags()
        {
            for (int i = 0; i < 4; i++)
            {
                Lights[i] = SceneAs<Level>().Session.GetFlag("BorderRune" + i);
            }
        }
    }

    [CustomEntity("PuzzleIslandHelper/BorderRuneButton")]
    public class BorderRuneButton : Solid
    {
        public int Index;
        public bool On
        {
            get
            {
                return SceneAs<Level>().Session.GetFlag("BorderRune" + Index);
            }
            set
            {
                SceneAs<Level>().Session.SetFlag("BorderRune" + Index, value);
            }
        }
        public MTexture Texture => GFX.Game["objects/PuzzleIslandHelper/borderRune/borderRuneButton"];
        public Image Image;
        public BorderRuneButton(EntityData data, Vector2 offset) : base(data.Position + offset, 8, 16, true)
        {
            Depth = -1;
            Index = data.Int("index");
            OnDashCollide = OnDashCollideMethod;
            Add(Image = new Image(Texture));
            Tag |= Tags.TransitionUpdate;
        }
        public override void Render()
        {
            Image.DrawSimpleOutline();
            base.Render();
        }
        private IEnumerator pressRoutine()
        {
            On = true;
            float from = Position.Y;
            while (Position.Y > from + 14)
            {
                MoveTowardsY(from + 14, 3);
                yield return null;
            }
            MoveToY(from + 14);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (On)
            {
                InstantPress();
            }
        }
        public void InstantPress()
        {
            MoveV(14);
        }
        public DashCollisionResults OnDashCollideMethod(Player player, Vector2 dir)
        {
            if (!On && HasPlayerOnTop() && dir.Y > 0)
            {
                Add(new Coroutine(pressRoutine()));
                return DashCollisionResults.Rebound;
            }
            return DashCollisionResults.NormalCollision;
        }
        public override void Update()
        {
            base.Update();

        }
    }
}