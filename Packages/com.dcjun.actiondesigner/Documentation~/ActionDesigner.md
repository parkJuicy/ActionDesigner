# Action Designer Documentation

## Overview

Action Designer is a node-based behavior tree system for Unity that allows you to visually design complex AI behaviors and state machines.

## Core Concepts

### Nodes
- **Behavior Nodes**: Execute actual tasks and actions
- **Condition Nodes**: Evaluate conditions and control flow

### Connections
- **Behavior → Condition**: After behavior completion, check condition
- **Condition → Behavior**: If condition is met, execute next behavior

### Execution Flow
1. Start from root node (must be a Behavior)
2. Execute current behavior
3. When behavior completes, evaluate connected conditions
4. Transition to next behavior based on condition results

## Editor Usage

### Opening the Editor
1. Select a GameObject with ActionRunner component
2. Open `Action Designer → Editor...` from menu

### Creating Nodes
1. Press **Space bar** in the editor
2. Search for and select desired node type
3. Node will be created at mouse position

### Connecting Nodes
1. Drag from output port (bottom) of parent node
2. Connect to input port (top) of child node
3. Valid connections will be highlighted in green

### Setting Root Node
1. Right-click on a Behavior node
2. Select "Set Root Node" from context menu

## API Reference

### IBehavior Interface
```csharp
public interface IBehavior
{
    void Start();           // Called when behavior begins
    bool Update(float deltaTime);  // Called each frame, return true when complete
    void End();            // Called when behavior completes normally
    void Stop();           // Called when behavior is interrupted
}
```

### ICondition Interface
```csharp
public interface ICondition
{
    bool Evaluate(float deltaTime);  // Return true if condition is met
    void Start();          // Called when condition evaluation begins
    void End();            // Called when condition evaluation ends
    void OnSuccess();      // Called when condition succeeds
}
```

### ActionRunner Methods
```csharp
public void StartAction();    // Begin action execution
public void StopAction();     // Stop action execution
public void PauseAction();    // Pause/resume action execution
```

## Examples

### Custom Behavior Example
```csharp
[System.Serializable]
public class MoveToTarget : IBehavior
{
    [SerializeField] private Transform target;
    [SerializeField] private float speed = 5f;
    
    private Transform transform;
    
    public void Start()
    {
        transform = ((MonoBehaviour)this).transform;
    }
    
    public bool Update(float deltaTime)
    {
        if (target == null) return true;
        
        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * speed * deltaTime;
        
        return Vector3.Distance(transform.position, target.position) < 0.1f;
    }
    
    public void End() { }
    public void Stop() { }
}
```

### Custom Condition Example
```csharp
[System.Serializable]
public class IsPlayerNearby : ICondition
{
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private LayerMask playerLayer = -1;
    
    public bool Evaluate(float deltaTime)
    {
        Collider[] players = Physics.OverlapSphere(
            transform.position, 
            detectionRange, 
            playerLayer
        );
        return players.Length > 0;
    }
    
    public void Start() { }
    public void End() { }
    public void OnSuccess() { }
}
```

## Best Practices

### Node Design
- Keep behaviors simple and focused on single tasks
- Use conditions for decision-making logic
- Design for reusability across different action trees

### Performance
- Avoid heavy computations in Update() methods
- Cache references in Start() when possible
- Use appropriate detection ranges for conditions

### Debugging
- Use runtime visualization to track execution flow
- Add debug logs in behavior methods for troubleshooting
- Test individual nodes before composing complex trees

## Troubleshooting

### Common Issues

**Nodes not connecting**
- Check connection rules: Behavior → Condition → Behavior
- Ensure nodes are valid (have assigned behavior/condition)

**Runtime execution stops**
- Verify root node is set and is a valid Behavior
- Check for missing implementations in custom nodes

**Editor not updating**
- Ensure ActionRunner component is present on selected GameObject
- Check console for compilation errors

### Performance Tips
- Limit condition evaluation frequency for expensive checks
- Use object pooling for frequently created/destroyed behaviors
- Profile behavior execution in complex trees