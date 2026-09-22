using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities.Programs;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using static Celeste.Overworld;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/FallingDebrisSpawner")]
    public class FallingDebrisSpawner : Entity
    {
        [Pooled]
        private class CustomDebris : Debris
        {
            public Solid OrigSolid;
            public CustomDebris() : base()
            {
                speed.Y = 40f;
            }
            public override void Awake(Scene scene)
            {
                base.Awake(scene);
                if (CollideFirst<Solid>() is Solid solid)
                {
                    OrigSolid = solid;
                    Collidable = false;
                }
                Depth = Calc.Random.Sign();
            }
            public override void Update()
            {
                float prevFadeLerp = fadeLerp;
                float prevLifeTimer = lifeTimer;
                float prevAlpha = alpha;
                alpha = 5 * Engine.DeltaTime;
                base.Update();
                fadeLerp = prevFadeLerp;
                lifeTimer = prevLifeTimer;
                alpha = prevAlpha;

                if (hasHitGround)
                {
                    if (fadeLerp < 1f)
                    {
                        fadeLerp = Calc.Approach(fadeLerp, 1f, 2f * Engine.DeltaTime);
                    }
                    if (lifeTimer > 0f)
                    {
                        lifeTimer -= Engine.DeltaTime;
                    }
                    else if (alpha > 0f)
                    {
                        alpha -= 4f * Engine.DeltaTime;
                        if (alpha <= 0f)
                        {
                            RemoveSelf();
                        }
                    }
                    image.Color = Color.Lerp(Color.White, Color.Gray, fadeLerp) * alpha;
                }

                if (!Collidable && OrigSolid != null)
                {
                    Collidable = true;
                    if (!CollideCheck(OrigSolid))
                    {
                        OrigSolid = null;
                    }
                    else
                    {
                        Collidable = false;
                        if (Y > SceneAs<Level>().Bounds.Bottom) RemoveSelf();
                    }
                }
            }
            public void MakeSmall()
            {
                Collider = new Hitbox(2f, 2f, -1f, -1f);
                image.Scale = Vector2.One * 0.5f;
                speed.Y /= 2;
            }
        }

        public FlagList Flag;
        public int MaxDebris;
        public int Loops;
        public float MaxInterval;
        public float MinInterval;
        public bool OnlyOnce;
        public bool Persistent;
        public float SmallDebrisChance;
        public float MaxXSpeed;
        public float MinXSpeed;
        public float MaxXOffset;
        public float MinXOffset;
        private EntityID id;
        private Coroutine coroutine;
        public Vector2 SpawnPosition;
        public char Tileset;
        private bool activated;
        private bool onFlagActivated;
        private bool prevFlagState;
        public FallingDebrisSpawner(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
        {
            this.id = id;
            onFlagActivated = data.Bool("onFlagActivated");
            Loops = data.Int("loops");
            Tileset = data.Char("tileset", '1');
            Flag = data.FlagList("flag");
            MaxDebris = data.Int("maxDebris");
            MaxInterval = data.Float("maxInterval");
            MinInterval = data.Float("minInterval");
            OnlyOnce = data.Bool("onlyOnce");
            Persistent = data.Bool("persistent");
            SmallDebrisChance = data.Float("smallDebrisChance");
            MaxXSpeed = data.Float("maxXSpeed");
            MinXSpeed = data.Float("minXSpeed");
            MaxXOffset = data.Float("maxXOffset");
            MinXOffset = data.Float("minXOffset");
            Add(coroutine = new Coroutine(false));
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            prevFlagState = !Flag;
        }
        public override void Update()
        {
            base.Update();
            bool flag = Flag;
            if (flag && (!activated || !OnlyOnce) && (!onFlagActivated || !prevFlagState))
            {
                activated = true;
                coroutine.Replace(routine());
                if (Persistent)
                {
                    SceneAs<Level>().Session.DoNotLoad.Add(id);
                }
            }
            prevFlagState = Flag;
        }
        private IEnumerator routine()
        {
            for (int i = 0; i < Loops; i++)
            {
                float interval = Calc.Random.Range(MinInterval, MaxInterval);
                if (interval > 0) yield return interval;
                for (int j = 0; j < MaxDebris; j++)
                {
                    Spawn();
                }
            }
            if (OnlyOnce)
            {
                RemoveSelf();
            }
        }
        public void Spawn()
        {
            CustomDebris debris = Scene.CreateAndAdd<CustomDebris>();
            Vector2 position = Position + Vector2.UnitX * Calc.Random.Range(MinXOffset, MaxXOffset);
            debris.Init(position, Tileset, true);
            debris.speed.X = Calc.Random.Range(MinXSpeed, MaxXSpeed);
            if (Calc.Random.Chance(SmallDebrisChance))
            {
                debris.MakeSmall();
            }

        }
    }
}