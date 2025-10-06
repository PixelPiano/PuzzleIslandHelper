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
using static Celeste.Autotiler;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{

    [Tracked]
    [CustomEntity("PuzzleIslandHelper/LibraryPressureBlockController")]
    public class LibraryPressureBlockController : Entity
    {
        public string Flag;
        public string[] CodeArray;
        public int CurrentIndex;
        public LibraryPressureBlockController(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Flag = data.Attr("flag");
            CodeArray = data.Attr("code").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }
        public void ReceiveKey(char key)
        {
            if (CodeArray == null || CodeArray.Length <= 0) return;
            if (CurrentIndex < CodeArray.Length)
            {
                if (CodeArray[CurrentIndex] == key.ToString())
                {
                    CurrentIndex++;
                }
                else
                {
                    CurrentIndex = 0;
                }
            }
            if (CurrentIndex == CodeArray.Length)
            {
                SceneAs<Level>().Session.SetFlag(Flag);
                CurrentIndex = 0;
            }
        }
    }
    [CustomEntity("PuzzleIslandHelper/LibraryPressureBlock")]
    [Tracked]
    public class LibraryPressureBlock : Solid
    {
        public Vector2 OrigPosition;
        public TileGrid tiles;
        private float yLerp;
        private FlagList Required;
        private char key;
        public LibraryPressureBlock(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, true)
        {
            OrigPosition = Position;
            Generated g = GFX.FGAutotiler.GenerateBox(data.Char("tiletype"), data.Width / 8, data.Height / 8);
            Add(tiles = g.TileGrid);
            Tag |= Tags.TransitionUpdate;
            Required = data.FlagList("requiredFlags");
            key = data.Char("key");
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
        }
        private float sinkTimer;
        private float prevLerp;
        public override void Update()
        {
            base.Update();
            prevLerp = yLerp;
            bool flag = Required;
            bool playerOnTop = HasPlayerOnTop();
            if (playerOnTop && flag)
            {
                sinkTimer = 0.3f;
            }
            else if (sinkTimer > 0f)
            {
                sinkTimer -= Engine.DeltaTime;
            }

            if (sinkTimer > 0f)
            {
                yLerp = Calc.Approach(yLerp, 1f, 3f * Engine.DeltaTime);
            }
            else
            {
                yLerp = Calc.Approach(yLerp, 0f, 3f * Engine.DeltaTime);
            }
            double sinkOffset = (double)8f * (double)Ease.SineInOut(yLerp);
            MoveToY((float)(OrigPosition.Y + sinkOffset));
            LiftSpeed = Vector2.Zero;
            if (flag && yLerp >= 1 && prevLerp < 1)
            {
                EmitKey();
            }
        }
        public void EmitKey()
        {
            foreach (LibraryPressureBlockController c in Scene.Tracker.GetEntities<LibraryPressureBlockController>())
            {
                c.ReceiveKey(key);
            }
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
        }
    }
}