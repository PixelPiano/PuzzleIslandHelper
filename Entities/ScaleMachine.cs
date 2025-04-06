using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/ScaleMachine")]
    [Tracked]
    public class ScaleMachine : Entity
    {
        public float LeftDoorOffset;
        public float RightDoorOffset;
        private float platformLength;
        private InvisibleBarrier topBarrier;
        private Solid[] blocks = new Solid[2];
        private int leftSize;
        private int rightSize;
        private Image below;
        private Entity FG;
        private Image fgImage;
        private JumpthruPlatform[] platforms = new JumpthruPlatform[2];
        public ScaleMachine(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
         
            LeftDoorOffset = data.Float("leftDoorOffset");
            RightDoorOffset = data.Float("rightDoorOffset");
            leftSize = data.Int("leftDoorSize", 1);
            rightSize = data.Int("rightDoorSize", 1);
            platformLength = data.Float("platformLength");
            fgImage = new Image(GFX.Game["objects/PuzzleIslandHelper/scaleMachine/fg"]);
            FG = new Entity(Position)
            {
                Depth = -1,
            };
            FG.Add(fgImage);
            Depth = 2;
            below = new Image(GFX.Game["objects/PuzzleIslandHelper/scaleMachine/lonn"]);
            Add(below);
            Collider = new Hitbox(below.Width, below.Height - 20, 0, 20);
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(FG);
        }
        public override void Render()
        {
            base.Render();
            Draw.Rect(Left, Top + LeftDoorOffset, 16, leftSize * 8, Color.Black);
            Draw.Rect(Right - 16, Top + RightDoorOffset, 16, rightSize * 8, Color.Black);
            //todo: make sprites for this

        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            FG.RemoveSelf();
        }
    }
}
