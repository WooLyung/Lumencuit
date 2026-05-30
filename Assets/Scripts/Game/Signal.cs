using static Lumencuit.Signal;

namespace Lumencuit
{
    /// <summary>
    /// 회로의 신호를 나타내는 구조체입니다.
    /// </summary>
    public readonly struct Signal
    {
        /// <summary>
        /// 신호의 색을 나타내는 열거형입니다.
        /// </summary>
        public enum SignalColor
        {
            Red, Green, Blue, Yellow, Cyan, Magenta, Black, White
        }

        private readonly bool r, g, b;

        private Signal(bool r, bool g, bool b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }

        // 색상
        public static readonly Signal Black = new(false, false, false);
        public static readonly Signal Red = new(true, false, false);
        public static readonly Signal Green = new(false, true, false);
        public static readonly Signal Blue = new(false, false, true);
        public static readonly Signal Yellow = new(true, true, false);
        public static readonly Signal Cyan = new(false, true, true);
        public static readonly Signal Magenta = new(true, false, true);
        public static readonly Signal White = new(true, true, true);

        // 단항 연산자
        public static Signal operator ~(Signal a) => new(!a.r, !a.g, !a.b);

        // 이항 연산자
        public static Signal operator &(Signal a, Signal b) => new(a.r && b.r, a.g && b.g, a.b && b.b);
        public static Signal operator |(Signal a, Signal b) => new(a.r || b.r, a.g || b.g, a.b || b.b);
        public static Signal operator -(Signal a, Signal b) => new(a.r && !b.r, a.g && !b.g, a.b && !b.b);
        public static Signal operator ^(Signal a, Signal b) => new(a.r != b.r, a.g != b.g, a.b != b.b);

        public SignalColor Color
        {
            get
            {
                if (r && !g && !b)
                    return SignalColor.Red;
                if (!r && g && !b)
                    return SignalColor.Green;
                if (!r && !g && b)
                    return SignalColor.Blue;
                if (r && g && !b)
                    return SignalColor.Yellow;
                if (!r && g && b)
                    return SignalColor.Cyan;
                if (r && !g && b)
                    return SignalColor.Magenta;
                if (r && g && b)
                    return SignalColor.White;
                return SignalColor.Black;
            }
        }
    }

    public static class SignalColorFunction
    {
        public static Signal ToSignal(this SignalColor color)
        {
            return color switch
            {
                SignalColor.Black => Black,
                SignalColor.Red => Red,
                SignalColor.Green => Green,
                SignalColor.Blue => Blue,
                SignalColor.Yellow => Yellow,
                SignalColor.Cyan => Cyan,
                SignalColor.Magenta => Magenta,
                SignalColor.White => White,
                _ => Black
            };
        }
    }
}