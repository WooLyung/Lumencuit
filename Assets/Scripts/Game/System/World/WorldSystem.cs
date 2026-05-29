using System.Collections.Generic;
using UnityEngine;

namespace Lumencuit
{
    /// <summary>
    /// 게이트를 포함한 장치들을 생성, 삭제, 관리하는 시스템입니다.
    /// </summary>
    public sealed class WorldSystem
    {
        private readonly WorldGrid worldGrid;
        private readonly List<IEntityEventListener> listeners = new();

        public WorldSystem()
        {
            worldGrid = new(10, 10);
        }

        public void AddListener(IEntityEventListener listener) => listeners.Add(listener);

        public bool CreateEntity(Entity entity, int x, int y)
        {
            if (!worldGrid.IsInside(x, y))
                return false;
            if (worldGrid.HasEntityAt(x, y))
                return false;
            worldGrid.SetEntityAt(entity, x, y);

            IEntityEventListener.EntityCreateEvent e = new IEntityEventListener.EntityCreateEvent(entity, new Vector2Int(x, y));
            foreach (IEntityEventListener listener in listeners)
                listener.OnEntityCreate(e);

            return true;
        }

        public bool RemoveEntity(int x, int y)
        {
            if (!worldGrid.IsInside(x, y))
                return false;
            if (!worldGrid.HasEntityAt(x, y))
                return false;

            IEntityEventListener.EntityRemoveEvent e = new IEntityEventListener.EntityRemoveEvent(worldGrid.GetEntityAt(x, y), new Vector2Int(x, y));
            foreach (IEntityEventListener listener in listeners)
                listener.OnEntityRemove(e);

            worldGrid.RemoveEntityAt(x, y);
            return true;
        }
    }
}