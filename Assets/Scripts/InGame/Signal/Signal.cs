using System;
using UnityEngine;

namespace Lumencuit
{
    /// <summary>
    /// 회로의 신호를 나타내는 구조체입니다.
    /// </summary>
    [Serializable]
    public struct Signal
    {
        [SerializeField] private byte value;

        public byte Value => (byte)(value & 0b111);

        private Signal(byte value)
        {
            this.value = (byte)(value & 0b111);
        }

        // 색상
        public static readonly Signal Black = new(0b000);
        public static readonly Signal Red = new(0b001);
        public static readonly Signal Green = new(0b010);
        public static readonly Signal Blue = new(0b100);
        public static readonly Signal Yellow = new(0b011);
        public static readonly Signal Magenta = new(0b101);
        public static readonly Signal Cyan = new(0b110);
        public static readonly Signal White = new(0b111);

        // 단항 연산자
        public static Signal operator ~(Signal a) => new((byte)(0b111 ^ a.value));

        // 이항 연산자
        public static Signal operator &(Signal a, Signal b) => new((byte)(a.value & b.value));
        public static Signal operator |(Signal a, Signal b) => new((byte)(a.value | b.value));
        public static Signal operator ^(Signal a, Signal b) => new((byte)(a.value ^ b.value));
        public static Signal operator -(Signal a, Signal b) => new((byte)(a.Value & ~b.Value & 0b111));
        
        // 비교 연산자
        public static bool operator ==(Signal a, Signal b) => a.Value == b.Value;
        public static bool operator !=(Signal a, Signal b) => !(a == b);

        public Color Color => value switch
        {
            0b001 => Color.red,
            0b010 => Color.green,
            0b100 => Color.blue,
            0b011 => Color.yellow,
            0b101 => Color.magenta,
            0b110 => Color.cyan,
            0b111 => Color.white,
            _ => Color.black,
        };

        public string Name => value switch
        {
            0b001 => "Red",
            0b010 => "Green",
            0b100 => "Blue",
            0b011 => "Yellow",
            0b101 => "Magenta",
            0b110 => "Cyan",
            0b111 => "White",
            _ => "Black",
        };

        public string ShortName => value switch
        {
            0b001 => "R",
            0b010 => "G",
            0b100 => "B",
            0b011 => "Y",
            0b101 => "M",
            0b110 => "C",
            0b111 => "W",
            _ => "K",
        };

        public static Signal FromValue(byte value) => new Signal(value);

        public override bool Equals(object obj)
        {
            return obj is Signal signal && this == signal;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}