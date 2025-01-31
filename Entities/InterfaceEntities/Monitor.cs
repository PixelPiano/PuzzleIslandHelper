using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities
{
    public class Monitor : Entity
    {
        public Color ScreenColor = Color.White;
        public Sprite Sprite;
        public float Alpha;
        public Interface Parent;
        public Image Cover;
        public float CoverAlpha;
        public bool TurningOff => Sprite.CurrentAnimationID == "turnOff";
        public bool Idle => Sprite.CurrentAnimationID == "idle";
        public bool StartingUp => Sprite.CurrentAnimationID == "boot";

        public List<Entity> Entities = [];
        public VirtualRenderTarget Target;
        public Monitor(Color color, Interface parent) : base()
        {
            Depth = Interface.BaseDepth - 1;
            Parent = parent;
            Target = VirtualContent.CreateRenderTarget("Monitor", 320, 180);
            Add(new BeforeRenderHook(BeforeRender));
            Sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/interface/");
            Cover = new Image(GFX.Game["objects/PuzzleIslandHelper/interface/cover00"]);
            Add(Cover);
            Sprite.AddLoop("idle", "idle", 1f);
            Sprite.AddLoop("off", "off", 0.1f);
            Sprite.Add("boot", "startUp", 0.07f, "idle");
            Sprite.Add("turnOff", "shutDown", 0.07f, "off");
            Cover.Color = Sprite.Color = color;
            Cover.Visible = Sprite.Visible = false;
            Add(Sprite);
            Sprite.Play("off");
            Collider = Sprite.Collider();
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Target?.Dispose();
        }
        public void BeforeRender()
        {

            Target.SetAsTarget(Color.Transparent);
            if (Scene is not Level level) return;
            if (!Parent.ForceHide)
            {
                Draw.SpriteBatch.StandardBegin(level.Camera.Matrix);
                Draw.Rect(Collider, Color.Black);
                //Cover.Render();
                Sprite.Render();
                foreach (InterfaceEntity e in Scene.Tracker.GetEntities<InterfaceEntity>().OrderByDescending(item => item.Depth))
                {
                    if (e.Visible)
                    {
                        e.InterfaceRender();
                    }
                }
                Draw.SpriteBatch.End();
            }
        }
        public override void Render()
        {
            if (Scene is not Level level || Alpha <= 0) return;
            Draw.SpriteBatch.Draw(Target, level.Camera.Position, ScreenColor * Alpha);
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
        }
        public void StartAnimation(bool fast = false)
        {
            Sprite.Play(fast ? "idle" : "boot");
        }
        public void EndAnimation()
        {
            Sprite.Play("turnOff");
        }
        public override void Update()
        {
            base.Update();
            Collider.Width = Sprite.Width;
            Collider.Height = Sprite.Height;
            /*          CoverAlpha = Sprite.CurrentAnimationID switch
                      {
                          "idle" => Calc.Approach(CoverAlpha, 1, Engine.DeltaTime),
                          "turnOff" => 0,
                          _ => Calc.Approach(CoverAlpha, 0, Engine.DeltaTime)
                      };*/
            //Cover.SetColor(Sprite.Color);
        }
    }

}