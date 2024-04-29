using Celeste.Mod.CommunalHelper;
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using ExtendedVariants.Entities.ForMappers;
using FrostHelper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Flora
{
    [CustomEntity("PuzzleIslandHelper/Flungus")]
    [Tracked]
    public class Flungus : Entity
    {
        public class Head
        {

        }
        public Flungus(EntityData data, Vector2 offset) : base(data.Position + offset)
        {

        }
    }
}
