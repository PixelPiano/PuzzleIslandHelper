using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using FMOD;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
namespace Celeste.Mod.PuzzleIslandHelper.Entities.FrequencyEntities
{
    //[CustomEntity("PuzzleIslandHelper/WipEntity")]
    public class TestEntity : Entity
    {
        public float x, y, w, h;
        public Color Color = Color.Red;
        private Envelope.States state;
        private float ease;
        public TestEntity(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Collider = new Hitbox(16,16);
            x = X; y = Y; w = h = 16;
            Envelope env = new Envelope(1, 1, 1, 1, false, true, Envelope.Modes.Looping, Ease.SineIn, Ease.SineOut);
            env.OnUpdate = (e) =>
            { 
                ease = e.Eased;
                state = e.State;
                float eased = e.Eased;
                switch (e.State)
                {
                    case Envelope.States.Attack:
                        x = Calc.LerpClamp(X, X - 8, eased);
                        y = Calc.LerpClamp(Y, Y - 8, eased);
                        w = h = Calc.LerpClamp(16, 32, eased);
                        break;
                    case Envelope.States.Sustain:
                        Color = Color.Lerp(Color.Red, Color.Lime, env.TimerPercent);
                        break;
                    case Envelope.States.Release:
                        x = Calc.LerpClamp(X - 8, X, 1 - eased);
                        y = Calc.LerpClamp(Y - 8, Y, 1 - eased);
                        w = h = Calc.LerpClamp(32, 16, 1 - eased);
                        break;
                    case Envelope.States.Delay:
                        Color = Color.Lerp(Color.Lime, Color.Red, env.TimerPercent);
                        break;
                }
            };
            Add(env);
        }
        public override void Render()
        {
            base.Render();
            Draw.Rect(x, y, w, h, Color);
        }
    }
    [CustomEntity("PuzzleIslandHelper/ForkAmpSpeaker")]
    [Tracked]
    public class ForkAmpSpeaker : Entity
    {
        public DotX3 Talk;
        public Sprite Screen;
        public ImageSwap Stand;
        public FlagList Flags;
        public float[] targetRates = new float[4];
        public Vector2 SpeakerPosition;
        private string facing;
        private EntityID id;
        private bool requiresLabPower;
        public ForkAmpSpeaker(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
        {
            requiresLabPower = data.Bool("requiresLabPower");
            Depth = 1;
            targetRates =
            [
                data.Int("rateA"),
                data.Int("rateB"),
                data.Int("rateC"),
                data.Int("rateD")
            ];
            SpeakerPosition = data.NodesOffset(offset)[0] - Position;
            Flags = data.FlagList("flag");
            facing = data.Attr("facing", "Right");
            this.id = id;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Add(Stand = new ImageSwap(GFX.Game, "objects/PuzzleIslandHelper/forkAmp/wirelessStand", false));
            Add(Screen = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/forkAmp/wirelessDisplay"));
            Screen.AddLoop("off", "", 0.1f, 0);
            Screen.Add("pushButton", "", 0.2f, "activate", 1, 1);
            Screen.Add("activate", "", 0.1f, "idle", 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12);
            Screen.Add("idle", "", 0.5f, "idleShine", 13, 13);
            Screen.Add("idleShine", "", 0.1f, "idle", 14, 15, 16, 17, 18, 19, 20, 21, 22);
            Screen.Play("off");
            Screen.Position = Vector2.One * 3;

            Collider = Stand.Collider();
            Add(Talk = new DotX3(Collider, Interact));
            Image image = new Image(GFX.Game["objects/PuzzleIslandHelper/forkAmp/isolatedSpeaker"]);
            //Add(image);
            image.Position = SpeakerPosition;
            if (facing == "Left")
            {
                image.Effects = SpriteEffects.FlipHorizontally;
            }
            Talk.Enabled = !(scene as Level).Session.GetFlag("ForkAmpSpeakerUsed:" + id.ToString());
        }
        public void SetUsed()
        {
            SceneAs<Level>().Session.SetFlag("ForkAmpSpeakerUsed:" + id.ToString());
        }
        public void Interact(Player player)
        {
            player.StateMachine.State = Player.StDummy;
            Add(new Coroutine(Routine(player)));
        }
        private SpeakerProgram program;
        public bool Running;
        public override void Update()
        {
            base.Update();
            Talk.Enabled = !SceneAs<Level>().Session.GetFlag("ForkAmpSpeakerUsed:" + id.ToString());
        }
        public IEnumerator Routine(Player player)
        {
            player.DisableMovement();
            if (requiresLabPower && !PianoModule.Session.RestoredPower)
            {
                yield return Textbox.Say("FrequencyScreenOff");
                player.EnableMovement();
                yield break;
            }

            Running = true;
            yield return ActivateScreen();
            Vector2 zoomPosition = Screen.RenderPosition + new Vector2(Screen.Width / 2f, 3.5f);
            yield return SceneAs<Level>().ZoomToWorld(zoomPosition, 180f, 2);
            program = new SpeakerProgram(targetRates, true);
            Scene.Add(program);
            yield return null;
            SceneAs<Level>().ResetZoom();
            while (!program.Finished)
            {
                yield return null;
            }
            SceneAs<Level>().ZoomSnapWorld(zoomPosition, 180f);
            yield return SceneAs<Level>().ZoomBack(2);
            player.EnableMovement();

            Screen.Play("off");
            Running = false;
        }
        private IEnumerator ActivateScreen()
        {
            Screen.Play("pushButton");
            while (Screen.CurrentAnimationID != "idle")
            {
                yield return null;
            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            if (Running)
            {
                (scene as Level).ResetZoom();
                program?.RemoveSelf();
                (scene as Level).EnableMovement();
                Screen.Play("off");
                Running = false;
            }
        }

    }

}
