using Celeste.Mod.Entities;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
// PuzzleIslandHelper.DigitalTransport
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/DigitalTransport")]
    [Tracked]
    public class DigitalTransport : Entity
    {
        private readonly string Room;
        private bool HubTeleport;
        private string ID;
        private EntityID myID;
        private Level level;
        private Player player;
        private readonly Sprite sprite;
        public static bool Transitioning;
        public bool WireTime;
        private List<Sprite> Wires = new();
        private readonly TalkComponent talk;
        private float Thickness = 4;
        private float GreenY;
        private float ColorMod;
        private float WireEnd;
        public bool Receiving;

        public BloomPoint Bloom;
        private DigitalTransport Target;
        private readonly VirtualRenderTarget Content = VirtualContent.CreateRenderTarget("WireTravel", 320, 180);
        private readonly VirtualRenderTarget Mask = VirtualContent.CreateRenderTarget("WireTravelMask", 320, 180);
        public DigitalTransport(EntityData data, Vector2 offset, EntityID id)
        : base(data.Position + offset)
        {
            myID = id;
            ID = data.Attr("targetId");
            HubTeleport = data.Bool("toHub");
            sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/transport/");
            sprite.AddLoop("idle", "outlet", 1f);

            Collider = new Hitbox(sprite.Width, sprite.Height);
            Room = data.Attr("roomName");
            Depth = 1;
            sprite.Play("idle");
            Add(talk = new TalkComponent(new Rectangle(0, 0, (int)Width, (int)Height * 3), Vector2.UnitX * Width / 2, Interact));
            Add(new BeforeRenderHook(BeforeRender));
        }

        public override void Update()
        {
            base.Update();
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
            Add(Bloom = new BloomPoint(1,30));
            
            Vector2 start = Position + Vector2.UnitX * sprite.Width;
            Vector2? EndPos = DoRaycast(level.SolidTiles.Grid, start, new Vector2(start.X, level.Bounds.Top));

            float xOffset = 0;
            WireEnd = EndPos.HasValue ? EndPos.Value.Y : level.Bounds.Top;
            int Range = (int)MathHelper.Distance(WireEnd, Position.Y);
            for (int i = 6; i < Range; i += (int)Wires[0].Height)
            {
                Sprite Wire = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/transport/");
                Wire.AddLoop("idle", "wire", 1f);
                Wire.Position.Y -= i;
                Wire.Position.X += xOffset;
                Wires.Add(Wire);
                Wire.Play("idle");
            }
            Add(Wires.ToArray());
            Add(sprite);
        }
        private IEnumerator Cutscene(Player player)
        {
            AddTag(Tags.Global);
            this.player = player;
            player.StateMachine.State = 11;
            Transitioning = true;
            //Animation of wire green glow thingy
            player.Visible = false;
            yield return new SwapImmediately(WireTravel(true, player));

            new FallWipe(SceneAs<Level>(), false, OnComplete)
            {
                Duration = 0.6f,
                EndTimer = 0.7f
            };


            yield return null;
        }
        private void Interact(Player player)
        {
            Add(new Coroutine(Cutscene(player)));
        }
        private void OnComplete()
        {
            bool wasNotInvincible = false;

            if (!SaveData.Instance.Assists.Invincible)
            {
                wasNotInvincible = true;
                SaveData.Instance.Assists.Invincible = true;
            }
            Level level = SceneAs<Level>();

            Add(new Coroutine(TeleportPlayer(player, wasNotInvincible, level.Camera)));
            new MountainWipe(SceneAs<Level>(), true, End)
            {
                Duration = 1f,
                EndTimer = 0.1f
            };
        }

        private void Drawing()
        {
            Vector2 start = new Vector2(Position.X, GreenY);
            Draw.Line(start, start + Vector2.UnitX * sprite.Width, Color.Lerp(Color.Green, Color.White, ColorMod), Thickness);
            Vector2 middle = start;
            Draw.Line(middle, middle + Vector2.UnitX * sprite.Width, Color.Lerp(Color.LightGreen, Color.White, ColorMod), Thickness / 2);
        }
        private void DrawWires()
        {
            foreach (Sprite s in Wires)
            {
                s.Render();
            }
        }
        private void BeforeRender()
        {
            if (WireTime)
            {
                EasyRendering.SetRenderMask(Mask, DrawWires, level);
                EasyRendering.DrawToObject(Content, Drawing, level, true);
                EasyRendering.MaskToObject(Content, Mask);
            }
        }
        public override void Render()
        {
            base.Render();
            if (WireTime)
            {
                Draw.SpriteBatch.Draw(Content, level.Camera.Position, Color.White);
            }
            //Drawing();

        }
        public IEnumerator WireTravel(bool Entering, Player player)
        {
            float maxHeight = Wires.Count * Wires[0].Height / 2;
            float target;
            float targetThick;
            if (Entering)
            {
                Thickness = 4;
                GreenY = Position.Y + 8;
                target = WireEnd - maxHeight;
                targetThick = maxHeight;
            }
            else
            {
                Thickness = maxHeight;
                GreenY = WireEnd - maxHeight/2;
                target = Position.Y + 4;
                targetThick = 4;
            }
            float origThick = Thickness;
            float orig = GreenY;
            WireTime = true;

            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                GreenY = Calc.LerpClamp(orig, target, Ease.QuintIn(i));
                Thickness = Calc.LerpClamp(origThick, targetThick, Ease.QuintIn(i));
                yield return null;
            }

            WireTime = false;
            if (!Entering)
            {
                player.Visible = true;
                Transitioning = false;
                player.StateMachine.State = 0;
            }
            GreenY = target;
            Thickness = targetThick;

            yield return null;
        }
        private IEnumerator TeleportPlayer(Player player, bool wasNotInvincible, Camera camera)
        {
            TeleportTo(SceneAs<Level>(), player, HubTeleport ? "digitalTeleportHub" : Room);
            yield return null;
            level = SceneAs<Level>();
            foreach (DigitalTransport target in level.Tracker.GetEntities<DigitalTransport>())
            {
                if (ID == target.ID && !string.IsNullOrEmpty(ID) && !string.IsNullOrEmpty(target.ID) && target.myID.ID != myID.ID)
                {
                    Target = target;
                    player.Position = target.Position;

                    SetOnGround(player);
                    camera.Position = player.CameraTarget;

                    break;
                }
            }

            if (wasNotInvincible)
            {
                SaveData.Instance.Assists.Invincible = false;
            }
            yield return null;

        }
        private IEnumerator EndingRoutine()
        {
            if (Target is not null)
            {
                Target.Add(new Coroutine(Target.WireTravel(false, player)));
            }
            RemoveTag(Tags.Global);
            RemoveSelf();
            yield return null;
        }
        private void SetOnGround(Entity entity)
        {

            if (Scene as Level is not null)
            {
                try
                {
                    while (!entity.CollideCheck<SolidTiles>())
                    {
                        entity.Position.Y += 8;
                    }
                    while (entity.CollideCheck<SolidTiles>())
                    {
                        entity.Position.Y -= 1;
                    }

                }
                catch
                {
                    Console.WriteLine($"{entity} could not find any SolidTiles below it to set it's Y Position to");
                }
                entity.Position.Y -= 1;
            }
        }
        private void End()
        {
            Add(new Coroutine(EndingRoutine()));
        }

        public static void TeleportTo(Scene scene, Player player, string room, Player.IntroTypes introType = Player.IntroTypes.Transition, Vector2? nearestSpawn = null)
        {
            Level level = scene as Level;
            if (level != null)
            {
                level.OnEndOfFrame += delegate
                {
                    level.TeleportTo(player, room, introType, nearestSpawn);
                };
            }
        }
        public static Vector2? DoRaycast(Grid grid, Vector2 start, Vector2 end)
        {

            start = (start - grid.AbsolutePosition) / new Vector2(grid.CellWidth, grid.CellHeight);
            end = (end - grid.AbsolutePosition) / new Vector2(grid.CellWidth, grid.CellHeight);
            Vector2 dir = Vector2.Normalize(end - start);
            int xDir = Math.Sign(end.X - start.X), yDir = Math.Sign(end.Y - start.Y);
            if (xDir == 0 && yDir == 0) return null;
            int gridX = (int)start.X, gridY = (int)start.Y;
            float nextX = xDir < 0 ? (float)Math.Ceiling(start.X) - 1 : xDir > 0 ? (float)Math.Floor(start.X) + 1 : float.PositiveInfinity;
            float nextY = yDir < 0 ? (float)Math.Ceiling(start.Y) - 1 : yDir > 0 ? (float)Math.Floor(start.Y) + 1 : float.PositiveInfinity;
            while (Math.Sign(end.X - start.X) != -xDir || Math.Sign(end.Y - start.Y) != -yDir)
            {
                if (grid[gridX, gridY])
                {
                    return grid.AbsolutePosition + start * new Vector2(grid.CellWidth, grid.CellHeight);
                }
                if (Math.Abs((nextX - start.X) * dir.Y) < Math.Abs((nextY - start.Y) * dir.X))
                {
                    start.Y += Math.Abs((nextX - start.X) / dir.X) * dir.Y;
                    start.X = nextX;
                    nextX += xDir;
                    gridX += xDir;
                }
                else
                {
                    start.X += Math.Abs((nextY - start.Y) / dir.Y) * dir.X;
                    start.Y = nextY;
                    nextY += yDir;
                    gridY += yDir;
                }
            }
            return null;
        }
    }
}