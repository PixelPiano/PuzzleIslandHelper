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
    public class Choice : ActionRegistryHandler
    {
        public override string Name => "choice";
        public override bool Dummy => false;
        public List<ActionInfo> Choices = [];
        public ActionRegistryHandler Selected;
        private XmlActionData data;
        public override void Parse(XmlActionData xml)
        {
            data = xml;
        }
        public override void Update(SingularityBoss s)
        {
            base.Update(s);
            Selected?.Update(s);
        }
        public override bool ContinueToNextAction(SingularityBoss s)
        {
            return Selected?.ContinueToNextAction(s) ?? true;
        }
        public override void Begin(SingularityBoss s)
        {
            Choices.Clear();
            string log = "Choice: ";
            for (int i = 0; i < data.Children.Count; i++)
            {
                XmlNode node = data.Children[i].Node;
                log += node.Name + ", ";
            }
            Engine.Commands.Log(log, Color.Cyan);
            log = "Options: ";
            for (int i = 0; i < data.Children.Count; i++)
            {
                XmlNode node = data.Children[i].Node;
                if (node is XmlElement element)
                {
                    if (TryBuildActionInfoFromXmlElement(element, true, out ActionInfo info))
                    {
                        log += info.Name + ", ";
                        Choices.Add(info);
                    }
                }
                else
                {
                    log += "NOT AN ELEMENT";
                }
            }
            Engine.Commands.Log(log, Color.Yellow);
            if (!(Choices == null || Choices.Count == 0))
            {
                int rand = Calc.Random.Range(0, Choices.Count);
                ActionInfo selectedInfo = Choices[rand];

                Selected = selectedInfo.Handler;
                Selected.Begin(s);
                Engine.Commands.Log("Selected: " + (Selected.Name + "(" + rand + ")"), Color.Lime);
            }
            else
            {
                Engine.Commands.Log("Selected: null", Color.Red);
            }
        }
        public override void End(SingularityBoss s, bool wasSkipped)
        {
            Selected?.End(s, wasSkipped);
        }
    }
}
