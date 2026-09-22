using Celeste.Mod.Core;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using Celeste.Mod.PuzzleIslandHelper.Entities.Singularity;
using FrostHelper.ModIntegration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using static Celeste.Mod.PuzzleIslandHelper.Boss.ActionRegistry;
using static Celeste.Mod.PuzzleIslandHelper.Entities.Singularity.SingularityBoss;

namespace Celeste.Mod.PuzzleIslandHelper.Boss.Actions
{
    public class Idle : ActionRegistryHandler
    {
        public override string Name => "idle";
        public override bool Dummy => false;
        private float speedMult;
        private string[] markers;
        private float delay;
        private float timer;
        private float time;
        public override void Parse(XmlActionData xml)
        {
            time = timer = xml.Get<float>("time", -1);
            speedMult = xml.Get<float>("speedMult", 1);
            delay = xml.Get<float>("delay", -1);
            string m = xml.GetString("markers", "");
            if (!string.IsNullOrEmpty(m))
            {
                markers = m.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            }
        }
        public override void Update(SingularityBoss s)
        {
            base.Update(s);
            timer -= Engine.DeltaTime;
        }
        public override bool ContinueToNextAction(SingularityBoss s)
        {
            return timer <= 0;
        }
        public override void Begin(SingularityBoss s)
        {
            timer = time;
            if (markers != null && markers.Length > 0)
            {
                s.Idle(delay, speedMult, markers);
            }
            else
            {
                s.Idle();
            }
        }
    }
}
