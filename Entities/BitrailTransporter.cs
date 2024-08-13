using System;
using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Helpers;
using Microsoft.Xna.Framework;
using Monocle;
using static Celeste.DustGraphic;
using static Celeste.Mod.PuzzleIslandHelper.Components.BitrailNode;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked]
    public class BitrailTransporter : Entity
    {
        public static bool MoveThroughTransitions => PianoModule.Session.DEBUGBOOL;
        private static bool PreventFamilyRegrabbing = false;
        public static float Speed => Player.DashSpeed;
        public static bool Transporting;
        public string CurrentLevelName => SceneAs<Level>().Session.Level;
        public Grid Grid => BitrailHelper.Grid;
        public Vector2 TransportPosition;
        public Vector2 Dir;
        public HashSet<BitrailNode> Nodes = new();
        public Dictionary<string, HashSet<BitrailNode>> ExitNodesByLevel = new();
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
        private bool ChangedDirectionsOnce;
        public BitrailTransporter() : base()
        {
            Depth = 1;
            Tag |= Tags.Global | Tags.TransitionUpdate;
            BitrailHelper.InitializeGrids();
            Collider = new Hitbox(Grid.Width, Grid.Height);
            Position = BitrailHelper.MapPosition;
            Nodes = BitrailHelper.CreateNodes();
            foreach (BitrailNode node in Nodes)
            {
                Add(node);
            }
            Add(exitAlarm = Alarm.Create(Alarm.AlarmMode.Persist, delegate { IgnoreNode = null; }, 0.4f, false));
            Add(dashAlarm = Alarm.Create(Alarm.AlarmMode.Persist, delegate { DashedFrom = null; }, 0.5f, false));
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Player = scene.GetPlayer();
            BitrailHelper.PrepareNodes(Nodes);
            BitrailHelper.AssignFamilies(Nodes);
            ExitColliders = BitrailHelper.CreateExitColliders(Nodes);
            ExitNodesByLevel = BitrailHelper.ExitNodesByLevel(Nodes, scene as Level);
        }
        public override void Update()
        {
            base.Update();
            if (Scene is not Level level || level.GetPlayer() is not Player player || player.Dead)
            {
                Transporting = false;
                DashedFrom = null;
                return;
            }
            Player = player;
            if (WaitingForEject && nodeTimeLimit > 0)
            {
                nodeTimer -= Engine.DeltaTime;
                if (nodeTimer > 0) CurrentNode.WarningAmount = 1 - nodeTimeLimit / nodeTimer;
                else LameEject(player);
            }
            UpdateColliderPositions();
            if (Transporting)
            {
                if (!wasTransporting)
                {
                    player.RefillDash();
                    player.RefillStamina();
                }
                TransportUpdate(player);
                if (!level.Transitioning)
                {
                    if (Input.DashPressed || Input.CrouchDashPressed)
                    {
                        DashEject(player);
                    }
                    else if (Input.Jump.Pressed)
                    {
                        HopEject(player, true);
                    }
                }
            }
            else if (player.DashAttacking && !level.Transitioning)
            {
                OnEnter(Colliding(player), player);
            }
            wasTransporting = Transporting;
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
        public void TransportUpdate(Player player)
        {
            if (!CurrentNode.IsEntryPoint && WaitingForEject)
            {
                StopEjectTimer();
            }
            Vector2 target = CurrentNode.RenderPosition;
            if (TransportPosition != target) //move to the position at the rate set by "Speed"
            {
                MoveWithParticles(Calc.Approach(TransportPosition, target, Speed * Engine.DeltaTime));
            }
            else if (CurrentNode != null)
            {
                if (CurrentNode.Node is BitrailNode.Nodes.Exit)
                {
                    HopEject(Player, true);
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
            player.MoveToX(TransportPosition.X + player.Width / 2);
            player.MoveToY(TransportPosition.Y + player.Height / 2);

        }
        public override void Render()
        {
            foreach (BitrailNode node in Nodes)
            {
                if (node.OnScreen)
                {
                    node.Render();
                }
            }
            if (Transporting)
            {
                Draw.Rect(TransportPosition, 8, 8, Color.Red);
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
        }
        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            Reset(this);
        }
        public static void Reset(BitrailTransporter t)
        {
            Transporting = false;
            t.FirstExitNode = null;
            t.LastExitNode = null;
            t.IgnoreNode = null;
            t.CurrentNode = null;
            t.PreviousNode = null;
        }
        public void DashEject(Player player)
        {
            Eject();
            DashedFrom = TransportPosition;
            dashAlarm.Start();
            player?.StateMachine.ForceState(Player.StDash);
        }
        public void HopEject(Player player, bool refill)
        {
            Eject();
            if (player != null)
            {
                if (refill)
                {
                    player.RefillStamina();
                    player.RefillDash();
                }
                player.Jump(false);
                player.Speed += Dir.SafeNormalize(0.5f) * Speed;
            }
        }
        public void LameEject(Player player)
        {
            Eject();
            if (player != null)
            {
                player.Speed.X += Dir.X * 0.5f * Speed;
            }
        }
        public void Eject()
        {
            ChangedDirectionsOnce = false;
            exitAlarm.Start();
            StopEjectTimer();
            IgnoreNode = LastExitNode;
            PreviousNode = CurrentNode;
            FirstExitNode = null;
            CurrentNode = null;
            Transporting = false;
        }
        public void MoveWithParticles(Vector2 to)
        {
            while (TransportPosition != to)
            {
                TransportPosition = Calc.Approach(TransportPosition, to, 1);
                //todo:emit particles
            }
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
        public void GetNextDirection()
        {
            if (CurrentNode == null) return;
            Vector2 prev = Dir;
            ControlTypes control = CurrentNode.Control;
            switch (CurrentNode.Node)
            {
                case BitrailNode.Nodes.ThreeWay:
                    switch (control)
                    {
                        case ControlTypes.Default:
                            Dir = ValidateDirection(InputDirection(Dir), prev);
                            break;
                        case ControlTypes.Full:
                            Vector2 newDir = FullInputDirection();
                            if (newDir == Vector2.Zero || CurrentNode.HasSameDirection(newDir)) Dir = newDir;
                            break;
                        case ControlTypes.None:
                            Dir = CurrentNode.NextIntersectionDirection(Dir);
                            break;
                    }
                    ChangedDirectionsOnce = true;
                    break;
                case BitrailNode.Nodes.FourWay:
                    switch (control)
                    {
                        case ControlTypes.Default:
                            Dir = ValidateDirection(InputDirection(Dir), prev);
                            break;
                        case ControlTypes.Full:
                            Vector2 newDir = FullInputDirection();
                            if (newDir == Vector2.Zero || CurrentNode.HasSameDirection(newDir)) Dir = newDir;
                            break;
                        case ControlTypes.None:
                            Dir = prev;
                            break;
                    }
                    ChangedDirectionsOnce = true;
                    break;
                case BitrailNode.Nodes.DeadEnd:
                    LastExitNode = CurrentNode;
                    switch (control)
                    {
                        case ControlTypes.Default:
                        case ControlTypes.Full:
                            Vector2 newDir = FullInputDirection();
                            if (newDir == Vector2.Zero || CurrentNode.HasSameDirection(newDir)) Dir = newDir;
                            break;
                        case ControlTypes.None:
                            Dir = CurrentNode.NextSingleDirection();
                            break;
                    }
                    ChangedDirectionsOnce = true;
                    break;
                case BitrailNode.Nodes.Corner:
                    Dir = CurrentNode.NextCornerDirection(Dir);
                    ChangedDirectionsOnce = true;
                    break;
            }
        }
        public void OnEnter(BitrailNode startingNode, Player player)
        {
            if (startingNode == null || !startingNode.IsEntryPoint) return;
            Transporting = true;
            FirstExitNode = LastExitNode = CurrentNode = startingNode;
            TransportPosition = CurrentNode.RenderPosition;
            player.Speed = Vector2.Zero;
            switch (CurrentNode.Control)
            {
                case ControlTypes.Default:
                    List<Direction> directions = CurrentNode.Directions;
                    if (player.DashDir.X != 0 && (directions.Contains(Direction.Left) || directions.Contains(Direction.Right)))
                    {
                        Dir = Calc.Ceiling(Vector2.UnitX * player.DashDir.X);
                    }
                    else if (player.DashDir.Y != 0 && (directions.Contains(Direction.Up) || directions.Contains(Direction.Down)))
                    {
                        Dir = Calc.Ceiling(Vector2.UnitY * player.DashDir.Y);
                    }
                    break;
                case ControlTypes.None:
                    Dir = CurrentNode.NextSingleDirection();
                    break;
                case ControlTypes.Full:
                    Dir = ValidateDirection(FullInputDirection(), Vector2.Zero);
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
                ExitColliders[pair.Key].Position = pair.Key.Position + Position - Vector2.One;
            }
        }
        public static void Unload()
        {
            Everest.Events.LevelLoader.OnLoadingThread -= LevelLoader_OnLoadingThread;
            On.Celeste.Player.Render -= Player_Render;
            On.Celeste.Player.TransitionTo -= Player_TransitionTo;
            On.Celeste.Player.Update -= Player_Update;
            Everest.Events.Player.OnSpawn -= Player_OnSpawn;
            On.Celeste.PlayerCollider.Check -= PlayerCollider_Check;
        }
        public static void Load()
        {
            Everest.Events.LevelLoader.OnLoadingThread += LevelLoader_OnLoadingThread;
            On.Celeste.Player.Render += Player_Render;
            On.Celeste.Player.TransitionTo += Player_TransitionTo;
            On.Celeste.Player.Update += Player_Update;
            Everest.Events.Player.OnSpawn += Player_OnSpawn;
            On.Celeste.PlayerCollider.Check += PlayerCollider_Check;
        }

        private static void Reset(Scene scene)
        {
            if (scene is Level level && level.Tracker.GetEntity<BitrailTransporter>() is BitrailTransporter bT)
            {
                Reset(bT);
            }
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
    }
}
