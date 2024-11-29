using Celeste.Mod.Core;
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/Chair")]
    [Tracked]
    public class Chair : JumpthruPlatform
    {
        public Image Image;
        public string Flag;
        private bool state;
        private bool exit;
        private bool sitting;
        public Vector2 SitOffset;
        public DotX3 Talk;
        public bool DisableTalk;
        private Coroutine secondRoutine;
        public enum SitFacings
        {
            Both,
            LeftOnly,
            RightOnly
        }
        private SitFacings facing;
        public Chair(EntityData data, Vector2 offset) : this(data.Position + offset, data.Int("depth", 1), data.Attr("path"), data.Attr("flag"), new Vector2(data.Float("sitX"), data.Float("sitY")), data.Enum<SitFacings>("canFace"))
        {
        }
        public Chair(Vector2 position, int depth, string path, string flag, Vector2 sitOffset, SitFacings facing) : base(position, 0, "default")
        {
            Depth = depth;
            Image = new Image(GFX.Game["objects/PuzzleIslandHelper/chairs/" + path]);
            Collider = new Hitbox(Image.Width, 5);
            columns = (int)(Width / 8);
            Flag = flag;
            this.SitOffset = sitOffset;
            Image.Position.Y -= sitOffset.Y;
            Position += sitOffset;
            this.facing = facing;
            Talk = new DotX3(0, -sitOffset.Y, Image.Width, Image.Height, Vector2.UnitX * Image.Width / 2, SitInteract);

            Add(Talk);
            secondRoutine = new Coroutine(false);
            Add(secondRoutine);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Components.RemoveAll<Image>();
            Add(Image);
        }
        private int playerState = Player.StDummy;
        public override void Update()
        {
            if (Scene.GetPlayer() is not Player player) return;

            state = string.IsNullOrEmpty(Flag) || SceneAs<Level>().Session.GetFlag(Flag);
            if (DisableTalk)
            {
                Talk.Enabled = false;
            }
            else if (!sitting)
            {
                Talk.Enabled = state;
            }
            if (sitting && !exit)
            {
                SetPlayerPosition(player);
            }
            base.Update();
        }
        public void SitInteract(Player player)
        {
            secondRoutine.Replace(moveToSit(player));
        }
        public void GetUp(Player player, int playerState)
        {
            sitting = false;
            exit = false;
            //todo: add player.Play("getUpFromSitting");
            Collidable = false;
            player.Collidable = true;
            player.StateMachine.State = playerState;
            player.MoveH((int)player.Facing);
            Alarm.Set(this, 0.5f, delegate { Collidable = true; });
        }
        public void GetUp(Player player)
        {
            GetUp(player, Player.StNormal);
        }
        private IEnumerator moveToSit(Player player)
        {
            sitting = true;
            player.DummyGravity = false;
            player.Collidable = false;
            player.StateMachine.State = Player.StDummy;
            player.Facing = facing switch
            {
                SitFacings.RightOnly => Facings.Right,
                SitFacings.LeftOnly => Facings.Left,
                _ => player.Facing
            };

            Vector2 sit = Image.RenderPosition + SitOffset;
            //todo: add player.Play("sitAnimation");
            yield return 0.2f;
            Vector2 from = new Vector2(player.Left, player.Bottom + 3);
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                player.Left = Calc.LerpClamp(from.X, sit.X, i);
                player.Bottom = Calc.LerpClamp(from.Y, sit.Y, Ease.CubeOut(i));
                yield return null;
            }
            player.Left = sit.X;
            player.Bottom = sit.Y;
        }
        public void InstantMoveToSeat(Player player)
        {
            sitting = true;
            exit = false;
            player.DummyGravity = false;
            player.Collidable = false;
            player.StateMachine.State = Player.StDummy;
            if (facing != SitFacings.Both)
            {
                player.Facing = facing switch
                {
                    SitFacings.RightOnly => Facings.Right,
                    SitFacings.LeftOnly => Facings.Left,
                };
            }
            SetPlayerPosition(player);
        }
        public void SetPlayerPosition(Player player)
        {
            player.BottomCenter = TopCenter;
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            if (sitting)
            {
                Player player = scene.GetPlayer();
                if (player != null)
                {
                    player.Collidable = true;
                    player.StateMachine.State = Player.StNormal;
                }
            }
        }
        public static MTexture GetTexture(string path)
        {
            return GFX.Game["objects/PuzzleIslandHelper/chairs/" + path];
        }
    }

}