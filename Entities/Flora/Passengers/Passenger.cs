using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Loaders;
using FMOD;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Flora.Passengers
{
    [Tracked]
    public abstract class Passenger : Actor
    {
        public string DataCutsceneID;
        public bool HasDataCutscene => !string.IsNullOrEmpty(DataCutsceneID);
        private readonly DotX3 dataTalk;
        public bool CutsceneWatched
        {
            get
            {
                return HasDataCutscene && SceneAs<Level>().Session.GetFlag(DataCutsceneID);
            }
            set
            {
                if (HasDataCutscene)
                {
                    SceneAs<Level>().Session.SetFlag(DataCutsceneID, value);
                }
            }
        }
        public Vector2 Speed;
        public bool onGround;
        public bool wasOnGround;
        public bool JumpLoop;
        public float CannotJumpTimer;
        public float JumpHeight = 150f;
        public bool HasGravity = true;
        public float GravityMult = 1;
        public string[] Dialogs;
        public bool HasDialogCutscenes => Dialogs != null && Dialogs.Length > 0;
        public enum DialogMethods
        {
            OnlyOnce,
            Loop,
            RepeatLast
        }
        public DialogMethods DialogMethod;
        public int DialogIndex;
        public bool CanStartDialogCutscene => HasDialogCutscenes && DialogIndex < Dialogs.Length;
        public Passenger(Vector2 position, float width, float height, string cutscene, string dialog, DialogMethods dialogMethod) : base(position)
        {
            if (!string.IsNullOrWhiteSpace(dialog))
            {
                Dialogs = dialog.Split(',');
            }
            Depth = 1;
            Collider = new Hitbox(width, height);
            DataCutsceneID = cutscene;
            Add(dataTalk = new DotX3(Collider, OnDataInteract));
            Tag |= Tags.TransitionUpdate;
            DialogMethod = dialogMethod;
        }
        public Passenger(Vector2 position, float width, float height, string cutscene, string dialog) : this(position, width, height, cutscene, dialog, DialogMethods.OnlyOnce)
        {
        }
        public Passenger(Vector2 position, float width, float height, string cutscene) : this(position, width, height, cutscene, null, DialogMethods.OnlyOnce)
        {
        }
        public Passenger(EntityData data, Vector2 offset) : this(data.Position + offset, 16, 16, data.Attr("cutsceneID"), data.Attr("dialog"), data.Enum<DialogMethods>("dialogMethod"))
        {

        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            UpdateDataTalk();
        }
        public override void Update()
        {
            base.Update();
            if (!HasGravity)
            {
                onGround = false;
            }
            else
            {
                onGround = OnGround();
                if (onGround)
                {
                    if (!wasOnGround)
                    {
                        OnJumpLand();
                    }
                    if (JumpLoop)
                    {
                        Jump();
                    }
                }
            }
            CannotJumpTimer = Math.Max(0, CannotJumpTimer - Engine.DeltaTime);
            UpdateDataTalk();
            MoveV(Speed.Y * Engine.DeltaTime, OnCollideV);
            MoveH(Speed.X * Engine.DeltaTime, OnCollideH);
            Speed.Y = Calc.Approach(Speed.Y, HasGravity ? 120f * GravityMult : 0, 900f * Engine.DeltaTime);
            Speed.X = Calc.Approach(Speed.X, 0f, 200f * Engine.DeltaTime);
            wasOnGround = onGround;
        }
        public void OnCollideV(CollisionData data)
        {
            Speed.Y = 0;
        }
        public virtual void Jump()
        {
            if (CannotJumpTimer > 0) return;
            Speed.Y = -JumpHeight;
        }
        public virtual void OnJumpLand()
        {
        }
        public void OnCollideH(CollisionData data)
        {
            Speed.X = 0;
        }
        public IEnumerator MoveXNaive(float x, float time, Ease.Easer ease = null)
        {
            yield return MoveToX(Position.X + x, time, ease);
        }
        public IEnumerator MoveToX(float to, float time, Ease.Easer ease = null)
        {
            float from = Position.X;
            ease ??= Ease.Linear;
            for (float i = 0; i < 1; i += Engine.DeltaTime / time)
            {
                Position.X = Calc.LerpClamp(from, to, ease(i));
                yield return null;
            }
            Position.X = to;
        }
        public virtual void OnCutsceneEnd(Level level)
        {
            CutsceneWatched = true;
        }
        public void UpdateDataTalk()
        {

            bool watched = CutsceneWatched;
            if(!HasDataCutscene || (HasDialogCutscenes && watched && !CanStartDialogCutscene) || (watched && !HasDialogCutscenes))
            {
                dataTalk.Enabled = false;
            }
        }
        public virtual void OnDataInteract(Player player)
        {
            if (HasDataCutscene && !CutsceneWatched)
            {
                PassengerCutsceneLoader.LoadCustomCutscene(DataCutsceneID, this, player, SceneAs<Level>());
            }
            else if (CanStartDialogCutscene)
            {
                AddDialogCutscene(player, Dialogs[DialogIndex]);
            }
        }
        public void AddDialogCutscene(Player player, string dialog)
        {
            SceneAs<Level>().Add(new DialogPassengerCutscene(this, player, dialog));
        }
    }
}