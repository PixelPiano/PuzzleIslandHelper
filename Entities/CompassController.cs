using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using Celeste.Mod.PuzzleIslandHelper.Entities.GearEntities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/CompassController")]
    [Tracked]
    public class CompassController : Entity
    {
        public FlagList Flag;
        public TalkComponent Talk;
        public Sprite Lectern;
        public Sprite Book;
        private float flipDelay = 0.1f;
        private float interactTimer;
        private bool lowerFlipRate;
        public bool Flipping;
        public CompassController(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Depth = 1;
            Lectern = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/compass/");
            Book = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/compass/");
            Lectern.AddLoop("active", "lecternOn", 0.1f);
            Lectern.AddLoop("inactive", "lecternOff", 0.1f);
            Book.AddLoop("active", "bookOn", 0.1f);
            Book.AddLoop("inactive", "bookOff", 0.1f);
            Book.AddLoop("flip", "bookFlip", 0.1f);
            Sprite flash = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/compass/");
            flash.Add("flash", "lecternFlash", 0.1f);
            flash.X -= 2;
            MTexture test = GFX.Game["objects/PuzzleIslandHelper/compass/lecternFlash06"];
            Add(Lectern, flash, Book);
            Lectern.Play("inactive");
            Book.Play("inactive");
            Collider = Lectern.Collider();
            Flag = data.FlagList();
            Book.OnLastFrame += (s) =>
            {
                if (s == "flip")
                {
                    if (lowerFlipRate && flipDelay > Engine.DeltaTime)
                    {
                        flipDelay = Math.Max(flipDelay - Engine.DeltaTime, Engine.DeltaTime * 2);
                        lowerFlipRate = false;
                        Book.animations["flip"].Delay = flipDelay;
                    }
                }
            };
            Add(Talk = new TalkComponent(new Rectangle(0, 0, (int)Width, (int)Height), Vector2.UnitX * Width / 2, player =>
            {
                string key = data.Attr("key");
                bool interacted = false;
                if (!string.IsNullOrEmpty(key))
                {
                    foreach (Compass c in Scene.Tracker.GetEntities<Compass>())
                    {
                        if (c.ID == key)
                        {
                            interacted = true;
                            c.Interact(player);
                        }
                    }
                }
                if (interacted)
                {
                    interactTimer = 0.5f;
                    flash.Play("flash");
                    if (Book.CurrentAnimationID == "flip")
                    {
                        lowerFlipRate = true;
                    }
                    Book.Play("flip");
                    Flipping = true;

                }
            }));
        }
        public void SnapFlip()
        {
            Flipping = false;
            interactTimer = 0;
            flipDelay = 0.1f;
            Book.animations["flip"].Delay = flipDelay;
            Book.Play(Talk.Enabled ? "active" : "inactive");
            lowerFlipRate = false;
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Talk.Enabled = Compass.Enabled && Flag;
            if (Talk.Enabled)
            {
                Lectern.Play("active");
                Book.Play("active");
            }
        }
        public override void Update()
        {
            Talk.Enabled = Compass.Enabled && Flag;
            Lectern.Play(Talk.Enabled ? "active" : "inactive");
            base.Update();
        }
    }
}