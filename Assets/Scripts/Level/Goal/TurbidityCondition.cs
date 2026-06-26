using System;
using UnityEngine;

namespace Lumencuit
{
    /// <summary>
    /// 목표 신호의 탁도 조건을 지정합니다.
    /// </summary>
    [Serializable]
    public abstract class TurbidityCondition
    {
        public enum TurbidityConditionType
        {
            None,
            Min,
            Max,
            Range
        };

        public abstract bool IsMatch(int turbidity);

        public static TurbidityConditionType GetTurbidityConditionType(object condition)
        {
            return condition switch
            {
                NoTurbidityCondition => TurbidityConditionType.None,
                MinTurbidityCondition => TurbidityConditionType.Min,
                MaxTurbidityCondition => TurbidityConditionType.Max,
                RangeTurbidityCondition => TurbidityConditionType.Range,
                _ => TurbidityConditionType.None
            };
        }

        public static TurbidityCondition CreateTurbidityCondition(TurbidityConditionType type)
        {
            return type switch
            {
                TurbidityConditionType.None => new NoTurbidityCondition(),
                TurbidityConditionType.Min => new MinTurbidityCondition(),
                TurbidityConditionType.Max => new MaxTurbidityCondition(),
                TurbidityConditionType.Range => new RangeTurbidityCondition(),
                _ => new NoTurbidityCondition()
            };
        }
    }

    /// <summary>
    /// 탁도 조건이 없습니다.
    /// </summary>
    [Serializable]
    public class NoTurbidityCondition : TurbidityCondition
    {
        public override bool IsMatch(int turbidity)
        {
            return true;
        }
    }

    /// <summary>
    /// 탁도가 특정 값 이상이어야 합니다.
    /// </summary>
    [Serializable]
    public class MinTurbidityCondition : TurbidityCondition
    {
        [SerializeField] private int min = 0;

        public int Min => min;

        public MinTurbidityCondition()
        {
        }

        public MinTurbidityCondition(int min)
        {
            this.min = min;
        }

        public override bool IsMatch(int turbidity)
        {
            return min <= turbidity;
        }
    }

    /// <summary>
    /// 탁도가 특정 값 이하여야 합니다.
    /// </summary>
    [Serializable]
    public class MaxTurbidityCondition : TurbidityCondition
    {
        [SerializeField] private int max = 0;

        public int Max => max;

        public MaxTurbidityCondition()
        {
        }

        public MaxTurbidityCondition(int max)
        {
            this.max = max;
        }

        public override bool IsMatch(int turbidity)
        {
            return turbidity <= max;
        }
    }

    /// <summary>
    /// 탁도가 지정 구간 내에 있어야 합니다.
    /// </summary>
    [Serializable]
    public class RangeTurbidityCondition : TurbidityCondition
    {
        [SerializeField] private int min = 0;
        [SerializeField] private int max = 0;

        public int Min => min;
        public int Max => max;

        public RangeTurbidityCondition()
        {
        }

        public RangeTurbidityCondition(int min, int max)
        {
            this.min = min;
            this.max = max;
        }

        public override bool IsMatch(int turbidity)
        {
            return min <= turbidity && turbidity <= max;
        }
    }
}
