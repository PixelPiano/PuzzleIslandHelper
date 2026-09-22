using Celeste.Mod.CommunalHelper;
using Celeste.Mod.CommunalHelper.Components;
using Celeste.Mod.CommunalHelper.Utils;
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked]
    internal class Model3D : Entity
    {
        public string Path;
        public Color Color = Color.White;
        public float Alpha = 1;
        public float Roll;
        public float Pitch;
        public float Yaw;
        public Vector3 Scale = Vector3.One;
        public MTexture MTexture;
        public Texture2D Texture;
        public Vector2 Scale2D => Scale.XY();
        public MTexture FallbackTexture => GFX.Game["objects/PuzzleIslandHelper/missingTexture"];

        //todo: update this once CommunalHelper gets the public methods update
        //todo: remove shapehelper once CommunalHelper gets public methods
        public Shape3D Shape;
        public Vector2 Origin;
        public void JustifyOrigin(float x, float y)
        {
            Origin = new Vector2(Width * x, Height * y);
        }
        public Model3D(string path, Vector2 position, MTexture texture = null)
            : this(path, position, texture, Vector2.One, 0, 0, 0) { }
        public Model3D(string path, Vector2 position, MTexture texture, Vector2 scale)
            : this(path, position, texture, scale, 0, 0, 0) { }
        public Model3D(string path, Vector2 position, MTexture texture, Vector2 scale, float roll, float pitch, float yaw) : base(position)
        {
            Path = path;
            Scale = new Vector3(scale, 1);
            MTexture = texture;
            Roll = roll;
            Pitch = pitch;
            Yaw = yaw;
            Collider = new Hitbox(32, 32);
            KeyComponent.ForKey(this, Microsoft.Xna.Framework.Input.Keys.I, () =>
            {
                toggle = !toggle;
            });
            Add(new BeforeRenderHook(() =>
            {
                //Shape.Position += new Vector3(Origin, 0);
            }));
            Add(new AfterRenderHook(() =>
            {
                //Shape.Position -= new Vector3(Origin, 0);
            }));
        }
        private bool toggle;
        public virtual Texture2D GetTexture()
        {
            return Texture;
        }
        public virtual Matrix GetMatrix()
        {
            return Matrix.CreateScale(Scale) * Matrix.CreateFromYawPitchRoll(Yaw, Pitch, Roll) * Matrix.CreateTranslation(Origin.X, -Origin.Y, 0);
        }
        
        public override void Added(Scene scene)
        {
            MTexture ??= FallbackTexture;
            Texture = MTexture.Texture.Texture_Safe;
            if (ShapeHelper.TryGetObj(Path, out var mesh))
            {
                Shape = new Shape3D(mesh)
                {
                    Matrix = GetMatrix(),
                    Texture = GetTexture()
                };
                Add(Shape);
            }
            base.Added(scene);
        }
        public override void Render()
        {
            if (Shape == null)
            {
                Draw.Rect(Position, 8, 8, Color.Red);
            }
            base.Render();
        }
        public override void Update()
        {
            base.Update();
            if (Shape != null)
            {
                Shape.Texture = GetTexture();
                Shape.Matrix = GetMatrix();
                Shape.SetTint(Color * Alpha);
            }
        }
    }
    [CustomEntity("PuzzleIslandHelper/WarpNode3D")]
    [TrackedAs(typeof(Model3D))]
    internal class WarpNode3D : Model3D
    {
        public WarpNode3D(EntityData data, Vector2 offset) : base("Models/PuzzleIslandHelper/WarpNode", data.Position + offset, GFX.Game["objects/PuzzleIslandHelper/testuv"])
        {
            Collider = new Hitbox(16, 16);
            Depth = -1000000;
        }
        public override void Update()
        {
            base.Update();
            //Pitch += Engine.DeltaTime * 3;
            //Roll += Engine.DeltaTime;
            Yaw += Engine.DeltaTime;
        }
    }
}
