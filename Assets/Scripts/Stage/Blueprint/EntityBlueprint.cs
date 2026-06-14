using System;
using UnityEngine;
using static Lumencuit.CircuitElement;
namespace Lumencuit
{
    /// <summary>
    /// 배치 전 스테이지에 등록된 배치 가능한 엔티티입니다.
    /// </summary>
    [Serializable]
    public class EntityBlueprint
    {
        [SerializeField] protected CircuitElementType type = CircuitElementType.Lamp;

        public CircuitElementType Type => type;
        public string Id => type.ToString();

        public EntityBlueprint()
        {
        }

        public EntityBlueprint(CircuitElementType type)
        {
            this.type = type;
        }

        public virtual EntityBlueprint Clone()
        {
            return new EntityBlueprint(type);
        }

        public override bool Equals(object obj)
        {
            return obj is EntityBlueprint other && GetType() == other.GetType() && type == other.type;
        }

        public virtual CircuitElement CreateElement()
        {
            return type.ToElement();
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(GetType(), type);
        }

        public static bool operator ==(EntityBlueprint a, EntityBlueprint b)
        {
            if (ReferenceEquals(a, b))
                return true; 
            if (a is null || b is null)
                return false;
            return a.Equals(b);
        }

        public static bool operator !=(EntityBlueprint a, EntityBlueprint b)
        {
            return !(a == b);
        }
    }
}