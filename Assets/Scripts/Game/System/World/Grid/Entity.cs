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
        public enum PortType { Input, Output, None };

        /// <summary>
        /// 회로 요소의 네 면의 입출력 여부를 나타냅니다.
        /// </summary>
        public struct Ports
        {
            public PortType Left, Right, Up, Down;

            public Ports(PortType left, PortType right, PortType up, PortType down)
            {
                Left = left;
                Right = right;
                Up = up;
                Down = down;
            }

            public static readonly Ports None = new(PortType.None, PortType.None, PortType.None, PortType.None);
        }

        public readonly CircuitElement Element;
        private Signal signal = Signal.Black;
        private Ports sides = Ports.None;

        public Entity(CircuitElement element)
        {
            Element = element;
        }

        public Entity(CircuitElement element, Signal signal)
        {
            Element = element;
            this.signal = signal;
        }

        public Entity(CircuitElement element, Signal signal, Ports sides)
        {
            Element = element;
            this.signal = signal;
            this.sides = sides;
        }

        public PortType LeftPort { get => sides.Left; set => sides.Left = value; }
        public PortType RightPort { get => sides.Right; set => sides.Right = value; }
        public PortType UpPort { get => sides.Up; set => sides.Up = value; }
        public PortType DownPort { get => sides.Down; set => sides.Down = value; }
        public Signal CurrSignal => signal;

        public Signal Flow(IReadOnlyList<Signal> inputs)
        {
            return signal = Element.Flow(inputs);
        }
    }
}