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
        public bool CanType;
        public Func<string, bool> OnSubmit;
        public static bool CurrentlySelected;
        public static bool PreviouslySelected;
        public static List<Binding> BlockedBindings = new();
        public UserInput(FakeTerminal terminal, int index, Color color, Func<string, bool> onSubmit = null) : base(terminal, "", index, color)
        {
            OnSubmit = onSubmit;
        }
        public static void RefreshBlockedBindings()
        {
            BlockedBindings = new()
            {
                Input.QuickRestart.Binding,
                Input.Pause.Binding
            };
        }
        public override void SceneBegin(Scene scene)
        {
            base.SceneBegin(scene);
            RefreshBlockedBindings();
        }
        public static void Load()
        {
            CurrentlySelected = PreviouslySelected = false;
            BlockedBindings.Clear();
            On.Monocle.VirtualButton.Update += VirtualButton_Update;
        }
        public static void Unload()
        {
            CurrentlySelected = PreviouslySelected = false;
            BlockedBindings.Clear();
            On.Monocle.VirtualButton.Update -= VirtualButton_Update;
        }
        private static void VirtualButton_Update(On.Monocle.VirtualButton.orig_Update orig, VirtualButton self)
        {
            if (CurrentlySelected && BlockedBindings.Contains(self.Binding))
            {
                self.ConsumeBuffer();
                self.ConsumePress();
                return;
            }
            orig(self);
        }
        public override void Update()
        {
            base.Update();
            WasCurrentIndex = IsCurrentIndex;
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            TextInput.OnInput += OnTextInput;
            CanType = true;
        }

        public IEnumerator WaitForSubmit()
        {
            while (!Submitted)
            {
                yield return null;
            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            TextInput.OnInput -= OnTextInput;
        }
        public void Reset()
        {
            Text = "";
            Submitted = false;
            CanType = true;
        }
        public void OnTextInput(char c)
        {
            if (Scene is not Level level || level.Paused || !CanType)
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
                    };
                    break;
                case '\b':

                    if (Text.Length > 0 && IsCurrentIndex)
                    {
                        Text = Text.Substring(0, Text.Length - 1);
                    }
                    break;
                case ' ':
                    if (!string.IsNullOrEmpty(Text) && Text.Length > 0 && IsCurrentIndex)
                    {
                        Text += c;
                    }
                    break;
                default:
                    {
                        if (IsCurrentIndex && !char.IsControl(c) && ActiveFont.FontSize.Characters.ContainsKey(c))
                        {
                            Text += c;
                        }
                        break;
                    }
            }
        }
    }
}