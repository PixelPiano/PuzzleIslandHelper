using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.Programs;
using Celeste.Mod.PuzzleIslandHelper.PuzzleData;
using FrostHelper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Mono.Cecil;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Core.Tokens;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities
{
    [TrackedAs(typeof(ComputerIcon))]
    public class YouHaveMail : DesktopClickable
    {
        public bool State;
        private Sprite sprite;
        private bool inRoutine;
        private string TextID, TabText;
        public YouHaveMail(Interface inter, string textId, string tabText) : base(inter)
        {
            TextID = textId;
            TabText = tabText;
            sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/interface/mail/");
            sprite.AddLoop("anim", "notif", 0.05f);
            sprite.AddLoop("idle", "empty", 0.1f);
            sprite.Play("idle");
            Add(sprite);
            Collider = new Hitbox(sprite.Width, sprite.Height);
            sprite.CenterOrigin();
            sprite.Position = new Vector2(Width / 2, Height / 2);
        }
        public override void Update()
        {
            base.Update();
            if (Interface.monitor != null)
            {
                Position = Interface.monitor.Center - new Vector2(Width / 2, Height / 2);
            }
        }
        public override void Begin(Scene scene)
        {
            base.Begin(scene);
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeIn, 1, false);
            tween.OnUpdate = (Tween t) =>
            {
                sprite.Scale.Y = t.Eased;
                sprite.Scale.X = 0.5f + t.Eased / 2;
            };
            tween.OnComplete = delegate { sprite.Scale = Vector2.One; sprite.Play("anim"); };
            Add(tween);
            Add(Alarm.Create(Alarm.AlarmMode.Oneshot, tween.Start, 0.3f, true));
        }
        public override void OnClick()
        {
            base.OnClick();
            if (!inRoutine)
            {
                inRoutine = true;
                Add(new Coroutine(sequence()));
            }
        }
        private IEnumerator sequence()
        {
            Interface.Window.Alpha = 0;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 0.5f)
            {
                sprite.Scale.Y = 1 - Ease.BigBackIn(i);
                yield return null;
            }
            sprite.Scale.Y = 0;
            yield return null;
            sprite.Visible = false;
            WindowContent content = Interface.GetProgram("mail");
            if (content is null) yield break;
            content.Visible = false;
            Interface.OpenCustom("mail",TextID, TabText);
            for (float i = 0; i < 1; i += Engine.DeltaTime / 0.4f)
            {
                Interface.Window.Alpha = Ease.SineInOut(i);
                yield return null;
            }
            Interface.Window.Alpha = 1;
            content.Visible = true;
        }
    }
}