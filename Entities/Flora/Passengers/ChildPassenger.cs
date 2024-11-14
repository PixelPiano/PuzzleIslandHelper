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
    [CustomEntity("PuzzleIslandHelper/Passengers/Child")]
    [Tracked]
    public class ChildPassenger : VertexPassenger
    {
        public ChildPassenger(EntityData data, Vector2 offset) : base(data.Position + offset, 12, 20, data.Attr("cutsceneID"), new(12, 20), new(-1, 1), 0.95f)
        {
            MinWiggleTime = 1;
            MaxWiggleTime = 2.5f;

            AddTriangle(new(0.1f, 0), new(0.8f, 0.2f), new(0.1f, 0.6f), 1, Vector2.One, new(1.4f, Ease.Linear, Color.DarkGreen, Color.DarkOliveGreen, Color.DarkOliveGreen));
            AddTriangle(new(0.8f, 0.25f), new(1f, 0.75f), new(0f, 0.75f), 0.5f, Vector2.One, new(1.2f, Ease.Linear, Color.Green, Color.DarkSeaGreen, Color.DarkSeaGreen));
            AddTriangle(new(0.2f, 0.8f), new(0.3f, 1), new(0.1f, 1), 0, Vector2.One, new(0.9f, Ease.Linear, Color.DarkGreen, Color.DarkOliveGreen, Color.DarkSeaGreen));
            AddTriangle(new(0.8f, 0.8f), new(0.9f, 1), new(0.7f, 1), 0, Vector2.One, new(0.9f, Ease.Linear, Color.DarkGreen, Color.DarkOliveGreen, Color.DarkSeaGreen));
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Bake();
        }
    }
}
