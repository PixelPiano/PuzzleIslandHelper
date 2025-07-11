using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/TalkingDecal")]
    [Tracked]
    public class TalkingDecal : Entity
    {
        public Sprite onSprite;
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
        public string String2;
        public TalkComponent Talk;
        public bool FlagState(string flag)
        {
            return string.IsNullOrEmpty(flag) || SceneAs<Level>().Session.GetFlag(flag);
        }
        public FlagList TalkFlagList;
        public FlagList VisibleFlagList;
        private Sprite offSprite;
        private VertexLight onLight;
        private VertexLight offLight;
        public TalkingDecal(EntityData data, Vector2 offset)
        : base(data.Position + offset)
        {
            TalkMode = data.Enum<TalkModes>("mode");
            VisibleFlagList = new FlagList(data.Attr("visibilityFlag"));
            TalkFlagList = new FlagList(data.Attr("talkEnabledFlag"));

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
                    String = data.Attr("onDialog");
                    String2 = data.Attr("offDialog");
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
                    String = data.Attr("onCutscene");
                    String = data.Attr("offCutscene");
                    break;
            }
            Outline = data.Bool("outline");
            Depth = data.Int("depth", 2);
            if (!string.IsNullOrEmpty(data.Attr("onDecalPath")))
            {
                onSprite = new Sprite(GFX.Game, "decals/");
                onSprite.AddLoop("idle", data.Attr("onDecalPath"), 0.1f);
                onSprite.Color = data.HexColor("color");
                onSprite.Play("idle");
                onSprite.CenterOrigin();
                onSprite.Position += onSprite.HalfSize();
                Add(onSprite);
            }
            if (!string.IsNullOrEmpty(data.Attr("offDecalPath")))
            {
                offSprite = new Sprite(GFX.Game, "decals/");
                offSprite.AddLoop("idle", data.Attr("offDecalPath"), 0.1f);
                offSprite.Color = data.HexColor("color");
                offSprite.CenterOrigin();
                offSprite.Play("idle");
                offSprite.Position += offSprite.HalfSize();
                Add(offSprite);
            }
            if (onSprite != null)
            {
                Collider = new Hitbox(onSprite.Width, onSprite.Height);
            }
            Tag |= Tags.TransitionUpdate;

            Talk = new TalkComponent(new Rectangle(0, 0, (int)Width, (int)Height), Vector2.UnitX * Width / 2, Interact);
            Talk.PlayerMustBeFacing = false;
            Add(Talk);
/*            if (onSprite != null)
            {
                Add(onLight = new VertexLight(onSprite.Center, Color.White, 1, (int)onSprite.Width, (int)(onSprite.Width * 2f)));
                onLight.Visible = false;
            }
            if (offSprite != null)
            {
                Add(offLight = new VertexLight(offSprite.Center, Color.White, 1, (int)offSprite.Width, (int)(offSprite.Width * 2f)));
                offLight.Visible = false;
            }*/
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
                    if (!string.IsNullOrEmpty(targetString))
                    {
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
                            Arg = targetString,
                        };
                        Scene.Add(cutscene);
                    }

                    break;
                case TalkModes.Cutscene:
                    if (!string.IsNullOrEmpty(targetString))
                    {
                        TriggerCustomEvent(player, targetString);
                    }
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
        private bool hasFirstString => !string.IsNullOrEmpty(String);
        private bool hasSecondString => !string.IsNullOrEmpty(String2);
        private string targetString;
        public override void Update()
        {
            base.Update();
            bool flagstate = TalkFlagList;
            if (flagstate)
            {
                if (hasFirstString)
                {
                    targetString = String;
                    Talk.Enabled = true;
                }
                else
                {
                    targetString = "";
                    Talk.Enabled = false;
                }
            }
            else
            {
                if (hasSecondString)
                {
                    targetString = String2;
                    Talk.Enabled = true;
                }
                else
                {
                    targetString = "";
                    Talk.Enabled = false;
                }
            }
            if(onSprite != null)
            {
                onSprite.Visible = flagstate;
                if(onLight != null)
                {
                    onLight.Visible = flagstate;
                }
            }
            if(offSprite != null)
            {
                offSprite.Visible = !flagstate;
                if(offLight != null)
                {
                    offLight.Visible = !flagstate;
                }
            }
        }
        public override void Render()
        {
            if (!VisibleFlagList) return;
            if (Outline)
            {
                if (onSprite != null && onSprite.Visible)
                {
                    onSprite.DrawSimpleOutline();
                }
                if (offSprite != null && offSprite.Visible)
                {
                    offSprite.DrawSimpleOutline();
                }
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
