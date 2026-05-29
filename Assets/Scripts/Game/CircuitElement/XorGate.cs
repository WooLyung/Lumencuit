using System.Collections.Generic;
using System.Linq;

namespace Lumencuit
{
    /// <summary>
    /// Xor 회로 요소입니다.
    /// </summary>
    public class XorGate : CircuitElement
    {
        public override string Id => "XorGate";
        public override int InSignalCount => 2;
        public override int OutSignalCount => 1;

        public override Signal Flow(IEnumerable<Signal> inputs)
        {
            Signal output = Signal.Black;
            foreach (Signal input in inputs)
                output ^= input;
            return output;
        }
    }
}