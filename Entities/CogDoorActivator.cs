using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using Celeste.Mod.Entities;
using System.Collections;
using FMOD.Studio;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System;
using System.IO;
using System.Windows.Media.Media3D;
using IL.MonoMod;
using static MonoMod.InlineRT.MonoModRule;
using System.Security.Policy;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{

    [CustomEntity("PuzzleIslandHelper/CogDoorActivator")]
    [TrackedAs(typeof(CogHolder))]
    public class CogDoorActivator : CogHolder
    {
        private float spins;
        public string DoorID;
        public List<CogDoor> LinkedDoors = new();
        private float rotations => Rotations;
        private bool co => DropCog;
        public CogDoorActivator(EntityData data, Vector2 offset) : base(data.Position + offset, data.Bool("onlyOnce"), Color.MediumPurple, 10f)
        {
            spins = data.Float("spins", 1);
            DoorID = data.Attr("doorID");
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            foreach (CogDoor door in (scene as Level).Tracker.GetEntities<CogDoor>())
            {
                if (!string.IsNullOrEmpty(door.DoorID) && DoorID == door.DoorID)
                {
                    LinkedDoors.Add(door);
                }
            }
        }
        public override IEnumerator WhileSpinning(Cog cog)
        {
            if (LinkedDoors.Count > 0)
            {
                while (Rotations < spins)
                {
                    yield return null;
                }
                foreach (CogDoor door in LinkedDoors)
                {
                    door.CheckRegister();
                }
            }
            yield return 0.2f;
            StopSpinning();
            yield return base.WhileSpinning(cog);
        }
        public override void Update()
        {
            base.Update();
            if (LinkedDoors.Count <= 0) return;
            foreach (CogDoor door in LinkedDoors)
            {
                if(!door.CanRevert && door.Amount >= 1) continue;
                door.Amount = Calc.Clamp(Rotations, 0, spins) / spins;
            }
        }
    }
}