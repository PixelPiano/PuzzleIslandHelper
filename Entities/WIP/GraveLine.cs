using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.WIP
{
    [CustomEntity("PuzzleIslandHelper/GraveLine")]
    [Tracked]
    public class GraveLine : Entity
    {
        public static MTexture StoneTexture => GFX.Game["objects/PuzzleIslandHelper/obsoleteStone/stone"];
        public static MTexture IconTexture => GFX.Game["objects/PuzzleIslandHelper/obsoleteStone/icon"];
        public List<LineStone> Graves = new();
        public float YSpeed = 50f;
        public GraveLine(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Depth = 1;
            Collider = new Hitbox(StoneTexture.Width, data.Height);

        }

        [TrackedAs(typeof(LineStone))]
        public class LineStone : ObsoleteStone
        {
            public Image Icon;
            public float LinePercent;
            public Color LimeColor;
            public LineStone(Vector2 position) : base(position, false)
            {
                Icon = new Image(IconTexture);
                Icon.Position = new Vector2(-8, 10);
                Add(Icon);
                Icon.Color = Color.Lime;
            }
            public override void Render()
            {
                Cycler.Color = Stone.Color = Color.Lerp(Color.Black, Color.White, LinePercent);
                Icon.Color = Color.Lerp(Color.Black, Color.Lime, LinePercent);
                base.Render();
                Vector2 p = Icon.RenderPosition + Icon.Size() - Vector2.UnitY;
                Draw.Line(p, p + Vector2.UnitX * 16, Icon.Color);
            }
            public override void Update()
            {
                base.Update();

            }

        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            for (float i = 0; i < Height - StoneTexture.Height + 3; i += StoneTexture.Height + 3)
            {
                LineStone stone = new LineStone(Position + Vector2.UnitY * i);
                Graves.Add(stone);
                scene.Add(stone);
            }
        }
        public override void Update()
        {
            base.Update();
            foreach (LineStone grave in Graves)
            {
                grave.Position.Y += (float)Math.Round(YSpeed * Engine.DeltaTime);
                if (grave.Top > Bottom)
                {
                    grave.Bottom = Position.Y;
                }
                grave.LinePercent = 1 - (Bottom - grave.Bottom) / Height;
            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            foreach (LineStone grave in Graves)
            {
                grave.RemoveSelf();
            }
        }
    }
}