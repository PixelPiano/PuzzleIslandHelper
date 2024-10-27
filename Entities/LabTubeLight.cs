using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{


    [CustomEntity("PuzzleIslandHelper/LabTubeLight")]
    [Tracked]
    public class LabTubeLight : Solid
    {
        public readonly int Length;
        private static bool ModSpeed;
        private static Color? ShockedHairColor;
        private Coroutine PeriodicFlicker;
        private bool RoutineAdded;
        private List<Image> images = new List<Image>();
        private SoundSource sfx;
        private BloomPoint bloom;
        private VertexLight light;
        private float DimAmount = 0.4f;
        private Color SpriteColor;
        public bool Broken;
        private bool State
        {
            get
            {
                return PianoModule.Session.RestoredPower && !Broken;
            }
        }
        private bool FlipY;
        private bool Flickering;
        private bool digital;
        private float dimAmount;
        public LabTubeLight(Vector2 position, int length, bool digital, bool broken, bool flipY) : base(position, length, 8, false)
        {
            Tag |= Tags.TransitionUpdate;

            this.digital = digital;
            Broken = broken;
            Position = position;
            FlipY = flipY;
            dimAmount = Broken ? 0.4f : 0.2f;
            SpriteColor = Color.Lerp(Color.White, Color.Black, dimAmount);
            Length = Math.Max(16, length);
            Depth = -1;
            MTexture mTexture = GFX.Game["objects/PuzzleIslandHelper/machines/gizmos/tubeLight" + (digital ? "Digi" : broken ? "Broken" : "")];
            Image image;
            int y = 0;
            if (Broken)
            {
                y = Calc.Random.Range(0, 3) * 8;
            }
            Add(image = new Image(mTexture.GetSubtexture(0, y, 8, 8)));
            image.Effects = flipY ? SpriteEffects.FlipVertically : default;
            image.Color = SpriteColor;
            images.Add(image);

            for (int i = 0; i < Length - 16; i += 8)
            {
                if (Broken)
                {
                    y = Calc.Random.Range(0, 3) * 8;
                }
                Add(image = new Image(mTexture.GetSubtexture(8, y, 8, 8)));
                image.Position.X = i + 8;
                image.Effects = flipY ? SpriteEffects.FlipVertically : default;
                images.Add(image);
            }
            if (Broken)
            {
                y = Calc.Random.Range(0, 3) * 8;
            }
            Add(image = new Image(mTexture.GetSubtexture(16, y, 8, 8)));
            image.Effects = flipY ? SpriteEffects.FlipVertically : default;
            image.Position.X = Length - 8;
            images.Add(image);

            Add(sfx = new SoundSource());
            Collider = new Hitbox(Length, 8);
            sfx.Position = new Vector2(Length / 2, 4);
            Add(bloom = new BloomPoint(new Vector2(Length / 2, 4), Broken ? 0.05f : 0.15f, Length / 2));
            Add(light = new VertexLight(new Vector2(Length / 2, flipY ? 0 : 24), digital ? Color.Green : Color.White, Broken ? 0.3f : 0.86f, (int)Width, (int)Width + 16));
            OnDashCollide = DashCollision;
        }
        [OnLoad]
        public static void Load()
        {
            IL.Celeste.Player.ReflectBounce += Player_Bounce;
            On.Celeste.PlayerHair.Render += RenderHook;
        }

        private static void Player_Bounce(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            // go to just after direction.Y is retrieved
            cursor.GotoNext(MoveType.After,
              instr => instr.MatchLdarg(0),
              instr => instr.MatchLdflda<Player>("Speed"),
              instr => instr.MatchLdarg(1),
              instr => instr.MatchLdfld<Vector2>("Y")
            );
            // go to just before it's put into Speed.Y
            cursor.GotoNext(MoveType.Before,
              instr => instr.MatchStfld<Vector2>("Y")
            );

            // get the multiplier
            cursor.EmitDelegate(getMult);
            // multiply them
            cursor.Emit(OpCodes.Mul);
        }
        private static float getMult()
        {
            return ModSpeed ? 1.5f : 1;
        }
        [OnUnload]
        public static void Unload()
        {
            IL.Celeste.Player.ReflectBounce -= Player_Bounce;
            On.Celeste.PlayerHair.Render -= RenderHook;
        }
        private static void RenderHook(On.Celeste.PlayerHair.orig_Render orig, PlayerHair self)
        {
            Color prev = self.Color;
            if (ShockedHairColor.HasValue)
            {
                self.Color = ShockedHairColor.Value;
            }
            orig(self);
            self.Color = prev;

        }
        private DashCollisionResults DashCollision(Player player, Vector2 direction)
        {
            if (State && !Broken)
            {
                //play spark sound
                //emit tiny electricity
                ModSpeed = true;
                AddShocks(player);
                Add(new Coroutine(HairColorFlash()));
                Add(new Coroutine(FlickerShort()));
                return DashCollisionResults.Bounce;
            }
            return DashCollisionResults.NormalCollision;
        }
        public void AddShocks(Player player)
        {
            int totalShocks = Calc.Random.Range(2, 5);
            for (int i = 0; i < totalShocks; i++)
            {
                RandomShock shock = new RandomShock(player.TopCenter, 1, Calc.Random.Range(1, 8), Calc.Random.Choose(1, 2) * Engine.DeltaTime, player,
                   Calc.Random.Choose(Color.AliceBlue, Color.LightBlue, Color.White));
                Scene.Add(shock);
            }
        }
        private IEnumerator HairColorFlash()
        {
            int frames = 2;
            ShockedHairColor = Color.White;
            yield return Engine.DeltaTime * frames;
            ShockedHairColor = null;
            yield return Engine.DeltaTime * frames;
            ShockedHairColor = Color.Yellow;
            yield return Engine.DeltaTime * frames;
            ShockedHairColor = null;
            yield return Engine.DeltaTime * frames;
            ShockedHairColor = Color.White;
            yield return Engine.DeltaTime * frames;
            ShockedHairColor = null;
            yield return Engine.DeltaTime * frames;
            ShockedHairColor = Color.Yellow;
            yield return Engine.DeltaTime * frames;
            ShockedHairColor = null;
            yield return Engine.DeltaTime * frames;
            ShockedHairColor = null;
        }
        public LabTubeLight(EntityData data, Vector2 position)
            : this(data.Position + position, Math.Max(16, data.Width), data.Bool("digital"), data.Bool("broken"), data.Bool("flipY"))
        {
        }
        public override void Update()
        {
            base.Update();
            UpdateVisuals();
        }
        private void UpdateVisuals()
        {
            if (!RoutineAdded && State)
            {
                if (PeriodicFlicker is null)
                {
                    PeriodicFlicker = new Coroutine(Flicker());
                }
                Add(PeriodicFlicker);
                RoutineAdded = true;
            }
            light.Visible = bloom.Visible = State;
            foreach (Image i in images)
            {
                if (!Flickering)
                {
                    i.Color = State ? SpriteColor : Color.Lerp(Color.White, Color.Black, dimAmount);
                }
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            PeriodicFlicker = new Coroutine(Flicker());
            light.Visible = bloom.Visible = State;
            if (State)
            {
                RoutineAdded = true;
                Add(PeriodicFlicker);
            }
        }
        private IEnumerator FlickerShort()
        {
            Flickering = true;

            sfx.Play("event:/PianoBoy/TubeLightSparks");
            while (sfx.InstancePlaying)
            {
                foreach (Image image in images)
                {
                    image.Color = Color.Lerp(SpriteColor, Color.Black, DimAmount);
                }
                float l = light.Alpha;
                float b = bloom.Alpha;
                light.Alpha -= 0.02f;
                bloom.Alpha -= 0.4f;
                yield return 0.07f;

                light.Alpha = l;
                bloom.Alpha = b;
                foreach (Image image in images)
                {
                    image.Color = SpriteColor;
                }
                yield return Calc.Random.Range(0.01f, 0.05f);
            }

            Flickering = false;
            ModSpeed = false;
        }
        private IEnumerator Flicker()
        {
            float min = Broken ? 0.5f : 3;
            float max = Broken ? 1 : 15;
            while (true)
            {
                yield return Calc.Random.Range(min, max);
                int loops = Broken ? 6 : 3;
                for (int i = 0; i < loops; i++)
                {
                    if (i == 0 || Calc.Random.Chance(0.7f))
                    {
                        while (Flickering)
                        {
                            yield return null;
                        }
                        if (Flickering && !Broken)
                        {
                            yield return Calc.Random.Range(1, 2);
                        }
                        Flickering = true;
                        foreach (Image image in images)
                        {
                            image.Color = Color.Lerp(SpriteColor, Color.Black, 0.05f);
                        }
                        float l = light.Alpha;
                        float b = bloom.Alpha;
                        light.Alpha -= 0.01f;
                        bloom.Alpha -= 0.2f;

                        yield return Calc.Random.Range(0.03f, 0.07f);
                        foreach (Image image in images)
                        {
                            image.Color = SpriteColor;
                        }
                        light.Alpha = l;
                        bloom.Alpha = b;
                        Flickering = false;
                        yield return Calc.Random.Range(0.03f, 0.07f);
                    }
                }
                yield return null;
            }
        }
    }
}