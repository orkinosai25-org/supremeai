# [SEC-01] Automated AI vulnerability scanning in CI pipeline

**Labels:** `enhancement`, `security`, `priority: high`

## User Story

**As a** security engineer,
**I want** AI to scan code for vulnerabilities automatically in CI,
**so that** security issues are caught early.

## Acceptance Criteria

- [ ] Scans complete in < 3 minutes per PR
- [ ] CVE severity is labelled (Critical/High/Medium/Low)
- [ ] Critical findings block merge by default (configurable per repo)
- [ ] Scan results posted as PR comments with remediation links
- [ ] SBOM generated for every build

## Story Details

| Field | Value |
|-------|-------|
| **Epic** | Security & Compliance |
| **Priority** | High |
| **Story Points** | 5 |
| **ID** | SEC-01 |
