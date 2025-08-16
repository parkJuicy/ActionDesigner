# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a Unity project called "Action Designer" (AD) - a visual node-based action design system similar to behavior trees. The project implements a flowchart system where Action and Condition nodes can be connected to control Unity object behaviors.

## Architecture

### Core Components

- **ActionDesigner.Runtime**: Core runtime system
  - `Action.cs`: Main action graph container with node management
  - `BaseNode.cs`: Abstract base class for all nodes with serialization support
  - `MotionNode.cs`/`ConditionNode.cs`: Concrete node implementations
  - `ActionRunner.cs`: Runtime execution system

- **ActionDesigner.Editor**: Unity Editor tooling
  - `ActionDesignerEditor.cs`: Main editor window using UI Toolkit
  - `ActionView.cs`: Visual graph editor with node manipulation
  - `NodeView.cs`: Individual node visualization and interaction
  - `NodeSearchWindow.cs`: Node creation search interface

### Key Systems

1. **Node System**: Type-based dynamic node creation using reflection and SerializeReference
2. **Task System**: Pluggable task implementations (Debug, Wait, Parallel, Sequencer)
3. **Condition System**: Runtime condition evaluation (KeyPress, EndCondition, AlwaysTrue)
4. **Editor Integration**: Custom property drawers and UI Toolkit-based visual editor

### External Dependencies

- **SerializeReferenceExtensions**: Custom serialization support for polymorphic references
- **InputSystem**: Custom input handling system (from ExternalModule project)

## Development Commands

This is a Unity project - standard Unity workflows apply:

- Open project in Unity Editor (2021.3 or later)
- Build through Unity Editor: File → Build Settings
- No external build scripts or package managers detected
- Testing done through Unity Test Runner

## File Structure

- `Assets/ActionDesigner/Runtime/`: Core runtime code
- `Assets/ActionDesigner/Editor/`: Editor-only code and UI definitions
- `Assets/ActionDesigner/Editor/UIToolkit/`: UXML/USS files for editor UI
- `Assets/SerializeReferenceExtensions/`: Polymorphic serialization utilities

## Key Features Implemented

- Visual node-based action designer with drag-and-drop interface
- Runtime action execution system
- Condition evaluation system
- Parallel and sequential task execution
- Node search and creation system
- Title editing and precise node positioning
- Input system integration

## Recent Development Focus

Based on recent commits:
- Node precise positioning functionality
- Title editing without breaking child connections
- Parallel sequencer development
- Root node input capability