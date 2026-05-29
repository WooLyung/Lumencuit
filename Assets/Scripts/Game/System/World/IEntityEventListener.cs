using UnityEngine;

namespace Lumencuit
{
    /// <summary>
    /// 월드 시스템에서의 엔티티 변화를 감지합니다.
    /// </summary>
    public interface IEntityEventListener
    {
        public class EntityCreateEvent
        {
            public readonly Entity Entity;
            public readonly Vector2Int Pos;

            public EntityCreateEvent(Entity entity, Vector2Int pos)
            {
                Entity = entity;
                Pos = pos;
            }
        }

        public class EntityRemoveEvent
        {
            public readonly Entity Entity;
            public readonly Vector2Int Pos;

            public EntityRemoveEvent(Entity entity, Vector2Int pos)
            {
                Entity = entity;
                Pos = pos;
            }
        }

        /// <summary>
        /// 엔티티가 생성될 때 호출됩니다.
        /// </summary>
        public void OnEntityCreate(EntityCreateEvent e);

        /// <summary>
        /// 엔티티가 삭제될 때 호출됩니다.
        /// </summary>
        public void OnEntityRemove(EntityRemoveEvent e);
    }
}
