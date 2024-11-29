using System;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Components
{
    [Tracked]
    public class TextboxListener : Component
    {
        public Action<FancyText.Portrait, FancyText.Char> OnNextChar;
        public Action<FancyText.Portrait> OnNextPortrait;
        public Action<FancyText.Portrait, string> OnNextMood;
        public Action<FancyText.Portrait> OnWait;
        public Textbox ActiveTextbox;
        private string portrait;
        public TextboxListener(string portraitName, Action<FancyText.Portrait, FancyText.Char> onNextChar = null, Action<FancyText.Portrait> onNextPortrait = null, Action<FancyText.Portrait,string> onNextMood = null, Action<FancyText.Portrait> onWait = null) : base(true, true)
        {
            portrait = portraitName;
            OnNextChar = onNextChar;
            OnNextPortrait = onNextPortrait;
            OnNextMood = onNextMood;
            OnWait = onWait;
        }
        private bool ValidPortrait(FancyText.Portrait p)
        {
            return string.IsNullOrEmpty(portrait) || (p != null && portrait == p.Sprite);
        }
        private void onNextChar(FancyText.Portrait p, FancyText.Char c)
        {
            if (Active && ValidPortrait(p))
            {
                OnNextChar?.Invoke(p, c);
            }
        }
        private void onNextPortrait(FancyText.Portrait p)
        {
            if (Active && ValidPortrait(p))
            {
                OnNextPortrait?.Invoke(p);
            }
        }
        private void onNextMood(FancyText.Portrait p, string anim)
        {
            if (Active && ValidPortrait(p))
            {
                OnNextMood?.Invoke(p, anim);
            }
        }
        private void onWait(FancyText.Portrait p)
        {
            if (Active && ValidPortrait(p))
            {
                OnWait?.Invoke(p);
            }
        }
        public void SetTextbox(Textbox box)
        {
            ActiveTextbox = box;
        }
        public override void Added(Entity entity)
        {
            base.Added(entity);
            TextboxInfo.OnNextChar += onNextChar;
            TextboxInfo.OnPortraitChange += onNextPortrait;
            TextboxInfo.OnMoodChange += onNextMood;
            TextboxInfo.OnWaitForInput += onWait;
            TextboxInfo.GetTextbox += SetTextbox;
        }
        public override void Removed(Entity entity)
        {
            base.Removed(entity);
            TextboxInfo.OnNextChar -= onNextChar;
            TextboxInfo.OnPortraitChange -= onNextPortrait;
            TextboxInfo.OnMoodChange -= onNextMood;
            TextboxInfo.OnWaitForInput -= onWait;
            TextboxInfo.GetTextbox -= SetTextbox;
        }
    }
}
