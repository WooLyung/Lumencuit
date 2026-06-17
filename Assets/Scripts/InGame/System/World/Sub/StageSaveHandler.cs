using Lumencuit.Save;
using System.Collections.Generic;
using UnityEngine;

namespace Lumencuit
{
    /// <summary>
    /// 월드 시스템의 데이터를 저장 및 불러옵니다.
    /// </summary>
    public sealed class StageSaveHandler : IEntityEventListener
    {
        private readonly WorldSystem worldSystem;
        private readonly StageData stageData;

        public StageSaveHandler(WorldSystem worldSystem, StageData stageData)
        {
            this.worldSystem = worldSystem;
            this.stageData = stageData;
        }

        public void OnGridUpdated(IEntityEventListener.GridUpdatedEvent e)
        {
            StageSaveData stageFileData = CreateStageFileData(e.WorldGridClone);
            SaveManagement.SetCurrentStage(stageFileData);
        }

        private StageSaveData CreateStageFileData(WorldGrid worldGrid)
        {
            StageSaveData saveData = new StageSaveData(stageData.StageId);

            for (int x = 0; x < worldGrid.Width; x++)
            {
                for (int y = 0; y < worldGrid.Height; y++)
                {
                    if (!worldGrid.TryGetEntityAt(x, y, out Entity entity))
                        continue;

                    saveData.AddEntity(new EntityFileDataV1
                    {
                        X = x,
                        Y = y,
                        BlueprintId = entity.MadeBy.Id,
                        SignalMask = entity.MadeBy is ColoredBlueprint coloredBlueprint ? coloredBlueprint.Signal.Mask : -1,
                        Ports = new PortFileDataV1
                        {
                            Left = (int)entity.LeftPort,
                            Right = (int)entity.RightPort,
                            Up = (int)entity.UpPort,
                            Down = (int)entity.DownPort
                        }
                    });
                }
            }

            return saveData;
        }

        /// <summary>
        /// 세이브 파일이 존재한다면 월드 시스템에 반영합니다.
        /// </summary> 
        public bool TryLoadStageData()
        {
            if (!SaveManagement.HasCurrentStage)
                return false;

            // 스테이지 데이터가 저장되어 있었다면 검증을 시작
            StageSaveData saveData = SaveManagement.CurrentStageData;

            if (saveData.StageId != stageData.StageId)
                return false;

            if (saveData.Entities == null)
                return false;

            // entityMap: 배치된 엔티티, fixedMap: 미리 배치되어 고정된 블루프린트
            Dictionary<Vector2Int, EntityFileDataV1> entityMap = new();
            Dictionary<Vector2Int, PrePlacedBlueprint> fixedMap = CreateFixedMap();

            // 배치 가능성 확인
            foreach (EntityFileDataV1 entityData in saveData.Entities)
            {
                Vector2Int pos = new Vector2Int(entityData.X, entityData.Y);

                if (!worldSystem.IsEnabledTile(pos.x, pos.y))
                    return false;

                if (entityData.Ports == null)
                    return false;

                if (!IsValidPortData(entityData.Ports))
                    return false;

                if (entityMap.ContainsKey(pos))
                    return false;

                entityMap.Add(pos, entityData);
            }

            // 모든 고정 블루프린트가 배치되었는지 확인
            if (!ValidateFixedEntities(entityMap, fixedMap))
                return false;

            // 고정 블루프린트를 제외하고 블루프린트 스택에 맞는지 확인
            if (!ValidateBlueprintStacks(entityMap, fixedMap))
                return false;

            // 포트 개수 및 연결성 확인
            if (!ValidatePorts(entityMap))
                return false;

            // 검증이 끝났다면 배치 시작
            foreach (KeyValuePair<Vector2Int, EntityFileDataV1> pair in entityMap)
            {
                Vector2Int pos = pair.Key;
                EntityFileDataV1 entityData = pair.Value;

                bool isFixed = fixedMap.ContainsKey(pos);

                if (!TryFindBlueprint(entityData, isFixed, fixedMap.TryGetValue(pos, out PrePlacedBlueprint fixedBlueprint) ? fixedBlueprint : null, out EntityBlueprint blueprint))
                    return false;

                EntityRequestResult result = worldSystem.TryPrePlaceEntity(blueprint, ToPorts(entityData.Ports), pos.x, pos.y, isFixed: isFixed);

                if (result != EntityRequestResult.Success)
                    return false;
            }

            return true;
        }

