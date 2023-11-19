using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Triggers;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using static MonoMod.InlineRT.MonoModRule;

// PuzzleIslandHelper.LabElevator
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/LabElevator")]
    public class LabElevator : Solid
    {
        private int counter;
        private bool moving;
        private CustomTalkComponent upButton;
        private CustomTalkComponent downButton;
        private float jitterAmount;
        private float moveSpeed;
        private float moveTime;
        private Vector2 startPosition;
        private Vector2 endPosition;
        public string flag;
        private Sprite doorSprite;
        private Image backGlass;
        private Image frontGlass;
        private Sprite buttonPanel;
        public Dictionary<int, float> Floors = new();
        private Entity back;
        private Entity front;
        private StoolPickupBarrier Barrier;
        private SoundSource click;
        private Sprite hover;
        private InvisibleBarrier barrierOne;
        private InvisibleBarrier barrierTwo;
        private InvisibleBarrier barrierThree;
        private static int Clicks;
        private float animBuffer;
        public string ElevatorID;
        private int CurrentFloor;
        private List<float> FloorAltitude = new();
        public LabElevator(EntityData data, Vector2 offset)
            : base(data.Position + offset, 48, 6, false)
        {
            ElevatorID = data.Attr("elevatorID");
            counter = 0;
            hover = new Sprite(GFX.Gui, "PuzzleIslandHelper/hover/");
            hover.AddLoop("idle", "digital", 0.1f, 9);
            hover.Add("intro", "digital", 0.07f, "idle");

            Tag |= Tags.TransitionUpdate;
            //Floors = data.Nodes.Length;
            List<Vector2> list = new();
            list = data.NodesWithPosition(offset).ToList();
            list.Add(Position);
            list = list.OrderBy(item => item.Y).ToList();

            foreach (Vector2 v in list)
            {
                FloorAltitude.Add(v.Y);
            }
            CurrentFloor = FloorAltitude.IndexOf(Position.Y);

            Sprite sprite = new Sprite(GFX.Gui, "PuzzleIslandHelper/hover/");
            sprite.AddLoop("idle", "digitalC", 0.1f);
            MTexture texture = GFX.Gui["PuzzleIslandHelper/hover/digitalC"];
            Add(upButton = new DotX3(8, -8, 12, 8, new Vector2(18, -10f), InteractUp));
            Add(downButton = new DotX3(32, -8, 12, 8, new Vector2(31, -10f), InteractDown));
            upButton.PlayerMustBeFacing = false;
            downButton.PlayerMustBeFacing = false;
            flag = data.Attr("flag");
            startPosition = Position;
            endPosition = Position + new Vector2(0f, data.Float("endPosition"));
            moveSpeed = data.Float("moveSpeed");
            moveTime = data.Float("moveTime");
            jitterAmount = data.Float("jitterAmount");
            Add(click = new SoundSource());
            Add(doorSprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/labElevator/"));
            doorSprite.AddLoop("idle", "idle", 0.1f);
            doorSprite.Rate = 1.5f;
            Collider = new Hitbox(48, 8, 0, 0);

            Depth = -10500;
            Add(new LightOcclude());
        }
        private void Interact(Player player, bool up)
        {
            if (player.Holding != null)
            {
                return;
            }
            //play click sound
            click.Position = (-Vector2.UnitY * buttonPanel.Height) + new Vector2(up ? 18 : 31, 40);
            click.UpdateSfxPosition();
            click.Play("event:/PianoBoy/Machines/ButtonPressC");
            PianoModule.Session.ButtonsPressed++;
            Clicks++;

            if (Clicks > 25 && !PianoModule.Session.RestoredPower)
            {
                PianoModule.Session.ThisTimeForSure = true;
            }

            if (!PianoModule.Session.RestoredPower)
            {
                return;
            }
            Add(new Coroutine(MoveElevator(up)));
        }
        private void InteractDown(Player player)
        {

            buttonPanel.Play("downPress");
            Interact(player, false);
        }
        private void InteractUp(Player player)
        {
            buttonPanel.Play("upPress");
            Interact(player, true);
        }
        public void JitterPlatforms(bool state)
        {
            counter++;
            if (counter == 5)
            {
                counter = 1;
                MoveToX(Position.X + jitterAmount);
            }
            //if (state)
            //else
            if (counter == 3)
            {
                MoveToX(Position.X - jitterAmount);
            }
        }
        private void SetStartingPosition(Scene scene)
        {
            if (!PianoModule.Session.RestoredPower)
            {
                float initialPosition = TryGetFloorLevel(1);
                ResetPlatforms(initialPosition);
                return;
            }
            Player player = (scene as Level).Tracker.GetEntity<Player>();
            float closest = 100000;
            int index = 0;
            for (int i = 0; i < Floors.Count; i++)
            {
                if (MathHelper.Distance(Floors[i], player.Position.Y) < MathHelper.Distance(closest, player.Position.Y))
                {
                    closest = Floors[i];
                    index = i;
                }
            }
            CurrentFloor = index;
            ResetPlatforms(closest);
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);

            SceneAs<Level>().Add(barrierOne = new InvisibleBarrier(new Vector2(Position.X, Position.Y - 30), 5, 13));
            SceneAs<Level>().Add(barrierTwo = new InvisibleBarrier(new Vector2(Position.X + 43, Position.Y - 30), 5, 13));
            SceneAs<Level>().Add(barrierThree = new InvisibleBarrier(new Vector2(Position.X, Position.Y - 40), 48, 10));
        }
        public override void Update()
        {
            base.Update();
            animBuffer += Engine.DeltaTime;
            upButton.Enabled = !moving;
            downButton.Enabled = !moving;
            Barrier.Position = Position - Vector2.UnitY * Barrier.Height;
            if (backGlass != null)
            {
                back.Position = Position;
            }
            if (frontGlass != null)
            {
                front.Position = Position;
            }
        }
        private int TryGetFloor(int floor)
        {
            return Floors.ContainsKey(floor) ? floor : CurrentFloor;
        }
        private float TryGetFloorLevel(int floor)
        {
            return Floors.ContainsKey(floor) ? Floors[floor] : Position.Y;
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);


            //Floors = data.Nodes.Length;
            List<ElevatorLevel> list = new();
            foreach (ElevatorLevel floor in scene.Tracker.GetEntities<ElevatorLevel>())
            {
                if (ElevatorID == floor.ElevatorID)
                {
                    if (!Floors.ContainsKey(floor.Floor))
                    {
                        Floors.Add(floor.Floor, floor.Position.Y);
                    }
                }
            }
            list = list.OrderBy(item => item.Floor).ToList();
            Floors.OrderBy(item => item.Key).ToList();


            foreach (ElevatorLevel v in list)
            {
                FloorAltitude.Add(v.Y);
            }
            CurrentFloor = TryGetFloor(1);
            scene.Add(back = new Entity(Position));


            scene.Add(front = new Entity(Position));
            back.Depth = 9000;
            front.Depth = -10500;
            back.Tag = Tag;
            front.Tag = Tag;
            backGlass = new Image(GFX.Game["objects/PuzzleIslandHelper/labElevator/glassBack"]);
            frontGlass = new Image(GFX.Game["objects/PuzzleIslandHelper/labElevator/glassFront"]);

            backGlass.Origin = new Vector2(0, 17);
            frontGlass.Origin = new Vector2(0, 40);
            back.Add(backGlass);
            buttonPanel = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/LabElevator/");
            buttonPanel.AddLoop("idle", "interact", 0.1f);
            buttonPanel.Add("upPress", "interactUp", 0.2f, "idle");
            buttonPanel.Add("downPress", "interactDown", 0.2f, "idle");
            buttonPanel.Y -= buttonPanel.Height;
            buttonPanel.Play("idle");

            buttonPanel.OnChange = (string s1, string s2) =>
                {
                    animBuffer = 0;
                };
            back.Add(buttonPanel);
            front.Add(frontGlass);
            doorSprite.Play("idle");
            scene.Add(Barrier = new StoolPickupBarrier(Position, (int)Width, (int)frontGlass.Height, 1, false, true, false));
            Barrier.Tag = Tag;
            Barrier.Depth = 9001;
            //SetStartingPosition(scene);
        }
        private void ResetPlatforms(float value)
        {
            MoveToY(value);
            barrierOne.MoveToY(value - 30);
            barrierTwo.MoveToY(value - 30);
            barrierThree.MoveToY(value - 40);
        }
        private void MovePlatforms(float value)
        {
            MoveTowardsY(value, moveSpeed * Engine.DeltaTime);
            barrierOne.MoveTowardsY(value - 30, moveSpeed * Engine.DeltaTime);
            barrierTwo.MoveTowardsY(value - 30, moveSpeed * Engine.DeltaTime);
            barrierThree.MoveTowardsY(value - 40, moveSpeed * Engine.DeltaTime);
        }
        public IEnumerator MoveElevator(bool up)
        {
            bool jitterState = false;
            if (!moving)
            {
                moving = true;
                Barrier.State = true;
                yield return null;
                while (Barrier.Opacity < 1)
                {
                    Barrier.Opacity += Engine.DeltaTime;
                    yield return null;
                }
                Barrier.Opacity = 1;
                yield return Engine.DeltaTime * 2;

                bool AtEdge = up ? CurrentFloor - 1 < 0 : CurrentFloor + 1 >= Floors.Count;
                float Altitude = AtEdge ? Position.Y : up ? Floors[CurrentFloor - 1] : Floors[CurrentFloor + 1];

                if (!AtEdge)
                {
                    while (up ? Position.Y > Altitude : Position.Y < Altitude)
                    {
                        MovePlatforms(Altitude);
                        JitterPlatforms(jitterState);
                        jitterState = !jitterState;
                        yield return null;
                    }
                    Position.Y = Altitude;
                    MoveToX(Position.X);
                }
                else
                {
                    float yBump = Position.Y + (up ? -4 : 4);
                    float origY = Position.Y;
                    for (float i = 0; i < 1; i += 0.05f)
                    {
                        MovePlatforms(Calc.LerpClamp(origY, yBump, Ease.QuintIn(i)));
                        yield return null;

                    }
                    for (float i = 0; i < 1; i += 0.05f)
                    {
                        MovePlatforms(Calc.LerpClamp(yBump, origY, i));
                        yield return null;
                    }
                    Position.Y = origY;
                }
                if (!AtEdge)
                {
                    CurrentFloor += up ? -1 : 1;
                }
                Barrier.State = false;
                yield return null;
                while (Barrier.Opacity > 0)
                {
                    Barrier.Opacity -= Engine.DeltaTime;
                    yield return null;
                }
                Barrier.Opacity = 0;
                yield return Engine.DeltaTime * 2;
                moving = false;
            }
        }
    }
}