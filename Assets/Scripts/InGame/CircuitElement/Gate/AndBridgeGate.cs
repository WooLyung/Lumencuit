using System.Collections.Generic;
using System.Linq;

namespace Lumencuit
{
    /// <summary>
    /// And 브릿지 회로 요소입니다.
    /// </summary>
    public sealed class AndBridgeGate : CircuitElement
    {
        private static CircuitElement instance;
        public override CircuitElementType Type => CircuitElementType.AndBridgeGate;
        public override int TurbidityDelta => 1;
        public override int InSignalCount => 2;
        public override int OutSignalCount => 1;
        public override bool IsGoal => true;

        private AndBridgeGate() { }

        public static CircuitElement Instance => instance ??= new AndBridgeGate();

        public override QuantumSignal Flow(IReadOnlyList<QuantumSignal> inputs)
        {
            if (inputs.Count() == 0)
                return QuantumSignal.Null;
            QuantumSignal output = QuantumSignal.White;
            foreach (QuantumSignal input in inputs)
                output &= input;
            return output;
        }
    }
}