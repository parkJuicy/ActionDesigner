using UnityEngine;

namespace ActionDesigner.Runtime
{
    /// <summary>
    /// Motion: 실제 작업을 수행하는 독립적 클래스
    /// </summary>
    public abstract class Motion
    {
        /// <summary>
        /// Motion 시작 시 호출됩니다.
        /// </summary>
        public virtual void Initialize(ActionRunner runner)
        {
        }

        /// <summary>
        /// 매 프레임 호출됩니다.
        /// </summary>
        /// <param name="runner">실행 중인 ActionRunner</param>
        /// <returns>Motion이 완료되었으면 true, 아직 실행 중이면 false</returns>
        public abstract bool Update(ActionRunner runner);

        /// <summary>
        /// Motion 완료 시 호출됩니다.
        /// </summary>
        public virtual void OnComplete(ActionRunner runner)
        {
        }

        /// <summary>
        /// Motion을 중단할 때 호출됩니다.
        /// </summary>
        public virtual void Stop()
        {
        }

        /// <summary>
        /// Motion에 대한 설명을 반환합니다. (에디터용)
        /// </summary>
        public virtual string GetDescription()
        {
            return GetType().Name;
        }

        /// <summary>
        /// Motion의 진행률을 반환합니다. (0.0 ~ 1.0)
        /// </summary>
        public virtual float GetProgress()
        {
            return 0.0f;
        }
    }
}
