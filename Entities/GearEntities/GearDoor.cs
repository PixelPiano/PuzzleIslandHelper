﻿using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using System.Collections.Generic;
using System;
using Celeste.Mod.PuzzleIslandHelper.Triggers;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.GearEntities
{

    [CustomEntity("PuzzleIslandHelper/GearDoor")]
    [Tracked]
    public class GearDoor : Solid
    {
        private readonly List<Image> images = new();
        public string DoorID;
        public bool Vertical;
        public bool UpOrLeft;
        public float Length;
        public Vector2 ClosedPosition;
        public Vector2 OpenPosition;
        public bool CanMoveBack;
        public bool Persistent;
        public bool Moved;
        public float Amount;
        public bool CanChange =>
            (CanMoveBack || (!CanMoveBack && !Moved)) &&
            (!Persistent || (Persistent && !PianoModule.Session.GearDoorStates.ContainsKey(EntityID)));
        public EntityID EntityID;
        public GearDoor(Vector2 position, bool vertical, bool upOrLeft, float length, string id, bool canRevert, bool persistent, EntityID entityID)
            : base(position, vertical ? 8 : length, vertical ? length : 8, false)
        {
            ClosedPosition = position;
            OpenPosition = ClosedPosition + (vertical ? Vector2.UnitY : Vector2.UnitX) * (upOrLeft ? -1 : 1) * length;
            Vertical = vertical;
            UpOrLeft = upOrLeft;
            Length = length;
            DoorID = id;
            CanMoveBack = canRevert;
            Persistent = persistent;
            EntityID = entityID;
            CreateTextures();
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (PianoModule.Session.GearDoorStates.ContainsKey(EntityID))
            {
                Moved = true;
                Amount = PianoModule.Session.GearDoorStates[EntityID];
                MoveDoorTo(Amount);
            }
        }
        public void CheckRegister()
        {
            if (Persistent && !PianoModule.Session.GearDoorStates.ContainsKey(EntityID))
            {
                PianoModule.Session.GearDoorStates.Add(EntityID, Amount);
            }
        }
        [Command("reveal_gears", "")]
        public static void RevealGears()
        {
            foreach (Gear g in Engine.Scene.Tracker.GetEntities<Gear>())
            {
                g.Visible = true;
                g.Active = true;
            }
        }
        public override void Update()
        {
            base.Update();
            MoveDoorTo(Amount);
        }
        public void MoveDoorTo(float percent)
        {
            if (!CanChange) return;
            MoveTo(Vector2.Lerp(ClosedPosition, OpenPosition, percent));
        }
        public GearDoor(EntityData data, Vector2 offset, EntityID id)
        : this(data.Position + offset,
               data.Bool("vertical"), data.Bool("upOrLeft"),
               data.Bool("vertical") ? data.Height : data.Width, data.Attr("doorID"),
               data.Bool("canRevert"), data.Bool("persistent"), id)
        {
        }

        public override void Render()
        {
            foreach (Image i in images)
            {
                i.DrawSimpleOutline();
            }
            base.Render();
        }
        private void CreateTextures()
        {
            Remove(images.ToArray());
            images.Clear();
            MTexture tex = GFX.Game["objects/PuzzleIslandHelper/gear/door"];
            int xm = Vertical ? 0 : 1, ym = Vertical ? 1 : 0;
            float rotation = xm * (float)(-Math.PI / 2);
            float xOff = xm * 8;
            Image first = new Image(tex.GetSubtexture(0, 0, 8, 8));
            first.Rotation = rotation;
            first.Y = xOff;
            Add(first);
            images.Add(first);
            for (int i = 8; i < xm * (Width - 8) + ym * (Height - 8); i += 8)
            {
                Image mid = new Image(tex.GetSubtexture(0, 8, 8, 8));
                mid.Position = new Vector2(xm * i, ym * i + xOff);
                mid.Rotation = rotation;
                Add(mid); images.Add(mid);
            }
            Image last = new Image(tex.GetSubtexture(0, 16, 8, 8));
            last.Position = new Vector2(xm * (Width - 8), ym * (Height - 8) + xOff);
            last.Rotation = rotation;
            Add(last);
            images.Add(last);
        }
    }

}