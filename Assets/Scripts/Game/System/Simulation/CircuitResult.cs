using System;

namespace Lumencuit
{
    /// <summary>
    /// WorldGrid의 엔티티 요청에 대한 결과를 나타냅니다.
    /// </summary>
    public readonly struct CircuitResult
    {
        public readonly string Result;

        private CircuitResult(string result)
        {
            Result = result;
        }

        public static readonly CircuitResult Success = new CircuitResult("Success");
        public static readonly CircuitResult Fail = new CircuitResult("Fail");
        public static readonly CircuitResult CantReach = new CircuitResult("CantReach");
        public static readonly CircuitResult UnplacedBlueprint = new CircuitResult("UnplacedBlueprint");
        public static readonly CircuitResult IncompleteCircuit = new CircuitResult("IncompleteCircuit");

        public static bool operator ==(CircuitResult a, CircuitResult b) => a.Result == b.Result;
        public static bool operator !=(CircuitResult a, CircuitResult b) => a.Result != b.Result;

        public override bool Equals(object obj)
        {
            return obj is CircuitResult result && Result == result.Result;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Result);
        }
    }
}
