using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Triggers
{
    [CustomEntity("PuzzleIslandHelper/FlagTalk")]
    [Tracked]
    public class FlagTalk : Trigger
    {
        private EntityID ID;
        private bool oncePerSession;
        private bool oncePerLevel;
        public List<(string, bool)> RequiredFlags;
        public List<(string, bool)> StartFlags;
        public List<(string, bool)> InvertFlags;
        private float talkDelay;
        private float timer;
        public bool Enabled => CheckRequiredFlags() && timer <= 0;
        public TalkComponent Talk;
        public FlagTalk(EntityData data, Vector2 offset, EntityID id) : base(data, offset)
        {
            ID = id;
            RequiredFlags = PianoUtils.ParseFlagsFromString(data.Attr("requiredFlags"));
            StartFlags = PianoUtils.ParseFlagsFromString(data.Attr("flagsOnBegin"));
            InvertFlags = PianoUtils.ParseFlagsFromString(data.Attr("flagsToInvert"));
            talkDelay = data.Float("talkBuffer");
            oncePerSession = data.Bool("oncePerSession");
            oncePerLevel = data.Bool("oncePerLevel");
            Add(Talk = new DotX3(Collider, Interact));
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Talk.Enabled = Enabled;
        }
        public override void Update()
        {
            base.Update();
            Talk.Enabled = Enabled;
            if(timer > 0)
            {
                timer -= Engine.DeltaTime;
            }
        }
        public void Interact(Player player)
        {
            Input.Dash.ConsumePress();
            foreach (var item in StartFlags)
            {
                item.Item1.SetFlag(item.Item2);
            }
            foreach (var item in InvertFlags)
            {
                item.Item1.InvertFlag();
            }
            base.OnEnter(player);
            timer = talkDelay;
            if (oncePerSession) SceneAs<Level>().Session.DoNotLoad.Add(ID);
            if (oncePerLevel) RemoveSelf();
        }

        public bool CheckRequiredFlags()
        {
            Level level = Scene as Level;
            return RequiredFlags.Count == 0 || RequiredFlags.Exists(item => level.Session.GetFlag(item.Item1) != item.Item2);
        }
    }
}
