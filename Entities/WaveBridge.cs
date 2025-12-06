using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using YamlDotNet.Core.Tokens;
using static Celeste.Autotiler;
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/WaveBridge")]
    [Tracked]
    public class WaveBridge : Entity
    {
        public float Alpha = 1;
        public char Tile;
        public float Duration = 1;
        public EntityID eid;
        public FlagList Flag, FlagOnEnd;
        private bool prevState;
        public bool BlendIn;
        public float ChunkWidth = 8;
        public bool FromAbove;
        public float Interval = 0.2f;
        public float Delay;
        public bool AtTarget
        {
            get
            {
                foreach (var b in Slices)
                {
                    if (!b.AtTarget) return false;
                }
                return true;
            }
            set
            {
                if (value)
                {
                    foreach (var b in Slices)
                    {
                        if (!b.AtTarget)
                        {
                            b.Snap();
                        }
                    }
                }
            }
        }
        public enum Modes
        {
            All,
            Forwards,
            Backwards,
            FromEnds,
            FromMiddle
        }
        public Modes Mode;
        public List<WaveBlock> Slices = [];

        public WaveBridge(Vector2 position, EntityID eid, Modes mode, char tile, float width, float height, bool fromAbove, bool blendIn, string flag, string flagOnEnd, float delay = 0, float chunkWidth = 8, float duration = 1, float interval = 0.1f)
            : base(position)
        {
            Interval = interval;
            Duration = duration;
            Mode = mode;
            Flag = new FlagList(flag);
            FlagOnEnd = new FlagList(flagOnEnd);
            BlendIn = blendIn;
            this.eid = eid;
            Tile = tile;
            Collider = new Hitbox(width, height);
            FromAbove = fromAbove;
            Delay = delay;
            ChunkWidth = chunkWidth;
        }

        public WaveBridge(EntityData data, Vector2 offset, EntityID eid)
            : this(data.Position + offset, eid, data.Enum<Modes>("mode"),
                  data.Char("tiletype", '3'),
                  data.Width, data.Height,
                  data.Bool("fromAbove"),
                  data.Bool("blendIn", true),
                  data.Attr("flag"),
                  data.Attr("flagOnEnd"),
                  data.Float("delay"),
                  data.Float("chunkWidth", 8),
                  data.Float("duration", 1), data.Float("interval", 0.1f))

        {
        }


        public override void Added(Scene scene)
        {
            base.Added(scene);
            prevState = Flag;
            (Color, Color) pair = (Color.Magenta, Color.Cyan);
            int mult = FromAbove ? -1 : 1;
            for (float x = 0; x < Width; x += ChunkWidth)
            {
                Vector2 pos = Position + Vector2.UnitX * x;
                Vector2 node = pos + Vector2.UnitY * Height * mult;
                WaveBlock slice = new WaveBlock(pos, node, ChunkWidth, Height, Tile, Flag.RawSingle, "", 8, BlendIn, FromAbove, pair.Item1, pair.Item2, Delay);
                Slices.Add(slice);
                scene.Add(slice);
                pair = (pair.Item2, pair.Item1);
            }
            float sliceDelay = Interval;
            switch (Mode)
            {
                case Modes.Forwards:
                    foreach (var slice in Slices)
                    {
                        slice.Delay += sliceDelay;
                        sliceDelay += Interval;
                    }
                    break;
                case Modes.Backwards:
                    for (int i = (Slices.Count) - (1); i >= 0; i--)
                    {
                        Slices[i].Delay += sliceDelay;
                        sliceDelay += Interval;
                    }
                    break;
                case Modes.FromMiddle:

                    for (int i = 0, inverse = Slices.Count - 1; i < Slices.Count && inverse > 0 && i < inverse; i++, inverse--)
                    {
                        sliceDelay += Interval;
                    }
                    for (int i = 0, inverse = Slices.Count - 1; i < Slices.Count && inverse > 0 && i < inverse; i++, inverse--)
                    {
                        Slices[inverse].Delay += sliceDelay;
                        Slices[i].Delay += sliceDelay;
                        sliceDelay -= Interval;
                    }
                    break;
                case Modes.FromEnds:
                    for (int i = 0, inverse = Slices.Count - 1; i < Slices.Count && inverse >= 0 && i < inverse; i++, inverse--)
                    {
                        Slices[inverse].Delay += sliceDelay;
                        Slices[i].Delay += sliceDelay;
                        sliceDelay += Interval;
                    }
                    break;
            }

        }
        public void Snap()
        {
            started = true;
            FlagOnEnd.State = true;
            foreach (WaveBlock slice in Slices)
            {
                if (!slice.AtTarget)
                {
                    slice.Snap();
                }
            }
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            prevState = Flag;
            if (Flag)
            {
                Snap();
            }
        }
        private bool started;
        public void EaseIn()
        {
            if (started) return;
            started = true;
            Flag.State = true;
            foreach (var slice in Slices)
            {
                slice.Start(Duration);
            }
        }
        public override void Update()
        {
            bool flag = Flag;
            if (flag)
            {
                if (!started && flag && !prevState)
                {
                    EaseIn();
                }
                base.Update();
            }
            prevState = flag;
            if (started)
            {
                foreach (var s in Slices)
                {
                    if (!s.AtTarget)
                    {
                        return;
                    }
                }
                FlagOnEnd.State = true;
            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            foreach (var a in Slices)
            {
                a.RemoveSelf();
            }
        }
    }
}