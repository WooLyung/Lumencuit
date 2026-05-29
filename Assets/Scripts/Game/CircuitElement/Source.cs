using System.Collections.Generic;

namespace Lumencuit
{
    /// <summary>
    /// 신호의 시작입니다.
    /// </summary>
    public sealed class Source : CircuitElement
    {
        public override string Id => "Source";
        public override int TurbidityDelta => 0;
        public override int InSignalCount => 0;
        public override int OutSignalCount => 1;

        // 소스의 Flow는 호출되지 않습니다.
        public override Signal Flow(IReadOnlyList<Signal> inputs)
        {
            return Signal.Black;
        }
    }
}