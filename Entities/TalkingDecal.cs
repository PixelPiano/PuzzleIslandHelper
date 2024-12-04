using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/TalkingDecal")]
    [Tracked]
    public class TalkingDecal : Entity
    {
        public Sprite sprite;
        public Image image;
        public bool Outline;

        public enum TalkModes
        {
            Teleport,
            Dialog,
            Cutscene
        }
        public enum UseModes
        {
            DontUse,
            Use,
            UseAndWaitFor
        }
        public enum ZoomModes
        {
            Screen,
            World
        }
        public enum CameraModes
        {
            World, Naive
        }
        public enum WalkModes
        {
            World, Naive
        }
        public ZoomModes ZoomMode;
        public CameraModes CamMode;
        public WalkModes WalkMode;
        public UseModes ZoomUse, WalkUse, CameraUse;
        private float zoomTime, walkX, camTime, zoomAmount, walkMult, cameraDelay;
        private bool walkBackwards, walkIntoWalls;
        private Vector2 camTo, zoomTo;
        public TalkModes TalkMode;
        private Player.IntroTypes introType;
        public Vector2? NearestSpawn;
        public string String;
        public TalkComponent Talk;
        public string VisibleFlag;
        public bool InvertVisibleFlag;
        public string TalkFlag;
        public bool InvertTalkFlag;
        public bool IsVisible => FlagState(VisibleFlag) != InvertVisibleFlag;
        public bool TalkEnabled => FlagState(TalkFlag) != InvertTalkFlag;
        public bool FlagState(string flag)
        {
            return string.IsNullOrEmpty(flag) || SceneAs<Level>().Session.GetFlag(flag);
        }
        public TalkingDecal(EntityData data, Vector2 offset)
        : base(data.Position + offset)
        {
            VisibleFlag = data.Attr("visibilityFlag");
            InvertVisibleFlag = !string.IsNullOrEmpty(VisibleFlag) && VisibleFlag[0] == '!';
            TalkFlag = data.Attr("talkEnabledFlag");
            InvertTalkFlag = !string.IsNullOrEmpty(TalkFlag) && TalkFlag[0] == '!';
            TalkMode = data.Enum<TalkModes>("mode");

            switch (TalkMode)
            {
                case TalkModes.Teleport:
                    String = data.Attr("room");
                    if (data.Bool("useNearestSpawn"))
                    {
                        NearestSpawn = new Vector2(data.Float("nearestSpawnX"), data.Float("nearestSpawnY"));
                    }
                    break;
                case TalkModes.Dialog:
                    String = data.Attr("dialog");
                    ZoomUse = data.Enum<UseModes>("zoomUsage");
                    CameraUse = data.Enum<UseModes>("cameraUsage");
                    WalkUse = data.Enum<UseModes>("walkUsage");
                    zoomTime = data.Float("zoomDuration");
                    walkX = data.Float("walkToX");
                    camTime = data.Float("cameraDuration");
                    zoomAmount = data.Float("zoomAmount");
                    zoomTo = new Vector2(data.Float("zoomX"), data.Float("zoomY"));
                    camTo = new Vector2(data.Float("camX"), data.Float("camY"));
                    ZoomMode = data.Enum<ZoomModes>("zoomMode");
                    CamMode = data.Enum<CameraModes>("camMode");
                    WalkMode = data.Enum<WalkModes>("walkMode");
                    walkMult = data.Float("speedMult", 1);
                    walkIntoWalls = data.Bool("walkIntoWalls");
                    walkBackwards = data.Bool("walkBackwards");
                    break;
                case TalkModes.Cutscene:
                    String = data.Attr("cutscene");
                    break;
            }
            Outline = data.Bool("outline");
            Depth = data.Int("depth", 2);
            sprite = new Sprite(GFX.Game, "decals/");
            sprite.AddLoop("idle", data.Attr("decalPath"), 0.1f);
            Add(sprite);
            sprite.Color = data.HexColor("color");
            sprite.CenterOrigin();
            sprite.Position += new Vector2(sprite.Width / 2, sprite.Height / 2);
            sprite.Visible = true;
            Collider = new Hitbox(sprite.Width, sprite.Height);
            Tag |= Tags.TransitionUpdate;

            Talk = new TalkComponent(new Rectangle(0, 0, (int)Width, (int)Height), Vector2.UnitX * Width / 2, Interact);
            Talk.PlayerMustBeFacing = false;
            Add(Talk);
        }
        public static bool TriggerCustomEvent(Player player, string eventID)
        {
            if (EventTrigger.CutsceneLoaders.TryGetValue(eventID, out var value))
            {
                Entity entity = value(null, player, eventID);
                if (entity != null)
                {
                    player.Scene.Add(entity);
                    return true;
                }
            }
            return false;
        }
        public void Interact(Player player)
        {
            switch (TalkMode)
            {
                case TalkModes.Teleport:
                    MapData data = (Scene as Level).Session.MapData;
                    if (data.Levels.Find(item => item.Name == String) != null)
                    {
                        Input.Dash.ConsumePress();
                        SceneAs<Level>().OnEndOfFrame += delegate
                        {
                            Input.Dash.ConsumePress();
                            SceneAs<Level>().TeleportTo(player, String, introType, NearestSpawn);

                        };
                    }
                    break;
                case TalkModes.Dialog:
                    DialogCutscene cutscene = new()
                    {
                        CameraUse = CameraUse,
                        ZoomUse = ZoomUse,
                        WalkUse = WalkUse,
                        WalkBackwards = walkBackwards,
                        KeepWalkingIntoWalls = walkIntoWalls,
                        WalkSpeedMult = walkMult,
                        Zoom = zoomAmount,
                        ZoomDuration = zoomTime,
                        ZoomTo = zoomTo,
                        ZoomMode = ZoomMode,
                        CamMode = CamMode,
                        WalkMode = WalkMode,
                        WalkToX = walkX,
                        CameraToPosition = camTo,
                        CameraToDuration = camTime,
                        CameraDelay = cameraDelay,
                        Player = player,
                        Arg = String,
                    };
                    Scene.Add(cutscene);
                    break;
                case TalkModes.Cutscene:
                    TriggerCustomEvent(player, String);
                    break;
            }
        }
        public class DialogCutscene : CutsceneEntity
        {
            public string Arg;
            public Player Player;
            public float Zoom, ZoomDuration;
            public Vector2 ZoomTo;
            public bool WaitForZoom => ZoomUse.Equals(UseModes.UseAndWaitFor);
            public bool WaitForCamera => CameraUse.Equals(UseModes.UseAndWaitFor);
            public bool WaitForWalk => WalkUse.Equals(UseModes.UseAndWaitFor);
            public bool UseZoom => (int)ZoomUse > 0;
            public bool UseWalkTo => (int)WalkUse > 0;
            public bool UseCameraTo => (int)CameraUse > 0;
            public Vector2 CameraToPosition;
            public CameraModes CamMode;
            public ZoomModes ZoomMode;
            public WalkModes WalkMode;
            public UseModes ZoomUse, WalkUse, CameraUse;
            public float CameraToDuration;
            public float WalkToX;
            public bool WalkBackwards;
            public float WalkSpeedMult = 1;
            public bool KeepWalkingIntoWalls;
            public float CameraDelay;
            public Ease.Easer CameraEase = Ease.CubeInOut;
            public DialogCutscene() : base() { }
            public override void OnBegin(Level level)
            {
                Add(new Coroutine(routine()));
            }
            private IEnumerator routine()
            {
                Player.DisableMovement();
                Coroutine zoomRoutine = null;
                Coroutine walkRoutine = null;
                Coroutine cameraRoutine = null;
                if (UseZoom)
                {
                    zoomRoutine = new Coroutine(ZoomMode == ZoomModes.World ? Level.ZoomToWorld(ZoomTo, Zoom, ZoomDuration) : Level.ZoomTo(ZoomTo, Zoom, ZoomDuration));
                    Add(zoomRoutine);
                }
                if (UseWalkTo)
                {
                    walkRoutine = new Coroutine(Player.DummyWalkTo(WalkMode == WalkModes.World ? WalkToX : Player.X + WalkToX, WalkBackwards, WalkSpeedMult, KeepWalkingIntoWalls));
                    Add(walkRoutine);
                }
                if (UseCameraTo)
                {
                    cameraRoutine = new Coroutine(CameraTo(CamMode == CameraModes.World ? CameraToPosition : Level.Camera.Position + CameraToPosition, CameraToDuration, CameraEase, CameraDelay));
                    Add(cameraRoutine);
                }
                while ((WaitForZoom && zoomRoutine != null && !zoomRoutine.Finished) || (WaitForWalk && walkRoutine != null && !walkRoutine.Finished) || (WaitForCamera && cameraRoutine != null && !cameraRoutine.Finished))
                {
                    yield return null;
                }

                yield return new SwapImmediately(Textbox.Say(Arg));
                if (UseZoom)
                {
                    yield return new SwapImmediately(Level.ZoomBack(ZoomDuration));
                }
                EndCutscene(Level);
            }
            public override void OnEnd(Level level)
            {
                Player?.EnableMovement();
                level.ResetZoom();
            }
        }
        public override void Update()
        {
            base.Update();
            Talk.Enabled = TalkEnabled;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            sprite.Play("idle");
        }
        public override void Render()
        {
            if (!IsVisible) return;
            if (Outline)
            {
                sprite.DrawSimpleOutline();
            }
            base.Render();

        }
    }

    [CustomEvent("PuzzleIslandHelper/TestEvent")]
    public class TestEvent : CutsceneEntity
    {
        private Player player;
        public TestEvent(EventTrigger trigger, Player player, string eventID) : base()
        {
            this.player = player;
        }
        public override void OnBegin(Level level)
        {
            Add(new Coroutine(cutscene()));
        }
        private IEnumerator cutscene()
        {
            //simple cutscene that turns the player to the left, waits one second, turns the player to the right, waits 0.5 seconds, then ends.
            player.StateMachine.State = Player.StDummy;
            player.Facing = Facings.Left;
            yield return 1f;
            player.Facing = Facings.Right;
            yield return 0.5f;
            EndCutscene(Level);
        }
        public override void OnEnd(Level level)
        {
            if (WasSkipped)
            {
                //make sure the player is facing right, as they would have been facing right at the end had the cutscene not been skipped.
                player.Facing = Facings.Right;
            }
            //set the player's state back to normal
            player.StateMachine.State = Player.StNormal;
        }
    }
}
