using Celeste.Mod.CommunalHelper;
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using ExtendedVariants.Variants;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using static Celeste.Trigger;

namespace Celeste.Mod.PuzzleIslandHelper.Triggers
{

    [CustomEntity("PuzzleIslandHelper/RoomLight")]
    [Tracked]
    public class RoomLight : Trigger
    {
        public RoomLight(EntityData data, Vector2 offset)
    : base(data, offset)
        {
            Tag |= Tags.TransitionUpdate;
        }
        private void CheckLight()
        {
            if(Scene is not Level level) return;
            level.Session.SetFlag("PuzzleIslandRestoredPower", PianoModule.Session.RestoredPower);
            float num = level.Session.LightingAlphaAdd = PianoModule.Session.RestoredPower ? PianoModule.Session.MinDarkness : PianoModule.Session.MaxDarkness;
            level.Lighting.Alpha = level.BaseLightingAlpha + num;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            CheckLight();
        }
        public override void Update()
        {
            base.Update();

            CheckLight();
        }
    }
}
