using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using System.Collections;
using Celeste.Mod.PuzzleIslandHelper.Components;
using System.Collections.Generic;
using System;
using Celeste.Mod.PuzzleIslandHelper.Entities.Cutscenes;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{

    [CustomEntity("PuzzleIslandHelper/DashCodeGate")]
    [Tracked]
    public class DashCodeGate : Solid
    {
        public int Requires;
        public int Collected => PianoModule.Session.CollectedIDs.Count;
        public int Size;
        private Vector2 offset;
        public float Counter
        {
            get => SceneAs<Level>().Session.GetSlider("RegisteredCollectedIDs");
            set => SceneAs<Level>().Session.SetSlider("RegisteredCollectedIDs", value);
        }
        public bool Opened
        {
            get => SceneAs<Level>().Session.GetFlag("DashCodeGate:" + id);
            set => SceneAs<Level>().Session.SetFlag("DashCodeGate:" + id, value);
        }
        private EntityID id;
        public DashCodeGate(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset, data.Width, data.Height, true)
        {
            this.id = id;
            Requires = data.Int("required");
            Size = (int)Math.Min(Width, Height);
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Add(new Coroutine(routine()));
        }
        private IEnumerator routine()
        {
            while (!Opened && Counter < Requires)
            {
                Player player = Scene.Tracker.GetEntity<Player>();
                if (player != null && Math.Abs(player.X - Center.X) < 80f && player.X < X)
                {
                    if (Counter == 0f && Collected > 0)
                    {
                        Audio.Play("event:/game/09_core/frontdoor_heartfill", Position);
                    }

                    int prevCounter = (int)Counter;
                    int target = Math.Min(Collected, Requires);
                    Counter = Calc.Approach(Counter, target, Engine.DeltaTime * (float)Requires * 0.8f);
                    if (prevCounter != (int)Counter)
                    {
                        yield return 0.1f;
                        if (Counter < (float)target)
                        {
                            Audio.Play("event:/game/09_core/frontdoor_heartfill", Position);
                        }
                    }
                }
                else
                {
                    Counter = Calc.Approach(Counter, 0f, Engine.DeltaTime * (float)Requires * 4f);
                }
                yield return null;
            }
            FreezeTimeBreak.Begin(this, '0', 1f);
            /*            while (true)
                        {

                            float speed = 5f;
                            float c = Counter;
                            if (delay > 0)
                            {
                                yield return delay;
                                delay = 0;
                            }
                            while (Counter < PianoModule.Session.CollectedIDs.Count)
                            {
                                c = (float)Math.Ceiling(Calc.Approach(Counter, PianoModule.Session.CollectedIDs.Count, speed * Engine.DeltaTime));
                                Counter = (int)c;
                                speed = Calc.Approach(speed, 30f, (speed / 5f) * 3f);
                                yield return Engine.DeltaTime * (30f - speed);
                            }
                            yield return null;
                        }*/
        }
        public override void Render()
        {
            Draw.HollowRect(Collider, Color.White);
            Draw.Rect(X + 1, Y + 1, Width - 2, Height - 2, Color.Black);

            float space = 4;
            float size = Math.Min(Width, Height);
            int maxPerRow = (int)((size - 8) / space);
            int rows = (int)Math.Ceiling(a: Requires / maxPerRow);
            int collected = Collected;
            for (int row = 1; row <= rows; row++)
            {
                int nodesInRow = (row * maxPerRow < Requires) ? maxPerRow : Requires - (row - 1) * maxPerRow;
                float y = Y + Height / 2 - (rows * space) / 2 + (row - 1) * space;
                float x = (X + Width / 2) + (-nodesInRow / 2 + 0.5f) * space;
                for (int col = 1; col <= nodesInRow; col++)
                {
                    Draw.Point(new Vector2(x + (col - 1) * space, y), Counter < collected ? Color.Red : Color.Lime);
                }
            }
        }
    }
}