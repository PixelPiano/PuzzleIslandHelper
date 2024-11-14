using Monocle;
using System;
using static Celeste.Mod.PuzzleIslandHelper.Components.BitrailNode;
using System.Reflection;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    public class TextboxInfo : Entity
    {
        private FancyText.Portrait Portrait;
        private string talking;
        private string lastMood;
        private string mood;
        private string lastTalking;
        private int lastIndex;
        private int currentIndex;
        /// <summary>
        /// Invoked when the first active <see cref="Textbox"/>'s index changes.
        /// <para></para>
        /// <see cref="FancyText.Portrait"/>: The current Portrait.
        /// <see cref="FancyText.Char"/>: The current Character.
        /// </summary>
        public static event Action<FancyText.Portrait, FancyText.Char> OnNextChar;
        /// <summary>
        /// Invoked when the portrait changes.
        /// <para></para>
        /// <see cref="FancyText.Portrait"/>: The current Portrait.
        /// </summary>
        public static event Action<FancyText.Portrait> OnPortraitChange;
        /// <summary>
        /// Invoked when the portrait theme changes.
        /// <para></para>
        /// <see cref="FancyText.Portrait"/>: The current Portrait.
        /// </summary>
        public static event Action<FancyText.Portrait> OnMoodChange;
        public static event Action<FancyText.Portrait> OnWaitForInput;

        public static event Action<Textbox> GetTextbox;
        public TextboxInfo() : base()
        {
            Tag = Tags.Global;
        }
        public override void Update()
        {
            base.Update();
            if (Scene.Tracker.GetEntities<Textbox>().Find(item => (item as Textbox).Opened) is Textbox t)
            {
                GetTextbox?.Invoke(t);
                if (t.portrait != Portrait)
                {
                    OnPortraitChange?.Invoke(t.portrait);
                }
                Portrait = t.portrait;

                if (t.waitingForInput)
                {
                    OnWaitForInput?.Invoke(Portrait);
                }

                if (t.portraitExists && !string.IsNullOrEmpty(t.PortraitAnimation))
                {
                    mood = t.PortraitAnimation;
                    if (lastMood != mood)
                    {
                        OnMoodChange?.Invoke(Portrait);
                    }
                    lastMood = t.PortraitAnimation;
                }
                else
                {
                    lastMood = mood = null;
                }

                currentIndex = Calc.Clamp(t.index, 0, t.Nodes.Count - 1);
                if (lastIndex != currentIndex)
                {
                    if (t.Nodes[currentIndex] is FancyText.Char c)
                    {
                        OnNextChar?.Invoke(Portrait, c);
                    }
                }
                lastIndex = currentIndex;
            }
            else
            {
                Portrait = null;
                talking = lastTalking = null;
                mood = lastMood = null;
                lastIndex = 0;
            }
        }
        [OnLoad]
        public static void Load()
        {
            Everest.Events.LevelLoader.OnLoadingThread += LevelLoader_OnLoadingThread;
        }

        private static void LevelLoader_OnLoadingThread(Level level)
        {
            level.Add(new TextboxInfo());
        }
        [OnUnload]
        public static void Unload()
        {
            Everest.Events.LevelLoader.OnLoadingThread -= LevelLoader_OnLoadingThread;
        }
    }
}