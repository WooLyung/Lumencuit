using System.Collections;
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
        private class SignalSet {
            public Signal Center = Signal.Black;
            public Signal Left = Signal.Black;
            public Signal Right = Signal.Black;
            public Signal Up = Signal.Black;
            public Signal Down = Signal.Black;
        };


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
        private void FlowAll(WorldGrid worldGrid, out bool cantReach)
        {
            SignalSet[,] signals = new SignalSet[worldGrid.Width, worldGrid.Height];
            int[,] remainedIn = new int[worldGrid.Width, worldGrid.Height];
            Queue<Vector2Int> queue = new();
            cantReach = false;

            // 신호 계산 후 큐에 넣기
            void AddQueue(Vector2Int next)
            {
                Entity entity = worldGrid.GetEntityAt(next.x, next.y);
                if (entity == null)
                    return;

                List<Signal> inputs = new();
                if (entity.UpPort == Entity.PortType.Input)
                    inputs.Add(signals[next.x, next.y].Up);
                if (entity.DownPort == Entity.PortType.Input)
                    inputs.Add(signals[next.x, next.y].Down);
                if (entity.RightPort == Entity.PortType.Input)
                    inputs.Add(signals[next.x, next.y].Right);
                if (entity.LeftPort == Entity.PortType.Input)
                    inputs.Add(signals[next.x, next.y].Left);

                signals[next.x, next.y].Center = entity.Flow(inputs);
                queue.Enqueue(next);
            }

            // 초기화
            for (int x = 0; x < worldGrid.Width; x++)
            {
                for (int y = 0; y < worldGrid.Height; y++)
                {
                    signals[x, y] = new SignalSet();
                    if (worldGrid.TryGetEntityAt(x, y, out Entity entity))
                        remainedIn[x, y] = entity.InPortCount;
                }
            }

            // 모든 소스로부터 계산 시작
            foreach (Vector2Int pos in worldGrid.GetAllSourcePositions())
            {
                Entity source = worldGrid.GetEntityAt(pos.x, pos.y);
                signals[pos.x, pos.y].Center = source.CurrSignal;
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
                    signals[front.x, front.y].Up = signals[next.x, next.y].Down = signals[front.x, front.y].Center;
                    if (--remainedIn[next.x, next.y] == 0)
                        AddQueue(next);
                }
                if (entity.DownPort == Entity.PortType.Output)
                {
                    Vector2Int next = front + Vector2Int.down;
                    signals[front.x, front.y].Down = signals[next.x, next.y].Up = signals[front.x, front.y].Center;
                    if (--remainedIn[next.x, next.y] == 0)
                        AddQueue(next);
                }
                if (entity.RightPort == Entity.PortType.Output)
                {
                    Vector2Int next = front + Vector2Int.right;
                    signals[front.x, front.y].Right = signals[next.x, next.y].Left = signals[front.x, front.y].Center;
                    if (--remainedIn[next.x, next.y] == 0)
                        AddQueue(next);
                }
                if (entity.LeftPort == Entity.PortType.Output)
                {
                    Vector2Int next = front + Vector2Int.left;
                    signals[front.x, front.y].Left = signals[next.x, next.y].Right = signals[front.x, front.y].Center;
                    if (--remainedIn[next.x, next.y] == 0)
                        AddQueue(next);
                }
            }

            // 렌더링 적용
            for (int x = 0; x < worldGrid.Width; x++)
            {
                for (int y = 0; y < worldGrid.Height; y++)
                {
                    if (worldGrid.TryGetEntityAt(x, y, out Entity entity))
                    {
                        SignalSet signalSet = signals[x, y];
                        Vector2Int pos = new Vector2Int(x, y);

                        NotifySignalUpdated(entity, pos);
                        if (entity.UpPort != Entity.PortType.None)
                            NotifyPortSignalUpdated(entity, Vector2Int.up, pos, signalSet.Up);
                        if (entity.DownPort != Entity.PortType.None)
                            NotifyPortSignalUpdated(entity, Vector2Int.down, pos, signalSet.Down);
                        if (entity.RightPort != Entity.PortType.None)
                            NotifyPortSignalUpdated(entity, Vector2Int.right, pos, signalSet.Right);
                        if (entity.LeftPort != Entity.PortType.None)
                            NotifyPortSignalUpdated(entity, Vector2Int.left, pos, signalSet.Left);
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

        private CircuitResult IsCircuitComplete(WorldGrid worldGrid)
        {
            for (int x = 0; x < worldGrid.Width; x++)
                for (int y = 0; y < worldGrid.Height; y++)
                    if (worldGrid.TryGetEntityAt(x, y, out Entity entity))
                        if (entity.Element.InSignalCount != entity.InPortCount || entity.Element.OutSignalCount != entity.OutPortCount)
                            return CircuitResult.Success;
            return CircuitResult.Success;
        }

        private CircuitResult CheckClearStage(WorldGrid worldGrid)
        {
            Dictionary<SignalColor, int> goalCounts = new();
            foreach (StageData.StageGoal goal in stageData.Goals)
                goalCounts[goal.SignalColor] = goal.Count;

            foreach (Vector2Int pos in worldGrid.GetAllGoalPositions())
            {
                Entity goal = worldGrid.GetEntityAt(pos.x, pos.y);
                if (!goalCounts.TryGetValue(goal.CurrSignal.Color, out int count) || count <= 0)
                    return CircuitResult.Fail;
                goalCounts[goal.CurrSignal.Color]--;
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

            // 그리드 전체 신호 계산
            FlowAll(worldGrid, out bool cantReach);
            
            // 사이클 혹은 도달 가능성 검사
            if (cantReach)
            {
                AlertResult(CircuitResult.CantReach);
                return;
            }

            // 회로 완성 검사
            CircuitResult result = IsCircuitComplete(worldGrid);
            if (result != CircuitResult.Success)
            {
                AlertResult(result);
                return;
            }

            // 목표 달성 검사
            result = CheckClearStage(worldGrid);
            if (result != CircuitResult.Success)
            {
                AlertResult(result);
                return;
            }

            AlertResult(result);
        }

        private void NotifySignalUpdated(Entity entity, Vector2Int pos)
        {
            ISimulationEventListener.SignalUpdatedEvent e = new ISimulationEventListener.SignalUpdatedEvent(entity, pos);
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