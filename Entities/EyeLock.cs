using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using FrostHelper.ModIntegration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/EyeLock")]
    [Tracked]
    public class EyeLock : Entity
    {
        public Image Behind;
        public class Lock : Actor
        {
            public float Gravity = 0;
            public float SpeedY = 0;
            public bool flashing;
            private float interval = 25;

            public Lock(EyeLock parent) : base(parent.Position)
            {
                Depth = 2;
                Collider = new Hitbox(10, 11, 11, 11);
                Add(new Image(GFX.Game["objects/PuzzleIslandHelper/lock/eye"]));
            }
            public void Drop()
            {
                Gravity = Player.Gravity;
            }
            public override void Update()
            {
                base.Update();
                if (flashing)
                {
                    if (Scene.OnInterval(interval * Engine.DeltaTime))
                    {
                        Visible = !Visible;
                        if (Visible)
                        {
                            interval -= 3;
                        }
                        if (interval <= 5)
                        {
                            RemoveSelf();
                            return;
                        }
                    }
                }
                else if (Gravity != 0)
                {
                    SpeedY = Calc.Approach(SpeedY, Gravity, 300f * Engine.DeltaTime);
                    MoveV(SpeedY * Engine.DeltaTime, OnCollideV);
                }
            }
            public void OnCollideV(CollisionData data)
            {
                if (Math.Abs(SpeedY) > 50f)
                {
                    SpeedY *= -0.6f;
                }
                else
                {
                    flashing = true;
                    SpeedY = 0;
                }
            }
        }
        public class Shine : Entity
        {
            public float Alpha = 0;
            public Image Image;
            public Shine(EyeLock parent) : base(parent.Position)
            {
                Depth = 1;
                Add(Image = new Image(GFX.Game["objects/PuzzleIslandHelper/lock/flash"]));
                Image.Color = Color.Transparent;
            }
            public override void Added(Scene scene)
            {
                base.Added(scene);
                Image.Color = Color.White * Alpha;
            }
            public override void Update()
            {
                base.Update();
                Image.Color = Color.White * Alpha;
            }
        }
        public FlagList Flag;
        public enum UnlockModes
        {
            Key,
            Flag
        }
        public UnlockModes UnlockMode;
        public FlagList FlagsToSet;
        public FlagList RequiredFlags;
        public TalkComponent Talk;
        public Lock LockActor;
        public Shine ShineEntity;
        public EntityID ID;
        private string lockID;
        public string IDFlag => "EyeLock:" + lockID;
        private bool talked;
        public EyeLock(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
        {
            Depth = 3;
            ID = id;
            lockID = data.Attr("lockID");
            Flag = data.FlagList("flag");
            UnlockMode = data.Enum<UnlockModes>("mode");
            FlagsToSet = data.FlagList("flagsToSet");
            RequiredFlags = data.FlagList("requiredFlags");
            Add(Behind = new Image(GFX.Game["objects/PuzzleIslandHelper/lock/behind"]));
            Collider = Behind.Collider();
            Add(Talk = new TalkComponent(new Rectangle(0, 0, (int)Width, (int)Height), Vector2.UnitX * Width / 2, p =>
            {
                talked = true;
                Input.Dash.ConsumePress();
                LockActor?.Drop();
                if (UnlockMode == UnlockModes.Key)
                {
                    PianoModule.Session.KeysUsed++;
                }
                if (!FlagsToSet.Empty)
                {
                    FlagsToSet.State = true;
                }
                SceneAs<Level>().Session.SetFlag(IDFlag);
            }));
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            LockActor = new Lock(this);
            ShineEntity = new Shine(this);
            if (!SceneAs<Level>().Session.GetFlag(IDFlag))
            {
                scene.Add(LockActor);
                if (CanUnlock())
                {
                    ShineEntity.Alpha = 1;
                }
                scene.Add(ShineEntity);
            }
            else
            {
                talked = true;
            }
            Talk.Enabled = !talked && Talkable();
            FlagUpdate();
        }
        public void FlagUpdate()
        {
            if (!Flag)
            {
                Talk.Enabled = false;
                Visible = ShineEntity.Visible = LockActor.Visible = false;
                ShineEntity.Active = LockActor.Active = false;
                ShineEntity.Alpha = 0;
            }
            else
            {
                Visible = ShineEntity.Visible = true;
                ShineEntity.Active = LockActor.Active = true;
                if (!LockActor.flashing)
                {
                    LockActor.Visible = true;
                }
            }
        }
        public override void Update()
        {
            FlagUpdate();
            if(!Flag) return;
            base.Update();
            Talk.Enabled = !talked && Talkable();
            if (Talk.Enabled)
            {
                ShineEntity.Alpha = Calc.Approach(ShineEntity.Alpha, (float)(Math.Sin(Scene.TimeActive * 0.8f) + 1) / 2, 10f * Engine.DeltaTime);
            }
            else
            {
                ShineEntity.Alpha = Calc.Approach(ShineEntity.Alpha, 0, 10f * Engine.DeltaTime);
            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            ShineEntity?.RemoveSelf();
            LockActor?.RemoveSelf();
        }
        public bool CanUnlock()
        {
            return UnlockMode switch
            {
                UnlockModes.Key => PianoModule.Session.KeysObtained - PianoModule.Session.KeysUsed > 0,
                UnlockModes.Flag => (bool)RequiredFlags,
                _ => true,
            };
        }
        public bool GetFlag()
        {
            return SceneAs<Level>().Session.GetFlag(IDFlag);
        }
        public bool Talkable()
        {
            return !GetFlag() && CanUnlock();
        }
    }
}