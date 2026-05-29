using System.Collections.Generic;
using System.Linq;

namespace Lumencuit
{
    /// <summary>
    /// Not 회로 요소입니다.
    /// </summary>
    public class NotGate : CircuitElement
    {
        public override string Id => "NotGate";
        public override int InSignalCount => 1;
        public override int OutSignalCount => 1;

        public override Signal Flow(IEnumerable<Signal> inputs)
        {
            foreach (Signal input in inputs)
                return ~input;
            return Signal.Black;
        }
    }
}