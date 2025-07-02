using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Cutscenes;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.WARP
{

    [Tracked]
    public class CapsuleWarpHandler : Entity
    {

        public WarpCapsule Parent;
        public WarpData Data;
        public WarpBeam Beam;
        public Player Player;
        public Calidus Calidus;
        public Vector2 PlayerPosSave;
        public Vector2 NewPosition;
        private Vector2? CalidusPosSave;
        public EntityID FirstParentID;
        public float DoorPercentSave;
        public bool Returning;
        public bool Teleported;
        private Action onEnd;
        private bool pull;
        public CapsuleWarpHandler(WarpCapsule parent, WarpData data, Player player, Action onEnd = null,  bool emergencyPull = false) : base()
        {
            pull = emergencyPull;
            Data = data;
            FirstParentID = parent.ID;
            Parent = parent;
            Player = player;
            this.onEnd = onEnd;
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Parent.InCutscene = true;
            Add(new Coroutine(Routine(Player)));
        }
        public void OnEnd(Player player, Level level)
        {
            Beam?.RemoveSelf(); //remove beam if it exists
            Parent = (Scene as Level).Tracker.GetEntity<WarpCapsule>(); //set receiving machine as parent
            if (Parent != null)
            {
                Parent.InstantOpenDoors();
                Parent.ShineAmount = 0;
                Parent.LeftDoor.MoveToBg();
                Parent.RightDoor.MoveToBg();
                Parent.Scene = Scene as Level;
            }
            onEnd?.Invoke();
            player.StateMachine.State = Player.StNormal;
            Parent.InCutscene = false;
            Parent.TimesTypeUsed++;
            RemoveSelf();
        }
        private IEnumerator playerToCenter(Player player)
        {
            yield return player.DummyWalkToExact((int)Parent.CenterX);
        }
        private IEnumerator calidusToCenter(Calidus calidus)
        {
            calidus.StopFollowing();
            calidus.Add(new Coroutine(calidus.FloatHeightTo(0, 0.5f)));
            yield return calidus.FloatTo(Parent.Center - Vector2.UnitX * 16);
        }
        public virtual void BeforeBegin()
        {

        }
        private IEnumerator warp(Player player, float time)
        {
            yield return Parent.PrepareToSend(player, time); //tell capsule to start preparing for warp
            if (Data is WarpData data && data.HasRune) Returning = PianoModule.SaveData.VisitedRuneSites.Contains(data.Rune); //store if player has warped here before
            if (Parent.UsesBeam) //if cutscene uses beam, create the beam and add it to the scene
            {
                Beam = new WarpBeam(Parent, Returning);
                Scene.Add(Beam);
            }
            if (Returning) //if player has already warped here, make the cutscene shorter
            {
                yield return 1.5f;
            }
            else
            {
                if (Beam != null)
                {
                    while (!Beam.ReadyForScale) //wait for the beam to get ready
                    {
                        yield return null;
                    }
                }
                yield return Parent.SendScale(); //scale the machine
            }
            AddTag(Tags.Global); //make this cutscene global so it doesn't get removed once the player is teleported
            Beam?.AddTag(Tags.Global); //if the beam exists, make that global too
            PlayerPosSave = player.Position - Parent.Position; //save the player's position relative to the machine
            if (Scene.Tracker.GetEntity<Calidus>() is Calidus calidus)
            {
                CalidusPosSave = calidus.Position - Parent.Position;
            }
            else
            {
                CalidusPosSave = null;
            }
            DoorPercentSave = Parent.DoorClosedPercent; //save the percent the doors are closed
            if (!Returning && Beam != null)
            {
                Beam.EmitBeam(10, (int)Parent.Width, this); //if first time visited and uses beam, emit the beam effects
            }
            yield return null;

            InstantTeleport(Scene as Level, player, TeleportCleanUp); //teleport the player at the end of this frame
            yield return null; //frame buffer so we don't act before the teleport occurs
            if (!Returning)
            {
                if (Beam != null)
                {
                    while (!Beam.Finished) //wait for the beam to finish doing stuff
                    {
                        yield return null;
                    }
                }
                yield return PianoUtils.Lerp(null, 0.4f, f => Parent.ShineAmount = 1 - f);
            }
            Parent.ShineAmount = 0;
        }
        public float WarpTime = 0.8f;
        private IEnumerator Routine(Player player)
        {
            Player = player;
            Calidus = Scene.Tracker.GetEntity<Calidus>();
            WarpCapsuleCutscene introCutscene = null;
            foreach (IntroWarpCutsceneTrigger trigger in Scene.Tracker.GetEntities<IntroWarpCutsceneTrigger>())
            {
                if (trigger.Check(player, this, Parent) is WarpCapsuleCutscene e)
                {
                    trigger.Chosen = true;
                    introCutscene = e;
                    break;
                }
            }
            if (introCutscene != null)
            {
                yield return null;
                yield return 4;
                while (introCutscene.InCutscene)
                {
                    yield return null;
                }
            }
            else
            {
                if (pull)
                {
                    WarpTime = 0.2f;
                    yield return pullSequence(Parent, Player);
                }
                else
                {
                    yield return playerToCenter(Player);
                    if(Calidus != null && Calidus.Following && Parent is WarpCapsuleAlpha)
                    {
                        yield return calidusToCenter(Calidus);
                    }
                }
            }
            yield return warp(player, WarpTime);
            yield return Parent.ReceivePlayerRoutine(Player, null);

            WarpCapsuleCutscene outroCutscene = null;
            foreach (OutroWarpCutsceneTrigger trigger in Engine.Scene.Tracker.GetEntities<OutroWarpCutsceneTrigger>())
            {
                if (trigger.Check(player, this, Parent) is WarpCapsuleCutscene e)
                {
                    outroCutscene = e;
                    break;
                }
            }
            if (outroCutscene != null)
            {
                yield return null;
                while (outroCutscene.InCutscene)
                {
                    yield return null;
                }
            }
            OnEnd(Player, SceneAs<Level>());
        }
        private IEnumerator pullSequence(WarpCapsule capsule, Player player)
        {
            Vector2 beamStart = new Vector2(capsule.CenterX, capsule.Floor.Top + 1);
            while (capsule.DoorState != WarpCapsule.DoorStates.Open)
            {
                capsule.MoveAlong(Calc.Approach(capsule.DoorClosedPercent, 0, 0.2f));
                yield return null;
            }
            capsule.InstantOpenDoors();
            playerBeam beam = new playerBeam(this, capsule.Center, 0.1f, Ease.CubeIn);
            Scene.Add(beam);
            beam.Depth = -1;
            yield return PianoUtils.Lerp(Ease.CubeIn, 0.3f, f => beam.To = Vector2.Lerp(beam.From, player.Center, f));
            beam.FadeAutomatically = true;
            player.DisableMovement();
            player.Speed = Vector2.Zero;
            player.LiftSpeed = Vector2.Zero;
            player.DummyGravity = false;
            player.DummyFriction = false;
            player.DummyAutoAnimate = false;
            player.MuffleLanding = true;
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                player.BottomCenter = Vector2.Lerp(player.BottomCenter, beamStart, Ease.CubeIn(i));
                beam.To = player.BottomCenter - Vector2.UnitY * player.Collider.HalfSize.Y;
                yield return null;
            }
        }
        private void TeleportCleanUp(Level level, Player player)
        {
            Teleported = true;
            Level l = Engine.Scene as Level;
            Player = l.GetPlayer();
            if (Parent != null)
            {
                Player.StateMachine.State = Player.StDummy;
                Player.BottomCenter = Parent.Floor.TopCenter;
                Player.ForceCameraUpdate = true;
                Parent.JustTeleportedTo = true;
            }
            l.Camera.Position = Player.CameraTarget;
            l.Camera.Position.Clamp(l.Bounds);
            l.Flash(Color.White, false);

            if (Parent != null)
            {
                WARPData.Scale = Vector2.One;
                Parent.DoorClosedPercent = DoorPercentSave;
                Parent.MoveAlong(DoorPercentSave);
                Parent.LeftDoor.MoveToFg();
                Parent.RightDoor.MoveToFg();
                Parent.InCutscene = true;
                Parent.UpdateScale(WARPData.Scale);
                if (Beam != null)
                {
                    Beam.Parent = Parent;
                    Beam.Sending = false;
                    Beam.Position = Parent.Floor.TopCenter;
                }
                if (!Returning)
                {
                    Parent.ShineAmount = 1;
                    Beam?.AddPulses();
                }
            }
        }
        public void InstantTeleport(Level level, Player player, Action<Level, Player> onEnd = null)
        {
            if (string.IsNullOrEmpty(Data.Room)) return;
            level.OnEndOfFrame += delegate
            {
                FirfilStorage.Release(false);
                Vector2 levelOffset = level.LevelOffset;
                Vector2 playerPosInLevel = player.Position - level.LevelOffset;
                Vector2 camPos = level.Camera.Position;
                float flash = level.flash;
                Color flashColor = level.flashColor;
                bool flashDraw = level.flashDrawPlayer;
                bool doFlash = level.doFlash;
                float zoom = level.Zoom;
                float zoomTarget = level.ZoomTarget;
                Facings facing = player.Facing;
                level.Remove(player);
                level.UnloadLevel();

                level.Session.Level = Data.Room;
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

                foreach (WarpCapsule capsule in level.Tracker.GetEntities<WarpCapsule>())
                {
                    if (Data is WarpData data && data.HasRune && capsule is WarpCapsuleAlpha alpha)
                    {
                        Parent = alpha;
                        if (data.Rune == alpha.RuneData.Rune)
                        {
                            break;
                        }
                    }
                    else
                    {
                        Parent = capsule;
                        break;
                    }
                }

                player.BottomCenter = Parent.Floor.TopCenter;//level.LevelOffset + playerPosInLevel;
                level.Camera.Position = Parent.Position + camPos;
                player.Hair.MoveHairBy(level.LevelOffset - levelOffset);
                level.Wipe?.Cancel();

                onEnd?.Invoke(level, player);
            };
        }
        private class playerBeam : Entity
        {
            public float percent;
            private MTexture texture;
            private float alpha = 0;
            private VirtualRenderTarget target;
            private VirtualRenderTarget mask;

            public Vector2 From;
            public Vector2 To;
            private float doorMaskAlpha;
            public bool FadeAutomatically;
            private static BlendState t = new()
            {
                ColorSourceBlend = Blend.One,
                ColorBlendFunction = BlendFunction.ReverseSubtract,
                ColorDestinationBlend = Blend.One,
                AlphaSourceBlend = Blend.Zero,
                AlphaBlendFunction = BlendFunction.Add,
                AlphaDestinationBlend = Blend.Zero
            };
            private CapsuleWarpHandler handler;
            public playerBeam(CapsuleWarpHandler handler, Vector2 from, float alphaTime, Ease.Easer ease) : base(from)
            {
                this.handler = handler;
                From = from;
                Tween.Set(this, Tween.TweenMode.Oneshot, alphaTime, Ease.Linear, t => alpha = t.Eased);
                texture = GFX.Game["objects/PuzzleIslandHelper/protoWarpCapsule/glowOrb"];
                target = VirtualContent.CreateRenderTarget("gloworb", 320, 180);
                mask = VirtualContent.CreateRenderTarget("hidemask", 320, 180);
                Add(new BeforeRenderHook(beforeRender));
            }
            public override void Update()
            {
                base.Update();
                if (FadeAutomatically)
                {
                    WarpCapsule parent = handler.Parent;
                    bool condition = parent.CollidePoint(To) || parent.LeftDoor.Depth <= 0;
                    doorMaskAlpha = Calc.Approach(doorMaskAlpha, condition ? 1 : 0, Engine.DeltaTime * 5);
                }
            }
            public override void Removed(Scene scene)
            {
                base.Removed(scene);
                target?.Dispose();
                target = null;
                mask?.Dispose();
                mask = null;
            }
            private void beforeRender()
            {
                if (Scene is not Level level || level.GetPlayer() is not Player player) return;
                target.SetAsTarget(true);
                mask.SetAsTarget(true);
                WarpCapsule parent = handler.Parent;
                Vector2 cam = level.Camera.Position;
                Vector2 offset = parent.Position - cam;
                WARP.Door left = parent.LeftDoor;
                WARP.Door right = parent.RightDoor;
                Draw.SpriteBatch.StandardBegin();

                Draw.Rect(offset.X - 16, offset.Y - 16, parent.Width + 32, 16, Color.White * doorMaskAlpha);

                Vector2 pos = left.Image.RenderPosition;
                Rectangle bounds = new Rectangle((int)(pos.X - cam.X - left.Width), (int)(pos.Y - cam.Y - left.Height), (int)left.Width, (int)left.Height);
                Draw.Rect(bounds, Color.White * doorMaskAlpha);


                Vector2 pos2 = right.Image.RenderPosition;
                Rectangle bounds2 = new Rectangle((int)(pos2.X - cam.X), (int)(pos2.Y - cam.Y - right.Height), (int)right.Width, (int)right.Height);
                Draw.Rect(bounds2, Color.White * doorMaskAlpha);

                //draw mask
                Draw.SpriteBatch.End();
                target.SetAsTarget();
                Draw.SpriteBatch.StandardBegin(level.Camera.Matrix);
                Draw.Line(From, To, Color.White, 5);
                texture.DrawCentered(To);
                Draw.SpriteBatch.End();

                Draw.SpriteBatch.StandardBegin(level.Camera.Matrix, t);
                Draw.SpriteBatch.Draw(mask, cam, Color.White);
                Draw.SpriteBatch.End();
            }
            public override void DebugRender(Camera camera)
            {
                base.DebugRender(camera);
                Draw.SpriteBatch.Draw(mask, camera.Position, Color.White);
            }
            public override void Render()
            {
                base.Render();
                if (Scene is not Level level || alpha <= 0) return;
                Draw.SpriteBatch.Draw(target, level.Camera.Position, Color.White * alpha);
            }
        }
    }

}
