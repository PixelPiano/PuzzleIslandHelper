using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/CollectableIndicator")]

    public class CollectableIndicator : Entity
    {

        private int Collectables;
        private List<string> flags = new();
        private List<Sprite> Body = new();
        private Sprite SpriteLeft;
        private Sprite SpriteRight;
        private Color Color = Color.Black * 0.4f;
        private List<BloomPoint> BloomPoints = new();
        private bool Hidden => PianoModule.Settings.HideCollectableIndicators;
        public CollectableIndicator(EntityData data, Vector2 offset)
       : base(data.Position + offset)
        {
            Depth = -10001;
            Tag |= Tags.TransitionUpdate;
            string prefix = data.Bool("miniHeart") ? "heart" : "tetris";
            Collectables = data.Int("collectables");
            string list = data.Attr("flags");
            flags = list.Split(new char[] { ',' }).ToList();
            while (flags.Count > Collectables)
            {
                flags.RemoveAt(flags.Count - 1);
            }
            SpriteLeft = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/collectableIndicator/");
            SpriteRight = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/collectableIndicator/");
            for (int i = 0; i < Collectables; i++)
            {
                Body.Add(new Sprite(GFX.Game, "objects/PuzzleIslandHelper/collectableIndicator/"));
                Body[i].AddLoop("idle", $"{prefix}Collect", 0.1f, 0);
                Body[i].AddLoop("collected", $"{prefix}Collect", 0.1f, 3);
                Body[i].Add("collect", $"{prefix}Collect", 0.1f, "collected");
                Add(Body[i]);
            }
            SpriteLeft.AddLoop("idle", "left", 0.1f);
            SpriteRight.AddLoop("idle", "right", 0.1f);
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Add(SpriteLeft);
            SpriteLeft.Play("idle");
            Add(SpriteRight);
            SpriteRight.Position.X += Collectables * 11 + SpriteLeft.Width;
            SpriteRight.Play("idle");

            for (int i = 0; i < Collectables; i++)
            {
                Body[i].Position.X += i * 11 + SpriteLeft.Width;
                BloomPoints.Add(new BloomPoint(Body[i].Position + new Vector2(6, 9), 1, 8));
                BloomPoints[i].Visible = false;
            }
            Add(BloomPoints.ToArray());
            for (int i = 0; i < Collectables; i++)
            {
                if ((scene as Level).Session.GetFlag(flags[i]))
                {
                    Body[i].Play("collected");
                    BloomPoints[i].Visible = true;
                }
                else
                {
                    Body[i].Play("idle");
                }
                Body[i].SetColor(Color.Lerp(Body[i].Color, Color.Black, 0.3f));
            }
            SpriteLeft.SetColor(Color.Lerp(SpriteLeft.Color, Color.Black, 0.3f));
            SpriteRight.SetColor(Color.Lerp(SpriteRight.Color, Color.Black, 0.3f));
        }
        public override void Update()
        {
            base.Update();
            if (Scene is not Level level || Hidden) return;
            for (int i = 0; i < Collectables; i++)
            {
                if (level.Session.GetFlag(flags[i]))
                {
                    if (Body[i].CurrentAnimationID != "collected")
                    {
                        Body[i].Play("collect");
                        Audio.Play("event:/game/09_core/frontdoor_heartfill", Position);
                        BloomPoints[i].Visible = true;
                    }
                }
            }
        }
        public override void Render()
        {
            if (!Hidden)
            {
                base.Render();
            }
        }

    }
}
