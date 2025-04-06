using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Triggers
{
    [Tracked]
    [CustomEntity("PuzzleIslandHelper/RandomTriggerTrigger")]
    public class RandomTriggerTrigger : Trigger
    {
        public enum Modes
        {
            OnEnter,
            OnLeave,
            OnStay
        }
        public bool OnTransition;
        public bool TriggeredOnce;
        public Modes Mode;
        public FlagData Flag;
        public enum OnceModes
        {
            None,
            PerRoom,
            PerSession
        }
        public OnceModes OnceMode;
        public EntityID ID;
        public Vector2[] Nodes;
        public RandomTriggerTrigger(EntityData data, Vector2 offset, EntityID id) : base(data, offset)
        {
            ID = id;
            Flag = new FlagData(data.Attr("flag"), data.Bool("inverted"), data.Bool("ignoreFlag"));
            if (data.Bool("transitionUpdate"))
            {
                Tag |= Tags.TransitionUpdate;
            }
            OnTransition = data.Bool("onTransition");
            OnceMode = data.Enum<OnceModes>("onceMode");
            Mode = data.Enum<Modes>("mode");
            Nodes = data.NodesOffset(offset);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (OnTransition && scene.GetPlayer() is Player player)
            {
                DoThing(player);
            }
        }
        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if (Mode is Modes.OnEnter)
            {
                DoThing(player);
            }
        }
        public override void OnStay(Player player)
        {
            base.OnStay(player);
            if (Mode is Modes.OnStay)
            {
                DoThing(player);
            }
        }
        public override void OnLeave(Player player)
        {
            base.OnLeave(player);
            if (Mode is Modes.OnLeave)
            {
                DoThing(player);
            }
        }
        public void DoThing(Player player)
        {
            if (Flag.State)
            {
                Level level = Scene as Level;
                if (OnceMode is OnceModes.None || !TriggeredOnce)
                {
                    List<Trigger> collided = [];
                    Collider prev = Collider;
                    Collider = new Hitbox(1, 1);
                    foreach (Vector2 v in Nodes)
                    {
                        if (CollideFirst<Trigger>(v) is Trigger trigger)
                        {
                            collided.Add(trigger);
                        }
                    }
                    Collider = prev;
                    if (collided.Count > 0)
                    {
                        collided.Random().OnEnter(player);
                    }
                }
                TriggeredOnce = true;
            }
            switch (OnceMode)
            {
                case OnceModes.PerRoom:
                    RemoveSelf();
                    break;
                case OnceModes.PerSession:
                    SceneAs<Level>().Session.DoNotLoad.Add(ID);
                    RemoveSelf();
                    break;
            }
        }
    }
}