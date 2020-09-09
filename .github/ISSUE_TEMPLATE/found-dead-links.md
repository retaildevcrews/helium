---
name: Found Dead Links
about: Dead link
title: 'Dead link(s) found in markdown files'
labels: 'Documentation'
assignees: ''

---

To find the dead links, run the following commands in the top-level directory:
- npm install -g markdown-link-check
- find . -name \\\*.md -exec markdown-link-check -c mlc_config.json {} \\\;