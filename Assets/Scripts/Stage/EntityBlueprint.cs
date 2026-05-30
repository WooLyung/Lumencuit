using NUnit.Framework;
using System;
using System.Collections.Generic;
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
        [SerializeField] private int count = 1;

        public CircuitElementType Type => type;
        public SignalColor SignalColor => signalColor;
        public int Count => count;
    }
}