namespace ActionDesigner.Runtime
{
    /// <summary>
    /// Motion: 실제 작업을 수행하는 독립적 클래스
    /// </summary>
    public interface IMotion
    {
        public void Start();
        
        public bool Update(float deltaTime);
        
        public void End() { }

        public void Stop() { }
    }
}
