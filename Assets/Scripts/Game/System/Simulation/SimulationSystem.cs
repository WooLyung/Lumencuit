using System.Collections.Generic;
using UnityEngine;

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
        private readonly List<ISignalEventListener> listeners = new();

        public SimulationSystem(WorldSystem worldSystem, StageData stageData)
        {
            worldSystem.AddListener(this);
            this.stageData = stageData;
        }

        public void AddListener(ISignalEventListener listener) => listeners.Add(listener);

        /// <summary>
        /// 그리드의 복사본을 이용해 전체 신호를 계산합니다.
        /// </summary>
        private void FlowAll(WorldGrid worldGrid)
        {
            SignalSet[,] signals = new SignalSet[worldGrid.Width, worldGrid.Height];
            int[,] remainedIn = new int[worldGrid.Width, worldGrid.Height];
            Queue<Vector2Int> queue = new();

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
                    if (worldGrid.HasEntityAt(x, y))
                    {
                        Entity entity = worldGrid.GetEntityAt(x, y);
                        remainedIn[x, y] = entity.InPortCount;
                    }
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
                    if (worldGrid.HasEntityAt(x, y))
                    {
                        SignalSet signalSet = signals[x, y];
                        Entity entity = worldGrid.GetEntityAt(x, y);
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
        }

        public void OnGridUpdated(IEntityEventListener.GridUpdatedEvent e)
        {
            FlowAll(e.WorldGridClone);
        }

        private void NotifySignalUpdated(Entity entity, Vector2Int pos)
        {
            ISignalEventListener.SignalUpdatedEvent e = new ISignalEventListener.SignalUpdatedEvent(entity, pos);
            foreach (ISignalEventListener listener in listeners)
                listener.OnSignalUpdated(e);
        }

        private void NotifyPortSignalUpdated(Entity entity, Vector2Int dir, Vector2Int pos, Signal signal)
        {
            ISignalEventListener.PortSignalUpdatedEvent e = new ISignalEventListener.PortSignalUpdatedEvent(entity, dir, pos, signal);
            foreach (ISignalEventListener listener in listeners)
                listener.OnPortSignalUpdated(e);
        }
    }
}