using Celeste.Mod.Backdrops;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Media.Media3D;

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
        private bool State
        {
            get
            {
                return PianoModule.Session.RestoredPower; 
            }
        }
        private bool Flickering;
        public LabTubeLight(Vector2 position, int length) : base(position, length, 8, false)
        {
            Tag |= Tags.TransitionUpdate;

            Position = position;
            SpriteColor = Color.Lerp(Color.White, Color.Black, 0.2f);
            Length = Math.Max(16, length);
            Depth = -1;
            MTexture mTexture = GFX.Game["objects/PuzzleIslandHelper/machines/gizmos/tubeLight"];
            Image image;
            Add(image = new Image(mTexture.GetSubtexture(0, 0, 8, 8)));
            image.Color = SpriteColor;
            images.Add(image);

            for (int i = 0; i < Length - 16; i += 8)
            {
                Add(image = new Image(mTexture.GetSubtexture(8, 0, 8, 8)));
                image.Position.X = i + 8;
                images.Add(image);

            }
            Add(image = new Image(mTexture.GetSubtexture(16, 0, 8, 8)));
            image.Position.X = Length - 8;
            images.Add(image);

            Add(sfx = new SoundSource());
            Collider = new Hitbox(Length, 8);
            sfx.Position = new Vector2(Length/2, 4);
            Add(bloom = new BloomPoint(new Vector2(Length / 2, 4), 0.4f, Length / 2));
            Add(light = new VertexLight(new Vector2(Length / 2, 24), Color.White, 0.97f, 64, 120));
            OnDashCollide = DashCollision;
        }

        internal static void Load()
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
        internal static void Unload()
        {
            IL.Celeste.Player.ReflectBounce -= Player_Bounce;
            On.Celeste.PlayerHair.Render -= RenderHook;
        }
        private static void RenderHook(On.Celeste.PlayerHair.orig_Render orig, PlayerHair self)
        {
            if (ShockedHairColor.HasValue)
            {
                self.Color = ShockedHairColor.Value;
            }
            orig(self);

        }
        private DashCollisionResults DashCollision(Player player, Vector2 direction)
        {
            if (State)
            {
                //play spark sound
                //emit tiny electricity
                ModSpeed = true;
                Add(new Coroutine(HairColorFlash()));
                Add(new Coroutine(FlickerShort()));
                return DashCollisionResults.Bounce;
            }
            return DashCollisionResults.NormalCollision;
        }
        private IEnumerator HairColorFlash()
        {
            ShockedHairColor = Color.White;
            yield return null;
            ShockedHairColor = Color.Yellow;
            yield return null;
            ShockedHairColor = null;
            yield return null;
        }
        public LabTubeLight(EntityData e, Vector2 position)
            : this(e.Position + position, Math.Max(16, e.Width))
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
                    i.Color = State ? SpriteColor : Color.Lerp(Color.White, Color.Black, 0.2f);
                }
            }
        }
        public override void Render()
        {
            foreach (Component component in Components)
            {
                (component as Image)?.DrawOutline();
            }
            base.Render();

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
            while (true)
            {
                yield return Calc.Random.Range(3, 15f);

                for (int i = 0; i < 3; i++)
                {
                    if (i == 0 || Calc.Random.Chance(0.7f))
                    {
                        while (Flickering)
                        {
                            yield return null;
                        }
                        if (Flickering)
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