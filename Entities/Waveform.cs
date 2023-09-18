using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using FMOD.Studio;

// PuzzleIslandHelper.Waveform
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/Waveform")]
    public class Waveform : Entity
    {
        public string flag;

        private string spriteName;

        private MTexture spectrogram;

        private MTexture subtex;

        private float width;

        private string sound;

        private bool playNew;

        private EventInstance sfx;

        private float position;

        private float length;
           
        private Color color;
        public Waveform(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            color = Calc.HexToColor(data.Attr("color"));
            flag = data.Attr("flag");
            position = 1;
            length = 1;
            playNew = true;
            spriteName = data.Attr("sprite");
            spectrogram = GFX.Game[spriteName];
            subtex = spectrogram.GetSubtexture(0, 0, spectrogram.Width, spectrogram.Height, subtex);
            sound = data.Attr("event", "event:/PianoBoy/defaultWaveform");
        }

        public override void Update()
        {
            base.Update();
            if (string.IsNullOrEmpty(flag) || SceneAs<Level>().Session.GetFlag(flag))
            {

                if (playNew || string.IsNullOrEmpty(flag)) //DON'T CHANGE
                {
                    sfx = Audio.Play(sound);
                    sfx.getDescription(out EventDescription desc);
                    desc.getLength(out int sfx_length);
                    length = sfx_length;
                    //Logger.Log(LogLevel.Warn, "PuzzleLog", "Played an event");
                    //Logger.Log(LogLevel.Warn, "PuzzleLog", spriteName);
                }

                sfx.getTimelinePosition(out int pos);
                position = pos;
                playNew = false;
                width = MathHelper.Lerp(0, spectrogram.Width, position / length);
                subtex = spectrogram.GetSubtexture(0, 0, (int)width, spectrogram.Height, subtex);
            }
            else
            {
                Audio.Stop(sfx);
                playNew = true;
                width = 0f;
                subtex = spectrogram.GetSubtexture(0, 0, (int)width, spectrogram.Height, subtex);
                
            }
        }

        public override void Render()
        {
            base.Render();
            if (subtex != null)
            {
                if (string.IsNullOrEmpty(flag) || SceneAs<Level>().Session.GetFlag(flag))
                {
                    subtex.Draw(Position, Vector2.Zero, color);
                }
            }
        }
    }
}