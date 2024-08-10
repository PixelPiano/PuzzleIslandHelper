using Celeste.Mod.Entities;
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
        private bool usesFlag;
        private Sprite doorSprite;
        private Vector2 orig_Position;
        private bool Unlocking;
        private bool Unlocked;
        private Vector2 destination;
        private bool Sideways;
        public CodeDoor(Vector2 position, string flag, bool sideways, bool usesFlag = true) : base(position, 8, 48, false)
        {
            Tag |= Tags.TransitionUpdate;
            this.flag = flag;
            Sideways = sideways;
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
            orig_Position = Position;
            Depth = -1;
            doorSprite.Play("idle");
            destination = new Vector2(Sideways ? orig_Position.X - Width : Position.X, Sideways ? Position.Y : orig_Position.Y - Width);
            this.usesFlag = usesFlag;
        }
        public CodeDoor(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Attr("flag"), data.Bool("sideways"))
        {

        }
        private IEnumerator UnlockRoutine()
        {
            Unlocked = true;
            Vector2 orig = Position;
            Vector2 shakeVector = Sideways ? Vector2.UnitX : Vector2.UnitY;
            for (int i = 0; i < 6; i++)
            {
                Position = orig + shakeVector;
                shakeVector *= -1;
                yield return null;
            }
            Position = orig;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 0.25f)
            {
                Position = Vector2.Lerp(orig, destination, Ease.QuintIn(i));
                yield return null;
            }
            Position = destination;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (usesFlag && (scene as Level).Session.GetFlag(flag))
            {
                Unlocking = true;
                Unlocked = true;
                doorSprite.Play("unlocked");
                Position = destination;
            }
        }
        public void Unlock()
        {
            if(Unlocked || Unlocking) return;
            Unlocking = true;
            Audio.Play("event:/game/09_core/frontdoor_heartfill", Position);
            doorSprite.Play("unlock");
            doorSprite.OnChange = (s, s2) =>
            {
                if (s == "unlock" && s2 == "unlocked" && !Unlocked)
                {
                    Add(new SoundSource("event:/PianoBoy/labDoorOpen"));
                    Add(new Coroutine(UnlockRoutine()));
                }
            };
        }
        public override void Update()
        {
            base.Update();
            if (!Unlocked && usesFlag && SceneAs<Level>().Session.GetFlag(flag) && !Unlocking)
            {
                Unlock();
            }
        }
    }
}
