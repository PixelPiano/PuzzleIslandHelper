using Celeste.Mod.Entities;
using Celeste.Mod.Meta;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Components.Visualizers;
using Celeste.Mod.PuzzleIslandHelper.Components.Visualizers.DSPs;
using Celeste.Mod.PuzzleIslandHelper.Cutscenes.GameshowEntities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System.Collections;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/LoneForkSpeaker")]
    [Tracked]
    public class LoneForkSpeaker : Entity
    {
        public Sprite Sprite;
        public bool Played;
        public DotX3 Talk;
        public bool Playing;
        public ForkAmpState State => PianoModule.Session.ForkAmpState;
        public float Amount;
        public SoundSource Sound;

        public float SavedVolume;
        public bool PlayedOnce;
        public LoneForkSpeaker(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Depth = 1;
            Sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/forkAmp/");
            Sprite.AddLoop("idle", "texture", 0.1f);
            Sprite.Play("idle");
            Add(Sprite);
            Collider = new Hitbox(Sprite.Width, Sprite.Height);
            Position -= new Vector2(Sprite.Width / 2, Sprite.Height / 2);
            Add(Talk = new DotX3(Collider, Interact));
            Add(Sound = new SoundSource());
                
        }
        public override void Update()
        {
            base.Update();

        }
        public void Interact(Player player)
        {
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
                yield return null;
            }
            Audio.MusicVolume = 0.1f;
        }
        public IEnumerator FadeMusicIn()
        {
            Sound.instance.getVolume(out float volume, out _);
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                Audio.MusicVolume = Calc.LerpClamp(Audio.MusicVolume, SavedVolume, i);
                Sound.instance.setVolume(Calc.LerpClamp(volume, 0, i));
                yield return null;
            }
            Sound.Stop();
            Audio.MusicVolume = SavedVolume;
        }
        public IEnumerator Routine(Player player)
        {
            if(Scene is not Level level) yield break;
            StartOscillators();
            for (int i = 0; i < 4; i++)
            {
                Sound.Param("Osc " + (i + 1), State.Rates[i]);
            }
            while (!Input.Jump)
            {
                yield return null;
            }
            StopOscillators();
            yield return null;
            OnEnd(player);
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
            if (PlayedOnce)
            {
                Audio.MusicVolume = SavedVolume;
            }
        }

        public void StopOscillators()
        {
            Playing = false;
            Add(new Coroutine(FadeMusicIn()));
        }
    }

}
