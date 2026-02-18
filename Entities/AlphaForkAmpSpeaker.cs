using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.FrequencyEntities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System.Collections;
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/AlphaForkAmpSpeaker")]
    [Tracked]
    public class AlphaForkAmpSpeaker : Entity
    {
        public float Volume;
        private float muffleMult = 1;
        public SoundSource Sound;
        private float[] targetRates = new float[4];
        private float[] amounts = new float[4];
        public FlagList Flags;
        public float Distortion { get; private set; }
        public AlphaForkAmpSpeaker(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Depth = 1;
            Image image = new Image(GFX.Game["objects/PuzzleIslandHelper/forkAmp/isolatedSpeaker"]);
            Add(image);
            if (data.Attr("facing", "Right") == "Left")
            {
                image.Effects = SpriteEffects.FlipHorizontally;
            }
            for (int i = 0; i < 4; i++)
            {
                targetRates[i] = data.Float("rate" + (char)(i + 65));
            }
            Flags = data.FlagList("flag");
            Collider = new Circle(64, image.Width / 2, image.Height / 2);
            Add(Sound = new SoundSource());
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            float distort = 0;
            for (int i = 0; i < 4; i++)
            {
                distort += amounts[i];
            }
            Distortion = distort / 4;
            if (Distortion == 1)
            {
                Flags.State = true;
            }
        }
        public override void Update()
        {
            base.Update();
            if (Scene.GetPlayer() is not Player player) return;
            float dist = Vector2.DistanceSquared(Center, player.Center);
            float target = 1 - (Calc.Clamp(dist, 0, 8) / 8f);
            bool muffled = Scene.CollideCheck<Solid>(Center, player.Center);
            muffleMult = Calc.Approach(muffleMult, muffled ? 0.4f : 0, Engine.DeltaTime * 3);
            if (target == 0 && Sound.InstancePlaying)
            {
                Sound.Stop();
            }
            else if (target > 0 && !Sound.InstancePlaying)
            {
                Sound.Play("event:/PianoBoy/Soundwaves/tuningForkLoop");
            }
            if (Sound.InstancePlaying)
            {
                Sound.instance.setVolume(Volume * muffleMult);
            }
            float distort = 0;
            for (int i = 0; i < 4; i++)
            {
                if (amounts[i] != 1 && MathHelper.Distance(targetRates[i], FrequencyData.GetRate(Scene, i)) < 4)
                {
                    amounts[i] = Calc.Approach(amounts[i], 1, Engine.DeltaTime);
                }
                else if (amounts[i] != 0)
                {
                    amounts[i] = Calc.Approach(amounts[i], 0, Engine.DeltaTime);
                }
                distort += amounts[i];
                Sound.Param("Osc " + (i + 1), FrequencyData.GetRate(Scene, i));
            }
            distort /= 4f;
            Volume = Calc.Approach(Volume, target, Engine.DeltaTime * 4f);
            //todo: figure out how to use snapshots and see if you can dynamically change the music volume...
            // ...without messing with the player's volume settings
            Sound.Param("Distort", distort);
            if (distort == 1 && Distortion < 1)
            {
                Flags.State = true;
            }
            Distortion = distort;
        }
    }
}
