using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Loaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Flora
{
    [Tracked]
    public abstract class Passenger : Actor
    {
        public string CutsceneID;
        public DotX3 Talk;
        public Image WIPImage;
        public Passenger(Vector2 position, float width, float height, string cutscene) : base(position)
        {
            Depth = 1;
            Add(WIPImage = new Image(GFX.Game["objects/PuzzleIslandHelper/passenger/wip"]));
            Collider = new Hitbox(width, height);
            CutsceneID = cutscene;
            Talk = new DotX3(Collider, Interact);
            if (!string.IsNullOrEmpty(cutscene))
            {
                Add(Talk);
            }
        }
        public Passenger(EntityData data, Vector2 offset) : this(data.Position + offset, 16, 16, data.Attr("cutsceneID"))
        {

        }
        public void Interact(Player player)
        {
            PassengerCutsceneLoader.LoadCustomCutscene(CutsceneID, this, player, SceneAs<Level>());
        }
        public override void Render()
        {
            /*            if (PianoModule.Session.DEBUGBOOL3)
                        {
                            if (Scene is not Level level) return;
                            Draw.SpriteBatch.End();
                            foreach(MeshComponent mesh in Meshes)
                            {
                                mesh.Draw(level.Camera.Matrix);
                            }
                            GameplayRenderer.Begin();
                        }
                        else
                        {
                            WIPImage.Render();
                        }*/

        }
    }
}