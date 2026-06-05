using System.Collections.Generic;

namespace Lumencuit
{
    /// <summary>
    /// 신호를 전달하는 회로 요소입니다.
    /// </summary>
    public sealed class Wire : CircuitElement
    {
        private static CircuitElement instance;
        public override CircuitElementType Type => CircuitElementType.Wire;
        public override int TurbidityDelta => 0;
        public override int InSignalCount => 1;
        public override int OutSignalCount => 1;

        private Wire() { }

        public static CircuitElement Instance => instance ??= new Wire();

        public override QuantumSignal Flow(IReadOnlyList<QuantumSignal> inputs)
        {
            foreach (QuantumSignal input in inputs)
                return input;
            return QuantumSignal.Null;
        }
    }
}