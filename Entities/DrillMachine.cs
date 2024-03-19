using Celeste.Mod.Entities;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using Celeste.Mod.PuzzleIslandHelper.Components;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/DrillMachine")]
    [Tracked]
    public class DrillMachine : Entity
    {
        private Sprite[] Drills = new Sprite[2];
        private Sprite[] Batteries = new Sprite[3];
        private Image[] BatteryFlash = new Image[3];
        private Vector2[] initPositions = new Vector2[2];
        private Image Box;
        private float SpinRate;
        private float YOffset;
        private float XOffset;
        private bool AtBreakingPoint;
        private DotX3 Talk;
        private bool wasIntact;
        private bool Exploded
        {
            get
            {
                if (Scene is not Level level) return false;
                return level.Session.GetFlag("drillExploded");
            }
            set
            {
                if (Scene is not Level level) return;
                level.Session.SetFlag("drillExploded", value);
            }
        }
        private bool Activated;
        private bool InCutscene;
        private bool Spinning;
        private Image TempExplosion;
        private bool CanActivate
        {
            get
            {
                return PianoModule.Session.DrillBatteryIds.Count > 2;
            }
        }
        public DrillMachine(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Depth = 1;
            TempExplosion = new Image(GFX.Game["objects/PuzzleIslandHelper/drillMachine/explosionHere"]);
            Add(TempExplosion);
            TempExplosion.Visible = false;
            Box = new Image(GFX.Game["objects/PuzzleIslandHelper/drillMachine/batteryBox"]);
            Add(Box);
            for (int i = 0; i < 2; i++)
            {
                Drills[i] = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/drillMachine/");
                Drills[i].AddLoop("idle", "drill", 0.1f, 0);
                Drills[i].AddLoop("drilling", "drill", 0.1f);
                Drills[i].Play("idle");
                Drills[i].X = -1 + i * (Box.Width + Drills[i].Width + 2);
                initPositions[i] = Drills[i].Position;
            }
            Add(Drills);
            Box.Position = new Vector2(Drills[0].Width, Drills[0].Height - Box.Height);
            for (int i = 0; i < 3; i++)
            {
                Batteries[i] = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/drillMachine/");
                BatteryFlash[i] = new Image(GFX.Game["objects/PuzzleIslandHelper/drillMachine/batteryFlash"]);
                Batteries[i].AddLoop("idle", "batteryBoxSlot", 0.1f);
                Batteries[i].AddLoop("lit","batteryBoxSlotLit",0.1f);
                BatteryFlash[i].Position = Batteries[i].Position = Box.Position + new Vector2(1 + (i * (1 + Batteries[i].Width)), 2);
                Batteries[i].Play("idle");
                BatteryFlash[i].Color = Color.Transparent;
            }
            Add(Batteries);
            Add(BatteryFlash);
            float height = Calc.Max(Box.Height, Drills[0].Height);
            Collider = new Hitbox(Box.Width + Drills[0].Width * 2, height);
            Talk = new DotX3(Collider, Interact)
            {
                PlayerMustBeFacing = false
            };
            Add(Talk);
        }
        private void Interact(Player player)
        {
            if (CanActivate && !Activated)
            {
                Exploded = false;
                Add(new Coroutine(Cutscene(player)));
                Activated = true;
            }
        }

        public void UpdateBatteries()
        {
            int count = PianoModule.Session.DrillBatteryIds.Count;
            for (int i = 0; i < Batteries.Length; i++)
            {
                Batteries[i].Play(count > i ? "lit" : "idle");
            }
        }
        public override void Render()
        {
            if (!Exploded)
            {
                Drills.DrawSimpleOutlines();
                Box.DrawSimpleOutline();
            }
            base.Render();
        }
        private void FlashBattery(int index, float duration)
        {
            if (BatteryFlash.Length < index) return;
            Tween t = Tween.Create(Tween.TweenMode.YoyoOneshot, Ease.SineIn, duration / 2);
            t.OnUpdate = (Tween t) =>
            {
                BatteryFlash[index].Color = Color.Lerp(Color.Transparent, Color.White, t.Eased);
            };
            Add(t);
            t.Start();
        }
        private void StartSpinning()
        {
            Spinning = true;
            XOffset = 1;
            foreach (Sprite s in Drills)
            {
                s.Play("drilling");
            }
        }
        private IEnumerator Cutscene(Player player)
        {
            if (Scene is not Level level || player is null) yield break;
            InCutscene = true;

            player.StateMachine.State = Player.StDummy;
            player.ForceCameraUpdate = false;

            Vector2 target = level.MarkerCentered("drillMarker");
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                level.Camera.Position = Vector2.Lerp(level.Camera.Position, target, i);
                yield return null;
            }
            yield return player.DummyWalkTo(Position.X - 32);
            yield return 0.2f;
            float flashLength = 0.7f;
            for (int i = 0; i < BatteryFlash.Length; i++)
            {
                FlashBattery(i, flashLength);
                yield return flashLength / 1.5f + 0.1f;
            }

            float div = 0.2f;
            StartSpinning();
            for (float i = 0; i < 1; i += Engine.DeltaTime / 2)
            {
                SpinRate = 1 - 0.9f * Ease.CubeIn(i);
                yield return null;
            }
            yield return 2;
            bool added = false;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 1)
            {
                YOffset = 8 * i;
                if (i > 0.7f && !added)
                {
                    Add(new Coroutine(ColorLerp()));
                    added = true;
                }
                yield return null;
            }
            yield return 2;
            AtBreakingPoint = true;
            yield return null;
            //Explode or something
            Exploded = true;
            player.StateMachine.State = Player.StNormal;
            player.ForceCameraUpdate = true;
            InCutscene = false;
            yield return null;
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (Exploded)
            {
                Explode(true);
            }
        }
        private IEnumerator ColorLerp()
        {
            float duration = 0.7f;
            while (!AtBreakingPoint)
            {
                for (float i = 0; i < 1; i += Engine.DeltaTime / duration)
                {
                    foreach (Sprite s in Drills)
                    {
                        s.Color = Color.Lerp(Color.White, Color.Red, Ease.SineInOut(i) / 2);
                    }
                    yield return null;
                }
                for (float i = 0; i < 1; i += Engine.DeltaTime / duration)
                {
                    foreach (Sprite s in Drills)
                    {
                        s.Color = Color.Lerp(Color.White, Color.Red, 0.5f - Ease.SineInOut(i) / 2);
                    }
                    yield return null;
                }
            }
            yield return null;
        }
        public void Explode(bool instant)
        {
            if (!instant)
            {

            }
            Talk.Enabled = false;
            Talk.Visible = false;
            foreach (Sprite s in Drills)
            {
                s.Visible = false;
            }
            foreach (Sprite s in Batteries)
            {
                s.Visible = false;
            }
            foreach (Image i in BatteryFlash)
            {
                i.Visible = false;
            }
            Box.Visible = false;
            TempExplosion.Visible = true;
        }
      
        public override void Update()
        {
            base.Update();
            if (wasIntact && Exploded)
            {
                Explode(false);
            }
            if (!InCutscene)
            {
                UpdateBatteries();
            }
            else if (Spinning)
            {
                UpdateDrills();
            }
            wasIntact = Exploded;
        }
        private void UpdateDrills()
        {
            if (Scene.OnInterval(5 / 60f))
            {
                XOffset = -XOffset;
            }
            for (int i = 0; i < 2; i++)
            {
                Drills[i].Position = initPositions[i] + new Vector2(XOffset, YOffset);
                Drills[i].Rate = 1 / SpinRate;
            }
        }

    }
}
