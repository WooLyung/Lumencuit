namespace Lumencuit
{
    /// <summary>
    /// 대응되지 않는 환경을 위한 미구현 인풋 시스템입니다.
    /// </summary>
    public sealed class NullInputSystem : InputSystem
    {
        public override void Update()
        {
        }
    }
}