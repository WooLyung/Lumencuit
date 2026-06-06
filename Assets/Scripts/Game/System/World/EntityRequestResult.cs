using System;

namespace Lumencuit
{
    /// <summary>
    /// WorldGrid의 엔티티 요청에 대한 결과를 나타냅니다.
    /// </summary>
    public readonly struct EntityRequestResult
    {
        public readonly string Result;

        private EntityRequestResult(string result)
        {
            Result = result;
        }

        public static readonly EntityRequestResult Success = new EntityRequestResult("Success");
        public static readonly EntityRequestResult CantUndo = new EntityRequestResult("CantUndo");
        public static readonly EntityRequestResult CantRedo = new EntityRequestResult("CantRedo");
        public static readonly EntityRequestResult Fail = new EntityRequestResult("Fail");
        public static readonly EntityRequestResult NeedWire = new EntityRequestResult("NeedWire");
        public static readonly EntityRequestResult InvalidTile = new EntityRequestResult("InvalidTile");
        public static readonly EntityRequestResult InvalidPath = new EntityRequestResult("InvalidPath");
        public static readonly EntityRequestResult AlreadyExist = new EntityRequestResult("AlreadyExist");
        public static readonly EntityRequestResult IsEmpty = new EntityRequestResult("IsEmpty");
        public static readonly EntityRequestResult UnavailableBlueprint = new EntityRequestResult("UnavailableBlueprint");
        public static readonly EntityRequestResult UnavailablePort = new EntityRequestResult("UnavailablePort");
        public static readonly EntityRequestResult IsFixed = new EntityRequestResult("IsFixed");
        public static readonly EntityRequestResult InvalidPort = new EntityRequestResult("InvalidPort");

        public static bool operator ==(EntityRequestResult a, EntityRequestResult b) => a.Result == b.Result;
        public static bool operator !=(EntityRequestResult a, EntityRequestResult b) => a.Result != b.Result;

        public override bool Equals(object obj)
        {
            return obj is EntityRequestResult result && Result == result.Result;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Result);
        }
    }
}
