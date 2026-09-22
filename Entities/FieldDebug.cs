using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities.Programs;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using static Celeste.Overworld;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [ConstantEntity("PuzzleIslandHelper/SubHUDEntity")]
    [Tracked]
    public class FieldDebug : Entity
    {
        public static DebugField Debug(Entity entity, string name, Func<string> function)
        {
            DebugField field = new DebugField(name, function);
            entity.Add(field);
            return field;
        }
        [Tracked]
        public class DebugField : Component
        {
            public Func<string> Function;
            public string Name;
            public string Text { private set; get; }
            public DebugField(string name, Func<string> getValue) : base(true, false)
            {
                Name = name;
                Function = getValue;
            }
            public override void Update()
            {
                base.Update();
                if (string.IsNullOrEmpty(Name))
                {
                    Name = "[unnamed]";
                }
                string value = "[no function provided]";
                if (Function != null)
                {
                    string v = Function.Invoke();
                    if (string.IsNullOrEmpty(v)) value = "[null]";
                    else value = v;
                }
                Text = Name + ": " + value;
            }
        }
        public FieldDebug() : base()
        {
            Tag |= TagsExt.SubHUD | Tags.Global;

        }
        public override void Render()
        {
            base.Render();
            Vector2 position = Vector2.Zero;
            foreach (DebugField field in Scene.Tracker.GetComponents<DebugField>())
            {
                if (!string.IsNullOrEmpty(field.Text))
                {
                    ActiveFont.Draw(field.Text, position, Color.White);
                    position.Y += ActiveFont.Measure(field.Text).Y;
                }

            }
        }
    }
}