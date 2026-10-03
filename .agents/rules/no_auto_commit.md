---
name: No Auto Commit
description: Prevents the agent from automatically staging or committing code without user review.
trigger: always_on
---

# Code Review & Commit Guardrails

- **Never commit automatically**: Do not run "git add", "git commit", or "git push" autonomously on behalf of the user.
- **Leave changes unstaged**: After modifying or creating files, leave them unstaged in the working directory.
- **User Review**: Always notify the user that the files have been generated/modified and prompt them to review the code and perform the commit themselves.
