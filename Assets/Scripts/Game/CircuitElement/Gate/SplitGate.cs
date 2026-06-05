using System.Collections.Generic;

namespace Lumencuit
{
    /// <summary>
    /// 한 신호를 두 갈래로 나누는 게이트입니다.
    /// </summary>
    public sealed class SplitGate : CircuitElement
    {
        private static CircuitElement instance;
        public override CircuitElementType Type => CircuitElementType.SplitGate;
        public override int TurbidityDelta => 0;
        public override int InSignalCount => 1;
        public override int OutSignalCount => 2;

        private SplitGate() { }

        public static CircuitElement Instance => instance ??= new SplitGate();

        public override QuantumSignal Flow(IReadOnlyList<QuantumSignal> inputs)
        {
            foreach (QuantumSignal input in inputs)
                return input;
            return QuantumSignal.Null;
        }
    }
}