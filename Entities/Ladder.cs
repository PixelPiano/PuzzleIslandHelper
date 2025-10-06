using Celeste.Mod.Entities;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;
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
            /*            [OnLoad]
                        public static void Load()
                        {
                            IL.Celeste.Actor.MoveVExact += Actor_MoveVExact;
                        }
                        [OnUnload]
                        public static void Unload()
                        {
                            IL.Celeste.Actor.MoveVExact -= Actor_MoveVExact;
                        }
                        private static void Actor_MoveVExact(ILContext il)
                        {
                            ILCursor cursor = new ILCursor(il);
                            bool findNext()
                            {
                                return cursor.TryGotoNext(MoveType.After,
                                    item => item.Match(OpCodes.Brfalse_S),
                                    item => item.MatchLdarg0(),
                                    item => item.MatchLdfld<Vector2>("movementCounter"));
                            }
                            if (findNext() && findNext())
                            {
                                Engine.Commands.Log("GOT THROUGH 1");
                                if (cursor.TryGotoPrev(MoveType.After, item => item.MatchLdloc3()))
                                {
                                    Engine.Commands.Log("GOT THROUGH 2");
                                    cursor.EmitLdloc3();
                                    cursor.EmitLdarg0();
                                    cursor.EmitDelegate(CollideWithActor);
                                    cursor.EmitAnd();
                                }
                            }
                        }
                        public static bool CollideWithActor(JumpThru jumpThru, Actor actor)
                        {
                            if(jumpThru is Step step && actor is Player player)
                            {
                                if(step.LetPlayerThrough) return false;
                            }
                            return true;
                        }*/
            public Image Image;
            public Ladder Parent;
            public float CollidableTimer;
            public bool ForceCollidable;
            public Vector2 Offset;
            public Step(Ladder parent, string path, Vector2 position, int width, bool safe) : base(parent.Position + position, width, safe)
            {
                Offset = position;
                Parent = parent;
                Image = new Image(GFX.Game[path]);
                Image.Y -= 3;
                Add(Image);
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
                Image.SetColor(Color.White * Parent.Alpha);
            }
            public void DrawOutline()
            {
                Image.DrawSimpleOutline();
            }
        }
        public List<Step> rungs = [];
        private bool stepsCollidable = true;
        private bool playerColliding;
        public float Alpha = 1;
        public string TexturePath;
        private float climbTimer;
        private FlagList Suspended;
        public float YSpeed = 20;
        private int prevMoveY = 0;
        public Ladder(EntityData data, Vector2 offset) : this(data.Position + offset, data.Height, data.Attr("texture", "objects/PuzzleIslandHelper/ladder"), data.Bool("visible"), data.Int("depth"), data.FlagList("suspendedFlags")) { }
        public Ladder(Vector2 position, int height, string texture, bool visible, int depth, FlagList suspendedFlags) : base(position)
        {
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
        public override void Update()
        {
            base.Update();
            if (Scene is not Level level || level.GetPlayer() is not Player player) return;
            if (!Suspended.Empty && !Suspended)
            {
                YSpeed = Calc.Approach(YSpeed, 300f, 900f * Engine.DeltaTime);
                MoveV(YSpeed * Engine.DeltaTime, OnCollideV);
            }
            UpdatePositions();
            bool playerWasColliding = playerColliding;
            playerColliding = player.CollideCheck(this);
            bool playerInTower = CollideFirst<TowerHead>() is TowerHead head && head.PlayerInside;
            bool inputAimedDown = Input.MoveY.Value > 0.5f && Math.Abs(Input.MoveX.Value) < 0.5f;
            if (playerColliding)
            {
                if (!stepsCollidable && (Input.MoveY.Value <= -1 || Input.Jump.Pressed || player.CollideCheck<Platform, Step>(player.Position + Vector2.UnitY)))
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
            foreach (var item in rungs)
            {
                item.Collidable = c && item.CollidableTimer <= 0 && !(inputAimedDown || !stepsCollidable) || item.ForceCollidable;
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
            bool canClimb = Math.Abs(Input.MoveX.Value) < 0.5f && Input.MoveY.Value == -1 && (climbTimer <= 0 || prevMoveY > -1);
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
                            break;
                        }
                        prev = item;
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
                Step rung = new Step(this, TexturePath, Vector2.UnitY * (i + 3), 16, true);
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