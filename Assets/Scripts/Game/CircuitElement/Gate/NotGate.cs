using System.Collections.Generic;

namespace Lumencuit
{
    /// <summary>
    /// Not 회로 요소입니다.
    /// </summary>
    public sealed class NotGate : CircuitElement
    {
        private static CircuitElement instance;
        public override CircuitElementType Type => CircuitElementType.NotGate;
        public override int TurbidityDelta => 1;
        public override int InSignalCount => 1;
        public override int OutSignalCount => 1;

        private NotGate() { }

        public static CircuitElement Instance => instance ??= new NotGate();

        public override QuantumSignal Flow(IReadOnlyList<QuantumSignal> inputs)
        {
            foreach (QuantumSignal input in inputs)
                return ~input;
            return QuantumSignal.Null;
        }
    }
}