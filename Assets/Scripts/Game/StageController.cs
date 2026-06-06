namespace Lumencuit
{
    /// <summary>
    /// 현 스테이지의 정보를 관리합니다.
    /// </summary>
    public sealed class StageController
    {
        private bool isCleared;

        public bool IsCleared => isCleared;

        public void Clear()
        {
            isCleared = true;
        }
    }
}