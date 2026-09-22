
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Boss.Actions;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;
using static Celeste.Mod.PuzzleIslandHelper.Entities.InvertAuth;
using static Celeste.Mod.PuzzleIslandHelper.Entities.TowerHead;

namespace Celeste.Mod.PuzzleIslandHelper.Triggers
{

    [CustomEntity("PuzzleIslandHelper/FreezeTrigger")]
    [Tracked]
    public class FreezeTrigger : Trigger
    {
        public enum FlagConditions
        {
            None,
            FlagActive,
            FlagInactive,
            FlagActivated,
            FlagDeactivated,
            FlagChanged
        }
        public enum PlayerConditions
        {
            None,
            Die,
            Spawn,
            Respawn,
            Dash,
            Jump,
            MoveX,
            MoveY,
            Move
        }
        public enum TriggerConditions
        {
            None,
            OnEnter,
            OnLeave,
            OnStayInside,
            OnStayOutside
        }
        public bool MustBeInside;
        public FlagList Flag, FlagOnEnd, FlagOnActivate;
        private FlagConditions flagCondition;
        private PlayerConditions playerCondition;
        private TriggerConditions triggerCondition;
        private bool previousState;
        private float loopDelay;
        private float freezeTime;
        private string startAudioEvent, endAudioEvent;
        private float delay;
        private bool loops;
        private bool pauseMovementWhenFrozen;
        private bool waiting;
        private bool staying;
        public bool PlayerJustDied;
        public bool PlayerJustSpawned;
        public bool PlayerJustJumped;
        public bool PlayerJustEntered;
        public bool PlayerJustLeft;
        public bool playerRespawningAfterDying;
        public bool OnlyOnce;
        public bool OnlyOncePerSession;
        public EntityID ID;
        private int prevPlayerState;
        private bool flagState => Flag;
        private bool flagOnActiveState => FlagOnActivate;
        private bool flagOnInactiveState => FlagOnEnd;
        private bool allowFrameOne;
        private bool frameOne;
        public FreezeTrigger(EntityData data, Vector2 offset, EntityID id) : base(data, offset)
        {
            allowFrameOne = data.Bool("allowFrameOne", false);

            ID = id;
            Flag = data.FlagList("flag");
            FlagOnEnd = data.FlagList("flagOnEnd");
            FlagOnActivate = data.FlagList("flagOnActivate");
            loopDelay = data.Float("repeatDelay");
            freezeTime = data.Float("duration");
            loops = data.Bool("loops");
            startAudioEvent = data.Attr("startAudioEvent", "event:/PianoBoy/invertGlitch2");
            endAudioEvent = data.Attr("endAudioEvent", "event:/PianoBoy/invertGlitch2");
            flagCondition = data.Enum<FlagConditions>("flagCondition");
            triggerCondition = data.Enum<TriggerConditions>("triggerCondition");
            playerCondition = data.Enum<PlayerConditions>("playerCondition");
            pauseMovementWhenFrozen = data.Bool("pauseMovementWhenFrozen", true);
            OnlyOnce = data.Bool("onlyOnce");
            OnlyOncePerSession = data.Bool("onlyOncePerSession");
        }
        [OnLoad]
        public static void Load()
        {
            Everest.Events.Player.OnDie += Player_OnDie;
            Everest.Events.Player.OnSpawn += Player_OnSpawn;
            On.Celeste.Player.Jump += Player_Jump;
        }
        [OnUnload]
        public static void Unload()
        {
            Everest.Events.Player.OnDie -= Player_OnDie;
            Everest.Events.Player.OnSpawn -= Player_OnSpawn;
            On.Celeste.Player.Jump -= Player_Jump;
        }

        private static void Player_Jump(On.Celeste.Player.orig_Jump orig, Player self, bool particles, bool playSfx)
        {
            orig(self, particles, playSfx);
            foreach (FreezeTrigger f in self.Scene.Tracker.GetEntities<FreezeTrigger>())
            {
                f.PlayerJustJumped = true;
            }
        }

        private static void Player_OnSpawn(Player obj)
        {
            foreach (FreezeTrigger f in obj.Scene.Tracker.GetEntities<FreezeTrigger>())
            {
                f.PlayerJustSpawned = true;
            }
        }

