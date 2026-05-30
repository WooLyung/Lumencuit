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
        public static readonly EntityRequestResult InvalidTile = new EntityRequestResult("InvalidTile");
        public static readonly EntityRequestResult AlreadyExist = new EntityRequestResult("AlreadyExist");
        public static readonly EntityRequestResult IsEmpty = new EntityRequestResult("IsEmpty");
    }
}
