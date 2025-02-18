using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities;
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
        public enum DoorStates
        {
            None,
            Automatic,
            Open,
            Closed
        }
        public bool CanActivate = true;
        public bool CheckForArea;
        public DoorStates DoorState;
        public DetectArea Area;
        public Dictionary<LabDoor, bool> Doors = [];
        public class Cutscene : CutsceneEntity
        {
            public Decontam Trigger;
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
                Audio.Play("event:/PianoBoy/steam");
                yield return 0.3f;
                setFlag(arg + "SteamFill", true);
                yield return 6f;
                setFlag(arg + "Steam", false);
                yield return 0.3f;
                setFlag(arg + "SteamFill", false);
                yield return 0.6f;
                Trigger.DoorState = DoorStates.Open;
                yield return null;

            }
            private void setFlag(string flag, bool value)
            {
                Level.Session.SetFlag(flag, value);
            }
            public override void OnEnd(Level level)
            {
                if (WasSkipped)
                {
                    Trigger.DoorState = DoorStates.Open;
                    Trigger.CheckForArea = true;
                }
            }
        }
        public Decontam(EntityData data, Vector2 offset) : base(data, offset)
        {
            AreaID = data.Attr("detectAreaID");
            Prefix = data.Attr("prefix");
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            foreach (DetectArea area in scene.Tracker.GetEntities<DetectArea>())
            {
                if (area.ID == AreaID)
                {
                    foreach (LabDoor door in Area.CollideAll<LabDoor>())
                    {
                        Doors.Add(door, door.automatic);
                    }
                }
            }
        }
        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            CheckForArea = false;
            if (CanActivate)
            {
                Scene.Add(new Cutscene(this));
                CanActivate = false;
            }
        }
        public override void Update()
        {
            base.Update();
            if (CheckForArea && !DetectArea.InArea(SceneAs<Level>(), AreaID))
            {
                Reset();
            }
            SetDoorState(DoorState);
        }
        public void SetDoorState(DoorStates state)
        {
            foreach (LabDoor door in Doors.Keys)
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
                        if (!door.Open)
                        {
                            door.OpenDoor();
                        }
                        break;
                    case DoorStates.Closed:
                        door.automatic = false;
                        door.Manual = true;
                        if (door.Open)
                        {
                            door.CloseDoor();
                        }
                        break;
                }
            }
        }
        public void Reset()
        {
            CanActivate = true;
            CheckForArea = false;
            DoorState = DoorStates.None;
            foreach (KeyValuePair<LabDoor, bool> pair in Doors)
            {
                pair.Key.automatic = pair.Value;
                pair.Key.Manual = false;
            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Reset();
        }
    }
}
