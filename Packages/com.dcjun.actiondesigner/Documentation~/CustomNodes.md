# 커스텀 노드 생성 가이드

새 프로젝트에서 ActionDesigner용 커스텀 노드를 만들 때 이 가이드를 따라주세요.

## 방법 1: Assembly Definition 없이 사용 (권장)

가장 간단한 방법입니다. 프로젝트의 `Assets/Scripts` 폴더에 바로 스크립트를 만들어주세요.

### 예제: DebugLogTask

```csharp
using UnityEngine;
using ActionDesigner.Runtime;

[System.Serializable]
public class DebugLogTask : IBehavior
{
    [SerializeField] private string message = "Hello World!";
    [SerializeField] private bool completeImmediately = true;
    
    public void Start()
    {
        Debug.Log($"[DebugLogTask] {message}");
    }
    
    public bool Update(float deltaTime)
    {
        return completeImmediately;
    }
    
    public void End() { }
    public void Stop() { }
}
```

## 방법 2: Assembly Definition 사용

더 체계적인 관리를 원한다면 Assembly Definition을 만들어주세요.

### 1. Assembly Definition 파일 생성

`Assets/Scripts/MyGameplay.asmdef` 파일을 만들고 다음 내용을 입력:

```json
{
    "name": "MyGameplay",
    "references": [
        "ActionDesigner.Runtime"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```

### 2. 해당 어셈블리 내에서 스크립트 작성

이제 `Assets/Scripts/` 폴더 내의 스크립트들이 ActionDesigner.Runtime을 참조할 수 있습니다.

## 문제 해결

### "Invalid Type" 에러가 발생하는 경우

1. **네임스페이스 확인**: 스크립트에 네임스페이스가 있다면 정확히 입력했는지 확인
2. **컴파일 확인**: Console에 컴파일 에러가 없는지 확인
3. **어셈블리 참조**: Assembly Definition을 사용한다면 `ActionDesigner.Runtime` 참조가 추가되었는지 확인

### 노드가 검색창에 나타나지 않는 경우

1. **인터페이스 구현**: `IBehavior` 또는 `ICondition`을 올바르게 구현했는지 확인
2. **Serializable 속성**: `[System.Serializable]` 속성이 클래스에 추가되었는지 확인
3. **추상 클래스**: 클래스가 `abstract`가 아닌지 확인

### 추천하는 폴더 구조

```
Assets/
├── Scripts/
│   ├── MyGameplay.asmdef (선택사항)
│   ├── Behaviors/
│   │   ├── MoveToTarget.cs
│   │   ├── AttackBehavior.cs
│   │   └── ...
│   └── Conditions/
│       ├── IsPlayerNearby.cs
│       ├── HasAmmo.cs
│       └── ...
```

## 네임스페이스 사용 예제

네임스페이스를 사용하고 싶다면:

```csharp
using UnityEngine;
using ActionDesigner.Runtime;

namespace MyGame.AI
{
    [System.Serializable]
    public class PatrolBehavior : IBehavior
    {
        // 구현...
    }
}
```

이렇게 하면 Action Designer에서 `MyGame.AI.PatrolBehavior`로 인식됩니다.