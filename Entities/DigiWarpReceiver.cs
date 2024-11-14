using Celeste.Mod.CommunalHelper;
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities.Programs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using static Celeste.Mod.PuzzleIslandHelper.Entities.InvertAuth;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/DigiWarpReceiver")]
    [Tracked]
    public class WarpCapsule : Entity
    {
        [Tracked]
        public class Machine : Entity
        {
            public static MTexture Texture => GFX.Game[Path + "terminal"];
            public Image Image;
            public WarpCapsule Parent;
            public DotX3 Talk;
            public Machine(WarpCapsule parent, Vector2 position) : base(position)
            {
                Depth = 1;
                Parent = parent;
                Collider = new Hitbox(Texture.Width, Texture.Height);
                Add(Image = new Image(Texture, true));
                Talk = new DotX3(Collider, Interact);
                Add(Talk);
            }
            public override void Render()
            {
                Image.DrawSimpleOutline();
                base.Render();
            }
            public void Interact(Player player)
            {
                Add(new Coroutine(Cutscene(player)));
            }
            public IEnumerator Cutscene(Player player)
            {
                Level level = Scene as Level;
                player.StateMachine.State = Player.StDummy;
                Vector2 from = level.Camera.Position;
                Vector2 to = level.Camera.Position + Vector2.UnitX * 80;
                for (float i = 0; i < 1; i += Engine.DeltaTime)
                {
                    level.Camera.Position = Vector2.Lerp(from, to, Ease.CubeOut(i));
                    yield return null;
                }
                FakeTerminal t = new FakeTerminal(level.Camera.Position + new Vector2(150, 45), 150, 80);
                Scene.Add(t);
                while (t.TransitionAmount < 1)
                {
                    yield return null;
                }
                WarpProgram program = new WarpProgram(Parent, t);
                Scene.Add(program);

                while (t.TransitionAmount > 0)
                {
                    yield return null;
                }
                to = from;
                from = level.Camera.Position;
                for (float i = 0; i < 1; i += Engine.DeltaTime)
                {
                    level.Camera.Position = Vector2.Lerp(from, to, Ease.CubeOut(i));
                    yield return null;
                }
                yield return 0.1f;
                player.StateMachine.State = Player.StNormal;
                yield return null;
            }
        }
        public const int XOffset = 10;
        public const string Path = "objects/PuzzleIslandHelper/digiWarpReceiver/";
        public static MTexture LonnTexture => GFX.Game[Path + "lonn"];
        public static Vector2 TargetScale = new Vector2(0.4f, 2f);
        public static Vector2 Scale = Vector2.One;
        public float DoorPercent;
        public float ShineAmount;
        public bool InCutscene;
        public bool ReadyForBeam;
        public bool Primary;
        public bool Enabled = true;
        public string WarpID;
        public string WarpPassword;
        public Door LeftDoor, RightDoor;
        public Image Bg, Fg, ShineTex;
        public SnapSolid Floor;
        public DotX3 Talk;
        public Machine InputMachine;
        public WarpBeam Beam;
        private Entity Shine;
        public EntityID ID;
        public string TargetID;
        public float DoorStallTimer;
        public WarpCapsule(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
        {
            ID = id;
            Primary = data.Bool("primary");
            InputMachine = new Machine(this, data.NodesOffset(offset)[0]);
            Tag |= Tags.TransitionUpdate;
            Depth = 3;
            WarpID = data.Attr("warpID").Replace(" ", "").ToLower();
            WarpPassword = data.Attr("password").Replace(" ", "").ToLower();
            Add(Bg = new Image(GFX.Game[Path + "bg"]));
            Collider = new Hitbox(Bg.Width, Bg.Height);
            Vector2 texoffset = new Vector2(Bg.Width / 2, Bg.Height);
            Bg.JustifyOrigin(0.5f, 1);
            Bg.Position += texoffset;

            Fg = new Image(GFX.Game[Path + "fg"]);
            ShineTex = new Image(GFX.Game[Path + "shine"]);
            ShineTex.Color = Color.White * 0;
            Add(Talk = new DotX3(Collider, Interact));
            if (Primary)
            {
                Talk.Enabled = Enabled = false;
            }
        }
        public void Interact(Player player)
        {
            if (ValidateID(TargetID))
            {
                Scene.Add(new WarpBack(this, player));
            }

        }
        public bool ValidateID(string id)
        {
            if (!string.IsNullOrEmpty(id) && id != WarpID && GetCapsuleData(id) != null)
            {
                return true;
            }
            return false;
        }
        public static bool ValidatePassword(string id, string password)
        {
            WarpCapsuleData data = GetCapsuleData(id);
            return data != null && (string.IsNullOrEmpty(data.Password) || data.Password.Equals(password));
        }
        public static WarpCapsuleData GetCapsuleData(string id)
        {
            if (!PianoMapDataProcessor.WarpLinks.ContainsKey(id))
            {
                return null;
            }
            return PianoMapDataProcessor.WarpLinks[id];
        }
        public void SetWarpTarget(string id)
        {
            if (ValidateID(id))
            {
                TargetID = id;
            }
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);

            Floor = new SnapSolid(Position + Vector2.UnitY * Height, Width, 2, true) { Fg };
            Shine = new Entity(Position) { ShineTex };
            Shine.Depth = Floor.Depth - 1;
            LeftDoor = new Door(Center, -1, XOffset);
            RightDoor = new Door(Center, 1, XOffset);
            scene.Add(Floor, Shine, LeftDoor, RightDoor);
            scene.Add(InputMachine);
            if (PianoModule.Session.PersistentWarpLinks.ContainsKey(ID))
            {
                TargetID = PianoModule.Session.PersistentWarpLinks[ID];
            }
            else
            {
                PianoModule.Session.PersistentWarpLinks.Add(ID, "");
            }
            if (ValidateID(TargetID))
            {
                InstantOpenDoors();
            }
            else
            {
                InstantCloseDoors();
            }
        }
        public override void Update()
        {
            base.Update();
            if (DoorStallTimer > 0)
            {
                DoorStallTimer = Calc.Approach(DoorStallTimer, 0, Engine.DeltaTime);
            }
            PianoModule.Session.PersistentWarpLinks[ID] = TargetID;
            Talk.Enabled = !InCutscene && Enabled;
            UpdateScale(InCutscene ? Scale : Vector2.One);
            ShineTex.Color = Color.White * ShineAmount;
            if (!InCutscene)
            {
                bool valid = ValidateID(TargetID);
                Enabled = valid;
                if (DoorStallTimer <= 0)
                {
                    MoveAlongTowards(valid);
                }
            }

        }
        public void MoveAlongTowards(bool open)
        {
            if (!open)
            {
                if (DoorPercent < 1)
                {
                    MoveAlong(Math.Min(DoorPercent + Engine.DeltaTime, 1));
                }
            }
            else if (DoorPercent > 0)
            {
                MoveAlong(Math.Max(DoorPercent - Engine.DeltaTime, 0));
            }
        }
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
            InputMachine.RemoveSelf();
            Shine.RemoveSelf();
            Floor.RemoveSelf();
            RightDoor.RemoveSelf();
            LeftDoor.RemoveSelf();
        }
        public void MoveAlong(float percent)
        {
            DoorPercent = percent;
            LeftDoor.SetTo(percent);
            RightDoor.SetTo(percent);
        }
        public IEnumerator MoveTo(float from, float to, float time, Ease.Easer ease)
        {
            ease ??= Ease.Linear;
            for (float i = 0; i < 1; i += Engine.DeltaTime / time)
            {
                MoveAlong(Calc.LerpClamp(from, to, ease(i)));
                yield return null;
            }
            MoveAlong(to);
        }
        public IEnumerator CloseAndOpen(float closeTime, float openTime)
        {
            Enabled = false;
            yield return MoveTo(0, 1, closeTime, null);
            yield return 0.1f;
            yield return MoveTo(1, 0, openTime, null);
            Enabled = true;
        }
        public void InstantOpenDoors()
        {
            Enabled = true;
            MoveAlong(0);
        }
        public void InstantCloseDoors()
        {
            Enabled = false;
            MoveAlong(1);
        }
        public void UpdateScale(Vector2 scale)
        {
            ShineTex.Scale = Bg.Scale = LeftDoor.Scale = RightDoor.Scale = scale;
        }
        public IEnumerator IntroRoutine(Player player)
        {
            player.StateMachine.State = Player.StDummy;
            yield return 0.1f;
            yield return MoveTo(DoorPercent, 0, 1.2f, Ease.BigBackIn);
            LeftDoor.MoveToBg();
            RightDoor.MoveToBg();
            DoorStallTimer = 0.5f;
            /*            if (Primary)
                        {
                            yield return MoveTo(0, 1, 0.8f, null);
                            Enabled = false;
                        }
                        else
                        {
                            Enabled = true;
                        }*/
            player.StateMachine.State = Player.StNormal;
        }
        public IEnumerator OutroRoutine(Player player)
        {
            player.StateMachine.State = Player.StDummy;
            InstantOpenDoors();

            LeftDoor.MoveToFg();
            RightDoor.MoveToFg();
            yield return MoveTo(0, 1, 0.8f, Ease.BigBackIn);
        }
        public IEnumerator ScaleFirst()
        {
            WarpBeam beam = Scene.Tracker.GetEntity<WarpBeam>();
            for (float i = 0; i < 1; i += Engine.DeltaTime / 0.1f)
            {
                Scale = Vector2.Lerp(Vector2.One, TargetScale, i);
                if (beam != null && beam.Parent == this)
                {
                    beam.YOffset = -Math.Max(0, LonnTexture.Height * (Scale.Y - 1));
                }
                yield return null;
            }
        }
        public IEnumerator ScaleSecond()
        {
            for (float i = 0; i < 1; i += Engine.DeltaTime / 0.5f)
            {
                Scale = Vector2.Lerp(TargetScale, Vector2.One, i);
                yield return null;
            }
            Scale = Vector2.One;
        }
        public class WarpBack : CutsceneEntity
        {
            public WarpCapsule Parent;
            public Player Player;
            public WarpBeam Beam;
            public bool Teleported;
            public Vector2 PlayerPosSave;
            public float DoorPercentSave;
            public EntityID FirstParentID;
            public WarpBack(WarpCapsule parent, Player player) : base()
            {
                FirstParentID = parent.ID;
                Parent = parent;
                Player = player;
            }
            public override void OnBegin(Level level)
            {
                Parent.InCutscene = true;
                Add(new Coroutine(Routine(Player)));
            }
            public override void OnEnd(Level level)
            {
                Parent.InCutscene = false;
                if (WasSkipped)
                {
                    if (Parent.ID.ID == FirstParentID.ID)
                    {
                        InstantTeleport(level, Player, Parent, CleanUp);
                    }
                    else
                    {
                        CleanUp(level, Player);
                    }
                }
            }
            private void TeleportCleanUp(Level level, Player player)
            {
                Teleported = true;
                Level = Engine.Scene as Level;
                Player = Level.GetPlayer();
                Parent = Level.Tracker.GetEntity<WarpCapsule>();
                if (Parent != null)
                {
                    Player.StateMachine.State = Player.StDummy;
                    Player.Position = Parent.Position + PlayerPosSave;
                    Player.ForceCameraUpdate = true;
                }
                Level.Camera.Position = Player.CameraTarget;
                Level.Camera.Position.Clamp(Level.Bounds);
                Level.Flash(Color.White, false);

                if (Parent != null)
                {
                    Scale = Vector2.One;
                    Parent.ShineAmount = 1;
                    Parent.DoorPercent = DoorPercentSave;
                    Parent.MoveAlong(DoorPercentSave);
                    Parent.LeftDoor.MoveToFg();
                    Parent.RightDoor.MoveToFg();
                    Parent.InCutscene = true;
                    Parent.UpdateScale(Scale);

                    Beam.Parent = Parent;
                    Beam.Sending = false;
                    Beam.Position = Parent.Floor.TopCenter;
                    Beam.AddPulses();
                }
            }
            private IEnumerator Routine(Player player)
            {
                Player = player;
                yield return player.DummyWalkToExact((int)Parent.CenterX);
                yield return Parent.OutroRoutine(player);
                Beam = new WarpBeam(Parent);
                Scene.Add(Beam);
                while (!Beam.ReadyForScale)
                {
                    yield return null;
                }
                yield return Parent.ScaleFirst();
                AddTag(Tags.Global);
                Beam.AddTag(Tags.Global);
                PlayerPosSave = player.Position - Parent.Position;
                DoorPercentSave = Parent.DoorPercent;
                Beam.EmitBeam(10, (int)Parent.Width, this);
                yield return null;
                InstantTeleport(Level, player, Parent, TeleportCleanUp);
                yield return null;
                while (!Beam.Finished)
                {
                    yield return null;
                }
                yield return PianoUtils.Lerp(null, 0.4f, f => Parent.ShineAmount = 1 - f);
                Parent.ShineAmount = 0;
                yield return Parent.IntroRoutine(Player);

                CleanUp(Level, Player);
                EndCutscene(Level);
            }
            private void CleanUp(Level level, Player player)
            {
                Beam?.RemoveSelf();
                Parent = level.Tracker.GetEntity<WarpCapsule>();
                if (Parent != null)
                {
                    if (Parent.Primary)
                    {
                        Parent.InstantCloseDoors();
                    }
                    else
                    {
                        Parent.InstantOpenDoors();
                    }
                    Parent.ShineAmount = 0;
                    Parent.LeftDoor.MoveToBg();
                    Parent.RightDoor.MoveToBg();
                }
                player.StateMachine.State = Player.StNormal;

            }
            public static void InstantTeleport(Level level, Player player, WarpCapsule from, Action<Level, Player> onEnd = null)
            {
                string room = PianoMapDataProcessor.WarpLinks[from.TargetID].Room;
                if (string.IsNullOrEmpty(room)) return;
                level.OnEndOfFrame += delegate
                {
                    Vector2 levelOffset = level.LevelOffset;
                    Vector2 playerPosInLevel = player.Position - level.LevelOffset;
                    Vector2 camPos = level.Camera.Position - from.Position;
                    float flash = level.flash;
                    Color flashColor = level.flashColor;
                    bool flashDraw = level.flashDrawPlayer;
                    bool doFlash = level.doFlash;
                    float zoom = level.Zoom;
                    float zoomTarget = level.ZoomTarget;
                    Facings facing = player.Facing;
                    level.Remove(player);
                    level.UnloadLevel();

                    level.Session.Level = room;
                    Session session = level.Session;
                    Level level2 = level;
                    Rectangle bounds = level.Bounds;
                    float left = bounds.Left;
                    bounds = level.Bounds;
                    session.RespawnPoint = level2.GetSpawnPoint(new Vector2(left, bounds.Top));
                    level.Session.FirstLevel = false;
                    level.LoadLevel(Player.IntroTypes.None);


                    level.Zoom = zoom;
                    level.ZoomTarget = zoomTarget;
                    level.flash = flash;
                    level.flashColor = flashColor;
                    level.doFlash = doFlash;
                    level.flashDrawPlayer = flashDraw;
                    player.Position = level.LevelOffset + playerPosInLevel;
                    if (level.Tracker.GetEntity<WarpCapsule>() is var r)
                    {
                        level.Camera.Position = r.Position + camPos;
                    }
                    else
                    {
                        level.Camera.Position = level.LevelOffset + camPos;
                    }
                    player.Facing = facing;
                    player.Hair.MoveHairBy(level.LevelOffset - levelOffset);
                    if (level.Wipe != null)
                    {
                        level.Wipe.Cancel();
                    }

                    onEnd?.Invoke(level, player);
                };
            }
        }
        public class Door : Entity
        {
            private class Lock : Entity
            {
                public Image Image;
                public Door Door;
                public Lock(Door door) : base(door.Position)
                {
                    Door = door;
                    Depth = 1;
                    Image = new Image(GFX.Game[Path + "lock"]);
                    Image.JustifyOrigin(0.5f, 1);
                    Image.Scale.X = door.xScale;
                    Collider = new Hitbox(Image.Width, Image.Height, -Image.Width / 2, -Image.Height / 2);
                    Image.Position.Y += Height / 2;
                    Add(Image);
                }
                public override void Render()
                {
                    Position = Door.Position;
                    base.Render();
                }
            }
            public Image Image;
            private Lock LockPlate;
            public Vector2 Scale = Vector2.One;
            public float xScale;
            public Vector2 Orig;
            private float xOffset;
            public Door(Vector2 position, int xScale, float xOffset) : base(position)
            {
                Depth = 2;
                Orig = position;
                this.xScale = xScale;
                this.xOffset = xOffset * xScale;
                Image = new Image(GFX.Game[Path + "doorFill00"]);
                Image.JustifyOrigin(0.5f, 1);
                Image.Scale.X = xScale;
                Collider = new Hitbox(Image.Width, Image.Height, -Image.Width / 2, -Image.Height / 2);
                Image.Position.Y += Height / 2;
                Add(Image);
                LockPlate = new Lock(this);
            }
            public override void Added(Scene scene)
            {
                base.Added(scene);
                scene.Add(LockPlate);
            }
            public override void Render()
            {
                ChangeTexture(Scale.Y >= 1.4f);
                Image.Scale = LockPlate.Image.Scale = new Vector2(Scale.X * xScale, Scale.Y);
                base.Render();
            }
            public override void Removed(Scene scene)
            {
                base.Removed(scene);
                LockPlate.RemoveSelf();
            }
            public void ChangeTexture(bool extend)
            {
                Image.Texture = GFX.Game[Path + "doorFill0" + (extend ? 1 : 0)];
            }
            public void SetTo(float percent)
            {
                Position.X = (int)Math.Round(Orig.X + xOffset * (1 - percent));
            }
            public void MoveToFg()
            {
                Depth = -2;
                LockPlate.Depth = -3;
            }
            public void MoveToBg()
            {
                Depth = 2;
                LockPlate.Depth = 1;
            }
        }
        [OnLoad]
        public static void Load()
        {
            On.Celeste.Player.Render += Player_Render;
        }
        [OnUnload]
        public static void Unload()
        {
            On.Celeste.Player.Render -= Player_Render;
        }

        private static void Player_Render(On.Celeste.Player.orig_Render orig, Player self)
        {
            Vector2 prevScale = self.Sprite.Scale;
            self.Sprite.Scale *= Scale;
            orig(self);
            self.Sprite.Scale = prevScale;
        }
    }

}