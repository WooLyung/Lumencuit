using System.Collections.Generic;

namespace Lumencuit
{
    /// <summary>
    /// 신호가 도달하는 목적지입니다.
    /// </summary>
    public sealed class Lamp : CircuitElement
    {
        private static CircuitElement instance;
        public override CircuitElementType Type => CircuitElementType.Lamp;
        public override int TurbidityDelta => 0;
        public override int InSignalCount => 1;
        public override int OutSignalCount => 0;
        public override bool RequiresOrderedInputs => false;

        private Lamp() { }

        public static CircuitElement Instance => instance ??= new Lamp();

        public override Signal Flow(IReadOnlyList<Signal> inputs)
        {
            foreach (Signal input in inputs)
                return input;
            return Signal.Black;
        }
    }
}