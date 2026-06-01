using UnityEngine;
using static UnityEditor.PlayerSettings;

namespace Lumencuit
{
    /// <summary>
    /// 월드 시스템에서의 엔티티 변화를 감지합니다.
    /// </summary>
    public interface IEntityEventListener
    {
        public class EntityCreatedEvent
        {
            public readonly Entity Entity;
            public readonly Vector2Int Pos;

            public EntityCreatedEvent(Entity entity, Vector2Int pos)
            {
                Entity = entity;
                Pos = pos;
            }
        }

        public class EntityRemovedEvent
        {
            public readonly Entity Entity;
            public readonly Vector2Int Pos;

            public EntityRemovedEvent(Entity entity, Vector2Int pos)
            {
                Entity = entity;
                Pos = pos;
            }
        }

        public class EntityPortUpdatedEvent
        {
            public readonly Entity Entity;
            public readonly Vector2Int Pos;

            public EntityPortUpdatedEvent(Entity entity, Vector2Int pos)
            {
                Entity = entity;
                Pos = pos;
            }
        }

        public class GridUpdatedEvent
        {
            public readonly WorldGrid WorldGridClone;

            public GridUpdatedEvent(WorldGrid worldGridClone)
            {
                WorldGridClone = worldGridClone;
            }
        }

        /// <summary>
        /// 엔티티가 생성될 때 호출됩니다.
        /// </summary>
        public void OnEntityCreated(EntityCreatedEvent e) { }

        /// <summary>
        /// 엔티티가 삭제될 때 호출됩니다.
        /// </summary>
        public void OnEntityRemoved(EntityRemovedEvent e) { }

        /// <summary>
        /// 엔티티 포트에 변화가 생겼을 때 호출됩니다.
        /// </summary>
        public void OnEntityPortUpdated(EntityPortUpdatedEvent e) { }

        /// <summary>
        /// 그리드의 변경이 끝난 후 호출됩니다.
        /// </summary>
        public void OnGridUpdated(GridUpdatedEvent e) { }
    }
}
