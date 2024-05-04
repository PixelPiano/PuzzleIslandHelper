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
    public class ForkAmpState
    {
        public float[] Rates = new float[4];
        public float[] Effectiveness = new float[4];
        public bool CanPlay;
        public int Unlocked;
    }
    [CustomEntity("PuzzleIslandHelper/ForkAmp")]
    [Tracked]
    public class ForkAmp : Entity
    {
        public Sprite Sprite;
        public bool Played;
        public DotX3 Talk;
        public string[] flags;
        public Oscillator[] Oscillators;
        public bool Playing;

        public float Amount;

        public class Oscillator : Entity
        {
            public bool InPlay = true;
            public ForkAmpDisplay Display;
            public float Rate;
            public float TargetRate;
            public const float TargetRange = 15;
            public float Effectiveness => 1 - Calc.Clamp(MathHelper.Distance(Rate, TargetRate), 0, TargetRange) / TargetRange;
            public bool Selected
            {
                get { return Display is not null && Display.Selected; }
                set { if (Display is not null) Display.Selected = value; }
            }
            public bool OnLeft;
            public void Play()
            {
                Scene.Add(Display = new ForkAmpDisplay(Position, 25, 300));
                Display.InPlay = InPlay;

            }
            public void Stop()
            {
                Display.Removing = true;
            }
            public int Index;

            public Oscillator(Vector2 position, Osc.Wave wave, int index) : base(position)
            {
                Index = index;
            }
            public void SetSelected(int currentIndex)
            {
                Selected = Index == currentIndex;
            }
            public override void Update()
            {
                base.Update();
                if (Display is not null)
                {
                    Display.InPlay = InPlay;
                    Rate = Display.Frequency;
                }

            }
        }
        public float SelectTimer;
        public float SelectDelay = 0.3f;
        public int CurrentIndex;
        public SoundSource Sound;

        public float SavedVolume;
        public bool PlayedOnce;
        public int DisplaysUnlocked;
        public const float TargetRange = 15;
        public ForkAmp(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Depth = 1;
            Sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/forkAmp/");
            Sprite.AddLoop("idle", "texture", 0.1f);
            Sprite.Play("idle");
            Add(Sprite);
            Collider = new Hitbox(Sprite.Width, Sprite.Height);
            Position -= new Vector2(Sprite.Width / 2, Sprite.Height / 2);
            Add(Talk = new DotX3(Collider, Interact));
            Oscillators = new Oscillator[4];
            float space = 16;
            for (int i = 0; i < 4; i++)
            {
                Vector2 position = Position - Vector2.UnitX * space * 2.5f;
                position.X += (i / 4f) * (space * 4);
                Oscillators[i] = new(position, (Osc.Wave)i, i);
            }
            Add(Sound = new SoundSource());

            Oscillators[0].SetSelected(0);
            flags = new string[4]
            {
                data.Attr("firstFlag"),data.Attr("secondFlag"),data.Attr("thirdFlag"),data.Attr("fourthFlag")
            };
        }
        public override void Update()
        {
            base.Update();
            
            DisplaysUnlocked = 0;
            for (int i = 0; i < 4; i++)
            {
                if (!string.IsNullOrEmpty(flags[i]) && SceneAs<Level>().Session.GetFlag(flags[i]))
                {
                    Oscillators[i].InPlay = true;
                    
                }
                else
                {
                    Oscillators[i].InPlay = false;
                }
            }
            PianoModule.Session.ForkAmpState.Unlocked = DisplaysUnlocked;
            for (int i = 0; i < 4; i++)
            {
                PianoModule.Session.ForkAmpState.Effectiveness[i] = Oscillators[i].Effectiveness;
            }
            if (SelectTimer <= 0)
            {
                if (Input.MoveX != 0)
                {
                    if(Input.MoveX.Value != 0)
                    {
                        for(int i = CurrentIndex + Input.MoveX.Value; i<4 && i >= 0; i+= Input.MoveX.Value)
                        {
                            if (Oscillators[i].InPlay)
                            {
                                CurrentIndex = Oscillators[i].Index;
                                
                            }
                        }
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        Oscillators[i].SetSelected(CurrentIndex);
                    }
                    SelectTimer = 0.3f;
                }
            }
            else
            {
                SelectTimer -= Engine.DeltaTime;
            }

        }
        public void Interact(Player player)
        {
            player.StateMachine.State = Player.StDummy;
            Scene.Add(Oscillators);
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
            StartOscillators();
            while (!Input.Jump)
            {
                for (int i = 0; i < 4; i++)
                {
                    Sound.Param("Osc " + (i + 1), Oscillators[i].Rate);
                    PianoModule.Session.ForkAmpState.Rates[i] = Oscillators[i].Rate;
                }
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
            foreach (Oscillator osc in Oscillators)
            {
                osc.Play();
            }
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
            foreach (Oscillator osc in Oscillators)
            {
                osc.Stop();
            }
            Add(new Coroutine(FadeMusicIn()));
        }
    }

}
