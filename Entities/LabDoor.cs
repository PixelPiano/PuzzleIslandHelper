using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
// PuzzleIslandHelper.LabDoor
namespace Celeste.Mod.PuzzleIslandHelper.Entities.GameplayEntities
{
    [CustomEntity("PuzzleIslandHelper/LabDoor")]
    [Tracked]
    public class LabDoor : Solid
    {
        public bool transitioning;

        public string flag;

        public bool auto;

        private float range;

        private Sprite doorSprite;

        public string start;

        public static int openSpeed = 100;

        public static int closeSpeed = 30;

        private int height = 48;

        private bool moving = false;

        private bool doorState;

        private Player player;
        public LabDoor(EntityData data, Vector2 offset)
            : base(data.Position + offset, 8, 48, false)
        {
            auto = data.Bool("automatic");
            flag = data.Attr("flag");
            start = data.Attr("startState");
            // TODO: read properties from data
            Add(doorSprite = GFX.SpriteBank.Create("labDoor"));
            Add(doorSprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/machineDoor/"));
            doorSprite.AddLoop("idle", "idle", 0.1f);
            doorSprite.AddLoop("open", "open", 0.1f, 8);
            doorSprite.AddLoop("closed", "close", 0.1f, 8);
            doorSprite.Add("opening", "open", 0.1f, "open");
            doorSprite.Add("closing", "close", 0.1f, "idle");
            doorSprite.Rate = 1.5f;
            Collider = new Hitbox(8, 48, 0, 0);
            range = data.Float("range") * 8;
            Depth = -8500;
            Add(new LightOcclude());
        }
        public IEnumerator Open()
        {
            doorState = true;
            doorSprite.Play("opening");
            while (Collider.Height != 0)
            {
                Collider.Height = Calc.Approach(Collider.Height, 0, openSpeed * Engine.DeltaTime);
                yield return null;
            }

            moving = false;
        }
        public IEnumerator Close()
        {
            doorState = false;
            doorSprite.Play("closing");
            while (Collider.Height != height)
            {
                Collider.Height = Calc.Approach(Collider.Height, height, closeSpeed * Engine.DeltaTime);
                closeSpeed += 7;
                yield return null;
            }
            closeSpeed = 30;
            moving = false;
        }
        public void stateChange(bool state)
        {
            if (!moving)
            {
                if (state && !doorState)
                {
                    moving = true;
                    Add(new SoundSource("event:/PianoBoy/labDoorOpen"));
                    Add(new Coroutine(Open()));
                }
                else if (!state && doorState)
                {
                    moving = true;
                    Add(new SoundSource("event:/PianoBoy/labDoorClose"));
                    Add(new Coroutine(Close()));
                }
            }
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            player = Scene.Tracker.GetEntity<Player>();
            if (player != null)
            {
                if (SceneAs<Level>().Session.GetFlag(flag) && !auto
                     || player.Position.Y < Y + Height + 8 && player.Position.Y > Y - 8
                     && (player.Position.X <= X + Width + range && player.Position.X > X + Width
                     || player.Position.X >= X - range && player.Position.X < X))
                {
                    doorState = true;
                    doorSprite.Play("open");
                    Collider = new Hitbox(8, 0, 0, 0);
                }
                else
                {
                    doorState = false;
                    doorSprite.Play("idle");
                    Collider = new Hitbox(8, 48, 0, 0);
                }
            }
        }
        public override void Update()
        {
            base.Update();
            player = Scene.Tracker.GetEntity<Player>();

            if (!auto)
            {
                stateChange(SceneAs<Level>().Session.GetFlag(flag));
            }
            else
            {
                if (player.Position.Y < Y + Height + 8 && player.Position.Y > Y - 8
                    && (player.Position.X <= X + Width + range && player.Position.X > X + Width
                    || player.Position.X >= X - range && player.Position.X < X))
                {
                    stateChange(true);
                }
                if ((player.Position.Y > Y + Height + 8 || player.Position.Y < Y - 8)
                    && (player.Position.X > X + Width + range || player.Position.X < X - range))
                {
                    stateChange(false);
                }
            }
        }
    }
}
