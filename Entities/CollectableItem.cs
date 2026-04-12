using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/CollectableItem")]
    [Tracked]
    public class CollectableItem : Entity
    {
        public Sprite Sprite;
        private string path;
        private bool doCutscene;
        private FlagData flagOnCollect;
        private FlagData flag;
        private bool permanent;
        private EntityID id;
        private bool removeOnCollect;
        public CollectableItem(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
        {
            path = data.Attr("texturePath");
            this.id = id;
            doCutscene = data.Bool("doCutscene");
            flagOnCollect = data.Attr("flagOnCollect");
            flag = data.Attr("flag");
            permanent = data.Bool("permanent");
            removeOnCollect = data.Bool("removeOnCollect");
            if (doCutscene)
            {
                Add(new GetItemComponent(p =>
                {
                    if (removeOnCollect && permanent)
                    {
                        SceneAs<Level>().Session.DoNotLoad.Add(id);
                    }
                }, data.Attr("flagOnCollect"), removeOnCollect, data.Attr("mainText"), data.Attr("subText"))
                {
                    RevertPlayerState = true,
                    WaitForInput = data.Bool("waitForInput", true)
                });
            }
            else
            {
                Add(new PlayerCollider(p =>
                {
                    flagOnCollect.State = !flagOnCollect.Inverted;
                    if (removeOnCollect)
                    {
                        if (permanent)
                        {
                            SceneAs<Level>().Session.DoNotLoad.Add(id);
                        }
                        RemoveSelf();
                    }
                }));
            }
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Add(Sprite = new Sprite(GFX.Game, path));
            Sprite.AddLoop("idle", "", 0.1f);
            Sprite.Play("idle");
            Collider = Sprite.Collider();
            Collidable = flag;
        }
        public override void Update()
        {
            base.Update();
            Collidable = flag;
        }
    }
}
