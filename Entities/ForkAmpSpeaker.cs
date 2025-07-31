using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/ForkAmpSpeaker")]
    [Tracked]
    public class ForkAmpSpeaker : Entity
    {
        public bool Played;
        public DotX3 Talk;
        public bool Playing;
        public bool PlayedOnce;
        public ForkAmpState State => PianoModule.Session.ForkAmpState;
        public float Amount;
        public SoundSource Sound;

        public float? SavedVolume;
        private bool canInteract = true;
        private Sprite Screen;
        public ForkAmpSpeaker(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Depth = 1;
            Image image = new Image(GFX.Game["objects/PuzzleIslandHelper/forkAmp/isolatedSpeaker"]);
            Add(image);
            Screen = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/forkAmp/wirelessScreen");
            Screen.AddLoop("off", "", 0.1f, 0);
            Screen.Add("pushButton", "", 0.2f, "activate", 1, 1);
            Screen.Add("activate", "", 0.1f, "idle", 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12);
            Screen.Add("idle", "", 0.5f, "idleShine", 13, 13);
            Screen.Add("idleShine", "", 0.1f, "idle", 14, 15, 16, 17, 18, 19, 20, 21, 22);
            Add(Screen);
            Screen.Play("off");
            Collider = new Hitbox(Screen.Width, Screen.Height);
            Add(Talk = new DotX3(Collider, Interact));
            Add(Sound = new SoundSource());
        }
        public override void Update()
        {
            base.Update();
            Talk.Enabled = canInteract;
        }
        public void Interact(Player player)
        {
            canInteract = false;
            player.StateMachine.State = Player.StDummy;
            PlayedOnce = true;
            Add(new Coroutine(Routine(player)));
        }
        public void OnEnd(Player player)
        {
            Add(new Coroutine(EndRoutine(player)));
        }
        private IEnumerator EndRoutine(Player player)
        {
            yield return null;
            player.StateMachine.State = Player.StNormal;
        }
        public IEnumerator FadeMusicOut()
        {
            SavedVolume = Audio.MusicVolume;
            Sound.instance.getVolume(out float volume, out _);
            Sound.instance.setVolume(0);
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                if (Audio.MusicVolume > 0.1f)
                {
                    Audio.MusicVolume = Calc.LerpClamp(Audio.MusicVolume, 0.1f, i);
                }
                Sound.instance.setVolume(Calc.LerpClamp(0, volume, i));
                Amount = i;
                yield return null;
            }
            Amount = 1;
            Audio.MusicVolume = 0.1f;
        }
        public IEnumerator FadeMusicIn()
        {
            if (SavedVolume.HasValue)
            {
                Sound.instance.getVolume(out float volume, out _);
                for (float i = 0; i < 1; i += Engine.DeltaTime)
                {
                    Amount = 1 - i;
                    Audio.MusicVolume = Calc.LerpClamp(Audio.MusicVolume, SavedVolume.Value, i);
                    Sound.instance.setVolume(Calc.LerpClamp(volume, 0, i));
                    yield return null;
                }
            }
            Amount = 0;
            Sound.Stop();
            if (SavedVolume.HasValue)
            {
                Audio.MusicVolume = SavedVolume.Value;
                SavedVolume = null;
            }
            canInteract = true;
        }
        public IEnumerator Routine(Player player)
        {
            yield return ActivateScreen();
            StartOscillators();
            for (int i = 0; i < 4; i++)
            {
                Sound.Param("Osc " + (i + 1), State.Rates[i]);
            }
            Sound.Param("Distort", SoundSpinner.DistortAmount);
            while (!Input.Jump)
            {
                yield return null;
            }
            StopOscillators();
            yield return null;
            OnEnd(player);
        }
        private IEnumerator ActivateScreen()
        {
            Screen.Play("pushButton");
            while (Screen.CurrentAnimationID != "idle")
            {
                yield return null;
            }
            yield return null;
        }
        public void StartOscillators()
        {
            Playing = true;
            Sound.Play("event:/PianoBoy/Soundwaves/tuningForkLoop");
            Add(new Coroutine(FadeMusicOut()));
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            if (PlayedOnce && SavedVolume.HasValue)
            {
                Audio.MusicVolume = SavedVolume.Value;
                SavedVolume = null;
            }
        }

        public void StopOscillators()
        {
            Playing = false;
            Screen.Play("off");
            Add(new Coroutine(FadeMusicIn()));
        }
    }

}
