using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

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
        private List<LightsIcon> Icons = new List<LightsIcon>();
        private float rectWidth;
        private float rectHeight;
        private Level l;
        public bool SettingState;
        private bool SwitchState;
        private bool _state;
        public bool State
        {
            get
            {
                return _state;
            }
            set
            {
                SwitchState = true;
                _state = value;
            }
        }
        private float offset = 0;
        private float LineThickness = 1;
        private float Thickness = 1;
        private bool Growing = true;
        private bool AffectIcons;
        private Player player;
        private bool MoveObjects;
        public float Opacity = 1;

        private VirtualRenderTarget _StoolPickupMask;
        private VirtualRenderTarget StoolPickupMask => _StoolPickupMask ??= VirtualContent.CreateRenderTarget("StoolPickupMask", 320, 180);

        private VirtualRenderTarget _StoolPickupObject;
        private VirtualRenderTarget StoolPickupObject => _StoolPickupObject ??= VirtualContent.CreateRenderTarget("StoolPickupObject", 320, 180);
        public StoolPickupBarrier(Vector2 position, int width, int height, float lineThickness = 1, bool moveObjects = false, bool affectIcons = false, bool startState = true) : base(position)
        {
            MoveObjects = moveObjects;
            rectWidth = width;
            LineThickness = lineThickness;
            AffectIcons = affectIcons;
            rectHeight = height;
            Collider = new Hitbox(rectWidth, rectHeight);
            _state = startState;
            Opacity = startState ? 1 : 0;
            Add(new BeforeRenderHook(BeforeRender));
        }
        public StoolPickupBarrier(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Width, data.Height, data.Float("lineThickness", 1), data.Bool("moveObjectsToEdge"), data.Bool("affectIcons")) { }
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
        private void Drawing()
        {
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            float posX;

            for (int i = -2; i < 8; i++)
            {
                posX = Position.X + offset + Width / 8 * i;
                Draw.Line(new Vector2(posX, Position.Y), new Vector2(posX + Width / 8, Position.Y + Height + 4), Color.White * (Thickness + 0.3f) * Opacity, Thickness + LineThickness);
            }
        }
        private void MaskDrawing()
        {
            Draw.Rect(Collider, Color.White);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (AffectIcons)
            {
                foreach (LightsIcon icon in scene.Tracker.GetEntities<LightsIcon>())
                {
                    Icons.Add(icon);
                }
            }
            Stools = scene.Tracker.GetEntities<Stool>();
        }
        public override void Render()
        {
            base.Render();
            Draw.Rect(Collider, Color.Lerp(Color.DarkGreen, Color.Green, Thickness) * (Flashing ? Flash : 0.5f) * Opacity);
            EasyRendering.DrawToGameplay(StoolPickupObject, l, Color.Lerp(Color.Green, Color.LimeGreen, Thickness * (Flashing ? Flash : 1)) * Opacity);
            Draw.HollowRect(Collider, Color.Lerp(Color.DarkGreen, Warning ? Color.Red : Color.Green, Thickness) * (Flashing ? Flash + 0.2f : 0.7f) * Opacity);
        }
        /*        private IEnumerator Respawn(Stool stool)
                {
                    From _color = stool.GlassTexture.From;
                    stool.Respawning = true;
                    for(float i = 1; i >=0; i -= 0.1f)
                    {
                        stool.GlassTexture.From = _color * i;
                        yield return null;
                    }
                    stool.Offset = stool.orig_Pos;
                    yield return 0.02f;
                    stool.Respawned = true;
                    for (float i = 0; i < 1; i += 0.1f)
                    {
                        stool.GlassTexture.From = _color * i;
                        yield return null;
                    }
                    yield return null;
                }*/
        public override void Update()
        {
            base.Update();
            player = SceneAs<Level>().Tracker.GetEntity<Player>();
            if (player is null || !State || SettingState)
            {
                return;
            }
            if (AffectIcons)
            {
                foreach (LightsIcon icon in Icons)
                {
                    if (CollideCheck(icon) && icon.OnGround() && MoveObjects)
                    {
                        if (MathHelper.Distance(Left, icon.Center.X) < MathHelper.Distance(Right, icon.Center.X))
                        {
                            icon.MoveTowardsX(Position.X - icon.Width * 1.5f, 2);
                        }
                        else
                        {
                            icon.MoveTowardsX(Position.X + Width + icon.Width * 1.5f, 2);
                        }
                    }
                    if (CollideCheck(icon) && icon.Hold.IsHeld)
                    {
                        Flash = 1f;
                        Flashing = true;
                        icon.Hold.Holder.Drop();
                    }
                }
            }

            foreach (Stool stool in Stools)
            {
                if (stool.MoveStool && CollideCheck(stool) && stool.OnGround() && MoveObjects)
                {
                    if (MathHelper.Distance(Left, stool.Center.X) < MathHelper.Distance(Right, stool.Center.X))
                    {
                        stool.MoveTowardsX(Position.X - stool.Width * 1.5f, 2);
                    }
                    else
                    {
                        stool.MoveTowardsX(Position.X + Width + stool.Width * 1.5f, 2);
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

            Thickness += Growing ? Engine.DeltaTime * LineThickness : -Engine.DeltaTime * LineThickness;
            Growing = Growing ? Thickness < LineThickness : Thickness < 0.1f;
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