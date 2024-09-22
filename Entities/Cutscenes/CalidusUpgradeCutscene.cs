using Celeste.Mod.PuzzleIslandHelper.Entities.WIP;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using static Celeste.Mod.PuzzleIslandHelper.Entities.CustomCalidusEntities.PlayerCalidus;
using static Celeste.Mod.PuzzleIslandHelper.Entities.Calidus;
using Looking = Celeste.Mod.PuzzleIslandHelper.Entities.Calidus.Looking;
using Mood = Celeste.Mod.PuzzleIslandHelper.Entities.Calidus.Mood;
using Celeste.Mod.PuzzleIslandHelper.Entities.CustomCalidusEntities;

namespace Celeste.Mod.PuzzleIslandHelper.Cutscenes
{
    [Tracked]
    public class CalidusUpgradeCutscene : CutsceneEntity
    {
        public Upgrades Upgrade;
        private PlayerCalidus Calidus;
        public bool Instant;

        public CalidusUpgradeCutscene(Upgrades upgrade, bool instant)
            : base()
        {
            Upgrade = upgrade;
            Instant = instant;
        }
        public void SetCutsceneFlag()
        {
            Level.Session.SetFlag("CalidusUpgrade" + Upgrade.ToString() + "Collected");
        }
        public static bool GetCutsceneFlag(Scene scene, Upgrades part)
        {
            return (scene as Level).Session.GetFlag("CalidusUpgrade" + part.ToString() + "Collected");
        }
        public override void OnBegin(Level level)
        {
            if (level.GetPlayer() is PlayerCalidus calidus)
            {
                Calidus = calidus;
                if (!Instant)
                {
                    switch (Upgrade)
                    {
                        case Upgrades.Nothing:
                            Add(new Coroutine(NothingScene()));
                            break;
                        case Upgrades.Grounded:
                            Add(new Coroutine(GroundedScene()));
                            break;
                        case Upgrades.Slowed:
                            Add(new Coroutine(SlowedScene()));
                            break;
                        case Upgrades.Weakened:
                            Add(new Coroutine(WeakenedScene()));
                            break;
                        case Upgrades.Vision:
                            Add(new Coroutine(VisionScene()));
                            break;
                        case Upgrades.Jumping:
                            Add(new Coroutine(JumpingScene()));
                            break;
                        case Upgrades.Sticky:
                            Add(new Coroutine(StickyScene()));
                            break;
                        case Upgrades.Rail:
                            Add(new Coroutine(RailScene()));
                            break;
                        case Upgrades.Blip:
                            Add(new Coroutine(BlipScene()));
                            break;
                    }
                }
                else
                {
                    ChangeInventory();
                    RemoveSelf();
                    //EndCutscene(Level);
                }
            }
        }
        public void ChangeInventory()
        {
            PianoCommands.SetCalidusInventory(Upgrade);
            //PianoModule.SaveData.CalidusInventory = Inventories[Upgrade];
        }
        private IEnumerator DigitalGlitch(float fadeDuration = 1.6f, float waitDuration = 0.8f)
        {
            for (float i = 0; i < 1; i += Engine.DeltaTime / fadeDuration)
            {
                Glitch.Value = Calc.LerpClamp(0, 0.4f, i);
                yield return null;
            }
            Glitch.Value = 1;
            yield return waitDuration;
            Glitch.Value = 0;
        }
        private IEnumerator NothingScene()
        {
            SetInventory(CalidusInventory.Nothing);
            SetLighting(Level, 1);
            Calidus.LightingShiftAmount = 1;
            yield return null;
            EndCutscene(Level);
        }
        private IEnumerator GroundedScene()
        {
            yield return DigitalGlitch();
            Level.Add(new MiniTextbox("calidus_grounded"));
            SetInventory(CalidusInventory.Grounded);
            SetLighting(Level, 1);
            Calidus.LightingShiftAmount = 1;
            yield return null;
            EndCutscene(Level);
        }
        private IEnumerator SlowedScene()
        {
            yield return DigitalGlitch(0.8f, 0.2f);
            Level.Add(new MiniTextbox("calidus_slowed"));
            SetInventory(CalidusInventory.Slowed);
            SetLighting(Level, 1);
            Calidus.LightingShiftAmount = 1;
            yield return null;
            EndCutscene(Level);
        }
        private IEnumerator WeakenedScene()
        {
            yield return DigitalGlitch(0.3f, 0.8f);
            Level.Add(new MiniTextbox("calidus_weakened"));
            SetInventory(CalidusInventory.Weakened);
            SetLighting(Level, 1);
            Calidus.LightingShiftAmount = 1;
            yield return null;
            EndCutscene(Level);
        }
        private IEnumerator VisionScene()
        {
            if (Level.Tracker.GetEntity<LoneEye>() is LoneEye eye)
            {
                Level.Session.DoNotLoad.Add(eye.id);
                yield return ZoomAndWalk(Calidus, "walkto", "camera", 1.4f, 1);
                yield return Textbox.Say("calidus_eye_cutscene", Normal, Stern, Surprised, AbsorbEye);
                yield return Level.ZoomBack(1f);
            }
            EndCutscene(Level);
        }
        private IEnumerator JumpingScene()
        {
            yield return null;
            EndCutscene(Level);
        }
        private IEnumerator StickyScene()
        {

            yield return null;
            EndCutscene(Level);
        }

