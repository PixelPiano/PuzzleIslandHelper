using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod;
using MonoMod.Utils;
using System.Collections;
using System.Runtime.InteropServices.ComTypes;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/DebrisSpawner")]
    [Tracked]
    public class DebrisSpawner : Entity
    {
        private string flag;
        private bool inverted;
        private bool oncePerSession;
        private char tileType;
        private EntityID id;
        private bool State
        {
            get
            {
                if (string.IsNullOrEmpty(flag))
                {
                    return true;
                }
                bool flagState = SceneAs<Level>().Session.GetFlag(flag);
                return inverted ? !flagState : flagState;
            }
        }
        public DebrisSpawner(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
        {
            this.id = id;
            flag = data.Attr("flag");
            inverted = data.Bool("inverted");
            oncePerSession = data.Bool("onlyOncePerSession");
            tileType = data.Char("tileType");
            Collider = new Hitbox(data.Width,data.Height);
        }
        public override void Update()
        {
            base.Update();
            if (State)
            {
                EmitDebris();
            }
        }
        private void EmitDebris()
        {
            for (int i = 0; i < Width / 8f; i++)
            {
                for (int j = 0; j < Height / 8f; j++)
                {
                    Scene.Add(Engine.Pooler.Create<Debris>().Init(Position + new Vector2(4 + i * 8, 4 + j * 8), tileType).BlastFrom(Center));
                }
            }
            if (oncePerSession)
            {
                SceneAs<Level>().Session.DoNotLoad.Add(id);
                RemoveSelf();
            }
        }
    }
}