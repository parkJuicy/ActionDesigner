using UnityEngine;

namespace ActionDesigner.Runtime
{
    /// <summary>
    /// Condition: 조건을 평가하여 다음 Motion으로 진행할지 결정하는 독립적 클래스
    /// </summary>
    public abstract class Condition
    {
        /// <summary>
        /// 조건을 평가합니다.
        /// </summary>
        /// <param name="runner">실행 중인 ActionRunner</param>
        /// <returns>조건이 만족되면 true, 아니면 false</returns>
        public abstract bool Evaluate(ActionRunner runner);

        /// <summary>
        /// Condition 평가 전 초기화
        /// </summary>
        public virtual void Initialize(ActionRunner runner)
        {
        }

        /// <summary>
        /// Condition 평가 완료 후 정리
        /// </summary>
        public virtual void Cleanup(ActionRunner runner)
        {
        }

        /// <summary>
        /// 조건에 대한 설명을 반환합니다. (에디터용)
        /// </summary>
        public virtual string GetDescription()
        {
            return GetType().Name;
        }

        /// <summary>
        /// 조건을 대기하면서 평가할지 여부
        /// true면 매 프레임 체크, false면 한 번만 체크
        /// </summary>
        public virtual bool IsWaitCondition => false;
    }
}
