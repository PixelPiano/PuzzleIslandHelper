using Celeste.Mod.PuzzleIslandHelper.Components.Visualizers;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.Programs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;

using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities
{

    [TrackedAs(typeof(BetterButton))]
    public class FreqPlayback : ListButton
    {
        public string EventName;
        public WaveProgram WaveProgram;
        public FreqPlayback(WaveProgram parent, string eventName, Func<string> customText = null) : base(parent, eventName, customText)
        {
            EventName = eventName;
            
            TextSize = 35f;
            TextOffset = new Vector2(16, 8);
            CustomText = customText;
            WaveProgram = parent;
        }
        public override void RunActions()
        {
            base.RunActions();

            if (WaveProgram is not null)
            {
                if (ForcePressed)
                {
                    WaveProgram.StopSound();
                    CurrentEventName = null;
                    Pressing = false;
                }
                else
                {
                    WaveProgram.PlaySound(this);
                    CurrentEventName = EventName;
                }
            }
        }
        public override void OnOpened(Scene scene)
        {
            base.OnOpened(scene);

        }
        public override void Added(Entity entity)
        {
            base.Added(entity);
            Text = CustomText is null ? EventName.Substring(EventName.LastIndexOf('/')).Remove(0, 1) + ".rec" : CustomText.Invoke();
        }
        public override void Update()
        {
            base.Update();
            ForcePressed = CurrentEventName == EventName;
        }
    }
}