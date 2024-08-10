using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Backdrops;
using System.Collections;
using System;

namespace Celeste.Mod.PuzzleIslandHelper.Effects
{
    [CustomBackdrop("PuzzleIslandHelper/HexField")]
    public class HexField : Backdrop
    {
        public int ActiveHexes;
        private float timer;
        public HexField(BinaryPacker.Element element) : base()
        {
        }
        public override void Update(Scene scene)
        {
            base.Update(scene);
            timer += Engine.DeltaTime;
            if (timer > 0.1f)
            {
                timer = 0;
                SpawnHex(scene);
            }
        }
        public override void Render(Scene scene)
        {
            base.Render(scene);
            foreach (Hex hex in scene.Tracker.GetEntities<Hex>())
            {
                if (hex.OnScreen) hex.Render();
            }
        }
        public void SpawnHex(Scene scene)
        {
            /*            Vector2 pos = new Vector2(100, 94);
                        float angle = Calc.Random.Range(0, 360f);
                        pos += Calc.AngleToVector((angle + 180f).ToRad(), 200);*/
            Vector2 pos = new Vector2(160, -16);
            float angle = 45f;
            Hex hex = new Hex(this, pos, Vector2.UnitX * 320, Calc.Random.Range(80, 200), angle.ToRad());
            scene.Add(hex);
        }
    }
    [Tracked]
    public class Hex : Entity
    {
        public int Num;
        public float Size;
        public float Speed;
        public float Direction;
        public float ActiveTimer;
        public bool Disappearing;
        public bool Adding;
        public int spacing = 2;
        public bool InRoutine => Adding || Disappearing;
        private Rectangle cameraBounds;
        private float[] texScale = new float[6];

