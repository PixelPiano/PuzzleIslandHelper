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
    public class Wait : ActionRegistryHandler
    {
        public override string Name => "wait";
        public override bool Dummy => false;
        private float? time;
        private float timer;
        public override void Parse(XmlActionData dict)
        {
            time = dict["time"].AsT<float>();
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
            timer = time ?? 0;
        }
    }
}
