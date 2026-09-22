using Celeste.Mod.Core;
using Celeste.Mod.PuzzleIslandHelper.Boss.Actions;
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using static Celeste.Mod.PuzzleIslandHelper.Boss.ActionRegistry;
using static Celeste.Mod.PuzzleIslandHelper.Entities.Singularity.SingularityBoss;

namespace Celeste.Mod.PuzzleIslandHelper.Boss
{
    public static class XmlActionDataExt
    {
        public static T Get<T>(this XmlActionData xml, string attr, T def) where T : struct, IParsable<T> => xml[attr]?.AsT<T>() ?? def;
        public static string GetString(this XmlActionData xml, string attr, string def) => xml[attr]?.Value ?? def;
        public static bool GetBool(this XmlActionData xml, string attr, bool def) => xml[attr]?.AsBool() ?? def;
        public static TParsable? GetNullable<TParsable>(this XmlActionData xml, string attr) where TParsable : struct, IParsable<TParsable> => xml[attr]?.AsNullable<TParsable>();
        public static Vector2 GetVector2(this XmlActionData xml, string attrX, string attrY, Vector2 def)
        {
            XmlNode nodeX = xml[attrX];
            XmlNode nodeY = xml[attrY];
            if (nodeX != null && nodeY != null) return new Vector2(nodeX.AsT<float>(), nodeY.AsT<float>());
            return def;
        }
        public static Ease.Easer GetEase(this XmlActionData xml, string attr, Ease.Easer def) => xml[attr]?.AsEase() ?? def ?? Ease.Linear;
    }
    public static class ActionRegistry
    {
        internal static Dictionary<string, Func<ActionRegistryHandler>> ActionTypeFactories { get; set; } = new Dictionary<string, Func<ActionRegistryHandler>>();
        public static readonly Dictionary<string, ActionInfo> RegisteredActions = new Dictionary<string, ActionInfo>();
        public static readonly Dictionary<string, PatternInfo> RegisteredPatterns = new Dictionary<string, PatternInfo>();
        public static bool PrintDebugInCommandsConsole = true;
        [Command("print_actions", "")]
        public static void PrintActions()
        {
            Engine.Commands.Log("-------");
            Engine.Commands.Log("All Registered Actions");
            Engine.Commands.Log("-------");
            foreach (var a in RegisteredActions)
            {
                Engine.Commands.Log("|" + a.Value, Color.Cyan);
            }
            Engine.Commands.Log("-------");
        }
        [Command("print_patterns", "")]
        public static void PrintPatterns()
        {
            Engine.Commands.Log("All Registered Patterns");
            Engine.Commands.Log("=======");
            foreach (var a in RegisteredPatterns)
            {
                Engine.Commands.Log("||" + a.Value.ToString(), Color.DeepPink);
            }
            Engine.Commands.Log("=======");
        }
        public struct ActionInfo
        {
            public string Name;
            public string Type;
            public XmlNode Base;
            public XmlActionData Attributes;
            public bool IsOrbAction;
            public bool WaitForEndIfOrb;
            public ActionInfo()
            {
            }
            public void CloneFrom(XmlNode node)
            {
                Attributes = new XmlActionData(node.Attributes);
                Base = node.CloneNode(true);
            }
            public XmlAttributeCollection DefaultCollection;
            internal ActionRegistryHandler Handler { get; set; }
            internal ActionInfo[] OrbActions;
            public override string ToString()
            {
                string output = Name;
                foreach (var pair in Attributes)
                {
                    output += '(' + pair.Key + " = " + pair.Value.Value + "), ";
                }

                return output;
            }
        }
        public struct PatternInfo
        {
            public string Name;
            public int Health;
            public string Next;
            public XmlNode Base;
            public ActionInfo[] Actions;
            public IEnumerator Routine(SingularityBoss s)
            {
                if (Actions.Length == 0)
                {
                    Engine.Commands.Log("Pattern has zero actions!", Color.Red);
                    yield break;
                }
                s.CurrentPattern = this;
                /*
                 * Main Action 1
                 *  Orb Action 1
                 *  Orb Action 2
                 *  Orb Action 3
                 * Main Action 2
                 * -----
                 * main action 1 plays
                 * Orb Action 1, 2 and 3 are queued up and await their required number of orbs.
                 * Once all Orb Actions are completed, continue to Main Action 2.
                 */
                foreach (ActionInfo info in Actions)
                {
                    yield return new SwapImmediately(info.Handler.Routine(s));
                }
                s.CurrentPattern = null;
            }
            public override string ToString()
            {
                string output = "[Pattern: " + Name + " -> Actions]\n";
                foreach (ActionInfo action in Actions)
                {
                    output += "[" + action.ToString() + "]\n";
                }
                return output;
            }
        }
        private static string test = "";
        [Command("print_test", "")]
        public static void PrintTest()
        {
            Engine.Commands.Log(test, Color.Cyan);
        }
        private static bool TryReadMovesetXml(ModAsset moveSetRegistry)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(moveSetRegistry.Stream);
            XmlElement root = xmlDocument["moveset"];
            if (root == null)
            {
                errorCode = "SingularityMoveset.xml: does not have a \"moveset\" root node.";
                return false;
            }
            XmlElement actions = root["actions"];
            if (actions == null)
            {
                errorCode = "SingularityMoveset.xml: \"moveset\" does not have an \"actions\" child node.";
                return false;
            }
            foreach (KeyValuePair<string, ActionInfo> item in ReadActionXmlElement(actions))
            {
                string key = item.Key;
                ActionInfo value = item.Value;
                RegisterDefaultAction(key, value);
            }
            XmlElement patterns = root["patterns"];
            if (patterns != null)
            {
                foreach (KeyValuePair<string, PatternInfo> item in ReadPatternXmlElement(patterns))
                {
                    string key = item.Key;
                    PatternInfo value = item.Value;
                    RegisterPattern(key, value);
                }
            }
            return true;
        }
        public static string TryBuildActionErrorCode = "";
        public static bool TryBuildActionInfoFromXmlElement(XmlElement element, bool pullFromRegistered, out ActionInfo info)
        {
            TryBuildActionErrorCode = "";
            info = default;
            string actionName = element.Attr("name", null)?.ToLower(); //the name of the action (i.e "SlamEasy", "ShockwaveSingleOrb")
            string actionType = element.Attr("type", null)?.ToLower(); //the type of action (dictated by entity states) (i.e "Slam", "Shockwave")
            if (actionName == null)
            {
                //search for the header name in registered actions and replace the handler
                if (pullFromRegistered)
                {
                    if (RegisteredActions.TryGetValue(element.Name, out ActionInfo foundinfo))
                    {
                        //default values from the attack preset definition
                        //create a new attribute dictionary by cloning all nodes so the original data is preserved.
                        //add/override any defined node in the preset instance.
                        XmlActionData newAttributes = new(element, foundinfo.DefaultCollection);
                        if (test.Length > 0) test += '\n';
                        test += newAttributes.ToString();
                        var handler = CreateHandlerOrNullFromActionType(foundinfo.Type, newAttributes);
                        if (handler == null)
                        {
                            TryBuildActionErrorCode = "Action type passed in doesn't exist!";
                            return false;
                        }

                        info.Name = foundinfo.Name;
                        info.Type = foundinfo.Type;
                        info.Attributes = newAttributes;
                        info.DefaultCollection = foundinfo.DefaultCollection;
                        info.Handler = CreateHandlerOrNullFromActionType(foundinfo.Type, info.Attributes);
                        return true;
                    }
                    else
                    {
                        TryBuildActionErrorCode = "Action \"" + element.Name + "\" not found in registered actions.";
                    }
                }
                else
                {
                    TryBuildActionErrorCode = "Attribute \"name\" not found in default action element \"" + element.Name + "\".";
                }
                return false;
            }
            else if (actionType == null)
            {
                TryBuildActionErrorCode = "Attribute \"type\" not found in default action element \"" + element.Name + "\".";
                return false;
            }
            info.Attributes = new XmlActionData(element.Attributes);
            info.DefaultCollection = element.Attributes;
            info.Type = actionType;
            info.Name = actionName;
            return true;
        }
        public static bool TryBuildPatternInfoFromXmlElement(XmlElement pattern, out PatternInfo info)
        {
            info = default;
            List<ActionInfo> actions = [];
            string name = pattern.Attr("name", null)?.ToLower();
            if (name == null) return false;
            foreach (XmlNode actionNode in pattern)
            {
                if (actionNode is XmlElement action)
                {
                    if (TryBuildActionInfoFromXmlElement(action, true, out ActionInfo actionInfo))
                    {
                        actions.Add(actionInfo);
                    }
                }
            }
            info.Health = pattern.AttrInt("health", -1);
            info.Next = pattern.Attr("next", "");
            info.Name = name;
            info.Actions = [.. actions];

            return true;
        }
        private static List<KeyValuePair<string, ActionInfo>> ReadActionXmlElement(XmlElement actionsElement)
        {
            /* Pattern
             * Attacks
             *      .Attacks
             * EndPattern
             * 
             */
            List<KeyValuePair<string, ActionInfo>> list = [];
            if (actionsElement == null)
            {
                return list;
            }
            foreach (XmlNode action in actionsElement)
            {
                if (!(action is XmlElement xmlElement2) ||
                    !TryBuildActionInfoFromXmlElement(xmlElement2, false, out ActionInfo info) ||
                    list.Exists(item => item.Key == info.Name))
                {
                    continue;
                }
                list.Add(new(info.Name, info));
            }
            return list;
        }
        private static List<KeyValuePair<string, PatternInfo>> ReadPatternXmlElement(XmlElement patternsElement)
        {
            List<KeyValuePair<string, PatternInfo>> list = [];
            if (patternsElement == null)
            {
                return list;
            }
            foreach (XmlNode pattern in patternsElement)
            {
                if (!(pattern is XmlElement xmlElement2) ||
                    !TryBuildPatternInfoFromXmlElement(xmlElement2, out PatternInfo info) ||
                    list.Exists(item => item.Key == info.Name))
                {
                    continue;
                }
                list.Add(new(info.Name, info));
            }
            return list;
        }
        private static string errorCode;
        private static string actionRegisterLog = "";
        private static string patternRegisterLog = "";
        private static void RegisterDefaultAction(string actionName, ActionInfo info)
        {
            if (info.Handler == null)
            {
                ActionRegistryHandler actionRegistryHandler2 = CreateHandlerOrNullFromActionType(info.Type, new XmlActionData(info.DefaultCollection));
                if (actionRegistryHandler2 != null)
                {
                    info.Handler = actionRegistryHandler2;
                }
            }
            actionRegisterLog += "\nRegistered actions \"" + actionName + "\".";
            RegisteredActions[actionName] = info;
        }
        private static void RegisterPattern(string patternName, PatternInfo info)
        {
            for (int i = 0; i < info.Actions.Length; i++)
            {
                if (info.Actions[i].Handler == null)
                {
                    ActionRegistryHandler actionRegistryHandler2 = CreateHandlerOrNullFromActionType(info.Actions[i].Type, info.Actions[i].Attributes);
                    if (actionRegistryHandler2 != null)
                    {
                        info.Actions[i].Handler = actionRegistryHandler2;
                    }
                }
            }
            patternRegisterLog += "\nRegistered pattern \"" + patternName + "\".";
            RegisteredPatterns[patternName] = info;
        }
        [Command("print_register_logs", "")]
        public static void PrintRegisterLogs()
        {
            Engine.Commands.Log(actionRegisterLog, Color.LightGreen);
            Engine.Commands.Log(patternRegisterLog, Color.LightGreen);
            if (!string.IsNullOrEmpty(errorCode))
            {
                Engine.Commands.Log(errorCode, Color.Red);
            }
        }
        public static void AddPropertyHandler<T>() where T : ActionRegistryHandler, new()
        {
            T val = new T();
            ActionTypeFactories[val.Name] = () => new T();
        }

