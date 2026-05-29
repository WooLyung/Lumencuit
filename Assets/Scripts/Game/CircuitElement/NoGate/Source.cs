using System.Collections.Generic;

namespace Lumencuit
{
    /// <summary>
    /// 신호의 시작입니다.
    /// </summary>
    public sealed class Source : CircuitElement
    {
        private static CircuitElement instance;
        public override string Id => "Source";
        public override int TurbidityDelta => 0;
        public override int InSignalCount => 0;
        public override int OutSignalCount => 1;

        private Source() { }

        public static CircuitElement Instance => instance ??= new Source();

        // 소스의 Flow는 호출되지 않습니다.
        public override Signal Flow(IReadOnlyList<Signal> inputs)
        {
            return Signal.Black;
        }
    }
}