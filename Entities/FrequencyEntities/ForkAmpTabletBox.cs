using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Components.Visualizers.DSPs;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.FrequencyEntities
{
    public class ForkAmpTabletTest : Entity
    {
        [Command("test_ui", "")]
        public static void TestUI()
        {
            Engine.Scene.Add(new ForkAmpTabletTest());
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Add(new Coroutine(routine()));
        }
        private IEnumerator routine()
        {
            ForkAmpUI ui = new ForkAmpUI(null);
            Scene.Add(ui);
            while (!ui.Finished)
            {
                yield return null;
            }
            RemoveSelf();
        }
    }
    [CustomEntity("PuzzleIslandHelper/ForkAmpTablet")]
    [Tracked]
    public class ForkAmpTabletBox : Entity
    {
        private Image box, plate, tabletInBox, inside;
        public static FlagList plateOpened = new FlagList("ForkAmpTabletBoxPlateOpened");
        public static FlagList boxShook = new FlagList("ForkAmpTabletBoxShook");
        public static FlagList tabletTaken = new FlagList("TabletPlundered");
        private TalkComponent talk;
        private BetterShaker shaker;
        public ForkAmpTabletBox(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Depth = 1;
            Add(box = new Image(GFX.Game["objects/PuzzleIslandHelper/forkAmp/box"]));
            Add(inside = new Image(GFX.Game["objects/PuzzleIslandHelper/forkAmp/empty"]));
            Add(tabletInBox = new Image(GFX.Game["objects/PuzzleIslandHelper/forkAmp/tabletInBox"]));
            Add(plate = new Image(GFX.Game["objects/PuzzleIslandHelper/forkAmp/musicPlate"]));
            plate.Position = inside.Position = tabletInBox.Position = new Vector2(2, 6);
            Collider = new Hitbox(box.Width, box.Height);
            Add(talk = new TalkComponent(new Rectangle(0, 0, (int)box.Width, (int)box.Height), Vector2.UnitY * Width / 2, p =>
            {
                if (!plateOpened)
                {
                    plateOpened.State = true;
                    inside.Visible = true;
                    tabletInBox.Visible = true;
                    Add(new Coroutine(routine()));
                }
                else
                {
                    if (!tabletTaken)
                    {
                        tabletTaken.State = true;
                        talk.Enabled = false;
                        tabletInBox.Visible = false;
                    }
                }
            }));
            talk.Enabled = false;
            Add(shaker = new BetterShaker(v =>
            {
                plate.Position += v;
            }));
        }
        public void PrepareForTalk()
        {
            plate.JustifyOrigin(0, 1);
            if (plate.Y == 6)
            {
                plate.Position.Y += plate.Height;
            }
            if (plate.Rotation == 0)
            {
                plate.Rotation = 5f.ToRad();
            }
            boxShook.State = true;
            talk.Enabled = true;
        }
        public void Shake()
        {
            if (!boxShook)
            {
                shaker.StartShaking(0.4f);
                PrepareForTalk();
            }
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (tabletTaken)
            {
                plate.JustifyOrigin(0, 1);
                plate.Y += plate.Height;
                plate.Rotation = MathHelper.PiOver2;
                inside.Visible = true;
                tabletInBox.Visible = false;
                talk.Enabled = false;
                boxShook.State = true;
                plateOpened.State = true;
            }
            else if (plateOpened || boxShook)
            {
                plate.JustifyOrigin(0, 1);
                plate.Y += plate.Height;
                plate.Rotation = MathHelper.PiOver2;
                inside.Visible = true;
                tabletInBox.Visible = true;
                boxShook.State = true;
                talk.Enabled = true;
            }
            else
            {
                inside.Visible = false;
                tabletInBox.Visible = false;
            }
        }
        private IEnumerator routine()
        {
            float offset = MathHelper.PiOver2;
            float maxSpeed = 10f;
            float speed = maxSpeed;
            float speedTarget = speed;
            float prevRot = plate.Rotation;
            while (Math.Abs(speedTarget) > 5f)
            {
                plate.Rotation += speed * Engine.DeltaTime;
                speed = Calc.Approach(speed, speedTarget, 200f * Engine.DeltaTime);
                
                if (Math.Sign(prevRot - offset) != Math.Sign(plate.Rotation - offset))
                {
                    speedTarget *= -0.5f;
                }
                prevRot = plate.Rotation;
                yield return null;
            }
            plate.Rotation = MathHelper.PiOver2;
        }
    }

}