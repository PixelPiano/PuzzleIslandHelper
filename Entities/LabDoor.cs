using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
// PuzzleIslandHelper.LabDoor
namespace Celeste.Mod.PuzzleIslandHelper.Entities
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
        private bool dependsOnLabPower;
        private bool firstFrame = true;
        public bool PowerState
        {
            get
            {
                return !dependsOnLabPower || PianoModule.Session.RestoredPower;
            }
        }
        public LabDoor(EntityData data, Vector2 offset)
            : base(data.Position + offset, 8, 48, false)
        {
            dependsOnLabPower = data.Bool("dependsOnLabPower", true);
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
            Tag |= Tags.TransitionUpdate;
            Detect = new Hitbox(Width + range * 2, Height + 16, Position.X - range, Position.Y - 8);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            player = Scene.Tracker.GetEntity<Player>();
            doorSprite.Play(State != States.Open || !PowerState ? "closed" : "open");
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Draw.HollowRect(Detect, Color.Blue);
        }
        public void Open()
        {
            if (!firstFrame)
            {
                Add(new SoundSource("event:/PianoBoy/labDoorOpen"));
            }
            doorSprite.Play("opening");
            State = States.Opening;
        }
        public void Close()
        {
            if (!firstFrame)
            {
                Add(new SoundSource("event:/PianoBoy/labDoorClose"));
            }
            doorSprite.Play("closing");
            State = States.Closing;
        }
        public override void Update()
        {
            if (!PowerState)
            {
                InstantClose();
            }
            else
            {
                bool flagState = string.IsNullOrEmpty(flag) || SceneAs<Level>().Session.GetFlag(flag);
                bool collided = Detect.Collide(player);
                switch (State)
                {
                    case States.Closed:
                        if ((!auto && flagState) || (auto && collided))
                        {
                            Open();
                        }
                        break;
                    case States.Closing:
                        Collider.Height = Calc.Approach(Collider.Height, height, closeSpeed * Engine.DeltaTime);
                        closeSpeed += closeRate;
                        if (Collider.Height >= height)
                        {
                            InstantClose();
                        }
                        break;
                    case States.Open:
                        if ((!auto && !flagState) || (auto && !collided))
                        {
                            Close();
                        }
                        break;
                    case States.Opening:
                        Collider.Height = Calc.Approach(Collider.Height, 0, openSpeed * Engine.DeltaTime);
                        openSpeed += openRate;
                        if (Collider.Height <= 0)
                        {
                            InstantOpen();
                        }
                        break;
                }
            }
            base.Update();
            firstFrame = false;
        }
        public void InstantClose()
        {
            Collider.Height = height;
            closeSpeed = 30;
            doorSprite.Play("closed");
            State = States.Closed;
        }
        public void InstantOpen()
        {
            Collider.Height = 0;
            openSpeed = 30;
            doorSprite.Play("open");
            State = States.Open;
        }
    }
}
