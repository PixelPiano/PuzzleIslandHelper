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
            private float interval = 20;
            public EyeLock Parent;
            private float blackLerp;
            private Image image;
            public ImageShine Shine;
            public bool Shining;
            public Lock(EyeLock parent) : base(parent.Position)
            {
                Parent = parent;
                Depth = parent.Depth - 1;
                Collider = new Hitbox(10, 8, 11, 11);
                Add(image = new Image(GFX.Game["objects/PuzzleIslandHelper/lock/eye"]));
            }
            public override void Added(Scene scene)
            {
                base.Added(scene);
                Add(Shine = new ImageShine(GFX.Game["objects/PuzzleIslandHelper/lock/shine"], 0));
                Shine.Position = Collider.Center;
            }
            public void Drop()
            {
                Gravity = Player.Gravity;
            }
            public override void Update()
            {
                base.Update();
                if (Shining)
                {
                    Shine.Alpha = Calc.Approach(Shine.Alpha, (float)(Math.Sin(Scene.TimeActive * 0.9f) + 1) / 2, 15f * Engine.DeltaTime);
                }
                else
                {
                    Shine.Alpha = Calc.Approach(Shine.Alpha, 0, Engine.DeltaTime * 10);
                }

                if (Gravity != 0)
                {
                    blackLerp = Calc.Approach(blackLerp, 0.5f, Engine.DeltaTime * 8);
                    image.Color = Color.Lerp(Color.White, Color.Black, blackLerp);
                    if (!flashing)
                    {
                        SpeedY = Calc.Approach(SpeedY, Gravity, 300f * Engine.DeltaTime);
                        MoveV(SpeedY * Engine.DeltaTime, OnCollideV);
                    }
                }
            }
            public override void Removed(Scene scene)
            {
                base.Removed(scene);
            }
            public void OnCollideV(CollisionData data)
            {
                if (Math.Abs(SpeedY) > 50f)
                {
                    SpeedY *= -0.6f;
                }
                else
                {
                    if (!flashing)
                    {
                        flashing = true;
                        Add(new Coroutine(flashingRoutine()));
                    }

                    SpeedY = 0;
                }
            }
            private IEnumerator flashingRoutine()
            {
                int interval = 20;
                while (interval > 5)
                {
                    yield return interval * Engine.DeltaTime;
                    Visible = !Visible;
                    if (Visible)
                    {
                        interval -= 3;
                    }
                }
                RemoveSelf();
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
        public ImageShine Shine;
        public EntityID ID;
        private string lockID;
        public string IDFlag => "EyeLock:" + lockID;
        private bool talked;
        private float flagDelay;
        private Alarm delayAlarm;
        public EyeLock(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
        {
            flagDelay = data.Float("flagSetDelay", 0.7f);
            Depth = data.Int("depth", 3);
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
                if (flagDelay > 0)
                {
                    delayAlarm = Alarm.Set(this, flagDelay, () =>
                    {
                        SetFlags(true);
                    });
                }
                else
                {
                    SetFlags(true);
                }
            }));
        }
        public void SetFlags(bool value)
        {
            if (!FlagsToSet.Empty)
            {
                FlagsToSet.State = value;
            }
            SceneAs<Level>().Session.SetFlag(IDFlag, value);
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            LockActor = new Lock(this);
            if (!SceneAs<Level>().Session.GetFlag(IDFlag))
            {
                scene.Add(LockActor);
                if (CanUnlock())
                {
                    LockActor.Shine.Alpha = 1;
                }
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
                Visible = LockActor.Visible = false;
                LockActor.Shine.Alpha = 0;
            }
            else
            {
                Visible = true;
                if (!LockActor.flashing)
                {
                    LockActor.Visible = true;
                }
            }
        }
        public override void Update()
        {
            FlagUpdate();
            if (!Flag) return;
            base.Update();
            Talk.Enabled = !talked && Talkable();
            LockActor.Shining = Talk.Enabled;
        }
        public override void Removed(Scene scene)
        {
            if (delayAlarm != null && delayAlarm.TimeLeft > 0)
            {
                SetFlags(true);
            }
            base.Removed(scene);
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