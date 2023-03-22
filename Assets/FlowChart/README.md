# FlowChart

## 디렉토리

- Assets폴더
  - FlowChart폴더
    - Editor폴더
      - [FlowChartEditor.cs](https://github.com/JuicyPark/ExternalModule/blob/main/Assets/FlowChart/Editor/FlowChartEditor.cs)
      - [FlowChartView.cs](https://github.com/JuicyPark/ExternalModule/blob/main/Assets/FlowChart/Editor/FlowChartView.cs)
      - [NodeView.cs](https://github.com/JuicyPark/ExternalModule/blob/main/Assets/FlowChart/Editor/NodeView.cs)
      - [InspectorView.cs](https://github.com/JuicyPark/ExternalModule/blob/main/Assets/FlowChart/Editor/InspectorView.cs)
    - Task폴더
      - [Task.cs](https://github.com/JuicyPark/ExternalModule/blob/main/Assets/FlowChart/Task/Task.cs)
      - [Action.cs](https://github.com/JuicyPark/ExternalModule/blob/main/Assets/FlowChart/Task/Action.cs)
      - [Condition.cs](https://github.com/JuicyPark/ExternalModule/blob/main/Assets/FlowChart/Task/Condition.cs)
    - [FlowChart.cs](https://github.com/JuicyPark/ExternalModule/blob/main/Assets/FlowChart/FlowChart.cs)
    - [Node.cs](https://github.com/JuicyPark/ExternalModule/blob/main/Assets/FlowChart/Node.cs)
    - [FlowChartRunner.cs](https://github.com/JuicyPark/ExternalModule/blob/main/Assets/FlowChart/FlowChartRunner.cs)

***

## 작업로그

[FlowChart System 개발 #21](https://github.com/ECONO-UNION/union-mentoring-1-Unity/pull/21)

[FlowChart Node를 Non ScriptableObject방식으로 변경 #23](https://github.com/ECONO-UNION/union-mentoring-1-Unity/pull/23)

***

## 사용방법

상단의 FlowChart -> Editor... 을 누르면 FlowChartEditor가 나타나게됩니다.

![1](https://user-images.githubusercontent.com/31693348/133912580-203e1170-244b-4323-b266-fc4fa53ebdfd.png)



Project창에서 Create -> FlowChart를 누르면 새로운 FlowChart를 생성할 수 있습니다.

![2](https://user-images.githubusercontent.com/31693348/133912673-5ceb815b-e463-48a2-85a0-b379f63e44b7.png)



사용자가 사용할 행동인 Action과 조건인 Condition을 직접 구현합니다. Action은 해당 노드가 실행될때 한 번 실행되는 Start()와 매프레임 실행되는 Update가 있고, Condition은 해당 노드의 자식 노드들로 넘어갈지 말지를 정하는 Check()를 갖고 있습니다.

![3](https://user-images.githubusercontent.com/31693348/133912676-6afc1ee7-16b9-49e4-98ea-17ef488db53c.png)



이렇게 Action과 Condition을 정의하였다면 FlowChartEditor에 돌아와서 오른쪽 마우스 클릭을 했을때 방금 정의했던 Action과 Condition을 확인 할 수 있습니다. 이제 원하는 순서에 맞춰 Action노드와 Condition노드를 배치해서 FlowChart를 완성한 후 좌상단의 **SAVE** 버튼을 눌러 해당 FlowChart를 저장할 수 있습니다.

![4](https://user-images.githubusercontent.com/31693348/133912677-125ac58e-b192-4e52-a679-b05cbd279fb2.png)



Hierachy에 새로운 GameObject를 생성하고 FlowChartRunner.cs를 추가합니다. FlowChartRunner에 위에서 만든 FlowChart를 끌어넣으면 FlowChart에 정의한 노드의 순서에 맞게 실행되는걸 볼 수 있습니다.

![4](https://user-images.githubusercontent.com/31693348/133912678-fa17262c-a1db-493c-928e-a5e8fed6a2eb.png)



게임을 시작해보면 현재 실행중인 노드는 초록색빛으로 조건에 부합되지  않아 실행되지 않는 노드는 검정색으로 표시되는것을 볼 수 있습니다.

![image](https://user-images.githubusercontent.com/31693348/133872736-2da9d4ea-aa98-485b-93ec-da6c10eae96c.png)

