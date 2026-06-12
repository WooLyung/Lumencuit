using System.Collections.Generic;

namespace Lumencuit
{
    /// <summary>
    /// 신호의 시작입니다. 싱글톤을 사용하지 않습니다.
    /// </summary>
    public sealed class Source : CircuitElement
    {
        private static CircuitElement instance;
        public override CircuitElementType Type => CircuitElementType.Source;
        public override int TurbidityDelta => 0;
        public override int InSignalCount => 0;
        public override int OutSignalCount => 1;

        public readonly QuantumSignal Signal;

        private Source(QuantumSignal signal) 
        {
            Signal = signal;
        }

        public static CircuitElement Create(QuantumSignal signal) => new Source(signal);

        public override QuantumSignal Flow(IReadOnlyList<QuantumSignal> inputs)
        {
            return Signal;
        }
    }
}