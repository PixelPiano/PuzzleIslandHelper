//PuzzleIslandHelper.CustomWater
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/CustomWater")]
    [TrackedAs(typeof(Water))]
    public class CustomWater : Water
    {
        private string flag;
        private bool inverted;
        public bool State
        {
            get
            {
                if (string.IsNullOrEmpty(flag))
                {
                    return true;
                }
                bool flagState = SceneAs<Level>().Session.GetFlag(flag);
                return inverted ? !flagState : flagState;
            }
        }
        public CustomWater(EntityData data, Vector2 offset)
            : base(data.Position + offset, data.Bool("topSurface", true), data.Bool("hasBottom"), data.Width, data.Height)
        {
            Get<DisplacementRenderHook>().RenderDisplacement = RenderDisplacementFlagged;
            inverted = data.Bool("inverted");
            flag = data.Attr("flag");
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            CheckState();
        }

        public override void Update()
        {
            if (!CheckState())
            {
                return;
            }
            foreach (Surface surface in Surfaces)
            {
                surface.Update();
            }

            foreach (WaterInteraction component in Scene.Tracker.GetComponents<WaterInteraction>())
            {
                Rectangle bounds = component.Bounds;
                bool flag = contains.Contains(component);
                bool flag2 = CollideRect(bounds);
                if (flag != flag2)
                {
                    if (bounds.Center.Y <= Center.Y && TopSurface != null)
                    {
                        TopSurface.DoRipple(bounds.Center.ToVector2(), 1f);
                    }
                    else if (bounds.Center.Y > Center.Y && BottomSurface != null)
                    {
                        BottomSurface.DoRipple(bounds.Center.ToVector2(), 1f);
                    }

                    bool flag3 = component.IsDashing();
                    int num = bounds.Center.Y < Center.Y && !Scene.CollideCheck<Solid>(bounds) ? 1 : 0;
                    if (flag)
                    {
                        if (flag3)
                        {
                            Audio.Play("event:/char/madeline/water_dash_out", bounds.Center.ToVector2(), "deep", num);
                        }
                        else
                        {
                            Audio.Play("event:/char/madeline/water_out", bounds.Center.ToVector2(), "deep", num);
                        }

                        component.DrippingTimer = 2f;
                    }
                    else
                    {
                        if (flag3 && num == 1)
                        {
                            Audio.Play("event:/char/madeline/water_dash_in", bounds.Center.ToVector2(), "deep", num);
                        }
                        else
                        {
                            Audio.Play("event:/char/madeline/water_in", bounds.Center.ToVector2(), "deep", num);
                        }

                        component.DrippingTimer = 0f;
                    }

                    if (flag)
                    {
                        contains.Remove(component);
                    }
                    else
                    {
                        contains.Add(component);
                    }
                }

                if (BottomSurface == null)
                {
                    continue;
                }

                Entity entity = component.Entity;
                if (!(entity is Player))
                {
                    continue;
                }

                if (flag2 && entity.Y > Bottom - 8f)
                {
                    if (playerBottomTension == null)
                    {
                        playerBottomTension = BottomSurface.SetTension(entity.Position, 0f);
                    }

                    playerBottomTension.Position = BottomSurface.GetPointAlong(entity.Position);
                    playerBottomTension.Strength = Calc.ClampedMap(entity.Y, Bottom - 8f, Bottom + 4f);
                }
                else if (playerBottomTension != null)
                {
                    BottomSurface.RemoveTension(playerBottomTension);
                    playerBottomTension = null;
                }
            }
        }
        private bool CheckState()
        {
            bool output = State;
            if (output)
            {
                Visible = true;
                Collidable = true;
            }
            else
            {
                Visible = false;
                Collidable = false;
            }
            return output;
        }
        public void RenderDisplacementFlagged()
        {
            if (State)
            {
                RenderDisplacement();
            }
        }
    }
}
