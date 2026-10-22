# Proposed Evaluation Methodology and Governance

**Status:** Proposal for design and expert review; not an implemented policy or operating council.  
**Revision date:** October 22, 2026.

This document describes a possible methodology for Supreme Model T‑X. A pilot should not begin until its human reviewers approve scope, rubric versions, access, rights, and safeguards.

## 1. Scope and claims

Evaluation results should address bounded questions about a named model and version, subject, dataset, rubric, harness, date, and conditions. They are not universal rankings, safety certifications, or guarantees of production readiness or legal compliance. Reports should describe factual reliability, unsupported claims, calibration and uncertainty, robustness, and appropriate safety behavior where relevant, along with known limitations.

## 2. Subject suites and expert review

Startup-owned subject suites are proposed for history, music, and cinema, with possible expansion after review.

| Suite | Example task dimensions | Design considerations |
|---|---|---|
| History | Source attribution, context, distinction between evidence and inference, treatment of competing interpretations | Record source provenance; reflect scholarly disagreement and avoid presenting a contested interpretation as the only valid account. |
| Music | Musical concepts and analysis, evidence for claims, contextual interpretation | Make cultural context explicit; separate assessable technical claims from plural judgments of meaning or quality. |
| Cinema | Narrative and technical analysis, factual attribution, support for interpretive claims | Distinguish checkable facts from interpretation; allow defensible readings to differ. |

Domain experts should review task validity, language and cultural assumptions, accessibility, and scoring guidance. Use sources only with appropriate rights and attribution. A score must not flatten cultural pluralism or imply one objective answer where a task is interpretive.

External benchmarks, including Terminal-Bench, are optional adapters—not substitutes for these subject suites. Before using any external material, verify the exact upstream dataset and version, license and permitted use, attribution requirements, and evaluation protocol. Do not imply endorsement, access, or an integration before it exists and is verified.

## 3. Rubrics, weights, and milestones

Each rubric version should document its intended construct, task sampling, dimensions, weight rationale, scoring anchors, evidence requirements, missing-data handling, critical-failure rules, and approval record. Weights should be chosen for the subject and use case, reviewed for construct validity and unintended incentives, and sum to 100%.

**Illustrative example only—not an approved rubric:** task performance 30%, factual support 25%, calibration 15%, robustness 15%, and appropriate safety behavior 15%. An example scale could run from 0 to 4, with written anchors for each dimension at each level. Actual dimensions, weights, anchors, and thresholds require expert approval before evaluation.

Report missing or unscorable evidence as missing with a reason; do not impute a passing result. State denominators and how aggregation handles incomplete results. A rubric may define non-compensable critical failure gates, so a strong average cannot offset a specified critical failure. Human review should examine borderline scores, critical failures, disagreements, and appeals. Preserve dimension scores and evidence alongside any weighted total.

Proposed progression is baseline → domain competence → robustness → human-reviewed guideline alignment. Each milestone needs an approved threshold, evidence standard, and review procedure. Thresholds describe evaluation outcomes; they do not alter a third-party model's parameters. A pass applies only to the recorded model/version, domain, date, rubric, dataset, harness, and settings.

## 4. Controlled model interviews and evaluator quality

A planned proprietary AI interviewer may issue controlled prompts and follow-ups, request verifiable support, and organize observable answers for scoring. It should not claim access to hidden chain-of-thought. AI judges assist the process but do not replace expert judgment.

Before using an interviewer in final assessment:

- train and tune it separately from protected held-out assessment data;
- obtain and document rights and consent for training and evaluation data; user queries are not automatically reusable;
- compare interviewer scores with independent expert ratings across relevant domains and model families;
- measure calibration, uncertainty, bias, and inter-rater agreement; document disagreement and use human adjudication;
- test for self-evaluation and model-family conflicts, and ensure evaluated models cannot influence their own assessment;
- version and record the evaluator, prompts, settings, and scoring instructions.

Training examples, prompts, and interviewer outputs must not leak held-out tests or compromise benchmark integrity. Report how test sets are protected and reviewed for contamination.

## 5. Reproducibility and evidence

For each run, record where applicable:

- model/provider identity, model version, evaluator version, date, and access conditions;
- task, dataset, and prompt versions; provenance, permissions, and held-out status;
- harness and software version, environment, settings, resource limits, and permitted network access;
- rubric version, weights, scoring instructions, raw dimension scores, missing data, uncertainty, and reviewer/adjudicator records;
- costs, failures, deviations, and artifacts needed to audit the result.

Use repeat runs or sampling where needed to characterize variance. Publish or share only evidence that can be shared lawfully and consistently with participant agreements. Preserve traceability from scorecard to permitted source evidence.

## 6. Human governance and model advice

### Proposed human council

The proposed council would include disciplinary and evaluation expertise proportionate to the work. Before a pilot, define transparent appointment criteria and terms; disclose conflicts and require recusal where appropriate; keep version histories for guidelines and rubrics; record decisions and rationale; and provide review, appeal, and revision routes. The council approves guidelines, rubrics, changes, and final milestone decisions. Membership and charter remain undecided.

### Advisory model council

Models may suggest test cases, critique a rubric, or propose roadmap options. Keep a provenance log and identify the model and prompt used. Human reviewers must assess proposals before adoption. The model council has no authority to alter a rubric or score, pass itself, grant access, or override a human decision. Agreement among models is not evidence by itself that a claim is true or safe.

## 7. Execution safeguards and impartiality

Terminal or code execution, if included, requires a purpose-built sandbox, least privilege, resource and time limits, and restricted network access. These are planned requirements, not claims of implemented controls.

Because the startup intends to train its own evaluator and assess other models, evaluation independence needs active safeguards. Separate evaluator development from final assessment, protect held-out sets, document conflicts and recusal, use independent expert calibration, retain versioned evidence, and allow human review and appeal. Disclose limitations and any relevant conflicts in reports.

## 8. Review before use

Before any pilot, the founder and designated human reviewers should approve: subject and intended use; rubric version and weights; critical failure gates and milestone thresholds; council appointments and conflict process; test-set and evaluator separation; model/data access; external benchmark permissions; execution safeguards; reporting and appeal arrangements; and the budget and cost-recording plan.
