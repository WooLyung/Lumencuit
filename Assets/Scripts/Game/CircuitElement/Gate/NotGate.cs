using System.Collections.Generic;

namespace Lumencuit
{
    /// <summary>
    /// Not 회로 요소입니다.
    /// </summary>
    public sealed class NotGate : CircuitElement
    {
        private static CircuitElement instance;
        public override string Id => "NotGate";
        public override int TurbidityDelta => 1;
        public override int InSignalCount => 1;
        public override int OutSignalCount => 1;

        private NotGate() { }

        public static CircuitElement Instance => instance ??= new NotGate();

        public override Signal Flow(IReadOnlyList<Signal> inputs)
        {
            foreach (Signal input in inputs)
                return ~input;
            return Signal.Black;
        }
    }
}