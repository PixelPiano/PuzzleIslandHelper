using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Cutscenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using static MonoMod.InlineRT.MonoModRule;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.WARP
{
    [CustomEntity("PuzzleIslandHelper/WarpCapsuleBeta")]
    [TrackedAs(typeof(WarpCapsule))]
    [Tracked]
    public class WarpCapsuleBeta : WarpCapsule
    {
        public const string LabID = "LabBetaCapsule";
        public const string TransitID = "TransitBetaCapsule";
        public const string TransitID2 = "TransitCapsule2";
        public static bool AllowWarpingToSameCapsule = true;
        public WarpCapsuleBeta(EntityData data, Vector2 offset, EntityID id)
            : base(data.Position + offset, id, data.Flag("disableFlag", "invertFlag"), data.Attr("warpID"),
                  "objects/PuzzleIslandHelper/protoWarpCapsule/")
        {
        }
        public override WarpData RetrieveWarpData(CapsuleList list)
        {
            return list.GetDataFromID(WarpID);
        }
        public override void Update()
        {
            base.Update();
            EvaluateOpenConditions();
        }
        public void EvaluateOpenConditions()
        {
            if (OwnWarpData == null || string.IsNullOrEmpty(OwnWarpData.ID))
            {
                TargetID = "";
                return;
            }
            if (OwnWarpData.ID == TransitID2 && (CalCut.Second.GetCutsceneFlag() || (Scene.Tracker.GetEntity<Calidus>() is Calidus c && c.Following)))
            {
                CalCut.First.Register();
                CalCut.FirstIntro.Register();
                CalCut.Second.Register();
                TargetID = LabID;
            }
            else if (OwnWarpData.ID == LabID && PianoModule.Session.RestoredPower)
            {
                CalCut.First.Register();
                CalCut.FirstIntro.Register();
                TargetID = TransitID2;
            }
            else if (SceneAs<Level>().Session.GetFlag("LabBetaWarpEnabled") && !CalCut.First.GetCutsceneFlag())
            {
                TargetID = TransitID;
            }
        }
        public override void Awake(Scene scene)
        {
            EvaluateOpenConditions();
            if (SceneAs<Level>().Session.GetFlag("keepLabBetaWarpOpen"))
            {
                CalCut.Second.Register();
                CalCut.SecondTryWarp.Register();
            }

            base.Awake(scene);
        }
        public override void Interact(Player player)
        {
            Teleport(player, TargetID);
        }
        public void Teleport(Player player, string id, bool pull = false, Action onEnd = null)
        {
            if (!Disabled)
            {
                if (PianoMapDataProcessor.WarpCapsules.TryGetValue(Scene.GetAreaKey(), out var list))
                {
                    if (list.GetDataFromID(id) is WarpData data && (AllowWarpingToSameCapsule || data != OwnWarpData))
                    {
                        Scene.Add(new CapsuleWarpHandler(this, data, player, onEnd, pull));
                    }
                }
            }
        }
        public override bool WarpEnabled()
        {
            return !string.IsNullOrEmpty(TargetID) &&  (AllowWarpingToSameCapsule || TargetID != WarpID);
        }
        [Command("open_to", "")]
        public static void OpenTo(string id)
        {
            foreach (WarpCapsuleBeta c in Engine.Scene.Tracker.GetEntities<WarpCapsuleBeta>())
            {
                c.TargetID = id;
            }
        }
    }
}
