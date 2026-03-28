#!/usr/bin/env bash
# create-issues.sh — Bulk-creates all SupremeAI DevOps user story issues via the GitHub CLI.
#
# Prerequisites:
#   - GitHub CLI (gh) installed and authenticated: https://cli.github.com
#   - Run from the repository root:  bash scripts/create-issues.sh
#
# Usage:
#   bash scripts/create-issues.sh [--repo OWNER/REPO] [--dry-run]
#
# Options:
#   --repo OWNER/REPO   Target repository (default: current repo detected by gh)
#   --dry-run           Print the gh commands without executing them

set -euo pipefail

REPO_FLAG=""
DRY_RUN=false

while [[ $# -gt 0 ]]; do
  case "$1" in
    --repo)   REPO_FLAG="--repo $2"; shift 2 ;;
    --dry-run) DRY_RUN=true; shift ;;
    *) echo "Unknown option: $1" >&2; exit 1 ;;
  esac
done

create_issue() {
  local title="$1"
  local body_file="$2"
  local labels="$3"

  local cmd="gh issue create ${REPO_FLAG} --title \"${title}\" --body-file \"${body_file}\" --label \"${labels}\""

  if [[ "${DRY_RUN}" == "true" ]]; then
    echo "[DRY-RUN] ${cmd}"
  else
    echo "Creating: ${title}"
    gh issue create ${REPO_FLAG} \
      --title "${title}" \
      --body-file "${body_file}" \
      --label "${labels}" || echo "  WARNING: failed to create '${title}'"
  fi
}

ISSUES_DIR="$(dirname "$0")/../docs/issues"

# ---------------------------------------------------------------------------
# Epic 1 — Infrastructure & Cloud Management
# ---------------------------------------------------------------------------
create_issue "[INF-01] Provision cloud infrastructure using AI-generated IaC templates"     "${ISSUES_DIR}/inf_01.md"   "enhancement,infrastructure,priority: high"
create_issue "[INF-02] AI-powered resource utilisation analysis and auto-scaling"            "${ISSUES_DIR}/inf_02.md"   "enhancement,infrastructure,priority: high"
create_issue "[INF-03] AI-powered infrastructure drift detection"                            "${ISSUES_DIR}/inf_03.md"   "enhancement,infrastructure,priority: medium"
create_issue "[INF-04] AI multi-cloud resource placement recommendations"                    "${ISSUES_DIR}/inf_04.md"   "enhancement,infrastructure,priority: medium"
create_issue "[INF-05] Automated AI-driven infrastructure patching"                          "${ISSUES_DIR}/inf_05.md"   "enhancement,infrastructure,security,priority: high"

# ---------------------------------------------------------------------------
# Epic 2 — CI/CD Pipeline Automation
# ---------------------------------------------------------------------------
create_issue "[CICD-01] AI code review and optimisation suggestions on PRs"                 "${ISSUES_DIR}/cicd_01.md"  "enhancement,ci/cd,priority: high"
create_issue "[CICD-02] AI-generated optimal CI/CD pipeline configurations"                  "${ISSUES_DIR}/cicd_02.md"  "enhancement,ci/cd,priority: high"
create_issue "[CICD-03] AI build failure prediction before pipeline execution"               "${ISSUES_DIR}/cicd_03.md"  "enhancement,ci/cd,priority: medium"
create_issue "[CICD-04] AI auto-fix for trivial lint and style CI failures"                  "${ISSUES_DIR}/cicd_04.md"  "enhancement,ci/cd,priority: low"
create_issue "[CICD-05] AI-generated release notes from commit history"                      "${ISSUES_DIR}/cicd_05.md"  "enhancement,ci/cd,priority: medium"

# ---------------------------------------------------------------------------
# Epic 3 — Monitoring & Alerting
# ---------------------------------------------------------------------------
create_issue "[MON-01] AI alert correlation and automated root-cause analysis"               "${ISSUES_DIR}/mon_01.md"   "enhancement,monitoring,priority: high"
create_issue "[MON-02] AI-powered anomaly detection for application metrics"                 "${ISSUES_DIR}/mon_02.md"   "enhancement,monitoring,priority: high"
create_issue "[MON-03] Automated AI incident report generation"                              "${ISSUES_DIR}/mon_03.md"   "enhancement,monitoring,priority: medium"
create_issue "[MON-04] AI-powered SLO breach prediction with 30-minute lead time"           "${ISSUES_DIR}/mon_04.md"   "enhancement,monitoring,priority: high"

