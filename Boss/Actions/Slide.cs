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
using System.Linq;
using System.Reflection;
using System.Xml;
using static Celeste.Mod.PuzzleIslandHelper.Boss.ActionRegistry;
using static Celeste.Mod.PuzzleIslandHelper.Entities.Singularity;

namespace Celeste.Mod.PuzzleIslandHelper.Boss.Actions
{
    public class Slide : ActionRegistryHandler
    {
        public override string Name => "slide";
        public override bool Dummy => false;
        private string marker;
        private Vector2 direction;
        private float speed;
        private Ease.Easer ease;
        private float easeTime;
        public override void Parse(ActionRegistry.XmlActionData dict)
        {
            marker = XMLParse.GetString(dict, "marker", "");
            speed = XMLParse.Get(dict, "speed", 40f);
            ease = dict.GetEase("ease", Ease.CubeOut);
            direction = dict.GetVector2("dirX", "dirY", Vector2.UnitX);
            easeTime = XMLParse.Get<float>(dict, "easeTime", -1);
        }
        public override void Begin(Singularity s)
        {
            
        }
        public override bool ContinueToNextAction(Singularity s)
        {
            return s.StateMachine.State != StSlam;
        }
    }
}
