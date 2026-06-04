using System;
using UnityEngine;
using static Lumencuit.CircuitElement;
namespace Lumencuit
{
    /// <summary>
    /// 신호 색 정보가 추가된 블루프린트입니다.
    /// </summary>
    [Serializable]
    public sealed class ColoredBlueprint : EntityBlueprint
    {
        [SerializeField] private QuantumSignal signal = QuantumSignal.Null;

        public ColoredBlueprint()
        {
        }

        public ColoredBlueprint(CircuitElementType type, QuantumSignal signal) : base(type)
        {
            this.signal = signal;
        }

        public QuantumSignal Signal => signal;

        public override EntityBlueprint Clone()
        {
            return new ColoredBlueprint(type, signal);
        }

        public override bool Equals(object obj)
        {
            return obj is ColoredBlueprint other && type == other.type && signal == other.signal;
        }

        public override CircuitElement CreateElement()
        {
            if (type == CircuitElementType.Source)
                return Source.Create(signal);
            return type.ToElement();
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(GetType(), type, signal);
        }

        public static bool HasColor(CircuitElementType type)
        {
            if (type == CircuitElementType.Source)
                return true;
            return false;
        }
    }
}