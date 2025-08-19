using ActionDesigner.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ActionDesigner.Editor
{
    public class ActionView : GraphView
    {
        [Obsolete("Obsolete")] public new class UxmlFactory : UxmlFactory<ActionView, UxmlTraits> { }
        ActionRunner _actionRunner;
        Runtime.Action _action;
        Vector2 _lastMousePosition;
        
        // 런타임 하이라이팅을 위한 변수들
        private int _lastExecutingNodeID = 0;
        private bool _isRuntimeUpdateActive = false;
        
        // 복사/붙여넣기를 위한 변수들
        private List<BaseNode> _copiedNodes = new List<BaseNode>();

        public Action<NodeView> OnNodeSelectionChanged { get; internal set; }

        public ActionView()
        {
            Insert(0, new GridBackground());

            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(UIToolkitPath.ussPath);
            styleSheets.Add(styleSheet);
            
            // 마우스 위치 추적
            RegisterCallback<MouseMoveEvent>(OnMouseMove);
            
            // 키보드 이벤트 등록
            RegisterCallback<KeyDownEvent>(OnKeyDown);
            
            // 키보드 이벤트 포커스 가능하도록 설정
            focusable = true;
            
            // 그리드 스냅 설정 (정확한 정렬을 위해)
            this.StretchToParentSize();
            
            // 더 정밀한 위치 제어를 위한 설정
            this.style.position = Position.Relative;
        }
        
        /// <summary>
        /// 모든 NodeView의 타이틀을 새로고침
        /// </summary>
        public void RefreshAllNodeTitles()
        {
            foreach (var element in graphElements)
            {
                if (element is NodeView nodeView)
                {
                    nodeView.UpdateTitle();
                }
            }
            
            // 노드 타입 변경 후 잘못된 연결 제거
            ValidateAndCleanConnections();
        }
        
        /// <summary>
        /// 잘못된 연결들을 검사하고 제거
        /// </summary>
        private void ValidateAndCleanConnections()
        {
            if (_action == null) return;
            
            var invalidConnections = new List<(BaseNode parent, BaseNode child)>();
            
            // 모든 연결 검사
            foreach (var node in _action.nodes)
            {
                for (int i = node.childrenID.Count - 1; i >= 0; i--)
                {
                    var childID = node.childrenID[i];
                    var childNode = _action.nodes.Find(n => n.id == childID);
                    
                    if (childNode == null)
                    {
                        // 자식 노드가 없음
                        node.childrenID.RemoveAt(i);
                        continue;
                    }
                    
                    // 연결 규칙 검증
                    if (!IsValidConnection(node, childNode))
                    {
                        invalidConnections.Add((node, childNode));
                    }
                }
            }
            
            // 잘못된 연결 제거
            foreach (var (parent, child) in invalidConnections)
            {
                RemoveConnection(parent, child);
                Debug.LogWarning($"잘못된 연결 제거: {parent.GetDisplayName()} -> {child.GetDisplayName()}");
            }
            
            if (invalidConnections.Count > 0)
            {
                // UI 새로고침
                RefreshGraphView();
                EditorUtility.SetDirty(_actionRunner);
            }
        }

        /// <summary>
        /// 연결이 유효한지 검증 (기존 연결 검증용)
        /// </summary>
        private bool IsValidConnection(BaseNode parent, BaseNode child)
        {
            return IsValidConnection(parent, child, true);
        }
        
        /// <summary>
        /// 연결이 유효한지 검증 (개선된 단일 메서드)
        /// </summary>
        private bool IsValidConnection(BaseNode parent, BaseNode child, bool isExistingConnection = false)
        {
            // 노드가 유효하지 않으면 연결 불가
            if (parent == null || child == null) return false;
            
            bool parentIsBehavior = parent is BehaviorNode behaviorParent && behaviorParent.IsValid;
            bool parentIsCondition = parent is ConditionNode conditionParent && conditionParent.IsValid;
            bool childIsBehavior = child is BehaviorNode behaviorChild && behaviorChild.IsValid;
            bool childIsCondition = child is ConditionNode conditionChild && conditionChild.IsValid;
            
            if (!parentIsBehavior && !parentIsCondition) return false;
            if (!childIsBehavior && !childIsCondition) return false;
            
            // Behavior → Condition만 허용
            if (parentIsBehavior && !childIsCondition) return false;
            
            // Condition → Behavior만 허용  
            if (parentIsCondition && !childIsBehavior) return false;
            
            // Condition은 하나의 자식만 가능 (체인 구조)
            // 단, 기존 연결 검증 시에는 현재 검사 중인 연결은 제외
            if (parentIsCondition && !isExistingConnection && parent.childrenID.Count >= 1) 
            {
                return false;
            }
            
            // 기존 연결 검증 시: 현재 child가 이미 parent의 자식에 포함되어 있다면 유효
            if (isExistingConnection && parentIsCondition && parent.childrenID.Contains(child.id))
            {
                // 다른 자식이 있는지 확인 (현재 child 제외)
                int otherChildrenCount = parent.childrenID.Count(id => id != child.id);
                if (otherChildrenCount > 0) return false; // 다른 자식이 있으면 무효
            }
            
            return true;
        }

        /// <summary>
        /// 연결 제거
        /// </summary>
        private void RemoveConnection(BaseNode parent, BaseNode child)
        {
            _action.RemoveChild(parent, child);
        }
        
        /// <summary>
        /// 더 안전한 연결 검증 및 정리 (전체 새로고침 없이)
        /// </summary>
        private void ValidateAndCleanConnectionsSafely()
        {
            if (_action == null) return;
            
            var invalidConnections = new List<(BaseNode parent, BaseNode child)>();
            
            // 모든 연결 검사
            foreach (var node in _action.nodes)
            {
                for (int i = node.childrenID.Count - 1; i >= 0; i--)
                {
                    var childID = node.childrenID[i];
                    var childNode = _action.nodes.Find(n => n.id == childID);
                    
                    if (childNode == null)
                    {
                        // 자식 노드가 없음
                        node.childrenID.RemoveAt(i);
                        continue;
                    }
                    
                    // 연결 규칙 검증
                    if (!IsValidConnection(node, childNode))
                    {
                        invalidConnections.Add((node, childNode));
                    }
                }
            }
            
            // 잘못된 연결 제거
            foreach (var (parent, child) in invalidConnections)
            {
                RemoveConnection(parent, child);
                Debug.LogWarning($"잘못된 연결 제거: {parent.GetDisplayName()} -> {child.GetDisplayName()}");
            }
            
            if (invalidConnections.Count > 0)
            {
                // 전체 새로고침 대신 Edge만 업데이트
                UpdateEdgesOnly();
                EditorUtility.SetDirty(_actionRunner);
            }
        }
        
        /// <summary>
        /// 노드는 그대로 두고 Edge만 업데이트
        /// </summary>
        private void UpdateEdgesOnly()
        {
            // 기존 Edge들만 제거
            var edgesToRemove = graphElements.ToList().OfType<Edge>().ToList();
            foreach (var edge in edgesToRemove)
            {
                RemoveElement(edge);
            }
            
            // Edge 다시 생성
            DrawEdge();
        }
        
        /// <summary>
        /// 그래프 뷰 전체 새로고침
        /// </summary>
        private void RefreshGraphView()
        {
            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements.ToList());
            graphViewChanged += OnGraphViewChanged;
            
            DrawNode();
            DrawEdge();
        }
        
        public void PopulateView(Runtime.Action action)
        {
            _action = action;

            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements.ToList());
            graphViewChanged += OnGraphViewChanged;

            DrawNode();
            DrawEdge();
            
            // 런타임 업데이트 시작
            StartRuntimeUpdate();
        }

        public void ClearView()
        {
            // 런타임 업데이트 중단
            StopRuntimeUpdate();
            
            _actionRunner = null;
            _action = null;
            DeleteElements(graphElements.ToList());
        }

        private void DrawNode()
        {
            _action.nodes.ForEach(node => CreateNodeView(node));
        }

        private void DrawEdge()
        {
            _action.nodes.ForEach(node =>
            {
                node.childrenID.ForEach(childID =>
                {
                    NodeView parentView = FindNodeView(node.id);
                    NodeView childView = FindNodeView(childID);

                    if (parentView != null && childView != null)
                    {
                        Edge edge = parentView.Output.ConnectTo(childView.Input);
                        AddElement(edge);
                    }
                });
            });
        }

        NodeView FindNodeView(int nodeID)
        {
            return GetNodeByGuid(nodeID.ToString()) as NodeView;
        }

        void CreateNodeView(BaseNode node)
        {
            NodeView nodeView = new NodeView(node, node.id == _action.rootID)
            {
                OnNodeSelected = OnNodeSelectionChanged,
                OnNodeRootSet = OnNodeRootSet,
                OnNodeTypeChanged = OnNodeTypeChanged
            };
            AddElement(nodeView);
        }
        
        /// <summary>
        /// 노드 타입 변경 시 호출되는 이벤트 핸들러
        /// </summary>
        void OnNodeTypeChanged(NodeView nodeView)
        {
            // 즉시 연결 검증 및 정리 (더 안전하게)
            ValidateAndCleanConnectionsSafely();
        }
        
        GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            if (_action == null || Application.isPlaying)
                return graphViewChange;

            // Delete Node
            if (graphViewChange.elementsToRemove != null)
            {
                graphViewChange.elementsToRemove.ForEach(element =>
                {
                    NodeView nodeView = element as NodeView;
                    if (nodeView != null)
                    {
                        _action.DeleteNode(nodeView.Node);
                    }

                    Edge edge = element as Edge;
                    if (edge != null)
                    {
                        NodeView parentView = edge.output.node as NodeView;
                        NodeView childView = edge.input.node as NodeView;
                        _action.RemoveChild(parentView.Node, childView.Node);
                    }
                });
            }

            // Create Edge with validation
            if (graphViewChange.edgesToCreate != null)
            {
                var validEdges = new List<Edge>();
                
                graphViewChange.edgesToCreate.ForEach(edge =>
                {
                    NodeView parentView = edge.output.node as NodeView;
                    NodeView childView = edge.input.node as NodeView;
                    
                    // 연결 규칙 검증
                    if (IsValidConnection(parentView.Node, childView.Node, false)) // 새 연결이므로 false
                    {
                        _action.AddChild(parentView.Node, childView.Node);
                        validEdges.Add(edge);
                    }
                    else
                    {
                        // 개선된 에러 메시지
                        ShowConnectionError(parentView.Node, childView.Node);
                        
                        // 잘못된 연결 제거
                        edge.output.Disconnect(edge);
                        edge.input.Disconnect(edge);
                        RemoveElement(edge);
                    }
                });
                
                graphViewChange.edgesToCreate = validEdges;
            }

            if (graphViewChange.movedElements != null)
            {
                graphViewChange.movedElements.ForEach((node) =>
                {
                    NodeView nodeView = node as NodeView;
                    if (nodeView?.Input?.connections != null)
                    {
                        foreach (var parentEdge in nodeView.Input.connections)
                        {
                            if (parentEdge == null) break;

                            var parent = parentEdge.output.node as NodeView;
                            parent?.Node?.childrenID.Sort(SortByHoriziontalPosition);
                        }
                    }
                });
            }

            EditorUtility.SetDirty(_actionRunner);
            return graphViewChange;
        }

        /// <summary>
        /// 개선된 연결 에러 메시지
        /// </summary>
        private void ShowConnectionError(BaseNode parent, BaseNode child)
        {
            string parentName = parent.GetDisplayName();
            string childName = child.GetDisplayName();
            
            bool parentIsBehavior = parent is BehaviorNode;
            bool parentIsCondition = parent is ConditionNode;
            bool childIsBehavior = child is BehaviorNode;
            bool childIsCondition = child is ConditionNode;
            
            if (parentIsBehavior && childIsBehavior)
            {
                Debug.LogWarning($"Behavior끼리는 연결할 수 없습니다. Behavior 뒤에는 Condition이 와야 합니다: {parentName} -> {childName}");
            }
            else if (parentIsCondition && childIsCondition)
            {
                Debug.LogWarning($"Condition끼리는 연결할 수 없습니다. Condition 뒤에는 Behavior이 와야 합니다: {parentName} -> {childName}");
            }
            else if (parentIsCondition && parent.childrenID.Count >= 1)
            {
                Debug.LogWarning($"Condition은 하나의 자식만 가질 수 있습니다 (체인 구조): {parentName}");
            }
            else
            {
                Debug.LogWarning($"유효하지 않은 연결입니다: {parentName} -> {childName}");
            }
        }

        int SortByHoriziontalPosition(int left, int right)
        {
            var leftNode = FindNodeView(left)?.Node;
            var rightNode = FindNodeView(right)?.Node;
            
            if (leftNode == null || rightNode == null) return 0;
            
            return leftNode.position.x < rightNode.position.x ? -1 : 1;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            // 우클릭 노드 생성 메뉴 제거 - 스페이스바 검색 사용
            // 기본 그래프뷰 메뉴만 유지 (복사, 붙여넣기 등)
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            if (Application.isPlaying)
            {
                return new List<Port>();
            }

            var compatiblePorts = new List<Port>();
            var startNodeView = startPort.node as NodeView;
            
            if (startNodeView == null)
                return compatiblePorts;

            foreach (var port in ports.ToList())
            {
                var endNodeView = port.node as NodeView;
                
                // 기본 조건: 반대 방향이고 다른 노드여야 함
                if (port.direction == startPort.direction || port.node == startPort.node)
                    continue;
                
                if (endNodeView == null)
                    continue;
                
                // Output 포트에서 시작하는 경우 (상위 -> 하위 연결)
                if (startPort.direction == Direction.Output)
                {
                    if (IsValidConnection(startNodeView.Node, endNodeView.Node, false)) // 새 연결
                    {
                        compatiblePorts.Add(port);
                    }
                }
                // Input 포트에서 시작하는 경우 (하위 -> 상위 연결)
                else
                {
                    if (IsValidConnection(endNodeView.Node, startNodeView.Node, false)) // 새 연결
                    {
                        compatiblePorts.Add(port);
                    }
                }
            }
            
            return compatiblePorts;
        }
        
        void OnNodeRootSet(int newRootNodeID)
        {
            var newRootNode = _action.FindNode(newRootNodeID);
            
            // 루트 노드는 Behavior만 가능
            if (newRootNode == null || !(newRootNode is BehaviorNode behaviorNode) || !behaviorNode.IsValid)
            {
                Debug.LogWarning("루트 노드는 Behavior만 설정할 수 있습니다.");
                return;
            }
            
            _action.rootID = newRootNodeID;
            
            // 루트 노드도 input을 가질 수 있으므로 연결 제거 로직 삭제
            // 기존: _action.nodes.ForEach(node => DisconnectRootParentEdge(node, newRootNodeID));
            
            // UI 새로고침
            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements.ToList());
            graphViewChanged += OnGraphViewChanged;
            
            DrawNode();
            DrawEdge();
        }
        
        /// <summary>
        /// 마우스 위치 추적
        /// </summary>
        private void OnMouseMove(MouseMoveEvent evt)
        {
            _lastMousePosition = evt.localMousePosition;
        }
        
        /// <summary>
        /// 키보드 이벤트 처리
        /// </summary>
        protected override void HandleEventBubbleUp(EventBase evt)
        {
            base.HandleEventBubbleUp(evt);
            
            if (evt is KeyDownEvent keyDownEvent)
            {
                // 스페이스바 감지
                if (keyDownEvent.keyCode == KeyCode.Space && !keyDownEvent.ctrlKey && !keyDownEvent.shiftKey && !keyDownEvent.altKey)
                {
                    if (_action != null && !Application.isPlaying)
                    {
                        OpenNodeSearchWindow();
                        evt.StopPropagation();
                        evt.PreventDefault();
                    }
                }
            }
        }
        
        /// <summary>
        /// 노드 검색 창 열기
        /// </summary>
        private void OpenNodeSearchWindow()
        {
            // 마우스 위치를 월드 좌표로 변환
            VisualElement contentViewContainer = ElementAt(1);
            Vector2 worldMousePosition = _lastMousePosition - (Vector2)contentViewContainer.transform.position;
            worldMousePosition *= 1 / contentViewContainer.transform.scale.x;
            
            NodeSearchWindow.Open(this, _lastMousePosition);
        }
        
        /// <summary>
        /// 지정된 위치에 노드 생성 (NodeSearchWindow에서 호출)
        /// </summary>
        public void CreateNodeAtPosition(Type nodeType, string baseType, Vector2 screenPosition)
        {
            if (_action == null || Application.isPlaying)
                return;
                
            // 스크린 좌표를 월드 좌표로 변환
            VisualElement contentViewContainer = ElementAt(1);
            Vector2 worldMousePosition = screenPosition - (Vector2)contentViewContainer.transform.position;
            worldMousePosition *= 1 / contentViewContainer.transform.scale.x;
            
            BaseNode node = _action.CreateNode(nodeType.Name, nodeType.Namespace, baseType, worldMousePosition);
            
            // 루트 노드는 Behavior만 가능
            if (_action.rootID == 0 && baseType == "Behavior")
                _action.rootID = node.id;

            CreateNodeView(node);
            EditorUtility.SetDirty(_actionRunner);
        }
        
        /// <summary>
        /// 런타임 상태 업데이트
        /// </summary>
        public void UpdateRuntimeStates(ActionRunner actionRunner, int currentNodeID)
        {
            _actionRunner = actionRunner;
            
            // 이전 하이라이팅 제거
            if (_lastExecutingNodeID != 0)
            {
                var lastNodeView = FindNodeView(_lastExecutingNodeID);
                lastNodeView?.SetRuntimeHighlight(false);
            }
            
            // 새 하이라이팅 적용
            if (currentNodeID != 0)
            {
                var currentNodeView = FindNodeView(currentNodeID);
                currentNodeView?.SetRuntimeHighlight(true);
            }
            
            _lastExecutingNodeID = currentNodeID;
        }
        
        /// <summary>
        /// 런타임 업데이트 시작
        /// </summary>
        private void StartRuntimeUpdate()
        {
            if (!_isRuntimeUpdateActive)
            {
                EditorApplication.update += UpdateRuntimeHighlight;
                _isRuntimeUpdateActive = true;
            }
        }
        
        /// <summary>
        /// 런타임 업데이트 중단
        /// </summary>
        private void StopRuntimeUpdate()
        {
            if (_isRuntimeUpdateActive)
            {
                EditorApplication.update -= UpdateRuntimeHighlight;
                _isRuntimeUpdateActive = false;
                _lastExecutingNodeID = 0;
                
                // 모든 노드의 하이라이팅 제거
                ClearAllRuntimeHighlights();
            }
        }
        
        /// <summary>
        /// 런타임 하이라이팅 업데이트
        /// </summary>
        private void UpdateRuntimeHighlight()
        {
            if (_actionRunner == null || _action == null)
                return;
                
            // 플레이 모드가 아니면 하이라이팅 제거
            if (!Application.isPlaying)
            {
                if (_lastExecutingNodeID != 0)
                {
                    ClearAllRuntimeHighlights();
                    _lastExecutingNodeID = 0;
                }
                return;
            }
            
            // 현재 실행 중인 노드 ID 가져오기
            int currentExecutingNodeID = _actionRunner.currentState == ActionRunnerState.Running ? _actionRunner.currentNodeID : 0;
            
            // 이전과 다르면 하이라이팅 업데이트
            if (currentExecutingNodeID != _lastExecutingNodeID)
            {
                // 이전 노드 하이라이팅 제거
                if (_lastExecutingNodeID != 0)
                {
                    var lastNodeView = FindNodeView(_lastExecutingNodeID);
                    lastNodeView?.SetRuntimeHighlight(false);
                }
                
                // 새 노드 하이라이팅 적용
                if (currentExecutingNodeID != 0)
                {
                    var currentNodeView = FindNodeView(currentExecutingNodeID);
                    currentNodeView?.SetRuntimeHighlight(true);
                }
                
                _lastExecutingNodeID = currentExecutingNodeID;
            }
        }
        
        /// <summary>
        /// 모든 노드의 런타임 하이라이팅 제거
        /// </summary>
        private void ClearAllRuntimeHighlights()
        {
            foreach (var element in graphElements)
            {
                if (element is NodeView nodeView)
                {
                    nodeView.SetRuntimeHighlight(false);
                }
            }
        }
        
        /// <summary>
        /// 키보드 이벤트 처리 (Ctrl+C, Ctrl+V)
        /// </summary>
        private void OnKeyDown(KeyDownEvent evt)
        {
            if (Application.isPlaying) return;
            
            // Ctrl+C (복사)
            if (evt.keyCode == KeyCode.C && evt.ctrlKey)
            {
                CopySelectedNodes();
                evt.StopPropagation();
                evt.PreventDefault();
            }
            // Ctrl+V (붙여넣기)
            else if (evt.keyCode == KeyCode.V && evt.ctrlKey)
            {
                PasteNodes();
                evt.StopPropagation();
                evt.PreventDefault();
            }
        }
        
        /// <summary>
        /// 선택된 노드들을 복사
        /// </summary>
        private void CopySelectedNodes()
        {
            if (_action == null) return;
            
            var selectedNodes = new List<BaseNode>();
            
            // 선택된 NodeView들에서 BaseNode 추출
            foreach (var element in selection)
            {
                if (element is NodeView nodeView)
                {
                    selectedNodes.Add(nodeView.Node);
                }
            }
            
            if (selectedNodes.Count == 0) return;
            
            _copiedNodes.Clear();
            
            // 선택된 노드들을 복사 (딥 카피)
            foreach (var node in selectedNodes)
            {
                var copiedNode = CloneNode(node);
                if (copiedNode != null)
                {
                    _copiedNodes.Add(copiedNode);
                }
            }
        }
        
        /// <summary>
        /// 복사된 노드들을 붙여넣기
        /// </summary>
        private void PasteNodes()
        {
            if (_action == null || _copiedNodes.Count == 0) return;
            
            // 기존 선택 해제
            ClearSelection();
            
            var newNodes = new List<BaseNode>();
            var oldToNewIdMap = new Dictionary<int, int>();
            
            // 노드들을 새로운 ID로 생성
            foreach (var copiedNode in _copiedNodes)
            {
                var newNode = CloneNode(copiedNode);
                if (newNode != null)
                {
                    // 새로운 ID 생성
                    var oldId = newNode.id;
                    newNode.id = System.Guid.NewGuid().GetHashCode();
                    oldToNewIdMap[oldId] = newNode.id;
                    
                    // 위치를 약간 오프셋 (오른쪽 아래로 30픽셀)
                    newNode.position += new Vector2(30f, 30f);
                    
                    // 자식 연결 정보는 일단 클리어 (나중에 복원)
                    newNode.childrenID.Clear();
                    
                    _action.nodes.Add(newNode);
                    newNodes.Add(newNode);
                }
            }
            
            // 자식 연결 관계 복원 (복사된 노드들 간의 연결만)
            for (int i = 0; i < _copiedNodes.Count; i++)
            {
                var originalNode = _copiedNodes[i];
                var newNode = newNodes[i];
                
                foreach (var childId in originalNode.childrenID)
                {
                    if (oldToNewIdMap.ContainsKey(childId))
                    {
                        newNode.childrenID.Add(oldToNewIdMap[childId]);
                    }
                }
            }
            
            // UI에 새 노드들 추가
            foreach (var newNode in newNodes)
            {
                var nodeView = CreateNodeViewForPaste(newNode);
                AddToSelection(nodeView); // 새로 생성된 노드들 선택
            }
            
            // Edge 다시 그리기 (새로운 연결들 표시)
            DrawEdge();
            
            EditorUtility.SetDirty(_actionRunner);
        }
        
        /// <summary>
        /// 노드를 복제 (딥 카피)
        /// </summary>
        private BaseNode CloneNode(BaseNode original)
        {
            if (original == null) return null;
            
            try
            {
                BaseNode clonedNode;
                
                // 노드 타입에 따라 새 인스턴스 생성
                if (original is BehaviorNode originalBehavior)
                {
                    var newBehavior = new BehaviorNode();
                    newBehavior.id = originalBehavior.id;
                    newBehavior.position = originalBehavior.position;
                    newBehavior.title = originalBehavior.title;
                    newBehavior.childrenID = new List<int>(originalBehavior.childrenID);
                    
                    // SerializeReference 객체를 수동으로 복제
                    if (originalBehavior.behavior != null)
                    {
                        newBehavior.behavior = DeepCloneBehavior(originalBehavior.behavior);
                    }
                    
                    clonedNode = newBehavior;
                }
                else if (original is ConditionNode originalCondition)
                {
                    var newCondition = new ConditionNode();
                    newCondition.id = originalCondition.id;
                    newCondition.position = originalCondition.position;
                    newCondition.title = originalCondition.title;
                    newCondition.childrenID = new List<int>(originalCondition.childrenID);
                    
                    // SerializeReference 객체를 수동으로 복제
                    if (originalCondition.condition != null)
                    {
                        newCondition.condition = DeepCloneCondition(originalCondition.condition);
                    }
                    
                    clonedNode = newCondition;
                }
                else
                {
                    Debug.LogError($"알 수 없는 노드 타입: {original.GetType()}");
                    return null;
                }
                
                return clonedNode;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"노드 복제 실패: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// IBehavior 객체를 깊은 복사
        /// </summary>
        private IBehavior DeepCloneBehavior(IBehavior original)
        {
            if (original == null) return null;
            
            try
            {
                // 리플렉션을 사용해서 새 인스턴스 생성
                var originalType = original.GetType();
                var cloned = (IBehavior)Activator.CreateInstance(originalType);
                
                // 모든 필드를 복사
                var fields = originalType.GetFields(System.Reflection.BindingFlags.Instance | 
                                                   System.Reflection.BindingFlags.Public | 
                                                   System.Reflection.BindingFlags.NonPublic);
                
                foreach (var field in fields)
                {
                    var value = field.GetValue(original);
                    
                    // SerializeReference 배열인 경우 (예: Sequencer의 behaviors)
                    if (field.FieldType.IsArray && field.FieldType.GetElementType().IsInterface)
                    {
                        if (value is Array originalArray)
                        {
                            var elementType = field.FieldType.GetElementType();
                            var clonedArray = Array.CreateInstance(elementType, originalArray.Length);
                            
                            for (int i = 0; i < originalArray.Length; i++)
                            {
                                var element = originalArray.GetValue(i);
                                if (element is IBehavior behaviorElement)
                                {
                                    clonedArray.SetValue(DeepCloneBehavior(behaviorElement), i);
                                }
                                else if (element is ICondition conditionElement)
                                {
                                    clonedArray.SetValue(DeepCloneCondition(conditionElement), i);
                                }
                                else if (element != null)
                                {
                                    // 다른 타입의 경우 기본 복사
                                    clonedArray.SetValue(element, i);
                                }
                            }
                            field.SetValue(cloned, clonedArray);
                        }
                    }
                    else
                    {
                        // 일반 필드는 그대로 복사
                        field.SetValue(cloned, value);
                    }
                }
                
                return cloned;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Behavior 복제 실패: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// ICondition 객체를 깊은 복사
        /// </summary>
        private ICondition DeepCloneCondition(ICondition original)
        {
            if (original == null) return null;
            
            try
            {
                // 리플렉션을 사용해서 새 인스턴스 생성
                var originalType = original.GetType();
                var cloned = (ICondition)Activator.CreateInstance(originalType);
                
                // 모든 필드를 복사
                var fields = originalType.GetFields(System.Reflection.BindingFlags.Instance | 
                                                   System.Reflection.BindingFlags.Public | 
                                                   System.Reflection.BindingFlags.NonPublic);
                
                foreach (var field in fields)
                {
                    var value = field.GetValue(original);
                    field.SetValue(cloned, value);
                }
                
                return cloned;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Condition 복제 실패: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// NodeView를 생성하고 반환 (붙여넣기용)
        /// </summary>
        private NodeView CreateNodeViewForPaste(BaseNode node)
        {
            NodeView nodeView = new NodeView(node, node.id == _action.rootID)
            {
                OnNodeSelected = OnNodeSelectionChanged,
                OnNodeRootSet = OnNodeRootSet,
                OnNodeTypeChanged = OnNodeTypeChanged
            };
            AddElement(nodeView);
            return nodeView;
        }
    }
}