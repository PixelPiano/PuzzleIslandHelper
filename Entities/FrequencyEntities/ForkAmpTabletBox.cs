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
        private ForkAmpUI ui;
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Add(new Coroutine(routine()));
        }
        private IEnumerator routine()
        {
            ui = new ForkAmpUI(null);
            Scene.Add(ui);
            while (!ui.Finished)
            {
                yield return null;
            }
            RemoveSelf();
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            ui?.RemoveSelf();
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
        private Entity faller;
        public ForkAmpTabletBox(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Depth = 1;
            Add(box = new Image(GFX.Game["objects/PuzzleIslandHelper/forkAmp/box"]));
            Add(inside = new Image(GFX.Game["objects/PuzzleIslandHelper/forkAmp/empty"]));
            Add(tabletInBox = new Image(GFX.Game["objects/PuzzleIslandHelper/forkAmp/tabletInBox"]));
            Add(plate = new Image(GFX.Game["objects/PuzzleIslandHelper/forkAmp/musicPlate"]));
            plate.Position = inside.Position = tabletInBox.Position = new Vector2(2, 6);
            Collider = new Hitbox(box.Width, box.Height);
            Add(talk = new TalkComponent(new Rectangle(0, 0, (int)box.Width, (int)box.Height), Collider.HalfSize - Vector2.UnitY * (plate.Height / 2), p =>
            {
                if (!plateOpened)
                {
                    plate.Visible = true;
                    plateOpened.State = true;
                    inside.Visible = true;
                    tabletInBox.Visible = true;
                    Add(new Coroutine(routine(p)));
                }
                else
                {
                    if (!tabletTaken)
                    {
                        plate.Visible = false;
                        inside.Visible = true;
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
                shaker.ShakeFor(0.4f);
                PrepareForTalk();
            }
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (tabletTaken)
            {
                plate.Visible = false;
                inside.Visible = true;
                tabletInBox.Visible = false;
                talk.Enabled = false;
                boxShook.State = true;
                plateOpened.State = true;
            }
            else if (plateOpened)
            {
                plate.Visible = false;
                inside.Visible = true;
                tabletInBox.Visible = true;
                boxShook.State = true;
                talk.Enabled = true;
            }
            else if (boxShook)
            {
                plate.Visible = true;
                plate.JustifyOrigin(0, 1);
                plate.Y += plate.Height;
                plate.Rotation = 5f.ToRad();
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
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            faller?.RemoveSelf();
        }
        private IEnumerator routine(Player player)
        {
            faller = new Entity(Position + new Vector2(2, 6));
            faller.Collider = new Hitbox(plate.Width, (int)(plate.Height * 0.7f));
            Scene.Add(faller);
            player.DisableMovement();
            shaker.ShakeFor(0.4f);
            yield return 0.4f;
            yield return 0.8f;
            float speed = 0;
            float plateOrig = plate.Y;
            float orig = faller.Y;
            while (!faller.CollideCheck<Solid>(faller.Position + Vector2.UnitY * (speed * Engine.DeltaTime)))
            {
                faller.Y += speed * Engine.DeltaTime;
                plate.Y = plateOrig + (faller.Y - orig);
                speed = Calc.Approach(speed, 160f, 900f * Engine.DeltaTime);
                yield return null;
            }
            faller.RemoveSelf();
            yield return Engine.DeltaTime * 7;
            for (int i = 7; i > -1; i--)
            {
                plate.Visible = false;
                yield return Engine.DeltaTime * i;
                plate.Visible = true;
                yield return Engine.DeltaTime * i;
            }
            plate.Visible = false;
            yield return 0.7f;
            player.EnableMovement();

            /*            float offset = MathHelper.PiOver2;
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
                        plate.Rotation = MathHelper.PiOver2;*/
        }

    }

}