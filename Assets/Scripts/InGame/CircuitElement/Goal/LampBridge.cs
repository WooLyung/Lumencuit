using System.Collections.Generic;

namespace Lumencuit
{
    /// <summary>
    /// 램프와 전선의 역할을 동시에 수행하는 회로 요소입니다.
    /// </summary>
    public sealed class LampBridge : CircuitElement
    {
        private static CircuitElement instance;
        public override CircuitElementType Type => CircuitElementType.LampBridge;
        public override int TurbidityDelta => 0;
        public override int InSignalCount => 1;
        public override int OutSignalCount => 1;
        public override bool IsGoal => true;

        private LampBridge() { }

        public static CircuitElement Instance => instance ??= new LampBridge();

        public override QuantumSignal Flow(IReadOnlyList<QuantumSignal> inputs)
        {
            foreach (QuantumSignal input in inputs)
                return input;
            return QuantumSignal.Null;
        }
    }
}