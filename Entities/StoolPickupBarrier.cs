using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Configuration;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/StoolPickupBarrier")]
    [Tracked]
    public class StoolPickupBarrier : Entity
    {
        private bool Warning = false;
        private float Flash = 0;
        private bool Flashing = false;
        private List<Entity> Stools = new List<Entity>();
        private float rectWidth;
        private float rectHeight;
        private Level l;
        private float offset = 0;
        private float Thickness = 1;
        private bool Growing = true;
        private Player player;

        private VirtualRenderTarget _StoolPickupMask;
        private VirtualRenderTarget StoolPickupMask => _StoolPickupMask ??= VirtualContent.CreateRenderTarget("StoolPickupMask", 320, 180);

        private VirtualRenderTarget _StoolPickupObject;
        private VirtualRenderTarget StoolPickupObject => _StoolPickupObject ??= VirtualContent.CreateRenderTarget("StoolPickupObject", 320, 180);

        public StoolPickupBarrier(EntityData data, Vector2 offset)
        : base(data.Position + offset)
        {
            rectWidth = data.Width;
            rectHeight = data.Height;
            Collider = new Hitbox(rectWidth, rectHeight);
            Add(new BeforeRenderHook(BeforeRender));
        }
        private void BeforeRender()
        {
            if (Scene as Level is null)
            {
                return;
            }
            l = Scene as Level;
            EasyRendering.SetRenderMask(StoolPickupMask, MaskDrawing, l);
            EasyRendering.DrawToObject(StoolPickupObject, Drawing, l);
            EasyRendering.MaskToObject(StoolPickupObject, StoolPickupMask);

        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Drawing();
        }
        private Vector2 ToInt(Vector2 vector)
        {
            return new Vector2((int)vector.X, (int)vector.Y);
        }
        private void Drawing()
        {
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            float posX;

            for (int i = -2; i < 8; i++)
            {
                posX = Position.X + offset + (Width / 8 * i);
                Draw.Line(new Vector2(posX, Position.Y), new Vector2(posX + (Width / 8), Position.Y + Height + 4), Color.White * (Thickness + 0.3f), Thickness + 1);
            }
        }
        private void MaskDrawing()
        {
            Draw.Rect(Collider, Color.White);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Stools = scene.Tracker.GetEntities<Stool>();
        }
        public override void Render()
        {
            base.Render();
            Draw.Rect(Collider, Color.Lerp(Color.DarkGreen, Color.Green, Thickness) * (Flashing ? Flash : 0.5f));
            EasyRendering.DrawToGameplay(StoolPickupObject, l, Color.Lerp(Color.Green, Color.LimeGreen, Thickness * (Flashing ? Flash : 1)));
            Draw.HollowRect(Collider, Color.Lerp(Color.DarkGreen, Warning? Color.Red : Color.Green, Thickness) * (Flashing ? Flash + 0.2f : 0.7f));
        }
/*        private IEnumerator Respawn(Stool stool)
        {
            Color _color = stool.sprite.Color;
            stool.Respawning = true;
            for(float i = 1; i >=0; i -= 0.1f)
            {
                stool.sprite.Color = _color * i;
                yield return null;
            }
            stool.Position = stool.orig_Pos;
            yield return 0.02f;
            stool.Respawned = true;
            for (float i = 0; i < 1; i += 0.1f)
            {
                stool.sprite.Color = _color * i;
                yield return null;
            }
            yield return null;
        }*/
        public override void Update()
        {
            base.Update();
            player = SceneAs<Level>().Tracker.GetEntity<Player>();
            if(player is null)
            {
                return;
            }
            foreach (Stool stool in Stools)
            {
                if (stool.MoveStool && CollideCheck(stool) && stool.OnGround())
                { 
                    if(MathHelper.Distance(Left,stool.Center.X) < MathHelper.Distance(Right, stool.Center.X))
                    {
                        stool.MoveTowardsX(Position.X - (stool.Width * 1.5f), 2);
                    }
                    else
                    {
                        stool.MoveTowardsX(Position.X+Width + (stool.Width * 1.5f), 2);
                    }
                }

                if (CollideCheck(stool) && stool.Hold.IsHeld)
                {
                    Flash = 1f;
                    Flashing = true;
                    stool.Hold.Holder.Drop();
                    stool.MoveStool = true;
                }
            }
            Thickness += Growing ? Engine.DeltaTime : -Engine.DeltaTime;
            Growing = Growing ? Thickness < 1 : Thickness < 0.1f;
            offset += Engine.DeltaTime * 10;
            offset %= Width / 8;
            if (Flashing)
            {
                Flash = Calc.Approach(Flash, 0f, Engine.DeltaTime * 4f);
                if (Flash <= 0f)
                {
                    Flashing = false;
                }
            }
        }
    }
}