using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;
using static UnityEngine.EventSystems.EventTrigger;

namespace Lumencuit
{
    /// <summary>
    /// 게임의 핵심 로직을 시뮬레이션하는 시스템입니다.
    /// </summary>
    public sealed class SimulationSystem : IEntityEventListener
    {
        /// <summary>
        /// 시뮬레이션 결과가 저장된 그리드입니다.
        /// </summary>
        public sealed class SimulatedGrid
        {
            public readonly int Width;
            public readonly int Height;
            public readonly QuantumSignal[,] Signals;
            public readonly QuantumSignal[,] UpPorts;
            public readonly QuantumSignal[,] DownPorts;
            public readonly QuantumSignal[,] RightPorts;
            public readonly QuantumSignal[,] LeftPorts;
            public readonly int[,] Turbidities;

            public SimulatedGrid(int width, int height)
            {
                Width = width;
                Height = height;

                Signals = new QuantumSignal[Width, Height];
                UpPorts = new QuantumSignal[Width, Height];
                DownPorts = new QuantumSignal[Width, Height];
                RightPorts = new QuantumSignal[Width, Height];
                LeftPorts = new QuantumSignal[Width, Height];
                Turbidities = new int[Width, Height];

                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        Signals[x, y] = QuantumSignal.Null;
                        UpPorts[x, y] = QuantumSignal.Null;
                        DownPorts[x, y] = QuantumSignal.Null;
                        RightPorts[x, y] = QuantumSignal.Null;
                        LeftPorts[x, y] = QuantumSignal.Null;
                        Turbidities[x, y] = 0;
                    }
                }
            }

