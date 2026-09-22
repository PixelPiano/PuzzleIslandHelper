using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.WIP
{
    [CustomEntity("PuzzleIslandHelper/BlowAwayDecal")]
    [Tracked]
    public class BlowAwayDecal : Entity
    {
        [Tracked]
        public class BlowAwayDecalController : Entity
        {
            public float Timer;
            public Vector2 Direction;
            public BlowAwayDecalController() : base()
            {
                Add(new DashListener((v) =>
                {
                    Timer = 1.4f;
                    Direction = v;
                }));
            }
            public override void Update()
            {
                base.Update();
                if (Scene.GetPlayer() is Player player && player.DashAttacking)
                {
                    if (Scene.Tracker.GetEntity<BlowAwayDecal>() != null)
                    {
                        List<Entity> entities = Scene.Tracker.GetEntities<BlowAwayDecal>();
                        List<Entity> colliding = [];
                        foreach (Entity e in entities)
                        {
                            if (player.CollideCheck(e))
                            {
                                colliding.Add(e);
                            }
                        }
                        colliding = [.. colliding.OrderBy(item => item.Depth)];
                        int targetDepth = -1;
                        foreach (BlowAwayDecal decal in colliding)
                        {
                            if (decal.Depth < player.Depth || decal.BlowingAway) continue;
                            else if (targetDepth < 0)
                            {
                                targetDepth = Depth;
                                decal.BlowAway(Direction);
                            }
                            else if (targetDepth < decal.Depth)
                            {
                                break;
                            }
                        }
                    }

                }
            }
        }
        public float Rotation;
        public Vector2 Scale;
        public Color Color;
        public Sprite Sprite;
        public float sineTimer = Calc.Random.NextFloat();
        public string Path;
        public List<List<MTexture>> Segments = [];
        public bool BlowingAway;
        public int SliceSize;
        public float SliceSinIncrement;
        public float Offset;
        public float WaveSpeed;
        public float WaveAmplitude;
        private float rippleWait;
        private float rippleEaseTime;
        private float rippleAmplitudeAdd;
        public bool DoEase;
        private float mult;
        private float scaleMult = 1;
        private float collapsePercent;
        private float multEased;
        public bool Stopped;
        private float tearMult;
        public bool Persistent;
        private Vector2 dir;
        public BlowAwayDecalController Controller;
        private EntityID id;
        private Coroutine rippleCoroutine;
        public float AdditionalAmplitude;
        public BlowAwayDecal(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
        {
            this.id = id;
            rippleWait = data.Float("rippleWait");
            rippleEaseTime = data.Float("rippleEaseTime");
            rippleAmplitudeAdd = data.Float("rippleAmplitudeAdd");
            Persistent = data.Bool("persistent");
            Path = "decals/" + data.Attr("path");
            Scale = new Vector2(data.Float("scaleX", 1), data.Float("scaleY", 1));
            Rotation = MathHelper.ToRadians(data.Float("rotation"));
            Color = data.HexColor("color", Color.White);
            Depth = data.Int("depth");
            SliceSize = data.Int("sliceSize", 1);
            SliceSinIncrement = data.Float("sliceSinIncrement", 0.1f);
            Offset = data.Float("offset");
            WaveSpeed = data.Float("waveSpeed");
            WaveAmplitude = data.Float("waveAmplitude");
            Add(rippleCoroutine = new Coroutine(false));
            if (data.Bool("periodicRipple"))
            {
                RippleHold(rippleEaseTime, rippleWait, rippleAmplitudeAdd);
            }
        }
        private IEnumerator rippleRoutine(float inOutTime, float waitTime, float amplitude)
        {
            AdditionalAmplitude = 0;
            for (float i = 0; i < 1; i += Engine.DeltaTime / inOutTime)
            {
                AdditionalAmplitude = Ease.SineInOut(i) * amplitude;
                yield return null;
            }
            yield return waitTime;
            for (float i = 0; i < 1; i += Engine.DeltaTime / inOutTime)
            {
                AdditionalAmplitude = (1 - Ease.SineInOut(i)) * amplitude;
                yield return null;
            }
            AdditionalAmplitude = 0;
            RippleHold(inOutTime, waitTime, amplitude);
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Sprite = new Sprite(GFX.Game, Path);
            Sprite.Scale = Scale;
            Sprite.Rotation = Rotation;
            Sprite.Color = Color;
            Sprite.AddLoop("idle", "", 0.1f);
            Sprite.Play("idle");
            Sprite.CenterOrigin();
            Collider = new Hitbox(Sprite.Width, Sprite.Height, -Sprite.Width / 2, -Sprite.Height / 2);
            foreach (MTexture texture in Sprite.GetFrames(""))
            {
                List<MTexture> list = new List<MTexture>();
                for (int i = 0; i < texture.Height; i += SliceSize)
                {
                    list.Add(texture.GetSubtexture(0, i, texture.Width, SliceSize));
                }
                Segments.Add(list);
            }
            Add(Sprite);
            Controller = PianoUtils.SeekController(scene, () => { return new BlowAwayDecalController(); });
        }
        public void BlowAway(Vector2 direction)
        {
            rippleCoroutine.Cancel();
            AdditionalAmplitude = 0;
            amplitudeMult = 1;
            rippleTimer = 0;
            Collider = new Hitbox(Width / 2, Height / 2, -Width / 4, -Height / 4);
            BlowingAway = true;
            Speed = direction * 120f;
            Tween.Set(this, Tween.TweenMode.Oneshot, 2, Ease.CubeOut, t =>
            {
                timeMult = Calc.LerpClamp(5, 1f, t.Eased);
            });
        }
        public void Ripple(float time = 0.4f)
        {
            tearMult = 0;
            rippleTimer = time;
            amplitudeMult = 0.5f;
        }
        public void RippleHold(float easeTime = 0.4f, float waitTimer = 1, float amplitude = 0.5f)
        {
            tearMult = 0;
            rippleTimer = easeTime * 2 + waitTimer;
            amplitudeMult = 0;
            rippleCoroutine.Replace(rippleRoutine(easeTime, waitTimer, amplitude));
        }
        public Vector2 Speed;
        private float timeMult = 5;
        private float amplitudeMult = 1;
        public override void Render()
        {
            if (BlowingAway || rippleTimer > 0)
            {
                float tearAmplitude = Width / 8 * multEased * tearMult;
                float amplitude = WaveAmplitude + AdditionalAmplitude;
                List<MTexture> list = Segments[Sprite.CurrentAnimationFrame];
                for (int i = 0; i < list.Count; i++)
                {
                    float percent = i / (float)list.Count;
                    //if (percent < collapsePercent) continue;
                    double tear = (Math.Sin(percent) * tearAmplitude);
                    double sin = Math.Sin(sineTimer * WaveSpeed + i * SliceSinIncrement);
                    float x = (float)(sin + tear) * amplitude * amplitudeMult;
                    list[i].Draw(
                        position: Sprite.RenderPosition - (Vector2.UnitX * (float)(tearAmplitude * amplitude * amplitudeMult) / 2) + new Vector2(x * multEased, 0f).Rotate(Rotation),
                        origin: new Vector2(Sprite.Origin.X, Sprite.Origin.Y - i * SliceSize),
                        color: Color.Lerp(Color, Color.Black, (collapsePercent * (1 - percent)) + (float)(sin + 1) / 2 * 0.5f * multEased),
                        Scale * scaleMult, Rotation);
                }
            }
            else
            {
                base.Render();
            }


        }
        private float rippleTimer;
        public override void Update()
        {
            Sprite.Scale = Scale;
            Sprite.Color = Color;
            Sprite.Rotation = Rotation;
            base.Update();

            if (rippleTimer > 0)
            {
                rippleTimer -= Engine.DeltaTime;
            }
            if ((BlowingAway || rippleTimer > 0) && !Stopped)
            {
                sineTimer += Engine.DeltaTime;
                mult = Calc.Approach(mult, 1, Engine.DeltaTime * 5);
            }
            else
            {
                mult = Calc.Approach(mult, 0, Engine.DeltaTime / 2);
            }
            multEased = Ease.SineInOut(mult);
            if (BlowingAway && !Stopped)
            {
                tearMult = (float)Math.Sin(sineTimer * timeMult);
                Rotation += Speed.Angle() * 0.005f;
                scaleMult = Calc.Approach(scaleMult, 0.8f, Engine.DeltaTime);
                Position += Speed * Engine.DeltaTime;
                Speed.X = Calc.Approach(Speed.X, 0, 40 * Engine.DeltaTime);
                Speed.Y = Calc.Approach(Speed.Y, 100f, 300f * Engine.DeltaTime);
                if (CollideCheck<Solid>())
                {
                    Stopped = true;
                    float from = Scale.Y;
                    Sprite.JustifyOrigin(0.5f, 1);
                    Sprite.Y += Sprite.Height / 2;
                    Tween.Set(this, Tween.TweenMode.Oneshot, 0.6f, Ease.CubeIn, t =>
                    {
                        collapsePercent = t.Eased;
                        Scale.Y = Calc.LerpClamp(from, 0, t.Eased);
                    }, t =>
                    {
                        if (Persistent)
                        {
                            SceneAs<Level>().Session.DoNotLoad.Add(id);
                        }
                        RemoveSelf();
                    });
                }
            }
        }
    }
}
