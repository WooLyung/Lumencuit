using System.Collections.Generic;

namespace Lumencuit
{
    /// <summary>
    /// SCC로부터 양자 신호를 생성합니다.
    /// </summary>
    public sealed class SignalGenerator : CircuitElement
    {
        private static CircuitElement instance;
        public override CircuitElementType Type => CircuitElementType.SignalGenerator;
        public override int TurbidityDelta => 0;
        public override int InSignalCount => 1;
        public override int OutSignalCount => 1;

        private SignalGenerator() { }

        public static CircuitElement Instance => instance ??= new SignalGenerator();

        public override QuantumSignal Flow(IReadOnlyList<QuantumSignal> inputs)
        {
            foreach (QuantumSignal input in inputs)
                return input;
            return QuantumSignal.Null;
        }
    }
}