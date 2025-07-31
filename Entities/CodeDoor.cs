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
        public FlagList Flag;
        private Sprite doorSprite;
        private Vector2 orig_Position;
        private bool Unlocking;
        private bool Unlocked;
        private readonly bool sideways;
        public CodeDoor(EntityData data, Vector2 offset) : base(data.Position + offset, 8, 48, false)
        {
            Tag |= Tags.TransitionUpdate;
            Flag = new FlagList(data.Attr("flag"));
            sideways = data.Bool("sideways");
            Add(doorSprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/machineDoor/"));
            doorSprite.AddLoop("idle", "codeIdle", 0.1f);
            doorSprite.AddLoop("unlocked", "codeUnlock", 0.1f, 10);
            doorSprite.Add("unlock", "codeUnlock", 0.1f, "unlocked");
            doorSprite.Rate = 1.5f;
            if (sideways)
            {
                doorSprite.CenterOrigin();
                doorSprite.Position += new Vector2(doorSprite.Height / 2, doorSprite.Width / 2);
                doorSprite.Rotation = 90f.ToRad();
            }
            Collider = new Hitbox(sideways ? doorSprite.Height : doorSprite.Width, sideways ? doorSprite.Width : doorSprite.Height);
            Add(new LightOcclude());
            orig_Position = Position;
            Depth = -1;
            doorSprite.Play("idle");
        }
        private IEnumerator UnlockRoutine()
        {
            Unlocked = true;
            Vector2 orig = orig_Position;
            Vector2 shakeVector = sideways ? Vector2.UnitX : Vector2.UnitY;
            for (int i = 0; i < 6; i++)
            {
                Position = orig + shakeVector;
                shakeVector *= -1;
                yield return null;
            }
            Position = orig;
            Vector2 offset = sideways ? Vector2.UnitX * -Width : Vector2.UnitY * -Height;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 0.25f)
            {
                Position = Vector2.Lerp(orig, orig + offset, Ease.QuintIn(i));
                yield return null;
            }
            Position = orig + offset;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (Flag.State)
            {
                Unlocking = true;
                Unlocked = true;
                doorSprite.Play("unlocked");
                Vector2 offset = sideways ? Vector2.UnitX * -Width : Vector2.UnitY * -Height;
                Position = orig_Position + offset;
            }
        }
        public void Unlock()
        {
            if (Unlocked || Unlocking) return;
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
            if (!Unlocked && !Unlocking && Flag.State)
            {
                Unlock();
            }
        }
    }
}
