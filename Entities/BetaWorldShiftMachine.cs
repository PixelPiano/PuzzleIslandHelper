using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.Programs;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/WorldShiftMachine")]
    [Tracked]
    public class BetaWorldShiftMachine : InterfaceMachine
    {
        public const int WIDTH = 128;
        public const int HEIGHT = 68;
        public Sprite Lights;
        public Sprite MiddleBeam;
        public Sprite Battery;
        public Sprite Screen;
        public Sprite Roof;
        public Sprite Core;
        public Image Base;
        public Image Pillars;
        public Platform Platform;
        public Hologlobe Hologram;

        public WorldShiftProgram Program;
        private const float maxLightDelay = 0.06f;
        public BetaWorldShiftMachine(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Collider = new Hitbox(WIDTH, HEIGHT);
            Depth = 2;
            BackgroundColor = Color.Blue;
            string path = "objects/PuzzleIslandHelper/worldShift/";
            Add(Roof = new Sprite(GFX.Game, path));
            Add(Pillars = new Image(GFX.Game[path + "pillars"]));
            Add(MiddleBeam = new Sprite(GFX.Game, path));
            Add(Battery = new Sprite(GFX.Game, path));
            Add(Core = new Sprite(GFX.Game, path));
            Add(Screen = new Sprite(GFX.Game, path));
            Add(Base = new Image(GFX.Game[path + "base"]));

            Battery.Position = new Vector2(14, 26);
            Screen.Position = new Vector2(50, 42);
            Core.Position = new Vector2(51, 18);
            MiddleBeam.Position.X = 47;
            MiddleBeam.Position.Y--;
            Base.Position.Y = Height - Base.Height;

            Roof.AddLoop("idle", "roof", 0.1f);
            MiddleBeam.AddLoop("off", "middleBeam", 0.1f, 0);
            MiddleBeam.AddLoop("on", "middleBeam", 0.1f);
            Battery.AddLoop("off", "battery", 0.1f, 0);
            Battery.AddLoop("on", "battery", 0.1f, 7);
            Battery.Add("start", "battery", 0.1f, "on");
            Core.AddLoop("off", "core", 0.1f, 0);
            Core.AddLoop("flicker", "core", 0.1f, 10, 11);
            Core.Add("intro", "core", 0.1f, "flicker");
            Screen.AddLoop("idle", "screen", 0.1f);


            Roof.Play("idle");
            MiddleBeam.Play("off");
            Battery.Play("off");
            Screen.Play("idle");
            Core.Play("off");

            Add(Talk = new DotX3(Screen.X, Screen.Y, Screen.Width, Screen.Height, new Vector2(Screen.X + Screen.Width / 2, 0), Interact));
            Talk.PlayerMustBeFacing = false;
            Platform = new JumpThru(Base.RenderPosition, (int)Base.Width, true);
        }
        public override void Update()
        {
            base.Update();
        }
        public override IEnumerator OnBegin(Player player, Level level)
        {
            SetSessionInterface();
            Interface.StartPreset("WorldShift");
            Program = Interface.GetProgram("WorldShift") as WorldShiftProgram;
            Program.SetProgress(WorldShiftProgram.Progress.Off);
            yield return null;
        }
        public override void Interact(Player player)
        {
            base.Interact(player);
        }
        public void Launch()
        {

        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(Platform);
            int size = 80;
            scene.Add(Hologram = new Hologlobe(
                position: Position + new Vector2(Width / 2 - (size / 2), -(size + 16)),
                width: size,
                height: size,
                baseColor: Color.Blue,
                glitchColor: Color.Blue,
                boxColor: Color.Red,
                front: Color.White,
                back: Color.DarkBlue * 0.5f));

        }
        public bool InSequence;
        public void Activate()
        {
            if (InSequence) return;
            Add(new Coroutine(sequence()));
        }
        private IEnumerator sequence()
        {
            Interface.InControl = false;
            yield return Interface.FlickerHide();
            InSequence = true;
            Hologram.Active = false;
            Battery.Play("start");
            Core.Play("intro");
            while (Core.CurrentAnimationID != "flicker")
            {
                yield return null;
            }
            MiddleBeam.Play("on");
            while (MiddleBeam.CurrentAnimationFrame < MiddleBeam.CurrentAnimationTotalFrames / 2)
            {
                yield return null;
            }
            for (int i = 0; i < 3; i++)
            {
                Hologram.Visible = true;
                yield return i * Engine.DeltaTime;
                Hologram.Visible = false;
                yield return Calc.Random.Range(0, i * 0.2f);
            }
            Hologram.Active = true;
            Hologram.Visible = true;
            //Program.SetProgress(WorldShiftProgram.Progress.Activated);
            yield return 1;
            yield return Interface.FlickerReveal();
            Interface.InControl = true;
            yield return null;
            InSequence = false;
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Platform.RemoveSelf();
            Hologram.RemoveSelf();
        }
    }
}