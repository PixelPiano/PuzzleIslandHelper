using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Xml;
using static Celeste.Mod.PuzzleIslandHelper.Boss.ActionRegistry;
using static Celeste.Mod.PuzzleIslandHelper.Entities.Singularity.SingularityBoss;
using Monocle;
using System.Linq;
using System.Collections.Generic;
using Celeste.Mod.PuzzleIslandHelper.Entities.Singularity;
namespace Celeste.Mod.PuzzleIslandHelper.Boss.Actions
{
    public class OrbActionGroupManager : ActionRegistryHandler
    {
        public override string Name => "orbmanager";
        public override bool Dummy => false;
        public Stack<ActionInfo> Actions = [];
        private List<Coroutine> coroutines = [];

        private List<Orb> getAvailable(SingularityBoss s) => s.Orbs.Where(item => !item.Attacking && !item.QueuedForAttack).ToList();
        public override void Parse(XmlActionData dict)
        {
        }
        public override void Begin(SingularityBoss s)
        {
            while (Actions.Count > 0)
            {
                Coroutine routine = new Coroutine(queueAction(s, Actions.Pop()));
                s.Add(routine);
                coroutines.Add(routine);
            }
        }
        public override void End(SingularityBoss s, bool wasSkipped)
        {
            base.End(s, wasSkipped);
            coroutines.RemoveSelves();
            foreach(Orb orb in s.Orbs)
            {
                orb.Auto();
            }
        }
        
        private IEnumerator queueAction(SingularityBoss s, ActionInfo info)
        {
            OrbActionRegistryHandler handler = info.Handler as OrbActionRegistryHandler ?? throw new Exception("tried to queue up a non orb-exclusive action!");
            List<Orb> orbs = getAvailable(s);
            while (orbs.Count < handler.RequiredOrbs)
            {
                orbs = getAvailable(s);
                yield return null;
            }
            foreach (Orb orb in orbs)
            {
                orb.QueuedForAttack = false;
                orb.Attacking = true;
            }
            foreach (Orb orb in orbs)
            {
                handler.BeginOrb(orb);
            }

            /////////////////////////
            List<Orb> toRemove = [];
            while (orbs.Count > 0)
            {
                foreach (Orb orb in orbs)
                {
                    if (handler.ContinueToNextActionOrb(orb))
                    {
                        toRemove.Add(orb);
                    }
                    else
                    {
                        handler.UpdateOrb(orb);
                    }
                }
                foreach (Orb orb in toRemove)
                {
                    handler.EndOrb(orb);
                    orbs.Remove(orb);
                }
                toRemove.Clear();
                yield return null;
            }
        }

        public override bool ContinueToNextAction(SingularityBoss s)
        {
            if (Actions.Count == 0)
            {
                foreach (Coroutine c in coroutines)
                {
                    if (!c.Finished) return false;
                }
                return true;
            }
            return false;

        }
    }
}
