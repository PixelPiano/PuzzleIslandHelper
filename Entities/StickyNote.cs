using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/StickyNote")]
    [Tracked]
    public class StickyNote : Entity
    {
        public Sprite Sprite;
        public float TextureScale = 1;
        public string CustomPath;

        public class UI : Entity
        {
            public MTexture Texture;
            public MTexture UserTexture;
            public float Eased;
            private StickyNote note;
            public bool Finished;
            private float startAngle = 45f.ToRad();
            public UI(StickyNote parent) : base()
            {
                Tag |= TagsExt.SubHUD;
                note = parent;
                if (!string.IsNullOrEmpty(note.CustomPath))
                {
                    UserTexture = GFX.Game[note.CustomPath];
                }
                Texture = GFX.Game["objects/PuzzleIslandHelper/stickyNote/uiTexture"];
                Add(new Coroutine(routine()));
            }
            private IEnumerator routine()
            {
                yield return PianoUtils.Lerp(Ease.SineInOut, 0.8f, f => Eased = f, true);
                while (!Input.MenuCancel)
                {
                    yield return null;
                }
                Input.Dash.ConsumePress();
                yield return PianoUtils.Lerp(Ease.SineInOut, 1, f => Eased = 1 - f, true);
                Finished = true;
            }
            public override void Render()
            {
                base.Render();
                if (Eased > 0)
                {
                    Draw.Rect(0, 0, 1920, 1080, Color.Black * Eased * 0.5f);
                    Vector2 pos = new Vector2(960, 1620 - Eased * 1080);
                    float angle = startAngle * (1 - Eased);
                    Texture.DrawCentered(pos, Color.Yellow, 1, angle);
                    UserTexture?.DrawCentered(pos, Color.White, note.TextureScale, angle);
                }

            }
        }
        public StickyNote(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            TextureScale = data.Float("textureScale", 1);
            CustomPath = data.Attr("userTexturePath");
            Depth = data.Int("depth", 10);
            Sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/stickyNote/");
            Sprite.AddLoop("idle", "idle", 0.1f);
            Sprite.Add("flutterEnd", "flutterEnd", 0.1f);
            Sprite.Add("flutterIn", "flutter", 0.1f, "flutterEnd");
            Sprite.Color = data.HexColor("color", Color.Yellow);
            Sprite.OnLastFrame += (s) =>
            {
                if (s == "flutterEnd")
                {
                    if (Calc.Random.Chance(0.2f))
                    {
                        Sprite.Play("flutterIn");
                    }
                    else
                    {
                        Sprite.Play("idle");
                        Alarm.Set(this, Calc.Random.Range(1, 3), () => Sprite.Play("flutterIn"));
                    }
                }
            };
            Add(Sprite);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Sprite.Play("idle");
            Alarm.Set(this, Calc.Random.Range(1, 3), () => Sprite.Play("flutterIn"));
            float height = 1;
            int maxHeight = 24;

            while (!scene.CollideCheck<Platform>(Position + Vector2.UnitY * height) && height < maxHeight)
            {
                height++;
            }
            Collider = new Hitbox(Sprite.Width + 8, height, -4);

            Add(new DotX3(Collider, p =>
            {
                Scene.DisableMovement();
                Add(new Coroutine(routine()));
            }));
        }
        private UI ui;
        private IEnumerator routine()
        {
            Scene.Add(ui = new UI(this));
            while (!ui.Finished)
            {
                yield return null;
            }
            ui.RemoveSelf();
            Scene.EnableMovement();
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            ui?.RemoveSelf();
        }
    }
}