using System.Collections.Generic;
using System.Linq;

namespace Lumencuit
{
    /// <summary>
    /// And 회로 요소입니다.
    /// </summary>
    public sealed class AndGate : CircuitElement
    {
        private static CircuitElement instance;
        public override CircuitElementType Type => CircuitElementType.AndGate;
        public override int TurbidityDelta => 1;
        public override int InSignalCount => 2;
        public override int OutSignalCount => 1;
        public override bool RequiresOrderedInputs => false;

        private AndGate() { }

        public static CircuitElement Instance => instance ??= new AndGate();

        public override Signal Flow(IReadOnlyList<Signal> inputs)
        {
            if (inputs.Count() == 0)
                return Signal.Black;

            Signal output = Signal.White;
            foreach (Signal input in inputs)
                output &= input;
            return output;
        }
    }
}