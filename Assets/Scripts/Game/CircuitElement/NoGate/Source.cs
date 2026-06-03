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
        public override bool RequiresOrderedInputs => false;

        public readonly Signal Signal;

        private Source(Signal signal) 
        {
            Signal = signal;
        }

        public static CircuitElement Create(Signal signal) => new Source(signal);

        public override Signal Flow(IReadOnlyList<Signal> inputs)
        {
            return Signal;
        }
    }
}