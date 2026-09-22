using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Triggers
{
    [CustomEntity("PuzzleIslandHelper/Decontam")]
    [Tracked]
    public class Decontam : Trigger
    {
        public string AreaID;
        public string Prefix;
        public FlagList DisableFlags;
        public enum DoorStates
        {
            Closed,
            Open,
            Automatic,
        }
        public bool CanActivate = true;
        public FlagList ActiveFlag;
        public FlagList FlagOnActivate;
        private bool wasActive;
        public bool CheckForArea;
        public DoorStates DoorState = DoorStates.Closed;
        public List<LabDoor> Doors = [];
        public class Cutscene : CutsceneEntity
        {
            public Decontam Trigger;
            private EventInstance Event;
            public Cutscene(Decontam trigger) : base()
            {
                Trigger = trigger;
            }
            public override void OnBegin(Level level)
            {
                if (level.GetPlayer() is Player player)
                {
                    Add(new Coroutine(Sequence(player)));
                }
            }
            public IEnumerator Sequence(Player player)
            {
                string arg = Trigger.Prefix;
                setFlag(arg + "Steam", false);
                Trigger.DoorState = DoorStates.Closed;
                yield return 0.7f;
                setFlag(arg + "Steam", true);
                Event = Audio.Play("event:/PianoBoy/steam");
                yield return 0.3f;
                setFlag(arg + "SteamFill", true);
                yield return 6f;
                setFlag(arg + "Steam", false);
                yield return 0.3f;
                setFlag(arg + "SteamFill", false);
                yield return 0.6f;
                Trigger.DoorState = DoorStates.Open;
                yield return null;
                EndCutscene(Level);
            }
            private void setFlag(string flag, bool value)
            {
                Level.Session.SetFlag(flag, value);
            }
            public override void OnEnd(Level level)
            {
                Event?.stop(STOP_MODE.ALLOWFADEOUT);
                Trigger.DoorState = DoorStates.Open;
                Trigger.CheckForArea = true;
                string arg = Trigger.Prefix;
                setFlag(arg + "SteamFill", false);
                setFlag(arg + "Steam", false);
            }
        }
        public Decontam(EntityData data, Vector2 offset) : base(data, offset)
        {
            AreaID = data.Attr("areaID");
            Prefix = data.Attr("prefix");
            ActiveFlag = data.FlagList("activateFlag");
            FlagOnActivate = data.FlagList("flagOnActivate");
            DisableFlags = data.FlagList("disableFlags");
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (ActiveFlag)
            {
                wasActive = true;
                DoorState = DoorStates.Automatic;
            }
            foreach (DetectArea area in scene.Tracker.GetEntities<DetectArea>())
            {
                if (area.ID == AreaID)
                {
                    foreach (LabDoor door in area.CollideAll<LabDoor>())
                    {
                        Doors.Add(door);
                        door.InstantClose();
                    }
                }
            }
            SetDoorState(DoorState);
        }
        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            CheckForArea = false;
            if (CanActivate && (DisableFlags.Empty || !DisableFlags))
            {
                Scene.Add(new Cutscene(this));
                CanActivate = false;
            }
        }
        private class panCutscene : CutsceneEntity
        {
            private FlagList flagOnActivate;
            private Decontam decontam;
            public panCutscene(Decontam parent, FlagList flag) : base()
            {
                decontam = parent;
                flagOnActivate = flag;
            }
            public override void OnBegin(Level level)
            {
                Add(new Coroutine(cutscene()));
            }

            private IEnumerator cutscene()
            {
                Level.DisableMovement();
                Rectangle bounds = Level.Bounds;
                Vector2 pos = Level.GetPlayer().CameraTarget;
                yield return CameraTo((decontam.Position - new Vector2(160, 90)).Clamp(bounds), 1, null, 1f);
                yield return 1f;
                flagOnActivate.State = true;
                decontam.DoorState = DoorStates.Automatic;
                yield return CameraTo(pos, 1, null, 1.5f);
                EndCutscene(Level);
            }
            public override void OnEnd(Level level)
            {
                level.EnableMovement();
                flagOnActivate.State = true;
                decontam.DoorState = DoorStates.Automatic;
            }
        }
        public override void Update()
        {
            base.Update();
            if (DisableFlags && !DisableFlags.Empty) return;
            bool isActive = ActiveFlag;
            if (isActive && !wasActive)
            {
                Scene.Add(new panCutscene(this, FlagOnActivate));
            }
            wasActive = isActive;
            if (CheckForArea && !DetectArea.InArea(SceneAs<Level>(), AreaID))
            {
                Reset();
            }
            SetDoorState(DoorState);
        }
        public void SetDoorState(DoorStates state)
        {
            foreach (LabDoor door in Doors)
            {
                switch (state)
                {
                    case DoorStates.Automatic:
                        door.automatic = true;
                        door.Manual = false;
                        break;
                    case DoorStates.Open:
                        door.automatic = false;
                        door.Manual = true;
                        if (door.State is LabDoor.States.Closed)
                        {
                            door.Open();
                        }
                        break;
                    case DoorStates.Closed:
                        door.automatic = false;
                        door.Manual = true;
                        if (door.State is LabDoor.States.Open)
                        {
                            door.Close();
                        }
                        break;
                }
            }
        }
        public void Reset()
        {
            CanActivate = true;
            CheckForArea = false;
            DoorState = DoorStates.Automatic;
            foreach (LabDoor door in Doors)
            {
                door.Manual = false;
                door.automatic = true;
            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Reset();
        }
    }
}
