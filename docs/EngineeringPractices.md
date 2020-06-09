# Engineering Practices

## Definition of Done

- Code changes reviewed & signed off
- Existing documentation is updated (readme, .md's)
- New documentation needed to support the change is created
- Code changes checked into master
- All existing automated tests (unit and/or e2e) pass successfully, new tests added as needed
- CI completes successfully
- CD completes successfully
- Smoke test production deployment for minimum of 2 weeks
- New changes in smoke test for 48hrs (ongoing)
- Create task for required artifacts

Engineering Playbook [Definition of Done](https://github.com/microsoft/code-with-engineering-playbook/blob/master/team-agreements/definition-of-done/readme.md)

## Markdown (md files)

- Use [markdownlint](https://marketplace.visualstudio.com/items?itemName=DavidAnson.vscode-markdownlint) add-in for VS Code
- Use "-" for unordered lists
- Add a blank line before and after ``` for visualization

## Tool for Code Reviews

- CodeFlow extension for GitHub [Link](https://www.1eswiki.com/wiki/CodeFlow_integration_with_GitHub_Pull_Requests)

## Kanban Management Best Practices

### Triage

> All net-new issues need to be triaged, leverage Notes for discussions points as needed

- Create the issue in the appropriate board and template
- Add project to the issue (i.e. Helium) - this is automatic add to master board
- Add all relevant tags (language, enchancement, bug, design review, bug, etc)
- Do not add Size, Priority, Milestone, or Assignee
- All issues will be triaged at the end of the Standup call

### Backlog

> Once issue has been triaged, move into the Backlog

- Once issue is triaged, add the appropriate Priority Tag and add net-new tags
- Do not add size, milestone or assignee

### Sprint Backlog

> Issues identified during milestone planning will be shifted into Milestone backlog

- Need to review and update priority and add estimated sizing and milestone labels

### In Progress

> Issues that the Dev Team is actively working on

- Add assignee, size, and milestone tags & ensure all relevant tags are added
- If a design review is required, schedule meeting when moving issue to backlog
- If task is bigger than "Size:L", break into smaller tasks to ensure completion during week sprint

### PR Submitted / Review

> _Pull Requests to create or update code, documentation, templates, etc and issues that need reviewed

- Complete the PR Template (ensure original issue is closed or referenced)
- Assign reviewers, assign yourself, add Project board, and Milestone
- If issue has multiple issue to close and/or reference, report each reference/close # on seperate line to ensure correct link

### Done

- Issue is completed, no further action rquired
- Ensure task checklist is complete

### Burndown

> During the final sprint of the milestone, create a burndown column for issues pivotal to achieve goal
