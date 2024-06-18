using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
// PuzzleIslandHelper.CodeDoor
namespace Celeste.Mod.PuzzleIslandHelper.Entities.PuzzleEntities
{
    [CustomEntity("PuzzleIslandHelper/CodeDoor")]
    [Tracked]
    public class CodeDoor : Solid
    {
        public string flag;

        private Sprite doorSprite;

        private Vector2 orig_Position;

        private bool Unlocking;
        private bool Unlocked;

        private Level level;
        private bool Buffer;

        private bool Sideways;
        public CodeDoor(EntityData data, Vector2 offset)
            : base(data.Position + offset, 8, 48, false)
        {
            Tag |= Tags.TransitionUpdate;
            flag = data.Attr("flag");
            Sideways = data.Bool("sideways");
            Add(doorSprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/machineDoor/"));
            doorSprite.AddLoop("idle", "codeIdle", 0.1f);
            doorSprite.AddLoop("unlocked", "codeUnlock", 0.1f, 10);
            doorSprite.Add("unlock", "codeUnlock", 0.1f, "unlocked");
            doorSprite.Rate = 1.5f;
            if (Sideways)
            {
                doorSprite.CenterOrigin();
                doorSprite.Position += new Vector2(doorSprite.Height / 2, doorSprite.Width / 2);
                doorSprite.Rotation = 90f.ToRad();
            }
            Collider = new Hitbox(Sideways ? doorSprite.Height : doorSprite.Width, Sideways ? doorSprite.Width : doorSprite.Height);
            Add(new LightOcclude());
            if (Sideways) Position.X += 8;
            orig_Position = Position;
            Depth = -1;
            doorSprite.Play("idle");
        }
        private IEnumerator Unlock()
        {
            Unlocked = true;
            float speed = 5;
            float destination = Sideways ? orig_Position.X - doorSprite.Width : orig_Position.Y - doorSprite.Height;

            float origY = Position.Y;
            float origX = Position.X;
            bool left = true;
            bool shake = true;
            float pos = Sideways ? Position.X : Position.Y;
            int buffer = 6;
            int bufferProg = buffer;
            for (float i = 0; i < 1; i += Engine.DeltaTime * speed)
            {
                if (shake)
                {
                    if (left)
                    {
                        if (Sideways)
                        {
                            Position.X--;
                        }
                        else
                        {
                            Position.Y--;
                        }

                    }
                    else
                    {
                        if (Sideways)
                        {
                            Position.X++;
                        }
                        else
                        {
                            Position.Y++;
                        }
                    }
                    left = !left;
                }
                shake = !shake;
                bufferProg--;
                pos = (int)Calc.LerpClamp(Sideways ? origX : origY, destination, Ease.QuintIn(i));
                if (bufferProg == 0)
                {
                    bufferProg = buffer;
                    if (Sideways)
                    {
                        Position.X = pos;
                    }
                    else
                    {
                        Position.Y = pos;
                    }

                }
                yield return null;
            }
            Position.Y = Sideways ? origY : destination;
            Position.X = Sideways ? destination : origX;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            level = scene as Level;
        }
        public override void Update()
        {
            base.Update();
            if (!Buffer)
            {
                if (level.Session.GetFlag(flag))
                {
                    Unlocking = true;
                    Unlocked = true;
                    doorSprite.Play("unlocked");
                    Position = orig_Position - (Sideways ? Vector2.UnitX * doorSprite.Width : Vector2.UnitY * doorSprite.Height);
                }
                Buffer = true;
            }
            if (level.Session.GetFlag(flag) && !Unlocking)
            {
                Unlocking = true;
                Audio.Play("event:/game/09_core/frontdoor_heartfill", Position);
                doorSprite.Play("unlock");
                doorSprite.OnChange = (s, s2) =>
                {
                    if (s == "unlock" && s2 == "unlocked" && !Unlocked)
                    {
                        Add(new SoundSource("event:/PianoBoy/labDoorOpen"));
                        Add(new Coroutine(Unlock()));
                    }
                };
            }
        }
    }
}
