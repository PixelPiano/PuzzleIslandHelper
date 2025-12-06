using Celeste.Mod.Entities;
using Iced.Intel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Monocle;
using System;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/ImpactReceiver")]
    [Tracked]
    public class ImpactReceiverBlock : Solid
    {
        public TileGrid tiles;
        public EffectCutout cutout;
        public char tileType;
        public float Alpha
        {
            get
            {
                return alpha;
            }
            set
            {
                tiles.Alpha = value;
                cutout.Alpha = value;
                alpha = value;
            }
        }
        private float alpha = 1;
        public EntityID ID;
        public string Code;
        public string Input;
        public bool Solved;
        public bool SetFlag;
        public string Flag;
        public ImpactReceiverBlock(EntityData data, Vector2 offset, EntityID id) : this(data.Position + offset, data.Width, data.Height, data.Char("tiletype", '3'), id, data.Attr("code")) { }

        public ImpactReceiverBlock(Vector2 position, float width, float height, char tileType, EntityID id, string code) : base(position, width, height, true)
        {
            Tag |= Tags.TransitionUpdate;
            Depth = -13000;
            this.tileType = tileType;
            Add(cutout = new EffectCutout());
            SurfaceSoundIndex = SurfaceIndex.TileToIndex[tileType];
            EnableAssistModeChecks = false;
            Code = code;
            ID = id;
            Add(new ImpactSignalComponent(item => AddInput(item.Key)));
        }
        public void AddInput(char c)
        {
            if (!Solved && !string.IsNullOrEmpty(Code))
            {
                Input += c;
                if (Input == Code)
                {
                    Solve();
                }
                else if (!Code.StartsWith(Input))
                {
                    Input = "";
                }
            }
        }
        public void Solve()
        {
            Solved = true;
            Collidable = false;
            Audio.Play("event:/game/general/passage_closed_behind", base.Center);
            Tween.Set(this, Tween.TweenMode.Oneshot, 1, Ease.Linear, t => Alpha = 1 - t.Eased, t => Alpha = 0);
            SceneAs<Level>().Session.DoNotLoad.Add(ID);
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            tiles = PianoUtils.GetTileGridOverlay(scene, X, Y, Width, Height, tileType);
            Add(tiles);
            Add(new TileInterceptor(tiles, highPriority: false));
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (CollideCheck<Player>())
            {
                Alpha = 0;
                Collidable = false;
            }
        }
    }

    [CustomEntity("PuzzleIslandHelper/ImpactReceiverFlag")]
    [Tracked]
    public class ImpactReceiverTrigger : Trigger
    {
        public ImpactReceiverTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            Code = data.Attr("code");
            Flag = data.Attr("flagOnSolved");
            FlagState = data.Bool("flagState");
            Add(new ImpactSignalComponent(item => AddInput(item.Key)));
        }
        public string Code;
        public string Input;
        public bool Solved;
        public bool SetFlag;
        public string Flag;
        public bool FlagState;
        public void AddInput(char c)
        {
            if (!Solved && !string.IsNullOrEmpty(Code))
            {
                Input += c;
                if (Input == Code)
                {
                    Solve();
                }
                else if (!Code.StartsWith(Input))
                {
                    Input = "";
                }

            }
        }
        public void Solve()
        {
            Solved = true;
            if (!string.IsNullOrEmpty(Flag))
            {
                SceneAs<Level>().Session.SetFlag(Flag, FlagState);
            }
        }
    }
}