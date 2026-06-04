using System.Collections.Generic;
using static Lumencuit.CircuitElement;

namespace Lumencuit
{
    /// <summary>
    /// 게이트, 조명, 소스 등의 회로 요소의 연산을 정의합니다.
    /// 소스를 제외한 자식 클래스는 싱글톤 패턴으로 정의합니다.
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
        /// 입력 포트의 순서를 지정합니다.
        /// </summary>
        public abstract bool RequiresOrderedInputs { get; }

        /// <summary>
        /// 최종 목표(램프)인지 지정합니다.
        /// </summary>
        public virtual bool IsGoal => false;

        /// <summary>
        /// 현재 연결된 입력 신호를 기반으로 출력 신호를 계산합니다.
        /// </summary>
        /// <param name="inputs">
        /// 현재 연결된 입력 신호입니다.
        /// </param>
        /// <returns>
        /// 계산된 출력 신호입니다. 입력이 부족한 경우 Null 신호를 출력합니다.
        /// </returns>
        public abstract QuantumSignal Flow(IReadOnlyList<QuantumSignal> inputs);
    }

    public static class CircuitElementTypeFunction
    {
        public static CircuitElement ToElement(this CircuitElementType type)
        {
            return type switch
            {
                CircuitElementType.Lamp => Lamp.Instance,
                CircuitElementType.LampBridge => LampBridge.Instance,
                CircuitElementType.Wire => Wire.Instance,
                CircuitElementType.AndGate => AndGate.Instance,
                CircuitElementType.NotGate => NotGate.Instance,
                CircuitElementType.OrGate => OrGate.Instance,
                CircuitElementType.SplitGate => SplitGate.Instance,
                CircuitElementType.SubtractGate => SubtractGate.Instance,
                CircuitElementType.XorGate => XorGate.Instance,
                _ => null
            };
        }
    }
}