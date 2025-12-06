using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Globalization;
using System.Text.RegularExpressions;
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/TalkingDecal")]
    [Tracked]
    public class TalkingDecal : Entity
    {
        public static void Teleport(Player player, string room, Vector2? nearestSpawn = null)
        {
            Input.Dash.ConsumePress();
            player.DisableMovement();
            new FadeWipe(player.Scene, false, () =>
            {
                player.Scene.OnEndOfFrame += () =>
                {
                    Level level = player.Scene as Level;
                    Vector2 respawnPosition = Vector2.Zero;
                    if (nearestSpawn.HasValue)
                    {
                        respawnPosition = nearestSpawn.Value;
                    }
                    level.TeleportTo(player, room, Player.IntroTypes.None, respawnPosition);
                    new FadeWipe(Engine.Scene, wipeIn: true, () =>
                    {
                        Engine.Scene.GetPlayer()?.EnableMovement();
                    });
                };
            });
        }
        public static void Teleport(Player player, string markerID, string room)
        {
            Input.Dash.ConsumePress();
            player.DisableMovement();
            new FadeWipe(player.Scene, false, () =>
            {
                player.Scene.OnEndOfFrame += () =>
                {
                    Level level = player.Scene as Level;
                    Vector2 respawnPosition = Vector2.Zero;
                    if (!string.IsNullOrEmpty(markerID))
                    {
                        if (MarkerExt.TryGetRoomData(markerID, room, Engine.Scene, out MarkerData data))
                        {
                            respawnPosition = data.PositionInRoom;
                        }
                    }
                    level.TeleportTo(player, room, Player.IntroTypes.None, respawnPosition);
                    new FadeWipe(Engine.Scene, wipeIn: true, () =>
                    {
                        Engine.Scene.GetPlayer()?.EnableMovement();
                    });
                };
            });
        }
        public Sprite onSprite;
        public Image image;
        public bool Outline;
        public FlagList FlagsOnTalk;
        public enum TalkModes
        {
            Teleport,
            Dialog,
            Cutscene,
            Flag
        }
        public enum ChooserModes
        {
            Single,
            Swap
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
        public enum TeleportModes
        {
            Instant,
            Glitch,
            Wipe
        }
        public enum Wipes
        {
            None,
            Angled,
            Curtain,
            Dream,
            Drop,
            Fade,
            Fall,
            Heart,
            KeyDoor,
            Mountain,
            Screen,
            Spotlight,
            Starfield,
            Wind
        }
        public ZoomModes ZoomMode;
        public CameraModes CamMode;
        public WalkModes WalkMode;
        public UseModes ZoomUse, WalkUse, CameraUse;
        public TeleportModes TeleportMode;
        private float glitchValue;
        private float glitchTime;
        private Wipes Wipe;
        private float zoomTime, walkX, camTime, zoomAmount, walkMult, cameraDelay;
        private float extendUp, extendDown, extendLeft, extendRight;
        private bool walkBackwards, walkIntoWalls;
        private Vector2 camTo, zoomTo;
        public TalkModes TalkMode;
        private Player.IntroTypes introType;
        public Vector2? NearestSpawn;
        public string String;
        public string String2;
        public string MarkerID;
        public TalkComponent Talk;
        public bool FlagState(string flag)
        {
            return string.IsNullOrEmpty(flag) || SceneAs<Level>().Session.GetFlag(flag);
        }
        public FlagList TalkEnabled;
        public FlagList RenderingEnabled;
        private Sprite offSprite;
        private VertexLight onLight;
        private VertexLight offLight;
        private bool useNearestSpawn;
        public float TalkAlpha = 1;

        private bool invertFlag;
        private enum FlagModes
        {
            Invert,
            SetTrue,
            SetFalse
        }
        private FlagModes flagMode;
        private string flag;
        private bool disableIfTrue;
        private bool disableIfFalse;
        public TalkingDecal(EntityData data, Vector2 offset)
        : base(data.Position + offset)
        {
            flag = data.Attr("flag");
            flagMode = data.Enum<FlagModes>("flagMode");
            disableIfTrue = data.Bool("disableIfTrue");
            disableIfFalse = data.Bool("disableIfFalse");
            MarkerID = data.Attr("markerID");
            FlagsOnTalk = new FlagList(data.Attr("flagsOnTalk"));
            TeleportMode = data.Enum<TeleportModes>("teleportMode");
            glitchValue = data.Float("glitchAmount");
            glitchTime = data.Float("glitchDuration", 0.3f);
            extendUp = data.Float("upExtend");
            extendDown = data.Float("downExtend");
            extendLeft = data.Float("leftExtend");
            extendRight = data.Float("rightExtend");
            TalkMode = data.Enum<TalkModes>("mode");
            RenderingEnabled = new FlagList(data.Attr("visibilityFlag"));
            TalkEnabled = new FlagList(data.Attr("talkEnabledFlag"));
            switch (data.Attr("visibleMode"))
            {
                case "Visible":
                    RenderingEnabled.ForcedValue = true;
                    break;
                case "Invisible":
                    RenderingEnabled.ForcedValue = false;
                    break;
            }
            switch (TalkMode)
            {
                case TalkModes.Teleport:
                    String = data.Attr("room");
                    if (data.Bool("useNearestSpawn"))
                    {
                        useNearestSpawn = true;
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
                offSprite.Play("idle");
                offSprite.CenterOrigin();
                offSprite.Position += offSprite.HalfSize();
                Add(offSprite);
            }
            Tag |= Tags.TransitionUpdate;
        }
        public Vector2 DrawAt;
        public Rectangle TalkBounds;
        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (onSprite != null)
            {
                Collider = new Hitbox(onSprite.Width, onSprite.Height);
            }
            int x = -(int)extendLeft;
            int y = -(int)extendUp;
            int width = (int)(Width + extendLeft + extendRight);
            int height = (int)(Height + extendUp + extendDown);
            TalkBounds = new Rectangle(x, y, width, height);
            DrawAt = Vector2.UnitX * TalkBounds.Width / 2;
            Talk = new TalkComponent(TalkBounds, DrawAt, Interact);
            Talk.PlayerMustBeFacing = false;
            Add(Talk);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            UpdateHoverUIAlpha();
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
            FlagsOnTalk.State = true;
            switch (TalkMode)
            {
                case TalkModes.Flag:
                    if (!string.IsNullOrEmpty(flag))
                    {
                        switch (flagMode)
                        {
                            case FlagModes.Invert:
                                SceneAs<Level>().Session.SetFlag(flag, !SceneAs<Level>().Session.GetFlag(flag));
                                break;
                            case FlagModes.SetTrue:
                                SceneAs<Level>().Session.SetFlag(flag);
                                break;
                            case FlagModes.SetFalse:
                                SceneAs<Level>().Session.SetFlag(flag, false);
                                break;
                        }
                    }
                    break;
                case TalkModes.Teleport:

                    MapData data = (Scene as Level).Session.MapData;
                    if (data.Levels.Find(item => item.Name == String) != null)
                    {
                        Input.Dash.ConsumePress();
                        if (glitchValue > 0 && glitchTime > 0)
                        {
                            float prev = Glitch.Value;
                            Entity glitchHelper = new Entity();
                            glitchHelper.AddTag(Tags.Persistent | Tags.TransitionUpdate);
                            Alarm.Set(glitchHelper, glitchTime, delegate { Glitch.Value = prev; RemoveSelf(); });
                            Scene.Add(glitchHelper);
                        }
                        else if (glitchValue > 0)
                        {
                            Glitch.Value = glitchValue;
                        }
                        if (TeleportMode is TeleportModes.Wipe)
                        {
                            if (NearestSpawn.HasValue)
                            {
                                Teleport(player, String, NearestSpawn);
                            }
                            else
                            {
                                Teleport(player, MarkerID, String);
                            }
                        }

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
        private string targetString;
        public void UpdateHoverUIAlpha()
        {
            Vector2 trueDrawAt = Position + TalkBounds.Location.ToVector2() + DrawAt;
            if (CollideCheck<Solid>(trueDrawAt) || CollideCheck<FakeWall>(trueDrawAt))
            {
                TalkAlpha = Calc.Approach(TalkAlpha, 0, Engine.DeltaTime * 2);
            }
            else
            {
                TalkAlpha = Calc.Approach(TalkAlpha, 1, Engine.DeltaTime * 2);
            }
            if (Talk.UI != null)
            {
                Talk.UI.alpha = TalkAlpha;
            }
        }
        public override void Update()
        {
            base.Update();
            if (TalkMode is TalkModes.Flag)
            {
                bool flagstate2 = string.IsNullOrEmpty(flag) || SceneAs<Level>().Session.GetFlag(flag);

                if ((disableIfTrue && flagstate2) || (disableIfFalse && !flagstate2))
                {
                    Talk.Enabled = false;
                }
                UpdateSprites(flagstate2);
            }
            else
            {
                bool flagstate = TalkEnabled;
                targetString = flagstate ? String : String2;
                targetString ??= "";
                Talk.Enabled = !string.IsNullOrEmpty(targetString);
                UpdateSprites(flagstate);
            }
            UpdateHoverUIAlpha();
        }
        public void UpdateSprites(bool value)
        {
            if (onSprite != null)
            {
                onSprite.Visible = value;
                if (onLight != null)
                {
                    onLight.Visible = value;
                }
            }
            if (offSprite != null)
            {
                offSprite.Visible = !value;
                if (offLight != null)
                {
                    offLight.Visible = !value;
                }
            }
        }
        public override void Render()
        {
            if (!RenderingEnabled) return;
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
