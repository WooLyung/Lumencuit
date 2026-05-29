using System.Collections.Generic;

namespace Lumencuit
{
    /// <summary>
    /// 신호를 전달하는 회로 요소입니다.
    /// </summary>
    public class Wire : CircuitElement
    {
        public override string Id => "Wire";
        public override int InSignalCount => 1;
        public override int OutSignalCount => 1;

        public override Signal Flow(IEnumerable<Signal> inputs)
        {
            foreach (Signal input in inputs)
                return input;
            return Signal.Black;
        }
    }
}