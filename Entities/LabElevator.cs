using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/LabElevator")]
    [Tracked]
    public class LabElevator : Solid
    {
        public bool Moving;
        public bool CanBeMoved
        {
            get
            {
                if (reliesOnLabPower)
                {
                    return PianoModule.Session.RestoredPower;
                }
                else
                {
                    return true;
                }
            }
        }
        private readonly CustomTalkComponent upButton;
        private readonly CustomTalkComponent downButton;
        private readonly float moveSpeed;
        private readonly Sprite doorSprite;
        private Sprite buttonPanel;
        public List<Vector2> Floors = new();
        private Entity back;
        private Entity front;
        private StoolPickupBarrier Barrier;
        private readonly SoundSource click;
        private readonly SoundSource moveSound;
        private InvisibleBarrier[] Barriers = new InvisibleBarrier[3];
        private Vector2 bOneOffset;
        private Vector2 bTwoOffset;
        private Vector2 bThreeOffset;
        private static int Clicks;
        public string ID;
        public int CurrentFloor;
        public int DefaultFloor;
        public bool SnapToClosestFloor;
        private bool reliesOnLabPower;
        public enum Events
        {
            Default,
            ButtonStuck,
            Broken
        }
        public Events Event;
        public static void SetFloor(string id, int floor)
        {
            if (Engine.Scene is Level level)
            {
                foreach (LabElevator le in level.Tracker.GetEntities<LabElevator>())
                {
                    if (le.CanBeMoved && le.ID == id)
                    {
                        le.SetToFloor(floor);
                    }
                }
            }
        }
        public IEnumerator MoveToEmpty(int floor)
        {
            yield return null;
        }
        public LabElevator(EntityData data, Vector2 offset)
            : base(data.Position + offset, 48, 6, false)
        {
            Depth = -10500;
            Tag |= Tags.TransitionUpdate;
            moveSpeed = data.Float("moveSpeed");
            DefaultFloor = data.Int("defaultFloor");
            SnapToClosestFloor = data.Bool("snapToClosestFloorOnSpawn");
            reliesOnLabPower = data.Bool("reliesOnLabPower");
            ID = data.Attr("elevatorID");
            Event = data.Enum<Events>("event");
            foreach (Vector2 vec in data.NodesWithPosition(offset))
            {
                Floors.Add(vec);
            }
            Floors.OrderBy(item => item.Y).ToList();

            Add(upButton = new DotX3(8, -8, 12, 8, new Vector2(18, -10f), InteractUp));
            Add(downButton = new DotX3(32, -8, 12, 8, new Vector2(31, -10f), InteractDown));
            Add(click = new SoundSource());
            Add(moveSound = new SoundSource());
            Add(doorSprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/labElevator/"));
            Add(new LightOcclude());

            upButton.PlayerMustBeFacing = false;
            downButton.PlayerMustBeFacing = false;
            doorSprite.AddLoop("idle", "idle", 0.1f);
            doorSprite.Rate = 1.5f;
            Collider = new Hitbox(48, 8, 0, 0);
            moveSound.Position = Center - Position;
        }
        private void StartMoveSound()
        {
            moveSound?.Play("event:/PianoBoy/Machines/ElevatorMoving", "Arrived", 0);
        }
        private void StopMoveSound()
        {
            moveSound?.Param("Arrived", 1);
        }
        private void Interact(Player player, bool up)
        {
            if (player.Holding != null)
            {
                return;
            }
            StartMoving(up);
        }
        public void StartMoving(bool up)
        {
            Add(new Coroutine(MoveElevator(!up)));
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
        public void SetToFloor(int floor)
        {
            CurrentFloor = floor;
            ResetPlatforms(GetFloor(floor));
        }
        /*        public void SetStartingPosition(Scene scene)
                {
                    if ((!PianoModule.Session.RestoredPower && reliesOnLabPower) || !SnapToClosestFloor)
                    {
                        SetToFloor(DefaultFloor);
                        return;
                    }
                    Player player = (scene as Level).Tracker.GetEntity<Player>();
                    float closest = int.MaxValue;
                    int index = 0;
                    for (int i = 0; i < Floors.Count; i++)
                    {
                        if (MathHelper.Distance(Floors[i].Y, player.Position.Y) < MathHelper.Distance(closest, player.Position.Y))
                        {
                            closest = Floors[i].Y;
                            index = i;
                        }
                    }
                    CurrentFloor = index;
                    ResetPlatforms(closest);
                }*/
        public override void Update()
        {
            base.Update();
            upButton.Enabled = !Moving;
            downButton.Enabled = !Moving;
            Barrier.Position = Position - Vector2.UnitY * Barrier.Height;
            if (back != null)
            {
                back.Position = Position;
            }
            if (front != null)
            {
                front.Position = Position;
            }
        }
        private float GetFloor(int floor)
        {
            if (floor < Floors.Count && floor >= 0) return Floors[floor].Y;
            return Position.Y;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);

            bOneOffset = new Vector2(0, -30);
            bTwoOffset = new Vector2(43, -30);
            bThreeOffset = new Vector2(0, -40);
            Barriers[0] = new InvisibleBarrier(Position + bOneOffset, 5, 13);
            Barriers[1] = new InvisibleBarrier(Position + bTwoOffset, 5, 13);
            Barriers[2] = new InvisibleBarrier(Position + bThreeOffset, 48, 10);

            scene.Add(Barriers);
            Image backGlass = new Image(GFX.Game["objects/PuzzleIslandHelper/labElevator/glassBack"]);
            Image frontGlass = new Image(GFX.Game["objects/PuzzleIslandHelper/labElevator/glassFront"]);

            backGlass.Origin = new Vector2(0, 17);
            frontGlass.Origin = new Vector2(0, 40);
            buttonPanel = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/LabElevator/");
            buttonPanel.AddLoop("idle", "interact", 0.1f);
            buttonPanel.Add("upPress", "interactUp", 0.2f, "idle");
            buttonPanel.Add("downPress", "interactDown", 0.2f, "idle");
            buttonPanel.Y -= buttonPanel.Height;
            buttonPanel.Play("idle");
            back = new Entity(Position)
            {
                Depth = 9000,
                Tag = Tag,
            };
            back.Add(backGlass);
            back.Add(buttonPanel);
            front = new Entity(Position)
            {
                Depth = -10500,
                Tag = Tag
            };
            front.Add(frontGlass);
            back.Tag = front.Tag = Tag;
            scene.Add(back, front);

            doorSprite.Play("idle");
            scene.Add(Barrier = new StoolPickupBarrier(Position, (int)Width, (int)frontGlass.Height, 1, false, true, false));
            Barrier.Tag = Tag;
            Barrier.Depth = 9001;
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            //SetStartingPosition(scene);
        }
        private void ResetPlatforms(float value)
        {
            MoveToY(value);
            Barriers[0].MoveToY(value + bOneOffset.Y);
            Barriers[1].MoveToY(value + bTwoOffset.Y);
            Barriers[2].MoveToY(value + bThreeOffset.Y);
        }
        private void MovePlatforms(float value)
        {
            MoveTowardsY(value, moveSpeed * Engine.DeltaTime);
            Barriers[0].MoveTowardsY(value + bOneOffset.Y, moveSpeed * Engine.DeltaTime);
            Barriers[1].MoveTowardsY(value + bTwoOffset.Y, moveSpeed * Engine.DeltaTime);
            Barriers[2].MoveTowardsY(value + bThreeOffset.Y, moveSpeed * Engine.DeltaTime);
        }
        public void ClickButton(bool upButton)
        {
            click.Position = -Vector2.UnitY * buttonPanel.Height + new Vector2(upButton ? 18 : 31, 40);
            click.UpdateSfxPosition();
            click.Play("event:/PianoBoy/Machines/ButtonPressC");
            PianoModule.Session.ButtonsPressed++;
            Clicks++;
            if (Clicks > 25 && !PianoModule.Session.RestoredPower)
            {
                PianoModule.SaveData.GiveAchievement("ThisTimeForSure");
            }
        }
        public IEnumerator MoveRoutine(int floor)
        {
            Player player = Scene.GetPlayer();
            if (!Moving)
            {
                StartMoveSound();
                Moving = true;
                Barrier.State = true;
                yield return null;
                while (Barrier.Opacity < 1)
                {
                    Barrier.Opacity += Engine.DeltaTime;
                    yield return null;
                }
                Barrier.Opacity = 1;
                yield return Engine.DeltaTime * 2;


                bool up = CurrentFloor < floor;
                int direction = up ? -1 : 1;
                int nextFloor = floor;
                bool atEdge = nextFloor < 0 || nextFloor >= Floors.Count;
                float altitude = GetFloor(nextFloor);
                float prevY = 0;
                if (!atEdge)
                {
                    while (Position.Y != altitude)
                    {
                        prevY = Position.Y;
                        MovePlatforms(altitude);
                        if (HasPlayerRider())
                        {
                            player.Hair.MoveHairBy(Vector2.UnitY * (Position.Y - prevY));
                        }
                        yield return null;
                    }
                    Position.Y = altitude;
                    ResetPlatforms(altitude);
                }
                else
                {
                    float yBump = Position.Y + direction * 4;
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
                    ResetPlatforms(origY);
                    Position.Y = origY;
                }
                StopMoveSound();
                if (!atEdge)
                {
                    CurrentFloor = nextFloor;
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
                Moving = false;
            }
        }
        public IEnumerator MoveElevator(bool up)
        {
            if (Scene is not Level level || level.GetPlayer() is not Player player) yield break;
            if (Event == Events.ButtonStuck)
            {
                //todo: add dialogue saying the buttons are stuck
                yield break;
            }
            else if (Event == Events.Broken)
            {
                //todo: add dialogue saying the elevator is beyond repair
                //todo: create damaged/broken elevator sprite
                yield break;
            }
            ClickButton(up);
            if (!PianoModule.Session.RestoredPower && reliesOnLabPower)
            {
                yield break;
            }
            yield return MoveRoutine(CurrentFloor + (up ? -1 : 1));
        }
    }
}
