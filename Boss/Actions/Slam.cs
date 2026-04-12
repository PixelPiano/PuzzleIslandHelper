using Celeste.Mod.PuzzleIslandHelper.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Xml;
using static Celeste.Mod.PuzzleIslandHelper.Boss.ActionRegistry;
using static Celeste.Mod.PuzzleIslandHelper.Entities.Singularity;

namespace Celeste.Mod.PuzzleIslandHelper.Boss.Actions
{
    public class Slam : ActionRegistryHandler
    {
        public override string Name => "slam";
        public override bool Dummy => false;
        public Vector2? Direction;
        public float Speed;
        public int Bounces;
        public override void Parse(ActionRegistry.XmlActionData dict)
        {
            Speed = XMLParse.Get(dict, "speed", 40f);
            bool followPlayer = XMLParse.GetBool(dict, "followPlayer", false);
            if (!followPlayer)
            {
                //Direction = XMLParse.GetVector2(dict, "x", "y", Vector2.UnitY);
            }
            Bounces = XMLParse.Get(dict, "bounces", 2);
        }
        public override void Begin(Singularity s)
        {
            if (Direction.HasValue)
            {
                s.Slam(Direction.Value, Speed, Bounces);
            }
            else
            {
                s.Slam(Speed, Bounces);
            }
        }
        public override bool ContinueToNextAction(Singularity s)
        {
            return s.StateMachine.State != Singularity.StSlam;
        }
    }
}
