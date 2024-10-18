using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Cutscenes;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.WIP
{
    [CustomEntity("PuzzleIslandHelper/WipEntity")]
    [Tracked]
    public class MemoryGridBox : Entity
    {
        private enum Sides
        {
            Right, Bottom, Left, Top
        }

        [TrackedAs(typeof(Image))]
        private class BoxSide : Image
        {
            private float lerpAmount;
            private Player player;
            private float angle;
            private Vector2 offset;
            public BoxSide(Sides side, float size) : base(null, true)
            {
                Texture = GFX.Game["objects/PuzzleIslandHelper/memoryGridBlock/" + side.ToString().ToLower()];
                angle = (int)side * MathHelper.PiOver2;
                offset = side switch
                {
                    Sides.Right => new Vector2(size, -Texture.Width),
                    Sides.Left => new Vector2(-Texture.Width, -Texture.Width),
                    Sides.Top => new Vector2(-Texture.Height, -Texture.Height),
                    Sides.Bottom => new Vector2(-Texture.Height, size),
                    _ => Vector2.Zero
                };
            }
            public override void EntityAwake()
            {
                base.EntityAwake();
                player = Scene.GetPlayer();
            }
            public override void Update()
            {
                base.Update();
                float playerAngle = (player.Center - (RenderPosition + this.HalfSize())).Angle();
                lerpAmount = Calc.LerpClamp(0.2f, 0.6f, Calc.AbsAngleDiff(angle, playerAngle) / (MathHelper.PiOver4 * 2f));
            }
            public override void Render()
            {
                Color prev = Color;
                Color = Color.Lerp(prev, Color.Black, lerpAmount);
                Position += offset;
                base.Render();
                Position -= offset;
                Color = prev;
            }
        }
        private List<BoxSide> BoxSides = new();
        public static MTexture NoiseTexture => GFX.Game["objects/PuzzleIslandHelper/noise"];
        public SubtextureShuffler Shuffler;
        public Image Noise;
        public Vector2 Offset;
        public Vector2 ShakeVector;
        public string EventID;
        public bool HasCutscene;
        public bool WhiteNoise;
        public bool ForceShake;
        private float yoffsetAmount = 4;
        private float shakeTimer;
        public float ShakeMult = 1;

        public MemoryGridBox(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Depth = 1;
            Tag |= Tags.TransitionUpdate;
            Collider = new Hitbox(16, 16);
            EventID = data.Attr("stringValue");
            HasCutscene = data.Bool("boolValue");
            WhiteNoise = true;
            Shuffler = new(NoiseTexture, 16, 16, Engine.DeltaTime, true);
            Add(Shuffler);
            for (int i = 0; i < 4; i++)
            {
                BoxSide side = new BoxSide((Sides)i, Width);
                BoxSides.Add(side);
                Add(side);
            }
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Player player = scene.GetPlayer();
            Noise = new Image(CycleStaticNoiseTexture());
            Tween hover = Tween.Create(Tween.TweenMode.YoyoLooping, Ease.SineInOut, Calc.Random.Range(0.9f, 1.2f), true);
            hover.OnUpdate = tween =>
            {
                Offset.Y = tween.Eased * yoffsetAmount;
            };
            Add(hover);
            if (HasCutscene)
            {
                Vector2 grounded = this.GroundedPosition();
                Collider = new Hitbox(16, (grounded.Y + 16) - Position.Y);
                Add(new DotX3(Collider, Interact));
            }
        }
        public override void Update()
        {
            base.Update();
            if (shakeTimer > 0 || ForceShake)
            {
                shakeTimer = Math.Max(shakeTimer - Engine.DeltaTime, 0);
                ShakeVector = Calc.Random.ShakeVector() * ShakeMult;
            }
            else
            {
                ShakeVector = Vector2.Zero;
            }
        }
        public override void Render()
        {
            Vector2 offset = Offset - Vector2.UnitY * (yoffsetAmount / 2) + ShakeVector;
            foreach (BoxSide side in BoxSides)
            {
                side.RenderOffset(offset);
            }
            Shuffler.RenderOffset(offset);

        }
        public void ShakeFor(float time, float mult = 1)
        {
            shakeTimer = time;
            ShakeMult = mult;
        }
        public void Interact(Player player)
        {
            Scene.Add(new MemoryGridBoxTouch(player, this));
        }
        public MTexture CycleStaticNoiseTexture()
        {
            MTexture tex = NoiseTexture;
            int x = (int)Calc.Random.Range(0, tex.Width - Width);
            int y = (int)Calc.Random.Range(0, tex.Height - Height);
            return tex.GetSubtexture(x, y, (int)Width, (int)Height);
        }
    }
}