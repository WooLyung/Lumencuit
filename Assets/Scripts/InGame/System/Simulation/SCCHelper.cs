using System.Collections.Generic;
using UnityEngine;
using static Lumencuit.SimulationSystem;

namespace Lumencuit
{
    /// <summary>
    /// SCC 계산을 위한 헬퍼 클래스입니다.
    /// </summary>
    public static class SCCHelper
    {
        /// <summary>
        /// 그리드로부터 SCC를 계산하여 반환합니다.
        /// </summary>
        public static List<List<Vector2Int>> FindSCCs(WorldGrid worldGrid)
        {
            List<List<Vector2Int>> result = new();

            Dictionary<Vector2Int, int> indexMap = new();
            Dictionary<Vector2Int, int> lowLinkMap = new();
            Stack<Vector2Int> stack = new();
            HashSet<Vector2Int> onStack = new();

            int index = 0;
            foreach (Vector2Int pos in worldGrid.GetAllEntityPositions())
                if (!indexMap.ContainsKey(pos))
                    StrongConnect(pos);

            return result;

            // Tarjan SCC
            void StrongConnect(Vector2Int pos)
            {
                indexMap[pos] = index;
                lowLinkMap[pos] = index;
                index++;

                stack.Push(pos);
                onStack.Add(pos);

                foreach (Vector2Int next in GetOutputConnectedPositions(worldGrid, pos))
                {
                    if (!indexMap.ContainsKey(next))
                    {
                        StrongConnect(next);
                        lowLinkMap[pos] = Mathf.Min(lowLinkMap[pos], lowLinkMap[next]);
                    }
                    else if (onStack.Contains(next))
                        lowLinkMap[pos] = Mathf.Min(lowLinkMap[pos], indexMap[next]);
                }

                if (lowLinkMap[pos] != indexMap[pos])
                    return;

                List<Vector2Int> component = new();
                while (true)
                {
                    Vector2Int curr = stack.Pop();
                    onStack.Remove(curr);
                    component.Add(curr);

                    if (curr == pos)
                        break;
                }

                if (component.Count > 1)
                    result.Add(component);
            }
        }

        private static IEnumerable<Vector2Int> GetOutputConnectedPositions(WorldGrid worldGrid, Vector2Int pos)
        {
            Entity entity = worldGrid.GetEntityAt(pos.x, pos.y);
            if (entity.UpPort == Entity.PortType.Output) yield return pos + Vector2Int.up;
            if (entity.DownPort == Entity.PortType.Output) yield return pos + Vector2Int.down;
            if (entity.RightPort == Entity.PortType.Output) yield return pos + Vector2Int.right;
            if (entity.LeftPort == Entity.PortType.Output) yield return pos + Vector2Int.left;
        }

        /// <summary>
        /// SCC에 포함된 신호 생성기 개수를 구합니다.
        /// </summary>
        public static int CountSignalGenerator(WorldGrid worldGrid, List<Vector2Int> scc)
        {
            int count = 0;
            foreach (Vector2Int pos in scc)
            {
                Entity entity = worldGrid.GetEntityAt(pos.x, pos.y);
                if (entity.Element.Type == CircuitElement.CircuitElementType.SignalGenerator)
                    count++;
            }
            return count;
        }

        /// <summary>
        /// 아직 충족되지 않은 SCC 입력 개수를 구합니다.
        /// </summary>
        public static int CountUnresolvedExternalInputs(WorldGrid worldGrid, bool[,] calculated, List<Vector2Int> scc)
        {
            HashSet<Vector2Int> sccSet = new(scc);
            int count = 0;

            foreach (Vector2Int pos in scc)
            {
                Entity entity = worldGrid.GetEntityAt(pos.x, pos.y);
                Check(entity.UpPort, pos, Vector2Int.up);
                Check(entity.DownPort, pos, Vector2Int.down);
                Check(entity.RightPort, pos, Vector2Int.right);
                Check(entity.LeftPort, pos, Vector2Int.left);
            }

            return count;

            void Check(Entity.PortType portType, Vector2Int pos, Vector2Int dir)
            {
                if (portType != Entity.PortType.Input)
                    return;
                Vector2Int connectedPos = pos + dir;
                if (sccSet.Contains(connectedPos))
                    return;
                if (!calculated[connectedPos.x, connectedPos.y])
                    count++;
            }
        }

