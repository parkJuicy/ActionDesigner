namespace ActionDesigner.Runtime
{
    /// <summary>
    /// Behavior: 실제 작업을 수행하는 독립적 클래스
    /// </summary>
    public interface IBehavior
    {
        public void Start() { }

        public bool Update(float deltaTime);
        
        public void End() { }

        public void Stop() { }
    }
}