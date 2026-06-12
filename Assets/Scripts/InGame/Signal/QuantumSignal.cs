using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Lumencuit
{
    /// <summary>
    /// 가능한 신호들의 집합입니다. Null 신호와 단일 신호를 포함합니다.
    /// </summary>
    [Serializable]
    public struct QuantumSignal
    {
        [SerializeField] private byte mask;

        public byte Mask => mask;
        public bool IsNull => mask == 0;
        public bool IsSingle => mask != 0 && (mask & (mask - 1)) == 0;
        public bool IsQuantum => !IsNull && !IsSingle;

        public QuantumSignal(byte mask)
        {
            this.mask = mask;
        }

        public bool Contains(Signal signal) => (mask & (1 << signal.Value)) != 0;
        public static QuantumSignal Null => new(0);
        public static QuantumSignal White => FromSignal(Signal.White);
        public static QuantumSignal Black => FromSignal(Signal.Black);
        public static QuantumSignal FromSignal(Signal signal) => new QuantumSignal((byte)(1 << signal.Value));

        public IEnumerable<Signal> GetSignals()
        {
            for (byte i = 0; i < 8; i++)
            {
                if ((mask & (1 << i)) != 0)
                    yield return Signal.FromValue(i);
            }
        }

        public Signal? ToSignal()
        {
            if (!IsSingle)
                return null;
            return GetSignals().ToList()[0];
        }

        /// <summary>
        /// 신호 연산으로 주어진 양자 신호를 연산합니다.
        /// </summary>
        private static QuantumSignal Operate(QuantumSignal a, QuantumSignal b, Func<Signal, Signal, Signal> operation)
        {
            byte result = 0;
            foreach (Signal left in a.GetSignals())
                foreach (Signal right in b.GetSignals())
                    result |= (byte)(1 << operation(left, right).Value);
            return new QuantumSignal(result);
        }

        /// <summary>
        /// 신호 연산으로 주어진 양자 신호를 연산합니다.
        /// </summary>
        private static QuantumSignal Operate(QuantumSignal a, Func<Signal, Signal> operation)
        {
            byte result = 0;
            foreach (Signal signal in a.GetSignals())
                result |= (byte)(1 << operation(signal).Value);
            return new QuantumSignal(result);
        }

        // 단항 연산자
        public static QuantumSignal operator ~(QuantumSignal a) => Operate(a, x => ~x);

        // 이항 연산자
        public static QuantumSignal operator &(QuantumSignal a, QuantumSignal b) => Operate(a, b, (x, y) => x & y);
        public static QuantumSignal operator |(QuantumSignal a, QuantumSignal b) => Operate(a, b, (x, y) => x | y);
        public static QuantumSignal operator ^(QuantumSignal a, QuantumSignal b) => Operate(a, b, (x, y) => x ^ y);
        public static QuantumSignal operator -(QuantumSignal a, QuantumSignal b) => Operate(a, b, (x, y) => x - y);
        
        // 비교 연산자
        public static bool operator ==(QuantumSignal a, QuantumSignal b) => a.mask == b.mask;
        public static bool operator !=(QuantumSignal a, QuantumSignal b) => !(a == b);

        public override bool Equals(object obj)
        {
            return obj is QuantumSignal signal && this == signal;
        }

        public override int GetHashCode()
        {
            return mask.GetHashCode();
        }

        public override string ToString()
        {
            if (IsNull)
                return "NULL";
            if (IsSingle)
                foreach (Signal signal in GetSignals())
                    return signal.Name;

            List<string> names = new();
            foreach (Signal signal in GetSignals())
                names.Add(signal.ShortName);

            return "{" + string.Join("", names) + "}";
        }
    }
}