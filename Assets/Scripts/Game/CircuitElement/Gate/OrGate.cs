using System.Collections.Generic;
using System.Linq;

namespace Lumencuit
{
    /// <summary>
    /// Or 회로 요소입니다.
    /// </summary>
    public sealed class OrGate : CircuitElement
    {
        private static CircuitElement instance;
        public override CircuitElementType Type => CircuitElementType.OrGate;
        public override int TurbidityDelta => 1;
        public override int InSignalCount => 2;
        public override int OutSignalCount => 1;

        private OrGate() { }

        public static CircuitElement Instance => instance ??= new OrGate();

        public override QuantumSignal Flow(IReadOnlyList<QuantumSignal> inputs)
        {
            if (inputs.Count() == 0)
                return QuantumSignal.Null;
            QuantumSignal output = QuantumSignal.Black;
            foreach (QuantumSignal input in inputs)
                output |= input;
            return output;
        }
    }
}