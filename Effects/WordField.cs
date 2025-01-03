﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using Celeste.Mod.Entities;
using Celeste.Mod.Backdrops;
using System.Collections.Generic;
using Celeste.Mod.PuzzleIslandHelper.Entities.WIP;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using System;

namespace Celeste.Mod.PuzzleIslandHelper.Effects
{
    [CustomBackdrop("PuzzleIslandHelper/WordField")]
    public class WordField : Backdrop
    {
        private string[] validSplits = [",", "\n", "{break}"];
        public string[] Phrases;
        public List<Chip> Chips = [];
        public int ChipCount => Chips.Count;
        public const int MaxChips = 100;
        public float WaitTimer;
        private float minSpeed, maxSpeed;
        private float minWait, maxWait;
        public static Rectangle Bounds = new Rectangle(-16, -16, 352, 212);
        private List<Chip> toRemove = [];
        private bool firstFrame = true;
        public WordField(BinaryPacker.Element data) : base()
        {
            minSpeed = data.AttrFloat("minSpeed");
            maxSpeed = data.AttrFloat("maxSpeed");
            minWait = data.AttrFloat("minWait");
            maxWait = data.AttrFloat("maxWait");

            string ph = data.Attr("phrases");
            string str = data.AttrBool("fromDialog") ? Dialog.Get(ph) : ph;
            Phrases = str.Split(validSplits, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        }

        public override void Update(Scene scene)
        {
            if (scene is not Level level || !IsVisible(level)) return;
            if (firstFrame)
            {
                WaitTimer = Calc.Random.Range(minWait, maxWait);
                firstFrame = false;
            }
            base.Update(scene);
            if (WaitTimer > 0)
            {
                WaitTimer -= Engine.DeltaTime;
            }
            else if (ChipCount < MaxChips)
            {
                AddChip(level);
            }
            foreach (Chip c in toRemove)
            {
                Chips.Remove(c);
            }
        }
        public void AddChip(Level level)
        {
            int depth = Calc.Random.Range(-10, 10);
            int size = Math.Max(1, 3 + depth / 2);
            float fadeTime = Calc.Random.Range(2f, 5f);
            float speed = Calc.Random.Range(minSpeed, maxSpeed) * Calc.Random.Sign();
            float alpha = Calc.Random.Range(0.3f, 0.6f);
            string phrase = Phrases.Random();
            Color color = Color.Lime.Shade(depth, 40);
            Vector2 position = level.Camera.Position - Vector2.One * 16 + Bounds.Random();

            Chip chip = new Chip(position, Phrases.Random(), Vector2.UnitX * speed, alpha, size, color);
            //chip.Scroll.X = Calc.Clamp(1 + (depth / 10f * 0.5f), 0.5f, 1.5f);
            //chip fades in, waits a random period between 10 and 20 seconds, then fades out and is removed.
            //chip.FadeTo(0.4f, startAlpha, fadeTime, Ease.SineInOut,
            chip.FlickerOn(
                delegate
                {
                    Alarm.Set(chip, Calc.Random.Range(10, 20f),
                    delegate
                    {
                        chip.FlickerOff(delegate { RemoveChip(chip); });
                        //chip.FadeTo(startAlpha, 0, fadeTime, Ease.SineInOut, delegate { RemoveChip(chip); });
                    }
                    );
                });
            chip.MakePersistent();
            level.Add(chip);
            Chips.Add(chip);
            WaitTimer = Calc.Random.Range(minWait, maxWait);
        }
        public void RemoveChip(Chip chip)
        {
            chip.RemoveSelf();
            toRemove.TryAdd(chip);
        }
    }
}