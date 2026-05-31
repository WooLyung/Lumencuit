using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Lumencuit
{
    /// <summary>
    /// 회로 요소 엔티티를 생성, 삭제, 관리하는 시스템입니다.
    /// </summary>
    public sealed class WorldSystem
    {
        private readonly WorldGrid worldGrid;
        private readonly List<EntityBlueprintStack> blueprints = new();
        private readonly List<IEntityEventListener> listeners = new();

        public WorldSystem(StageData stageData)
        {
            worldGrid = new(stageData);
            foreach (EntityBlueprintStack blueprint in stageData.Blueprints)
                blueprints.Add(blueprint.Clone());
        }

        public void AddListener(IEntityEventListener listener) => listeners.Add(listener);

        public int Width => worldGrid.Width;
        public int Height => worldGrid.Height;
        public bool IsEnabledTile(int x, int y) => worldGrid.IsEnabledTile(x, y);
        public bool IsInside(int x, int y) => worldGrid.IsInside(x, y);
        public bool HasEntityAt(int x, int y) => worldGrid.HasEntityAt(x, y);
        public Entity GetEntityAt(int x, int y) => worldGrid.GetEntityAt(x, y);

        public bool TryGetEntityAt(int x, int y, out Entity entity)
        {
            if (HasEntityAt(x, y))
            {
                entity = GetEntityAt(x, y);
                return true;
            }
            entity = null;
            return false;
        }

        public EntityRequestResult TryCreateEntityByBlueprint(EntityBlueprint blueprint, int x, int y)
        {
            if (!worldGrid.IsEnabledTile(x, y))
                return EntityRequestResult.InvalidTile;
            if (worldGrid.HasEntityAt(x, y))
                return EntityRequestResult.AlreadyExist;
            if (!blueprints.Any(blueprintStack => blueprintStack.Blueprint == blueprint && blueprintStack.Count > 0))
                return EntityRequestResult.UnavailableBlueprint;
            
            for (int i = 0; i < blueprints.Count; i++)
            {
                if (blueprints[i].Blueprint == blueprint && blueprints[i].Count > 0)
                {
                    blueprints[i].Count--;
                    break;
                }
            }

            Entity entity = new Entity(blueprint.Type.ToElement(), blueprint.SignalColor.ToSignal());
            worldGrid.SetEntityAt(entity, x, y);
            NotifyEntityCreated(entity, new Vector2Int(x, y));

            return EntityRequestResult.Success;
        }

        public EntityRequestResult TryCreateEntity(Entity entity, int x, int y)
        {
            if (!worldGrid.IsEnabledTile(x, y))
                return EntityRequestResult.InvalidTile;
            if (worldGrid.HasEntityAt(x, y))
                return EntityRequestResult.AlreadyExist;

            worldGrid.SetEntityAt(entity, x, y);
            NotifyEntityCreated(entity, new Vector2Int(x, y));

            return EntityRequestResult.Success;
        }

        public EntityRequestResult TryRemoveEntity(int x, int y)
        {
            if (!worldGrid.IsEnabledTile(x, y))
                return EntityRequestResult.InvalidTile;
            if (!worldGrid.HasEntityAt(x, y))
                return EntityRequestResult.IsEmpty;

            Entity entity = worldGrid.GetEntityAt(x, y);
            worldGrid.RemoveEntityAt(x, y);
            NotifyEntityRemoved(entity, new Vector2Int(x, y));

            return EntityRequestResult.Success;
        }

        private void NotifyEntityCreated(Entity entity, Vector2Int pos)
        {
            IEntityEventListener.EntityCreatedEvent e = new IEntityEventListener.EntityCreatedEvent(entity, pos);
            foreach (IEntityEventListener listener in listeners)
                listener.OnEntityCreated(e);
        }

        private void NotifyEntityRemoved(Entity entity, Vector2Int pos)
        {
            IEntityEventListener.EntityRemovedEvent e = new IEntityEventListener.EntityRemovedEvent(entity, pos);
            foreach (IEntityEventListener listener in listeners)
                listener.OnEntityRemoved(e);
        }
    }
}