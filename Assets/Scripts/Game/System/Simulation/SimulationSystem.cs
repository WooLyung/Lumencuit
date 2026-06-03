using System.Collections.Generic;
using UnityEngine;
using static Lumencuit.Signal;

namespace Lumencuit
{
    /// <summary>
    /// 게임의 핵심 로직을 시뮬레이션하는 시스템입니다.
    /// </summary>
    public sealed class SimulationSystem : IEntityEventListener
    {
        /// <summary>
        /// 모든 방향을 포함하는 신호 집합입니다.
        /// </summary>
        private class SignalSet
        {
            public Signal Center = Black;
            public Signal Left = Black;
            public Signal Right = Black;
            public Signal Up = Black;
            public Signal Down = Black;
        };

        /// <summary>
        /// 시뮬레이션 결과가 저장된 그리드입니다.
        /// </summary>
        private class SimulatedGrid
        {
            public readonly int Width;
            public readonly int Height;
            public readonly Signal[,] Signals;
            public readonly Signal[,] UpPorts;
            public readonly Signal[,] DownPorts;
            public readonly Signal[,] RightPorts;
            public readonly Signal[,] LeftPorts;

            public SimulatedGrid(int width, int height)
            {
                Width = width;
                Height = height;

                Signals = new Signal[Width, Height];
                UpPorts = new Signal[Width, Height];
                DownPorts = new Signal[Width, Height];
                RightPorts = new Signal[Width, Height];
                LeftPorts = new Signal[Width, Height];
                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        Signals[x, y] = Black;
                        UpPorts[x, y] = Black;
                        DownPorts[x, y] = Black;
                        RightPorts[x, y] = Black;
                        LeftPorts[x, y] = Black;
                    }
                }
            }
        }

        private readonly StageData stageData;
        private readonly List<ISimulationEventListener> listeners = new();

        public SimulationSystem(WorldSystem worldSystem, StageData stageData)
        {
            worldSystem.AddListener(this);
            this.stageData = stageData;
        }

        public void AddListener(ISimulationEventListener listener) => listeners.Add(listener);

        /// <summary>
        /// 그리드의 복사본을 이용해 전체 신호를 계산합니다.
        /// </summary>
        private void FlowAll(WorldGrid worldGrid, out bool cantReach, out SimulatedGrid simulatedGrid)
        {
            simulatedGrid = new SimulatedGrid(worldGrid.Width, worldGrid.Height);
            int[,] remainedIn = new int[worldGrid.Width, worldGrid.Height];
            Queue<Vector2Int> queue = new();
            cantReach = false;

            // 신호 계산 후 큐에 넣기
            void AddQueue(Vector2Int next, SimulatedGrid simulatedGrid)
            {
                Entity entity = worldGrid.GetEntityAt(next.x, next.y);
                if (entity == null)
                    return;

                List<Signal> inputs = new();
                if (entity.UpPort == Entity.PortType.Input)
                    inputs.Add(simulatedGrid.UpPorts[next.x, next.y]);
                if (entity.DownPort == Entity.PortType.Input)
                    inputs.Add(simulatedGrid.DownPorts[next.x, next.y]);
                if (entity.RightPort == Entity.PortType.Input)
                    inputs.Add(simulatedGrid.RightPorts[next.x, next.y]);
                if (entity.LeftPort == Entity.PortType.Input)
                    inputs.Add(simulatedGrid.LeftPorts[next.x, next.y]);

                simulatedGrid.Signals[next.x, next.y] = entity.Flow(inputs);
                queue.Enqueue(next);
            }

            // 초기화
            for (int x = 0; x < worldGrid.Width; x++)
                for (int y = 0; y < worldGrid.Height; y++)
                    if (worldGrid.TryGetEntityAt(x, y, out Entity entity))
                        remainedIn[x, y] = entity.InPortCount;

            // 모든 소스로부터 계산 시작
            foreach (Vector2Int pos in worldGrid.GetAllSourcePositions())
            {
                Entity source = worldGrid.GetEntityAt(pos.x, pos.y);
                simulatedGrid.Signals[pos.x, pos.y] = (source.Element as Source)?.Signal ?? Black;
                queue.Enqueue(pos);
            }

            // 위상 정렬
            while (queue.Count > 0)
            {
                Vector2Int front = queue.Dequeue();
                Entity entity = worldGrid.GetEntityAt(front.x, front.y);
                
                if (entity.UpPort == Entity.PortType.Output)
                {
                    Vector2Int next = front + Vector2Int.up;
                    simulatedGrid.UpPorts[front.x, front.y] = simulatedGrid.DownPorts[next.x, next.y] = simulatedGrid.Signals[front.x, front.y];
                    if (--remainedIn[next.x, next.y] == 0)
                        AddQueue(next, simulatedGrid);
                }
                if (entity.DownPort == Entity.PortType.Output)
                {
                    Vector2Int next = front + Vector2Int.down;
                    simulatedGrid.DownPorts[front.x, front.y] = simulatedGrid.UpPorts[next.x, next.y] = simulatedGrid.Signals[front.x, front.y];
                    if (--remainedIn[next.x, next.y] == 0)
                        AddQueue(next, simulatedGrid);
                }
                if (entity.RightPort == Entity.PortType.Output)
                {
                    Vector2Int next = front + Vector2Int.right;
                    simulatedGrid.RightPorts[front.x, front.y] = simulatedGrid.LeftPorts[next.x, next.y] = simulatedGrid.Signals[front.x, front.y];
                    if (--remainedIn[next.x, next.y] == 0)
                        AddQueue(next, simulatedGrid);
                }
                if (entity.LeftPort == Entity.PortType.Output)
                {
                    Vector2Int next = front + Vector2Int.left;
                    simulatedGrid.LeftPorts[front.x, front.y] = simulatedGrid.RightPorts[next.x, next.y] = simulatedGrid.Signals[front.x, front.y];
                    if (--remainedIn[next.x, next.y] == 0)
                        AddQueue(next, simulatedGrid);
                }
            }

            // 렌더링 적용
            for (int x = 0; x < worldGrid.Width; x++)
            {
                for (int y = 0; y < worldGrid.Height; y++)
                {
                    if (worldGrid.TryGetEntityAt(x, y, out Entity entity))
                    {
                        Vector2Int pos = new Vector2Int(x, y);

                        NotifySignalUpdated(entity, pos, simulatedGrid.Signals[x, y]);
                        if (entity.UpPort != Entity.PortType.None)
                            NotifyPortSignalUpdated(entity, Vector2Int.up, pos, simulatedGrid.UpPorts[x, y]);
                        if (entity.DownPort != Entity.PortType.None)
                            NotifyPortSignalUpdated(entity, Vector2Int.down, pos, simulatedGrid.DownPorts[x, y]);
                        if (entity.RightPort != Entity.PortType.None)
                            NotifyPortSignalUpdated(entity, Vector2Int.right, pos, simulatedGrid.RightPorts[x, y]);
                        if (entity.LeftPort != Entity.PortType.None)
                            NotifyPortSignalUpdated(entity, Vector2Int.left, pos, simulatedGrid.LeftPorts[x, y]);
                    }
                }
            }

            // 사이클 검사
            for (int x = 0; x < worldGrid.Width; x++)
            {
                for (int y = 0; y < worldGrid.Height; y++)
                {
                    if (worldGrid.TryGetEntityAt(x, y, out Entity entity))
                    {
                        if (remainedIn[x, y] != 0)
                        {
                            cantReach = true;
                            return;
                        }
                    }
                }
            }
        }

        private CircuitResult IsCircuitComplete(WorldGrid worldGrid, List<EntityBlueprintStack> blueprints)
        {
            // 설치되지 않은 블루프린트
            foreach (EntityBlueprintStack blueprint in blueprints)
                if (blueprint.Count > 0)
                    return CircuitResult.UnplacedBlueprint;

            // 연결되지 않은 포트
            for (int x = 0; x < worldGrid.Width; x++)
                for (int y = 0; y < worldGrid.Height; y++)
                    if (worldGrid.TryGetEntityAt(x, y, out Entity entity))
                        if (entity.Element.InSignalCount != entity.InPortCount || entity.Element.OutSignalCount != entity.OutPortCount)
                            return CircuitResult.IncompleteCircuit;

            return CircuitResult.Success;
        }

        private CircuitResult CheckClearStage(WorldGrid worldGrid, SimulatedGrid simulatedGrid)
        {
            Dictionary<SignalColor, int> goalCounts = new();
            foreach (StageData.StageGoal goal in stageData.Goals)
                goalCounts[goal.SignalColor] = goal.Count;

            foreach (Vector2Int pos in worldGrid.GetAllGoalPositions())
            {
                SignalColor signal = simulatedGrid.Signals[pos.x, pos.y].Color;
                if (!goalCounts.TryGetValue(signal, out int count) || count <= 0)
                    return CircuitResult.Fail;
                goalCounts[signal]--;
            }

            foreach (int count in goalCounts.Values)
                if (count != 0)
                    return CircuitResult.Fail;

            return CircuitResult.Success;
        }

        public void OnGridUpdated(IEntityEventListener.GridUpdatedEvent e)
        {
            void AlertResult(CircuitResult result)
            {
                NotifyCircuitResult(result);
            }

            // worldGrid는 복사본으로, 기존 월드 시스템에 영향을 주지 않습니다.
            WorldGrid worldGrid = e.WorldGridClone;
            List<EntityBlueprintStack> blueprints = e.BlueprintsClone;

            // 그리드 전체 신호 계산
            FlowAll(worldGrid, out bool cantReach, out SimulatedGrid simulatedGrid);
            
            // 사이클 혹은 도달 가능성 검사
            if (cantReach)
            {
                AlertResult(CircuitResult.CantReach);
                return;
            }

            // 회로 완성 검사
            CircuitResult result = IsCircuitComplete(worldGrid, blueprints);
            if (result != CircuitResult.Success)
            {
                AlertResult(result);
                return;
            }

            // 목표 달성 검사
            result = CheckClearStage(worldGrid, simulatedGrid);
            if (result != CircuitResult.Success)
            {
                AlertResult(result);
                return;
            }

            AlertResult(result);
        }

        private void NotifySignalUpdated(Entity entity, Vector2Int pos, Signal signal)
        {
            ISimulationEventListener.SignalUpdatedEvent e = new ISimulationEventListener.SignalUpdatedEvent(entity, pos, signal);
            foreach (ISimulationEventListener listener in listeners)
                listener.OnSignalUpdated(e);
        }

        private void NotifyPortSignalUpdated(Entity entity, Vector2Int dir, Vector2Int pos, Signal signal)
        {
            ISimulationEventListener.PortSignalUpdatedEvent e = new ISimulationEventListener.PortSignalUpdatedEvent(entity, dir, pos, signal);
            foreach (ISimulationEventListener listener in listeners)
                listener.OnPortSignalUpdated(e);
        }

        private void NotifyCircuitResult(CircuitResult result)
        {
            ISimulationEventListener.CircuitResultEvent e = new ISimulationEventListener.CircuitResultEvent(result);
            foreach (ISimulationEventListener listener in listeners)
                listener.OnCircuitResultEvent(e);
        }
    }
}