            public SimulatedGrid Clone()
            {
                SimulatedGrid clone = new SimulatedGrid(Width, Height);
                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        clone.Signals[x, y] = Signals[x, y];
                        clone.UpPorts[x, y] = UpPorts[x, y];
                        clone.DownPorts[x, y] = DownPorts[x, y];
                        clone.RightPorts[x, y] = RightPorts[x, y];
                        clone.LeftPorts[x, y] = LeftPorts[x, y];
                        clone.Turbidities[x, y] = Turbidities[x, y];
                    }
                }
                return clone;
            }
        }

        /// <summary>
        /// 신호 계산에 대한 결과입니다.
        /// </summary>
        public enum FlowResult
        {
            Success, HasCycle, CantReach, MultipleSignalGenerators
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
        // 1. SCC를 계산합니다.
        // 2. SCC의 신호 생성기 개수를 검사합니다.
        // 3. 위상 정렬로 신호를 계산합니다.
        // 4. 필요 입력이 충족된 SCC를 찾습니다.
        // 5. 신호 생성기로부터 8종의 신호 가능성을 검사합니다.
        // 6. (5)로부터 양자 신호를 생성하고 위상 정렬 탐색 대상으로 추가합니다.
        // 7. (3)을 반복합니다.
        private void FlowAll(WorldGrid worldGrid, out FlowResult result, out SimulatedGrid simulatedGrid)
        {
            simulatedGrid = new SimulatedGrid(worldGrid.Width, worldGrid.Height);

            int[,] remainedIn = new int[worldGrid.Width, worldGrid.Height];
            bool[,] calculated = new bool[worldGrid.Width, worldGrid.Height];
            Queue<Vector2Int> queue = new();
            result = FlowResult.Success;

            // SCC를 찾고 검사
            List<List<Vector2Int>> sccs0 = SCCHelper.FindSCCs(worldGrid);
            List<List<Vector2Int>> sccs = new();
            foreach (var scc in sccs0)
            {
                int count = SCCHelper.CountSignalGenerator(worldGrid, scc);
                if (count == 0)
                    result = FlowResult.HasCycle;
                else if (count >= 2)
                    result = FlowResult.MultipleSignalGenerators;
                else
                    sccs.Add(scc);
            }

            // 신호 계산 후 큐에 넣기
            void CalculateSignal(Vector2Int next, SimulatedGrid simulatedGrid)
            {
                Entity entity = worldGrid.GetEntityAt(next.x, next.y);
                if (entity == null)
                    return;

                List<QuantumSignal> inputs = new();
                int turbidity = 0;
                if (entity.UpPort == Entity.PortType.Input)
                {
                    inputs.Add(simulatedGrid.UpPorts[next.x, next.y]);
                    turbidity = Mathf.Max(turbidity, simulatedGrid.Turbidities[next.x, next.y + 1]);
                }
                if (entity.DownPort == Entity.PortType.Input)
                {
                    inputs.Add(simulatedGrid.DownPorts[next.x, next.y]);
                    turbidity = Mathf.Max(turbidity, simulatedGrid.Turbidities[next.x, next.y - 1]);
                }
                if (entity.RightPort == Entity.PortType.Input)
                {
                    inputs.Add(simulatedGrid.RightPorts[next.x, next.y]);
                    turbidity = Mathf.Max(turbidity, simulatedGrid.Turbidities[next.x + 1, next.y]);
                }
                if (entity.LeftPort == Entity.PortType.Input)
                {
                    inputs.Add(simulatedGrid.LeftPorts[next.x, next.y]);
                    turbidity = Mathf.Max(turbidity, simulatedGrid.Turbidities[next.x - 1, next.y]);
                }

                calculated[next.x, next.y] = true;
                simulatedGrid.Signals[next.x, next.y] = entity.Flow(inputs);
                simulatedGrid.Turbidities[next.x, next.y] = turbidity + entity.Element.TurbidityDelta;
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
                simulatedGrid.Signals[pos.x, pos.y] = source.Flow(new List<QuantumSignal>());
                queue.Enqueue(pos);
            }

            // 위상 정렬
            while (true)
            {
                // 신호 계산
                while (queue.Count > 0)
                {
                    Vector2Int front = queue.Dequeue();
                    Entity entity = worldGrid.GetEntityAt(front.x, front.y);
                    int turbidity = simulatedGrid.Turbidities[front.x, front.y];

                    if (entity.UpPort == Entity.PortType.Output)
                    {
                        Vector2Int next = front + Vector2Int.up;
                        simulatedGrid.UpPorts[front.x, front.y] = simulatedGrid.DownPorts[next.x, next.y] = simulatedGrid.Signals[front.x, front.y];
                        if (--remainedIn[next.x, next.y] == 0)
                            CalculateSignal(next, simulatedGrid);
                    }
                    if (entity.DownPort == Entity.PortType.Output)
                    {
                        Vector2Int next = front + Vector2Int.down;
                        simulatedGrid.DownPorts[front.x, front.y] = simulatedGrid.UpPorts[next.x, next.y] = simulatedGrid.Signals[front.x, front.y];
                        if (--remainedIn[next.x, next.y] == 0)
                            CalculateSignal(next, simulatedGrid);
                    }
                    if (entity.RightPort == Entity.PortType.Output)
                    {
                        Vector2Int next = front + Vector2Int.right;
                        simulatedGrid.RightPorts[front.x, front.y] = simulatedGrid.LeftPorts[next.x, next.y] = simulatedGrid.Signals[front.x, front.y];
                        if (--remainedIn[next.x, next.y] == 0)
                            CalculateSignal(next, simulatedGrid);
                    }
                    if (entity.LeftPort == Entity.PortType.Output)
                    {
                        Vector2Int next = front + Vector2Int.left;
                        simulatedGrid.LeftPorts[front.x, front.y] = simulatedGrid.RightPorts[next.x, next.y] = simulatedGrid.Signals[front.x, front.y];
                        if (--remainedIn[next.x, next.y] == 0)
                            CalculateSignal(next, simulatedGrid);
                    }
                }

                // 입력이 모두 충족된 SCC 계산
                HashSet<List<Vector2Int>> sccsSet = new(sccs);
                bool newNode = false;
                foreach (var scc in sccs)
                {
                    int count = SCCHelper.CountUnresolvedExternalInputs(worldGrid, calculated, scc);
                    if (count > 0)
                        continue;
                    sccsSet.Remove(scc);
                    newNode = true;

                    SCCHelper.TryGetSignalGenerator(worldGrid, scc, out Vector2Int generatorPos);
                    QuantumSignal generatorSignal = SCCHelper.CalculateSignalGeneratorSCC(worldGrid, simulatedGrid, remainedIn, scc);
                    calculated[generatorPos.x, generatorPos.y] = true;
                    simulatedGrid.Signals[generatorPos.x, generatorPos.y] = generatorSignal;
                    simulatedGrid.Turbidities[generatorPos.x, generatorPos.y] = 0;
                    queue.Enqueue(generatorPos);
                }
                sccs = new(sccsSet);

                if (!newNode)
                    break;
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

            if (result != FlowResult.Success)
                return;

            // 도달 가능성 검사
            for (int x = 0; x < worldGrid.Width; x++)
                for (int y = 0; y < worldGrid.Height; y++)
                    if (worldGrid.TryGetEntityAt(x, y, out Entity entity))
                        if (remainedIn[x, y] > 0)
                            result = FlowResult.CantReach;
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
            List<StageGoal> goalSlots = new();

            foreach (StageGoal goal in stageData.Goals)
                for (int i = 0; i < goal.Count; i++)
                    goalSlots.Add(goal);

            List<(QuantumSignal signal, int turbidity)> lamps = new();
            foreach (Vector2Int pos in worldGrid.GetAllGoalPositions())
                lamps.Add((simulatedGrid.Signals[pos.x, pos.y], simulatedGrid.Turbidities[pos.x, pos.y]));

            if (goalSlots.Count != lamps.Count)
                return CircuitResult.Fail;

            int[] matchedGoalByLamp = new int[lamps.Count];

            for (int i = 0; i < matchedGoalByLamp.Length; i++)
                matchedGoalByLamp[i] = -1;

            for (int goalIndex = 0; goalIndex < goalSlots.Count; goalIndex++)
            {
                bool[] visitedLamp = new bool[lamps.Count];
                if (!TryMatchGoal(goalIndex, goalSlots, lamps, matchedGoalByLamp, visitedLamp))
                    return CircuitResult.Fail;
            }

            return CircuitResult.Success;
        }

        /// <summary>
        /// 이분 매칭으로 램프와 스테이지 목표를 매칭합니다.
        /// </summary>
        /// <param name="goalIndex">현재 매칭을 시도할 목표</param>
        /// <param name="goals">목표 리스트</param>
        /// <param name="lamps">램프 리스트</param>
        /// <param name="matchedGoalByLamp">지금까지 매칭된 목표와 램프</param>
        /// <param name="visitedLamp">현재 연산에서 매칭을 시도했던 램프</param>
        /// <returns></returns>
        private static bool TryMatchGoal(int goalIndex, List<StageGoal> goals, List<(QuantumSignal signal, int turbidity)> lamps, int[] matchedGoalByLamp, bool[] visitedLamp)
        {
            StageGoal goal = goals[goalIndex];

            for (int lampIndex = 0; lampIndex < lamps.Count; lampIndex++)
            {
                if (visitedLamp[lampIndex])
                    continue;

                if (!goal.IsMatch(lamps[lampIndex].signal, lamps[lampIndex].turbidity))
                    continue;

                visitedLamp[lampIndex] = true;
                if (matchedGoalByLamp[lampIndex] == -1 || TryMatchGoal(matchedGoalByLamp[lampIndex], goals, lamps, matchedGoalByLamp, visitedLamp))
                {
                    matchedGoalByLamp[lampIndex] = goalIndex;
                    return true;
                }
            }

            return false;
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
            FlowAll(worldGrid, out FlowResult flowResult, out SimulatedGrid simulatedGrid);
            
            // 플로우 실패 처리
            switch (flowResult)
            {
                case FlowResult.CantReach:
                    AlertResult(CircuitResult.CantReach);
                    return;
                case FlowResult.HasCycle:
                    AlertResult(CircuitResult.HasCycle);
                    return;
                case FlowResult.MultipleSignalGenerators:
                    AlertResult(CircuitResult.MultipleSignalGenerators);
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

        private void NotifySignalUpdated(Entity entity, Vector2Int pos, QuantumSignal signal)
        {
            ISimulationEventListener.SignalUpdatedEvent e = new ISimulationEventListener.SignalUpdatedEvent(entity, pos, signal);
            foreach (ISimulationEventListener listener in listeners)
                listener.OnSignalUpdated(e);
        }

        private void NotifyPortSignalUpdated(Entity entity, Vector2Int dir, Vector2Int pos, QuantumSignal signal)
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