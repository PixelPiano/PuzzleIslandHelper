using Celeste.Mod.CommunalHelper.Utils;
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.DEBUG;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Threading;
using TAS;
using static Celeste.Mod.PuzzleIslandHelper.Entities.FlowTexture;
using static Celeste.MoonGlitchBackgroundTrigger;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Tower
{
    [CustomEntity("PuzzleIslandHelper/TowerCutsceneTalker")]
    [Tracked]
    public class TowerCutsceneActivator : Entity
    {
        public TalkComponent Talk;
        public string TeleportTo;
        public TowerCutsceneActivator(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            TeleportTo = data.Attr("teleportTo");
            Collider = new Hitbox(data.Width, data.Height);
            Add(Talk = new TalkComponent(new Rectangle(0, 0, data.Width, data.Height), Vector2.UnitX * data.Width / 2, p =>
            {
                if (Scene.Tracker.GetEntity<Eye>() is Eye t)
                {
                    Scene.Add(new OrbGetCutscene(t, p));
                }
            })
            { PlayerMustBeFacing = false });
        }
        public override void Update()
        {
            base.Update();
            Eye eye = Scene.Tracker.GetEntity<Eye>();
            Talk.Enabled = eye != null && !(eye.Type == Eye.Types.Backend && OrbFlags.BlueCollected) && !(eye.Type == Eye.Types.Transit && OrbFlags.GreenCollected);
        }
    }
}