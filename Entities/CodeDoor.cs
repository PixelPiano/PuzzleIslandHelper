using Celeste.Mod.Entities;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
// PuzzleIslandHelper.CodeDoor
namespace Celeste.Mod.PuzzleIslandHelper.Entities
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
        public CodeDoor(EntityData data, Vector2 offset)
            : base(data.Position + offset, 8, 48, false)
        {
            Tag |= Tags.TransitionUpdate;
            flag = data.Attr("flag");
            Add(doorSprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/machineDoor/"));
            doorSprite.AddLoop("idle", "codeIdle", 0.1f);
            doorSprite.AddLoop("unlocked", "codeUnlock", 0.1f, 10);
            doorSprite.Add("unlock", "codeUnlock", 0.1f, "unlocked");
            doorSprite.Rate = 1.5f;
            Collider = new Hitbox(8, 48);
            Add(new LightOcclude());
            orig_Position = Position;
            Depth = -1;
            doorSprite.Play("idle");
        }
        private IEnumerator Unlock()
        {
            Unlocked = true;
            float speed = 5;
            float destination = orig_Position.Y - doorSprite.Height;

            float origY = Position.Y;
            float origX = Position.X;
            bool left = true;
            bool shake = true;
            float pos = Position.Y;
            int buffer = 6;
            int bufferProg = buffer;
            for (float i = 0; i < 1; i += Engine.DeltaTime * speed)
            {
                if (shake)
                {
                    if (left)
                    {
                        Position.X--;
                    }
                    else
                    {
                        Position.X++;
                    }
                    left = !left;
                }
                shake = !shake;
                bufferProg--;
                pos = (int)Calc.LerpClamp(origY, destination, Ease.QuintIn(i));
                if(bufferProg == 0)
                {
                    bufferProg = buffer;
                    Position.Y = pos;
                }
                yield return null;
            }
            Position.Y = destination;
            Position.X = origX;
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
                    Position = orig_Position - Vector2.UnitY * doorSprite.Height;
                }
                Buffer = true;
            }
            if (level.Session.GetFlag(flag) && !Unlocking)
            {
                Unlocking = true;
                Audio.Play("event:/game/09_core/frontdoor_heartfill", Position);
                doorSprite.Play("unlock");
                doorSprite.OnChange = (string s, string s2) =>
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
