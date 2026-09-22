using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.Cutscenes;
using Iced.Intel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.WIP
{
    [CustomEntity("PuzzleIslandHelper/MiniHeartChecker")]
    [Tracked]
    public class MiniHeartChecker : Solid
    {
        public class Heart : GraphicsComponent
        {
            public string Flag;
            public string Path;
            public MTexture Outline;
            public MTexture BaseTexture;
            public MTexture Texture;
            public float Amount
            {
                get => amount;
                set
                {
                    if (value != amount)
                    {
                        switch (value)
                        {
                            case <= 0:
                                Texture = null;
                                break;
                            case >= 1:
                                Texture = BaseTexture;
                                break;
                            default:
                                int y = BaseTexture.Height - (int)(amount * BaseTexture.Height);
                                Texture = BaseTexture.GetSubtexture(0, y, BaseTexture.Width, BaseTexture.Height - y);
                                break;
                        }
                    }
                    amount = value;
                }
            }
            private float amount;
            public Heart(Vector2 position, MTexture texture, MTexture outline, string flag, Color color) : base(true)
            {
                Position = position;
                BaseTexture = texture;
                Outline = outline;
                Flag = flag;
                Color = color;
            }
            public void Activate(float duration)
            {
                Audio.Play("event:/game/09_core/frontdoor_heartfill", RenderPosition);
                Tween.Set(Entity, Tween.TweenMode.Oneshot, duration, Ease.Linear, t =>
                {
                    Amount = t.Eased;
                }, t =>
                {
                    Amount = 1;
                });
            }
            public override void Render()
            {
                base.Render();
                Vector2 p = RenderPosition;
                Texture?.Draw(p + Vector2.UnitY * (BaseTexture.Height - Texture.Height), Vector2.Zero, Color);
                Outline.Draw(p, Vector2.Zero, Color);
            }
        }
        public List<Heart> Hearts = [];
        public float XDetect;
        public float YDetect;
        private string path;
        private string[] flags;
        private Color[] colors;
        private float padY;
        private Color wallColor;
        private Color edgeColor;
        private FlagList flagsOnComplete;
        private EntityID id;
        public bool PlayCutscene;
        public void RemoveAndFlagAsGone()
        {
            RemoveSelf();
            SceneAs<Level>().Session.DoNotLoad.Add(id);
        }
        public void Break()
        {

            Audio.Play("event:/game/general/wall_break_stone", Position);

            for (int i = 0; (float)i < base.Width / 8f; i++)
            {
                for (int j = 0; (float)j < base.Height / 8f; j++)
                {
                    base.Scene.Add(Engine.Pooler.Create<Debris>().Init(Position + new Vector2(4 + i * 8, 4 + j * 8), '3', true).BlastFrom(TopCenter));
                }
            }

            Collidable = false;
            RemoveAndFlagAsGone();
        }
        public MiniHeartChecker(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset, data.Width, data.Height, true)
        {
            Depth = -12999;
            path = data.Attr("spritePath");
            if (string.IsNullOrEmpty(path))
            {
                path = "objects/PuzzleIslandHelper/miniHeartChecker/";
            }
            flagsOnComplete = data.FlagList("flagsOnComplete");
            flags = data.Attr("flags").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            string[] colorStringArray = data.Attr("colors").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (colorStringArray.Length > 0)
            {
                colors = new Color[colorStringArray.Length];
                for (int i = 0; i < colorStringArray.Length; i++)
                {
                    colors[i] = Calc.HexToColorWithAlpha(colorStringArray[i]);
                }
            }
            else colors = [Color.White];
            wallColor = Calc.HexToColorWithAlpha(data.Attr("wallColor"));
            edgeColor = data.HexColor("edgeColor", Color.Black);
            XDetect = data.Float("xDetect", -1);
            YDetect = data.Float("yDetect", -1);
            padY = data.Float("padY", 4);
            this.id = id;
            PlayCutscene = data.Bool("freezeTimeCutscene");
        }
        private float heartsWidth(float heartSpriteWidth, int hearts, float pad) => hearts * (heartSpriteWidth + pad) - pad;
        private int heartsPossible(float edgeSpriteWidth, float heartSpriteWidth, float width, int required, float pad)
        {
            float rowWidth = width - 2 * edgeSpriteWidth;

            for (int i = 0; i < required; i++)
            {
                if (heartsWidth(heartSpriteWidth, i, pad) > rowWidth) return i - 1;
            }
            return required;
        }
        public override void Added(Scene scene)
        {
            MTexture outline = GFX.Game[path + "outline"];
            MTexture texture = GFX.Game[path + "texture"];
            base.Added(scene);
            float width = Width;
            float height = Height;
            int hearts = flags.Length;
            float heartWidth = outline.Width;
            float heartHeight = outline.Height;
            if (hearts > 0)
            {
                int heartIndex = 0;
                int fits = heartsPossible(2, heartWidth, width, hearts, 4);
                int rows = (int)Math.Ceiling((float)hearts / fits);
                for (int row = 0; row < rows; row++)
                {
                    int displayedHearts = heartsPossible(2, heartWidth, width, hearts, 4);
                    float drawWidth = heartsWidth(heartWidth, displayedHearts, 4);
                    float startX = (float)Math.Round((width - drawWidth) / 2);
                    float startY = (float)Math.Round(rows / 2 * (heartHeight + padY)) - padY + (padY - 4);
                    for (int col = 0; col < displayedHearts; col++)
                    {
                        float drawX = startX + col * (heartWidth + 4);
                        float drawY = startY + row * (heartHeight + padY) + height / 2 - padY * 1.5f + (padY - 4);
                        Heart heart = new Heart(new Vector2(drawX, drawY), texture, outline, flags[heartIndex], colors[heartIndex % colors.Length]);
                        Hearts.Add(heart);
                        Add(heart);
                        heartIndex++;
                    }
                    hearts -= displayedHearts;
                }
            }

            if (AllCollected && ViewedRemoval)
            {
                setAsCompleteOnRemoved = true;
                SceneAs<Level>().Session.DoNotLoad.Add(id);
                RemoveSelf();
            }
            else
            {
                for (int i = 0; i < Math.Min(Collected, Required); i++)
                {
                    Hearts[i].Amount = 1;
                }
                Add(new Coroutine(routine()));
            }
        }
        public int Required => flags.Length;
        public int Collected => flags.Where(item => string.IsNullOrEmpty(item) || SceneAs<Level>().Session.GetFlag(item)).Count();
        public bool AllCollected => Counter >= Required;
        public bool ViewedRemoval
        {
            get => SceneAs<Level>().Session.GetFlag("MiniHeartCheckerRemoved:" + id);
            set => SceneAs<Level>().Session.SetFlag("MiniHeartCheckerRemoved:" + id, value);
        }
        public float Counter
        {
            get => SceneAs<Level>().Session.GetSlider("MiniHeartChecker:" + id);
            set => SceneAs<Level>().Session.SetSlider("MiniHeartChecker:" + id, value);
        }
        public bool Opened
        {
            get => SceneAs<Level>().Session.GetFlag("MiniHeartCheckerOpened:" + id);
            set => SceneAs<Level>().Session.SetFlag("MiniHeartCheckerOpened:" + id, value);
        }
        public bool Destroyed
        {
            get => SceneAs<Level>().Session.GetFlag("MiniHeartCheckerDestroyed:" + id);
            set => SceneAs<Level>().Session.SetFlag("MiniHeartCheckerDestroyed:" + id, value);
        }
        private bool setAsCompleteOnRemoved;
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            if (setAsCompleteOnRemoved)
            {
                Opened = true;
                Destroyed = true;
                flagsOnComplete.State = true;
            }
        }
        private IEnumerator routine()
        {
            float delay = 0.1f;
            float delayDecay = 0;
            while (!Opened && Counter < Required)
            {
                Player player = Scene.Tracker.GetEntity<Player>();
                if (player != null //player exists
                    && (XDetect <= 0 || MathHelper.Distance(player.CenterX, CenterX) <= XDetect)
                    //XDetect is disabled or player.CenterX is within XDetect pixels from Left or Right of solid
                    && (YDetect <= 0 || MathHelper.Distance(player.CenterY, CenterY) <= YDetect))
                //YDetect is disabled or player.CenterY is within YDetect pixels from Top or Bottom of solid
                {
                    if (Counter == 0 && Collected > 0)
                    {
                        Hearts[0].Activate(0.1f);
                        yield return delay;
                        delayDecay = Math.Max(0.05f, delayDecay - Engine.DeltaTime / 2f);
                    }
                    int prevCounter = (int)Counter;
                    int targetCounter = Math.Min(Collected, Required);
                    Counter = Calc.Approach(Counter, targetCounter, Engine.DeltaTime * Required * 0.8f);
                    float newCounter = Counter;
                    int newCounterFloor = (int)newCounter;

                    if (prevCounter != newCounterFloor && newCounter < targetCounter)
                    {
                        yield return delay - delayDecay;
                        Hearts[newCounterFloor].Activate(0.1f);
                        delayDecay = Math.Max(0.05f, delayDecay - Engine.DeltaTime / 2f);
                    }
                    else
                    {
                        delayDecay = 0;
                    }
                }
                else
                {
                    delayDecay = 0;
                }
                yield return null;
            }
            Opened = true;
            flagsOnComplete.State = true;
            Camera c = SceneAs<Level>().Camera;
            bool onScreen = Left >= c.Left && Right <= c.Right && Top >= c.Top && Bottom <= c.Bottom;
            if (onScreen) //prevent cutscene from playing if the entity is on screen.
            {
                Audio.Play("event:/game/general/fallblock_shake", Center);
                StartShaking(-1);
                yield return 0.8f;
                StopShaking();
                Audio.Play("event:/game/general/wall_break_stone", Center);

                for (int i = 0; i < Width / 8f; i++)
                {
                    for (int j = 0; j < Height / 8f; j++)
                    {
                        Scene.Add(Engine.Pooler.Create<Debris>().Init(Position + new Vector2(4 + i * 8, 4 + j * 8), '0', true).BlastFrom(Center));
                    }
                }
                SceneAs<Level>().Session.DoNotLoad.Add(id);
                RemoveSelf();
            }
            else
            {
                FreezeTimeBreak.Begin(this, '0', 1f, default, true, true, () =>
                {
                    SceneAs<Level>().Session.DoNotLoad.Add(id);
                });
            }
        }
        public Vector2 ShakeVector;
        public BetterShaker Shaker;
        public override void OnShake(Vector2 amount)
        {
            base.OnShake(amount);
            ShakeVector += amount;
            foreach (Heart h in Hearts)
            {
                h.Position += amount;
            }
        }
        public override void Render()
        {
            Vector2 p = Position + ShakeVector;
            Draw.HollowRect(p, Width, Height, edgeColor);
            Draw.Rect(p.X + 1, p.Y + 1, Width - 2, Height - 2, wallColor);
            base.Render();
        }
    }
}