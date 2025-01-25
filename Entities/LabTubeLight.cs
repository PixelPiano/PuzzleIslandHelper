using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;
using System.Collections;
using System.Collections.Generic;

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
        public static float MultDecay = 0;
        public bool Broken;
        private bool State
        {
            get
            {
                return PianoModule.Session.RestoredPower && !Broken;
            }
        }
        private enum directions
        {
            Up, Down, Left, Right
        }
        private directions dir;
        private bool Flickering;
        private float dimAmount;
        public bool Horizontal;
        private void addImage(string path, Vector2 pos, Vector2 quadPos, Vector2 scale, float rotation)
        {

            Image image = new Image(GFX.Game[path].GetSubtexture((int)quadPos.X, (int)quadPos.Y + (Broken ? Calc.Random.Range(0, 3) * 8 : 0), 8, 8));
            image.CenterOrigin();
            image.Position = pos + image.HalfSize();
            image.Scale = scale;
            image.Rotation = rotation;
            image.Color = SpriteColor;
            images.Add(image);
            Add(image);
        }
        public LabTubeLight(EntityData data, Vector2 offset) : base(data.Position + offset, 0, 0, true)
        {
            Tag |= Tags.TransitionUpdate;
            Broken = data.Bool("broken");
            dimAmount = Broken ? 0.4f : 0.2f;
            SpriteColor = Color.Lerp(Color.White, Color.Black, dimAmount);
            Depth = -1;

            dir = data.Enum<directions>("facing", directions.Down);
            string p = "objects/PuzzleIslandHelper/machines/gizmos/tubeLight";
            bool digital = data.Bool("digital");
            if (digital)
            {
                p += "Digi";
            }
            else
            {
                p += "Broken";
            }

            float rot = 0;
            Vector2 scale = Vector2.One;
            if (dir is directions.Up)
            {
                scale.Y = -1;
            }
            else if (dir is directions.Right)
            {
                scale.X = -1;
                rot = (float)-Math.PI / 2;
            }
            else if (dir is directions.Left)
            {
                rot = (float)Math.PI / 2;
            }
            Horizontal = (int)dir < 2;
            Vector2 m = Horizontal ? Vector2.UnitX : Vector2.UnitY;
            Length = Math.Max(Horizontal ? data.Width : data.Height, 8);
            addImage(p, Vector2.Zero, Vector2.Zero, scale, rot);
            for (int i = 8; i < Length - 8; i += 8)
            {
                addImage(p, new Vector2(i) * m, Vector2.UnitX * 8, scale, rot);
            }
            addImage(p, new Vector2(Length - 8) * m, Vector2.UnitX * 16, scale, rot);

            float x = Horizontal ? Length / 2 : 4;
            float y = Horizontal ? 4 : Length / 2;
            Add(sfx = new SoundSource());
            Collider = new Hitbox(x * 2, y * 2);
            sfx.Position = new Vector2(x, y);


            Add(bloom = new BloomPoint(new Vector2(x, y), Broken ? 0.05f : 0.15f, Length / 2));
            if (dir is directions.Up)
            {
                y += 16;
            }
            Add(light = new VertexLight(new Vector2(x, y), digital ? Color.Green : Color.White, Broken ? 0.3f : 0.86f, Length, Length + 16));
            OnDashCollide = DashCollision;
            Add(new DashListener(OnDash));
        }
        private bool givenDash;
        private void OnDash(Vector2 dir)
        {
            if (givenDash)
            {
                givenDash = false;
            }
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
              instr => instr.MatchLdfld<Vector2>("X")
            );
            cursor.GotoNext(MoveType.Before,
              instr => instr.MatchStfld<Vector2>("X")
            );

            // get the multiplier
            cursor.EmitDelegate(getXMult);
            // multiply them
            cursor.Emit(OpCodes.Mul);

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
            cursor.EmitDelegate(getYMult);
            // multiply them
            cursor.Emit(OpCodes.Mul);
        }
        public static Vector2 BounceMult = Vector2.One * 1.5f;
        private static float getXMult()
        {
            return ModSpeed ? (BounceMult.X <= 0 ? -1 : 1) + BounceMult.X : 1;
        }
        private static float getYMult()
        {
            return ModSpeed ? (BounceMult.Y <= 0 ? -1 : 1) + BounceMult.Y : 1;
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
            if ((Horizontal && direction.Y == 0) || (!Horizontal && direction.X == 0) || (Horizontal && (player.Right < Left || player.Left > Right)) || (!Horizontal && (player.Bottom < Top || player.Top > Bottom)))
            {
                return DashCollisionResults.NormalCollision;
            }
            else if (!Horizontal && direction.X == 0) return DashCollisionResults.Rebound;
            if (State && !Broken)
            {
                ModSpeed = true;
                float max = 0.6f;
                float min = 0.3f;
                BounceMult = Horizontal ? new Vector2(min, max) : new Vector2(max, min);
                if (Horizontal)
                {
                    BounceMult.X = 0;
                    BounceMult.Y = 0.5f;
                }
                else
                {
                    BounceMult.X = 0.7f;
                    if (direction.Y > 0)
                    {
                        BounceMult.Y = -0.3f;
                    }
                    else
                    {
                        BounceMult.Y = 0.3f;
                    }
                }
                AddShocks(player);
                if (!givenDash)
                {
                    player.Dashes++;
                    givenDash = true;
                }
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
                RandomShock shock = new RandomShock(player.TopCenter, 3, Calc.Random.Range(4, 8), Calc.Random.Choose(1, 2) * Engine.DeltaTime, player,
                   Calc.Random.Choose(Color.AliceBlue, Color.LightBlue, Color.White));
                Scene.Add(shock);
            }
        }
        private IEnumerator HairColorFlash()
        {
            int frames = 2;
            Color[] colors = [Color.White, Color.Yellow];
            for (int i = 0; i < 2; i++)
            {
                foreach (Color c in colors)
                {
                    ShockedHairColor = c;
                    yield return Engine.DeltaTime * frames;
                    ShockedHairColor = null;
                    if (!givenDash) yield break;
                    yield return Engine.DeltaTime * frames;
                }
            }
            if (givenDash && Scene.GetPlayer() is Player player)
            {
                player.Dashes = Math.Max(0, player.Dashes - 1);
                givenDash = false;
            }
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            if (givenDash)
            {
                Draw.Rect(Position, 4, 4, Color.Green);
            }
            else
            {
                Draw.Rect(Position, 4, 4, Color.Red);
            }
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
                ModSpeed = false;
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