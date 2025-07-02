using Celeste.Mod.PuzzleIslandHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Runtime.CompilerServices;

namespace Celeste.Mod.PuzzleIslandHelper.Components
{
    [Tracked(false)]
    public class CalidusCollider : Component
    {
        public Action<Calidus> OnCollide;

        public Collider Collider;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public CalidusCollider(Action<Calidus> onCollide, Collider collider = null)
            : base(active: false, visible: false)
        {
            OnCollide = onCollide;
            Collider = collider;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public bool Check(Calidus calidus)
        {
            Collider collider = Collider;
            if (collider == null)
            {
                if (calidus.CollideCheck(base.Entity))
                {
                    OnCollide(calidus);
                    return true;
                }

                return false;
            }

            Collider collider2 = base.Entity.Collider;
            base.Entity.Collider = collider;
            bool num = calidus.CollideCheck(base.Entity);
            base.Entity.Collider = collider2;
            if (num)
            {
                OnCollide(calidus);
                return true;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void DebugRender(Camera camera)
        {
            if (Collider != null)
            {
                Collider collider = base.Entity.Collider;
                base.Entity.Collider = Collider;
                Collider.Render(camera, Color.HotPink);
                base.Entity.Collider = collider;
            }
        }
    }
}
