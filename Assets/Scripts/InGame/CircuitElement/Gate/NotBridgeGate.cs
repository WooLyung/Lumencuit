using System.Collections.Generic;

namespace Lumencuit
{
    /// <summary>
    /// Not 브릿지 회로 요소입니다.
    /// </summary>
    public sealed class NotBridgeGate : CircuitElement
    {
        private static CircuitElement instance;
        public override CircuitElementType Type => CircuitElementType.NotBridgeGate;
        public override int TurbidityDelta => 1;
        public override int InSignalCount => 1;
        public override int OutSignalCount => 1;
        public override bool IsGoal => true;

        private NotBridgeGate() { }

        public static CircuitElement Instance => instance ??= new NotBridgeGate();

        public override QuantumSignal Flow(IReadOnlyList<QuantumSignal> inputs)
        {
            foreach (QuantumSignal input in inputs)
                return ~input;
            return QuantumSignal.Null;
        }
    }
}