        private Dictionary<Vector2Int, PrePlacedBlueprint> CreateFixedMap()
        {
            Dictionary<Vector2Int, PrePlacedBlueprint> fixedMap = new();
            foreach (PrePlacedBlueprint ppb in stageData.PrePlacedBlueprints)
                fixedMap.Add(ppb.Position, ppb);
            return fixedMap;
        }

        private bool ValidateFixedEntities(Dictionary<Vector2Int, EntityFileDataV1> entityMap, Dictionary<Vector2Int, PrePlacedBlueprint> fixedMap)
        {
            foreach (KeyValuePair<Vector2Int, PrePlacedBlueprint> pair in fixedMap)
            {
                Vector2Int pos = pair.Key;
                PrePlacedBlueprint ppb = pair.Value;

                if (!entityMap.TryGetValue(pos, out EntityFileDataV1 entityData))
                    return false;

                if (entityData.BlueprintId != ppb.Blueprint.Id)
                    return false;

                if (entityData.SignalMask != GetSignalMask(ppb.Blueprint))
                    return false;

                if (!ContainsRequiredPorts(entityData.Ports, ppb.Ports))
                    return false;
            }

            return true;
        }

        private bool ValidateBlueprintStacks(Dictionary<Vector2Int, EntityFileDataV1> entityMap, Dictionary<Vector2Int, PrePlacedBlueprint> fixedMap)
        {
            Dictionary<(string, int), int> counts = new();

            foreach (EntityBlueprintStack stack in stageData.Blueprints)
            {
                var key = (stack.Blueprint.Id, GetSignalMask(stack.Blueprint));
                if (!counts.ContainsKey(key))
                    counts[key] = 0;
                counts[key] += stack.Count;
            }

            foreach (KeyValuePair<Vector2Int, EntityFileDataV1> pair in entityMap)
            {
                if (fixedMap.ContainsKey(pair.Key))
                    continue;

                EntityFileDataV1 entityData = pair.Value;
                var key = (entityData.BlueprintId, entityData.SignalMask);

                if (IsWireId(entityData.BlueprintId))
                    continue;

                if (!counts.ContainsKey(key))
                    return false;

                if (--counts[key] < 0)
                    return false;
            }

            return true;
        }

        private bool ValidatePorts(Dictionary<Vector2Int, EntityFileDataV1> entityMap)
        {
            foreach (KeyValuePair<Vector2Int, EntityFileDataV1> pair in entityMap)
            {
                Vector2Int pos = pair.Key;
                EntityFileDataV1 entityData = pair.Value;

                if (!TryFindBlueprint(entityData, false, null, out EntityBlueprint blueprint))
                {
                    if (!TryFindBlueprintFromFixedMap(entityData, pos, out blueprint))
                        return false;
                }

                Entity.Ports ports = ToPorts(entityData.Ports);
                Entity tempEntity = new Entity(blueprint.Clone(), ports);

                if (tempEntity.InPortCount > tempEntity.Element.InSignalCount)
                    return false;

                if (tempEntity.OutPortCount > tempEntity.Element.OutSignalCount)
                    return false;

                if (!ValidatePortConnection(entityMap, pos, entityData, Vector2Int.left, ports.Left))
                    return false;

                if (!ValidatePortConnection(entityMap, pos, entityData, Vector2Int.right, ports.Right))
                    return false;

                if (!ValidatePortConnection(entityMap, pos, entityData, Vector2Int.up, ports.Up))
                    return false;

                if (!ValidatePortConnection(entityMap, pos, entityData, Vector2Int.down, ports.Down))
                    return false;
            }

            return true;
        }

        private bool ValidatePortConnection(Dictionary<Vector2Int, EntityFileDataV1> entityMap, Vector2Int pos, EntityFileDataV1 entityData, Vector2Int dir, Entity.PortType portType)
        {
            if (portType == Entity.PortType.None)
                return true;

            Vector2Int otherPos = pos + dir;

            if (!entityMap.TryGetValue(otherPos, out EntityFileDataV1 otherEntityData))
                return false;

            Entity.PortType otherPort = GetPort(ToPorts(otherEntityData.Ports), -dir);

            if (portType == Entity.PortType.Input && otherPort != Entity.PortType.Output)
                return false;

            if (portType == Entity.PortType.Output && otherPort != Entity.PortType.Input)
                return false;

            bool currentIsWire = IsWireId(entityData.BlueprintId);
            bool otherIsWire = IsWireId(otherEntityData.BlueprintId);

            if (!currentIsWire && !otherIsWire)
                return false;

            return true;
        }

