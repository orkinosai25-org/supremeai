# [SEC-04] AI detection of secrets accidentally committed to source control

**Labels:** `enhancement`, `security`, `priority: high`

## User Story

**As a** developer,
**I want** AI to detect secrets accidentally committed to source control,
**so that** credentials are never exposed.

## Acceptance Criteria

- [ ] Detection runs on every commit and PR (pre-receive hook + CI)
- [ ] Offending commit is blocked before it reaches the remote
- [ ] Credential rotation runbook is suggested for any detected secret
- [ ] Historical secret scanning available for existing branches
- [ ] Detection covers API keys, passwords, certificates, and tokens

## Story Details

| Field | Value |
|-------|-------|
| **Epic** | Security & Compliance |
| **Priority** | High |
| **Story Points** | 3 |
| **ID** | SEC-04 |
