using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Globalization;
using System.Text.RegularExpressions;
using YamlDotNet.Core.Tokens;
using static Celeste.Mod.PuzzleIslandHelper.Entities.FrequencyEntities.FrequencyData;
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/FrequencyDecal")]
    [Tracked]
    public class FrequencyDecal : Entity
    {
        public Sprite sprite;
        public Image image;
        public bool Outline;
        private float scaleX, scaleY, rotation, rotationRate, rotationInterval;
        public FlagList visibleFlag;
        private Color color;
        private string decalPath;
        public FlagList frequencyFlag;
        private float frequencyStartRadius;
        private float frequencyEndRadius;
        public float[] targetFrequencies = new float[4];
        public float Percent;
        public bool ModifiesFrequency => Active && frequencyFlag && Array.Exists(targetFrequencies, f => f >= 0);
        private bool colliding;
        private FrequencyMod mod;
        public FrequencyDecal(EntityData data, Vector2 offset)
        : base(data.Position + offset)
        {
            scaleX = data.Float("scaleX", 1);
            scaleY = data.Float("scaleY", 1);
            rotation = data.Float("rotation").ToRad();
            rotationRate = data.Float("rotationRate").ToRad();
            rotationInterval = data.Float("rotationInterval", -1);
            visibleFlag = data.FlagList("visibleFlag");
            frequencyFlag = data.FlagList("frequencyFlag");
            Outline = data.Bool("outline");
            Depth = data.Int("depth", 2);
            decalPath = data.Attr("decalPath");
            color = data.HexColor("color");
            frequencyStartRadius = data.Float("startRadius");
            frequencyEndRadius = data.Float("endRadius");
            targetFrequencies = [data.Float("frequencyA", -1), data.Float("frequencyB", -1), data.Float("frequencyC", -1), data.Float("frequencyD", -1)];
            Tag |= Tags.TransitionUpdate;

            Add(mod = new FrequencyMod((f, i) =>
            {
                if (i >= 0 && i < 5 && targetFrequencies[i] >= 0)
                {
                    return Calc.LerpClamp(f,targetFrequencies[i], Percent);
                }
                return f;
            })
            { });
            Add(new PlayerCollider(p =>
            {
                colliding = true;
                float dist = Vector2.Distance(Center, p.Center);
                if (dist < frequencyStartRadius)
                {
                    Percent = 1;
                }
                else if (dist < frequencyEndRadius)
                {
                    Percent = 1 - (dist - frequencyStartRadius) / (frequencyEndRadius - frequencyStartRadius);
                }
                else
                {
                    Percent = 0;
                }
            }));
            Add(new PostUpdateHook(() =>
            {
                colliding = false;
            }));
        }
       
        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (!string.IsNullOrEmpty(decalPath))
            {
                sprite = new Sprite(GFX.Game, "decals/");
                sprite.AddLoop("idle", decalPath, 0.1f);
                sprite.Color = color;
                sprite.Play("idle");
                sprite.CenterOrigin();
                sprite.Position += sprite.HalfSize();
                sprite.Scale = new Vector2(scaleX, scaleY);
                sprite.Rotation = rotation;
                Add(sprite);
            }
            Collider = new Circle(frequencyEndRadius);
            if (sprite != null)
            {
                Collider.Center = sprite.Center;
            }
        }
        public override void Update()
        {
            base.Update();
            mod.Enabled = frequencyFlag && colliding;
            if (rotationRate != 0 && (rotationInterval < 0 || Scene.OnInterval(rotationInterval)))
            {
                rotation = (rotation + rotationRate) % MathHelper.TwoPi;
            }
            if (sprite != null)
            {
                sprite.Visible = visibleFlag;
                sprite.Rotation = rotation;
            }
        }

        public override void Render()
        {
            if (Outline)
            {
                if (sprite != null && sprite.Visible)
                {
                    sprite.DrawSimpleOutline();
                }
            }
            base.Render();

        }
    }
}
