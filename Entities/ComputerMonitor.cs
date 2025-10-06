using Celeste.Mod.Entities;
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
    [CustomEntity("PuzzleIslandHelper/InterfaceMonitor")]
    [Tracked]
    public class ComputerMonitor : AbstractMonitor
    {
        public Vector2 TextOffset = new Vector2(5, 3);
        public float TextWidth => Width - TextOffset.X * 2;
        public float TextHeight => Height - TextOffset.Y * 2;
        public static string InteractedFlag = "TriedToTurnOnLabComputer";
        public ComputerMonitor(EntityData data, Vector2 offset) : base(data.Position + offset)
        {

        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (InteractedFlag.GetFlag() && !PianoModule.Session.RestoredPower)
            {
                Play("Battery");
                CenterIcon();
            }
        }
        public override IEnumerator Routine(bool on)
        {
            yield return null;
            Level level = SceneAs<Level>();
            /*            ComputerMonitorChat chat = new(SceneAs<Level>().WorldToScreen(Position + TextOffset), "chatTest", TextWidth * 6, TextHeight * 6, 4);
                        Scene.Add(chat);
                        while (chat.Active) yield return null;*/
            yield return base.Routine(on);

/*            if (on && PianoModule.Session.DEBUGBOOL4)
            {
                yield return 1;
                Flicker(5, 0.1f);
                while (Flickering) yield return null;
                Icon.Visible = false;
                Chat chat = new Chat(Position, Width, Height);
                yield return chat.Routine();
                yield return 0.1f;
                Flicker(5, 0.1f);
                while (Flickering) yield return null;
                Icon.Visible = true;
            }*/
        }
        public override bool CanActivate()
        {
            return true;//PianoModule.Session.MonitorActivated;
        }
    }
    /*    [CustomEntity("PuzzleIslandHelper/TransitMonitor")]
        [TrackedAs(typeof(Monitor))]
        public class TransitMonitor : Monitor
        {
            public TransitMonitor(EntityData data, Vector2 offset) : base(data.Position + offset)
            {

            }
            public override bool CanActivate()
            {
                return true;
            }
            public override IEnumerator Routine(bool on)
            {
                yield return base.Routine(on);
                if (on)
                {
                    yield return 1;
                    Flicker(5, 0.1f);
                    while (Flickering) yield return null;
                    Icon.Visible = false;
                    Chat chat = new Chat(Position, Width, Height, Depth - 1);
                    while (!chat.Finished)
                    {
                        yield return null;
                    }
                    yield return 0.1f;
                    Flicker(5, 0.1f);
                    while (Flickering) yield return null;
                    Icon.Visible = true;
                }

            }
        }*/
}