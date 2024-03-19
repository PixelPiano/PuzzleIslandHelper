using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Effects;
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
        private float elevatorSpeed;
        private float elevatorPercent;
        private CustomTalkComponent GoUp;
        private CustomTalkComponent GoDown;

        public Image Front;
        public Image Back;
        public Sprite Gears;
        public Roof roof;
        public Sprite Rocks;
        public bool Moving;
        private int particleBuffer;
        public ParticleType GearSparks = new()
        {
            Size = 1,
            Color = Color.Orange,
            Color2 = Color.Yellow,
            LifeMin = 0.2f,
            LifeMax = 0.7f,
            SpeedMin = 10f,
            SpeedMax = 50f,
            DirectionRange = 30f.ToRad()
        };
        private float prevY;
        private ParticleSystem sparkSystem;
        public class Roof : Solid
        {
            public Roof(Vector2 position, float width, float height) : base(position, width, height, true)
            {
                AddTag(Tags.TransitionUpdate);
                Add(new LightOcclude());
            }
        }
        public override void Render()
        {
            int frame = ((int)Position.Y / 6 % 2) + 1;
            DrawGearFrame(frame);
            sparkSystem.Render();
            base.Render();
        }
        public void EmitSparks()
        {

            for (int i = 0; i < 4; i++)
            {
                float rotation = (i < 2 ? 270f : 90f).ToRad();
                Vector2 offset = new Vector2(18 + (i == 1 || i == 3 ? 0 : 31), i < 2 ? -5 : Back.Height + 5);
                float max = i < 2 ? -15 : -5;
                float min = i < 2 ? -20 : -10;
                GearSparks.Acceleration = new Vector2(Calc.Random.Range(-5, 5f), Calc.Random.Range(-30, 30));
                sparkSystem.Emit(GearSparks, 1, Back.RenderPosition + offset, Vector2.UnitX, rotation);
            }
        }

        private void DrawGearFrame(int frame)
        {
            MTexture tex = GFX.Game["objects/PuzzleIslandHelper/gear/elevatorGear0" + frame];
            Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, Back.RenderPosition - Vector2.UnitY * 4, Color.White);
            Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, Back.RenderPosition + Vector2.UnitY * (Back.Height - 3), Color.White);
        }
        public CrystalElevator(Vector2 position, float width) : base(position, width, 3, true)
        {
            Depth = 1;
            Front = new Image(GFX.Game["objects/PuzzleIslandHelper/gear/elevatorFront"]);
            Back = new Image(GFX.Game["objects/PuzzleIslandHelper/gear/elevator"]);
            Collider = new Hitbox(Front.Width, 3, 0, Front.Height - 3);
            Add(Back);

            Front.RenderPosition = Back.RenderPosition;
            GoUp = new CustomTalkComponent(21, 43, 8, 11, new Vector2(25, 43), InteractGoUp, CustomTalkComponent.SpecialType.UpArrow);
            GoDown = new CustomTalkComponent(37, 43, 8, 11, new Vector2(41, 43), InteractGoDown, CustomTalkComponent.SpecialType.DownArrow);

            GoUp.PlayerMustBeFacing = GoDown.PlayerMustBeFacing = false;
            Add(GoUp, GoDown);
            AddTag(Tags.TransitionUpdate);
            Add(new LightOcclude());
        }
        public void Interact(int floor, Player player, int floorToCheck)
        {
            Add(new Coroutine(StartRide(floor, player, floorToCheck)));
        }
        public void InteractGoUp(Player player)
        {
            Interact(Floor + 1, player, Floor);
        }
        public void InteractGoDown(Player player)
        {
            Interact(Floor - 1, player, Floor - 1);
        }
        public IEnumerator StartRide(int floor, Player player, int floorToCheck)
        {
            player.StateMachine.State = Player.StDummy;
            player.ForceCameraUpdate = true;
            if (!PianoModule.Session.FixedElevator)
            {
                yield return Textbox.Say("elevatorRocks");
            }
            else
            {
                StartShaking(0.2f);
                yield return 0.2f;
                if (!(floor < 0 || floor > Floors.Count || !AllFixedAt(floorToCheck)))
                {
                    int floorsTravelled = (int)Calc.Max(Floor, floor) - (int)Calc.Min(Floor, floor);
                    float travelTime = 5;

                    yield return RideToFloor(floor, floorsTravelled * travelTime);
                }
            }
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
            Moving = true;
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
            Floor = floor;
            PianoModule.Session.FurthestElevatorLevel = (int)Calc.Max(PianoModule.Session.FurthestElevatorLevel, Floor);
            Moving = false;
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
            MoveToY((float)Math.Floor(start.Y + (end.Y - start.Y) * elevatorPercent) - 28);
            if (HasPlayerRider())
            {
                Player player = level.GetPlayer();
                player.Hair.MoveHairBy(Vector2.UnitY * elevatorSpeed);
            }
            if (roof is not null && Back is not null) roof.MoveTo(Back.RenderPosition + Vector2.UnitY * 3);
            if (affectCamera)
            {
                level.Camera.Y = Calc.LerpClamp(level.Camera.Y, Y - 60f, percent);
            }
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            sparkSystem = new ParticleSystem(Depth - 1, 200);
            scene.Add(sparkSystem);
            sparkSystem.Visible = false;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Player player = scene.GetPlayer();
            if (player is null) return;
            Rocks = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/gear/");
            Rocks.AddLoop("idle", "rocks", 0.1f);
            Rocks.AddLoop("shake", "rockShake", 0.1f);
            Rocks.AddLoop("cleared", "rockCrumble", 0.1f, 7);
            Rocks.Add("crumble", "rockCrumble", 0.1f, "cleared");
            Add(Rocks);
            Rocks.Play("idle");
            if (PianoModule.Session.FixedElevator)
            {
                Rocks.Visible = false;
            }
            Rocks.Position = new Vector2(Width / 2 - Rocks.Width / 2, Back.Height / 2 + 4);
            int num = 0;
            float d = int.MaxValue;
            CrystalElevatorLevel closest = null;
            foreach (CrystalElevatorLevel level in (scene as Level).Tracker.GetEntities<CrystalElevatorLevel>().OrderByDescending(item => item.Y))
            {
                float dist = Vector2.DistanceSquared(level.Position, player.Position);
                if (dist < d)
                {
                    d = dist;
                    closest = level;
                }
                num++;
                Floors.Add(level);
            }
            Floor = closest != null ? (int)Calc.Min(PianoModule.Session.FurthestElevatorLevel, closest.FloorNum) : PianoModule.Session.FurthestElevatorLevel;
            MoveElevatorTowards(Floor, 1, false);
            scene.Add(roof = new Roof(Back.RenderPosition + Vector2.UnitY * 3, Back.Width, 3));
            roof.Add(Front);
            roof.Depth = -1;
        }
        public void ClearRocks()
        {
            Add(new Coroutine(RockRoutine()));
        }
        private IEnumerator RockRoutine()
        {
            if (Scene is not Level level || level.GetPlayer() is not Player player || PianoModule.Session.FixedElevator) yield break;
            InvertOverlay.HoldState = true;
            yield return null;
            player.StateMachine.State = Player.StDummy;
            
            PianoModule.Session.FixedElevator = true;
            
            Rocks.Play("shake");
            yield return 0.7f;
            Rocks.Play("crumble");
            while(Rocks.CurrentAnimationID != "cleared")
            {
                yield return null;
            }
            InvertOverlay.HoldState = false;
            player.StateMachine.State = Player.StNormal;

        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            scene.Remove(roof);
            scene.Remove(sparkSystem);
        }
        public CrystalElevator(EntityData data, Vector2 offset) : this(data.Position + offset, data.Width)
        {
        }
        public Vector2 GetFloorAt(int index)
        {
            if (index < 0) return Vector2.Zero;
            return Floors[index].Position;
        }
        public bool AllFixedAt(int floor)
        {
            CrystalElevatorLevel level = Floors.Find(item => item.FloorNum == floor);
            return level is not null && level.Fixed();
        }

        public override void Update()
        {
            Front.RenderPosition = Back.RenderPosition;

            base.Update();
            if (Moving)
            {
                if (particleBuffer >= 4 && (int)(prevY - Position.Y) != 0)
                {
                    particleBuffer = 0;
                    EmitSparks();
                }
                else
                {
                    particleBuffer++;
                }

            }
            if (InvertOverlay.State && !PianoModule.Session.FixedElevator)
            {
                ClearRocks();
            }
            prevY = Position.Y;
        }
    }
}
