using Microsoft.Xna.Framework;
using Monocle;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities
{
    [TrackedAs(typeof(TextLine))]
    public class UserInput : TextLine
    {
        public bool Submitted;
        public bool CanType = true;
        public static bool BlockHotkeys;
        public Func<string, bool> OnSubmit;
        public static List<Binding> BlockedBindings = new();
        public UserInput(FakeTerminal terminal, Color color, Func<string, bool> onSubmit = null) : base(terminal, "", color)
        {
            OnSubmit = onSubmit;
        }
        public string GetTextLower()
        {
            return Text.ToLower();
        }
        public string[] GetText(params char[] seperators)
        {
            return Text.Split(seperators);
        }
        public static void RefreshBlockedBindings()
        {
            BlockedBindings = new()
            {
                Input.QuickRestart.Binding,
                Input.Pause.Binding
            };
        }
        [OnLoad]
        public static void Load()
        {
            BlockedBindings.Clear();
            On.Monocle.VirtualButton.Update += VirtualButton_Update;
        }
        [OnUnload]
        public static void Unload()
        {
            BlockHotkeys = false;
            BlockedBindings.Clear();
            On.Monocle.VirtualButton.Update -= VirtualButton_Update;
        }


        private static void VirtualButton_Update(On.Monocle.VirtualButton.orig_Update orig, VirtualButton self)
        {
            if (BlockHotkeys && BlockedBindings.Contains(self.Binding))
            {
                self.ConsumePress();
                return;
            }
            orig(self);
        }
        public override void SceneBegin(Scene scene)
        {
            base.SceneBegin(scene);
            RefreshBlockedBindings();
        }
        public override void Update()
        {
            base.Update();
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            BlockHotkeys = true;
            RefreshBlockedBindings();
            TextInput.OnInput += OnTextInput;
        }
        public IEnumerator WaitForSubmit()
        {
            while (!Submitted)
            {
                yield return null;
            }
        }
        public void Clear()
        {
            Text = "";
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            BlockHotkeys = false;
            TextInput.OnInput -= OnTextInput;
        }
        public void Reset()
        {
            Clear();
            Submitted = false;
            CanType = true;
        }
        public void OnTextInput(char c)
        {
            if (!IsCurrentIndex || !CanType || Scene is not Level level || level.Paused)
            {
                return;
            }
            switch (c)
            {
                case '\r':
                    Engine.Scene.OnEndOfFrame += delegate
                    {
                        if (string.IsNullOrEmpty(Text))
                        {
                            return;
                        }
                        Submitted = true;
                        CanType = false;
                        Terminal.AddText(Text, Color.SlateBlue);
                    };
                    break;
                case '\b':

                    if (Text.Length > 0)
                    {
                        Text = Text.Substring(0, Text.Length - 1);
                    }
                    break;
                case ' ':
                    if (!string.IsNullOrEmpty(Text) && Text.Length > 0)
                    {
                        Text += c;
                    }
                    break;
                default:
                    {
                        if (!char.IsControl(c) && ActiveFont.FontSize.Characters.ContainsKey(c))
                        {
                            Text += c;
                        }
                        break;
                    }
            }
            Terminal.Renderer.UpdateSelected();
        }
    }
}