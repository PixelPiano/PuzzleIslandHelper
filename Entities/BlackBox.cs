using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/BlackBox")]
    [Tracked]
    public class BlackBox : Entity
    {
        public string Flag;
        private bool inverted;
        private float fadeTime;
        public float Alpha;
        public bool FlagState => Scene is Level level && (string.IsNullOrEmpty(Flag) || level.Session.GetFlag(Flag) != inverted);
        private bool prevState;
        private Coroutine routine;
        public BlackBox(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Flag = data.Attr("flag");
            inverted = data.Bool("inverted");
            Collider = new Hitbox(data.Width, data.Height);
            Depth = data.Int("depth");
            routine = new Coroutine(false);
            Add(routine);
            Tag |= Tags.TransitionUpdate;
        }
        public override void Update()
        {
            base.Update();
            if(prevState != FlagState)
            {
                routine.Replace(FadeTo(prevState ? 1 : 0));
            }
        }
        private IEnumerator FadeTo(float to)
        {
            float from = Alpha;
            for(float i = 0; i<1; i+=Engine.DeltaTime / fadeTime)
            {
                Alpha = Calc.LerpClamp(from, to, i);
                yield return null;
            }
        }
    }
}
