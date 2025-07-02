using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
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

        private List<Image> images = new List<Image>();
        private SoundSource sfx;
        private BloomPoint bloom;
        private VertexLight light;
        private float DimAmount = 0.4f;
        private Color SpriteColor;
        public static float MultDecay = 0;
        public bool TakeAwayDash = false;
        public bool Broken;
        public bool RestoredPower => PianoModule.Session.RestoredPower;
        private enum directions
        {
            Up, Down, Left, Right
        }
        private directions dir;
        private bool Flickering;
        private float dimAmount;
        public bool Horizontal;
        public static float DashTimer;
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
            Depth = -51;

            dir = data.Enum("facing", directions.Down);
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
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
        }
        private bool usedDash;
        private void OnDash(Vector2 dir)
        {
            if (trackDash)
            {
                usedDash = true;
            }
        }
        [OnLoad]
        public static void Load()
        {
            On.Celeste.PlayerHair.Render += RenderHook;
            On.Celeste.Player.ReflectBounce += Player_ReflectBounce;
        }

        private static void Player_ReflectBounce(On.Celeste.Player.orig_ReflectBounce orig, Player self, Vector2 direction)
        {
            orig(self, direction);
            if (ModSpeed)
            {
                self.Speed *= BounceMult;
                ModSpeed = false;

            }
        }
        public static Vector2 BounceMult = Vector2.One;
        [OnUnload]
        public static void Unload()
        {
            On.Celeste.PlayerHair.Render -= RenderHook;
            On.Celeste.Player.ReflectBounce -= Player_ReflectBounce;
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
            if (!RestoredPower) return DashCollisionResults.NormalCollision;
            if ((!Horizontal && direction.X == 0) || (Horizontal && direction.Y == 0)) //if collided on side edge
            {
                Add(new Coroutine(FlickerShort()));
                return DashCollisionResults.Rebound;
            }
            switch (dir) //if collided in direction opposite of facing direction
            {
                case directions.Up:
                    if (player.Top > Bottom) return DashCollisionResults.NormalCollision;
                    break;
                case directions.Down:
                    if (player.Bottom < Top) return DashCollisionResults.NormalCollision;
                    break;
                case directions.Left:
                    if (player.Left > Right) return DashCollisionResults.NormalCollision;
                    break;
                case directions.Right:
                    if (player.Right < Left) return DashCollisionResults.NormalCollision;
                    break;
            }
            ModSpeed = true;
            if (Horizontal)
            {
                if (direction.X != 0)
                {
                    BounceMult.X = 1.7f;
                    BounceMult.Y = 1.3f;
                }
                else
                {
                    BounceMult.X = 1f;
                    BounceMult.Y = 1.5f;
                }
            }
            else
            {
                BounceMult.X = 1.6f;
                BounceMult.Y = direction.Y > 0 ? -1.5f : 1.5f;
            }
            if (Broken)
            {
                BounceMult *= 0.5f;
                if (Calc.Random.Chance(0.5f))
                {
                    AddShock(player, 1, 1, 2);
                    Add(new Coroutine(FlickerShort()));
                }
            }
            else
            {
                for (int i = 0; i < Calc.Random.Range(2, 5); i++)
                {
                    AddShock(player, Calc.Random.Range(2, 5), Calc.Random.Range(3, 8), Calc.Random.Range(1, 3));
                }
                player.UseRefill(false);
                Add(new Coroutine(HairColorFlash(player)));
                Add(new Coroutine(FlickerShort()));
            }
            return DashCollisionResults.Bounce;
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            ModSpeed = false;
        }
        public void AddShock(Player player, int strands, int generations, int frames)
        {
            RandomShock shock = new RandomShock(player.TopCenter, strands, generations, frames * Engine.DeltaTime, player, Calc.Random.Choose(Color.AliceBlue, Color.LightBlue, Color.White));
            Scene.Add(shock);
        }
        private bool trackDash;
        private IEnumerator HairColorFlash(Player player)
        {
            trackDash = true;
            int frames = 3;
            Color[] colors = [Color.White, Color.Yellow];
            for (int i = 0; i < 2; i++)
            {
                foreach (Color c in colors)
                {
                    ShockedHairColor = c;
                    yield return Engine.DeltaTime * frames;
                    ShockedHairColor = null;
                    yield return Engine.DeltaTime * frames;
                }
            }
            trackDash = false;
            if (!usedDash && TakeAwayDash)
            {
                player.Dashes = Math.Max(player.Dashes - 1, 0);  
            }
            usedDash = false;
        }
        public override void Update()
        {
            base.Update();
            light.Visible = bloom.Visible = RestoredPower && !Broken;
            foreach (Image i in images)
            {
                if (!Flickering)
                {
                    i.Color = (RestoredPower && !Broken) ? SpriteColor : Color.Lerp(Color.White, Color.Black, dimAmount);
                }
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            PeriodicFlicker = new Coroutine(IdleFlickerLoop());
            Add(PeriodicFlicker);
            light.Visible = bloom.Visible = RestoredPower && !Broken;
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
        }
        private IEnumerator IdleFlickerLoop()
        {
            float min = Broken ? 0.5f : 3;
            float max = Broken ? 1 : 15;
            while (true)
            {
                while (!RestoredPower)
                {
                    yield return null;
                }
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