using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
// PuzzleIslandHelper.TSwitch
namespace Celeste.Mod.PuzzleIslandHelper.Entities.GameplayEntities
{
    [CustomEntity("PuzzleIslandHelper/TSwitch")]
    [Tracked]
    public class TSwitch : Solid
    {
        private float colorRate;
        private Sprite sprite;
        private Sprite Arrows;
        private EntityID id;
        private float startY;
        private float endY;
        private bool Pressed;
        private bool FirstContact;
        private BloomPoint Bloom;
        private bool Used
        {
            get { return PianoModule.Session.PressedTSwitches.ContainsKey(id); }
        }
        private ParticleType Dust = new ParticleType
        {
            Size = 3,
            Direction = MathHelper.Pi * 3 / 4,
            DirectionRange = MathHelper.Pi / 6,
            Color = Color.Black,
            Color2 = Color.Green,
            SpeedMin = 10f,
            SpeedMax = 50f,
            LifeMin = 0.5f,
            LifeMax = 0.7f,
            ColorMode = ParticleType.ColorModes.Choose,
            FadeMode = ParticleType.FadeModes.Linear

        };
        private ParticleSystem system;
        public TSwitch(EntityData data, Vector2 offset, EntityID id)
          : base(data.Position + offset, 24, 16, false)
        {
            this.id = id;
            startY = Position.Y;
            endY = Position.Y + 8;
            Add(Bloom = new BloomPoint(new Vector2(12, 0), 0, 24));
            Add(sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/tswitch/"));
            sprite.AddLoop("idle", "block", 1f);
            sprite.AddLoop("bright", "flash", 0.1f, 2);
            sprite.Add("fadeBack", "flashReverse", 0.1f, "idle");
            sprite.Play("idle");
            sprite.Add("flash", "flash", 0.05f, "bright");
            sprite.Position.X -= 2;

            Add(Arrows = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/tswitch/"));
            Arrows.AddLoop("idle", "arrows", 0.1f);
            Arrows.Color = Color.LightGreen;
            Arrows.Play("idle");
            Collider = new ColliderList(new Hitbox(24, 8), new Hitbox(8, 8, 8, 8));
            OnDashCollide = OnDash;
            Add(new LightOcclude());
        }
        private void DustParticles()
        {
            float angle1 = Calc.ToDeg(165);
            float angle2 = Calc.ToDeg(15);
            for (int i = 0; i < 8; i++)
            {
                system.Emit(Dust, Position + Vector2.UnitY * i, angle1);
            }
            for (int i = 0; i < 8; i++)
            {
                system.Emit(Dust, Position + new Vector2(24, i), Dust.Direction + MathHelper.Pi);
            }
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            scene.Add(system = new ParticleSystem(Depth + 1, 500));
            if (!Used)
            {
                Add(new Coroutine(MoveRoutine(), true));
            }
            else
            {
                foreach (KeyValuePair<EntityID, Vector2> entity in PianoModule.Session.PressedTSwitches)
                {
                    if (entity.Key.ID == id.ID)
                    {
                        Position = entity.Value;
                        Pressed = true;
                    }
                }
            }
        }
        public override void Update()
        {
            base.Update();
            if (HasPlayerRider() && !FirstContact)
            {
                FirstContact = true;
                sprite.Play("flash");
            }
            if (FirstContact && !HasPlayerRider())
            {
                FirstContact = false;
                sprite.Play("fadeBack");
            }
            colorRate += Engine.DeltaTime;
            colorRate %= 1;

            Arrows.Color = Color.Lerp(Color.LightGreen, Color.Green, colorRate);
        }
        private DashCollisionResults OnDash(Player player, Vector2 direction)
        {
            if (direction.Y > 0)
            {
                Pressed = true;
            }
            return DashCollisionResults.Rebound;
        }

        private IEnumerator MoveRoutine()
        {
            while (!Pressed && !Used)
            {
                for (float i = 0; i < 1f; i += Engine.DeltaTime)
                {
                    if (Pressed || Used)
                    {
                        break;
                    }
                    Bloom.Alpha = 1 - i;
                    MoveToY(startY + Calc.LerpClamp(0, 8, Ease.SineInOut(i)));
                    yield return null;
                }
                for (float i = 0; i < 1f; i += Engine.DeltaTime)
                {
                    if (Pressed || Used)
                    {
                        break;
                    }
                    Bloom.Alpha = i;
                    MoveToY(endY - Calc.LerpClamp(0, 8, Ease.SineInOut(i)));
                    yield return null;
                }
            }
            if (!Used)
            {

                while (!CollideCheck<Solid>())
                {
                    MoveToY(Position.Y + 4);
                    DustParticles();
                    yield return null;
                }
                //PlayEvent collide sound
                for (float i = 0; i < 2; i++)
                {
                    MoveToY(Position.Y + 4);
                    DustParticles();
                    yield return null;
                }
                Arrows.Visible = false;
                StartShaking(0.4f);
                yield return 0.4f;
                if (!PianoModule.Session.PressedTSwitches.ContainsKey(id))
                {
                    PianoModule.Session.PressedTSwitches.Add(id, Position);
                }
            }
        }
        public override void OnShake(Vector2 amount)
        {
            base.OnShake(amount);
            MoveH(amount.X);
        }
    }
}