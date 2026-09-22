using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities.Programs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using static Celeste.Overworld;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    public class Act : Entity
    {
        public int Size;
        public string Text;
        public float BGAlpha = 1;
        public float TextAlpha = 1;
        public bool CanContinue;
        public bool PlayerPressed;
        public bool PlayerCanPress;
        public Action OnTextFadeStart, OnBGFadeStart;
        public float TextFadeTime = 1;
        public float BGFadeTime = 2;
        public float BGFadeDelay = 1.4f;

        public Act(string text) : base()
        {
            Text = text;
            Tag |= TagsExt.SubHUD;
        }
        public IEnumerator Routine(Player player)
        {
            player.StateMachine.State = Player.StDummy;
            while (!CanContinue)
            {
                if (PlayerCanPress && Input.MenuConfirm.Pressed)
                {
                    PlayerPressed = true;
                }
                yield return null;
            }
            while (!PlayerPressed)
            {
                if (PlayerCanPress)
                {
                    PlayerPressed = Input.MenuConfirm.Pressed;
                }
                yield return null;
            }
            OnTextFadeStart?.Invoke();
            FadeText(TextFadeTime);
            yield return BGFadeDelay;
            OnBGFadeStart?.Invoke();
            FadeBg(BGFadeTime);
            yield return 2.3f;
        }
        public virtual void FadeBg(float time)
        {
            Tween.Set(this, Tween.TweenMode.Oneshot, time, Ease.SineInOut, t =>
            {
                SetBGAlpha(1 - t.Eased);
            }, t => SetBGAlpha(0));
        }
        public void SetBGAlpha(float alpha)
        {
            BGAlpha = alpha;
        }
        public virtual void SetTextAlpha(float alpha)
        {
            TextAlpha = alpha;
        }
        public void FadeText(float time)
        {
            Tween.Set(this, Tween.TweenMode.Oneshot, time, Ease.SineInOut, t =>
            {
                SetTextAlpha(1 - t.Eased);
            }, t => SetTextAlpha(0));
        }
        public override void Render()
        {
            float width = 1920;
            float height = 1080;
            Vector2 p = new Vector2(width / 2, height / 2);
            int size = (int)(height / 5f);
            Draw.Rect(-20, -20, width + 40, height + 40, Color.Black * BGAlpha);
            base.Render();
            ActiveFont.Draw(Text, p, Vector2.One / 2, Vector2.One * size / ActiveFont.BaseSize, Color.White * TextAlpha);
        }
    }
}