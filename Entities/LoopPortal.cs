using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities.Programs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using static Celeste.Overworld;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/LoopPortal")]
    public class LoopPortal : Entity
    {
        public LoopPortal(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Collider = new Hitbox(data.Height, data.Width);
            Rectangle bounds = new Rectangle(0, 0, data.Height, data.Width);
            Add(new TalkComponent(bounds, Collider.HalfSize, (p) =>
            {
                p.DisableMovement();
                Scene.Add(new cutscene(this));
            }));
        }
        private class cutscene : CutsceneEntity
        {
            public LoopPortal Portal;
            public cutscene(LoopPortal portal) : base(true, true)
            {
                Portal = portal;
            }
            public override void OnBegin(Level level)
            {
                Add(new Coroutine(ending()));
            }
            private IEnumerator ending()
            {
                yield return CameraTo(Portal.Center - new Vector2(160, 90), 1, Ease.CubeOut, 0.2f);
                for (int i = 0; i < 14; i++)
                {
                    Pulse.Circle(Portal, Pulse.Fade.Late, Pulse.Mode.Oneshot, Portal.Collider.HalfSize, 0, Portal.Width / 2, 0.4f, true, Color.White, Color.Transparent, Ease.CubeOut);
                    yield return 0.3f;
                }
                EndCutscene(Level);
            }
            public override void OnEnd(Level level)
            {
                level.CompleteArea(true, false);
            }
        }
        public override void Render()
        {
            base.Render();
        }
        public override void Update()
        {
            base.Update();
            if (Scene.OnInterval(0.3f))
            {
                Pulse.Diamond(this, Pulse.Fade.Linear, Pulse.Mode.Oneshot, Collider.HalfSize, 0, Width / 3, 0.9f, true, Color.White, Color.Transparent, null, Ease.CubeOut);
            }

        }
    }
}