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
    public class Choice : ActionRegistryHandler
    {
        public override string Name => "choice";
        public override bool Dummy => false;
        public List<ActionInfo> Choices = [];
        public ActionRegistryHandler Selected;
        private string log = "";
        private XmlActionData data;
        public override void Parse(XmlActionData xml)
        {
            data = xml;
        }
        public override void Update(Singularity s)
        {
            base.Update(s);
            Selected?.Update(s);
        }
        public override bool ContinueToNextAction(Singularity s)
        {
            return Selected?.ContinueToNextAction(s) ?? true;
        }
        public override void Begin(Singularity s)
        {
            string log = "Choice: ";
            for (int i = 0; i < data.Children.Count; i++)
            {
                XmlNode node = data.Children[i].Node;
                log += node.Name + ", ";
            }
            Engine.Commands.Log(log, Color.Cyan);
            log = "Added: ";
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
            }
            Engine.Commands.Log(log, Color.Yellow);
            if (!(Choices == null || Choices.Count == 0))
            {
                Selected = Choices.Random().Handler;
                Selected.Begin(s);
            }
            Engine.Commands.Log("Selected: " + Selected?.Name ?? "null", Color.Lime);
        }
        public override void End(Singularity s, bool wasSkipped)
        {
            Selected?.End(s, wasSkipped);
        }
    }
}
