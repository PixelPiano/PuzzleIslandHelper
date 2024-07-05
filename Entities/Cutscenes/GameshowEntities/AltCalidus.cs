using Celeste.Mod.Entities;
using Celeste.Mod.LuaCutscenes;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Cutscenes.GameshowEntities
{
    [CustomEntity("PuzzleIslandHelper/AltCalidus")]
    [Tracked]
    public class AltCalidus : Entity
    {
        public Sprite Machine;
        public VertexLight Light;
        public TalkComponent Talk;
        private AltCalidusScene Cutscene;
        public AltCalidus(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            string path = "objects/PuzzleIslandHelper/gameshow/host/";
            Depth = 2;
            Add(Machine = new Sprite(GFX.Game, path));
            Machine.AddLoop("idle", "machine", 0.1f);

            Machine.Play("idle");
            Collider = new Hitbox(Machine.Width, Machine.Height);
            Add(Light = new VertexLight(Color.White, 1, 32, 64));
            Light.Position += Vector2.One * 24;
            Add(Talk = new TalkComponent(new Rectangle(0, 0, (int)Machine.Width, (int)Machine.Height), new Vector2(Width / 2, 0), Interact));
        }
        public void Interact(Player player)
        {
            if (Cutscene != null) return;
            SceneAs<Level>().Add(Cutscene = new AltCalidusScene(player, this));
        }

        public class AltCalidusScene : CutsceneEntity
        {
            public Player Player;
            public AltCalidus Washer;
            public enum States
            {
                BeforeGameshow,
                AfterGameshow,
                Inactive
            }
            public States State => PianoModule.Session.AltCalidusSceneState;
            public AltCalidusScene(Player player, AltCalidus washer) : base()
            {
                Player = player;
                Washer = washer;
            }

            public override void OnBegin(Level level)
            {
                Player.StateMachine.State = Player.StDummy;
                Add(new Coroutine(Cutscene()));
            }
            public override void OnEnd(Level level)
            {
                if (WasSkipped && State == States.BeforeGameshow)
                {
                    Level.Session.SetFlag("gameStart");
                    Player.X = Washer.Right + 8;
                }
                Player.StateMachine.State = Player.StNormal;
                level.ResetZoom();
            }
            private IEnumerator movePlayer()
            {
                yield return Player.DummyWalkTo(Washer.Right + 8);
                Player.Facing = Facings.Left;
            }
            public void WipeFaces()
            {
                foreach (AudienceMember member in Scene.Tracker.GetEntities<AudienceMember>())
                {
                    member.Die();
                }
                Level.Session.SetFlag("FacesWiped");
            }
            private IEnumerator Cutscene()
            {
                Coroutine r = new Coroutine(movePlayer());
                Add(r);
                Level.Session.SetFlag("gameStart", false);
                if (Level.Session.GetFlag("washerNoResponse"))
                {
                    yield return Textbox.Say("machineNoMore");
                }
                else
                {
                    Vector2 zoomTo = Washer.Center - Vector2.UnitY * 28;
                    yield return Level.ZoomTo(zoomTo - Level.Camera.Position, 1.5f, 1);
                    while (!r.Finished)
                    {
                        yield return null;
                    }
                    Player.Facing = Facings.Left;
                    yield return null;

                    switch (State)
                    {
                        case States.Inactive:
                            break;
                        case States.BeforeGameshow:
                            yield return Textbox.Say("verify");
                            yield return 0.1f;
                            Level.Session.SetFlag("gameStart");
                            Player.StateMachine.State = Player.StNormal;
                            break;
                        case States.AfterGameshow:
                            yield return Textbox.Say("faceWipe1");
                            yield return ChoicePrompt.Prompt("madelineSayNoWipe", "madelineSayYesWipe");
                            if (ChoicePrompt.Choice == 1)
                            {
                                yield return Textbox.Say("yesWipe");
                                yield return 1;
                                WipeFaces();
                                yield return Textbox.Say("yesWipe3");
                                Level.Session.SetFlag("washerNoResponse");
                            }
                            else
                            {
                                yield return Textbox.Say("noWipe");

                            }
                            break;
                    }
                    yield return Level.ZoomBack(1);
                }

                EndCutscene(Level);

            }
        }

        public override void Update()
        {
            base.Update();
            Talk.Enabled = Cutscene is null;
        }
    }
}
