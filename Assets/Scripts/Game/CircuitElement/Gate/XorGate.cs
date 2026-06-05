using System.Collections.Generic;

namespace Lumencuit
{
    /// <summary>
    /// Xor 회로 요소입니다.
    /// </summary>
    public sealed class XorGate : CircuitElement
    {
        private static CircuitElement instance;
        public override CircuitElementType Type => CircuitElementType.XorGate;
        public override int TurbidityDelta => 1;
        public override int InSignalCount => 2;
        public override int OutSignalCount => 1;

        private XorGate() { }

        public static CircuitElement Instance => instance ??= new XorGate();

        public override QuantumSignal Flow(IReadOnlyList<QuantumSignal> inputs)
        {
            QuantumSignal output = QuantumSignal.Black;
            foreach (QuantumSignal input in inputs)
                output ^= input;
            return output;
        }
    }
}