        private IEnumerator RailScene()
        {

            yield return null;
            EndCutscene(Level);
        }

        private IEnumerator BlipScene()
        {
            yield return null;
            EndCutscene(Level);
        }

        private IEnumerator ZoomAndWalk(PlayerCalidus player, float walkToX, Vector2 screenSpaceFocusPoint, float zoom, float duration, float speedMultiplier = 1)
        {
            Coroutine z = new Coroutine(Level.ZoomTo(screenSpaceFocusPoint, zoom, duration));
            Add(z);
            yield return player.DummyMoveTo(walkToX, speedMultiplier);
            while (!z.Finished) yield return null;
        }
        private IEnumerator ZoomAndWalk(PlayerCalidus player, string walkToMarker, string cameraMarker, float zoom, float duration, float speedMultiplier = 1)
        {
            Vector2 screen = Level.Marker(cameraMarker, true);
            float walk = Level.Marker(walkToMarker).X;
            yield return ZoomAndWalk(player, walk, screen, zoom, duration, speedMultiplier);
        }
        public override void OnEnd(Level level)
        {
            level.ResetZoom();
            Glitch.Value = 0;
            //SetInventory(Upgrade);
            if (Calidus is not null)
            {
                if (Calidus.StateMachine.State == DummyState) Calidus.StateMachine.State = NormalState;
                Calidus.Emotion(Mood.Normal);
            }
        }
        private IEnumerator AbsorbEye()
        {
            if (Scene.Tracker.GetEntity<LoneEye>() is not LoneEye eye) yield break;

            Calidus.SpriteOffset.X++;
            for (int i = 0; i < 3; i++)
            {
                Calidus.SpriteOffset.X -= 2;
                yield return null;
                yield return null;
                Calidus.SpriteOffset.X += 2;
                yield return null;
                yield return null;
            }
            Calidus.SpriteOffset.X--;

            //add absorb effect
            yield return eye.StartFloating(16, 2f, 0.4f);
            yield return 1f;
            yield return eye.Absorb(Calidus.Center, 0.4f, Color.White);
            SetInventory(Upgrades.Vision);
            yield return null;
        }
        private IEnumerator Happy()
        {
            Calidus.Emotion(Mood.Happy);
            yield return null;
        }
        private IEnumerator Stern()
        {
            Calidus.Emotion(Mood.Stern);
            yield return null;
        }
        private IEnumerator Normal()
        {
            Calidus.Emotion(Mood.Normal);
            yield return null;
        }
        private IEnumerator RollEye()
        {
            Calidus.Emotion(Mood.RollEye);
            yield return null;
        }
        private IEnumerator Laughing()
        {
            Calidus.Emotion(Mood.Laughing);
            yield return null;
        }
        private IEnumerator Shakers()
        {
            Calidus.Emotion(Mood.Shakers);
            yield return null;
        }
        private IEnumerator Nodders()
        {
            Calidus.Emotion(Mood.Nodders);
            yield return null;
        }
        private IEnumerator Closed()
        {
            Calidus.Emotion(Mood.Closed);
            yield return null;
        }
        private IEnumerator Angry()
        {
            Calidus.Emotion(Mood.Angry);
            yield return null;
        }
        private IEnumerator Surprised()
        {
            Calidus.Emotion(Mood.Surprised);
            yield return null;
        }
        private IEnumerator Wink()
        {
            Calidus.Emotion(Mood.Wink);
            yield return null;
        }
        private IEnumerator Eugh()
        {
            Calidus.Emotion(Mood.Eugh);
            yield return null;
        }
        private IEnumerator LookLeft()
        {
            Calidus.Look(Looking.Left);
            yield return null;
        }
        private IEnumerator LookRight()
        {
            Calidus.Look(Looking.Right);
            yield return null;
        }
        private IEnumerator LookUp()
        {
            Calidus.Look(Looking.Up);
            yield return null;
        }
        private IEnumerator LookDown()
        {
            Calidus.Look(Looking.Down);
            yield return null;
        }
        private IEnumerator LookUpRight()
        {
            Calidus.Look(Looking.UpRight);
            yield return null;
        }
        private IEnumerator LookDownRight()
        {
            Calidus.Look(Looking.DownRight);
            yield return null;
        }
        private IEnumerator LookDownLeft()
        {
            Calidus.Look(Looking.DownLeft);
            yield return null;
        }
        private IEnumerator LookUpLeft()
        {
            Calidus.Look(Looking.UpLeft);
            yield return null;
        }
        private IEnumerator LookCenter()
        {
            Calidus.Look(Looking.Center);
            yield return null;
        }
        private IEnumerator LookPlayer()
        {
            Calidus.Look(Looking.Player);
            yield return null;
        }
    }
}