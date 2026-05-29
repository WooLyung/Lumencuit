using System.Collections.Generic;

namespace Lumencuit
{
    /// <summary>
    /// 그리드에 설치된 회로 요소를 나타내는 객체입니다.
    /// </summary>
    public sealed class Entity
    {
        /// <summary>
        /// 회로 요소의 각 면의 입출력 여부를 나타냅니다.
        /// </summary>
        public enum Side { Input, Output, None };

        /// <summary>
        /// 회로 요소의 네 면의 입출력 여부를 나타냅니다.
        /// </summary>
        public struct Sides
        {
            public Side Left, Right, Up, Down;

            public Sides(Side left, Side right, Side up, Side down)
            {
                Left = left;
                Right = right;
                Up = up;
                Down = down;
            }

            public static readonly Sides None = new(Side.None, Side.None, Side.None, Side.None);
        }

        private readonly CircuitElement element;
        private Signal signal = Signal.Black;
        private Sides sides = Sides.None;

        public Entity(CircuitElement element)
        {
            this.element = element;
        }

        public Entity(CircuitElement element, Signal signal)
        {
            this.element = element;
            this.signal = signal;
        }

        public Entity(CircuitElement element, Signal signal, Sides sides)
        {
            this.element = element;
            this.signal = signal;
            this.sides = sides;
        }

        public Side LeftIO { get => sides.Left; set => sides.Left = value; }
        public Side RightIO { get => sides.Right; set => sides.Right = value; }
        public Side UpIO { get => sides.Up; set => sides.Up = value; }
        public Side DownIO { get => sides.Down; set => sides.Down = value; }
        public Signal CurrSignal => signal;

        public Signal Flow(IReadOnlyList<Signal> inputs)
        {
            return signal = element.Flow(inputs);
        }
    }
}