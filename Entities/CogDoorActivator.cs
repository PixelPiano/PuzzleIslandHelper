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

    [CustomEntity("PuzzleIslandHelper/GearDoorActivator")]
    [TrackedAs(typeof(GearHolder))]
    public class GearDoorActivator : GearHolder
    {
        private float spins;
        public string DoorID;
        public List<GearDoor> LinkedDoors = new();
        public GearDoorActivator(EntityData data, Vector2 offset) : base(data.Position + offset, data.Bool("onlyOnce"), Color.MediumPurple, 10f)
        {
            spins = data.Float("spins", 1);
            DoorID = data.Attr("doorID");
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            foreach (GearDoor door in (scene as Level).Tracker.GetEntities<GearDoor>())
            {
                if (!string.IsNullOrEmpty(door.DoorID) && DoorID == door.DoorID)
                {
                    LinkedDoors.Add(door);
                }
            }
        }
        public override IEnumerator WhileSpinning(Gear gear)
        {
            if (LinkedDoors.Count > 0)
            {
                while (Rotations < spins)
                {
                    yield return null;
                }
                foreach (GearDoor door in LinkedDoors)
                {
                    door.CheckRegister(); //add the door state to a list to prevent open doors from closing if the player re-enters the room
                }
            }
            yield return 0.2f;
            StopSpinning();
            yield return base.WhileSpinning(gear);
        }
        public override void Update()
        {
            base.Update();
            foreach (GearDoor door in LinkedDoors)
            {
                if (!door.CanRevert && door.Amount >= 1) continue;
                door.Amount = Calc.Clamp(Rotations, 0, spins) / spins;
            }
        }
    }
}