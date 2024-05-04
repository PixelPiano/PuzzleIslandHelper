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

        public int closeRate = 7;
        public int openRate = 7;

        public int openSpeed = 30;

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
        public Collider Detect;
        public LabDoor(EntityData data, Vector2 offset)
            : base(data.Position + offset, 8, 48, false)
        {
            auto = data.Bool("automatic");
            flag = data.Attr("flag");
            State = data.Bool("startState") ? States.Open : States.Closed;
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
            Detect = new Hitbox(Width + range * 2, Height + 16, Position.X - range, Position.Y - 8);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            player = Scene.Tracker.GetEntity<Player>();
            if (State == States.Open)
            {
                doorSprite.Play("open");
            }
            else
            {
                doorSprite.Play("closed");
            }
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Draw.HollowRect(Detect, Color.Blue);
        }
        public void Open()
        {
            Add(new SoundSource("event:/PianoBoy/labDoorOpen"));
            doorSprite.Play("opening");
            State = States.Opening;
        }
        public void Close()
        {
            Add(new SoundSource("event:/PianoBoy/labDoorClose"));
            doorSprite.Play("closing");
            State = States.Closing;
        }
        public override void Update()
        {
            bool state = string.IsNullOrEmpty(flag) || SceneAs<Level>().Session.GetFlag(flag);
            bool collided = Detect.Collide(player);
            switch (State)
            {
                case States.Closed:
                    Collider.Height = height;
                    closeSpeed = 30;
                    doorSprite.Play("closed");
                    if ((!auto && state) || (auto && collided))
                    {
                        Open();
                    }
                    break;
                case States.Closing:
                    Collider.Height = Calc.Approach(Collider.Height, height, closeSpeed * Engine.DeltaTime);
                    closeSpeed += closeRate;
                    if (Collider.Height >= height)
                    {
                        Collider.Height = height;
                        State = States.Closed;
                    }
                    break;
                case States.Open:
                    Collider.Height = 0;
                    openSpeed = 30;
                    doorSprite.Play("open");
                    if ((!auto && !state) || (auto && !collided))
                    {
                        Close();
                    }
                    break;
                case States.Opening:
                    Collider.Height = Calc.Approach(Collider.Height, 0, openSpeed * Engine.DeltaTime);
                    openSpeed += openRate;
                    if (Collider.Height <= 0)
                    {
                        Collider.Height = 0;
                        State = States.Open;
                    }
                    break;
            }

            base.Update();
        }
    }
}
