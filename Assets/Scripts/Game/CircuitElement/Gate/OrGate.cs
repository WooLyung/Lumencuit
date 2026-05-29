using System.Collections.Generic;

namespace Lumencuit
{
    /// <summary>
    /// Or 회로 요소입니다.
    /// </summary>
    public sealed class OrGate : CircuitElement
    {
        public override string Id => "OrGate";
        public override int TurbidityDelta => 1;
        public override int InSignalCount => 2;
        public override int OutSignalCount => 1;

        public override Signal Flow(IReadOnlyList<Signal> inputs)
        {
            Signal output = Signal.Black;
            foreach (Signal input in inputs)
                output |= input;
            return output;
        }
    }
}