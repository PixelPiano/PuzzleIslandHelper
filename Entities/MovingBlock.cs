using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/MovingBlock")]
    [Tracked]
    public class MovingBlock : Solid
    {
        public struct MovingBlockData
        {
            public int MoveLimit;
            public int MovesUsed;
            public bool AtNode;
        }
        public static readonly Dictionary<EntityID, MovingBlockData> PermanentBlockData = [];
        [OnLoad]
        public static void Load()
        {
            PermanentBlockData.Clear();
            Everest.Events.Player.OnDie += Player_OnDie;
            Everest.Events.Player.OnSpawn += Player_OnSpawn;
        }
        [OnUnload]
        public static void Unload()
        {
            PermanentBlockData.Clear();
            Everest.Events.Player.OnDie -= Player_OnDie;
            Everest.Events.Player.OnSpawn -= Player_OnSpawn;
        }
        private static void Player_OnSpawn(Player obj)
        {
/*            foreach (MovingBlock block in obj.Scene.Tracker.GetEntities<MovingBlock>())
            {
                if (block.mode is activationModes.PlayerRespawn)
                {
                    block.OverrideConditionForOneFrame();
                }
            }*/
        }
        private static void Player_OnDie(Player obj)
        {
/*            foreach (MovingBlock block in obj.Scene.Tracker.GetEntities<MovingBlock>())
            {
                if (block.mode is activationModes.PlayerDie)
                {
                    block.OverrideConditionForOneFrame();
                }
            }*/
        }

        private EntityID id;
        private char tiletype;
        private TileGrid tileGrid;
        private Vector2 node;
        private Vector2 start;
        public MovingBlockData Data;
        private enum activationModes
        {
            Never,
            Always,
            PlayerRiding,
            PlayerClimbing,
            PlayerClimbingOrRiding,
            ActorRiding,
            Flag,
            DashCollide,
            PlayerDie,
            PlayerRespawn,
            Awake,
            Removed
        }
        private activationModes mode;
        public FlagData Flag;
        public bool Shakes;
        public bool Permanent;
        private bool collidable;
        private Vector2 next;
        private int moveLimit;
        private int moves;
        private bool conditionOverride;
        private float overrideTimer;
        private Vector2? emergencySnap;
        public bool HasMovesLeft => moveLimit < 0 || moves < moveLimit;
        public void OverrideConditionForOneFrame()
        {
            conditionOverride = true;
            overrideTimer = Engine.DeltaTime;
        }
        public MovingBlock(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset, data.Width, data.Height, true)
        {
            mode = data.Enum<activationModes>("activationType");
            Shakes = data.Bool("shakes");
            Flag = data.Flag("flag", "inverted");
            tiletype = data.Char("tiletype", '3');
            Depth = -12999;
            node = data.NodesOffset(offset)[0];
            start = Position;
            next = node;
            this.id = id;
            moveLimit = data.Int("moveLimit", -1);
            collidable = data.Bool("collidable", true);
            Tag |= Tags.TransitionUpdate;
            Add(new Coroutine(Sequence()));
            OnDashCollide = DashCollision;
        }
        public DashCollisionResults DashCollision(Player player, Vector2 direction)
        {
            if (mode is activationModes.DashCollide)
            {
                OverrideConditionForOneFrame();
            }
            return DashCollisionResults.NormalCollision;
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Add(new LightOcclude());
            Add(tileGrid = GFX.FGAutotiler.GenerateBox(tiletype, (int)Width / 8, (int)Height / 8).TileGrid);
            Add(new TileInterceptor(tileGrid, highPriority: true));
            Collidable = collidable;

            if (Permanent)
            {
                if (PermanentBlockData.TryGetValue(id, out var blockData))
                {
                    Data = blockData;
                    moveLimit = Data.MoveLimit;
                    moves = Data.MovesUsed;
                    if (Data.AtNode)
                    {
                        next = start;
                        MoveTo(node);
                    }
                }
                else
                {
                    Data.AtNode = false;
                    Data.MoveLimit = moveLimit;
                    Data.MovesUsed = moves;
                    PermanentBlockData.Add(id, Data);
                }
            }

            if (mode is activationModes.Awake)
            {
                OverrideConditionForOneFrame();
            }
            if (CollideCheck<Player>())
            {
                RemoveSelf();
            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            if (Permanent)
            {
                if (emergencySnap.HasValue)
                {
                    Data.AtNode = emergencySnap.Value == node;
                    Data.MovesUsed++;
                }
                if (mode is activationModes.Removed && HasMovesLeft)
                {
                    Data.AtNode = !Data.AtNode;
                    Data.MovesUsed++;
                }
                PermanentBlockData[id] = Data;
            }
        }
        public override void Update()
        {
            base.Update();
            if (overrideTimer > 0)
            {
                overrideTimer -= Engine.DeltaTime;
            }
            else
            {
                conditionOverride = false;
            }
        }
        public IEnumerator Sequence()
        {
            while (true)
            {
                while (!CheckCondition())
                {
                    yield return null;
                }
                if (Shakes)
                {
                    ShakeSfx();
                    StartShaking();
                    Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                    yield return 0.2f;
                    float timer = 0.4f;
                    while (timer > 0f)
                    {
                        yield return null;
                        timer -= Engine.DeltaTime;
                    }

                    StopShaking();
                    for (int i = 2; (float)i < Width; i += 4)
                    {
                        if (Scene.CollideCheck<Solid>(TopLeft + new Vector2(i, -2f)))
                        {
                            SceneAs<Level>().Particles.Emit(FallingBlock.P_FallDustA, 2, new Vector2(X + (float)i, Y), Vector2.One * 4f, MathF.PI / 2f);
                        }

                        SceneAs<Level>().Particles.Emit(FallingBlock.P_FallDustB, 2, new Vector2(X + (float)i, Y), Vector2.One * 4f);
                    }
                }
                float speed = 0f;
                float maxSpeed = 160f;
                Level level = SceneAs<Level>();
                moves++;
                if (Permanent)
                {
                    Data.MovesUsed++;
                }
                emergencySnap = next;
                while (Position != next)
                {
                    speed = Calc.Approach(speed, maxSpeed, 500f * Engine.DeltaTime);
                    Vector2 newPosition = Calc.Approach(Position, next, speed * Engine.DeltaTime);
                    MoveTo(newPosition);
                    yield return null;
                }
                emergencySnap = null;
                next = next == node ? start : node;
                if (Permanent)
                {
                    Data.AtNode = next == start;
                }
                yield return null;
                if (Shakes)
                {
                    Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                    StartShaking();
                    yield return 0.2f;
                    StopShaking();
                }
            }
        }
        public void ShakeSfx()
        {
            if (tiletype == '3')
            {
                Audio.Play("event:/game/01_forsaken_city/fallblock_ice_shake", base.Center);
            }
            else if (tiletype == '9')
            {
                Audio.Play("event:/game/03_resort/fallblock_wood_shake", base.Center);
            }
            else if (tiletype == 'g')
            {
                Audio.Play("event:/game/06_reflection/fallblock_boss_shake", base.Center);
            }
            else
            {
                Audio.Play("event:/game/general/fallblock_shake", base.Center);
            }
        }
        public void ImpactSfx()
        {
            if (tiletype == '3')
            {
                Audio.Play("event:/game/01_forsaken_city/fallblock_ice_impact", base.BottomCenter);
            }
            else if (tiletype == '9')
            {
                Audio.Play("event:/game/03_resort/fallblock_wood_impact", base.BottomCenter);
            }
            else if (tiletype == 'g')
            {
                Audio.Play("event:/game/06_reflection/fallblock_boss_impact", base.BottomCenter);
            }
            else
            {
                Audio.Play("event:/game/general/fallblock_impact", base.BottomCenter);
            }
        }
        public bool CheckCondition()
        {
            return mode switch
            {

                activationModes.PlayerRiding => HasPlayerOnTop(),
                activationModes.PlayerClimbing => HasPlayerClimbing(),
                activationModes.PlayerClimbingOrRiding => HasPlayerRider(),
                activationModes.Always => true,
                activationModes.ActorRiding => HasRider(),
                activationModes.Flag => Flag.State,
                activationModes.Never => false,
                _ => conditionOverride
            } && HasMovesLeft;
        }
    }
}