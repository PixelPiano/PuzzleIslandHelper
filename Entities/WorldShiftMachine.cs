using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities.Programs;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/WorldShiftMachine")]
    [Tracked]
    public class WorldShiftMachine : Entity
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
        public Hologlobe Hologlobe;
        public DotX3 Talk;

        private const float maxLightDelay = 0.06f;
        public WorldShiftMachine(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Collider = new Hitbox(WIDTH, HEIGHT);
            Depth = 2;
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

        public void Interact(Player player)
        {
            if (Scene is not Level level) return;
            Add(new Coroutine(sequence(player, level)));
        }
        public IEnumerator sequence(Player player, Level level)
        {
            player.StateMachine.State = Player.StDummy;
            Vector2 from = level.Camera.Position;
            Vector2 to = level.Camera.Position + Vector2.UnitX * 80;
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                level.Camera.Position = Vector2.Lerp(from, to, Ease.CubeOut(i));
                yield return null;
            }
            FakeTerminal t = new FakeTerminal(level.Camera.Position + new Vector2(162, 2), 150, 80);
            Scene.Add(t);
            while (t.TransitionAmount < 1)
            {
                yield return null;
            }
            HologramProgram program = new HologramProgram(t);
            Scene.Add(program);

            while (t.TransitionAmount > 0)
            {
                yield return null;
            }
            to = from;
            from = level.Camera.Position;
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                level.Camera.Position = Vector2.Lerp(from, to, Ease.CubeOut(i));
                yield return null;
            }
            yield return 0.1f;
            player.StateMachine.State = Player.StNormal;
            yield return null;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(Platform);
            int size = 80;
            scene.Add(Hologlobe = new Hologlobe(
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
        public IEnumerator Activate()
        {
            yield return Hologlobe.FadeInRoutine();
        }
        public IEnumerator Deactivate()
        {
            yield return Hologlobe.FadeOutRoutine();
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Platform.RemoveSelf();
            Hologlobe.RemoveSelf();
        }
    }

}