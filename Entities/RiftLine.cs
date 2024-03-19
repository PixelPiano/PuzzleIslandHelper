using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/RiftLine")]
    [Tracked]
    public class RiftLine : Entity
    {
        private bool OnScreen;
        private Vector2 Node;
        private Vector2 Start;
        private Vector2 End;

        public RiftLine(EntityData data, Vector2 offset)
        : base(data.Position + offset)
        {
            Tag |= Tags.TransitionUpdate;
            Node = data.Nodes[0] + offset;
            Depth = -10001;

        }
        public override void Update()
        {
            base.Update();
            if (Scene is not Level level) return;
            Camera camera = level.Camera;
            Rectangle c = new Rectangle((int)camera.X, (int)camera.Y, 320, 180);
            OnScreen = Collide.RectToLine(c, Start, End);
        }
       
        public override void Render()
        {
            base.Render();

            if (OnScreen)
            {
                Draw.Line(Position, Node, Color.Green, 3);
            }
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Draw.Line(Position, Node, Color.Yellow);
        }
        private bool InView()
        {
            if (!Visible)
            {
                return true;
            }
            Camera camera = (Scene as Level).Camera;
            Rectangle c = new Rectangle((int)camera.X, (int)camera.Y, 320, 180);
            OnScreen = Collide.RectToLine(c, Start, End);
            return OnScreen;
        }
    }
}