using Celeste.Mod.CommunalHelper.Utils;
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.DEBUG;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using static Celeste.Mod.PuzzleIslandHelper.Entities.Tower.BetaTower;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Tower
{
    [CustomEntity("PuzzleIslandHelper/TowerEntrance")]
    [Tracked]
    //currently wip as I decide on the final design for the tower interior/exterior
    public class TowerEntrance : Entity
    {
        [Command("give_tower_key", "")]
        public static void GiveTowerKey(bool heart)
        {
            PianoModule.Session.TowerKeys.Add(new() { Heart = heart });
        }
        public class floorPlatform : JumpThru
        {
            public floorPlatform(Vector2 position, int width, bool safe) : base(position, width, safe)
            {
            }
            public override void Update()
            {
                base.Update();
                Player player = Scene.GetPlayer();
                if (player != null)
                {
                    CenterX = player.CenterX;
                }
            }
        }
        public floorPlatform floor;
        public Vector2 BottomCenterSpawnOffset => Vector2.UnitX * (int)Facing * (Width / 2 + WalkInDist);
        private class Transition : CutsceneEntity
        {
            private float alpha = 0;
            private TowerEntrance entrance;
            private TowerEntranceData toData;
            private Player player;
            private LevelData toRoomData;
            private bool dialogue;
            private bool firstTime;
            public Transition(bool dialogue, bool shake, TowerEntrance from, TowerEntranceData toData, Player player) : base()
            {
                this.firstTime = shake;
                this.dialogue = dialogue;
                entrance = from;
                Depth = int.MinValue + 10;
                this.toData = toData;
                this.player = player;
            }
            public override void OnBegin(Level level)
            {
                if (dialogue && !entrance.IsExit)
                {
                    player.StateMachine.State = Player.StDummy;
                    Add(new Coroutine(dialogueRoutine()));
                }
                else
                {
                    if (toData != null)
                    {
                        foreach (var a in level.Session.MapData.Levels)
                        {
                            if (a.Name == toData.Room)
                            {
                                toRoomData = a;
                                break;
                            }
                        }
                    }
                    if (toRoomData != null)
                    {
                        player.StateMachine.State = Player.StDummy;
                        player.DummyGravity = false;
                        if (entrance.floor == null)
                        {
                            entrance.SpawnFloor(Level, player);
                        }
                        Add(new Coroutine(routine()));
                    }
                }
            }
            private IEnumerator dialogueRoutine()
            {
                yield return Textbox.Say("a key is needed");
                EndCutscene(Level);
            }
            private IEnumerator routine()
            {
                if (firstTime && !entrance.IsExit)
                {
                    yield return 0.1f;
                    entrance.KeyPresent = true;
                    yield return 1f;
                    entrance.ShakeFor(2);
                    yield return 2f;
                }
                else
                {
                    entrance.ShakeFor(0.5f);
                    yield return 0.5f;
                }
                entrance.floor.Collidable = true;
                Level.SolidTiles.Collidable = false;
                yield return entrance.EnterRoutine(player);
                for (float i = 0; i < 1; i += Engine.DeltaTime)
                {
                    alpha = i;
                    yield return null;
                }
                alpha = 1;
                AddTag(Tags.Global);
                PianoUtils.InstantTeleport(Level, toData.Room, toRoomData.Position + toData.PositionInRoom + new Vector2((entrance.Width / 2) + (int)toData.Facing * (WalkInDist + entrance.Width / 2), toData.Height), OnTeleportEnd);
                yield return null;
                RemoveTag(Tags.Global);
                yield return 0.5f;
                for (float i = 0; i < 1; i += Engine.DeltaTime)
                {
                    alpha = 1 - i;
                    yield return null;
                }
                alpha = 0;
                if (entrance.Scene == Level)
                {
                    yield return entrance.ExitRoutine(player);
                }
                EndCutscene(Level);
            }

            private void OnTeleportEnd(Level level, Player player)
            {
                this.player = player;
                Level = level;
                player.StateMachine.State = Player.StDummy;
                foreach (TowerEntrance entrance in level.Tracker.GetEntities<TowerEntrance>())
                {
                    if (entrance.ID == this.entrance.DestinationID)
                    {
                        this.entrance = entrance;
                        Vector2 bottomCenter = entrance.BottomCenter - entrance.BottomCenterSpawnOffset;
                        player.BottomCenter = bottomCenter;
                        entrance.Hider.Disabled = false;
                        entrance.Shade.Amount = 0.8f;
                        if (entrance.floor == null) entrance.SpawnFloor(level, player);
                        entrance.floor.TopCenter = player.BottomCenter;
                        entrance.floor.Collidable = true;
                        level.SolidTiles.Collidable = false;
                        break;
                    }
                }
            }
            public override void OnEnd(Level level)
            {
                entrance.floor.Collidable = false;
                level.SolidTiles.Collidable = true;
                if (!dialogue)
                {
                    if (WasSkipped)
                    {
                        if (entrance.ID != toData.ID)
                        {
                            foreach (var a in level.Session.MapData.Levels)
                            {
                                if (a.Name == toData.Room)
                                {
                                    toRoomData = a;
                                    break;
                                }
                            }
                            if (toRoomData != null)
                            {
                                level.OnEndOfFrame += () =>
                                {
                                    level.TeleportTo(player, toRoomData.Name, Player.IntroTypes.Transition, toData.PositionInRoom);
                                    player.StateMachine.State = Player.StNormal;
                                    foreach (TowerEntrance e in level.Tracker.GetEntities<TowerEntrance>())
                                    {
                                        if (e.ID == toData.ID)
                                        {
                                            player.BottomCenter = e.BottomCenter;
                                            break;
                                        }
                                    }
                                };
                            }
                        }
                        else
                        {
                            entrance.coroutine.Cancel();
                            entrance.YOffset = 0;
                            entrance.Hider.Disabled = true;
                            entrance.Shade.Amount = 0;
                            Player player = level.GetPlayer();
                            player.BottomCenter = entrance.BottomCenter;
                            player.Collidable = true;
                            player.StateMachine.State = Player.StNormal;
                        }
                    }
                    else
                    {
                        player.StateMachine.State = Player.StNormal;
                    }
                }
                else
                {
                    player.StateMachine.State = Player.StNormal;
                }
            }
            public override void Render()
            {
                base.Render();
                if (alpha > 0)
                {
                    Draw.Rect(SceneAs<Level>().Camera.Position, 320, 180, Color.Black * alpha);
                }
            }
        }
        public Facings Facing;
        public enum States
        {
            Decayed,
            Young
        }
        public States State;
        public bool IsExit;
        public bool RequiresKey;
        public bool KeyPresent;
        public PlayerHider Hider;
        public PlayerShade Shade;
        public string DestinationID;
        public FlagList TriggerCutscene;
        public string CutsceneID;
        public string ID;
        public bool CutsceneOnEnter;
        public FlagList FlagsOnCutscene;
        public MTexture[] Symbols;
        private MTexture doorTop;
        private MTexture doorMiddle;
        public float DoorScaleFactor;
        public float YOffset
        {
            get => yoffset;
            set
            {
                if (yoffset != value) Dirty = true;
                yoffset = value;
            }
        }
        private float yoffset;
        public VirtualRenderTarget Texture;
        public VirtualRenderTarget Back;
        private bool backDrawn;
        public bool Dirty = true;
        public bool Animating;
        private Coroutine coroutine;
        public TalkComponent Talk;
        public EntityID EntityID;
        private TowerEntranceData toData;
        public bool ToDataExists => toData != null;
        private float shakeTimer;
        private int shakeMult = 1;
        private int shakeOffset;
        private SpriteEffects effects;
        public PianoModuleSession.TowerKeyData Key;
        public TowerEntrance(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
        {
            Depth = 10;
            EntityID = id;
            Texture = VirtualContent.CreateRenderTarget("tower door", data.Width, data.Height);
            Back = VirtualContent.CreateRenderTarget("tower door back", data.Width, data.Height);
            Facing = data.Enum<Facings>("facing");
            effects = Facing switch
            {
                Facings.Right => SpriteEffects.None,
                _ => SpriteEffects.FlipHorizontally
            };
            State = data.Enum<States>("state");
            IsExit = data.Bool("exit");
            RequiresKey = data.Bool("requiresKey");
            ID = data.Attr("entranceID");
            DestinationID = data.Attr("destinationID");
            CutsceneID = data.Attr("cutsceneID");
            CutsceneOnEnter = data.Bool("cutsceneOnEnter");
            TriggerCutscene = data.FlagList("cutsceneTriggerFlags");
            FlagsOnCutscene = data.FlagList("flagsOnCutscene");
            Collider = new Hitbox(24, data.Height);
            Vector2 hiderOffset = default;
            hiderOffset.X = Facing == Facings.Right ? -40 : data.Width;
            hiderOffset.Y = 0;
            Hider = new PlayerHider(hiderOffset, 40, data.Height);
            Hider.Disabled = true;
            Shade = new PlayerShade();
            Add(new BeforeRenderHook(BeforeRender));
            Rectangle talkBounds = default;
            if (Facing == Facings.Right)
            {
                talkBounds.X = 4;
                talkBounds.Width = data.Width - 4;
            }
            else
            {
                talkBounds.Width = data.Width;
            }
            talkBounds.Height = (int)Height;
            Talk = new TalkComponent(talkBounds, Vector2.UnitX * (data.Width / 2), OnTalk);
            Add(coroutine = new Coroutine(false));
            Add(Talk);
            Add(Hider);
            Add(Shade);
        }
        public void BeforeRender()
        {
            if (Dirty)
            {
                Vector2 scale = new Vector2((int)Facing, 1);
                Texture.SetAsTarget(true);
                Draw.SpriteBatch.Begin();
                doorTop.Draw(Vector2.UnitY * YOffset, Vector2.Zero, Color.Gray, 1, 0, effects);
                doorMiddle.Draw(Vector2.UnitY * (8 + YOffset), Vector2.Zero, Color.Gray, new Vector2(1, DoorScaleFactor), 0, effects);
                if (RequiresKey && !IsExit)
                {
                    MTexture symbol = KeyPresent ? Symbols[1] : Symbols[0];
                    symbol.DrawCentered(Collider.HalfSize + Vector2.UnitY * YOffset, Color.White, scale);
                }
                Draw.SpriteBatch.End();
                Dirty = false;
                if (!backDrawn)
                {
                    Engine.Graphics.GraphicsDevice.SetRenderTarget(Back);
                    Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
                    Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, RasterizerState.CullNone);
                    //Render normal stuff here
                    Draw.SpriteBatch.Draw(Texture, Vector2.Zero, Color.White);
                    Draw.SpriteBatch.End();
                    Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, TrailManager.MaxBlendState);
                    //blend state forces the buffer to combine the colors in such a way that the result is always white (with alpha preserved)
                    Draw.Rect(0f, 0f, Back.Width, Back.Height, new Color(1f, 1f, 1f, 1f));
                    Draw.SpriteBatch.End();
                    backDrawn = true;
                }
            }
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);

        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (floor == null && scene.GetPlayer() is Player player)
            {
                SpawnFloor(scene, player);
            }
            string path = "objects/PuzzleIslandHelper/towerLock/";
            Symbols = [.. GFX.Game.GetAtlasSubtextures(path + (State == States.Decayed ? "heartSlant" : "sigilSlant"))];
            MTexture door = GFX.Game[path + "door"];
            doorTop = door.GetSubtexture(0, 0, 24, 8);
            doorMiddle = door.GetSubtexture(0, 8, 24, 8);
            DoorScaleFactor = (Height - 8) / 8f;
            var key = scene.GetAreaKey();
            if (!string.IsNullOrEmpty(DestinationID) && PianoMapDataProcessor.TowerEntrances.ContainsKey(key))
            {
                PianoMapDataProcessor.TowerEntrances[key].TryGetValue(DestinationID, out toData);
            }
            KeyPresent = PianoModule.Session.FindTowerKeyLinkedToId(EntityID) != null;

        }
        public override void Render()
        {
            base.Render();
            if (!Dirty)
            {
                if (YOffset != 0)
                {
                    Draw.SpriteBatch.Draw(Back, Position + Vector2.UnitX * shakeOffset, IsExit ? Color.White : Color.Black);
                }
                if (YOffset > -Height)
                {
                    Draw.SpriteBatch.Draw(Texture, Position + Vector2.UnitX * shakeOffset, Color.White);
                }
            }
        }
        public override void Update()
        {
            if (shakeTimer > 0)
            {
                shakeTimer -= Engine.DeltaTime;
                if (shakeTimer <= 0)
                {
                    shakeTimer = 0;
                    shakeOffset = 0;
                }
            }
            if (shakeTimer != 0)
            {
                if (Scene.OnInterval(0.05f))
                {
                    shakeMult *= -1;
                }
                shakeOffset = shakeMult;
            }
            Talk.Enabled = !Animating && toData != null;
            base.Update();
            float shadeValue = 0;
            Player player = Scene.GetPlayer();
            if (player != null)
            {
                if (Hider.HidingPlayer)
                {
                    shadeValue = 0.8f;
                }
            }
            Shade.Amount = Calc.Approach(Shade.Amount, shadeValue, Engine.DeltaTime * 3f);
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Back.Dispose();
            Texture.Dispose();
        }
        public void OnTalk(Player player)
        {
            PianoModuleSession.TowerKeyData existingKeyData = Key ?? PianoModule.Session.FindTowerKeyLinkedToId(EntityID);
            if (existingKeyData != null || !RequiresKey)
            {
                //unlocked, proceed normally
                Activate(player, existingKeyData, false);
            }
            else
            {
                //search for key
                PianoModuleSession.TowerKeyData getKey = PianoModule.Session.FindUnusedTowerKey(State == States.Decayed);
                //if no usable key exists, play dialogue. Otherwise, play firstTime sequence
                Activate(player, getKey, getKey != null);
            }
        }
        public void Activate(Player player, PianoModuleSession.TowerKeyData key, bool firstTime)
        {
            Key = key;
            if (key != null)
            {
                key.LinkedEntityIDs.Add(EntityID);
            }
            Scene.Add(new Transition(key == null, firstTime, this, toData, player));
        }
        public void Enter(Player player)
        {
            coroutine.Replace(EnterRoutine(player));
        }
        public void Exit(Player player)
        {
            coroutine.Replace(ExitRoutine(player));
        }
        public void ShakeFor(float time = -1)
        {
            shakeTimer = time;
        }
        public const int WalkInDist = 30;
        public IEnumerator EnterRoutine(Player player)
        {
            Animating = true;
            player.Facing = (Facings)(-(int)Facing);
            yield return openDoorRoutine();
            Hider.Disabled = false;
            yield return 0.5f;
            float x = CenterX - BottomCenterSpawnOffset.X;
            yield return player.DummyWalkTo(x);
            coroutine.Replace(closeDoorRoutine());
        }
        private IEnumerator closeDoorRoutine(float delay = 0)
        {
            Animating = true;
            if (delay > 0) yield return delay;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 0.8f)
            {
                YOffset = -Height + Ease.CubeOut(i) * Height;
                yield return null;
            }
            YOffset = 0;
            Animating = false;
        }
        private IEnumerator openDoorRoutine()
        {
            Animating = true;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 0.8f)
            {
                YOffset = Ease.CubeIn(i) * -Height;
                yield return null;
            }
            YOffset = -Height;
        }
        public IEnumerator ExitRoutine(Player player)
        {
            Animating = true;
            Hider.Disabled = false;
            Shade.Amount = 0.8f;
            player.Facing = Facing;
            yield return openDoorRoutine();
            yield return 0.5f;
            yield return player.DummyWalkTo(CenterX);
            Hider.Disabled = true;
            coroutine.Replace(closeDoorRoutine(0.5f));
        }
        public void SpawnFloor(Scene scene, Player player)
        {
            floor = new floorPlatform(player.BottomCenter - Vector2.UnitX * (int)Facing * 8, 16, true);
            floor.Collidable = false;
            scene.Add(floor);
        }

    }
}