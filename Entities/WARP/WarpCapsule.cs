﻿using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.WARP
{
    [Tracked(true)]
    public abstract class WarpCapsule : Entity
    {
        public string WarpID = "";
        public string TargetID = "";
        public bool WaitingForCutscene;
        public int TimesTypeUsed
        {
            get
            {
                if (UsesRune)
                {
                    return PianoModule.Session.TimesUsedCapsuleWarpWithRunes;
                }
                return PianoModule.Session.TimesUsedCapsuleWarp;
            }
            set
            {
                if (UsesRune)
                {
                    PianoModule.Session.TimesUsedCapsuleWarpWithRunes = value;
                }
                else
                {
                    PianoModule.Session.TimesUsedCapsuleWarp = value;
                }
            }
        }
        public bool JustTeleportedTo
        {
            get => justTeleportedToTimer > 0;
            set => justTeleportedToTimer = 0.5f;
        }

        public enum DoorStates
        {
            Idle,
            Closing,
            Opening,
            Closed,
            Open
        }
        public DoorStates DoorState
        {
            get
            {
                if (DoorClosedPercent >= 1) return DoorStates.Closed;
                if (DoorClosedPercent <= 0) return DoorStates.Open;
                if (DoorClosedPercent == lastDoorPercent) return DoorStates.Idle;
                if (DoorClosedPercent > lastDoorPercent) return DoorStates.Closing;
                return DoorStates.Opening;
            }
        }
        private float lastDoorPercent;
        private float justTeleportedToTimer;
        public MTexture LonnTexture => GFX.Game[Path + "lonn"];
        public string Path = "objects/PuzzleIslandHelper/digiWarpReceiver/";
        public float DoorClosedPercent, DoorStallTimer, ShineAmount;
        public bool InCutscene, CanEnter = true, DoorsIdle = true, InvertFlag, Accessible, Blocked;
        public FlagData Flag;
        public EntityID ID;
        public Image Bg, Fg;
        public DotX3 Talk;
        public SnapSolid Floor;
        public Door LeftDoor, RightDoor;
        public WarpRune WarpRune;
        public WarpData Data;
        public bool UsesRune;
        public bool UsesBeam;
        public WarpCapsule(EntityData data, Vector2 offset, EntityID id) : this(data.Position + offset, id, data.Flag("disableFlag", "invertFlag"), data.Attr("warpID"), data.Attr("path"), false, false)
        {
        }
        public WarpCapsule(Vector2 position, EntityID id, FlagData flag, string warpID, string path = null, bool usesRune = false, bool usesBeam = false) : base(position)
        {
            WarpID = warpID;
            Tag |= Tags.TransitionUpdate;
            Depth = 10;
            ID = id;
            if (!string.IsNullOrEmpty(path))
            {
                Path = path;
            }
            Flag = flag;
            UsesRune = usesRune;
            UsesBeam = usesBeam;
            Add(Bg = new Image(GFX.Game[Path + "bg"]));
            Bg.JustifyOrigin(0.5f, 1);
            Collider = Bg.Collider();
            Vector2 texoffset = new Vector2(Bg.Width / 2, Bg.Height);
            Bg.Position += texoffset;
            Fg = new Image(GFX.Game[Path + "fg"]);
            Add(Talk = new DotX3(Collider, Interact));
            Talk.PlayerMustBeFacing = false;
            Add(new BathroomStallComponent(null, Block, Unblock));
            Add(new PostUpdateHook(delegate { lastDoorPercent = DoorClosedPercent; }));
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (PianoMapDataProcessor.WarpCapsules.TryGetValue(Scene.GetAreaKey(), out var list))
            {
                Data = RetrieveWarpData(list);
            }
        }
        public abstract WarpData RetrieveWarpData(CapsuleList list);
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Floor = new SnapSolid(Position + Vector2.UnitY * Height, Width, 2, true) { Fg };
            LeftDoor = new Door(Center, -1, WARPData.XOffset, Path);
            RightDoor = new Door(Center, 1, WARPData.XOffset, Path);
            scene.Add(Floor, LeftDoor, RightDoor);
            if (WarpEnabled() && Flag.State)
            {
                InstantOpenDoors();
            }
            else
            {
                InstantCloseDoors();
            }
        }
        public bool Disabled;
        public void Disable()
        {
            Floor.Collidable = false;
            Floor.Visible = false;
            LeftDoor.Visible = false;
            RightDoor.Visible = false;
            Visible = false;
            Talk.Enabled = false;
            Disabled = true;
        }
        public void Enable()
        {
            Floor.Collidable = true;
            Floor.Visible = true;
            LeftDoor.Visible = true;
            RightDoor.Visible = true;
            Visible = true;
            Disabled = false;
            Accessible = Flag.State;
            Floor.Collidable = !Blocked;
            Talk.Enabled = !InCutscene && CanEnter && Accessible && !Blocked;
        }
        public override void Update()
        {
            base.Update();
            if (!Disabled)
            {
                DoorStallTimer = Calc.Approach(DoorStallTimer, 0, Engine.DeltaTime);
                Accessible = Flag.State;
                Floor.Collidable = !Blocked;
                Talk.Enabled = !InCutscene && CanEnter && Accessible && !Blocked;
                UpdateScale(InCutscene ? WARPData.Scale : Vector2.One);
                if (!InCutscene)
                {
                    bool valid = WarpEnabled();
                    CanEnter = valid;
                    if (DoorStallTimer <= 0)
                    {
                        MoveAlongTowards(valid);
                    }
                }
            }
        }
        public abstract bool WarpEnabled();
        public override void Render()
        {
            LeftDoor.Image.DrawSimpleOutline();
            RightDoor.Image.DrawSimpleOutline();
            Bg.DrawSimpleOutline();
            Fg.DrawSimpleOutline();
            base.Render();
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Floor.RemoveSelf();
            RightDoor.RemoveSelf();
            LeftDoor.RemoveSelf();
        }
        public abstract void Interact(Player player);
        public void Block()
        {
            Blocked = true;
        }
        public void Unblock()
        {
            Alarm.Set(this, 0.3f, delegate { Blocked = false; });
        }
        public void MoveAlongTowards(bool open)
        {
            if (!open)
            {
                if (DoorClosedPercent < 1)
                {
                    MoveAlong(Math.Min(DoorClosedPercent + Engine.DeltaTime, 1));
                }
                else
                {
                    MoveAlong(1);
                }
            }
            else if (DoorClosedPercent > 0)
            {
                MoveAlong(Math.Max(DoorClosedPercent - Engine.DeltaTime, 0));
            }
            else
            {
                MoveAlong(0);
            }
        }
        public void MoveAlong(float percent)
        {
            DoorClosedPercent = percent;
            LeftDoor.SetTo(percent);
            RightDoor.SetTo(percent);
        }
        public void OpenDoors(float time)
        {
            CanEnter = true;
            Add(new Coroutine(OpenDoorsRoutine(time)));
        }
        public void CloseDoors(float time)
        {
            CanEnter = false;
            Add(new Coroutine(CloseDoorsRoutine(time)));
        }
        public void InstantOpenDoors()
        {
            CanEnter = true;
            MoveAlong(0);
        }
        public void InstantCloseDoors()
        {
            CanEnter = false;
            MoveAlong(1);
        }
        public void UpdateScale(Vector2 scale)
        {
            Bg.Scale = LeftDoor.Scale = RightDoor.Scale = scale;
        }
        public IEnumerator MoveTo(float from, float to, float time, Ease.Easer ease)
        {
            DoorsIdle = false;
            ease ??= Ease.Linear;
            for (float i = 0; i < 1; i += Engine.DeltaTime / time)
            {
                MoveAlong(Calc.LerpClamp(from, to, ease(i)));
                yield return null;
            }
            MoveAlong(to);
            DoorsIdle = true;
        }
        public IEnumerator CloseAndOpen(float closeTime, float openTime)
        {
            CanEnter = false;
            yield return new SwapImmediately(CloseDoorsRoutine(closeTime));
            DoorsIdle = false;
            yield return 0.1f;
            yield return new SwapImmediately(OpenDoorsRoutine(openTime));
            CanEnter = true;
        }
        public IEnumerator OpenDoorsRoutine(float openTime)
        {
            float from = DoorClosedPercent;
            yield return new SwapImmediately(MoveTo(from, 0, openTime, null));
        }
        public IEnumerator CloseDoorsRoutine(float closeTime)
        {
            float from = DoorClosedPercent;
            yield return new SwapImmediately(MoveTo(from, 1, closeTime, null));
        }
        public PlayerShade Shade;
        public virtual IEnumerator ReceivePlayerRoutine(Player player, bool? setPlayerStateToNormal)
        {
            if (Shade == null)
            {
                Scene.Add(Shade = new PlayerShade(0.5f));
            }
            LeftDoor.MoveToFg();
            RightDoor.MoveToFg();
            InstantCloseDoors();
            player.StateMachine.State = Player.StDummy;
            yield return 0.1f;
            yield return MoveTo(DoorClosedPercent, 0, 1.2f, Ease.BigBackIn);
            Shade?.Fade(0, 0.7f, Ease.SineIn, true);
            LeftDoor.MoveToBg();
            RightDoor.MoveToBg();
            DoorStallTimer = 0.5f;
            CanEnter = true;
            ValidateStatusAfterWarp(Scene);
            if (setPlayerStateToNormal.HasValue && setPlayerStateToNormal.Value) player.StateMachine.State = Player.StNormal;
        }
        public virtual void ValidateStatusAfterWarp(Scene scene)
        {

        }
        public virtual IEnumerator PrepareToSend(Player player, float time)
        {
            player.StateMachine.State = Player.StDummy;
            InstantOpenDoors();

            LeftDoor.MoveToFg();
            RightDoor.MoveToFg();
            yield return MoveTo(0, 1, time, Ease.BigBackIn);
        }
        public IEnumerator SendScale()
        {
            WarpBeam beam = Scene.Tracker.GetEntity<WarpBeam>();
            for (float i = 0; i < 1; i += Engine.DeltaTime / 0.1f)
            {
                WARPData.Scale = Vector2.Lerp(Vector2.One, WARPData.TargetScale, i);
                if (beam != null && beam.Parent == this)
                {
                    beam.YOffset = -Math.Max(0, LonnTexture.Height * (WARPData.Scale.Y - 1));
                }
                yield return null;
            }
        }
    }
}
