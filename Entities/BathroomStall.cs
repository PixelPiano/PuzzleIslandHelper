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
using System.Security.Cryptography.X509Certificates;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/BathroomStall")]
    [Tracked]
    public class BathroomStall : Entity
    {
        public Solid[] Walls = new Solid[2];
        public bool StartFG;
        public bool CanEnter;
        public class Door : Entity
        {
            public bool Opened;
            public bool Failing;
            private static string path = "objects/PuzzleIslandHelper/bathroom/stallDoor0";
            public static MTexture ClosedTex => GFX.Game[path + '0'];
            public static MTexture OpenTex => GFX.Game[path + '1'];
            public static MTexture FailTex => GFX.Game[path + '2'];
            public Image image;
            public Door(Vector2 position) : base(position)
            {
                Depth = 3;
                Add(image = new Image(ClosedTex));
            }
            public void Open()
            {
                Opened = true;
                Depth = 3;
                image.X = 0;
                image.Texture = OpenTex;
            }
            public void CloseFail()
            {
                Failing = true;
                Depth = 2;
                image.Texture = FailTex;
                image.X = 0;
                Alarm.Set(this, 0.7f, delegate
                {
                    image.Texture = OpenTex;
                    Failing = false;
                    Opened = true;
                    Depth = 3;
                    image.X = 0;
                });
            }
            public void Close()
            {
                Opened = false;
                Depth = 4;
                image.X = 3;
                image.Texture = ClosedTex;
            }
            public void FG()
            {
                Depth = -100;
            }
            public void BG()
            {
                Depth = Opened ? 3 : 4;
            }
        }
        public class Barriers : Entity
        {
            public static MTexture ClosedTex => GFX.Game["objects/PuzzleIslandHelper/bathroom/stallSides00"];
            public static MTexture OpenTex => GFX.Game["objects/PuzzleIslandHelper/bathroom/stallSides01"];
            public Image image;
            public Barriers(Vector2 position) : base(position)
            {
                Add(image = new Image(ClosedTex));
                Depth = 5;
            }
            public void Open()
            {
                image.Texture = OpenTex;
            }
            public void Close()
            {
                image.Texture = ClosedTex;
            }
        }
        public Door StallDoor;
        public Barriers StallSides;
        public EntityID ID;
        public string Name;
        public bool StartOpen;
        public bool IsOpen;
        public bool Blocked;
        public float OpenWidth;
        public float ClosedWidth;
        public DotX3 Talk;
        public bool PlayerInside;
        public Collider OpenDoorCollider;
        public Collider ClosedDoorCollider;
        public Collider InsideCollider;
        public Collider ClosedTalkCollider;
        public Collider OpenedTalkCollider;
        public BathroomStallComponent BlockChecker;
        private static Dictionary<TalkComponent.TalkComponentUI, float> hiddenMults = new();
        private const float hideRate = 0.25f;
        public BathroomStall(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
        {
            StartFG = data.Bool("startFG");
            ID = id;
            Name = data.Attr("name");
            StartOpen = data.Bool("startOpen");
            OpenWidth = Door.OpenTex.Width;
            ClosedWidth = Barriers.OpenTex.Width;
        }
        private static void TalkComponent_Update(On.Celeste.TalkComponent.orig_Update orig, TalkComponent self)
        {
            if (!TalkCovered(self))
            {
                if (self.UI != null)
                {
                    if (hiddenMults.ContainsKey(self.UI))
                    {
                        hiddenMults[self.UI] += hideRate;
                        if (hiddenMults[self.UI] >= 1)
                        {
                            hiddenMults.Remove(self.UI);
                        }
                    }
                }
                orig(self);
            }
            else if (self.UI != null)
            {
                if (!hiddenMults.ContainsKey(self.UI))
                {
                    hiddenMults.Add(self.UI, 1);
                }
                else
                {
                    hiddenMults[self.UI] = Math.Max(hiddenMults[self.UI] - hideRate, 0);
                }
            }
        }
        public static bool TalkCovered(TalkComponent self)
        {
            if (self.Entity != null && self.Entity is not BathroomStall && Engine.Scene is Level level)
            {
                Rectangle rect = new Rectangle((int)self.Entity.X + self.Bounds.X, (int)self.Entity.Y + self.Bounds.Y, self.Bounds.Width, self.Bounds.Height);
                if (level.CollideFirst<BathroomStall>(rect) is not null)
                {
                    return true;
                }
            }
            return false;
        }
        public void OnBlocked()
        {
            if (!IsOpen) Blocked = true;
        }
        public void OnUnblocked()
        {
            Blocked = false;
        }
        public void ActivateWalls()
        {
            if (Walls[0] != null && Walls[1] != null)
            {
                Walls[0].Collidable = Walls[1].Collidable = true;
            }
        }
        public void DeactivateWalls()
        {
            if (Walls[0] != null && Walls[1] != null)
            {
                Walls[0].Collidable = Walls[1].Collidable = false;
            }
        }
        public override void Update()
        {
            base.Update();
            Talk.Enabled = !(Blocked || StallDoor.Failing);
            if (PlayerInside)
            {
                Collider prev = Collider;
                Collider = InsideCollider;
                if (!CollideCheck<Player>())
                {
                    PlayerInside = false;
                    DeactivateWalls();
                    StallDoor.BG();
                }
                Collider = prev;

            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            StallSides = new(Position);
            StallDoor = new(Position - new Vector2(Door.OpenTex.Width - Barriers.OpenTex.Width, 0));
            if (StartFG)
            {
                if (PianoModule.Session.BathroomStallOpened)
                {
                    StallDoor.BG();
                }
                else
                {
                    StallDoor.FG();
                }
            }
            scene.Add(StallSides, StallDoor);
            ClosedDoorCollider = new Hitbox(ClosedWidth, Barriers.ClosedTex.Height);
            OpenDoorCollider = new Hitbox(OpenWidth - ClosedWidth, Door.OpenTex.Height, ClosedWidth - OpenWidth + 3);
            Add(BlockChecker = new BathroomStallComponent(ClosedDoorCollider, OnBlocked, OnUnblocked));
            ClosedTalkCollider = new Hitbox(34, 8, 3, ClosedDoorCollider.Height - 8);
            OpenedTalkCollider = new Hitbox(OpenWidth - ClosedWidth + (CanEnter ? 20 : 0), 8, ClosedWidth - OpenWidth + 3, ClosedDoorCollider.Height - 8);
            Add(Talk = new DotX3(ClosedTalkCollider, Interact));
            Talk.PlayerMustBeFacing = false;
            InsideCollider = new Hitbox(ClosedDoorCollider.Width - 2, Height + 16, 1, -16);
            if (StartOpen)
            {
                SetFlag(true);
            }
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (GetFlag())
            {
                Open();
            }
            else
            {
                Close();
            }
            Collider = IsOpen ? OpenDoorCollider : ClosedDoorCollider;

            Walls[0] = new Solid(Position + Vector2.UnitY, 3, Height - 1, true);
            Walls[1] = new Solid(Walls[0].Position + (Vector2.UnitX * (ClosedWidth - 3)), 3, Height - 1, true);
            scene.Add(Walls);
            Walls[0].Collidable = Walls[1].Collidable = false;
        }
        public static bool Cooldown;
        public static void Buffer(Entity entity)
        {
            Cooldown = true;
            Alarm.Set(entity, 0.1f, delegate { Cooldown = false; });
            TalkBlock.DisableFor(0.3f);
            Input.Dash.ConsumePress();
        }
        public void Interact(Player player)
        {
            if (Cooldown) return;
            Buffer(this);
            //todo: ADD FUNNY METAL SOUNDS
            if (IsOpen)
            {
                Close();
            }
            else
            {
                Open();
            }
        }
        public void UpdateComponents()
        {
            foreach (BathroomStallComponent c in Scene.Tracker.GetComponents<BathroomStallComponent>())
            {
                c.UpdateComponent();
            }
        }

        public void Open()
        {
            IsOpen = true;
            Collider = OpenDoorCollider;
            StallDoor.Open();
            StallDoor.BG();
            StallSides.Open();
            DeactivateWalls();
            SetFlag(true);
            UpdateComponents();
            Talk.Bounds.X = (int)OpenedTalkCollider.Position.X;
            Talk.Bounds.Y = (int)OpenedTalkCollider.Position.Y;
            Talk.Bounds.Width = (int)OpenedTalkCollider.Width;
            Talk.Bounds.Height = (int)OpenedTalkCollider.Height;
            Talk.DrawAt = OpenedTalkCollider.Position + Vector2.UnitX * OpenedTalkCollider.HalfSize.X;
            foreach (BathroomStall stall in Scene.Tracker.GetEntities<BathroomStall>())
            {
                if (stall != this && !stall.IsOpen && CollideCheck(stall))
                {
                    stall.StallDoor.BG();
                }
            }
        }
        public void Close()
        {

            if (StallDoor.Failing) return;
            if (BlockChecker.Check())
            {
                StallDoor.CloseFail();
                return;
            }
            IsOpen = false;
            StallDoor.Close();
            StallSides.Close();
            Collider = ClosedDoorCollider;
            Talk.Bounds.X = (int)ClosedTalkCollider.Position.X;
            Talk.Bounds.Y = (int)ClosedTalkCollider.Position.Y;
            Talk.Bounds.Width = (int)ClosedTalkCollider.Width;
            Talk.Bounds.Height = (int)ClosedTalkCollider.Height;
            Talk.DrawAt = ClosedTalkCollider.Position + Vector2.UnitX * ClosedTalkCollider.HalfSize.X;
            if (CollideCheck<Player>())
            {
                ActivateWalls();
                StallDoor.FG();
                PlayerInside = true;
            }
            else
            {
                PlayerInside = false;
                DeactivateWalls();
                StallDoor.BG();
            }
            SetFlag(false);
            UpdateComponents();
        }
        public void SetFlag(bool value)
        {
            if (!string.IsNullOrEmpty(Name))
            {
                SceneAs<Level>().Session.SetFlag("BathroomStall:" + Name, value);
            }
        }
        public bool GetFlag()
        {
            if (string.IsNullOrEmpty(Name))
            {
                return false;
            }
            return SceneAs<Level>().Session.GetFlag("BathroomStall:" + Name);
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            StallSides?.RemoveSelf();
            StallDoor?.RemoveSelf();
            Walls[0].RemoveSelf();
            Walls[1].RemoveSelf();
        }
        [OnLoad]
        public static void Load()
        {
            hiddenMults.Clear();
            On.Celeste.TalkComponent.Update += TalkComponent_Update;
            On.Celeste.TalkComponent.TalkComponentUI.Render += TalkComponentUI_Render;
        }

        [OnUnload]
        public static void Unload()
        {
            hiddenMults.Clear();
            On.Celeste.TalkComponent.Update -= TalkComponent_Update;
            On.Celeste.TalkComponent.TalkComponentUI.Render -= TalkComponentUI_Render;
        }
        private static void TalkComponentUI_Render(On.Celeste.TalkComponent.TalkComponentUI.orig_Render orig, TalkComponent.TalkComponentUI self)
        {
            float prev = self.alpha;
            if (hiddenMults.ContainsKey(self))
            {
                self.alpha *= hiddenMults[self];
            }
            orig(self);
            self.alpha = prev;
        }
    }
}