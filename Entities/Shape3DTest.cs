using Celeste.Mod.CommunalHelper.Utils;
using Celeste.Mod.CommunalHelper.Components;
using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.PuzzleIslandHelper.Helpers;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked]
    [CustomEntity("PuzzleIslandHelper/Entity3D")]
    public class Shape3DTest : Entity
    {
        private Shape3D Shape;
        public Shape3DTest(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Collider = new Hitbox(32, 32);
            MTexture pink = GFX.Game["objects/PuzzleIslandHelper/pink"];
            string path = data.Attr("modelPath");
            if(ShapeHelper.TryGetObj(path, out var mesh))
            {
                Add(Shape = new Shape3D(mesh)
                {
                    Texture = pink.Texture.Texture_Safe,
                    Matrix = Matrix.CreateScale(32, 32, 32)
                });
            }
        }
    }
    
}