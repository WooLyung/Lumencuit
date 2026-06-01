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
        public override bool RequiresOrderedInputs => false;


        private SplitGate() { }

        public static CircuitElement Instance => instance ??= new SplitGate();

        public override Signal Flow(IReadOnlyList<Signal> inputs)
        {
            foreach (Signal input in inputs)
                return input;
            return Signal.Black;
        }
    }
}