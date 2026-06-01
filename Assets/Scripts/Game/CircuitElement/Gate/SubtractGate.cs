using System.Collections.Generic;
using System.Linq;

namespace Lumencuit
{
    /// <summary>
    /// Subtract 회로 요소입니다.
    /// </summary>
    public sealed class SubtractGate : CircuitElement
    {
        private static CircuitElement instance;
        public override CircuitElementType Type => CircuitElementType.SubtractGate;
        public override int TurbidityDelta => 1;
        public override int InSignalCount => 2;
        public override int OutSignalCount => 1;
        public override bool RequiresOrderedInputs => true;

        private SubtractGate() { }

        public static CircuitElement Instance => instance ??= new SubtractGate();

        public override Signal Flow(IReadOnlyList<Signal> inputs)
        {
            if (inputs.Count() == 0)
                return Signal.Black;
            if (inputs.Count() == 1)
                return inputs.First();

            return inputs.First() - inputs.Last();
        }
    }
}