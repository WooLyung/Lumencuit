using System;

namespace Lumencuit
{
    public partial class Entity
    {
        /// <summary>
        /// 회로 요소의 네 면의 입출력 여부를 나타냅니다.
        /// </summary>
        [Serializable]
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

            public static bool operator ==(Ports a, Ports b) => a.Left == b.Left && a.Right == b.Right && a.Up == b.Up && a.Down == b.Down;
            public static bool operator !=(Ports a, Ports b) => !(a == b);

            public override bool Equals(object obj)
            {
                return obj is Ports ports && this == ports;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Left, Right, Up, Down);
            }

            public static readonly Ports None = new(PortType.None, PortType.None, PortType.None, PortType.None);
        }
    }
}