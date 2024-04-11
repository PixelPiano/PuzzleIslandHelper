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

        public int openSpeed = 100;

        public int closeSpeed = 30;

        private int height = 48;

        private Player player;

        public enum States
        {
            Closed,
            Closing,
            Open,
            Opening
        }
        public States State;
        public States prevState;
        public LabDoor(EntityData data, Vector2 offset)
            : base(data.Position + offset, 8, 48, false)
        {
            auto = data.Bool("automatic");
            flag = data.Attr("flag");
            start = data.Attr("startState");
            // TODO: read properties from data
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
        public override void Update()
        {

            prevState = State;
            if (!auto)
            {
                bool flagState = SceneAs<Level>().Session.GetFlag(flag);
                if (flagState && State != States.Open)
                {
                    State = States.Opening;
                }
                else if (!flagState && State != States.Closed)
                {
                    State = States.Closing;
                }
            }
            else
            {
                if (player.Position.Y < Y + Height + 8 && player.Position.Y > Y - 8
                    && (player.Position.X <= X + Width + range && player.Position.X > X + Width
                    || player.Position.X >= X - range && player.Position.X < X))
                {
                    State = States.Opening;
                }
                if ((player.Position.Y > Y + Height + 8 || player.Position.Y < Y - 8)
                    && (player.Position.X > X + Width + range || player.Position.X < X - range))
                {
                    State = States.Closing;
                }
            }
            if (prevState != State)
            {
                switch (State)
                {
                    case States.Opening:
                        Add(new SoundSource("event:/PianoBoy/labDoorOpen"));
                        doorSprite.Play("opening");
                        break;
                    case States.Closing:
                        Add(new SoundSource("event:/PianoBoy/labDoorClose"));
                        doorSprite.Play("closing");
                        break;
                }
            }
            switch (State)
            {
                case States.Closed:
                    Collider.Height = height;
                    closeSpeed = 30;
                    doorSprite.Play("closed");
                    break;
                case States.Open:
                    Collider.Height = 0;
                    doorSprite.Play("open");
                    break;
                case States.Closing:
                    Collider.Height = Calc.Approach(Collider.Height, height, closeSpeed * Engine.DeltaTime);
                    closeSpeed += 7;
                    break;
                case States.Opening:
                    Collider.Height = Calc.Approach(Collider.Height, 0, openSpeed * Engine.DeltaTime);
                    break;
            }
            if (Collider.Height == height)
            {
                State = States.Closed;
            }
            if (Collider.Height == 0)
            {
                State = States.Open;
            }
            base.Update();
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            player = Scene.Tracker.GetEntity<Player>();
        }
    }
}
