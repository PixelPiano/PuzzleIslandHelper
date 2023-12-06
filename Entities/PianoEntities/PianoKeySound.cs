using Celeste.Mod.CommunalHelper;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.PianoEntities
{
    [Tracked]
    public class PianoKeySound : SoundSource
    {
        public string Key;
        public static bool ForceDamp;
        public static bool Sustain;
        public EventInstance Instance;
        public Vector2 MiddleOfScreen
        {
            get
            {
                if (Scene is not Level level)
                {
                    return Entity.Position;
                }
                return level.Camera.GetBounds().Center.ToVector2();
            }
        }

        public PianoKeySound() : base()
        {
        }
        public new SoundSource Play(string path, string param = null, float value = 0f)
        {
            Stop(true);
            EventName = path;
            EventDescription eventDescription = Audio.GetEventDescription(path);
            if (eventDescription != null)
            {
                eventDescription.createInstance(out instance);
                eventDescription.is3D(out is3D);
                eventDescription.isOneshot(out isOneshot);
            }

            if (instance != null)
            {
                if (is3D)
                {
                    Vector2 position = Position;
                    if (Entity != null)
                    {
                        position += Entity.Position;
                    }

                    Audio.Position(instance, position);
                }

                if (param != null)
                {
                    instance.setParameterValue(param, value);
                }

                instance.start();
                Playing = true;
            }

            return this;
        }
        public void Play(string path, float damp)
        {
            Play(path);
            Param("fade", 1);
            Param("damp", ForceDamp ? 0 : damp);
        }
        public void Release(bool sustain = false)
        {
            if (!sustain)
            {
                Param("fade", 0);
            }
        }
    }
}