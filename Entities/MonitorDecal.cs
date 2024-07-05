using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/MonitorDecal")]
    [Tracked]
    public class MonitorDecal : Entity
    {
        public Sprite Border;
        public Sprite Monitor;
        public Sprite Buttons;
        public Sprite Rays;
        public Sprite CustomDecal;
        public string customDecalPath;
        private Level level;
        public Color ScreenColor;

        private bool Animated;
        private bool HasRays;
        public bool UsesDecal;
        private bool HasButtons;
        public bool ScaleDecal;
        private float LightDelay;

        public string GroupID;
        public bool FromController;
        private float CurrentLightOffset;
        private float LightOffset;
        private VirtualRenderTarget Target = VirtualContent.CreateRenderTarget("MonitorDecal", 320, 180);
        public MonitorDecal(EntityData data, Vector2 offset)
        : base(data.Position + offset)
        {
            customDecalPath = data.Attr("inScreenDecal");
            ScreenColor = data.HexColor("screenColor", Color.Black);
            Animated = data.Bool("onLight");
            HasRays = data.Bool("hasRays");
            UsesDecal = !string.IsNullOrEmpty(customDecalPath);
            HasButtons = data.Bool("buttons");
            ScaleDecal = data.Bool("scaleDecal");
            LightDelay = data.Float("lightDelay", 1);
            LightOffset = data.Float("lightStartOffset", 0);
            string path = "objects/PuzzleIslandHelper/machines/gizmos/";
            Border = new Sprite(GFX.Game, path);
            Monitor = new Sprite(GFX.Game, path);
            Buttons = new Sprite(GFX.Game, path);
            Rays = new Sprite(GFX.Game, path);

            Border.AddLoop("idle", "screen", LightDelay, Animated ? new int[] { 0, 1 } : new int[] { 0 });
            Border.AddLoop("delay", "screen", 0.1f, 1);
            if (Animated)
            {
                Border.OnLastFrame = (string s) =>
                {
                    if (s == "delay")
                    {
                        CurrentLightOffset += Engine.DeltaTime;
                        if (CurrentLightOffset >= LightOffset)
                        {
                            Border.Play("idle");
                        }
                    }
                };
                Border.Play("delay");
            }
            else
            {
                Border.Play("idle");
            }
            //Border.PlayEvent("idle");
            Border.Visible = false;
            Add(Border);
            Monitor.AddLoop("idle", "inside", 0.1f);
            Monitor.Play("idle");
            Monitor.Visible = false;
            Add(Monitor);
            Collider = new Hitbox(Monitor.Width, Monitor.Height);
            if (HasButtons)
            {
                Buttons.AddLoop("idle", "screenButtons", 0.1f);
                Buttons.Play("idle");
                Add(Buttons);
                Buttons.Visible = false;
            }
            if (UsesDecal)
            {
                CustomDecal = new Sprite(GFX.Game, customDecalPath);
                CustomDecal.AddLoop("idle", "", 0.1f);
                CustomDecal.Play("idle");
                CustomDecal.Visible = false;
                if (ScaleDecal)
                {
                    int width = 20;
                    int height = 12;
                    CustomDecal.Scale = new Vector2(width / CustomDecal.Width, height / CustomDecal.Height);
                    CustomDecal.Position += new Vector2((CustomDecal.Width - width) / 2, (CustomDecal.Height - height) / 2);

                }
                Add(CustomDecal);
            }
            if (HasRays)
            {
                Rays.AddLoop("idle", "screenRays", 0.1f);
                Rays.Play("idle");
                Rays.Color = Color.White * 0.5f;
                Add(Rays);
                Rays.Visible = false;
            }

            Depth = 9001;
            Add(new BeforeRenderHook(BeforeRender));

            GroupID = data.Attr("groupId");
        }
        private void BeforeRender()
        {
            if (!FromController)
            {
                Target.DrawThenMask(Monitor, (Action)DrawScreen, level.Camera.Matrix);
            }
        }
        private void DrawScreen()
        {
            Draw.Rect(Collider, ScreenColor);
            if (UsesDecal)
            {
                CustomDecal.Render();
            }

        }
        public void RenderInside()
        {
            Draw.SpriteBatch.Draw(Target, level.Camera.Position, Color.White);
        }
        public override void Render()
        {
            base.Render();
            if (!FromController)
            {
                RenderInside();
                Rays.Render();
                Border.Render();
                Buttons.Render();
            }
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
        }
    }
}