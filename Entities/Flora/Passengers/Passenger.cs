using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Loaders;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using static Celeste.Mod.PuzzleIslandHelper.Entities.Flora.Passengers.Passenger;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Flora.Passengers
{
    [Tracked]
    public abstract class Passenger : Actor
    {
        public bool CutsceneOnTransition;
        public string[] CutsceneArgs;
        public string DataCutsceneID;
        public bool HasDataCutscene => !string.IsNullOrEmpty(DataCutsceneID);
        private readonly DotX3 Talk;
        public bool CutsceneWatched
        {
            get => HasDataCutscene && SceneAs<Level>().Session.GetFlag(DataCutsceneID);
            set
            {
                if (HasDataCutscene)
                {
                    SceneAs<Level>().Session.SetFlag(DataCutsceneID, value);
                }
            }
        }
        public string[] dataDialogs;
        public bool HasDialogCutscenes => dataDialogs != null && dataDialogs.Length > 0;
        public enum DialogMethods
        {
            OnlyOnce,
            Loop,
            RepeatLast
        }
        public DialogMethods DialogMethod;
        public int DialogIndex;
        public string DialogDebug
        {
            get
            {
                if (dataDialogs == null) return "None";
                string output = "";
                foreach (string s in dataDialogs)
                {
                    output += s + ",";
                }
                return output;
            }
        }
        public bool CanStartDialogCutscene => HasDialogCutscenes && DialogIndex < dataDialogs.Length && TalkFlag;
        public FlagList ActiveFlag;
        public FlagList TalkFlag;
        public bool FlagState => ActiveFlag;
        public Vector2 Speed;
        public bool onGround;
        public bool wasOnGround;
        public bool EndlessJump;
        public float CannotJumpTimer;
        public float JumpHeight = 150f;
        public bool HasGravity = true;
        public float GravityMult = 1;
        public string Debug2;
        public EntityID EID;

        public string argsDebug;
        public Passenger(EntityData data, Vector2 offset, EntityID id) : this(data, offset, id, 16, 16)
        {
        }
        public Passenger(EntityData data, Vector2 offset, EntityID id, float width, float height) : base(data.Position + offset)
        {
            EID = id;
            string dialog = Debug2 = data.Attr("dialog");
            ActiveFlag = new FlagList(data.Attr("flag"));
            TalkFlag = new FlagList(data.Attr("dialogFlags"));
            if (!string.IsNullOrWhiteSpace(dialog))
            {
                dataDialogs = dialog.Split(',');
            }
            Depth = 5;
            Collider = new Hitbox(width, height);
            DataCutsceneID = data.Attr("cutsceneID");
            Add(Talk = new DotX3(Collider, OnInteract));
            Tag |= Tags.TransitionUpdate;
            DialogMethod = data.Enum<DialogMethods>("dialogMethod");
            CutsceneOnTransition = data.Bool("cutsceneOnTransition");
            CutsceneArgs = data.Attr("cutsceneArgs").Split(',');
        }
        public Passenger(Vector2 position, EntityID id, float width, float height, string cutscene, string dialog, DialogMethods dialogMethod, string flag = "", bool cutsceneOnTransition = false, string cutsceneArgs = "") : base(position)
        {
            EID = id;
            Debug2 = dialog;
            ActiveFlag = new FlagList(flag);
            if (!string.IsNullOrWhiteSpace(dialog))
            {
                dataDialogs = dialog.Split(',');
            }
            Depth = 1;
            Collider = new Hitbox(width, height);
            DataCutsceneID = cutscene;
            Add(Talk = new DotX3(Collider, OnInteract));
            Tag |= Tags.TransitionUpdate;
            DialogMethod = dialogMethod;
            CutsceneOnTransition = cutsceneOnTransition;
            CutsceneArgs = cutsceneArgs.Split(',');
        }
        public Passenger(Vector2 position, EntityID id, float width, float height, string cutscene, string dialog, string cutsceneArgs = "") : this(position, id, width, height, cutscene, dialog, DialogMethods.OnlyOnce, cutsceneArgs: cutsceneArgs)
        {
        }
        public Passenger(Vector2 position, EntityID id, float width, float height, string cutscene, string cutsceneArgs = "") : this(position, id, width, height, cutscene, null, DialogMethods.OnlyOnce, cutsceneArgs: cutsceneArgs)
        {
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (ActiveFlag)
            {
                if (CutsceneOnTransition && scene.GetPlayer() is Player player)
                {
                    OnInteract(player);
                }
                Talk.Enabled = HasDataCutscene ? !CutsceneWatched && TalkFlag : HasDialogCutscenes && CanStartDialogCutscene;
            }
            else
            {
                Talk.Enabled = false;
            }
        }
        public override void Update()
        {
            base.Update();
            if (ActiveFlag)
            {
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
                        if (EndlessJump)
                        {
                            Jump();
                        }
                    }
                }
                CannotJumpTimer = Math.Max(0, CannotJumpTimer - Engine.DeltaTime);
                Talk.Enabled = HasDataCutscene ? !CutsceneWatched : HasDialogCutscenes && CanStartDialogCutscene;
                MoveV(Speed.Y * Engine.DeltaTime, OnCollideV);
                MoveH(Speed.X * Engine.DeltaTime, OnCollideH);
                Speed.Y = Calc.Approach(Speed.Y, HasGravity ? 120f * GravityMult : 0, 900f * Engine.DeltaTime);
                Speed.X = Calc.Approach(Speed.X, 0f, 200f * Engine.DeltaTime);
                wasOnGround = onGround;
            }
            else
            {
                Talk.Enabled = false;
            }
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
        public virtual void OnInteract(Player player)
        {
            if (HasDataCutscene && !CutsceneWatched)
            {
                PassengerCutsceneLoader.LoadCustomCutscene(DataCutsceneID, this, player, SceneAs<Level>(), CutsceneArgs);
            }
            else if (CanStartDialogCutscene)
            {
                AddDialogCutscene(player, dataDialogs[DialogIndex], CutsceneArgs);
            }
        }
        public void AddDialogCutscene(Player player, string dialog, params string[] args)
        {
            SceneAs<Level>().Add(new DialogPassengerCutscene(this, player, dialog, args));
        }
    }
}