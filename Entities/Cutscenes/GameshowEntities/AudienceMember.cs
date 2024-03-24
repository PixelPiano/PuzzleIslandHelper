using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.PuzzleIslandHelper.Cutscenes.GameshowEntities
{
    [CustomEntity("PuzzleIslandHelper/AudienceMember")]
    [Tracked]
    public class AudienceMember : Entity
    {
        public Sprite Sprite;
        public AudienceMember(Vector2 position, string faceType) : base(position)
        {
            Sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/gameshow/audience/" + faceType + "/");
            Sprite.AddLoop("idle", faceType + "Face", 0.1f);
            Sprite.AddLoop("cheer", faceType + "Laugh", 0.1f);
            Add(Sprite);
            Sprite.Play("idle");
        }
        public void Cheer()
        {
            Sprite.Play("cheer");
        }
        public void StopCheering()
        {
            Sprite.Play("idle");
        }
        public IEnumerator CheerRoutine(float duration)
        {
            Cheer();
            yield return duration;
            StopCheering();
        }
        public AudienceMember(EntityData data, Vector2 offset) : this(data.Position + offset, data.Attr("faceType"))
        {

        }
    }
}
