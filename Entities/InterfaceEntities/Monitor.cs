using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities
{
    public class Monitor : Entity
    {
        public Sprite Sprite;
        public float Alpha;
        public Interface Parent;
        public Image Cover;
        public float CoverAlpha;
        public bool TurningOff => Sprite.CurrentAnimationID == "turnOff";
        public bool Idle => Sprite.CurrentAnimationID == "idle";
        public bool StartingUp => Sprite.CurrentAnimationID == "boot";
        public void SetColor(Color color)
        {
            Sprite.SetColor(color);
        }
        public Monitor(Color color, Interface parent) : base()
        {
            Depth = Interface.BaseDepth;
            Sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/interface/");
            Cover = new Image(GFX.Game["objects/PuzzleIslandHelper/interface/cover00"]);
            Add(Cover);
            Sprite.AddLoop("idle", "idle", 1f);
            Sprite.AddLoop("off", "off", 0.1f);
            Sprite.Add("boot", "startUp", 0.07f, "idle");
            Sprite.Add("turnOff", "shutDown", 0.07f, "off");
            Sprite.SetColor(color);
            Cover.Color = Color.Transparent;
            Add(Sprite);
            Sprite.Play("off");
            Collider = Sprite.Collider();
            Parent = parent;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
        }
        public override void Render()
        {
            if (Parent.ForceHide) return;
            if (Alpha > 0)
            {
                Draw.Rect(Collider, Color.Black * Alpha);
            }
            base.Render();
        }
        public void StartUp(bool fast = false)
        {
            Sprite.Play(fast ? "idle" : "boot");
        }
        public void TurnOff()
        {
            Sprite.Play("turnOff");
        }
        public override void Update()
        {
            base.Update();
            Collider.Width = Sprite.Width;
            Collider.Height = Sprite.Height;
            CoverAlpha = Sprite.CurrentAnimationID switch
            {
                "idle" => Calc.Approach(CoverAlpha, 1, Engine.DeltaTime),
                "turnOff" => 0,
                _ => Calc.Approach(CoverAlpha, 0, Engine.DeltaTime)
            };
            Cover.SetColor(Sprite.Color * CoverAlpha);
        }
    }

}