using System.Collections;
using System.Collections.Generic;

namespace Lumencuit
{
    /// <summary>
    /// 게이트와 선, 조명을 포함하는 추상 클래스입니다.
    /// </summary>
    public abstract class CircuitElement
    {
        /// <summary>
        /// 신호 전달 방향을 나타냅니다.
        /// </summary>
        public enum Direction { Left, Right, Up, Down };

        /// <summary>
        /// 내부적으로 사용되는 식별자입니다.
        /// </summary>
        public abstract string Id { get; }

        /// <summary>
        /// 회로 요소에 입력되는 신호의 수입니다.
        /// </summary>
        public abstract int InSignalCount { get; }

        /// <summary>
        /// 회로 요소에서 출력되는 신호의 수입니다.
        /// </summary>
        public abstract int OutSignalCount { get; }

        /// <summary>
        /// 회로 요소의 입력 기반으로 계산된 출력입니다.
        /// </summary>
        public abstract Signal Flow(IEnumerable<Signal> inputs);
    }
}