        public class XmlActionData : List<KeyValuePair<string, XmlAttribute>>
        {
            public XmlNode Node;
            public List<XmlActionData> Children = [];
            public XmlAttribute this[string s]
            {
                get
                {
                    foreach (KeyValuePair<string, XmlAttribute> pair in this)
                    {
                        if (pair.Key == s)
                        {
                            return pair.Value;
                        }
                    }
                    return null;
                }
                set
                {
                    for (int i = 0; i < Count; i++)
                    {
                        if (this[i].Key == s)
                        {
                            this[i] = new KeyValuePair<string, XmlAttribute>(s, value);
                            return;
                        }
                    }
                    Add(new(s, value));
                }
            }
            public XmlActionData(XmlNode node, XmlAttributeCollection defaultCollection) : base()
            {
                AddCollectionAsClones(defaultCollection);
                Node = node.Clone();
                CombineAttributesFrom(node);
                if (Node.HasChildNodes)
                {
                    foreach (XmlNode child in Node.ChildNodes)
                    {
                        if (RegisteredActions.TryGetValue(child.Name, out ActionInfo childAction))
                        {
                            Children.Add(new XmlActionData(child, childAction.DefaultCollection));
                        }
                    }
                }
            }
            public XmlActionData(XmlAttributeCollection collection) : base()
            {
                AddCollectionAsClones(collection);
            }
            public void AddCollectionAsClones(XmlAttributeCollection collection)
            {
                foreach (XmlNode node in collection)
                {
                    if (node is XmlAttribute attr)
                    {
                        Add(new(attr.Name, attr.Clone() as XmlAttribute));
                    }
                }
            }

