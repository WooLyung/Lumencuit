using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;

namespace Lumencuit
{
    /// <summary>
    /// 그리드에 설치된 회로 요소를 나타내는 객체입니다.
    /// </summary>
    public partial class Entity
    {
        /// <summary>
        /// 회로 요소의 각 면의 입출력 여부를 나타냅니다.
        /// </summary>
        public enum PortType { None = 0, Input = 1, Output = 2 };

        public readonly EntityBlueprint MadeBy;
        public readonly CircuitElement Element;
        private readonly bool isFixed = false;
        private Ports ports = Ports.None;

        public Entity(EntityBlueprint madeBy)
        {
            MadeBy = madeBy;
            Element = madeBy.CreateElement();
        }

        public Entity(EntityBlueprint madeBy, Ports ports) : this(madeBy)
        {
            this.ports = ports;
        }

        public Entity(EntityBlueprint madeBy, Ports ports, bool isFixed) : this(madeBy, ports)
        {
            this.isFixed = isFixed;
        }

        public PortType LeftPort { get => ports.Left; set => ports.Left = value; }
        public PortType RightPort { get => ports.Right; set => ports.Right = value; }
        public PortType UpPort { get => ports.Up; set => ports.Up = value; }
        public PortType DownPort { get => ports.Down; set => ports.Down = value; }
        public Ports GetPorts() => ports;
        public int InPortCount => (LeftPort == PortType.Input ? 1 : 0) + (DownPort == PortType.Input ? 1 : 0) + (RightPort == PortType.Input ? 1 : 0) + (UpPort == PortType.Input ? 1 : 0);
        public int OutPortCount => (LeftPort == PortType.Output ? 1 : 0) + (DownPort == PortType.Output ? 1 : 0) + (RightPort == PortType.Output ? 1 : 0) + (UpPort == PortType.Output ? 1 : 0);
        public bool IsFixed => isFixed;

        public QuantumSignal Flow(IReadOnlyList<QuantumSignal> inputs)
        {
            return Element.Flow(inputs);
        }

        public Entity Clone()
        {
            return new Entity(MadeBy, ports);
        }
    }
}