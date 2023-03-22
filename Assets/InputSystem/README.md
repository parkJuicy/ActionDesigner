# InputSystem

## 디렉토리

- Assets폴더
  - InputSystem폴더
    - Editor폴더
      - [InputSystemEditor.cs](https://github.com/JuicyPark/ExternalModule/blob/main/Assets/InputSystem/Editor/InputSystemEditor.cs)
    - [InputManager.cs](https://github.com/JuicyPark/ExternalModule/blob/main/Assets/InputSystem/InputManager.cs)
    - [InputSetting.cs](https://github.com/JuicyPark/ExternalModule/blob/main/Assets/InputSystem/InputSetting.cs)

***

## 작업로그

[Input System 모듈개발 #4](https://github.com/ECONO-UNION/union-mentoring-1-Unity/pull/4)

[PlayerInput 개발 및 InputSystem 로직 개선 #10](https://github.com/ECONO-UNION/union-mentoring-1-Unity/pull/10)

[키변경 #15](https://github.com/ECONO-UNION/union-mentoring-1-Unity/pull/15)

***

## 사용방법

상단에 InputSystem 메뉴를 들어가서 Set Input system을 누르면 InputSystem Editor창이 뜨게됩니다. 사용자는 원하는 위치에 사용할 키의 정보들이 들어갈 InputSetting을 해당 위치에 지정후 **Refresh**하게 되면 해당 InputSetting을 불러오게 됩니다.

![1](https://user-images.githubusercontent.com/31693348/133912734-e176f957-732b-4a19-833a-5715e17210b8.png)



사용할 키들을 Key 리스트에 string : Name과 KeyCode : Code를 입력한 뒤 **Apply Input System**을 누를경우 하나의 파일(InputNames)이 생성됩니다. 해당 파일엔 방금 사용자가 지정한 Key들의 이름과 InputSetting의 위치를 담고 있습니다.

![2](https://user-images.githubusercontent.com/31693348/133912735-3575a798-5676-4b45-86f0-72ff96cab3fd.png)



사용자는 InputManager의 GetKey 메소드를 이용해서 자신이 정의한 Key들의 상태를 불러올 수 있습니다.

**(위 이미지에서 사용자가 지정한 Attack, Jump, Run이 자동으로 enum타입으로 생성되어 손쉽게 사용할 수 있습니다!😍)**

![image](https://user-images.githubusercontent.com/31693348/133870109-df4aeaf6-aa13-4d08-a2ec-0489ca94667a.png)

예시 )

![3](https://user-images.githubusercontent.com/31693348/133912736-d223118b-98e0-4722-b36b-7aa4a0f929d1.png)
