using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/TallLamp")]
    public class TallLamp : Actor
    {
        public FlagList FlagSetOnInteract;
        public float Radius;
        private int startFade, endFade;
        public Color StartColor;
        private Color color
        {
            get => light.Color;
            set
            {
                light2.Color = value;
                light.Color = value;
                bulb.Color = value * lightAlpha;
            }
        }
        private float lightAlpha;
        private float bloomAlpha;
        public bool PlaySound;
        public bool Instant;
        public FlagList CanInteractFlag;
        public bool LightVisible
        {
            get => light.Visible;
            set
            {
                light.Visible = value;
                light2.Visible = value;
                bulb.Visible = value;
                bloom.Visible = value;
            }
        }
        private TalkComponent talk;
        private VertexLight light, light2;
        private BloomPoint bloom;
        private Image image;
        private Image bulb;
        private MTexture onTexture, offTexture;
        private Tween tween;
        private string path = "objects/PuzzleIslandHelper/tallLamp";
        public TallLamp(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Depth = 1;
            FlagSetOnInteract = data.FlagList("flagSetOnInteract");
            CanInteractFlag = data.FlagList("canInteractFlag");
            Radius = data.Float("lightRadius", 32);
            startFade = (int)(Radius / 2f);
            endFade = (int)Radius;
            StartColor = data.HexColor("lightColor", Color.White);
            lightAlpha = data.Float("lightAlpha", 1);
            bloomAlpha = data.Float("bloomAlpha", 1);
            PlaySound = data.Bool("playSound");
            Instant = data.Bool("instant");
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            onTexture = GFX.Game[path + "On"];
            offTexture = GFX.Game[path + "Off"];
            Rectangle r = new Rectangle(0, onTexture.Height - 18, 14, 18);
            Add(talk = new DotX3(r.X, r.Y, r.Width, r.Height, Vector2.UnitX * 19, p =>
            {
                SwitchState(Instant, PlaySound, true);
            }));
            Add(image = new Image(onTexture));
            Add(bulb = new Image(GFX.Game[path + "BulbOn"]));
            Add(light = new VertexLight(StartColor, lightAlpha, startFade, endFade));
            Add(light2 = new VertexLight(StartColor, lightAlpha, startFade, endFade));
            Add(bloom = new BloomPoint(bloomAlpha, Radius));
            Collider = onTexture.Collider();
            light.Position += new Vector2(17, 18);
            light2.Position += new Vector2(20, Height - 1);
            bloom.Position = light.Position;

            TurnOff(true, false);
        }
        public override void Render()
        {
            image.DrawSimpleOutline();
            base.Render();
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (FlagSetOnInteract)
            {
                TurnOn(true, false, false);
            }
        }
        public void TurnOn(bool instant = false, bool playSound = true, bool emitParticles = true)
        {
            tween?.Stop();
            tween?.RemoveSelf();
            if (playSound)
            {
                PlayClick();
            }
            FlagSetOnInteract.State = true;
            LightVisible = true;
            image.Texture = onTexture;
            if (!instant)
            {
                Color startColor = Color.White * 0.7f;
                color = startColor;
                bloom.Alpha = bloomAlpha;
                tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.BackOut, 1f, start: true);
                tween.OnUpdate = (Tween t) =>
                {
                    color = Color.Lerp(startColor, StartColor, t.Eased);
                    light.StartRadius = startFade + (1f - t.Eased) * 32f;
                    light.EndRadius = endFade + (1f - t.Eased) * 32f;
                    bloom.Alpha = (bloomAlpha + 0.5f * (1f - t.Eased)) * bloomAlpha;
                };
                tween.OnComplete = (Tween t) =>
                {
                    TurnOn(true, false, false);
                };
                Add(tween);
            }
            else
            {
                LightVisible = true;
                color = StartColor;
                light.StartRadius = startFade;
                light.EndRadius = endFade;
                bloom.Alpha = bloomAlpha;
            }
            if (emitParticles)
            {
                SceneAs<Level>().ParticlesFG.Emit(Torch.P_OnLight, 12, Position + light.Position, new Vector2(3f, 3f));
            }
        }
        public void PlayClick()
        {
            Audio.Play("event:/PianoBoy/Machines/ButtonPressC", Position + talk.Bounds.TopCenter());
        }
        public void TurnOff(bool instant = false, bool playSound = true)
        {
            tween?.Stop();
            tween?.RemoveSelf();
            FlagSetOnInteract.State = false;
            image.Texture = offTexture;
            if (playSound)
            {
                PlayClick();
            }
            if (!instant)
            {
                LightVisible = true;
                Color startColor = light.Color * 0.7f;
                color = startColor;
                float startBloom = bloom.Alpha;
                float startRadius = light.StartRadius;
                float endRadius = light.EndRadius;
                tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.BackOut, 1f, start: true);
                tween.OnUpdate = (Tween t) =>
                {
                    color = Color.Lerp(startColor, Color.Transparent, t.Eased);
                    light.StartRadius = startRadius * 0.7f * (1f - t.Eased);
                    light.EndRadius = endRadius * 0.7f * (1f - t.Eased);
                    bloom.Alpha = (startBloom * 0.5f * t.Eased);
                };
                tween.OnComplete = (Tween t) =>
                {
                    TurnOff(true, false);
                };
                Add(tween);
            }
            else
            {
                LightVisible = false;
                color = Color.Transparent;
                light.StartRadius = 0;
                light.EndRadius = 0;
                bloom.Alpha = 0;
            }
        }
        public void SwitchState(bool instant, bool playSound, bool emitParticles)
        {
            if (!FlagSetOnInteract)
            {
                TurnOn(instant, playSound, emitParticles);
            }
            else
            {
                TurnOff(instant, playSound);
            }
        }
        public override void Update()
        {
            talk.Enabled = CanInteractFlag;
            base.Update();
        }
    }
}
