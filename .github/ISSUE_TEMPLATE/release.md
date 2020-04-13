---
name: Release Template
about: Verify code is ready to release
title: ''
labels: Release
assignees: ''

---

This checklist is for verifing the release is ready to publish and published correctly.

## Release
- Title / Repo
- vx.x.x.x

### Validation
- [ ] All packages up to date (or task created)
- [ ] Remove unused packages
- [ ] Code Version updated
- [ ] Code Review completed
- [ ] All existing automated tests (unit and e2e) pass successfully, new tests added as needed
- [ ] Code changes checked into master
- [ ] Sync github actions from master template
- [ ] Existing documentation is updated (readme, .md's)
- [ ] New documentation needed to support the change is created
- [ ] CI completes successfully
- [ ] CD completes successfully
- [ ] Smoke test deployed for 48 hours

### Release
- [ ] Resolve to-do from code
- [ ] Verify all new libraries and dependencies are customer approved
- [ ] Tag repo with version tag
- [ ] Ensure CI-CD runs correctly
- [ ] Validate e2e testing
- [ ] Close Release Task
