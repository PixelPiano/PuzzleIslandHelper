using Celeste.Mod.CommunalHelper;
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using ExtendedVariants.Variants;
using FrostHelper.ModIntegration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Monocle;
using System;
using System.Collections.Generic;
using VivHelper.Entities;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{

    [CustomEntity("PuzzleIslandHelper/ThreadedBlock")]
    [Tracked]
    public class ThreadedBlock : Solid
    {
        public string CounterName;
        public float CounterSpace;
        private int CurrentCount => ThreadedBlockMachine.Counters[CounterName];
        public Vector2 OrigPosition;
        public Vector2 SwitchPosition => OrigPosition - (Vector2.UnitY * CurrentCount * CounterSpace);
        public TileGrid tiles;
        private float dashEase, yLerp;
        private Vector2 dashDirection;
        public ThreadedBlock(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, true)
        {
            OrigPosition = Position;
            CounterSpace = data.Float("counterSpace");
            CounterName = data.Attr("counterName");
            if (!ThreadedBlockMachine.Counters.ContainsKey(CounterName))
            {
                ThreadedBlockMachine.Counters.Add(CounterName, 0);
            }
            Add(tiles = GFX.FGAutotiler.GenerateBox(data.Char("tiletype"), data.Width / 8, data.Height / 8).TileGrid);
            Tag |= Tags.TransitionUpdate;
            OnDashCollide = OnDash;
        }
        public DashCollisionResults OnDash(Player player, Vector2 direction)
        {
            if (dashEase <= 0.2f)
            {
                dashEase = 1f;
                dashDirection = direction;
            }

            return DashCollisionResults.NormalOverride;
        }
        private float JITBarrier(float f)
        {
            return f;
        }
        private float sinkTimer;
        public override void Update()
        {
            base.Update();

            bool flag = HasPlayerRider();
            if (flag)
            {
                sinkTimer = 0.3f;
            }
            else if (sinkTimer > 0f)
            {
                sinkTimer -= Engine.DeltaTime;
            }

            if (sinkTimer > 0f)
            {
                yLerp = Calc.Approach(yLerp, 1f, 1f * Engine.DeltaTime);
            }
            else
            {
                yLerp = Calc.Approach(yLerp, 0f, 1f * Engine.DeltaTime);
            }
            dashEase = Calc.Approach(dashEase, 0f, Engine.DeltaTime * 1.5f);


            Vector2 vector = Calc.YoYo(Ease.QuadIn(dashEase)) * dashDirection * 8f;
            Vector2 value = SwitchPosition;
            double yOffset = (double)JITBarrier(12f) * (double)Ease.SineInOut(yLerp);
            double num2 = (double)value.Y + yOffset;
            MoveToY((float)(num2 + (double)vector.Y));
            MoveToX(value.X + vector.X);
            foreach (DottedLine line in Lines)
            {
                line.Offset = new Vector2(-vector.X, -(float)yOffset * 2 - vector.Y);
            }
            LiftSpeed = Vector2.Zero;
        }
        public DottedLine[] Lines;

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            MoveToY(SwitchPosition.Y);

            Rectangle bounds = (scene as Level).Bounds;
            Lines = new DottedLine[2];
            for (int i = 0; i < 2; i++)
            {
                float x = Left + 8 + i * (Width - 16);
                Lines[i] = new DottedLine(new Vector2(x, bounds.Bottom + 16), new Vector2(x, bounds.Top - 16), 1, FancyLine.ColorModes.Fade, 8, 2, 0.05f, Color.Lime, Color.Cyan, 60f);
            }
            scene.Add(Lines);
            OrigPosition = Position;
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            foreach (DottedLine line in Lines)
            {
                line.RemoveSelf();
            }
        }
    }
}