        public static MTexture Numbers = GFX.Game["objects/PuzzleIslandHelper/hexField/numbers"];
        public static MTexture Ecks = GFX.Game["objects/PuzzleIslandHelper/hexField/x"];
        public static MTexture RedTex = GFX.Game["objects/PuzzleIslandHelper/hexField/red"];
        public static MTexture GreenTex = GFX.Game["objects/PuzzleIslandHelper/hexField/green"];
        public static MTexture BlueTex = GFX.Game["objects/PuzzleIslandHelper/hexField/blue"];
        public MTexture[] Textures = new MTexture[16];
        public HexField Parent;
        private int padding = 160;
        public int[] values = new int[6];
        public string Message = "000000";
        public Color color;
        public bool OnScreen;
        private float timeOffScreen;
        public static ParticleType Particle = new ParticleType()
        {
            Size = 3,
            SizeRange = 2,
            ScaleOut = true,
            LifeMin = 1,
            LifeMax = 2,
            SpeedMin = 5,
            SpeedMax = 10,
            RotationMode = ParticleType.RotationModes.Random,
            SpinMin = 0,
            SpinMax = 2

        };
        public void Combine(Hex from)
        {
            Message = "";
            for (int i = 0; i < 6; i++)
            {
                int fromthis = values[i];
                int fromthat = from.values[i];
                int tothis = (fromthis + fromthat) % 16;
                values[i] = tothis;
                Message += tothis.ToString("X");
            }
            color = Calc.HexToColor(Message);
        }
        public Hex(HexField from, Vector2 spawn, Vector2 range, float speed, float angleRad) : base()
        {
            Message = "";
            for (int i = 0; i < 16; i++)
            {
                Textures[i] = GFX.Game["objects/PuzzleIslandHelper/hexField/" + i.ToString("X")];
            }
            for (int i = 0; i < 6; i++)
            {
                values[i] = Calc.Random.Range(0, 16);
                Message += values[i].ToString("X");
            }
            color = Calc.HexToColor(Message);
            Parent = from;
            Size = Calc.Random.Range(0.5f, 1);
            Speed = speed;
            Direction = angleRad;
            Position = spawn + Calc.Random.Range(-range, range);
            Collider = new Hitbox(8 * Size * 9 + (spacing * Size * 3), 8);
            cameraBounds = new Rectangle(-padding / 2, -padding / 2, 320 + padding, 180 + padding);
            for (int i = 0; i < 4; i++)
            {
                texScale[i] = 1;
            }
            Tag |= Tags.TransitionUpdate | Tags.Global;
            Visible = false;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (Parent != null)
            {
                Parent.ActiveHexes++;
            }
            if (Parent.ActiveHexes > 100)
            {
                RemoveSelf();
            }
        }
        public override void Render()
        {
            base.Render();
            DrawTex(RedTex, 0, -0.5f);
            DrawNumber(values[0], 1);
            DrawNumber(values[1], 2);
            DrawTex(GreenTex, 3, -0.5f);
            DrawNumber(values[2], 4);
            DrawNumber(values[3], 5);
            DrawTex(BlueTex, 6, -0.5f);
            DrawNumber(values[4], 7);
            DrawNumber(values[5], 8);
        }
        public void DrawCharacter(int index)
        {
            DrawTex(Ecks, index, 0);
        }
        public void DrawNumber(int num, int index)
        {
            DrawTex(Textures[num], index, 0);
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Draw.HollowRect(camera.Position + Position, Width, Height, Color.Red);
        }
        public void DrawTex(MTexture tex, int index, float scaleDiff)
        {
            float x = index * (8 * Size) + (spacing * Size) * index - scaleDiff * 8;
            x += (1 - Size) * 8;
            tex.Draw(Position + Vector2.UnitX * x, Vector2.Zero, Color.Lerp(color, Color.White, whiteLerp), Math.Max(Size + scaleAdd + scaleDiff, 0));
        }
        public override void Update()
        {
            base.Update();
            ActiveTimer += Engine.DeltaTime;
            if (Scene.OnInterval(10 / 60f))
            {
                float newdir = Direction + 180f.ToRad();
                Particle.SpeedMin = Speed / 2;
                Particle.SpeedMax = Speed;
                SceneAs<Level>().ParticlesFG.Emit(
                    Particle,
                    Center + SceneAs<Level>().Camera.Position,
                    Color.Lerp(color, Color.White, 0.1f),
                    newdir);
            }
            OnScreen = cameraBounds.Intersects(Collider.Bounds);

            if (!OnScreen)
            {
                timeOffScreen += Engine.DeltaTime;
                if (timeOffScreen >= 4)
                {
                    RemoveSelf();
                }
            }
            else
            {
                timeOffScreen = 0;
            }
            if (!InRoutine && OnScreen)
            {
                foreach (Hex hex in Scene.Tracker.GetEntities<Hex>())
                {
                    if (hex == this || !hex.OnScreen) continue;
                    if (!hex.InRoutine && CollideCheck(hex) && Speed > hex.Speed)
                    {
                        AddFrom(hex);
                    }
                }
            }

            Position += Calc.AngleToVector(Direction, Speed * Engine.DeltaTime);
        }
        private float scaleAdd;
        private float whiteLerp;
        public void AddFrom(Hex hex)
        {
            Num = (Num + hex.Num) % 100;
            Combine(hex);
            /*Wiggler w1, w2;*/
            Wiggler w;
            Add(w = Wiggler.Create(0.5f, 4f, delegate (float f) { scaleAdd = (f * 0.3f); }, true, true));
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineOut, 0.5f, true);
            tween.OnUpdate = (Tween t) =>
            {
                whiteLerp = 0.8f - t.Eased * 0.8f;
            };
            Add(tween);
            hex.Disappear();
        }
        public void Disappear()
        {
            Disappearing = true;
            Add(new Coroutine(ShrinkAll()));
        }
        private IEnumerator ShiftNumber(int to)
        {
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                for (int j = 2; j < 4; j++)
                {
                    texScale[j] = Calc.LerpClamp(1, 0, i);
                }
                yield return null;
            }
            for (int j = 2; j < 4; j++)
            {
                texScale[j] = 0;
            }
            Num = (Num + to) % 100;
            yield return 0.05f;
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                for (int j = 2; j < 4; j++)
                {
                    texScale[j] = Calc.LerpClamp(0, 1, i);
                }
                yield return null;
            }
            for (int j = 2; j < 4; j++)
            {
                texScale[j] = 1;
            }
            Adding = false;
            Disappearing = false;
        }
        private IEnumerator ShrinkAll()
        {
            float from = Speed;
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                scaleAdd = Calc.LerpClamp(0, -1, i);
                Speed = Calc.LerpClamp(from, 0, i);
                yield return null;
            }
            RemoveSelf();
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            if (Parent != null)
            {
                Parent.ActiveHexes--;
            }
        }
    }
}