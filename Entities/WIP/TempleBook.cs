using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.WIP
{
    [CustomEntity("PuzzleIslandHelper/TempleBook")]
    [Tracked]
    [Obsolete]
    public class TempleBook : Entity
    {
        public Image Book;
        public DotX3 Talk;
        private string flag;
        private EntityID id;
        private VertexLight Light;
        private bool fading;
        
        public TempleBook(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
        {
            Depth = 1;
            flag = data.Attr("flag");
            Add(Book = new Image(GFX.Game["objects/PuzzleIslandHelper/redBook"]));
            Add(Light = new VertexLight(Color.White, 0.5f, 32, 64));
            Collider = new Hitbox(Book.Width, Book.Height);
            Add(Talk = new DotX3(0, 0, Width, Height + 12, Vector2.UnitX * Width / 2, Interact));
            this.id = id;
        }
        public void PickUp()
        {
            Book.Visible = false;
            fading = true;
            Light.Alpha += 0.3f;
        }
        public override void Update()
        {
            base.Update();
            if (fading)
            {
                Light.Alpha = Calc.Approach(Light.Alpha, 0, Engine.DeltaTime / 7f);
            }
        }
        public void Interact(Player player)
        {
            player.StateMachine.State = Player.StDummy;
            SceneAs<Level>().Session.DoNotLoad.Add(id);
            Scene.Add(new Cutscene(this, flag));
            Talk.Enabled = false;
            Talk.Visible = false;

        }
        public class Cutscene : CutsceneEntity
        {
            private string flag;
            private TempleBook book;
            public Cutscene(TempleBook book, string flag)
            {
                this.book = book;
                this.flag = flag;
            }
            public override void OnBegin(Level level)
            {
                if (level.GetPlayer() is Player player)
                {
                    Add(new Coroutine(cutscene(player)));
                }

            }
            private IEnumerator cutscene(Player player)
            {
                player.StateMachine.State = Player.StDummy;
                Vector2 zoomTo = book.Center - Vector2.UnitY * 28;
                Coroutine zoom;
                Add(zoom = new Coroutine(Level.ZoomTo(zoomTo - Level.Camera.Position, 1.5f, 1)));

                //player walk to book
                yield return player.DummyWalkTo(book.X);
                player.Facing = Facings.Right;
                while (!zoom.Finished) yield return null;

                //player pick up book
                book.PickUp();
                yield return Textbox.Say("templeBookA");
                //dialogue: Oh, this must be Calidus' book!
                //...I wonder why this is so important to him?
                //Well, time to head back!
                yield return Level.ZoomBack(1);
                EndCutscene(Level);
            }

            public override void OnEnd(Level level)
            {
                level.Session.SetFlag(flag);
                if (level.GetPlayer() is not Player player) return;
                player.StateMachine.State = Player.StNormal;
                Level.ResetZoom();
                book.Visible = false;
            }
        }
    }
    [CustomEntity("PuzzleIslandHelper/TempleBookCutscene")]
    public class TempleBookRevealCutscene : Trigger
    {
        public string Flag;
        public string TorchFlag;
        public bool AddedCutscene;
        private EntityID id;
        public TempleBookRevealCutscene(EntityData data, Vector2 offset, EntityID id) : base(data, offset)
        {
            Flag = data.Attr("flag");
            this.id = id;
            TorchFlag = data.Attr("torchFlag");
        }
        private void CheckAndAddCutscene(Player player)
        {
            if (!AddedCutscene && (string.IsNullOrEmpty(Flag) || SceneAs<Level>().Session.GetFlag(Flag)) && player.Speed.X < 0)
            {
                Scene.Add(new Cutscene(this));
                AddedCutscene = true;
                SceneAs<Level>().Session.DoNotLoad.Add(id);
            }
        }
        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            CheckAndAddCutscene(player);
        }
        public override void OnStay(Player player)
        {
            base.OnStay(player);
            CheckAndAddCutscene(player);
        }
        public class Cutscene : CutsceneEntity
        {
            private string flag;
            private string torchFlag;
            public Cutscene(TempleBookRevealCutscene trigger) : base()
            {
                flag = trigger.Flag;
                torchFlag = trigger.TorchFlag;
            }
            public override void OnBegin(Level level)
            {
                if (level.GetPlayer() is Player player)
                {
                    Add(new Coroutine(cutscene(player)));
                }

            }
            private IEnumerator waitForShaking()
            {
                while (Level.shakeTimer > 0)
                {
                    yield return null;
                }
            }
            private IEnumerator turnOnTorches()
            {
                Level.Session.SetFlag(torchFlag);
                yield return null;
                Level.Lighting.Alpha = Level.BaseLightingAlpha;
            }
            private IEnumerator wait()
            {
                yield return 1;
            }
            private IEnumerator getUp()
            {
                yield return null;
            }
            private IEnumerator cutscene(Player player)
            {
                player.StateMachine.State = Player.StDummy;
                yield return player.DummyWalkTo(Level.Marker("playerWalkTo").X);
                Level.Shake(1);
                //player fall over
                //player.Play("fallDown");

                yield return Textbox.Say("templeBookB", waitForShaking, turnOnTorches, wait, getUp);
                //dialogue: Ah!
                //Grrr...
                //{waitForShaking}
                //You done with your tantrum? Good.
                //{turnOnTorches}
                //*torches flicker on*
                //huh?
                //W-wow...
                EndCutscene(Level);
            }

            public override void OnEnd(Level level)
            {
                if (level.GetPlayer() is Player player)
                {
                    player.StateMachine.State = Player.StNormal;
                }
            }
        }
    }


}