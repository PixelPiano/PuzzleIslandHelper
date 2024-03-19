using Celeste.Mod.Entities;
using Celeste.Mod.LuaCutscenes;
using IL.Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/Washer")]
    [Tracked]
    public class Washer : Actor
    {
        public static bool Activated;
        private Sprite sprite;
        private int OffAttempts
        {
            get
            {
                return PianoModule.Session.WasherSwitchAttempts;
            }
            set
            {
                PianoModule.Session.WasherSwitchAttempts = value;
            }
        }
        private TalkComponent Talk;
        public Washer(Vector2 position) : base(position)
        {
            sprite = new Sprite(GFX.Game, "characters/PuzzleIslandHelper/Washer/");
            sprite.AddLoop("idle", "idle", 0.1f);
            sprite.AddLoop("off", "idle", 0.1f, 30);
            sprite.Position.X--;
            Add(sprite);
            Collider = new Hitbox(sprite.Width - 1, sprite.Height, -1);
            Add(Talk = new TalkComponent(Collider.Bounds, new Vector2(Width / 2, 0), Interact));
        }

        public Washer(EntityData data, Vector2 offset) : this(data.Position + offset) { }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            sprite.Play(Activated ? "idle" : "off");
        }
        public override void Update()
        {
            base.Update();
        }
        private void Interact(Player player)
        {
            player.StateMachine.State = Player.StDummy;
            if (Activated)
            {
                Add(new Coroutine(OnRoutine(player)));
            }
            else
            {
                Add(new Coroutine(OffRoutine(OffAttempts, player)));
            }
        }
        private IEnumerator OffRoutine(int attempts, Player player)
        {
            if (attempts == 7)
            {
                Add(new Coroutine(PlayerKick(player)));
            }
            yield return Textbox.Say(attempts switch
            {
                0 => "washerIsOff",
                1 => "washerIsStillOff",
                2 => "washerReallyIsOff",
                > 2 and < 6 => "washerIsReallyStillOff",
                6 => "washerButtonsAreSticking",
                7 => "washerKicked",
                _ => ""
            });
            OffAttempts++;
            OffAttempts %= 7;
            player.StateMachine.State = Player.StNormal;
            yield return null;
        }
        private IEnumerator OnRoutine(Player player)
        {
            Level level = Scene as Level;
            Coroutine routine = new Coroutine(player.DummyWalkTo(Collider.AbsoluteRight));
            Add(routine);
            yield return level.ZoomTo(level.WorldToScreen(CenterLeft), 1.6f, 1);
            while (routine.Active)
            {
                yield return null;
            }
            if (level.Session.GetFlag("gameshowWin"))
            {
                yield return WipeFaces();
            }
            else
            {
                GameshowSetup(level);
                yield return 0.1f;
                level.Session.SetFlag("Intro");
                level.Session.SetFlag("gameStart");
            }
            yield return level.ZoomBack(1);
            player.StateMachine.State = Player.StNormal;
            yield return null;
        }
        private void GameshowSetup(Level level)
        {
            //please dedicate a full day or two to remaking the gameshow in c# please
            level.Session.SetFlag("rewindTeleport", false);
            level.Session.SetFlag("laughing", false);
            level.Session.SetFlag("light1", false);
            level.Session.SetFlag("light2", false);
            level.Session.SetFlag("light3", false);
            level.Session.SetFlag("light4", false);
            level.Session.SetFlag("light5", false);
            level.Session.SetFlag("light6", false);
            level.Session.SetFlag("bigLose", false);
            level.Session.SetFlag("eraseLife1", false);
            level.Session.SetFlag("eraseLife2", false);
            level.Session.SetFlag("eraseLife3", false);
            level.Session.SetFlag("eraseLife4", false);
            level.Session.SetFlag("eraseLife5", false);
            level.Session.SetFlag("notIntro", false);
            level.Session.SetFlag("lifeFlash", false);
        }
        private IEnumerator WipeFaces()
        {
            yield return Textbox.Say("faceWipe1");
            yield return ChoicePrompt.Prompt("madelineSayNoWipe", "madelineSayYesWipe");
            if (ChoicePrompt.Choice == 1)
            {
                yield return Textbox.Say("yesWipe");
            }
            else
            {
                yield return Textbox.Say("noWipe");
            }
        }
        private IEnumerator PlayerKick(Player player)
        {
            float x = player.Sprite.Position.X;
            float distance = 4;
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                yield return null;
                player.Sprite.Position.X = x - i * distance;
            }
            //todo: machine kicked sound effect
            x = player.Sprite.Position.X;
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                yield return null;
                player.Sprite.Position.X = x + i * distance;
            }
        }
    }
}
