namespace Lumencuit
{
    /// <summary>
    /// 회로의 신호를 나타내는 구조체입니다.
    /// </summary>
    public struct Signal
    {
        private bool r, g, b;

        private Signal(bool r, bool g, bool b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }

        // 색상
        public static Signal Black = new(false, false, false);
        public static Signal Red = new(true, false, false);
        public static Signal Green = new(false, true, false);
        public static Signal Blue = new(false, false, true);
        public static Signal Yellow = new(true, true, false);
        public static Signal Cyan = new(false, true, true);
        public static Signal Magenta = new(true, false, true);
        public static Signal White = new(true, true, true);

        // 단항 연산자
        public static Signal operator ~(Signal a) => new(!a.r, !a.g, !a.b);

        // 이항 연산자
        public static Signal operator &(Signal a, Signal b) => new(a.r && b.r, a.g && b.g, a.b && b.b);
        public static Signal operator |(Signal a, Signal b) => new(a.r || b.r, a.g || b.g, a.b || b.b);
        public static Signal operator -(Signal a, Signal b) => new(a.r && !b.r, a.g && !b.g, a.b && !b.b);
        public static Signal operator ^(Signal a, Signal b) => new(a.r != b.r, a.g != b.g, a.b != b.b);
    }
}
