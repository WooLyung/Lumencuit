using System.Collections.Generic;
using System.Linq;

namespace Lumencuit
{
    /// <summary>
    /// Subtract 회로 요소입니다.
    /// </summary>
    public class SubtractGate : CircuitElement
    {
        public override string Id => "SubtractGate";
        public override int InSignalCount => 2;
        public override int OutSignalCount => 1;

        public override Signal Flow(IEnumerable<Signal> inputs)
        {
            if (inputs.Count() == 0)
                return Signal.Black;
            if (inputs.Count() == 1)
                return inputs.First();

            return inputs.First() - inputs.Last();
        }
    }
}