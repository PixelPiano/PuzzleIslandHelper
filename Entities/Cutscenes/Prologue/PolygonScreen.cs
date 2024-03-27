using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Cutscenes.Prologue
{
    [Tracked]
    public class PolygonScreen : Entity
    {
        public Vector2[] Points;
        public PolygonScreen() : base(Vector2.Zero)
        {

        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);

        }
        public Vector2[] GetScreenPoints(Level level)
        {
            List<Vector2> points = new List<Vector2>();

            return points.ToArray();
        }
    }
}
