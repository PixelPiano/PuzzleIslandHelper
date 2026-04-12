using Celeste.Mod.Core;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using FrostHelper.ModIntegration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Xml;
using static Celeste.Mod.PuzzleIslandHelper.Boss.ActionRegistry;
using static Celeste.Mod.PuzzleIslandHelper.Entities.Singularity;

namespace Celeste.Mod.PuzzleIslandHelper.Boss.Actions
{
    public class SpikeSlam : ActionRegistryHandler
    {
        public override string Name => "spikeslam";

        public override bool Dummy => false;

        private int loops;
        private float mult = 1;
        private int length;
        private bool targetPlayer;
        private bool blockWalls;
        private string[] markers;
        private float spikeDuration = 0.6f;
        public override void Parse(XmlActionData dict)
        {
            string m = dict.GetString("targets", "");
            if (!string.IsNullOrEmpty(m))
            {
                markers = m.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            }
            mult = XMLParse.Get<float>(dict, "mult", 1);
            loops = XMLParse.Get(dict, "loops", 0);
            length = XMLParse.Get(dict, "length", 1);
            targetPlayer = dict.GetBool("targetPlayer", false);
            blockWalls = dict.GetBool("blockWalls", false);
            spikeDuration = dict.Get<float>("spikeDuration", 0.6f);
        }
        public override bool ContinueToNextAction(Singularity s)
        {
            return s.StateMachine.State != Singularity.StSpikeSlam;
        }
        public override void Begin(Singularity s)
        {
            s.CallOrbs();
            s.SpikeSlam(markers, loops, length, mult, spikeDuration, targetPlayer, blockWalls);
        }

    }
}
