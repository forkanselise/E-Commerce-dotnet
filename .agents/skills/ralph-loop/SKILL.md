---
name: ralph-loop
description: Defines the autonomous Ralph Loop methodology for executing tasks. The AI uses external files as the source of truth, works in focused iterations, and relies on fresh context.
trigger: always_on
---

# The Ralph Loop (Autonomous Iteration)

When executing long-running or complex tasks, you MUST follow the **Ralph Loop** methodology:

## 1. External Memory as Source of Truth
- Do not rely on long conversation history, as context windows degrade.
- Always read your current tasks, requirements, and progress from local files (e.g., `implementation_plan.md`, `task.md`, or a specific `PROMPT.md`).
- Update these files immediately when you make progress or encounter blockers.

## 2. Autonomous Execution
- Treat each iteration as a highly focused, independent session.
- Work on ONE specific task from the file system.
- Write code, run tests, and verify your own work using bash commands.

## 3. Self-Correction & Persistence
- If tests fail or errors occur, document the error in your scratchpad or task file.
- Fix the error and run verification again.
- Continue looping autonomously until the specific micro-task is completed.

## 4. State Commits
- Once a micro-task is verified and working, check off the item in `task.md`.
- Exit the loop cleanly so the next iteration starts with a fresh, uncluttered context, reading the newly updated file state.

## 5. Iteration Limits (Safety Guard)
- **Max Iterations:** The Ralph Loop MUST NOT exceed a maximum of **10 iterations** for a single micro-task. 
- If you reach 10 iterations without successfully completing the task, stop looping immediately, document the blocker in your scratchpad, and request human assistance.
