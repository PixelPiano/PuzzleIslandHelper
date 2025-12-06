using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

// PuzzleIslandHelper.LabDoor
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/KeyDoor")]
    [Tracked]
    public class KeyDoor : Entity
    {
        public MTexture TopPart;
        public MTexture BottomPart;
        public MTexture Sigil;
        public float YLerp;
        public float FlashLerp;
        public enum Modes
        {
            Keys,
            Flag
        }
        public Modes Mode;
        public FlagList Flag;
        public EntityID ID;
        public string Room;
        public string MarkerID;
        private VirtualRenderTarget target;
        private DotX3 Talk;
        private float limit = 0.7f;

        public static int KeysLeft => PianoModule.Session.KeysObtained - PianoModule.Session.KeysUsed;
        public bool Registered => SceneAs<Level>().Session.GetFlag("KeyDoor:" + MarkerID);
        public KeyDoor(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
        {
            Depth = 4;
            Mode = data.Enum<Modes>("mode");
            Flag = data.FlagList("flag");
            ID = id;
            Room = data.Attr("room");
            MarkerID = data.Attr("marker");
            Add(new BeforeRenderHook(() =>
            {
                target.SetAsTarget(true);
                if (YLerp < 1)
                {
                    Draw.SpriteBatch.Begin();
                    Draw.Rect(0, 0, Width, Height, Color.Black);
                    TopPart.Draw(-Vector2.UnitY * YLerp * Height);
                    BottomPart.Draw(Vector2.UnitY * YLerp * Height);
                    if (FlashLerp * masterFade > 0)
                    {
                        Sigil.Draw(-Vector2.UnitY * YLerp * Height, Vector2.Zero, Color.White * FlashLerp * masterFade);
                    }
                    Draw.SpriteBatch.End();
                }
            }));
            Add(coroutine = new Coroutine(false));
        }
        public override void Render()
        {
            base.Render();
            if (YLerp < 1)
            {
                Draw.SpriteBatch.Draw(target, Position, Color.White);
            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            target?.Dispose();
        }
        public bool HasKey => KeysLeft != 0;
        public bool RequiresFlag => Mode == Modes.Flag;
        public bool RequiresKey => Mode == Modes.Keys;
        public bool LoadOpened => (RequiresFlag && Flag) || Registered;
        public bool CanTalk => !opening && (!Closed || (HasKey && RequiresKey) || (Flag && RequiresFlag));
        public bool CanFade => !Closed || (HasKey && RequiresKey) || (Flag && RequiresFlag);
        public bool Closed => YLerp == 0;
        private float masterFade;
        public override void Update()
        {
            base.Update();
            if (Talk != null)
            {
                Talk.Enabled = CanTalk;
            }
            if (CanFade && Closed)
            {
                masterFade = Calc.Approach(masterFade, 1, Engine.DeltaTime);
            }
            else
            {
                masterFade = Calc.Approach(masterFade, 0, Engine.DeltaTime / 2);
            }
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (CanFade)
            {
                masterFade = 1;
            }
            TopPart = GFX.Game["objects/PuzzleIslandHelper/keyDoor/top"];
            BottomPart = GFX.Game["objects/PuzzleIslandHelper/keyDoor/bottom"];
            Sigil = GFX.Game["objects/PuzzleIslandHelper/keyDoor/sigil"];
            Collider = TopPart.Collider();
            target = VirtualContent.CreateRenderTarget("keyDoorTarget", (int)Width, (int)Height);
            if (LoadOpened)
            {
                Open(true);
            }
            Add(new Coroutine(fadeRoutine()));
            Add(Talk = new DotX3(Collider, p =>
                {
                    if (RequiresFlag && Flag)
                    {
                        if (!Closed)
                        {
                            TalkingDecal.Teleport(Scene.GetPlayer(), MarkerID, Room);
                        }
                        else
                        {
                            Input.Dash.ConsumePress();
                            Open(false);
                        }
                    }
                    else if (RequiresKey)
                    {
                        if (!Registered)
                        {
                            if (KeysLeft > 0)
                            {
                                PianoModule.Session.KeysUsed++;
                                SceneAs<Level>().Session.SetFlag("KeyDoor:" + MarkerID);
                                Input.Dash.ConsumePress();
                                Open(false);
                            }
                        }
                        else
                        {
                            TalkingDecal.Teleport(Scene.GetPlayer(), MarkerID, Room);
                        }
                    }

                }));
            Talk.PlayerMustBeFacing = false;
        }
        private IEnumerator fadeRoutine()
        {
            while (true)
            {
                yield return 0.5f;
                yield return PianoUtils.Lerp(Ease.SineInOut, 1.5f, f => FlashLerp = f, true);
                yield return 0.5f;
                yield return PianoUtils.ReverseLerp(Ease.SineInOut, 1.5f, f => FlashLerp = f, true);
            }
        }
        [Command("give_pi_key", "")]
        public static void GiveKey(int num = 1)
        {
            PianoModule.Session.KeysObtained+= num;
        }
        public class WipeTeleport : Entity
        {
            public static WipeTeleport Create(Player player, string room, string marker = "")
            {
                WipeTeleport t = new(player, room, marker);
                Engine.Scene?.Add(t);
                return t;
            }
            private Player player;
            private string room;
            private string marker;
            public WipeTeleport(Player player, string room, string marker)
            {
                Tag |= Tags.Persistent;
                Tag |= Tags.TransitionUpdate;
                this.player = player;
                this.room = room;
                this.marker = marker;

            }
            public override void Awake(Scene scene)
            {
                base.Awake(scene);
                player.DisableMovement();
                Vector2? position = null;
                if (Marker.TryFind(marker, out Vector2 position2))
                {
                    position = position2;
                }
                Scene.OnEndOfFrame += () =>
                {
                    SceneAs<Level>().TeleportTo(player, room, Player.IntroTypes.None, position);
                };
            }
        }
        private bool opening;
        public void Open(bool instant = false)
        {
            if (instant)
            {
                YLerp = limit;
                masterFade = 0;
            }
            else
            {
                coroutine.Replace(openRoutine());
            }
        }
        private Coroutine coroutine;
        private IEnumerator openRoutine()
        {
            opening = true;
            yield return new SwapImmediately(PianoUtils.Lerp(Ease.Linear, 0.2f, f =>
            {
                YLerp = f * 0.1f;
            }));
            yield return 0.2f;
            yield return new SwapImmediately(PianoUtils.Lerp(Ease.SineInOut, 1, f =>
            {
                YLerp = Calc.LerpClamp(0.1f, limit, f);
            }));
            YLerp = limit;
            opening = false;
        }
        public void Close()
        {
            YLerp = 0;
            coroutine?.Cancel();
            opening = false;
        }
    }
}
