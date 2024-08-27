using System;
using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Helpers;
using ExtendedVariants.Variants;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
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
        /*public class NodeAnimation : Sprite
        {
            public enum Anims
            {
                Enter,
                Exit
            }
            private bool hide = true;
            public NodeAnimation() : base(GFX.Game, "objects/PuzzleIslandHelper/bitRail/nodeAnimations/")
            {
                OnFinish = (s) =>
                {
                    RemoveSelf();
                };

            }
            public void PlayAnimation(Anims anim)
            {
            }
        }*/
        private static VirtualRenderTarget _buffer;
        public static VirtualRenderTarget Buffer = _buffer ??= VirtualContent.CreateRenderTarget("bitrailTarget", 24, 24);
        private static VirtualRenderTarget _mask;
        public static VirtualRenderTarget Mask = _mask ??= VirtualContent.CreateRenderTarget("bitrailMask", 24, 24);
        private BeforeRenderHook hook;
        public BitrailTransporter() : base()
        {
            Depth = -10002;
            Tag |= Tags.Global | Tags.TransitionUpdate;
            Particles = new ParticleSystem(Depth - 1, 200);
            Particles.Tag = Tag;
            BitrailHelper.InitializeGrids();
            Position = BitrailHelper.MapPosition;
            Nodes = BitrailHelper.CreateNodes();
            foreach (BitrailNode node in Nodes)
            {
                Add(node);
            }
            Add(exitAlarm = Alarm.Create(Alarm.AlarmMode.Persist, delegate { IgnoreNode = null; }, 0.4f, false));
            Add(dashAlarm = Alarm.Create(Alarm.AlarmMode.Persist, delegate { DashedFrom = null; }, 0.5f, false));
            Add(hook = new BeforeRenderHook(BeforeRender));
        }



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

            foreach (BitrailNode node in Nodes)
            {
                if (node.Node is not BitrailNode.Nodes.Single or BitrailNode.Nodes.DeadEnd or BitrailNode.Nodes.Exit)
                {
                    if (Collide.CheckPoint(level.SolidTiles, node.Position))
                    {
                        node.SwitchToInSolid();
                    }
                }
            }
        }
        private float dashTimer = 0;
        public static bool IgnoreLevel;
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
            bool playerDashing = pC != null ? pC.Blipping : player.DashAttacking;
            dashTimer = Calc.Max(0, dashTimer - Engine.DeltaTime);
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
                    if (pC != null)
                    {
                        if (dashTimer <= 0 && Input.CrouchDashPressed)
                        {
                            Vector2 dir = new Vector2(Input.MoveX.Value, Input.MoveY.Value);
                            if (dir == Vector2.Zero) dir = Dir;
                            Vector2 speed = dir * 240f;
                            if (pC.TryGetBlipTarget(pC.Position, speed, out Vector2 target))
                            {
                                pC.CrouchDashed = true;
                                DashEject(player);
                            }
                        }
                        else if (!player.CollideCheck<Solid>())
                        {
                            if (dashTimer <= 0 && Input.DashPressed)
                            {
                                dashTimer = Engine.DeltaTime * 10;
                                DashEject(player);
                            }
                        }
                    }
                    else if (!player.CollideCheck<Solid>())
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
            else if (dashTimer <= 0 && playerDashing && !level.Transitioning)
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
                    LameEject(player);
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
                Draw.Rect(SceneAs<Level>().Camera.Position, 8, 8, Color.Blue);
                return;
            }
            foreach (BitrailNode node in Nodes)
            {
                if (node.OnScreen)
                {
                    node.Render();
                }
            }
            if (Transporting)
            {
                Draw.Rect(SceneAs<Level>().Camera.Position, 16, 8, Color.Yellow);
                Draw.SpriteBatch.Draw((Texture2D)Buffer, RailPosition - Vector2.One * 12, Color.White);
                //Draw.Rect(RailPosition - Vector2.One * 4, 8, 8, Color.Red);
            }
        }
        private void RenderMask()
        {
            string levelName = CurrentLevelName;
            Vector2 topLeft = RailPosition - Vector2.One * 12;
            if (NodesByLevel.ContainsKey(levelName))
            {
                foreach (BitrailNode node in NodesByLevel[levelName])
                {
                    if (node.OnScreen && Vector2.DistanceSquared(node.RenderPosition, RailPosition) < 144f)
                    {
                        node.RenderAt(node.RenderPosition - topLeft);
                    }
                }
            }
        }
        private void RenderOverlay()
        {
            Draw.Rect(Vector2.Zero, 24, 24, Color.Red);
        }

        private void BeforeRender()
        {
            if (IgnoreLevel) return;
            Buffer.DrawThenMaskWhite(RenderMask, RenderOverlay, Matrix.Identity);
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
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Reset(this);
            Particles.RemoveSelf();
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
        public void HopEject(Player player, bool refill)
        {
            Eject(player, true);
            if (player != null && player is not PlayerCalidus)
            {
                if (refill)
                {
                    player.RefillStamina();
                    player.RefillDash();
                }
                player.Jump(false);
            }
        }
        public void LameEject(Player player)
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

        public void Eject(Player player)
        {
            if (player is PlayerCalidus c)
            {
                c.InRail = false;
            }
            exitAlarm.Start();
            StopEjectTimer();
            Dir = Vector2.Zero;
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
                            Dir = CurrentNode.NextIntersectionDirection(InputDirection(Dir));
                            break;
                        case ControlTypes.Full:
                            Vector2 newDir = FullInputDirection();
                            if (CurrentNode.NextDirectionValid(newDir)) Dir = newDir;
                            break;
                        case ControlTypes.None:
                            Dir = CurrentNode.NextIntersectionDirection(Dir);
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
                            if (CurrentNode.HasSameDirection(newDir)) Dir = newDir;
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
        }

        public void OnEnter(BitrailNode startingNode, Player player)
        {
            if (startingNode == null || !startingNode.IsEntryPoint) return;
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
                    if (player.DashDir.X != 0 && (directions.Contains(Direction.Left) || directions.Contains(Direction.Right)))
                    {
                        Dir = Calc.Ceiling(Vector2.UnitX * player.DashDir.X);
                    }
                    else if (player.DashDir.Y != 0 && (directions.Contains(Direction.Up) || directions.Contains(Direction.Down)))
                    {
                        Dir = Calc.Ceiling(Vector2.UnitY * player.DashDir.Y);
                    }
                    break;
                case ControlTypes.Full:
                    Vector2 newDir = FullInputDirection();
                    if (CurrentNode.HasSameDirection(newDir)) Dir = newDir;
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
            _mask?.Dispose();
            _mask = null;
            _buffer?.Dispose();
            _buffer = null;
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
    }
}
