using Celeste.Mod.CommunalHelper;
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Flora.Passengers
{
    [CustomEntity("PuzzleIslandHelper/Passengers/Old")]
    [Tracked]
    public class OldPassenger : VertexPassenger
    {
        public OldPassenger(EntityData data, Vector2 offset) : base(data.Position + offset, 16, 20, data.Attr("cutsceneID"), new(16, 20), new(-1, 1), 1.6f)
        {
            MinWiggleTime = 2;
            MaxWiggleTime = 4f;
            AddTriangle(new(0.1f, 0.45f), new(0.47f, 0f), new(0.65f, 0.45f), 1, Vector2.One, new(1.4f, Ease.Linear, Color.LightGreen, Color.DarkOliveGreen, Color.DarkOliveGreen));
            AddTriangle(new(0.5f, 0f), new(1f, 0f), new(0.75f, 0.45f), 0.5f, Vector2.One, new(1.2f, Ease.Linear, Color.LightGreen, Color.DarkOliveGreen, Color.DarkOliveGreen));
            AddTriangle(new(1f, 0f), new(1f, 1), new(0.4f, 1), 0, Vector2.One, new(0.9f, Ease.Linear, Color.DarkGreen, Color.DarkOliveGreen, Color.DarkSeaGreen));
            AddTriangle(new(0.2f, 1), new(0.75f, 0.4f), new(0.4f, 1), 0, Vector2.Zero);
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Bake();
        }
    }
}
