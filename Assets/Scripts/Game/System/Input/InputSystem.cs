namespace Lumencuit
{
    /// <summary>
    /// 유니티의 입력을 가공하여 제공하는 시스템입니다.
    /// 구동 환경에 따라 서로 다른 입력을 정의합니다.
    /// </summary>
    public abstract class InputSystem
    {
        public abstract void Update();
    }
}