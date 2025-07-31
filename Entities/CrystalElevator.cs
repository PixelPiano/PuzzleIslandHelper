using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Effects;
using Celeste.Mod.PuzzleIslandHelper.Entities.GearEntities;
using FrostHelper;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/CrystalElevator")]
    [Tracked]
    public class CrystalElevator : Solid
    {

        public static Dictionary<string, HashSet<int>> DestroyedCustomSpinnerIDs => PianoModule.Session.DestroyedCustomSpinnerIDs;
        public class Roof : Solid
        {
            public CrystalElevator Parent;
            public Collider SpinnerCollider;
            public Roof(CrystalElevator parent, Image front, Vector2 position, float width, float height) : base(position, width, height, true)
            {
                Depth = -1;
                Parent = parent;
                AddTag(Tags.TransitionUpdate);
                Add(new LightOcclude());
                Add(front);
                SpinnerCollider = new Hitbox(Width, Parent.Bottom - Top, 0, 0);
            }
            public override void DebugRender(Camera camera)
            {
                base.DebugRender(camera);
                Collider prev = Collider;
                Collider = SpinnerCollider;
                Draw.HollowRect(Collider, Color.LightBlue);
                Collider = prev;
            }
            public bool ForceCheckSpinner(Entity spinner)
            {
                bool output = false;
                Collider prev = Collider;
                Collider = SpinnerCollider;
                if (spinner != this && Collidable)
                {
                    output = spinner.Collider.Collide(this);
                }
                Collider = prev;
                return output;
            }
            public void CheckForSpinners(Level level, bool instant = false)
            {
                Collider prev = Collider;
                Collider = SpinnerCollider;
                HashSet<int> list = [];
                foreach (CustomSpinner spinner in level.Tracker.GetEntities<CustomSpinner>())
                {
                    if (ForceCheckSpinner(spinner))
                    {
                        if (instant)
                        {
                            spinner.RemoveSelf();
                        }
                        else
                        {
                            spinner.Destroy();
                        }
                        list.Add(spinner.ID);
                    }
                }
                if (DestroyedCustomSpinnerIDs.TryGetValue(level.Session.Level, out var list2))
                {
                    foreach (int i in list)
                    {
                        list2.Add(i);
                    }
                }
                else
                {
                    DestroyedCustomSpinnerIDs.Add(level.Session.Level, list);
                }
                foreach (CrystalStaticSpinner spinner2 in level.Tracker.GetEntities<CrystalStaticSpinner>())
                {
                    if (ForceCheckSpinner(spinner2))
                    {
                        if (!instant)
                        {
                            spinner2.Destroy();
                        }
                        else
                        {
                            spinner2.RemoveSelf();
                        }
                    }
                }
                Collider = prev;
            }
        }
        public class CrystalElevatorLevel : Component
        {
            public int FloorNum;
            public List<GearHolder> Holders = new();
            public bool Free => Holders is null || Holders.Count <= 0;
            public Vector2 Position;
            public CrystalElevatorLevel(Vector2 position, int floor) : base(true, true)
            {
                Position = position;
                FloorNum = floor;
            }

            public override void EntityAwake()
            {
                base.EntityAwake();
                foreach (GearHolder holder in Entity.Scene.Tracker.GetEntities<GearHolder>())
                {
                    if (holder.ID == FloorNum)
                    {
                        Holders.Add(holder);
                    }
                }
            }
            public bool Fixed()
            {
                if (Free) return true;
                foreach (GearHolder holder in Holders)
                {
                    if (!holder.Fixed)
                    {
                        return false;
                    }
                }
                return true;
            }
            public override void Removed(Entity entity)
            {
                base.Removed(entity);
                entity.Scene.Remove(Holders);
            }
        }
        public static ParticleType GearSparks = new()
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
        public int Floor;
        public int Count => Floors is null ? 0 : Floors.Count;
        private int particleBuffer;
        public float Percent;
        private float elevatorSpeed;
        private float elevatorPercent;
        private float prevY;
        public bool Moving;
        public Vector2[] FloorPositions;
        public List<CrystalElevatorLevel> Floors = [];
        private readonly CustomTalkComponent goUp;
        private readonly CustomTalkComponent goDown;
        public static Dictionary<EntityID, int> Furthest => PianoModule.Session.CrystalElevatorFurthestLevelReached;
        public Image Front, Back, Rocks;
        public Sprite Gears;
        public Roof roof;
        private ParticleSystem sparkSystem;
        public EntityID ID;
        public Vector2 OrigPosition;
        public string Flag;
        public bool ControlsBlocked => string.IsNullOrEmpty(Flag) ? false : Scene is Level level && !level.Session.GetFlag(Flag);
        public CrystalElevator(EntityData data, Vector2 offset, EntityID id) : this(data.Position + offset, data.Width, data.NodesWithPosition(offset), id, data.Attr("flag"))
        {
        }
        public CrystalElevator(Vector2 position, float width, Vector2[] floorPositions, EntityID id, string flag) : base(position, width, 3, true)
        {
            Depth = 1;
            Flag = flag;
            OrigPosition = position;
            ID = id;
            FloorPositions = floorPositions;
            Front = new Image(GFX.Game["objects/PuzzleIslandHelper/gear/elevatorFront"]);
            Back = new Image(GFX.Game["objects/PuzzleIslandHelper/gear/elevator"]);
            Collider = new Hitbox(Front.Width, 3, 0, Front.Height - 3);
            goUp = new CustomTalkComponent(21, 43, 8, 11, new Vector2(25, 43), InteractGoUp, CustomTalkComponent.SpecialType.UpArrow);
            goDown = new CustomTalkComponent(37, 43, 8, 11, new Vector2(41, 43), InteractGoDown, CustomTalkComponent.SpecialType.DownArrow);

            goUp.PlayerMustBeFacing = goDown.PlayerMustBeFacing = false;
            Add(Back, goUp, goDown, new LightOcclude());
            Tag |= Tags.TransitionUpdate;
            roof = new Roof(this, Front, Back.RenderPosition + Vector2.UnitY * 3, Back.Width, 3);
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            sparkSystem = new ParticleSystem(Depth - 1, 200);
            scene.Add(sparkSystem);
            sparkSystem.Visible = false;

            for (int i = 0; i < FloorPositions.Length; i++)
            {
                CrystalElevatorLevel level = new(FloorPositions[i], i);
                Add(level);
                Floors.Add(level);
            }
            scene.Add(roof);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (scene is not Level level || level.GetPlayer() is not Player player) return;
            if (DestroyedCustomSpinnerIDs.TryGetValue(level.Session.Level, out var list))
            {
                foreach (CustomSpinner spinner in level.Tracker.GetEntities<CustomSpinner>())
                {
                    if (list.Contains(spinner.ID))
                    {
                        spinner.RemoveSelf();
                    }
                }
            }
            Front.RenderPosition = Back.RenderPosition;
            int furthest = 0;
            if (Furthest.TryGetValue(ID, out int value))
            {
                furthest = value;
            }
            else
            {
                Furthest.Add(ID, 0);
            }
            Add(Rocks = new Image(GFX.Game["objects/PuzzleIslandHelper/gear/rocks"]));
            Rocks.Position = new Vector2(Width / 2 - Rocks.Width / 2, Back.Height / 2 + 4);
            Rocks.Visible = !ControlsBlocked;
            CrystalElevatorLevel closest = Floors.OrderBy(item => Vector2.DistanceSquared(item.Position, player.Position)).First();
            Floor = closest != null ? (int)Calc.Min(furthest, closest.FloorNum) : furthest;
            MoveElevatorTowards(level, null, Floor, 1, false, true);
        }
        public override void Update()
        {
            Front.RenderPosition = Back.RenderPosition;
            base.Update();
            if (Furthest.TryGetValue(ID, out int value))
            {
                Furthest[ID] = (int)Calc.Max(value, Floor);
            }
            else
            {
                Furthest.Add(ID, Floor);
            }
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
            if (Rocks != null)
            {
                Rocks.Visible = !ControlsBlocked;
            }
            prevY = Position.Y;
        }
        public override void Render()
        {
            int frame = (int)Position.Y / 6 % 2 + 1;
            DrawGearFrame(frame);
            sparkSystem.Render();
            base.Render();
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            roof.RemoveSelf();
            sparkSystem.RemoveSelf();
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
        public void MoveElevatorTowards(Level level, Player player, int floor, float percent, bool affectCamera = true, bool instant = false)
        {
            Vector2 start = GetFloorAt(Floor);
            Vector2 end = GetFloorAt(floor);
            float length = (start - end).Length();
            elevatorSpeed = Calc.Approach(elevatorSpeed, 64f, 120f * Engine.DeltaTime);
            elevatorPercent = Calc.Approach(elevatorPercent, percent, elevatorSpeed / length * Engine.DeltaTime);
            float newY = (float)Math.Floor(start.Y + (end.Y - start.Y) * elevatorPercent);
            MoveElevatorToY(newY, true, instant);
            if (affectCamera) level.Camera.Y = Calc.LerpClamp(level.Camera.Y, Y - 60f, percent);
        }
        public void MoveToFloor(int floor, bool collideSpinners = true, bool instant = false)
        {
            float y = GetFloorAt(floor).Y;
            MoveElevatorToY(y, collideSpinners, instant);
            Floor = floor;

        }
        public void MoveElevatorToY(float y, bool collideSpinners = true, bool instant = false)
        {
            Level level = Scene as Level;
            Player player = level.GetPlayer();
            while (Y != y)
            {
                float prev = Y;
                MoveTowardsY(y, 1);
                if (player != null && player.Hair != null && HasPlayerRider())
                {
                    player.Hair.MoveHairBy(Vector2.UnitY * (Y - prev));
                }
                roof.MoveTo(Back.RenderPosition + Vector2.UnitY * 3);
                if (collideSpinners)
                {
                    roof.CheckForSpinners(level, instant);
                }
            }
        }
        [Command("crystal_elevator", "")]
        public static void a(int floor, bool spinners)
        {
            foreach (CrystalElevator e in Engine.Scene.Tracker.GetEntities<CrystalElevator>())
            {
                e.MoveToFloor(floor, spinners, true);
            }
        }

        public void StartRide(Level level, int floor, Player player, int floorToCheck)
        {
            Add(new Coroutine(RideRoutine(level, floor, player, floorToCheck)));
        }
        public void InteractGoUp(Player player)
        {
            StartRide(SceneAs<Level>(), Floor + 1, player, Floor);
        }
        public void InteractGoDown(Player player)
        {
            StartRide(SceneAs<Level>(), Floor - 1, player, Floor - 1);
        }
        public IEnumerator RideRoutine(Level level, int floor, Player player, int floorToCheck)
        {
            player.StateMachine.State = Player.StDummy;
            player.ForceCameraUpdate = true;
            if (!ControlsBlocked)
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

                    yield return RideToFloor(level, player, floor, floorsTravelled * travelTime);
                }
            }
            player.StateMachine.State = Player.StNormal;
            yield return null;
        }
        public IEnumerator RideToFloor(Level level, Player player, int floor, float duration)
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
                MoveElevatorTowards(level, player, floor, lerp);
                elevatorPercent = lerp;
                yield return null;
            }
            Floor = floor;
            Furthest[ID] = (int)Calc.Max(Furthest[ID], Floor);
            Moving = false;
            StartShaking(shakeDuration);
            yield return shakeDuration + 0.2f;
        }
        public void EmitSparks()
        {
            for (int i = 0; i < 4; i++)
            {
                float rotation = (i < 2 ? 270f : 90f).ToRad();
                Vector2 offset = new(18 + (i == 1 || i == 3 ? 0 : 31), i < 2 ? -5 : Back.Height + 5);
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

        public Vector2 GetFloorAt(int index)
        {
            return index < 0 || index >= Floors.Count ? OrigPosition : Floors[index].Position;
        }
        public bool AllFixedAt(int floor)
        {
            return floor >= 0 && floor < Floors.Count && Floors[floor].Fixed();
        }

    }
}