        private bool TryFindBlueprint(EntityFileDataV1 entityData, bool isFixed, PrePlacedBlueprint fixedBlueprint, out EntityBlueprint blueprint)
        {
            blueprint = null;

            if (IsWireId(entityData.BlueprintId))
            {
                if (entityData.SignalMask >= 0)
                    return false;

                blueprint = new EntityBlueprint(CircuitElement.CircuitElementType.Wire);
                return true;
            }

            if (isFixed)
            {
                if (fixedBlueprint == null)
                    return false;

                if (fixedBlueprint.Blueprint.Id != entityData.BlueprintId)
                    return false;

                if (GetSignalMask(fixedBlueprint.Blueprint) != entityData.SignalMask)
                    return false;

                blueprint = fixedBlueprint.Blueprint;
                return true;
            }

            foreach (EntityBlueprintStack stack in stageData.Blueprints)
            {
                if (stack.Blueprint.Id != entityData.BlueprintId)
                    continue;

                if (GetSignalMask(stack.Blueprint) != entityData.SignalMask)
                    continue;

                blueprint = stack.Blueprint;
                return true;
            }

            return false;
        }

        private bool TryFindBlueprintFromFixedMap(EntityFileDataV1 entityData, Vector2Int pos, out EntityBlueprint blueprint)
        {
            blueprint = null;

            foreach (PrePlacedBlueprint prePlaced in stageData.PrePlacedBlueprints)
            {
                if (prePlaced.Position != pos)
                    continue;

                if (prePlaced.Blueprint.Id != entityData.BlueprintId)
                    continue;

                if (GetSignalMask(prePlaced.Blueprint) != entityData.SignalMask)
                    continue;

                blueprint = prePlaced.Blueprint;
                return true;
            }

            return false;
        }

        private Entity.Ports ToPorts(PortFileDataV1 portData)
        {
            return new Entity.Ports
            {
                Left = (Entity.PortType)portData.Left,
                Right = (Entity.PortType)portData.Right,
                Up = (Entity.PortType)portData.Up,
                Down = (Entity.PortType)portData.Down
            };
        }

        private Entity.PortType GetPort(Entity.Ports ports, Vector2Int dir)
        {
            if (dir == Vector2Int.left)
                return ports.Left;

            if (dir == Vector2Int.right)
                return ports.Right;

            if (dir == Vector2Int.up)
                return ports.Up;

            if (dir == Vector2Int.down)
                return ports.Down;

            return Entity.PortType.None;
        }

        private bool ContainsRequiredPorts(PortFileDataV1 filePorts, Entity.Ports requiredPorts)
        {
            if (filePorts == null)
                return false;

            return ContainsRequiredPort(filePorts.Left, requiredPorts.Left) && ContainsRequiredPort(filePorts.Right, requiredPorts.Right) && ContainsRequiredPort(filePorts.Up, requiredPorts.Up) && ContainsRequiredPort(filePorts.Down, requiredPorts.Down);
        }

        private bool ContainsRequiredPort(int savedPortValue, Entity.PortType requiredPort)
        {
            Entity.PortType savedPort = (Entity.PortType)savedPortValue;
            if (requiredPort == Entity.PortType.None)
                return true;
            return savedPort == requiredPort;
        }

        private bool IsValidPortData(PortFileDataV1 portData)
        {
            return IsValidPortValue(portData.Left) && IsValidPortValue(portData.Right) && IsValidPortValue(portData.Up) && IsValidPortValue(portData.Down);
        }

        private bool IsValidPortValue(int value)
        {
            return value == (int)Entity.PortType.None || value == (int)Entity.PortType.Input || value == (int)Entity.PortType.Output;
        }

        private int GetSignalMask(EntityBlueprint blueprint)
        {
            if (blueprint is ColoredBlueprint coloredBlueprint)
                return coloredBlueprint.Signal.Mask;
            return -1;
        }

        private bool IsWireId(string blueprintId)
        {
            return blueprintId == CircuitElement.CircuitElementType.Wire.ToString();
        }
    }
}