        private static void Player_OnDie(Player obj)
        {
            foreach (FreezeTrigger f in obj.Scene.Tracker.GetEntities<FreezeTrigger>())
            {
                f.PlayerJustDied = true;
                f.playerRespawningAfterDying = true;
            }
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            PlayerJustEntered = true;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            previousState = Flag;
            frameOne = true;
        }
        public override void OnStay(Player player)
        {
            base.OnStay(player);
            staying = true;
        }
        public override void OnLeave(Player player)
        {
            base.OnLeave(player);
            PlayerJustLeft = true;
            staying = false;
        }

        public override void Update()
        {
            base.Update();
            if (waiting)
            {
                FlagOnEnd.State = true;
                waiting = false;
                if (!string.IsNullOrEmpty(endAudioEvent))
                {
                    Audio.Play(endAudioEvent, Center);
                }
                if (pauseMovementWhenFrozen && !SceneAs<Level>().InCutscene)
                {
                    Scene.GetPlayer().StateMachine.State = prevPlayerState;
                    prevPlayerState = 0;
                }
                if (OnlyOncePerSession)
                {
                    SceneAs<Level>().Session.DoNotLoad.Add(ID);
                    RemoveSelf();
                    return;
                }
                else if (OnlyOnce)
                {
                    RemoveSelf();
                    return;
                }
            }
            bool flag = Flag;
            bool flagCondition = this.flagCondition switch
            {
                FlagConditions.FlagActive => flag,
                FlagConditions.FlagInactive => !flag,
                FlagConditions.FlagActivated => flag && !previousState,
                FlagConditions.FlagDeactivated => !flag && previousState,
                FlagConditions.FlagChanged => flag != previousState,
                _ => !Flag.Empty
            };
            bool playerCondition = Scene.GetPlayer() is Player player && this.playerCondition switch
            {
                PlayerConditions.None => true,
                PlayerConditions.Die => PlayerJustDied,
                PlayerConditions.Spawn => PlayerJustSpawned,
                PlayerConditions.Respawn => PlayerJustSpawned && playerRespawningAfterDying,
                PlayerConditions.Dash => player.StartedDashing,
                PlayerConditions.Jump => PlayerJustJumped,
                PlayerConditions.MoveX => !player.Dead && player.StateMachine.State != Player.StDummy && player.PreviousPosition.X != player.X,
                PlayerConditions.MoveY => !player.Dead && player.StateMachine.State != Player.StDummy && player.PreviousPosition.Y != player.Y,
                PlayerConditions.Move => !player.Dead && player.StateMachine.State != Player.StDummy && player.PreviousPosition != player.Position
            };
            bool triggerCondition = this.triggerCondition switch
            {
                TriggerConditions.None => true,
                TriggerConditions.OnEnter => PlayerJustEntered,
                TriggerConditions.OnLeave => PlayerJustLeft,
                TriggerConditions.OnStayInside => staying,
                TriggerConditions.OnStayOutside => !staying
            };
            if (PlayerJustSpawned)
            {
                playerRespawningAfterDying = false;
            }
            PlayerJustDied = false;
            PlayerJustSpawned = false;
            PlayerJustJumped = false;
            PlayerJustEntered = false;
            PlayerJustLeft = false;
            previousState = flag;
            if(!allowFrameOne && frameOne)
            {
                frameOne = false;
                return;
            }
            if (delay > 0)
            {
                delay -= Engine.DeltaTime;
            }
            if (!waiting && delay <= 0)
            {
                if (flagCondition && playerCondition && triggerCondition)
                {
                    FlagOnActivate.State = true;
                    if (!string.IsNullOrEmpty(startAudioEvent))
                    {
                        Audio.Play(startAudioEvent, Center);
                    }
                    if (freezeTime > 0)
                    {
                        waiting = true;
                        prevPlayerState = Scene.GetPlayer().StateMachine.State;
                        if (pauseMovementWhenFrozen) Scene.DisableMovement();
                        if (loops)
                        {
                            delay = loopDelay;
                        }
                        Celeste.Freeze(freezeTime);
                    }
                }
            }
            frameOne = false;
        }
    }
}
