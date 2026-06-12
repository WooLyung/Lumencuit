using UnityEngine;

namespace Lumencuit
{
    /// <summary>
    /// 시뮬레이션의 처리를 감지합니다.
    /// </summary>
    public interface ISimulationEventListener
    {
        public class SignalUpdatedEvent
        {
            public readonly Entity Entity;
            public readonly Vector2Int Pos;
            public readonly QuantumSignal Signal;

            public SignalUpdatedEvent(Entity entity, Vector2Int pos, QuantumSignal signal)
            {
                Entity = entity;
                Pos = pos;
                Signal = signal;
            }
        }

        public class PortSignalUpdatedEvent
        {
            public readonly Entity Entity;
            public readonly Vector2Int Dir;
            public readonly Vector2Int Pos;
            public readonly QuantumSignal Signal;

            public PortSignalUpdatedEvent(Entity entity, Vector2Int dir, Vector2Int pos, QuantumSignal signal)
            {
                Entity = entity;
                Dir = dir;
                Pos = pos;
                Signal = signal;
            }
        }

        public class CircuitResultEvent
        {
            public readonly CircuitResult Result;

            public CircuitResultEvent(CircuitResult result)
            {
                Result = result;
            }
        }

        /// <summary>
        /// 엔티티의 신호가 바뀌었을 때 호출됩니다.
        /// </summary>
        public void OnSignalUpdated(SignalUpdatedEvent e) { }

        /// <summary>
        /// 엔티티 포트의 신호가 바뀌었을 때 호출됩니다.
        /// </summary>
        public void OnPortSignalUpdated(PortSignalUpdatedEvent e) { }

        /// <summary>
        /// 회로 검사가 끝난 후 호출됩니다.
        /// </summary>
        public void OnCircuitResultEvent(CircuitResultEvent e) { }
    }
}
