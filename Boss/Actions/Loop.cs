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
    public class Loop : ActionRegistryHandler
    {
        public override string Name => "loop";
        public override bool Dummy => false;
        public int MaxLoops;
        public int Health;
        public int DamageReceived;
        private int loopsRan;
        public List<ActionInfo> Actions = [];
        private List<KeyValuePair<string, Color>> logs = [];
        public override void Parse(XmlActionData xml)
        {
            logs.Clear();
            MaxLoops = xml.Get<int>("maxloops", -1);
            Health = xml.Get<int>("health", -1);
            logs.Add(new("--------\nBeginning evaluation of loop with " + xml.Children.Count + " children, Max Loops: " + MaxLoops + ".", Color.Yellow));
            for (int i = 0; i < xml.Children.Count; i++)
            {
                XmlNode node = xml.Children[i].Node;
                string nodeLog = "";
                if (node is XmlElement element)
                {
                    logs.Add(new("\n\tnode \"" + node.Name + "\" is XmlElement!", Color.Yellow));
                    if (TryBuildActionInfoFromXmlElement(element, true, out ActionInfo info))
                    {
                        Actions.Add(info);
                        bool handlerNull = info.Handler == null;
                        logs.Add(new("\tSuccessfully built ActionInfo from XmlElement \"" + element.Name + "\"" + (handlerNull ? "...But the handler is null." : "!"), handlerNull ? Color.DarkRed : Color.Lime));
                        logs.Add(new(info.ToString(), Color.Gray));
                    }
                    else
                    {
                        nodeLog += "\t could not build ActionInfo from XmlElement \"" + element.Name + "\"";
                        nodeLog += "\tActionInfoError:" + TryBuildActionErrorCode;
                        logs.Add(new(nodeLog, Color.Red));
                    }
                }
                else
                {
                    logs.Add(new("\tNode \"" + node.Name + "\" is not an XmlElement!", Color.Red));
                }
            }
        }
        public override void Update(SingularityBoss s)
        {
            base.Update(s);
        }
        public bool ContinueLoop(SingularityBoss s)
        {
            if (Health > 0 && DamageReceived >= Health) return false;
            if (MaxLoops >= 0 && loopsRan >= MaxLoops) return false;
            return true;
        }
        public override bool ContinueToNextAction(SingularityBoss s)
        {
            return !ContinueLoop(s);
        }
        private IEnumerator regularRoutine(SingularityBoss s)
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
                        OnUpdate += action.Handler.Update;
                        yield return new SwapImmediately(action.Handler.Routine(s));
                        OnUpdate -= action.Handler.Update;
                    }
                }
                Engine.Commands.Log("Loop #" + (loopsRan + 1) + " Complete", Color.Red);
                loopsRan++;
                yield return null;
                foreach (ActionInfo action in Actions)
                {
                    if (action.Handler != this)
                    {
                        action.Handler.Reset(s);
                    }
                }
            }
        }
        public override IEnumerator Routine(SingularityBoss s)
        {
            Coroutine routine = new Coroutine(regularRoutine(s), true);
            s.Add(routine);
            Begin(s);
            while (ContinueLoop(s))
            {
                yield return null;
            }

            routine.Cancel();
            routine.RemoveSelf();
            End(s, false);
        }
        public void OnTakeDamage()
        {
            DamageReceived++;
        }
        public override void Begin(SingularityBoss s)
        {
            DamageReceived = 0;
            loopsRan = 0;
            foreach (var p in logs)
            {
                Engine.Commands.Log(p.Key, p.Value);
            }
            s.OnTakeDamage += OnTakeDamage;
        }
        public override void End(SingularityBoss s, bool wasSkipped)
        {
            s.OnTakeDamage -= OnTakeDamage;
        }
    }
}
