using Celeste.Mod.Entities;
using Celeste.Mod.Meta;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Components.Visualizers;
using Celeste.Mod.PuzzleIslandHelper.Components.Visualizers.DSPs;
using Celeste.Mod.PuzzleIslandHelper.Cutscenes.GameshowEntities;
using Celeste.Mod.PuzzleIslandHelper.Entities.GameplayEntities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System.Collections;
using System.Drawing.Design;
using System.Linq;
using System.Linq.Expressions;
using VivHelper;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/MemoryPhone")]
    [Tracked]
    public class MemoryPhone : Entity
    {
        public enum PhoneTypes
        {
            Can,
            Payphone,
            Regular,
            Modern
        }
        public PhoneTypes PhoneType;
        public MemoryPhone(EntityData data, Vector2 offset) : base(data.Position + offset)
        {

        }
    }

}
