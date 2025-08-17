# Action Designer Samples

이 폴더에는 Action Designer를 사용하는 방법을 보여주는 예제들이 포함되어 있습니다.

## Basic Usage

### 포함된 샘플 노드들

#### Conditions
- **KeyPressCondition**: 특정 키 입력을 감지
- **AlwaysTrueCondition**: 항상 true를 반환 (테스트용)

#### Behaviors  
- **DebugLogBehavior**: 콘솔에 로그 메시지 출력
- **RotateBehavior**: 오브젝트를 지정된 속도로 회전

### 사용 방법

1. SampleNodes.cs를 프로젝트에 복사
2. 빈 GameObject 생성
3. ActionRunner 컴포넌트 추가
4. Action Designer 에디터 열기
5. 샘플 노드들을 사용하여 행동 트리 구성

### 예제 구성

```
[Root] DebugLogBehavior ("시작!")
   ↓
KeyPressCondition (Space키)
   ↓  
RotateBehavior (2초간 회전)
   ↓
AlwaysTrueCondition
   ↓
DebugLogBehavior ("완료!")
```

이렇게 구성하면:
1. "시작!" 메시지 출력
2. Space키 입력 대기
3. 2초간 오브젝트 회전
4. "완료!" 메시지 출력

### 커스텀 노드 만들기

SampleNodes.cs를 참고하여 자신만의 Behavior와 Condition을 만들어보세요!