# ---------------------------------------------------------------------------
# Epic 4 — Security & Compliance
# ---------------------------------------------------------------------------
create_issue "[SEC-01] Automated AI vulnerability scanning in CI pipeline"                   "${ISSUES_DIR}/sec_01.md"   "enhancement,security,priority: high"
create_issue "[SEC-02] AI continuous compliance auditing against policy frameworks"          "${ISSUES_DIR}/sec_02.md"   "enhancement,security,compliance,priority: high"
create_issue "[SEC-03] AI-powered vulnerability prioritisation by business risk"             "${ISSUES_DIR}/sec_03.md"   "enhancement,security,priority: high"
create_issue "[SEC-04] AI detection of secrets accidentally committed to source control"     "${ISSUES_DIR}/sec_04.md"   "enhancement,security,priority: high"

# ---------------------------------------------------------------------------
# Epic 5 — Testing & Quality Assurance
# ---------------------------------------------------------------------------
create_issue "[QA-01] AI test case generation from user stories"                             "${ISSUES_DIR}/qa_01.md"    "enhancement,testing,priority: high"
create_issue "[QA-02] AI-powered intelligent test selection for changed code"                "${ISSUES_DIR}/qa_02.md"    "enhancement,testing,priority: medium"
create_issue "[QA-03] AI detection of flaky tests with suggested fixes"                      "${ISSUES_DIR}/qa_03.md"    "enhancement,testing,priority: medium"

# ---------------------------------------------------------------------------
# Epic 6 — Deployment & Release Management
# ---------------------------------------------------------------------------
create_issue "[DEP-01] AI optimal deployment window recommendations"                         "${ISSUES_DIR}/dep_01.md"   "enhancement,deployment,priority: medium"
create_issue "[DEP-02] Intelligent AI canary deployments with automated rollback"            "${ISSUES_DIR}/dep_02.md"   "enhancement,deployment,priority: high"
create_issue "[DEP-03] AI-generated deployment runbooks per service per release"             "${ISSUES_DIR}/dep_03.md"   "enhancement,deployment,priority: medium"

# ---------------------------------------------------------------------------
# Epic 7 — Observability & Performance
# ---------------------------------------------------------------------------
create_issue "[OBS-01] AI automated log analysis and actionable insights"                    "${ISSUES_DIR}/obs_01.md"   "enhancement,observability,priority: high"
create_issue "[OBS-02] AI performance optimisation suggestions from profiling data"          "${ISSUES_DIR}/obs_02.md"   "enhancement,observability,priority: medium"
create_issue "[OBS-03] AI-powered capacity planning recommendations"                         "${ISSUES_DIR}/obs_03.md"   "enhancement,observability,priority: medium"

# ---------------------------------------------------------------------------
# Epic 8 — MLOps & AI Model Management
# ---------------------------------------------------------------------------
create_issue "[ML-01] Automated A/B testing for AI model deployments"                        "${ISSUES_DIR}/ml_01.md"    "enhancement,mlops,priority: high"
create_issue "[ML-02] Automated model performance monitoring and retraining triggers"        "${ISSUES_DIR}/ml_02.md"    "enhancement,mlops,priority: high"
create_issue "[ML-03] Automated feature store management with lineage tracking"              "${ISSUES_DIR}/ml_03.md"    "enhancement,mlops,priority: medium"

# ---------------------------------------------------------------------------
# Epic 9 — Collaboration & Documentation
# ---------------------------------------------------------------------------
create_issue "[COL-01] AI automatic documentation updates on infrastructure changes"        "${ISSUES_DIR}/col_01.md"   "enhancement,documentation,priority: medium"
create_issue "[COL-02] AI-generated sprint reports and metrics summaries"                    "${ISSUES_DIR}/col_02.md"   "enhancement,documentation,priority: low"
create_issue "[COL-03] AI contextual guidance chatbot for DevOps task onboarding"           "${ISSUES_DIR}/col_03.md"   "enhancement,documentation,priority: medium"

echo ""
echo "Done. 33 issues processed."
