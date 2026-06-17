using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Lumencuit
{
    /// <summary>
    /// 회로 요소 엔티티를 생성, 삭제, 관리하는 시스템입니다.
    /// </summary>
    public sealed class WorldSystem
    {
        private WorldGrid worldGrid;
        private readonly StageSaveAdapter stageSaveHandler;
        private readonly StageData stageData;

        private readonly List<EntityBlueprintStack> blueprints = new();
        private readonly List<IEntityEventListener> listeners = new();

        private readonly Stack<WorldSnapshot> undoStack = new();
        private readonly Stack<WorldSnapshot> redoStack = new();

        public WorldSystem(StageData stageData)
        {
            worldGrid = new(stageData);
            stageSaveHandler = new(this, stageData);
            this.stageData = stageData;

            // [임시] 카메라 위치 변경
            Camera.main.transform.position = new Vector3((stageData.Width - 1) / 2f, (stageData.Height - 1) / 2f, Camera.main.transform.position.z);
        }

        public void Init()
        {
            foreach (EntityBlueprintStack blueprint in stageData.Blueprints)
                blueprints.Add(blueprint.Clone());

            if (!stageSaveHandler.TryLoadStageData())
            {
                blueprints.Clear();
                foreach (EntityBlueprintStack blueprint in stageData.Blueprints)
                    blueprints.Add(blueprint.Clone());

                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        if (TryGetEntityAt(x, y, out Entity entity))
                        {
                            worldGrid.RemoveEntityAt(x, y);
                            NotifyEntityRemoved(entity, new Vector2Int(x, y));
                        }
                    }
                }
                InitPrePlacedBlueprint(stageData);
            }

            // 세이브파일을 불러오더라도 undo/redo 기록은 복구하지 않습니다.
            // undo 스택은 현재 그리드를 포함합니다.
            PushUndoStack();

            AddListener(stageSaveHandler);
            NotifyGridUpdated();
        }

        public void Reset()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (!TryGetEntityAt(x, y, out Entity entity))
                        continue;
                    worldGrid.RemoveEntityAt(x, y);
                    NotifyEntityRemoved(entity, new Vector2Int(x, y));
                }
            }

            blueprints.Clear();
            foreach (EntityBlueprintStack blueprint in stageData.Blueprints)
                blueprints.Add(blueprint.Clone());

            InitPrePlacedBlueprint(stageData);

            undoStack.Clear();
            redoStack.Clear();
            PushUndoStack();
            NotifyGridUpdated();
        }

        private void PushUndoStack()
        {
            undoStack.Push(new WorldSnapshot(worldGrid, blueprints));
            redoStack.Clear();
        }

        public void InitPrePlacedBlueprint(StageData stageData)
        {
            foreach (PrePlacedBlueprint ppb in stageData.PrePlacedBlueprints)
                TryPrePlaceEntity(ppb.Blueprint, ppb.Ports, ppb.Position.x, ppb.Position.y);
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

        public EntityRequestResult TryPrePlaceEntity(EntityBlueprint blueprint, Entity.Ports ports, int x, int y, bool isFixed = true)
        {
            if (!worldGrid.IsEnabledTile(x, y))
                return EntityRequestResult.InvalidTile;
            if (worldGrid.HasEntityAt(x, y))
                return EntityRequestResult.AlreadyExist;
            
            if (!isFixed && blueprint.Type != CircuitElement.CircuitElementType.Wire)
            {
                EntityBlueprintStack stack = blueprints.FirstOrDefault(stack => stack.Blueprint == blueprint && stack.Count > 0);
                if (stack == null)
                    return EntityRequestResult.UnavailableBlueprint;
                stack.Count--;
            }

            Entity entity = new Entity(blueprint.Clone(), ports, isFixed);
            worldGrid.SetEntityAt(entity, x, y);
            NotifyEntityCreated(entity, new Vector2Int(x, y));

            return EntityRequestResult.Success;
        }

        public EntityRequestResult TryCreateEntity(EntityBlueprint blueprint, int x, int y)
        {
            if (!worldGrid.IsEnabledTile(x, y))
                return EntityRequestResult.InvalidTile;
            if (worldGrid.HasEntityAt(x, y))
                return EntityRequestResult.AlreadyExist;

            EntityBlueprintStack stack = blueprints.FirstOrDefault(stack => stack.Blueprint == blueprint && stack.Count > 0);
            if (stack == null)
                return EntityRequestResult.UnavailableBlueprint;
            stack.Count--;

            Entity entity = new Entity(blueprint.Clone());
            worldGrid.SetEntityAt(entity, x, y);
            NotifyEntityCreated(entity, new Vector2Int(x, y));
            NotifyGridUpdated();

            PushUndoStack();
            return EntityRequestResult.Success;
        }

        public EntityRequestResult TryReplaceEntity(EntityBlueprint blueprint, int x, int y)
        {
            bool isWire = blueprint.Type == CircuitElement.CircuitElementType.Wire;

            if (!worldGrid.IsEnabledTile(x, y))
                return EntityRequestResult.InvalidTile;
            if (!worldGrid.TryGetEntityAt(x, y, out Entity oldEntity))
                return EntityRequestResult.IsEmpty;
            if (oldEntity.IsFixed)
                return EntityRequestResult.IsFixed;

            Entity newEntity = new Entity(blueprint.Clone(), oldEntity.GetPorts());
            if (newEntity.Element.InSignalCount < oldEntity.InPortCount || (isWire && oldEntity.InPortCount != 1))
                return EntityRequestResult.InvalidPort;
            if (newEntity.Element.OutSignalCount < oldEntity.OutPortCount || (isWire && oldEntity.OutPortCount != 1))
                return EntityRequestResult.InvalidPort;

            if (!isWire)
            {
                EntityBlueprintStack newStack = blueprints.FirstOrDefault(stack => stack.Blueprint == blueprint && stack.Count > 0);
                if (newStack == null)
                    return EntityRequestResult.UnavailableBlueprint;
                newStack.Count--;
            }

            EntityBlueprintStack oldStack = blueprints.FirstOrDefault(stack => stack.Blueprint == oldEntity.MadeBy);
            if (oldStack != null)
                oldStack.Count++;

            worldGrid.SetEntityAt(newEntity, x, y);
            NotifyEntityRemoved(oldEntity, new Vector2Int(x, y));
            NotifyEntityCreated(newEntity, new Vector2Int(x, y));
            NotifyGridUpdated();

            PushUndoStack();
            return EntityRequestResult.Success;
        }

        public EntityRequestResult TryRemoveEntity(int x, int y)
        {
            if (!worldGrid.IsEnabledTile(x, y))
                return EntityRequestResult.InvalidTile;

            if (!worldGrid.HasEntityAt(x, y))
                return EntityRequestResult.IsEmpty;

            Vector2Int pos = new Vector2Int(x, y);
            Entity entity = worldGrid.GetEntityAt(x, y);

            if (entity.IsFixed)
                return EntityRequestResult.IsFixed;
            if (WireHelper.IsWire(entity))
            {
                if (TryRemoveWireNetwork(pos))
                {
                    NotifyGridUpdated();
                    PushUndoStack();
                    return EntityRequestResult.Success;
                }
                return EntityRequestResult.Fail;
            }

            EntityBlueprintStack stack = blueprints.FirstOrDefault(stack => stack.Blueprint == entity.MadeBy);
            if (stack != null)
                stack.Count++;

            RemoveAllConnectedWireNetworks(new Vector2Int(x, y));
            worldGrid.RemoveEntityAt(x, y);
            NotifyEntityRemoved(entity, pos);
            NotifyGridUpdated();

            PushUndoStack();
            return EntityRequestResult.Success;
        }

        public EntityRequestResult TryRemoveEntityRange(Vector2Int start, Vector2Int end)
        {
            bool success = false;
            for (int x = Mathf.Min(start.x, end.x); x <= Mathf.Max(start.x, end.x); x++)
            {
                for (int y = Mathf.Min(start.y, end.y); y <= Mathf.Max(start.y, end.y); y++)
                {
                    if (!worldGrid.IsEnabledTile(x, y))
                        continue;

                    if (!worldGrid.HasEntityAt(x, y))
                        continue;

                    Vector2Int pos = new Vector2Int(x, y);
                    Entity entity = worldGrid.GetEntityAt(x, y);

                    if (entity.IsFixed)
                        continue;
                    if (WireHelper.IsWire(entity))
                    {
                        if (TryRemoveWireNetwork(pos))
                            success = true;
                        continue;
                    }

                    EntityBlueprintStack stack = blueprints.FirstOrDefault(stack => stack.Blueprint == entity.MadeBy);
                    if (stack != null)
                        stack.Count++;

                    RemoveAllConnectedWireNetworks(new Vector2Int(x, y));
                    worldGrid.RemoveEntityAt(x, y);
                    NotifyEntityRemoved(entity, pos);
                    success = true;
                }
            }

            if (success)
            {
                NotifyGridUpdated();
                PushUndoStack();
            }
            return success ? EntityRequestResult.Success : EntityRequestResult.Fail;
        }

        private void RemoveAllConnectedWireNetworks(Vector2Int pos)
        {
            if (!TryGetEntityAt(pos.x, pos.y, out Entity entity))
                return;
            if (WireHelper.IsWire(entity))
                return;

            if (entity.LeftPort != Entity.PortType.None)
                TryRemoveWireNetwork(pos + Vector2Int.left);
            if (entity.RightPort != Entity.PortType.None)
                TryRemoveWireNetwork(pos + Vector2Int.right);
            if (entity.UpPort != Entity.PortType.None)
                TryRemoveWireNetwork(pos + Vector2Int.up);
            if (entity.DownPort != Entity.PortType.None)
                TryRemoveWireNetwork(pos + Vector2Int.down);
        }

        private void SetPort(Entity entity, Vector2Int dir, Entity.PortType portType)
        {
            if (dir == Vector2Int.left)
                entity.LeftPort = portType;
            else if (dir == Vector2Int.right)
                entity.RightPort = portType;
            else if (dir == Vector2Int.up)
                entity.UpPort = portType;
            else if (dir == Vector2Int.down)
                entity.DownPort = portType;
        }

        private void SetPort(ref Entity.Ports ports, Vector2Int dir, Entity.PortType portType)
        {
            if (dir == Vector2Int.left)
                ports.Left = portType;
            else if (dir == Vector2Int.right)
                ports.Right = portType;
            else if (dir == Vector2Int.up)
                ports.Up = portType;
            else if (dir == Vector2Int.down)
                ports.Down = portType;
        }

        /// <summary>
        /// 제공된 경로로 선 생성을 시도합니다. 입출력 좌표를 포함합니다.
        /// </summary>
        /// <param name="path">선을 연결할 전체 경로</param>
        public EntityRequestResult TryCreateWire(List<Vector2Int> path)
        {
            if (path.Count < 3)
                return EntityRequestResult.NeedWire;

            Vector2Int startPos = path[0];
            Vector2Int endPos = path[^1];

            if (!worldGrid.IsEnabledTile(startPos.x, startPos.y) || !worldGrid.IsEnabledTile(endPos.x, endPos.y))
                return EntityRequestResult.InvalidTile;
            if (!worldGrid.HasEntityAt(startPos.x, startPos.y) || !worldGrid.HasEntityAt(endPos.x, endPos.y))
                return EntityRequestResult.IsEmpty;

            Entity startEntity = worldGrid.GetEntityAt(startPos.x, startPos.y);
            Entity endEntity = worldGrid.GetEntityAt(endPos.x, endPos.y);

            if (startEntity.OutPortCount >= startEntity.Element.OutSignalCount)
                return EntityRequestResult.UnavailablePort;
            if (endEntity.InPortCount >= endEntity.Element.InSignalCount)
                return EntityRequestResult.UnavailablePort;

            for (int i = 1; i < path.Count; i++)
            {
                Vector2Int prev = path[i - 1];
                Vector2Int curr = path[i];

                if (Mathf.Abs(prev.x - curr.x) + Mathf.Abs(prev.y - curr.y) != 1)
                    return EntityRequestResult.InvalidPath;
                if (!worldGrid.IsEnabledTile(curr.x, curr.y))
                    return EntityRequestResult.InvalidPath;
                if (i != path.Count - 1 && worldGrid.HasEntityAt(curr.x, curr.y))
                    return EntityRequestResult.InvalidPath;
            }

            for (int i = 1; i < path.Count - 1; i++)
            {
                Vector2Int pos = path[i];
                if (worldGrid.HasEntityAt(pos.x, pos.y))
                    return EntityRequestResult.AlreadyExist;
            }

            SetPort(startEntity, path[1] - startPos, Entity.PortType.Output);
            SetPort(endEntity, path[^2] - endPos, Entity.PortType.Input);
            NotifyEntityPortUpdated(startEntity, startPos);
            NotifyEntityPortUpdated(endEntity, endPos);

            EntityBlueprint wireBlueprint = new EntityBlueprint(CircuitElement.CircuitElementType.Wire);
            for (int i = 1; i < path.Count - 1; i++)
            {
                Vector2Int pos = path[i];
                Vector2Int prev = path[i - 1];
                Vector2Int next = path[i + 1];

                Entity.Ports ports = Entity.Ports.None;
                SetPort(ref ports, prev - pos, Entity.PortType.Input);
                SetPort(ref ports, next - pos, Entity.PortType.Output);

                Entity wireEntity = new Entity(wireBlueprint.Clone(), ports);
                worldGrid.SetEntityAt(wireEntity, pos.x, pos.y);
                NotifyEntityCreated(wireEntity, pos);
            }

            NotifyGridUpdated();
            PushUndoStack();
            return EntityRequestResult.Success;
        }

        public EntityRequestResult TryUndo()
        {
            if (undoStack.Count < 2)
                return EntityRequestResult.CantUndo;
            redoStack.Push(undoStack.Pop());

            WorldSnapshot snapshot = undoStack.Peek();
            UpdateToSnapshot(snapshot);

            NotifyGridUpdated();
            return EntityRequestResult.Success;
        }

        public EntityRequestResult TryRedo()
        {
            if (redoStack.Count < 1)
                return EntityRequestResult.CantRedo;
            undoStack.Push(redoStack.Pop());

            WorldSnapshot snapshot = undoStack.Peek();
            UpdateToSnapshot(snapshot);

            NotifyGridUpdated();
            return EntityRequestResult.Success;
        }

        private void UpdateToSnapshot(WorldSnapshot snapshot)
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    if (worldGrid.TryGetEntityAt(x, y, out Entity entity))
                    {
                        if (!snapshot.WorldGrid.TryGetEntityAt(x, y, out Entity snapshotEntity))
                            NotifyEntityRemoved(entity, pos);
                        else if (snapshotEntity.MadeBy != entity.MadeBy)
                        {
                            NotifyEntityRemoved(entity, pos);
                            NotifyEntityCreated(snapshotEntity, pos);
                        }
                        else if (snapshotEntity.GetPorts() != entity.GetPorts())
                                NotifyEntityPortUpdated(snapshotEntity, pos);
                    }
                    else
                    {
                        if (snapshot.WorldGrid.TryGetEntityAt(x, y, out Entity snapshotEntity))
                            NotifyEntityCreated(snapshotEntity, pos);
                    }
                }
            }
            worldGrid = snapshot.WorldGrid.Clone();

            blueprints.Clear();
            foreach (EntityBlueprintStack blueprint in snapshot.Blueprints)
                blueprints.Add(blueprint.Clone());
        }

        /// <summary>
        /// 선과 연결된 모든 선 네트워크를 제거하고, 연결된 회로 요소의 포트 연결을 끊습니다.
        /// </summary>
        /// <param name="pos">네트워크에 속한 선의 위치</param>
        private bool TryRemoveWireNetwork(Vector2Int pos)
        {
            List<Vector2Int> wirePositions = CollectConnectedWires(pos);
            if (wirePositions == null)
                return false;

            // 네트워크에 연결된 회로 요소의 포트 끊기
            Vector2Int start = wirePositions.First();
            if (TryGetEntityAt(start.x, start.y, out Entity startWire))
            {
                Vector2Int? dir = WireHelper.GetWireInDir(startWire);
                if (dir != null)
                {
                    Vector2Int elementPos = (Vector2Int)dir + start;
                    if (TryGetEntityAt(elementPos.x, elementPos.y, out Entity element))
                    {
                        SetPort(element, -(Vector2Int)dir, Entity.PortType.None);
                        NotifyEntityPortUpdated(element, elementPos);
                    }
                }
            }

            Vector2Int end = wirePositions.Last();
            if (TryGetEntityAt(end.x, end.y, out Entity endWire))
            {
                Vector2Int? dir = WireHelper.GetWireOutDir(endWire);
                if (dir != null)
                {
                    Vector2Int elementPos = (Vector2Int)dir + end;
                    if (TryGetEntityAt(elementPos.x, elementPos.y, out Entity element))
                    {
                        SetPort(element, -(Vector2Int)dir, Entity.PortType.None);
                        NotifyEntityPortUpdated(element, elementPos);
                    }
                }
            }

            // 네트워크의 모든 선 제거
            foreach (Vector2Int wirePos in wirePositions)
            {
                if (!worldGrid.HasEntityAt(wirePos.x, wirePos.y))
                    continue;
                Entity wire = worldGrid.GetEntityAt(wirePos.x, wirePos.y);
                worldGrid.RemoveEntityAt(wirePos.x, wirePos.y);
                NotifyEntityRemoved(wire, wirePos);
            }

            return true;
        }

        /// <summary>
        /// 선과 연결된 모든 선 네트워크의 좌표를 순서에 맞춰 반환합니다.
        /// </summary>
        /// <param name="pos">네트워크에 속한 선의 위치</param>
        private List<Vector2Int> CollectConnectedWires(Vector2Int pos)
        {
            List<Vector2Int> wires = new();
            Vector2Int curr = pos;
            Vector2Int? next = null;

            while (true)
            {
                if (!TryGetEntityAt(curr.x, curr.y, out Entity entity))
                    return null;
                if (!WireHelper.IsWire(entity))
                {
                    if (next == null)
                        return null;
                    curr = (Vector2Int)next;
                    break;
                }
                Vector2Int? pre = WireHelper.GetWireIn(entity, curr);
                if (pre == null)
                    return null;
                next = curr;
                curr = (Vector2Int)pre;
            }

            while (true)
            {
                if (!TryGetEntityAt(curr.x, curr.y, out Entity entity))
                    return null;
                if (!WireHelper.IsWire(entity))
                    break;
                wires.Add(curr);
                next = WireHelper.GetWireOut(entity, curr);
                if (next == null)
                    return null;
                curr = (Vector2Int)next;
            }

            return wires;
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

        private void NotifyEntityPortUpdated(Entity entity, Vector2Int pos)
        {
            IEntityEventListener.EntityPortUpdatedEvent e = new IEntityEventListener.EntityPortUpdatedEvent(entity, pos);
            foreach (IEntityEventListener listener in listeners)
                listener.OnEntityPortUpdated(e);
        }

        private void NotifyGridUpdated()
        {
            List<EntityBlueprintStack> blueprintsClone = new();
            foreach (EntityBlueprintStack blueprint in blueprints)
                blueprintsClone.Add(blueprint.Clone());

            WorldGrid worldGridClone = worldGrid.Clone();
            IEntityEventListener.GridUpdatedEvent e = new IEntityEventListener.GridUpdatedEvent(worldGridClone, blueprintsClone);
            foreach (IEntityEventListener listener in listeners)
                listener.OnGridUpdated(e);

            GameEventBus.NotifyGridChanged(new GridUpdatedEvent(worldGridClone));
        }
    }
}