using Celeste.Mod.PuzzleIslandHelper.Cutscenes;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Flora.Passengers
{
    [Tracked]
    public abstract class PassengerCutscene : CutsceneEntity
    {
        public Passenger Passenger;
        public Player Player;
        private Coroutine zoomRoutine;
        private Coroutine moveRoutine;
        public bool CameraMoving
        {
            get
            {
                return moveRoutine.Active && !moveRoutine.Finished;
            }
            set
            {
                moveRoutine.Active = value;
            }
        }
        public bool CameraZooming
        {
            get
            {
                return zoomRoutine.Active && !zoomRoutine.Finished;
            }
            set
            {
                zoomRoutine.Active = value;
            }
        }
        public bool OncePerSession = true;
        public bool CanBeDisabled = true;
        public PassengerCutscene(Passenger passenger, Player player) : base()
        {
            Passenger = passenger;
            Player = player;
            zoomRoutine = new Coroutine(false);
            moveRoutine = new Coroutine(false);
            Add(zoomRoutine, moveRoutine);
        }
        public void CancelCameraZoom()
        {
            zoomRoutine.Cancel();
        }
        public void CancelCameraMove()
        {
            moveRoutine.Cancel();
        }
        public bool GetFlag(string flag)
        {
            return Level.Session.GetFlag(flag);
        }
        public void SetFlag(string flag, bool value = true)
        {
            Level.Session.SetFlag(flag, value);
        }
        public void MoveCameraTo(Vector2 pos, float time, Ease.Easer ease = null)
        {
            moveRoutine.Replace(MoveCameraToRoutine(pos, time, ease));
        }
        public void MoveCamera(float h, float v, float time, Ease.Easer ease = null)
        {
            moveRoutine.Replace(MoveCameraToRoutine(Level.Camera.Position + new Vector2(h, v), time, ease));
        }
        public void ZoomTo(Vector2 worldPos, float zoom, float time)
        {
            zoomRoutine.Replace(ZoomToRoutine(worldPos, zoom, time));
        }
        public void ZoomToCenter(Entity to, Vector2 offset, float zoom, float time)
        {
            ZoomTo(to.Center + offset, zoom, time);
        }
        public void ZoomAcross(Vector2 worldPos, float zoom, float time)
        {
            zoomRoutine.Replace(ZoomAcrossRoutine(worldPos, zoom, time));
        }
        public void CenterCamera(Entity on, Vector2 offset, float time, Ease.Easer ease = null)
        {
            MoveCameraTo(on.Center - new Vector2(160, 90) + offset, time, ease);
        }
        public IEnumerator MoveCameraToRoutine(Vector2 pos, float time, Ease.Easer ease = null)
        {
            ease ??= Ease.CubeIn;
            Vector2 from = Level.Camera.Position;
            for (float i = 0; i < 1; i += Engine.DeltaTime / time)
            {
                Level.Camera.X = Calc.LerpClamp(from.X, pos.X, ease(i));
                Level.Camera.Y = Calc.LerpClamp(from.Y, pos.Y, ease(i));
                yield return null;
            }
            Level.Camera.Position = pos;
        }
        public IEnumerator MoveCameraRoutine(float h, float v, float time, Ease.Easer ease = null)
        {
            yield return new SwapImmediately(MoveCameraToRoutine(Level.Camera.Position + new Vector2(h, v), time, ease));
        }
        public IEnumerator ZoomToRoutine(Vector2 worldPos, float zoom, float time)
        {
            yield return new SwapImmediately(Level.ZoomToWorld(worldPos, zoom, time));
        }
        public IEnumerator ZoomAcrossRoutine(Vector2 worldPos, float zoom, float time)
        {
            yield return new SwapImmediately(Level.ZoomAcrossWorld(worldPos, zoom, time));
        }

        public IEnumerator Wait1()
        {
            yield return 1f;
        }
        public IEnumerator Wait2()
        {
            yield return 2f;
        }
        public IEnumerator Wait3()
        {
            yield return 3f;
        }
        [OnLoad]
        public static void Load()
        {
            On.Celeste.CutsceneEntity.EndCutscene += CutsceneEntity_EndCutscene;
        }
        [OnUnload]
        public static void Unload()
        {
            On.Celeste.CutsceneEntity.EndCutscene -= CutsceneEntity_EndCutscene;
        }
        private static void CutsceneEntity_EndCutscene(On.Celeste.CutsceneEntity.orig_EndCutscene orig, CutsceneEntity self, Level level, bool removeSelf)
        {
            if (self is PassengerCutscene pc)
            {
                if (pc.OncePerSession && pc.CanBeDisabled)
                {
                    pc.Passenger.OnCutsceneEnd(level);
                }
            }
            orig(self, level, removeSelf);
        }
    }

    public class DialogPassengerCutscene : PassengerCutscene
    {
        public string Dialog;
        public DialogPassengerCutscene(Passenger passenger, Player player, string dialog) : base(passenger, player)
        {
            Dialog = dialog;
            OncePerSession = false;
            CanBeDisabled = false;
        }
        public override void OnBegin(Level level)
        {
            Player.DisableMovement();
            Add(new Coroutine(cutscene()));
        }
        private IEnumerator cutscene()
        {
            yield return Textbox.Say(Dialog.Trim(), Wait1, Wait2, Wait3);
            EndCutscene(Level);
        }
        public override void OnEnd(Level level)
        {
            Player.EnableMovement();
            switch (Passenger.DialogMethod)
            {
                case Passenger.DialogMethods.OnlyOnce:
                    Passenger.DialogIndex++;
                    break;
                case Passenger.DialogMethods.Loop:
                    Passenger.DialogIndex = (Passenger.DialogIndex + 1) % Passenger.Dialogs.Length;
                    break;
                case Passenger.DialogMethods.RepeatLast:
                    Passenger.DialogIndex = (int)Calc.Min(Passenger.Dialogs.Length, Passenger.DialogIndex + 1);
                    break;
            }
        }
    }
}