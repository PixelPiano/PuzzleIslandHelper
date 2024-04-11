using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/FloatingDecal")]
    [Tracked]
    public class FloatingDecal : Entity
    {
        public Sprite sprite;
        private float interval;
        private float startAmount;
        private Vector2 to;
        private Vector2 from;
        private string flag;
        private bool inverted;
        private bool flagState => Scene is Level level && (string.IsNullOrEmpty(flag) || level.Session.GetFlag(flag) == inverted);
        public FloatingDecal(EntityData data, Vector2 offset)
        : base(data.Position + offset)
        {
            Depth = data.Int("depth", 2);
            interval = data.Float("interval");

            float delay = 1f / (data.Float("fps") / 2f);
            sprite = new Sprite(GFX.Game, "decals/");
            flag = data.Attr("flag", "floating_decal");
            inverted = data.Bool("invertFlag");
            startAmount = Calc.Random.Range(0f, 1f);
            string path = data.Attr("decalPath");
            if (path.Contains("decals/"))
            {
                path.Replace("decals/", "");
            }
            sprite.AddLoop("idle", data.Attr("decalPath"), delay);
            Add(sprite);
            sprite.Color = data.HexColor("color");
            sprite.Rotation = data.Float("rotation").ToRad();
            sprite.CenterOrigin();
            sprite.Position += new Vector2(sprite.Width / 2, sprite.Height / 2);
            sprite.Scale = new Vector2(data.Float("scaleX", 1), data.Float("scaleY", 1));
            Vector2 spriteoff = new Vector2((sprite.Width / 2 * sprite.Scale.X), (sprite.Height / 2 * sprite.Scale.Y));
            Position -= spriteoff;
            to = data.Nodes[0] + offset - spriteoff;
            from = Position;
            Collider = new Hitbox(sprite.Width, sprite.Height);
            Visible = flagState;
            Add(new Coroutine(Routine()));
        }
        private IEnumerator Routine()
        {
            while (!Visible)
            {
                yield return null;
            }
            Position.Y = Calc.LerpClamp(from.Y, to.Y, Ease.SineInOut(startAmount));
            for (float i = startAmount; i < 1; i += Engine.DeltaTime / interval)
            {
                while (!Visible)
                {
                    yield return null;
                }
                Position.Y = Calc.LerpClamp(from.Y, to.Y, Ease.SineInOut(i));
                yield return null;
            }

            while (true)
            {
                for (float i = 0; i < 1; i += Engine.DeltaTime / interval)
                {
                    while (!Visible)
                    {
                        yield return null;
                    }
                    Position.Y = Calc.LerpClamp(to.Y, from.Y, Ease.SineInOut(i));
                    yield return null;
                }
                for (float i = 0; i < 1; i += Engine.DeltaTime / interval)
                {
                    while (!Visible)
                    {
                        yield return null;
                    }
                    Position.Y = Calc.LerpClamp(from.Y, to.Y, Ease.SineInOut(i));
                    yield return null;
                }
            }
        }
        public override void Update()
        {
            base.Update();
            Visible = flagState;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            sprite.Play("idle");
        }
    }
}
