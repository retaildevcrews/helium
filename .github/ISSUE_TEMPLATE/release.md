---
name: 'Release Template'
about: Verify code is ready to release
title: "[DoDREVIEW]"
labels: Release
assignees: ''

---

This checklist is for verifing the release is ready to publish and published correctly.

## Release
- Title / Repo
- vx.x.x.x

### Validation
- [ ] Documentation updated as appropriate
- [ ] Run full code quality rules (all feedback resolved or task created)
- [ ] All packages up to date (or task created)
- [ ] Remove unused packages
- [ ] Code Version updated
- [ ] Code Reviews completed (all feedback resolved or task created)
- [ ] End to end smoke test for 48 hours


### Release
- [ ] Tag repo with version tag
- [ ] Ensure CI-CD runs correctly
- [ ] Close Release Task
