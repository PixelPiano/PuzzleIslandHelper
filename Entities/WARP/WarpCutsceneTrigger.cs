using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.WARP
{
    [Tracked]
    public abstract class WarpCapsuleCutscene : CutsceneEntity
    {
        public CapsuleWarpHandler Handler;
        public WarpCapsule Capsule;
        public bool Intro;
        public bool InCutscene = true;
        public FlagList IntroFlagOnComplete;
        public FlagList OutroFlagOnComplete;
        public WarpCapsuleCutscene(EventTrigger trigger, Player player, string eventID) : base()
        {

        }
        public override void OnBegin(Level level)
        {

        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            InCutscene = false;
        }
        public override void OnEnd(Level level)
        {
            InCutscene = false;
            if (Intro && !IntroFlagOnComplete.Empty)
            {
                IntroFlagOnComplete.State = true;
            }
            else if (!Intro && !OutroFlagOnComplete.Empty)
            {
                OutroFlagOnComplete.State = true;
            }
        }
        public IEnumerator playerToCenter(Player player)
        {
            yield return player.DummyWalkToExact((int)Capsule.CenterX);
        }
        public void snapPlayer(Player player)
        {
            player.X = (int)Capsule.CenterX;
        }
        public void snapCalidus(Calidus calidus)
        {
            calidus.StopFollowing();
            calidus.Position = Capsule.Center;
        }
        public IEnumerator calidusToCenter(Calidus calidus)
        {
            calidus.StopFollowing();
            calidus.Add(new Coroutine(calidus.FloatHeightTo(0, 0.5f)));
            yield return calidus.FloatTo(Capsule.Center - Vector2.UnitX * 8);
        }

    }
    [Tracked]
    public abstract class WarpCutsceneTrigger : EventTrigger
    {
        private FlagList FlagList;
        public EntityID ID;
        public FlagList OnComplete;
        public bool Chosen;
        public string FlagListState => FlagList.ToString();
        public WarpCutsceneTrigger(EntityData data, Vector2 offset, EntityID id) : base(data, offset)
        {
            ID = id;
            FlagList = new FlagList(data.Attr("requiredFlag"));
            OnComplete = new FlagList(data.Attr("flagOnComplete"));
            Depth = int.MinValue;
            Visible = true;
        }
        public override void OnEnter(Player player)
        {
        }
        public bool FlagState => FlagList.State;
        public override void Render()
        {
            base.Render();
            //Draw.Rect(SceneAs<Level>().Camera.Position, 30, 30, Chosen ? Color.Red : Color.Black);
        }
        public WarpCapsuleCutscene Check(Player player, CapsuleWarpHandler handler, WarpCapsule capsule)
        {
            if (FlagList.State)
            {
                Player player2 = player;
                if (triggered)
                {
                    return null;
                }
                triggered = true;
                Level level = Scene as Level;
                if (CutsceneLoaders.TryGetValue(Event, out var value))
                {
                    Entity entity = value(this, player, Event);
                    if (entity is not WarpCapsuleCutscene cutscene) return null;
                    cutscene.Handler = handler;
                    cutscene.Capsule = capsule;
                    cutscene.Intro = this is IntroWarpCutsceneTrigger;
                    if (cutscene.Intro)
                    {
                        cutscene.IntroFlagOnComplete = OnComplete;
                    }
                    else
                    {
                        cutscene.OutroFlagOnComplete = OnComplete;
                    }
                    Scene.Add(cutscene);
                    return cutscene;
                }
            }
            return null;
        }
    }
    [CustomEntity("PuzzleIslandHelper/IntroWarpCutsceneTrigger")]
    [Tracked]
    public class IntroWarpCutsceneTrigger : WarpCutsceneTrigger
    {
        public IntroWarpCutsceneTrigger(EntityData data, Vector2 offset, EntityID id) : base(data, offset, id)
        {
        }
    }
    [CustomEntity("PuzzleIslandHelper/OutroWarpCutsceneTrigger")]
    [Tracked]
    public class OutroWarpCutsceneTrigger : WarpCutsceneTrigger
    {
        public OutroWarpCutsceneTrigger(EntityData data, Vector2 offset, EntityID id) : base(data, offset, id)
        {
        }
    }
}
