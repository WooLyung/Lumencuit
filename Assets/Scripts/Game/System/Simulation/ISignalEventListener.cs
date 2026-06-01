using UnityEngine;
using static Lumencuit.IEntityEventListener;

namespace Lumencuit
{
    /// <summary>
    /// 엔티티의 신호 변화를 감지합니다.
    /// </summary>
    public interface ISignalEventListener
    {
        public class SignalUpdatedEvent
        {
            public readonly Entity Entity;
            public readonly Vector2Int Pos;

            public SignalUpdatedEvent(Entity entity, Vector2Int pos)
            {
                Entity = entity;
                Pos = pos;
            }
        }

        public class PortSignalUpdatedEvent
        {
            public readonly Entity Entity;
            public readonly Vector2Int Dir;
            public readonly Vector2Int Pos;
            public readonly Signal Signal;

            public PortSignalUpdatedEvent(Entity entity, Vector2Int dir, Vector2Int pos, Signal signal)
            {
                Entity = entity;
                Dir = dir;
                Pos = pos;
                Signal = signal;
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
    }
}
