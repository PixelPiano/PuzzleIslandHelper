using Celeste.Mod.Entities;
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
    [CustomEntity("PuzzleIslandHelper/SteamEmitter")]
    [Tracked]
    public class SteamEmitter : Entity
    {
        public string Flag;
        public enum Directions
        {
            Right, Up, Left, Down
        }
        public float Angle;
        private float delay;
        private Vector2 emitOffset;
        private float timer;
        private float interval;
        public SteamEmitter(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Angle = (MathHelper.Pi / -2f) * (float)data.Enum<Directions>("direction");
            interval = data.Float("interval",0.3f);
            Flag = data.Attr("flag");
            Collider = new Hitbox(8,8);
            emitOffset = Calc.AngleToVector(Angle, 4);

        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            delay = Calc.Random.NextFloat();
            timer = delay + interval;
        }
        public override void Update()
        {
            base.Update();
            if(Scene is not Level level) return;
            if(string.IsNullOrEmpty(Flag) || level.Session.GetFlag(Flag))
            {
                if(timer <= 0)
                {
                    level.ParticlesBG.Emit(ParticleTypes.Steam,Center + emitOffset,Color.White,Angle);
                    timer = interval;
                }
                else timer -= Engine.DeltaTime;
            }
        }
    }
}
