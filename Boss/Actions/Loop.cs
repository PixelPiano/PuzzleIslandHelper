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
    public class Loop : ActionRegistryHandler
    {
        public override string Name => "loop";
        public override bool Dummy => false;
        public int MaxLoops;
        public int Health;
        public int DamageReceived;
        private int loopsRan;
        public List<ActionInfo> Actions = [];
        private string log = "";
        public override void Parse(XmlActionData xml)
        {
            MaxLoops = xml.Get<int>("maxloops", -1);
            Health = xml.Get<int>("health", -1);
            log += "Children = " + xml.Children.Count;
            for (int i = 0; i < xml.Children.Count; i++)
            {
                XmlNode node = xml.Children[i].Node;

                if (node is XmlElement element)
                {
                    log += "\nnode \"" + node.Name + "\" is XmlElement!";
                    if (TryBuildActionInfoFromXmlElement(element, true, out ActionInfo info))
                    {
                        Actions.Add(info);
                        bool handlerNull = info.Handler == null;
                        log += "\nSuccessfully built ActionInfo from XmlElement \""+element.Name+"\"";
                        log += handlerNull ? "...But the handler is null." : "!";
                        log += "\n" + info.ToString();
                    }
                    else
                    {
                        log += "\n could not build ActionInfo from XmlElement \"" + element.Name + "\"";
                        log += "\nActionInfoError:" + TryBuildActionErrorCode;
                    }
                }
                else
                {
                    log += "\nNode \"" + node.Name + "\" is not an XmlElement!";
                }
            }
        }
        public override void Update(Singularity s)
        {
            base.Update(s);
        }
        public bool ContinueLoop(Singularity s)
        {
            if (Health > 0 && DamageReceived >= Health) return false;
            if (MaxLoops >= 0 && loopsRan >= MaxLoops) return false;
            return true;
        }
        public override bool ContinueToNextAction(Singularity s)
        {
            return !ContinueLoop(s);
        }
        private IEnumerator regularRoutine(Singularity s)
        {
            if (Actions.Count == 0)
            {
                Engine.Commands.Log("No Actions In Loop!!!", Color.Red);
            }
            while (true)
            {
                foreach (ActionInfo action in Actions)
                {
                    if (action.Handler != this)
                    {
                        action.Handler.OnUpdate += Update;
                        yield return new SwapImmediately(action.Handler.Routine(s));
                        action.Handler.OnUpdate -= Update;
                    }
                }
                loopsRan++;
                yield return null;
            }
        }
        public override IEnumerator Routine(Singularity s)
        {
            Coroutine routine = new Coroutine(regularRoutine(s), true);
            s.Add(routine);
            Begin(s);
            while (ContinueLoop(s))
            {
                yield return null;
            }
            routine.RemoveSelf();
            End(s, false);
        }
        public void OnTakeDamage()
        {
            DamageReceived++;
        }
        public override void Begin(Singularity s)
        {
            Engine.Commands.Log(log);
            s.OnTakeDamage += OnTakeDamage;
        }
        public override void End(Singularity s, bool wasSkipped)
        {
            s.OnTakeDamage -= OnTakeDamage;
        }
    }
}
