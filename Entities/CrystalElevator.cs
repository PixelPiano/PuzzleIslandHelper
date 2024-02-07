using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.Cutscenes;
using Celeste.Mod.PuzzleIslandHelper.Triggers;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/CrystalElevator")]
    [Tracked]
    public class CrystalElevator : Solid
    {
        public List<CrystalElevatorLevel> Floors = new();
        public int Floor;
        public float Percent;
        public int Count
        {
            get
            {
                if (Floors is null) return 0;
                return Floors.Count;
            }
        }
        public float YOffset;
        private float elevatorSpeed;
        private float elevatorPercent;
        private CustomTalkComponent GoUp;
        private CustomTalkComponent GoDown;

        public Image Front;
        public Image Back;
        public Sprite Gears;
        public Roof roof;
        public class Roof : Solid
        {
            public Roof(Vector2 position, float width, float height) : base(position, width, height, true)
            {
                AddTag(Tags.TransitionUpdate);
            }
        }
        public override void Render()
        {

            int frame = ((int)Position.Y / 6 % 2) + 1;
            DrawGearFrame(frame);
            base.Render();
        }
        private void DrawGearFrame(int frame)
        {
            MTexture tex = GFX.Game["objects/PuzzleIslandHelper/cog/elevatorGear0" + frame];
            Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, Back.RenderPosition - Vector2.UnitY * 4, Color.White);
            Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, Back.RenderPosition + Vector2.UnitY * (Back.Height - 3), Color.White);
        }
        public CrystalElevator(Vector2 position, float width) : base(position, width, 3, true)
        {
            Depth = 1;
            Front = new Image(GFX.Game["objects/PuzzleIslandHelper/cog/elevatorFront"]);
            Back = new Image(GFX.Game["objects/PuzzleIslandHelper/cog/elevator"]);
            Collider = new Hitbox(Front.Width, 3, 0, Front.Height - 3);
            YOffset = Front.Height;
            Add(Back);

            Front.RenderPosition = Back.RenderPosition;
            GoUp = new CustomTalkComponent(21, 43, 8, 11, new Vector2(25, 43), InteractGoUp, CustomTalkComponent.SpecialType.UpArrow);
            GoDown = new CustomTalkComponent(37, 43, 8, 11, new Vector2(41, 43), InteractGoDown, CustomTalkComponent.SpecialType.DownArrow);

            GoUp.PlayerMustBeFacing = GoDown.PlayerMustBeFacing = false;
            Add(GoUp, GoDown);
            AddTag(Tags.TransitionUpdate);
        }
        public void Interact(int floor, Player player)
        {
            if (floor < 0 || floor > Floors.Count) return;
            Add(new Coroutine(StartRide(floor, player)));
        }
        public void InteractGoUp(Player player)
        {

            Interact(Calc.Clamp(Floor + 1, 0, Floors.Count - 1), player);
        }
        public void InteractGoDown(Player player)
        {
            Interact(Calc.Clamp(Floor - 1, 0, Floors.Count - 1), player);
        }
        public IEnumerator StartRide(int floor, Player player)
        {
            if (Scene is not Level level) yield break;
            if (!PianoModule.SaveData.FixedFloors.Contains(floor))
            {
                if (!AllFixedAt(floor))
                {
                    yield break;
                }
                PianoModule.SaveData.FixedFloors.Add(floor);
            }

            player.StateMachine.State = Player.StDummy;
            player.ForceCameraUpdate = true;

            int floorsTravelled = (int)Calc.Max(Floor, floor) - (int)Calc.Min(Floor, floor);
            float travelTime = 5;
            yield return RideToFloor(floor, floorsTravelled * travelTime);
            player.StateMachine.State = Player.StNormal;


            yield return null;
        }
        public override void OnShake(Vector2 amount)
        {
            base.OnShake(amount);
            bool hasPlayer = HasPlayerOnTop();
            roof.Position += amount;
            Position += amount;
            if (hasPlayer)
            {
                Player player = SceneAs<Level>().GetPlayer();
                if (player != null)
                {
                    player.Position += amount;
                }
            }
        }
        public IEnumerator RideToFloor(int floor, float duration)
        {
            elevatorPercent = 0;
            elevatorSpeed = 0;
            float shakeDuration = 0.2f;
            StartShaking(shakeDuration);
            yield return shakeDuration + 0.2f;
            for (float i = 0; i < 1; i += Engine.DeltaTime / duration)
            {
                float lerp = Ease.SineInOut(i);
                MoveElevatorTowards(floor, lerp);
                elevatorPercent = lerp;
                yield return null;
            }
            PianoModule.SaveData.LastElevatorLevel = Floor = floor;
            StartShaking(shakeDuration);
            yield return shakeDuration + 0.2f;
        }
        public void MoveElevatorTowards(int floor, float percent, bool affectCamera = true)
        {
            if (Scene is not Level level) return;
            Vector2 start = GetFloorAt(Floor);
            Vector2 end = GetFloorAt(floor);
            float num = (start - end).Length();
            elevatorSpeed = Calc.Approach(elevatorSpeed, 64f, 120f * Engine.DeltaTime);
            elevatorPercent = Calc.Approach(elevatorPercent, percent, elevatorSpeed / num * Engine.DeltaTime);
            MoveToY((float)Math.Floor(start.Y + (end.Y - start.Y) * elevatorPercent) - Back.Height);
            if (roof is not null && Back is not null) roof.MoveTo(Back.RenderPosition + Vector2.UnitY * 3);
            if (affectCamera)
            {
                level.Camera.Y = Calc.LerpClamp(level.Camera.Y, Y - 60f, percent);
            }
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            List<CrystalElevatorLevel> floors = new();
            foreach (CrystalElevatorLevel level in (scene as Level).Tracker.GetEntities<CrystalElevatorLevel>())
            {
                floors.Add(level);
            }
            floors = floors.OrderByDescending(item => item.Y).ToList();
            int num = 0;
            foreach (CrystalElevatorLevel level in floors)
            {
                level.FloorNum = num;
                num++;
                Floors.Add(level);
            }
            Floor = PianoModule.SaveData.LastElevatorLevel;
            MoveElevatorTowards(Floor, 1, false);
            scene.Add(roof = new Roof(Back.RenderPosition + Vector2.UnitY * 3, Back.Width, 3));
            roof.Add(Front);
            roof.Depth = -1;
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            scene.Remove(roof);
            PianoModule.SaveData.LastElevatorLevel = Floor;
        }
        public CrystalElevator(EntityData data, Vector2 offset) : this(data.Position + offset, data.Width)
        {
        }
        public Vector2 GetFloorAt(int index)
        {
            if (index < 0 || index >= Floors.Count) return Vector2.Zero;
            return Floors[index].Position;
        }
        public bool AllFixedAt(int floor)
        {
            if (Scene is not Level level) return false;
            bool floorOccupied = false;
            bool atLeastOneBroken = false;
            foreach (CogHolder holder in level.Tracker.GetEntities<CogHolder>())
            {
                if (holder.ID == floor)
                {
                    floorOccupied = true;
                    if (!holder.UsedOnce)
                    {
                        atLeastOneBroken = true;
                        break;
                    }
                }
            }
            return !atLeastOneBroken;
        }

        public override void Update()
        {
            Front.RenderPosition = Back.RenderPosition;
            base.Update();
        }
    }
}
