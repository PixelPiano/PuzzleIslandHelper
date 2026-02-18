using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components.Visualizers;
using Celeste.Mod.PuzzleIslandHelper.Components.Visualizers.DSPs;
using Celeste.Mod.PuzzleIslandHelper.Entities.FrequencyEntities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using YamlDotNet.Core.Tokens;


namespace Celeste.Mod.PuzzleIslandHelper.Entities.Hints
{
    [CustomEntity("PuzzleIslandHelper/FrequencyOrb")]
    [Tracked]
    public class FrequencyOrb : Entity
    {
        public FrequencyOrbComponent Component;
        private static int[] getIndices(EntityData data)
        {
            List<int> output = [];
            for (int i = 0; i < 4; i++)
            {
                if (data.Bool("useIndex" + i))
                {
                    output.Add(i);
                }
            }
            return [.. output];
        }
        public FrequencyOrb(EntityData data, Vector2 offset)
            : this(getIndices(data), data.Position + offset, data.Position + offset + data.Nodes[0], data.HexColor("color"), data.HexColor("edge"), data.Float("scale", 1), data.Bool("alwaysActive", true), data.Bool("fadeWhenNotPlaying"))
        {

        }
        public FrequencyOrb(int index, Vector2 position, Vector2 node, Color color, Color edge, float scale = 1, bool alwaysActive = true, bool fadeWhenNotPlaying = false) : base(position)
        {
            Add(Component = new FrequencyOrbComponent(index, Vector2.Zero, node, color, edge, scale, alwaysActive, fadeWhenNotPlaying));
        }
        public FrequencyOrb(int[] indices, Vector2 position, Vector2 node, Color color, Color edge, float scale = 1, bool alwaysActive = true, bool fadeWhenNotPlaying = false) : base(position)
        {
            Add(Component = new FrequencyOrbComponent(indices, Vector2.Zero, node, color, edge, scale, alwaysActive, fadeWhenNotPlaying));
        }
    }
    [Tracked]
    public class FrequencyOrbComponent : Sprite
    {
        public float? CustomFrequency;

        public Vector2 From, To;
        public Color Edge;
        public int[] Indices;
        public int Index => Indices[0];
        public float Alpha = 1;
        private float wiggleMult;
        private Vector2 scaleAdder;
        private int prevIndex;
        private float timer;
        public string ID;
        public bool AlwaysActive;
        public bool Disabled;
        public bool CanFade;
        public bool CanMove = true;
        public bool CanWiggle = true;
        public bool AutoAdjustAlpha = true;
        public bool AutoAdjustWiggler = true;
        public bool Hidden = false;
        public const string DefaultPath = "objects/PuzzleIslandHelper/forkAmp/orb";
        public FrequencyOrbComponent(int index, Vector2 from, Vector2 to, Color color, Color edge, float scale = 1, bool alwaysActive = true, bool fadeWhenNotPlaying = false)
            : this([index], from, to, color, edge, scale, alwaysActive, fadeWhenNotPlaying) { }
        public FrequencyOrbComponent(int[] indices, Vector2 from, Vector2 to, Color color, Color edge, float scale = 1, bool alwaysActive = true, bool fadeWhenNotPlaying = false)
            : this(DefaultPath, indices, from, to, color, edge, scale, alwaysActive, fadeWhenNotPlaying) { }
        public FrequencyOrbComponent(string path, int index, Vector2 from, Vector2 to, Color color, Color edge, float scale = 1, bool alwaysActive = true, bool fadeWhenNotPlaying = false)
            : this(path, [index], from, to, color, edge, scale, alwaysActive, fadeWhenNotPlaying) { }
        public FrequencyOrbComponent(string path, int[] indices, Vector2 from, Vector2 to, Color color, Color edge, float scale = 1, bool alwaysActive = true, bool fadeWhenNotPlaying = false) : base(GFX.Game, path)
        {
            Indices = indices;
            From = from;
            To = to;
            Color = color;
            Edge = edge;
            Scale = Vector2.One * scale;
            AlwaysActive = alwaysActive;
            CanFade = fadeWhenNotPlaying;
        }
        public override void Added(Entity entity)
        {
            base.Added(entity);
            AddLoop("idle", "", 0.1f);
            Play("idle");
            CenterOrigin();
            if (!AlwaysActive && CanFade && AutoAdjustAlpha)
            {
                Alpha = 0;
            }
            if (CustomFrequency.HasValue)
            {
                SnapToCustomFrequency(CustomFrequency.Value);
            }
        }
        public override void Render()
        {
            if (!Hidden && Alpha > 0)
            {
                Color c = Color;
                Color *= Alpha;
                Scale += scaleAdder;
                base.Render();
                Scale -= scaleAdder;
                Color = c;
            }
        }
        public void SnapToGlobalFrequency(int index)
        {
            if (prevIndex != index)
            {
                timer = 0;
            }
            float rate = FrequencyData.GetRate(Scene, index, ID);
            float lerp = rate / FrequencyData.Max;
            Position = Vector2.Lerp(From, To, lerp);
        }
        public void SnapToCustomFrequency(float frequency)
        {
            float lerp = frequency / FrequencyData.Max;
            Position = Vector2.Lerp(From, To, lerp);
        }
        public override void Update()
        {
            base.Update();

            timer += Engine.DeltaTime;
            scaleAdder = Vector2.One * ((float)(Math.Sin(timer * 4) + 1) / 2f * wiggleMult) * 1f;
            float alphaTarget = 0;
            float wiggleTarget = 0;
            FrequencyData.TryGetIndex(Scene, out int index);
            if (!Disabled)
            {
                if (AlwaysActive || ForkAmpSound.GlobalPlaying)
                {
                    alphaTarget = 1;
                    if (CustomFrequency.HasValue)
                    {
                        if (CanMove) SnapToCustomFrequency(CustomFrequency.Value);
                        return;
                    }
                    if (Indices.Length > 1)
                    {
                        bool noneFound = true;
                        for (int i = 0; i < Indices.Length; i++)
                        {
                            if (index == Indices[i])
                            {
                                noneFound = false;
                                if (CanMove) SnapToGlobalFrequency(index);
                                wiggleTarget = 1;
                                break;
                            }
                        }
                        if (noneFound) wiggleTarget = 0;

                    }
                    else if (Indices.Length > 0)
                    {
                        if (CanMove) SnapToGlobalFrequency(Indices[0]);
                        wiggleTarget = index == Indices[0] ? 1 : 0;
                    }
                }
                else
                {
                    wiggleTarget = 0;
                    alphaTarget = 0;
                }
            }
            else
            {
                alphaTarget = AlwaysActive ? 1 : 0;
                wiggleTarget = 0;
            }
            if (AutoAdjustAlpha)
            {
                Alpha = CanFade ? Calc.Approach(Alpha, alphaTarget, Engine.DeltaTime) : 1;
            }
            if (AutoAdjustWiggler)
            {
                wiggleMult = CanWiggle ? Calc.Approach(wiggleMult, wiggleTarget, Engine.DeltaTime * 4) : 0;
            }
            prevIndex = index;
        }
    }
}
