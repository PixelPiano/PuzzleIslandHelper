using Celeste.Mod.Entities;
using Celeste.Mod.FancyTileEntities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using VivHelper.Colliders;
using System.Linq;
using System.Net;
// PuzzleIslandHelper.ArtifactSlot
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/LabReactor")]
    [Tracked]
    public class LabReactor : Entity
    {
        private readonly CustomFloatingBlock[] Blocks = new CustomFloatingBlock[3];
        private Level level;
        private Vector2 offset;
        List<Sprite> Circles = new();
        private int CircleAmount = 12;
        List<Vector2> orig_Scale = new();
        List<Vector2> AdjustScale = new();
        private Color CircleColor = Color.Green;
        private float ScaleLerp = 0;
        private VirtualRenderTarget Target = VirtualContent.CreateRenderTarget("ReactorTarget", 320, 180);
        public LabReactor(EntityData data, Vector2 offset)
           : base(data.Position - new Vector2(17 * 8, 14 * 8) + offset)
        {
            this.offset = offset;
            Collider = new Hitbox(34 * 8, 28 * 8);
            for (int i = 0; i < CircleAmount; i++)
            {
                Sprite temp = new Sprite(GFX.Game, "utils/PuzzleIslandHelper/");
                temp.AddLoop("idle", "circle", 1f);

                temp.Play("idle");

                temp.Scale = new Vector2(Width / (i + 1) / temp.Width * 0.5f * CircleAmount, Height / (i + 1) / temp.Height * 0.5f * CircleAmount);
                temp.CenterOrigin();
                temp.Color = Color.Lerp(Color.Black, Color.Green, i * (1f / CircleAmount));
                temp.Position += new Vector2(Width / 2, Height / 2);
                orig_Scale.Add(temp.Scale);
                Circles.Add(temp);
                AdjustScale.Add(Vector2.Zero);
            }
            for (int i = 0; i < Circles.Count; i++)
            {
                float SizeIncrease = (1f - i / Circles.Count) * 0.5f;
                Circles[i].Scale += Vector2.One * SizeIncrease;
            }
            Tween ScaleTween = Tween.Create(Tween.TweenMode.YoyoLooping, Ease.SineInOut, 7, false);
            ScaleTween.OnUpdate = (Tween t) =>
            {
                ScaleLerp = t.Eased;
            };
            Add(ScaleTween);
            ScaleTween.Start();
            Add(Circles.ToArray());
            Add(new BeforeRenderHook(BeforeRender));
        }
        public override void Update()
        {
            base.Update();
            #region Circle Update
            for (int i = 0; i < Circles.Count; i++)
            {
                float SizeChange = (1f - i / Circles.Count);
                Circles[i].Scale = orig_Scale[i] + (orig_Scale[i] * ScaleLerp * 0.5f) - (Vector2.One * SizeChange);
            }
            #endregion
        }

        private void BeforeRender()
        {
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
            Blocks[0] = new CustomFloatingBlock(Position + new Vector2(0, 5 * 8), Vector2.Zero, "", 'W', 8 * 8, 15 * 8, false, 0,true); //left
            Blocks[1] = new CustomFloatingBlock(Position + new Vector2(22 * 8, 0), Vector2.Zero, "", 'W', 11 * 8, 11 * 8, false, 1,true); //right
            Blocks[2] = new CustomFloatingBlock(Position + new Vector2(10 * 8, 16 * 8), Vector2.Zero, "", 'W', 23 * 8, 12 * 8, false, 2, true); //bottom
            scene.Add(Blocks);
        }

        public void Remove()
        {
            foreach (CustomFloatingBlock b in Blocks)
            {
                b?.RemoveSelf();
            }

        }

        public override void Render()
        {
            base.Render();
        }

    }
}
