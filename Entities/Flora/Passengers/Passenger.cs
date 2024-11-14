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
                return HasDataCutscene && SceneAs<Level>().Session.GetFlag("PuzzleIslandHelper/PassengerCutscene/" + DataCutsceneID);
            }
            set
            {
                if (HasDataCutscene)
                {
                    SceneAs<Level>().Session.SetFlag("PuzzleIslandHelper/PassengerCutscene/" + DataCutsceneID, value);
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
        public string DialogName;
        public Passenger(Vector2 position, float width, float height, string cutscene) : base(position)
        {
            Depth = 1;
            Collider = new Hitbox(width, height);
            DataCutsceneID = cutscene;
            Add(dataTalk = new DotX3(Collider, OnDataInteract));
        }
        public Passenger(EntityData data, Vector2 offset) : this(data.Position + offset, 16, 16, data.Attr("cutsceneID"))
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
        public void UpdateDataTalk()
        {
            if (!HasDataCutscene || CutsceneWatched)
            {
                dataTalk.Enabled = false;
            }
        }
        public virtual void OnDataInteract(Player player)
        {
            PassengerCutsceneLoader.LoadCustomCutscene(DataCutsceneID, this, player, SceneAs<Level>());
        }
    }
}