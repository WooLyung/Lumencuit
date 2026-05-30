using System.Collections.Generic;
using static Lumencuit.CircuitElement;

namespace Lumencuit
{
    /// <summary>
    /// 게이트, 조명, 소스 등의 회로 요소의 연산을 정의합니다.
    /// 자식 클래스는 싱글톤 패턴으로 정의합니다.
    /// </summary>
    public abstract class CircuitElement
    {
        /// <summary>
        /// 회로 요소의 종류를 정의하는 열거형입니다.
        /// </summary>
        public enum CircuitElementType
        {
            Lamp, LampBridge, Source, Wire,
            AndGate, NotGate, OrGate, SplitGate, SubtractGate, XorGate
        }

        public abstract CircuitElementType Type { get; }

        /// <summary>
        /// 신호가 회로 요소를 지남에 따른 탁도 변화량입니다.
        /// </summary>
        public abstract int TurbidityDelta { get; }

        /// <summary>
        /// 회로 요소에 입력되는 신호의 수입니다.
        /// </summary>
        public abstract int InSignalCount { get; }

        /// <summary>
        /// 회로 요소에서 출력되는 신호의 수입니다.
        /// </summary>
        public abstract int OutSignalCount { get; }

        /// <summary>
        /// 현재 연결된 입력 신호를 기반으로 출력 신호를 계산합니다.
        /// </summary>
        /// <param name="inputs">
        /// 현재 연결된 입력 신호입니다.
        /// </param>
        /// <returns>
        /// 계산된 출력 신호입니다. 입력이 부족한 경우 Black 신호를 출력합니다.
        /// </returns>
        public abstract Signal Flow(IReadOnlyList<Signal> inputs);
    }

    public static class CircuitElementTypeFunction
    {
        public static CircuitElement ToElement(this CircuitElementType type)
        {
            return type switch
            {
                CircuitElementType.Lamp => Lamp.Instance,
                CircuitElementType.LampBridge => Lamp.Instance,
                CircuitElementType.Source => Lamp.Instance,
                CircuitElementType.Wire => Lamp.Instance,
                CircuitElementType.AndGate => Lamp.Instance,
                CircuitElementType.NotGate => Lamp.Instance,
                CircuitElementType.OrGate => Lamp.Instance,
                CircuitElementType.SplitGate => Lamp.Instance,
                CircuitElementType.SubtractGate => Lamp.Instance,
                CircuitElementType.XorGate => Lamp.Instance,
                _ => null
            };
        }
    }
}