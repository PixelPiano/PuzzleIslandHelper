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
        public Action<FancyText.Portrait> OnNextMood;
        public Action<FancyText.Portrait> OnWait;
        public Textbox ActiveTextbox;
        public TextboxListener(Action<FancyText.Portrait, FancyText.Char> onNextChar = null, Action<FancyText.Portrait> onNextPortrait = null, Action<FancyText.Portrait> onNextMood = null, Action<FancyText.Portrait> onWait = null) : base(true, true)
        {
            OnNextChar = onNextChar;
            OnNextPortrait = onNextPortrait;
            OnNextMood = onNextMood;
            OnWait = onWait;
        }
        public void SetTextbox(Textbox box)
        {
            ActiveTextbox = box;
        }
        public override void Added(Entity entity)
        {
            base.Added(entity);
            TextboxInfo.OnNextChar += OnNextChar;
            TextboxInfo.OnPortraitChange += OnNextPortrait;
            TextboxInfo.OnMoodChange += OnNextMood;
            TextboxInfo.OnWaitForInput += OnWait;
            TextboxInfo.GetTextbox += SetTextbox;
        }
        public override void Removed(Entity entity)
        {
            base.Removed(entity);
            TextboxInfo.OnNextChar -= OnNextChar;
            TextboxInfo.OnPortraitChange -= OnNextPortrait;
            TextboxInfo.OnMoodChange -= OnNextMood;
            TextboxInfo.OnWaitForInput -= OnWait;
            TextboxInfo.GetTextbox -= SetTextbox;
        }
    }
}
