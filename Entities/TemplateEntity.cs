using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/TemplateEntity")] //an attribute that links plugins from the map editor to entity classes
    [Tracked] //lets us grab any/all instance of this entity in a level by using "level.Tracker.GetEntities<[ClassName]>()"
    public class TemplateEntity : Entity
    {
        private string flag;
        private bool inverted;

        private Sprite Sprite;
        private Player player;

        private Entity helperEntity;

        private bool flagState;

        private float timer;
        private const float waitTime = 1f;
        public TemplateEntity(EntityData data, Vector2 offset, EntityID id) : this(data.Position + offset) //'offset' is the position of the level the entity is in. 
        {
            //EntityData holds data.... for the entity. (wow)
            //Each entity will always have a position, a name, a width, a height, the name of the level, and an id.

            //Each entity has a 'Collider' property. A collider is useful for checking if the entity is colliding with another entity (duh)
            //The Collider's position is relative to the Entity's position...
            //...so setting the Collider's position to {16,8} will move it 16 pixels to the right and 8 pixels down from the Entity's position.
            Collider = new Hitbox(data.Width, data.Height);

            //EntityData also contains custom attributes from the entity's map editor plugin. (Covered more in "Loenn plugin template")
            flag = data.Attr("flag");
            inverted = data.Bool("inverted");

            //Sprites are components, which means you can add them to the entity to be managed automatically.
            Sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/templatePlugin/");
            Sprite.Color = data.HexColor("color", Color.White);

            //Add a loop to the sprite under the id "idle"
            Sprite.AddLoop("idle", "spin", 0.1f, 0); //string name, string path, float delay, params int[] frames
            Sprite.Add("spin", "spin", 0.1f, "idle"); //string name, string path, float delay, string into
            Sprite.Play("idle");
            //Add the sprite to the entity
            Add(Sprite);
        }

        //Additional constructor not required, but useful if you plan to use the entity directly in code somewhere else because recreating EntityData from scratch is annoying
        public TemplateEntity(Vector2 position) : base(position)
        {
            //usually you would feed all the values you need down into the final constructor to ensure it always functions properly...
            //...we're not doing that here because it would make the explanations messy.
            //an example of a good final constructor before feeding it down into the base constructor would be:

            /*
             * public TemplateEntity(Vector2 position, float width, float height, string flag = "", bool inverted = false) : base(position)
             * {
             *      Collider = new Hitbox(width, height);
             *      this.flag = flag;
             *      this.inverted = inverted;
             * }
             */
        }

        //Entity.cs has multiple overridable methods, making it super easy to get creative with it.

        //Called once the entity is added to the scene
        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(helperEntity = new Entity(Position));
        }

        //Called once all entities have been added to the scene 
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            //This method is useful for grabbing entities. It's not a good idea to grab entities in Added as some may have not yet loaded.
            Level level = scene as Level;
            player = level.Tracker.GetEntity<Player>();
        }

        //Called once per frame. Not called if Entity.Active is false.
        //Used for game logic, not for rendering.
        public override void Update()
        {
            base.Update();
            if (Scene is not Level level) return; //Quick and easy way to get the current level as a variable
            if (string.IsNullOrEmpty(flag))
            {
                flagState = true;
            }
            else
            {
                //if flag is false but we've inverted the condition, the flag state won't match inverted, which is true
                //otherwise, if flag is true and we haven't inverted the condition, the flag state won't match inverted, which is false
                flagState = level.Session.GetFlag(flag) != inverted;
            }

            //wait one second and then tell the sprite to play the "spin" animation.
            //the sprite automatically resets to the "idle" animation since we set the "into" parameter
            if (timer > 1f)
            {
                Sprite.Play("spin");
                timer = 0;
            }
            else
            {
                timer += Engine.DeltaTime;
            }
        }

        //Handles drawing and rendering. Not called if Entity.Visible is false.
        public override void Render()
        {
            //base.Render() renders all the components the Entity is managing, including our Machine component.
            //Any component that isn't Visible won't be rendered.
            base.Render();
        }

        //Called right before the entity gets removed from the scene. Useful if the entity adds other entities to the scene and needs to remove them.
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            //removes the entity we added to the scene earlier
            scene.Remove(helperEntity);
        }
    }
}