            public void CombineAttributesFrom(XmlNode node)
            {
                if (node != null)
                {
                    foreach (XmlNode node2 in node.Attributes)
                    {
                        XmlNode clonedNode = node2.Clone();
                        if (clonedNode is XmlAttribute attribute)
                        {
                            this[attribute.Name] = attribute;
                        }
                    }
                }
            }
            public override string ToString()
            {
                string output = "";
                output += Node.Name + " -> ";

                bool split = false;
                foreach (var a in this)
                {
                    if (split) output += ", ";
                    output += "[" + a.Key + " = " + a.Value.Value + "]";
                    split = true;
                }
                output += " (Children = " + Children.Count + ")";
                foreach (XmlActionData child in Children)
                {
                    output += "\n\t" + child.ToString();
                }
                return output;
            }
        }
        internal static ActionRegistryHandler CreateHandlerOrNullFromActionType(string actionType, XmlActionData data)
        {
            if (!ActionTypeFactories.TryGetValue(actionType, out var value))
            {
                return null;
            }
            ActionRegistryHandler actionRegistryHandler = value();
            actionRegistryHandler.Parse(data);
            return actionRegistryHandler;
        }
        public abstract class ActionRegistryHandler
        {
            public abstract string Name { get; }
            public abstract bool Dummy { get; }
            public event Action<SingularityBoss> OnUpdate;
            public event Action<SingularityBoss> OnBegin;
            public event Action<SingularityBoss> OnEnd;
            public virtual void SetValue(string name, string value)
            {
                Type t = GetType();
                BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                try
                {
                    foreach (var f in t.GetFields(flags))
                    {
                        if (name.Trim().ToLower() == f.Name.Trim().ToLower())
                        {
                            Type ftype = f.FieldType;
                            if (ftype == typeof(bool))
                            {
                                f.SetValue(this, bool.Parse(value), BindingFlags.SetField, null, null);
                            }
                            else if (ftype == typeof(int))
                            {
                                f.SetValue(this, int.Parse(value), BindingFlags.SetField, null, null);
                            }
                            else if (ftype == typeof(float))
                            {
                                f.SetValue(this, float.Parse(value), BindingFlags.SetField, null, null);
                            }
                            else if (ftype == typeof(string))
                            {
                                f.SetValue(this, value, BindingFlags.SetField, null, null);
                            }
                            else if (ftype == typeof(string[]))
                            {
                                f.SetValue(this, value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries), BindingFlags.SetField, null, CultureInfo.InvariantCulture);
                            }
                        }
                    }
                }
                catch
                {
                    Engine.Commands.Log("whuh oh");
                }
            }
            public void PrintValues()
            {
                string output = "{" + Name + "}:\n{";
                int newLine = 4;
                int fields = 0;
                foreach (FieldInfo f in GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    output += "[" + f.Name + " = " + f.GetValue(this) + "] ";
                    fields++;
                    if (fields >= newLine)
                    {
                        fields = 0;
                        output += '\n';
                    }
                }
                Engine.Commands.Log(output);
            }
            public abstract void Parse(XmlActionData xml);
            public virtual void Reset(SingularityBoss s)
            {

            }
            public virtual void Begin(SingularityBoss s)
            {
                OnBegin?.Invoke(s);
            }
            public virtual void Update(SingularityBoss s)
            {
                OnUpdate?.Invoke(s);
            }
            public virtual void End(SingularityBoss s, bool wasSkipped)
            {
                OnEnd?.Invoke(s);
            }
            public abstract bool ContinueToNextAction(SingularityBoss s);
            public override string ToString()
            {
                return base.ToString();
            }
            public virtual IEnumerator Routine(SingularityBoss s)
            {
                Engine.Commands.Log("Starting Action:" + Name);
                Begin(s);
                while (!ContinueToNextAction(s))
                {
                    Update(s);
                    yield return null;
                }
                End(s, false);
            }
        }
        public abstract class OrbActionRegistryHandler : ActionRegistryHandler
        {
            public int RequiredOrbs;
            public bool CanContinue;
            public HashSet<Orb> FinishedOrbs = [];
            public override IEnumerator Routine(SingularityBoss s)
            {
                Begin(s);
                while (!ContinueToNextAction(s))
                {
                    Update(s);
                    yield return null;
                }
                End(s, false);
            }
            public abstract void BeginOrb(Orb orb);
            public abstract void UpdateOrb(Orb orb);
            public virtual void EndOrb(Orb orb)
            {
                orb.QueuedForAttack = false;
                orb.Attacking = false;
            }
            public abstract bool ContinueToNextActionOrb(Orb orb);
            public override bool ContinueToNextAction(SingularityBoss s)
            {
                return CanContinue;
            }
        }
        private static bool _loaded;


        [Command("load_actions", "")]
        [OnLoad]
        [OnLoadContent]
        public static void Load()
        {
            _loaded = false;
            errorCode = "";
            test = "";
            actionRegisterLog = "Actions not loaded";
            patternRegisterLog = "Patterns not loaded";
            TryBuildActionErrorCode = "";
            RegisteredActions.Clear();
            RegisteredPatterns.Clear();
            ActionTypeFactories.Clear();
            if (Everest.Content.TryGet("ModFiles/PuzzleIslandHelper/SingularityMoveset", out ModAsset asset))
            {
                AddPropertyHandler<Slam>();
                AddPropertyHandler<SpikeSlam>();
                AddPropertyHandler<Slide>();
                AddPropertyHandler<Idle>();
                AddPropertyHandler<Wait>();
                AddPropertyHandler<Choice>();
                AddPropertyHandler<Loop>();
                AddPropertyHandler<OrbBounce>();
                AddPropertyHandler<Move>();
                actionRegisterLog = "";
                patternRegisterLog = "";
                if (!TryReadMovesetXml(asset))
                {
                    actionRegisterLog = "Actions not loaded";
                    patternRegisterLog = "Patterns not loaded";
                    Logger.Warn("PuzzleIslandHelper + Singularity", "Unable to read Moveset.xml");
                }
                else
                {
                    _loaded = true;
                }
            }
            else
            {
                errorCode = "Unable to find SingularityMoveset.xml";
            }
        }
    }
}
