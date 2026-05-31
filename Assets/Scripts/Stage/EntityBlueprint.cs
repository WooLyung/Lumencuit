using System;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using static Lumencuit.CircuitElement;
using static Lumencuit.Signal;

namespace Lumencuit
{
    /// <summary>
    /// 배치 전 스테이지에 등록된 배치 가능한 엔티티입니다.
    /// </summary>
    [Serializable]
    public sealed class EntityBlueprint
    {
        [SerializeField] private CircuitElementType type = CircuitElementType.Lamp;
        [SerializeField] private SignalColor signalColor = SignalColor.Black;

        public CircuitElementType Type => type;
        public SignalColor SignalColor => signalColor;

        public EntityBlueprint()
        {
        }

        public EntityBlueprint(CircuitElementType type, SignalColor signalColor)
        {
            this.type = type;
            this.signalColor = signalColor;
        }

        public EntityBlueprint Clone()
        {
            return new EntityBlueprint(type, signalColor);
        }

        public override bool Equals(object obj)
        {
            return obj is EntityBlueprint other && this == other;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(type, signalColor, Type, SignalColor);
        }

        public static bool operator ==(EntityBlueprint a, EntityBlueprint b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if (a is null && b is null)
                return true;
            if (a is null || b is null)
                return false;
            return a.Type == b.Type && a.SignalColor == b.SignalColor;
        }

        public static bool operator !=(EntityBlueprint a, EntityBlueprint b)
        {
            return !(a == b);
        }
    }
}