﻿using System.Collections;
using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.AssemblyPublicizer;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using vitmod;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Cutscenes.Prologue
{
    [CustomEntity("PuzzleIslandHelper/PIPrologueBridge")]
    [Tracked]
    public class PrologueBridge : Entity
    {
        private bool falling;

        private int width;

        private string flag;

        private bool left;

        private float actDelay;

        private float actSpeed;

        private SoundSource sfx;

        private List<Rectangle> tileSizes;

        private List<PrologueBridgeTile> tiles = new List<PrologueBridgeTile>();

        private int id;

        public PrologueBridge(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            width = data.Width;
            flag = data.Attr("flag");
            left = data.Bool("left");
            actDelay = data.Float("delay");
            actSpeed = data.Float("speed", 0.8f) / 10f;
            id = data.ID;
            Add(sfx = new SoundSource());
            tileSizes = new List<Rectangle>
            {
                new Rectangle(0, 0, 8, 20),
                new Rectangle(0, 0, 8, 13),
                new Rectangle(0, 0, 8, 13),
                new Rectangle(0, 0, 8, 8),
                new Rectangle(0, 0, 8, 8),
                new Rectangle(0, 0, 8, 8),
                new Rectangle(0, 0, 8, 7),
                new Rectangle(0, 0, 8, 8),
                new Rectangle(0, 0, 8, 8),
                new Rectangle(0, 0, 16, 16),
                new Rectangle(0, 0, 16, 16)
            };
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);

            Calc.PushRandom(id);
            int num = 0;
            int num2 = (int)Math.Floor((decimal)width / 8m);
            while (num < num2)
            {
                int num3 = ((num != num2 - 1) ? Calc.Random.Next(tileSizes.Count) : Calc.Random.Next(tileSizes.Count - 2));
                PrologueBridgeTile customBridgeTile = new PrologueBridgeTile(Position + new Vector2(num * 8, 0f), tileSizes[num3],num3);
                tiles.Add(customBridgeTile);
                SceneAs<Level>().Add(customBridgeTile);
                num = ((num3 < tileSizes.Count - 2) ? (num + 1) : (num + 2));
            }

            Calc.PopRandom();
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);

            (scene as Level).Session.SetFlag(flag, false);
        }

        public override void Update()
        {
            base.Update();
            if (SceneAs<Level>().Session.GetFlag(flag) && !falling)
            {
                falling = true;
                Add(new Coroutine(FallRoutine()));
            }
            if (SceneAs<Level>().Tracker.GetEntity<Player>()?.Dead ?? true)
            {
                sfx.Stop();
            }
        }

        public IEnumerator FallRoutine()
        {
            yield return actDelay;
            sfx.Play("event:/game/00_prologue/bridge_rumble_loop");
            for (int i = 0; i < tiles.Count; i++)
            {
                int tileIndex = (left ? (tiles.Count - 1 - i) : i);
                PrologueBridgeTile tile = tiles[tileIndex];
                tile.Fall();
                yield return actSpeed;
            }

            sfx.Stop();
        }
    }
}