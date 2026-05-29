using System.Collections.Generic;

namespace Lumencuit
{
    /// <summary>
    /// 한 신호를 두 갈래로 나누는 게이트입니다.
    /// </summary>
    public sealed class SplitGate : CircuitElement
    {
        public override string Id => "SplitGate";
        public override int TurbidityDelta => 0;
        public override int InSignalCount => 1;
        public override int OutSignalCount => 1;

        public override Signal Flow(IReadOnlyList<Signal> inputs)
        {
            foreach (Signal input in inputs)
                return input;
            return Signal.Black;
        }
    }
}