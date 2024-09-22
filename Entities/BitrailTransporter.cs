using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.CustomCalidusEntities;
using Celeste.Mod.PuzzleIslandHelper.Helpers;
using static Celeste.Mod.PuzzleIslandHelper.Components.BitrailNode;
using static Celeste.Mod.PuzzleIslandHelper.Entities.InvertAuth;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked]
    public class BitrailTransporter : Entity
    {
        //public static bool MoveThroughTransitions => PianoModule.Session.DEBUGBOOL;
        private static bool PreventFamilyRegrabbing = false;
        public const float Speed = 140f;
        public static bool Transporting;
        public string CurrentLevelName => SceneAs<Level>().Session.Level;
        public Grid Grid => BitrailHelper.Grid;
        public Vector2 RailPosition;
        public Vector2 Dir;
        public HashSet<BitrailNode> Nodes = new();
        public Dictionary<string, HashSet<BitrailNode>> ExitNodesByLevel = new();
        public static Dictionary<string, HashSet<BitrailNode>> NodesByLevel = new();
        public Dictionary<BitrailNode, Collider> ExitColliders = new();
        public BitrailNode CurrentNode;
        public BitrailNode IgnoreNode;
        public BitrailNode LastExitNode;
        public BitrailNode FirstExitNode;
        public BitrailNode PreviousNode;
        public Player Player;
        private bool wasTransporting;
        public bool WaitingForEject;
        private Alarm exitAlarm;
        private Alarm dashAlarm;
        private Vector2? DashedFrom;
        private float nodeTimeLimit;
        private float nodeTimer;
        public float WarningAmount;
        public ParticleSystem Particles;
        private float dist;
        private float distLimit = 7;
        private PlayerCalidus pC;
        private Entity solidChecker;
        public Rectangle MaskBounds;
        public enum AutoDirModes
        {
            Off,
            TurnLeft,
            TurnRight,
            Clock
        }
        public AutoDirModes DirMode = AutoDirModes.Clock;
        public ParticleType Particle = new()
        {
            Size = 2,
            Source = GFX.Game["objects/PuzzleIslandHelper/particles/line01"],
            Color = Color.LimeGreen * 0.5f,
            Color2 = Color.Cyan * 0.8f,
            ColorMode = ParticleType.ColorModes.Fade,
            FadeMode = ParticleType.FadeModes.Linear,
            ScaleOut = true,
            SpinFlippedChance = true,
            Friction = 1,
            LifeMin = 0.5f,
            LifeMax = 1.5f,
            SpeedMin = 5,
            SpeedMax = 10,
            SpinMin = 0.5f,
            SpinMax = 20
        };
        public BitrailTransporter() : base()
        {
            Depth = 1;
            //Depth = -1000000;
            Tag |= Tags.Global | Tags.TransitionUpdate;
            Particles = new ParticleSystem(Depth - 1, 200);
            Particles.Tag = Tag;
        }


        private float lastExitNodeReenterTimer;
        public override void Added(Scene scene)
        {
            base.Added(scene);
            BitrailHelper.InitializeGrids();
            Position = BitrailHelper.MapPosition;
            Nodes = BitrailHelper.CreateNodes();
            foreach (BitrailNode node in Nodes)
            {
                Add(node);
            }
            Add(exitAlarm = Alarm.Create(Alarm.AlarmMode.Persist, delegate { IgnoreNode = null; }, 0.4f, false));
            Add(dashAlarm = Alarm.Create(Alarm.AlarmMode.Persist, delegate { DashedFrom = null; }, 0.5f, false));
            solidChecker = new();
            solidChecker.Collider = new Hitbox(8, 8);
            Scene.Add(solidChecker);

        }
        private List<Vector2> checkedPositions = new();
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Level level = scene as Level;
            scene.Add(Particles);
            Player = scene.GetPlayer();
            pC = Player as PlayerCalidus;
            BitrailHelper.PrepareNodes(Nodes);
            BitrailHelper.AssignFamilies(Nodes);
            ExitColliders = BitrailHelper.CreateExitColliders(Nodes);
            NodesByLevel = BitrailHelper.NodesByLevel(Nodes, level);
            ExitNodesByLevel = BitrailHelper.NodesByLevel(Nodes, level,
                item => item.Node is BitrailNode.Nodes.DeadEnd or BitrailNode.Nodes.Single);
            FGRenderer = new BitrailFGRenderer(this);
            Scene.Add(FGRenderer);
            foreach (BitrailNode node in Nodes)
            {
                solidChecker.Position = node.RenderPosition;
                if (solidChecker.CollideCheck<Solid>())
                {
                    node.SwitchToInSolid();
                }
            }
            solidChecker.RemoveSelf();
        }
        private float dashTimer = 0;
        public static bool IgnoreLevel;

        public Vector2 PrevSign;
        public Vector2 CurrentSign;
        public Vector2 LastSign;
        private Vector2 lastAim;
        private Vector2 lastDashAim;
        private Vector2 lastJumpAim;
        private int dashBuffer;
        private int jumpBuffer;
        public override void Update()
        {
            IgnoreLevel = !NodesByLevel.ContainsKey(CurrentLevelName);
            if (IgnoreLevel)
            {
                Transporting = false;
                DashedFrom = null;
                return;
            }
            base.Update();
            dashBuffer = (int)Calc.Max(dashBuffer - 1, 0);
            jumpBuffer = (int)Calc.Max(jumpBuffer - 1, 0);
            PrevSign = (RailPosition - PrevRailPosition).Sign();
            PrevRailPosition = RailPosition;
            if (Scene is not Level level || level.GetPlayer() is not Player player || player.Dead)
            {
                Transporting = false;
                DashedFrom = null;
                return;
            }
            Player = player;
            if (Player is PlayerCalidus)
            {
                pC = Player as PlayerCalidus;
            }
            if (Input.Jump.Pressed)
            {
                jumpBuffer = 4;
            }
            if (Input.Dash.Pressed)
            {
                dashBuffer = 4;
            }
            bool playerCanEnterRail = pC != null ? pC.Blipping || pC.AvailableForRail : player.DashAttacking;
            dashTimer = Calc.Max(0, dashTimer - Engine.DeltaTime);
            lastExitNodeReenterTimer = Calc.Max(0, dashTimer - Engine.DeltaTime);
            if (WaitingForEject && nodeTimeLimit > 0)
            {
                nodeTimer -= Engine.DeltaTime;
                if (nodeTimer > 0) CurrentNode.WarningAmount = 1 - nodeTimeLimit / nodeTimer;
                else OutOfTimeEject(player);
            }
            UpdateColliderPositions();
            if (Transporting)
            {

                Vector2 aim = new Vector2(Input.MoveX.Value, Input.MoveY.Value);
                if (aim != Vector2.Zero)
                {
                    lastAim = aim;
                    if (dashBuffer == 4)
                    {
                        lastDashAim = aim;
                    }
                    else if (jumpBuffer == 4)
                    {
                        lastJumpAim = aim;
                    }
                }
                if (!wasTransporting)
                {
                    player.RefillDash();
                    player.RefillStamina();
                }
                TransportUpdate(player);
                if (CurrentNode != null && !level.Transitioning && CurrentNode.Control is not ControlTypes.None)
                {
                    bool inSolid = player != null && player.CollideCheck<Solid>();
                    if (pC != null)
                    {
                        if (Input.CrouchDashPressed && pC.HasBlip)
                        {
                            if (dashTimer <= 0)
                            {
                                Vector2 dir = new Vector2(Input.MoveX.Value, Input.MoveY.Value);
                                if (dir == Vector2.Zero) dir = Dir;
                                Vector2 speed = dir * 240f;
                                if (pC.TryGetBlipTarget(pC.Position, speed, out Vector2 target))
                                {
                                    Input.CrouchDash.ConsumeBuffer();
                                    DashEject(player);
                                }
                            }
                        }
                        else if (!inSolid)
                        {
                            if (CanEject(CurrentNode.Node, out bool dashed, out bool jumped))
                            {
                                dashTimer = Engine.DeltaTime * 10f;
                                if (dashed)
                                {
                                    Vector2 mult = new Vector2(190f, 130f);
                                    pC.NoGravityFor(0.1f);
                                    Eject(pC, true, lastDashAim * mult);
                                    Input.Dash.ConsumePress();
                                    Input.Dash.ConsumeBuffer();
                                }
                                else if (jumped)
                                {
                                    DirectionalEject(pC, false, Vector2.Zero);
                                    if (lastJumpAim.Y <= 0)
                                    {
                                        pC.Jumps = (int)Calc.Max(1, pC.Jumps);
                                        pC.CalidusJump(false, false);
                                    }
                                    Input.Jump.ConsumePress();
                                }
                                else
                                {
                                    DirectionalEject(pC, false, Vector2.Zero);
                                }
                            }
                        }
                    }
                    else if (!inSolid)
                    {
                        if (dashTimer <= 0 && Input.DashPressed)
                        {
                            dashTimer = Engine.DeltaTime * 10;
                            DashEject(player);
                        }
                        else if (Input.Jump.Pressed)
                        {
                            HopEject(player, true);
                        }
                    }
                }

            }
            else if (dashTimer <= 0 && playerCanEnterRail && !level.Transitioning)
            {
                lastAim = lastDashAim = lastJumpAim = Vector2.Zero;
                OnEnter(Colliding(player), player);
            }
            else
            {
                lastAim = Vector2.Zero;
            }
            amountTraveled = RailPosition - PrevRailPosition;
            CurrentSign = amountTraveled.Sign();
            if (CurrentSign != PrevSign && CurrentSign != Vector2.Zero)
            {
                LastSign = PrevSign;
            }
            wasTransporting = Transporting;
        }
        public bool CanEject(Nodes node, out bool dashed, out bool jumped)
        {
            dashed = dashBuffer > 0;
            jumped = jumpBuffer > 0;
            if (!(dashTimer <= 0 && (dashed || jumped)))
            {
                return false;
            }
            if (node is BitrailNode.Nodes.Single && lastExitNodeReenterTimer <= 0)
            {
                return true;
            }
            if (node is BitrailNode.Nodes.DeadEnd)
            {
                if (CurrentNode != IgnoreNode && (LastExitNode != FirstExitNode || lastExitNodeReenterTimer <= 0))
                {
                    return true;
                }
            }
            return false;
        }
        public BitrailNode Colliding(Player player)
        {
            string name = CurrentLevelName;
            if (!string.IsNullOrEmpty(name) && ExitNodesByLevel.ContainsKey(name))
            {
                foreach (BitrailNode node in ExitNodesByLevel[name])
                {
                    if (ExitColliders.ContainsKey(node) &&
                        Collide.CheckRect(player, ExitColliders[node].Bounds) &&
                        node != IgnoreNode &&
                        node != FirstExitNode)
                    {
                        if (Collide.CheckRect(player, ExitColliders[node].Bounds) && node != IgnoreNode && node != FirstExitNode)
                        {
                            if (PreventFamilyRegrabbing && PreviousNode != null && DashedFrom.HasValue
                                && PreviousNode.Family.EntryPoints.Contains(node))
                            {
                                if (Vector2.Distance(node.RenderPosition + Vector2.One * 4, DashedFrom.Value) > 24)
                                {
                                    return node;
                                }
                                return null;
                            }
                            else
                            {
                                if (DashedFrom.HasValue && Input.Jump) return null;
                                return node;
                            }
                        }
                    }
                }
            }
            return null;
        }
        public Vector2 amountTraveled;
        public void TransportUpdate(Player player)
        {
            if (!CurrentNode.IsEntryPoint && WaitingForEject)
            {
                StopEjectTimer();
            }
            Vector2 target = CurrentNode.RenderPosition + Vector2.One * 4;
            if (RailPosition != target) //move to the position at the rate set by "Speed"
            {
                Vector2 prev = RailPosition;
                RailPosition = Calc.Approach(RailPosition, target, Speed * Engine.DeltaTime);
                dist += Vector2.Distance(prev, RailPosition);
                if (dist > distLimit)
                {
                    for (float d = 0; d < dist; d += distLimit)
                    {
                        EmitParticle(Vector2.Lerp(prev, RailPosition, d / dist));
                    }
                    dist %= distLimit;
                }
            }
            else if (CurrentNode != null)
            {
                if (CurrentNode.Node is BitrailNode.Nodes.Exit)
                {
                    ExitEject(player);
                    return;
                }
                else if (CurrentNode.Bounces > 0)
                {
                    CurrentNode.BounceBack();
                    Dir = ValidateDirection(-Dir, Vector2.Zero);
                }
                else
                {
                    GetNextDirection();
                    if (CurrentNode.TimeLimit > 0 && !WaitingForEject) StartEjectTimer(CurrentNode);
                }
                PreviousNode = CurrentNode;
                CurrentNode = CurrentNode.GetNeighbor(Dir);
            }
            Vector2 offset;
            if (player is PlayerCalidus)
            {
                offset = Vector2.One * -4;
            }
            else
            {
                offset = Vector2.UnitY * player.Height / 2;
            }
            player.Position.X = RailPosition.X + offset.X;
            player.Position.Y = RailPosition.Y + offset.Y;
            player.Position.Floor();
        }
        public override void Render()
        {
            if (IgnoreLevel)
            {
                return;
            }
            foreach (Vector2 vec in checkedPositions)
            {
                Draw.Point(vec, Color.White);
            }
            foreach (BitrailNode node in Nodes)
            {
                if (node.OnScreen && !node.InSolid)
                {
                    node.Render();
                }
            }
            if (Transporting)
            {
                Draw.Rect(RailPosition - Vector2.One * 4, 8, 8, Color.Red);
            }
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            foreach (KeyValuePair<BitrailNode, Collider> pair in ExitColliders)
            {
                Color color = pair.Key == FirstExitNode ? Color.Cyan : pair.Key == IgnoreNode ? Color.Gray : pair.Key == LastExitNode ? Color.Yellow : Color.Blue;
                Draw.HollowRect(pair.Value, color);
            }
            Draw.Point(RailPosition, Color.White);

        }
        public BitrailFGRenderer FGRenderer;
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Reset(this);
            Particles.RemoveSelf();
            FGRenderer?.RemoveSelf();
        }
        public static void Reset(BitrailTransporter t)
        {
            IgnoreLevel = false;
            Transporting = false;
            t.FirstExitNode = null;
            t.LastExitNode = null;
            t.IgnoreNode = null;
            t.CurrentNode = null;
            t.PreviousNode = null;
        }
        public Vector2 PrevRailPosition;
        public bool Stopped => PrevRailPosition == RailPosition;
        public void DashEject(Player player)
        {
            Eject(player);
            DashedFrom = RailPosition;
            dashAlarm.Start();
            if (player != null)
            {
                if (player is PlayerCalidus c)
                {
                    c.StateMachine.ForceState(PlayerCalidus.BlipState);
                }
                else
                {
                    player?.StateMachine.ForceState(Player.StDash);
                }
                player.Visible = false;
                Add(new Coroutine(VisibilityRoutine(player)));
            }
        }
        private IEnumerator VisibilityRoutine(Player player)
        {
            yield return null;
            player.Visible = true;
        }
        public void Eject(Player player, bool refill, Vector2 speed)
        {
            Eject(player, true);
            if (player != null)
            {
                if (refill)
                {
                    player.RefillStamina();
                    player.RefillDash();
                }
                player.Speed = speed;
                player.Visible = true;
            }
        }
        public void DirectionalEject(Player player, bool refill, Vector2 dir, float speedMult = 1)
        {
            Vector2 speed = new Vector2(Math.Sign(dir.X) * 60f, Math.Sign(dir.Y) * 180f) * speedMult;
            Eject(player, refill, speed);
        }

        public void HopEject(Player player, bool refill)
        {
            Eject(player, true);
            if (player != null)
            {
                if (refill)
                {
                    player.RefillStamina();
                    player.RefillDash();
                }
                if (player is PlayerCalidus pc)
                {
                    pc.Jumps = (int)Calc.Max(1, pc.Jumps);
                    pc.CalidusJump(false);
                    //pc.Speed.Y = -250f;
                    pc.Visible = true;
                }
                else
                {
                    player.Jump(false);
                }
            }

        }

        public void OutOfTimeEject(Player player)
        {
            if (player != null)
            {
                if (player.CollideCheck<Solid>())
                {
                    Dir = Vector2.Zero;
                    player.Die(Vector2.Zero);
                }
                else
                {
                    player.Speed.Y += Dir.Y * 0.5f * Speed;
                    player.Speed.X += Dir.X * 0.5f * Speed;
                    player.Visible = false;
                    Add(new Coroutine(VisibilityRoutine(player)));
                }
            }
            Eject(player, true);
        }
        public void ExitEject(Player player)
        {
            if (player != null)
            {
                if (player.CollideCheck<Solid>())
                {
                    Dir = Vector2.Zero;
                    player.Die(Vector2.Zero);
                }
                else
                {
                    if (Dir.Y < 0)
                    {
                        player.Speed.Y -= 180f;
                        if (player is PlayerCalidus pC)
                        {
                            pC.NoGravityFor(0.1f);
                        }
                    }
                    player.Speed.X += Dir.X * Speed;
                    player.Visible = false;
                    Add(new Coroutine(VisibilityRoutine(player)));
                }
            }
            Eject(player, true);
        }

        public void Eject(Player player)
        {

            if (player is PlayerCalidus c)
            {
                c.InRail = false;
                c.Absorb.Disabled = true;
                c.Position = CurrentNode.RenderPosition + Vector2.One;
            }
            lastExitNodeReenterTimer = Engine.DeltaTime * 10f;
            exitAlarm.Start();
            StopEjectTimer();
            Dir = Vector2.Zero;
            if (CurrentNode.Node is BitrailNode.Nodes.DeadEnd or BitrailNode.Nodes.Single)
            {
                LastExitNode = CurrentNode;
            }
            IgnoreNode = LastExitNode;
            PreviousNode = CurrentNode;
            FirstExitNode = null;
            CurrentNode = null;
            Transporting = false;
        }
        public void Eject(Player player, bool forceNormalState)
        {
            if (forceNormalState && player is PlayerCalidus c)
            {
                c.StateMachine.ForceState(PlayerCalidus.NormalState);
            }
            Eject(player);
        }
        private bool swap;
        private void EmitParticle()
        {
            Vector2 offset = (Dir.X != 0 ? Vector2.UnitY : Vector2.UnitX) * (swap ? -1 : 1);
            Particle.Acceleration = offset;
            Particles.Emit(Particle, RailPosition + offset * 2, offset.Angle());
            swap = !swap;
        }
        private void EmitParticle(Vector2 position)
        {
            Vector2 offset = (Dir.X != 0 ? Vector2.UnitY : Vector2.UnitX) * (swap ? -1 : 1);
            Particle.Acceleration = offset;
            Particles.Emit(Particle, position + offset * 2, offset.Angle());
            swap = !swap;
        }
        public void StartEjectTimer(BitrailNode node)
        {
            WaitingForEject = true;
            nodeTimer = nodeTimeLimit = node.TimeLimit;
        }
        public void StopEjectTimer()
        {
            WaitingForEject = false;
            nodeTimer = nodeTimeLimit = 0;
        }
        public Vector2 FullInputDirection()
        {
            if (Input.MoveX.Value != 0) return Vector2.UnitX * Input.MoveX.Value;
            else if (Input.MoveY.Value != 0) return Vector2.UnitY * Input.MoveY.Value;
            return Vector2.Zero;
        }
        public Vector2 InputDirection(Vector2 dir)
        {
            if (dir.X == 0)
            {
                if (Input.MoveX.Value != 0) dir = Vector2.UnitX * Input.MoveX.Value;
                else if (Input.MoveY.Value != 0) dir = Vector2.UnitY * Input.MoveY.Value;
            }
            else if (dir.Y == 0)
            {
                if (Input.MoveY.Value != 0) dir = Vector2.UnitY * Input.MoveY.Value;
                else if (Input.MoveX.Value != 0) dir = Vector2.UnitX * Input.MoveX.Value;
            }
            return dir;
        }
        public Vector2 PreviousDirection;
        public void GetNextDirection()
        {
            if (CurrentNode == null) return;
            Vector2 prev = Dir;
            if (PreviousDirection == Vector2.Zero || PreviousDirection == Dir)
            {
                PreviousDirection = Dir.Rotate(90f.ToRad());
            }
            ControlTypes control = CurrentNode.Control;
            switch (CurrentNode.Node)
            {
                case BitrailNode.Nodes.ThreeWay:
                    switch (control)
                    {
                        case ControlTypes.Default:
                            Dir = CurrentNode.NextIntersectionDirection(PreviousDirection, Dir, InputDirection(Dir), DirMode);
                            break;
                        case ControlTypes.Full:
                            Vector2 newDir = FullInputDirection();
                            if (CurrentNode.NextDirectionValid(newDir)) Dir = newDir;
                            break;
                        case ControlTypes.None:
                            Dir = CurrentNode.NextIntersectionDirection(PreviousDirection, Dir, Dir, DirMode);
                            break;
                    }
                    break;
                case BitrailNode.Nodes.FourWay:
                    switch (control)
                    {
                        case ControlTypes.Default:
                            Dir = InputDirection(Dir);
                            break;
                        case ControlTypes.Full:
                            Vector2 newDir = FullInputDirection();
                            if (CurrentNode.NextDirectionValid(newDir)) Dir = newDir;
                            break;
                        case ControlTypes.None:
                            Dir = prev;
                            break;
                    }
                    break;
                case BitrailNode.Nodes.DeadEnd:
                    LastExitNode = CurrentNode;
                    switch (control)
                    {
                        case ControlTypes.Default:
                        case ControlTypes.Full:
                            Vector2 newDir = FullInputDirection();
                            if (CurrentNode.HasDirection(newDir)) Dir = newDir;
                            break;
                        case ControlTypes.None:
                            Dir = CurrentNode.NextSingleDirection();
                            break;
                    }
                    break;
                case BitrailNode.Nodes.Corner:
                    Dir = CurrentNode.NextCornerDirection(Dir);
                    break;
            }
            if (Dir != prev && Dir != -prev && Dir != Vector2.Zero && prev != Vector2.Zero)
            {
                PreviousDirection = prev;
            }
        }

        public void OnEnter(BitrailNode startingNode, Player player)
        {
            if (startingNode == null || startingNode == IgnoreNode || !startingNode.IsEntryPoint) return;
            if (player is PlayerCalidus c)
            {
                if (c.NoRailTimer > 0) return;
                c.StateMachine.ForceState(PlayerCalidus.RailState);
                c.InRail = true;
                c.AddBitrailAbsorb(startingNode);
            }
            dashTimer = Engine.DeltaTime * 10;
            Transporting = true;
            FirstExitNode = LastExitNode = CurrentNode = startingNode;
            RailPosition = CurrentNode.RenderPosition + Vector2.One * 4;
            player.Speed = Vector2.Zero;
            switch (CurrentNode.Control)
            {
                case ControlTypes.Default:
                    List<Direction> directions = CurrentNode.Directions;
                    if (player is PlayerCalidus)
                    {
                        Dir = BitrailHelper.DirectionToVector(directions[0]);
                    }
                    else if (player.DashDir.X != 0 && (directions.Contains(Direction.Left) || directions.Contains(Direction.Right)))
                    {
                        Dir = Calc.Ceiling(Vector2.UnitX * player.DashDir.X);
                    }
                    else if (player.DashDir.Y != 0 && (directions.Contains(Direction.Up) || directions.Contains(Direction.Down)))
                    {
                        Dir = Calc.Ceiling(Vector2.UnitY * player.DashDir.Y);
                    }
                    else
                    {
                        Dir = BitrailHelper.DirectionToVector(directions[0]);
                    }
                    break;
                case ControlTypes.Full:
                    Vector2 newDir = FullInputDirection();
                    if (CurrentNode.HasDirection(newDir)) Dir = newDir;
                    break;
                case ControlTypes.None:
                    Dir = CurrentNode.NextSingleDirection();
                    break;
            }

            player.Collidable = false;
        }
        public Vector2 ValidateDirection(Vector2 dir, Vector2 fallback)
        {
            return dir == -fallback || !CurrentNode.NextDirectionValid(dir) ? fallback : dir;
        }
        public void UpdateColliderPositions()
        {
            foreach (KeyValuePair<BitrailNode, Collider> pair in ExitColliders)
            {
                ExitColliders[pair.Key].Position = pair.Key.Position + Position;
            }
        }
        public static void Unload()
        {
            Everest.Events.LevelLoader.OnLoadingThread -= LevelLoader_OnLoadingThread;
            On.Celeste.Player.Render -= Player_Render;
            On.Celeste.Player.TransitionTo -= Player_TransitionTo;
            Everest.Events.Level.OnTransitionTo -= Level_OnTransitionTo;
            On.Celeste.Player.Update -= Player_Update;
            Everest.Events.Player.OnSpawn -= Player_OnSpawn;
            Everest.Events.Player.OnDie -= Player_OnDie;
            On.Celeste.PlayerCollider.Check -= PlayerCollider_Check;
            NodesByLevel.Clear();
        }


        public static void Load()
        {
            Everest.Events.LevelLoader.OnLoadingThread += LevelLoader_OnLoadingThread;
            On.Celeste.Player.Render += Player_Render;
            On.Celeste.Player.TransitionTo += Player_TransitionTo;
            Everest.Events.Level.OnTransitionTo += Level_OnTransitionTo;
            On.Celeste.Player.Update += Player_Update;
            Everest.Events.Player.OnDie += Player_OnDie;
            Everest.Events.Player.OnSpawn += Player_OnSpawn;
            On.Celeste.PlayerCollider.Check += PlayerCollider_Check;
        }

        private static void Player_OnDie(Player obj)
        {
            Reset(obj.Scene);
            CheckedLevel = false;
        }
        public override void SceneBegin(Scene scene)
        {
            base.SceneBegin(scene);
            //CheckLevel((scene as Level).Session.Level);
        }

        private static void Level_OnTransitionTo(Level level, LevelData next, Vector2 direction)
        {
            //CheckLevel(next.Name);
        }

        private static void Reset(Scene scene)
        {
            if (scene is Level level && level.Tracker.GetEntity<BitrailTransporter>() is BitrailTransporter bT)
            {
                Reset(bT);
            }
        }
        public static bool CheckedLevel;
        public static void CheckLevel(string levelName)
        {
            if (CheckedLevel) return;
            IgnoreLevel = !NodesByLevel.ContainsKey(levelName);
            CheckedLevel = true;
        }
        private static void Player_OnSpawn(Player obj)
        {
            Reset(obj.Scene);
        }

        private static void Player_Update(On.Celeste.Player.orig_Update orig, Player self)
        {
            bool prev = self.Collidable;
            if (Transporting)
            {
                self.Collidable = false;
            }
            orig(self);
            self.Collidable = prev;
        }

        private static bool Player_TransitionTo(On.Celeste.Player.orig_TransitionTo orig, Player self, Vector2 target, Vector2 direction)
        {

            if (Transporting)
            {
                self.UpdateHair(applyGravity: false);
                self.UpdateCarry();
                return true;
            }
            return orig(self, target, direction);
        }


        private static bool PlayerCollider_Check(On.Celeste.PlayerCollider.orig_Check orig, PlayerCollider self, Player player)
        {
            if (Transporting) return false;
            return orig(self, player);
        }

        private static void Player_Render(On.Celeste.Player.orig_Render orig, Player self)
        {
            if (!Transporting)
            {
                orig(self);
            }
        }
        private static void LevelLoader_OnLoadingThread(Level level)
        {
            level.Add(new BitrailTransporter());
        }
        public class BitrailFGRenderer : Entity
        {
            public BitrailTransporter Parent;
            public Rectangle MaskBounds;
            public const int BufferSize = 8;
            public const int HalfSize = BufferSize / 2;
            public const int HalfSizeSquared = HalfSize * HalfSize;
            public string LevelName => Parent.CurrentLevelName;
            private static VirtualRenderTarget _buffer;
            public static VirtualRenderTarget Buffer = _buffer ??= VirtualContent.CreateRenderTarget("bitrailTarget", BufferSize, BufferSize);
            private static VirtualRenderTarget _mask;
            public static VirtualRenderTarget Mask = _mask ??= VirtualContent.CreateRenderTarget("bitrailMask", BufferSize, BufferSize);

            public BitrailFGRenderer(BitrailTransporter parent) : base()
            {
                Parent = parent;
                Depth = -100001;
                Tag = Parent.Tag;
                Add(new BeforeRenderHook(BeforeRender));
                Collider = new Hitbox(BufferSize, BufferSize);
                Collider.CenterOrigin();
            }
            private void BeforeRender()
            {
                if (IgnoreLevel) return;
                Buffer.DrawThenMask(RenderMask, RenderOverlay, Matrix.Identity);
            }
            private void RenderMask()
            {
                string levelName = LevelName;
                Vector2 topLeft = Parent.RailPosition - Vector2.One * HalfSize;
                if (NodesByLevel.ContainsKey(levelName))
                {
                    foreach (BitrailNode node in NodesByLevel[levelName])
                    {
                        if (node.InMask)
                        {
                            node.RenderAt(node.RenderPosition - topLeft);
                        }
                    }
                }
            }
            private void RenderOverlay()
            {
                Draw.Rect(Vector2.Zero, BufferSize, BufferSize, Color.Red);
            }
            public override void Render()
            {
                base.Render();
                if (IgnoreLevel) return;
                foreach (BitrailNode node in Parent.Nodes)
                {
                    if (node.OnScreen && node.InSolid)
                    {
                        node.Render();
                    }
                }
                if (Transporting)
                {
                    Draw.SpriteBatch.Draw((Texture2D)Buffer, Parent.RailPosition - Vector2.One * HalfSize, Color.Red);
                }
            }
            public bool TryGetNodesByLevel(out HashSet<BitrailNode> nodes)
            {
                string name = LevelName;
                if (string.IsNullOrEmpty(name) || IgnoreLevel || NodesByLevel == null || !NodesByLevel.ContainsKey(name))
                {
                    nodes = null;
                    return false;
                }
                nodes = NodesByLevel[name];
                return true;
            }
            public override void Update()
            {
                base.Update();
                Position = Parent.RailPosition;
                foreach (BitrailNode node in Parent.Nodes)
                {
                    node.InMask = Transporting && node.OnScreen && CollideRect(node.Bounds);
                }
            }
            public override void Removed(Scene scene)
            {
                base.Removed(scene);
                _buffer?.Dispose();
                _buffer = null;
                _mask?.Dispose();
                _mask = null;
            }
            public override void DebugRender(Camera camera)
            {
                base.DebugRender(camera);
                if (TryGetNodesByLevel(out var nodes))
                {
                    foreach (BitrailNode node in nodes)
                    {
                        if (node.OnScreen && node.InSolid)
                        {
                            Draw.HollowRect(node.RenderPosition, 8, 8, Color.Magenta);
                        }
                    }
                    Vector2 center = camera.Position + Vector2.One * 16;
                    Draw.Circle(center, 16, Color.White, 32);
                    Draw.Line(center, center + Parent.lastAim * 16, Color.Red);
                    Draw.Point(center, Color.Red);
                    if (Transporting)
                    {
                        Draw.Rect(Parent.RailPosition + Parent.PreviousDirection.Sign() * 8, 8, 8, Color.Red);
                        Draw.Rect(Parent.RailPosition + Parent.Dir.Sign() * 8, 8, 8, Color.Cyan);
                    }
                }
            }

        }
    }
}
