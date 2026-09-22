using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using System.Collections;
using Celeste.Mod.PuzzleIslandHelper.Components;
using System.Linq;
using System.Collections.Generic;
using Celeste.Mod.CommunalHelper;
using System;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{

    [CustomEntity("PuzzleIslandHelper/BatterySwitch")]
    [Tracked]
    public class BatterySwitch : Entity
    {
        public string OrigRoom;
        public EntityID ID;
        public FlagList FlagToSet;
        public string CutsceneRoom;
        public string CutsceneCameraMarker;
        public Sprite Handle;
        public Image Back;
        public Image Fill;
        public string SwitchID;
        private bool idEmpty => string.IsNullOrEmpty(SwitchID);
        public DotX3 Talk;
        public bool Activated => !idEmpty && PianoModule.Session.ActivatedBatterySwitches.Contains(SwitchID);
        public BatterySwitch(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
        {
            CutsceneRoom = data.Attr("cutsceneRoom");
            CutsceneCameraMarker = data.Attr("cutsceneCameraMarker");
            FlagToSet = data.FlagList("flagToSet");
            SwitchID = data.Attr("switchID");
            Depth = 1;
            Back = new Image(GFX.Game["objects/PuzzleIslandHelper/drillMachine/batterySwitchBack"]);
            Fill = new Image(GFX.Game["objects/PuzzleIslandHelper/drillMachine/batterySwitchColor"]);
            Handle = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/drillMachine/batterySwitchHandle");
            Handle.AddLoop("idleDown", "", 0.1f, 0);
            Handle.AddLoop("idleUp", "", 0.1f, 6);
            Handle.Add("activate", "", 0.1f, "idleUp");
            Handle.Visible = false;
            Add(Back, Fill, Handle);
            Collider = new Hitbox(Back.Width, Back.Height);
            Talk = new DotX3(0, 0, Width, Height + 16, Vector2.UnitX * Collider.HalfSize.X, Interact);
            Add(Talk);
            Tag |= Tags.TransitionUpdate;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Fill.Color = Activated ? Color.Green : Color.Red;
            Handle.Play(Activated ? "idleUp" : "idleDown");
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            OrigRoom = (scene as Level).Session.Level;
            Talk.Enabled = !Activated;
        }
        public override void Update()
        {
            base.Update();
            Talk.Enabled = !Activated;
        }
        public override void Render()
        {
            //Back.DrawSimpleOutline();
            base.Render();
            //Handle.DrawSimpleOutline();
            Handle.Render();
        }
        private void Interact(Player player)
        {
            if (!Activated)
            {
                Scene.Add(new Cutscene(this));
            }
        }
        public IEnumerator SpriteRoutine()
        {
            Handle.Play("activate");
            Handle.Rate /= 2;
            while (Handle.CurrentAnimationFrame < Handle.CurrentAnimationTotalFrames / 2)
            {
                yield return null;
            }
            Handle.Rate *= 2;
            while (Handle.CurrentAnimationID != "idleUp")
            {
                yield return null;
            }
            for (float i = 0; i < 1; i += Engine.DeltaTime / 0.2f)
            {
                Fill.Color = Color.Lerp(Fill.Color, Color.White, i);
                yield return null;
            }
            Fill.Color = Color.White;
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                Fill.Color = Color.Lerp(Color.White, Color.ForestGreen, Ease.SineOut(i));
                yield return null;
            }
        }
        public IEnumerator ActivateBatterySwitchPlatesAndWait(Scene scene, string id)
        {
            foreach (BatterySwitchPlate p in ActivateBatterySwitchPlates(scene, id, false))
            {
                while (p.InRoutine) yield return null;
            }
        }
        public List<BatterySwitchPlate> ActivateBatterySwitchPlates(Scene scene, string id, bool skipToEnd = false)
        {
            List<BatterySwitchPlate> plates = [];
            foreach (BatterySwitchPlate plate in (scene as Level).Tracker.GetEntities<BatterySwitchPlate>())
            {
                if (!plate.Activated && plate.SwitchID == id)
                {
                    plates.Add(plate);
                    plate.Activate(skipToEnd);
                }
            }
            return plates;
        }
        public class Cutscene : CutsceneEntity
        {
            public BatterySwitch Parent;
            private float blackFade;
            private Vector2 origPlayerPosition;
            private Player player;
            private Vector2 origCamPosition;
            public bool Teleports;
            public Cutscene(BatterySwitch parent) : base()
            {
                Parent = parent;
                Depth = int.MinValue;
            }
            public override void OnBegin(Level level)
            {
                player = level.GetPlayer();
                player.DisableMovement();
                origPlayerPosition = player.Position;
                origCamPosition = level.Camera.Position;
                Teleports = !string.IsNullOrEmpty(Parent.CutsceneRoom) && level.Session.MapData.Levels.Exists(item => item.Name == Parent.CutsceneRoom);
                Add(new Coroutine(cutscene(player)));
            }
            private IEnumerator cutscene(Player player)
            {
                Vector2 nextCam = new Vector2(Math.Max((int)Parent.CenterX - 160, Level.Bounds.Left), Level.Camera.Y);
                Vector2 prev = Level.Camera.Position;
                for (float i = 0; i < 1; i += Engine.DeltaTime)
                {
                    Level.Camera.Position = Vector2.Lerp(prev, nextCam, Ease.SineOut(i));
                    yield return null;
                }
                yield return null;
                player.StateMachine.State = Player.StDummy;
                yield return 0.2f;
                yield return Parent.SpriteRoutine();

                PianoModule.Session.ActivatedBatterySwitches.Add(Parent.SwitchID);
                foreach (BatterySwitchPlate p in Parent.ActivateBatterySwitchPlates(Level, Parent.SwitchID, false))
                {
                    while (p.InRoutine) yield return null;
                }
                if (Teleports)
                {
                    yield return PianoUtils.Lerp(Ease.SineInOut, 1, f => blackFade = f, true);
                    Vector2 prevCameraPosition = Level.Camera.Position;
                    Tag |= Tags.Global;
                    PianoUtils.InstantTeleport(Scene, Parent.CutsceneRoom, player.Position, (l, p) =>
                    {
                        Level = l;
                        if (Marker.TryFind(l, Parent.CutsceneCameraMarker, out Vector2 position))
                        {

                            l.Camera.Position = position - new Vector2(160, 90);
                            l.Camera.Position = l.Camera.GetBounds().ClampTo(l.Bounds).Location.ToVector2();
                            Tag &= ~Tags.Global;
                        }
                    });
                    yield return null;
                    yield return PianoUtils.Lerp(Ease.SineInOut, 1, f => blackFade = 1 - f, true);
                    yield return 1f;
                    foreach (BatterySwitchPlate switchPlate in Level.Tracker.GetEntities<BatterySwitchPlate>())
                    {
                        if (!switchPlate.Activated && switchPlate.SwitchID == "Cutscene:" + Parent.SwitchID)
                        {
                            yield return switchPlate.Activate();
                        }
                    }
                    Parent.FlagToSet.State = !Parent.FlagToSet.Inverted;
                    yield return 1;
                    yield return PianoUtils.Lerp(Ease.SineInOut, 1, f => blackFade = f, true);
                    Tag |= Tags.Global;
                    PianoUtils.InstantTeleport(Scene, Parent.OrigRoom, player.Position, (l, p) =>
                    {
                        Level = l;
                        Level.Camera.Position = origCamPosition;
                        Tag &= ~Tags.Global;
                    });

                    yield return null;
                    yield return PianoUtils.Lerp(Ease.SineInOut, 1, f => blackFade = 1 - f, true);
                }
                EndCutscene(Level);
            }
            public override void Render()
            {
                base.Render();
                if (blackFade > 0)
                {
                    Camera camera = (Engine.Scene as Level).Camera;
                    Draw.Rect(camera.Position, 320, 180, Color.Black * blackFade);
                }
            }
            public override void OnEnd(Level level)
            {
                if (Teleports)
                {
                    PianoModule.Session.ActivatedBatteryLines.Add("Cutscene" + Parent.SwitchID);
                }
                PianoModule.Session.ActivatedBatterySwitches.Add(Parent.SwitchID);
                Parent.FlagToSet.State = !Parent.FlagToSet.Inverted;
                if (WasSkipped && Teleports)
                {
                    blackFade = 0;
                    if (level.Session.Level != Parent.OrigRoom)
                    {
                        PianoUtils.InstantTeleport(Engine.Scene, Parent.OrigRoom, origPlayerPosition, (l, p) =>
                        {
                            p.EnableMovement();
                            l.Camera.Position = origCamPosition;
                        });
                    }
                }
                else
                {
                    level.EnableMovement();
                    level.Camera.Position = origCamPosition;
                }
            }
        }
    }
}