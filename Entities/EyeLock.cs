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
            public Lock(EyeLock parent) : base(parent.Position)
            {
                Parent = parent;
                Depth = parent.Depth - 1;
                Collider = new Hitbox(10, 8, 11, 11);
                Add(image = new Image(GFX.Game["objects/PuzzleIslandHelper/lock/eye"]));
            }
            public void Drop()
            {
                Gravity = Player.Gravity;
            }
            public override void Update()
            {
                base.Update();
                if (Gravity != 0)
                {
                    blackLerp = Calc.Approach(blackLerp, 0.5f, Engine.DeltaTime * 8);
                    image.Color = Color.Lerp(Color.White, Color.Black, blackLerp);
                }
                if (flashing)
                {
                    if (Scene.OnInterval(interval * Engine.DeltaTime))
                    {
                        Visible = !Visible;
                        if (Visible)
                        {
                            interval *= 0.7f;
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
                    flashing = true;
                    SpeedY = 0;
                }
            }
        }
        public class Shine : Entity
        {
            public float Alpha = 0;
            public Image Image;
            public Image Image2;
            public Shine(EyeLock parent) : base(parent.Position)
            {
                Depth = parent.Depth - 2;
                Add(Image2 = new Image(GFX.Game["objects/PuzzleIslandHelper/lock/shine"]));
                Image2.Scale = Vector2.One * 2f;
                Image2.Origin = new Vector2(16);
                Image2.Position += new Vector2(16);
                Add(Image = new Image(GFX.Game["objects/PuzzleIslandHelper/lock/shine"]));
                Image.Color = Image2.Color = Color.Transparent;
                Collider = Image.Collider();
            }
            public override void Added(Scene scene)
            {
                base.Added(scene);
                Image2.Color = (Image.Color = Color.White * Alpha) * 0.7f;
            }
            public override void Update()
            {
                base.Update();
                Image2.Color = (Image.Color = Color.White * Alpha) * 0.7f;
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
            if (!Flag) return;
            base.Update();
            Talk.Enabled = !talked && Talkable();
            if (Talk.Enabled)
            {
                ShineEntity.Alpha = Calc.Approach(ShineEntity.Alpha, (float)(Math.Sin(Scene.TimeActive * 0.9f) + 1) / 2, 15f * Engine.DeltaTime);
            }
            else
            {
                ShineEntity.Alpha = Calc.Approach(ShineEntity.Alpha, 0, 10f * Engine.DeltaTime);
            }
        }
        public override void Removed(Scene scene)
        {
            if (delayAlarm != null && delayAlarm.TimeLeft > 0)
            {
                SetFlags(true);
            }
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