        /// <summary>
        /// SCC에 포함된 신호 생성기를 찾습니다.
        /// </summary>
        public static bool TryGetSignalGenerator(WorldGrid worldGrid, List<Vector2Int> scc, out Vector2Int generatorPos)
        {
            generatorPos = Vector2Int.zero;
            foreach (Vector2Int pos in scc)
            {
                Entity entity = worldGrid.GetEntityAt(pos.x, pos.y);
                if (entity.Element.Type != CircuitElement.CircuitElementType.SignalGenerator)
                    continue;
                generatorPos = pos;
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// 신호 생성기에서 생성될 양자 신호를 계산합니다.
        /// </summary>
        public static QuantumSignal CalculateSignalGeneratorSCC(WorldGrid worldGrid, SimulatedGrid simulatedGrid, int[,] remainedIn, List<Vector2Int> scc)
        {
            if (!TryGetSignalGenerator(worldGrid, scc, out Vector2Int generatorPos))
                return QuantumSignal.Null;

            byte resultMask = 0;
            for (byte i = 0; i < 8; i++)
            {
                Signal testSignal = Signal.FromValue(i);
                QuantumSignal testQuantumSignal = QuantumSignal.FromSignal(testSignal);
                QuantumSignal returnedSignal = SimulateSCCWithGeneratorSignal(worldGrid, simulatedGrid, remainedIn, generatorPos, testQuantumSignal);
                if (returnedSignal == testQuantumSignal)
                    resultMask |= (byte)(1 << i);
            }

            return new QuantumSignal(resultMask);
        }

        /// <summary>
        /// 신호 생성기에서 사용 가능한 신호를 시뮬레이션합니다.
        /// </summary>
        private static QuantumSignal SimulateSCCWithGeneratorSignal(WorldGrid worldGrid, SimulatedGrid simulatedGrid, int[,] remainedIn, Vector2Int generatorPos, QuantumSignal testSignal)
        {
            SimulatedGrid simulatedGridClone = simulatedGrid.Clone();
            int[,] remainedInClone = (int[,])remainedIn.Clone();
            Queue<Vector2Int> queue = new();

            Entity generator = worldGrid.GetEntityAt(generatorPos.x, generatorPos.y);
            simulatedGridClone.Signals[generatorPos.x, generatorPos.y] = testSignal;
            queue.Enqueue(generatorPos);

            while (queue.Count > 0)
            {
                Vector2Int front = queue.Dequeue();
                Entity entity = worldGrid.GetEntityAt(front.x, front.y);

                if (entity.UpPort == Entity.PortType.Output)
                {
                    Vector2Int next = front + Vector2Int.up;
                    simulatedGridClone.UpPorts[front.x, front.y] = simulatedGridClone.DownPorts[next.x, next.y] = simulatedGridClone.Signals[front.x, front.y];
                    if (--remainedInClone[next.x, next.y] == 0)
                        CalculateSignal(next);
                }
                if (entity.DownPort == Entity.PortType.Output)
                {
                    Vector2Int next = front + Vector2Int.down;
                    simulatedGridClone.DownPorts[front.x, front.y] = simulatedGridClone.UpPorts[next.x, next.y] = simulatedGridClone.Signals[front.x, front.y];
                    if (--remainedInClone[next.x, next.y] == 0)
                        CalculateSignal(next);
                }
                if (entity.RightPort == Entity.PortType.Output)
                {
                    Vector2Int next = front + Vector2Int.right;
                    simulatedGridClone.RightPorts[front.x, front.y] = simulatedGridClone.LeftPorts[next.x, next.y] = simulatedGridClone.Signals[front.x, front.y];
                    if (--remainedInClone[next.x, next.y] == 0)
                        CalculateSignal(next);
                }
                if (entity.LeftPort == Entity.PortType.Output)
                {
                    Vector2Int next = front + Vector2Int.left;
                    simulatedGridClone.LeftPorts[front.x, front.y] = simulatedGridClone.RightPorts[next.x, next.y] = simulatedGridClone.Signals[front.x, front.y];
                    if (--remainedInClone[next.x, next.y] == 0)
                        CalculateSignal(next);
                }
            }

            return GetGeneratorInputSignal(generator, simulatedGridClone, generatorPos);

            // 신호 계산 후 큐에 넣기
            void CalculateSignal(Vector2Int pos)
            {
                Entity entity = worldGrid.GetEntityAt(pos.x, pos.y);
                List<QuantumSignal> inputs = new();

                if (entity.UpPort == Entity.PortType.Input)
                    inputs.Add(simulatedGridClone.UpPorts[pos.x, pos.y]);
                if (entity.DownPort == Entity.PortType.Input)
                    inputs.Add(simulatedGridClone.DownPorts[pos.x, pos.y]);
                if (entity.RightPort == Entity.PortType.Input)
                    inputs.Add(simulatedGridClone.RightPorts[pos.x, pos.y]);
                if (entity.LeftPort == Entity.PortType.Input)
                    inputs.Add(simulatedGridClone.LeftPorts[pos.x, pos.y]);
                simulatedGridClone.Signals[pos.x, pos.y] = entity.Flow(inputs);
                queue.Enqueue(pos);
            }

            // 신호 생성기로 돌아온 신호를 구합니다.
            QuantumSignal GetGeneratorInputSignal(Entity generator, SimulatedGrid simulatedGrid, Vector2Int generatorPos)
            {
                if (generator.UpPort == Entity.PortType.Input)
                    return simulatedGrid.UpPorts[generatorPos.x, generatorPos.y];
                if (generator.DownPort == Entity.PortType.Input)
                    return simulatedGrid.DownPorts[generatorPos.x, generatorPos.y];
                if (generator.RightPort == Entity.PortType.Input)
                    return simulatedGrid.RightPorts[generatorPos.x, generatorPos.y];
                if (generator.LeftPort == Entity.PortType.Input)
                    return simulatedGrid.LeftPorts[generatorPos.x, generatorPos.y];
                return QuantumSignal.Null;
            }
        }
    }
}
