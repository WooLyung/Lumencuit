using System.Collections.Generic;

namespace Lumencuit
{
    /// <summary>
    /// 신호가 도달하는 목적지입니다.
    /// </summary>
    public sealed class Lamp : CircuitElement
    {
        private static CircuitElement instance;
        public override string Id => "Lamp";
        public override int TurbidityDelta => 0;
        public override int InSignalCount => 1;
        public override int OutSignalCount => 0;

        private Lamp() { }

        public static CircuitElement Instance => instance ??= new Lamp();

        public override Signal Flow(IReadOnlyList<Signal> inputs)
        {
            return Signal.Black;
        }
    }
}