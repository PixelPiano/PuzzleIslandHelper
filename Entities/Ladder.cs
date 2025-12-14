using Celeste.Mod.Entities;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static MonoMod.InlineRT.MonoModRule;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/Ladder")]
    [Tracked]
    public class Ladder : Actor
    {
        [Tracked]
        public class Step : JumpThru
        {
            public Sprite Sprite;
            public Ladder Parent;
            public float CollidableTimer;
            public bool ForceCollidable;
            public Vector2 Offset;
            public bool Extending;
            public bool Extended;
            public FlagList ExtendFlag;
            public Step(Ladder parent, string path, Vector2 position, int width, bool safe, string flag = "") : base(parent.Position + position, width, safe)
            {
                ExtendFlag = new FlagList(flag);
                Offset = position;
                Parent = parent;
                Sprite = new Sprite(GFX.Game, path);
                Sprite.AddLoop("idle", "ladder", 0.1f);
                Sprite.Add("extend", "ladderExtend", 0.05f, "idle");
                Sprite.AddLoop("hide", "reallyBadLadder", 0.1f);
                Sprite.Y -= 3;
                Add(Sprite);
                Sprite.Play("idle");
                Sprite.Origin = new Vector2(Sprite.Width / 2, Sprite.Height);
                Sprite.Position += Sprite.Origin;
                Visible = false;
                Tag |= Tags.TransitionUpdate;
            }
            public void UpdatePosition()
            {
                if (Position != Parent.Position + Offset)
                {
                    MoveTo(Parent.Position + Offset);
                }
            }
            public override void Update()
            {
                base.Update();
                if (CollidableTimer > 0)
                {
                    CollidableTimer -= Engine.DeltaTime;
                }
                Sprite.SetColor(Color.White * Parent.Alpha);
            }
            public void DrawOutline()
            {
                Sprite.DrawSimpleOutline();
            }
            public IEnumerator ExtendRoutine()
            {
                Extending = true;
                Sprite.Play("extend");
                while (Sprite.CurrentAnimationID != "idle")
                {
                    yield return null;
                }
                Extended = true;
                Extending = false;

            }
            public void Extend(bool instant = false)
            {
                if (instant)
                {
                    Extended = true;
                    Extending = false;
                    Sprite.Play("idle");
                    Collidable = true;
                }
                else
                {
                    Add(new Coroutine(ExtendRoutine()));
                }
            }
        }
        public bool Extending;
        public bool Extended;
        public void Extend(bool instant)
        {
            if (instant)
            {
                foreach (Step step in rungs)
                {
                    step.Extend(true);
                }
                Extended = true;
                Extending = false;
            }
            else
            {
                Extending = true;
                Add(new Coroutine(extendRoutine()));
            }
        }
        private IEnumerator extendRoutine()
        {
            foreach (Step step in rungs)
            {
                step.Sprite.Play("hide");
                step.Collidable = false;
            }
            foreach (Step step in rungs.Reverse<Step>())
            {
                yield return new SwapImmediately(step.ExtendRoutine());
                step.Collidable = true;
            }
            Extended = true;
            Extending = false;
        }
        public bool TryingToClimb => Math.Abs(Input.MoveX.Value) < 0.5f && Input.MoveY.Value == -1 && (climbTimer <= 0 || prevMoveY > -1);
        public List<Step> rungs = [];
        public Step TopRung;
        private bool stepsCollidable = true;
        private bool playerColliding;
        public float Alpha = 1;
        public string TexturePath;
        private float climbTimer;
        private FlagList Suspended;
        private FlagList ExtendFlag;
        public float YSpeed = 20;
        private int prevMoveY = 0;
        public bool CollidableWhileHoldingDown = false;
        public Ladder(EntityData data, Vector2 offset) : this(data.Position + offset, data.Height, data.Attr("texture", "objects/PuzzleIslandHelper/ladder"), data.Bool("visible"), data.Int("depth"), data.FlagList("suspendedFlags"), data.FlagList("extendFlag")) { }
        public Ladder(Vector2 position, int height, string texture, bool visible, int depth, FlagList suspendedFlags, FlagList extendFlag) : base(position)
        {
            ExtendFlag = extendFlag;
            IgnoreJumpThrus = true;
            Suspended = suspendedFlags;
            Collider = new Hitbox(16, height);
            Depth = depth;
            Visible = visible;
            TexturePath = texture;
            Tag |= Tags.TransitionUpdate;
            TransitionListener listener = new();
            listener.OnInBegin = () =>
            {
                if (!Suspended.Empty && !Suspended)
                {
                    SnapDown();
                }
            };
            Add(listener);
            Add(new PlayerCollider(p =>
            {
                playerColliding = true;
            }));
        }
        public void FreezeCheck(Level level)
        {
            foreach (Step step in rungs)
            {
                if (step.Y - level.Bounds.Top < 16)
                {
                    step.Collidable = false;
                    step.CollidableTimer = 30 * Engine.DeltaTime;
                }
            }
        }
        public void UpdatePositions()
        {
            foreach (var rung in rungs)
            {
                rung.UpdatePosition();
            }
        }
        public void SnapDown()
        {
            while (!CollideCheck<Platform, Step>(Position + Vector2.UnitY))
            {
                Y++;
                UpdatePositions();
                if (SceneAs<Level>().Bounds.Bottom < Top)
                {
                    RemoveSelf();
                    break;
                }
            }
        }
        public void OnCollideV(CollisionData data)
        {
            YSpeed *= -0.4f;
            if (Math.Abs(YSpeed) < 20f && data.Direction == Vector2.UnitY)
            {
                YSpeed = 0;
                UpdatePositions();
            }
        }
        public TowerHead TowerHead;
        public override void Update()
        {
            bool playerWasColliding = playerColliding;
            playerColliding = false;
            base.Update();
            if (Scene is not Level level || level.GetPlayer() is not Player player) return;
            if (ExtendFlag && !Extended && !Extending)
            {
                Extend(false);
            }
            if (!Suspended.Empty && !Suspended)
            {
                YSpeed = Calc.Approach(YSpeed, 300f, 900f * Engine.DeltaTime);
                MoveV(YSpeed * Engine.DeltaTime, OnCollideV);
            }
            UpdatePositions();

            bool playerInTower = false;
            if (TowerHead != null)
            {
                playerInTower = TowerHead.PlayerInside;
            }
            bool inputAimedDown = !CollidableWhileHoldingDown && Input.MoveY.Value > 0.5f && Math.Abs(Input.MoveX.Value) < 0.5f;
            if (playerColliding)
            {
                if (!stepsCollidable && (CollidableWhileHoldingDown || Input.MoveY.Value <= -1 || Input.Jump.Pressed || player.CollideCheck<Platform, Step>(player.Position + Vector2.UnitY)))
                {
                    stepsCollidable = true;
                }
            }
            else if (playerWasColliding)
            {
                stepsCollidable = true;
            }
            if (climbTimer > 0)
            {
                climbTimer -= Engine.DeltaTime;
            }
            bool c = Collidable && !playerInTower;
            bool extendFlag = ExtendFlag;
            foreach (var item in rungs)
            {
                item.Collidable = (extendFlag && (Extended || item.Extended)) && c && item.CollidableTimer <= 0 && !(inputAimedDown || !stepsCollidable) || item.ForceCollidable;
            }
            if (!inputAimedDown && c)
            {
                ClimbCheck(player);
            }
            prevMoveY = Input.MoveY.Value;
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (!Suspended.Empty && !Suspended)
            {
                SnapDown();
            }
            FreezeCheck(scene as Level);
            TowerHead = scene.Tracker.GetEntity<TowerHead>();
            if (ExtendFlag)
            {
                Extended = true;
                Extend(true);
            }
            else
            {
                foreach (Step s in rungs)
                {
                    s.Sprite.Play("hide");
                    s.Collidable = false;
                }
            }
        }
        public bool PlayerOnGroundOrNonStepJumpThru(Player player)
        {
            bool prev = player.IgnoreJumpThrus;
            player.IgnoreJumpThrus = true;
            bool onGround = player.OnGround();
            player.IgnoreJumpThrus = prev;
            if (onGround) return true;
            List<Entity> list = Scene.Tracker.Entities[typeof(Step)];
            foreach (Entity item in Scene.Tracker.Entities[typeof(JumpThru)])
            {
                if (!list.Contains(item) && !Collide.Check(player, item) && Collide.Check(player, item, player.Position + Vector2.UnitY))
                {
                    return true;
                }
            }
            return false;
        }
        public void ClimbCheck(Player player)
        {
            bool canClimb = TryingToClimb;
            Step prev = null;
            bool prevIgnore = player.IgnoreJumpThrus;
            player.IgnoreJumpThrus = true;
            bool onGround = PlayerOnGroundOrNonStepJumpThru(player);
            player.IgnoreJumpThrus = prevIgnore;
            if (canClimb)
            {
                if (onGround)
                {
                    Step closestTo = null;
                    float dist = float.MaxValue;
                    foreach (var item in rungs)
                    {
                        if (player.CollideCheck(item))
                        {
                            float d = MathHelper.Distance(item.Top, player.Bottom);
                            if (item.Top < player.Bottom && dist > d)
                            {
                                closestTo = item;
                                dist = d;
                            }

                        }
                    }
                    if (closestTo != null)
                    {
                        player.MoveToY(closestTo.Y);
                        climbTimer = Engine.DeltaTime * 5f;
                        return;
                    }
                }
                else
                {
                    foreach (var item in rungs)
                    {
                        if (item.Collidable && item.HasPlayerRider() && prev != null && prev.Collidable)
                        {
                            player.MoveToY(prev.Y);
                            climbTimer = Engine.DeltaTime * 5f;
                            return;
                        }
                        prev = item;
                    }
                }

                if (TopRung != null && TopRung.HasPlayerRider())
                {
                    foreach (Platform platform in SceneAs<Level>().Tracker.GetEntities<Platform>())
                    {
                        if (platform is not Solid && (platform is not Step || !rungs.Contains(platform)))
                        {
                            if (player.Bottom > platform.Top && player.CollideCheck(platform))
                            {
                                player.MoveToY(platform.Top);
                                return;
                            }
                        }
                    }
                }
            }
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Rectangle bounds = (scene as Level).Bounds;
            for (int i = 0; i < Height; i += 8)
            {
                float y = Y + (i + 3);
                if (y > bounds.Bottom) break;
                if (y < bounds.Top) continue;
                Step rung = new Step(this, "objects/PuzzleIslandHelper/", Vector2.UnitY * (i + 3), 16, true);
                if (TopRung == null)
                {
                    TopRung = rung;
                }
                rungs.Add(rung);
                scene.Add(rung);
            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            foreach (var p in rungs)
            {
                p.RemoveSelf();
            }
        }
        public override void Render()
        {
            foreach (var p in rungs)
            {
                p.DrawOutline();
            }
            base.Render();
            foreach (var p in rungs)
            {
                p.Render();
            }
        }
    }
}