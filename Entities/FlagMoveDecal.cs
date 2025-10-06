using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/FlagMoveDecal")]
    [Tracked]
    public class FlagMoveDecal : Entity
    {
        public Sprite sprite;
        public Image image;
        public bool Outline;
        public FlagList RenderingEnabled;
        public FlagList PositionFlag;
        public Vector2 Orig, Node;
        private VertexLight light;
        private Vector2 lightOffset;
        private float moveSpeed;

        public FlagMoveDecal(EntityData data, Vector2 offset)
        : base(data.Position + offset)
        {
            string path = data.Attr("path");
            PositionFlag = data.FlagList("flags");
            Orig = Position;
            Node = data.NodesOffset(offset)[0];
            Outline = data.Bool("outline");
            Depth = data.Int("depth", 2);
            lightOffset = new Vector2(data.Float("lightX"), data.Float("lightY"));
            Collider = new Hitbox(1, 1);
            moveSpeed = data.Float("speed", 40f);
            if (!string.IsNullOrEmpty(path))
            {
                sprite = new Sprite(GFX.Game, "decals/");
                sprite.AddLoop("idle", path, 0.1f);
                sprite.Color = data.HexColor("color");
                sprite.Play("idle");
                if (data.Bool("centerLight", true))
                {
                    lightOffset += sprite.HalfSize();
                }
                sprite.CenterOrigin();
                sprite.Position += sprite.HalfSize();
                sprite.Rotation = data.Float("rotation").ToRad();
                sprite.Scale = new Vector2(data.Float("scaleX", 1), data.Float("scaleY", 1));
                Add(sprite);
                Collider = new Hitbox(sprite.Width, sprite.Height);
            }
            if (data.Bool("useLight"))
            {
                light = new VertexLight(lightOffset, data.HexColor("lightColor", Color.White), data.Float("lightAlpha", 1), data.Int("lightStartFade", 32), data.Int("lightEndFade", 64));
                Add(light);
            }
            Tag |= Tags.TransitionUpdate;
            Add(new BlockerComponent());

        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (sprite != null)
            {
                Collider = new Hitbox(sprite.Width, sprite.Height);
            }
            if (PositionFlag)
            {
                Position = Node;
            }
        }
        private bool shaking;
        private float shakeTimer;
        private Vector2 shakeAmount;
        public override void Update()
        {
            base.Update();
            if (PositionFlag)
            {
                if (Position != Node)
                {
                    StartShaking();
                    Position = Calc.Approach(Position, Node, moveSpeed * Engine.DeltaTime);
                }
                else
                {
                    StopShaking();
                }
            }
            else
            {
                if (Position != Orig)
                {
                    StartShaking();
                    Position = Calc.Approach(Position, Orig, moveSpeed * Engine.DeltaTime);
                }
                else
                {
                    StopShaking();
                }
            }
            if (!shaking)
            {
                return;
            }
            if (Scene.OnInterval(0.04f))
            {
                Vector2 vector = shakeAmount;
                shakeAmount = Calc.Random.ShakeVector();
                OnShake(shakeAmount - vector);
            }
            if (shakeTimer > 0f)
            {
                shakeTimer -= Engine.DeltaTime;
                if (shakeTimer <= 0f)
                {
                    shaking = false;
                    StopShaking();
                }
            }
        }

        public void StartShaking(float time = 0f)
        {
            shaking = true;
            shakeTimer = time;
        }
        public void StopShaking()
        {
            shaking = false;
            if (shakeAmount != Vector2.Zero)
            {
                OnShake(-shakeAmount);
                shakeAmount = Vector2.Zero;
            }
        }

        public void OnShake(Vector2 amount)
        {
            if (sprite != null)
            {
                sprite.Position += amount;
            }
            if (light != null)
            {
                light.Position += amount;
            }
        }
        public override void Render()
        {
            if (!RenderingEnabled) return;
            if (Outline)
            {
                if (sprite != null && sprite.Visible)
                {
                    sprite.DrawSimpleOutline();
                }
            }
            base.Render();

        }
    }
}
