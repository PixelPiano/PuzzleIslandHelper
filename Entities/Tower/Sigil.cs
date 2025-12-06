using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.DEBUG;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Tower
{
    [Tracked]
    [CustomEntity("PuzzleIslandHelper/Column/SigilKey")]
    public class Sigil : Entity
    {
        public bool Behind;
        public char Key;
        public bool Activated
        {
            get => activated;
            set
            {
                ActivatedTime = Scene.TimeActive;
                activated = true;
            }
        }
        private bool activated;
        public Image Image;
        public float GrowAlpha = 0;
        public float GrowScale = 1;
        public float BgGrowAlpha = 0;
        public float BgGrowScale = 1;
        public float ActivatedTime;
        public ColorShifter swap = new ColorShifter(Color.Cyan, Color.LightBlue, Color.Blue);
        public ColorShifter scatter = new ColorShifter(Color.Cyan, Color.LightBlue);

        public ColorShifter glowSwap = [Color.Cyan, Color.Teal, Color.LightBlue];
        public Sigil(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Collider = data.Collider();
            Vector2 node = data.NodesOffset(offset)[0] - Position;
            Add(new DashListener(OnDashed));
            Depth = data.Int("depth");
            Behind = data.Bool("behind");
            Add(Image = new Image(GFX.Game[data.Attr("texture")]));
            Image.CenterOrigin();
            Image.Position += node + Image.HalfSize();
            Key = data.Char("key");
            Add(swap, scatter, glowSwap);
            swap.Pause();
            scatter.Pause();
            glowSwap.Pause();
        }
        public override void DebugRender(Camera camera)
        {
            Draw.HollowRect(Collider, Collidable ? Color.Orange : Color.Yellow);
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
        }
        private IEnumerator routine()
        {
            for (float i = 0, j = 0; i < 1 && j < 1; i += Engine.DeltaTime, j += Engine.DeltaTime / 1.5f)
            {
                float easeA = Ease.CubeInOut(i);
                float easeB = Ease.CubeInOut(j);
                GrowScale = 1 + easeA;
                GrowAlpha = 1 - easeA;
                BgGrowScale = 1 + easeB;
                BgGrowAlpha = 1 - easeB;
                yield return null;
            }
            BgGrowAlpha = 0;
            GrowAlpha = 0;
            GrowScale = 0;
            BgGrowScale = 0;
        }
        public void OnDashed(Vector2 dir)
        {
            if (dir.Y == -1 && CollideCheck(Scene.GetPlayer()))
            {
                Activate(false);
            }
        }
        public void Activate(bool instant)
        {
            if (Activated) return;
            Pulse.Circle(this, Pulse.Fade.InAndOut, Pulse.Mode.Oneshot, Collider.Center, 0, Image.Width, 1, true, Color.Cyan, Color.Transparent, Ease.CubeOut, Ease.CubeOut);
            ActivatedTime = Scene.TimeActive;
            Activated = true;
            if (!instant)
            {
                Add(new Coroutine(routine()));
            }
        }
        public void Deactivate(bool instant)
        {
            Activated = false;
        }
        public override void Update()
        {
            base.Update();
            if (Activated)
            {
                if (Scene.OnInterval(Engine.DeltaTime * 4f, ActivatedTime))
                {
                    Image.Color = swap.Current;
                    swap.AdvanceColors();
                    scatter.AdvanceColors();
                }
            }
            else
            {
                if (Image.Color != Color.Black)
                {
                    Image.Color = Color.Lerp(Image.Color, Color.Black, Engine.DeltaTime * 4);
                }
            }
        }
        public override void Render()
        {
            Color origColor = Image.Color;
            Vector2 origScale = Image.Scale;
            if (Activated)
            {

                Color c = Color.Lerp(Color.White, scatter.Current, 0.7f);
                Vector2 p = Image.Position;
                for (int i = 0; i < 3; i++)
                {
                    Image.Position = p + Calc.Random.ShakeVector();
                    Image.DrawOutline(c * 0.06f, i + 2);
                }
                Image.Position = p;
                Image.DrawOutline(Color.Lerp(Color.White, swap.Current, 0.7f) * 0.4f);
            }
            else
            {
                Image.DrawOutline(Color.DarkGray);
            }
            if (BgGrowAlpha > 0 && BgGrowScale > 0)
            {
                Image.Scale = BgGrowScale * Vector2.One;
                Image.Color = origColor * BgGrowAlpha;
                Image.Render();
            }
            Image.Scale = origScale;
            Image.Color = origColor;
            base.Render();
            if (GrowAlpha > 0 && GrowScale > 0)
            {
                Image.Scale = GrowScale * Vector2.One;
                Image.Color = origColor * GrowAlpha;
                Image.Render();
            }
            Image.Scale = origScale;
            Image.Color = origColor;
        }
    }
}