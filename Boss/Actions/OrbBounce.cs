using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using static Celeste.Mod.PuzzleIslandHelper.Boss.ActionRegistry;
using static Celeste.Mod.PuzzleIslandHelper.Entities.Singularity.SingularityBoss;

namespace Celeste.Mod.PuzzleIslandHelper.Boss.Actions
{
    public class OrbBounce : OrbActionRegistryHandler
    {
        public override string Name => "orbbounce";
        public override bool Dummy => false;
        public Vector2? Direction;
        public float Speed;
        public int Bounces;
        public override void Parse(XmlActionData dict)
        {
            Speed = dict.Get<float>("speed", 40f);
            bool followPlayer = dict.GetBool("followPlayer", false);
            if (!followPlayer)
            {
                Direction = dict.GetVector2("x", "y", Vector2.UnitY);
            }
            Bounces = dict.Get<int>("bounces", 2);
            RequiredOrbs = dict.Get<int>("orbs", 1);
        }
        public override void BeginOrb(Orb orb)
        {
            orb.OnBounceEnd += Orb_OnBounceEnd;
            orb.Bounce(Bounces, Speed, 1, true, null);
        }

        private void Orb_OnBounceEnd(Orb obj)
        {
            FinishedOrbs.Add(obj);
            obj.OnBounceEnd -= Orb_OnBounceEnd;
        }

        public override void UpdateOrb(Orb orb)
        {
        }

        public override bool ContinueToNextActionOrb(Orb orb)
        {
            return FinishedOrbs.Contains(orb);
        }
    }
}
