# Supreme Model T‑X — Investor Pitch

> **Discussion draft · revised October 22, 2026**
>
> This document distinguishes repository artifacts, founder-reported context, proposals, and future plans. It is not evidence of live operations, customer traction, external endorsement, or independent certification.

---

## Slide 1 — The proposition

**Supreme Model T‑X**

*Human-governed, multidisciplinary evaluation for AI models.*

We are proposing a platform to benchmark, evaluate, and grade models across subjects. Domain experts define the tasks and standards; reproducible evaluations and structured interviews produce evidence for human review. Orchestration and API components in the repository are potential enabling infrastructure, not the product's core purpose.

**Status:** Product direction and roadmap, not a claim that the proposed platform or councils are operational.

---

## Slide 2 — Why evaluation needs context

Models can sound confident while being mistaken, poorly calibrated, brittle under follow-up, or unsafe in a particular context. A single leaderboard score can conceal differences between domains, prompts, versions, and evaluation conditions.

Supreme Model T‑X aims to make those differences inspectable through:

- domain-specific tasks and rubrics authored and approved by people with relevant expertise;
- documented evidence, uncertainty, and limitations alongside scores;
- repeatable model/version comparisons with human review and appeal;
- carefully scoped findings—not universal claims that a model is safe or suitable for every use.

This is a proposed approach to contributing evidence to AI governance, not a claim to solve all AI risks or satisfy legal requirements.

---

## Slide 3 — Planned evaluation flow

```text
Human council approves versioned guidelines and rubrics
        ↓
Experts design subject tasks; approved external benchmark adapters are scoped
        ↓
Models are evaluated under recorded, reproducible conditions
        ↓
Controlled follow-up interviews probe observable answers and evidence
        ↓
Domain scorecards, weighted scores, uncertainty, and milestone decisions
        ↓
Human review, adjudication, and an appeal/revision route
        ↓
Advisory model council suggests tests and roadmap options
        ↓
Human council reviews and approves any roadmap or rubric changes
```

The human council retains final authority throughout. Model-generated suggestions are advisory and are logged with their provenance.

---

## Slide 4 — A multidisciplinary suite, built domain by domain

Dedicated history, music, and cinema suites are proposed as startup-owned work, with scope to add other subjects after expert and rights review.

| Proposed suite | Examples of evaluation questions | Important limits |
|---|---|---|
| **History** | Are claims attributed to appropriate sources and placed in context? Can the model explain competing interpretations and distinguish evidence from inference? | Sources can be incomplete or contested; a rubric should not erase legitimate scholarly disagreement. |
| **Music** | Can a model analyze musical structures or concepts and explain its reasoning with relevant evidence? Does it handle cultural context appropriately? | Judgments about meaning, quality, and tradition may be plural and culturally situated. |
| **Cinema** | Can a model analyze narrative and technical choices, and accurately attribute factual claims? | Interpretive readings can differ; factual attribution and subjective analysis should be scored separately. |

Task design should respect cultural pluralism, lawful source use, consent, and expert disagreement. Scores describe performance on the stated tasks and rubric, not the worth of a culture, artwork, or interpretation.

---

## Slide 5 — Terminal-Bench: one bounded track

Terminal-Bench is an external benchmark of terminal-task performance for AI agents. A future adapter may help test coding and terminal capabilities as one track within a broader evaluation program; it is not the whole platform and does not make DevOps the startup's purpose.

Any use is contingent on verifying the selected upstream version, benchmark terms, attribution, permissions, execution environment, and reproducibility requirements. Results should identify the exact dataset/version and harness used. History, music, and cinema are proposed subject suites, not Terminal-Bench extensions.

Planned safeguards for terminal evaluation include sandboxing, least privilege, resource limits, and restricted network access. These are requirements for a future pilot, not claims about safeguards already implemented.

Sources: [Terminal-Bench](https://www.tbench.ai/) and its [upstream repository](https://github.com/harbor-framework/terminal-bench).

---

## Slide 6 — A trained model interviewer, with human oversight

A future proprietary AI evaluator/interviewer is intended to ask controlled prompts and follow-ups, record observable answers, request verifiable support, and help organize grading evidence. It will not be represented as having access to hidden chain-of-thought, and its judgments will not replace expert review.

Before use in final assessment, the interviewer would need:

- separate training and held-out assessment data, with rights and consent reviewed;
- calibration against expert ratings, bias analysis, inter-rater agreement checks, and uncertainty reporting;
- tests for self-evaluation and model-family conflicts;
- versioned prompts, evaluator model, settings, and reproducible records;
- human adjudication of disagreement and an appeal/revision process.

User queries are not automatically training data. Evaluated models must not influence their own assessment.

---

## Slide 7 — Weighted scorecards and progressive milestones

Each approved domain rubric would state its dimensions, weights and rationale, scoring scale, evidence standard, and treatment of missing or ambiguous data. A weighted score summarizes performance under that rubric; it does not change the parameters or capabilities of a third-party model.

**Illustrative only — not an approved rubric:** a sample scorecard could weight task performance 30%, factual support 25%, calibration 15%, robustness 15%, and appropriate safety behavior 15% (100% total). Dimensions might use a documented 0–4 scale. Domain experts would set and validate actual criteria and weights before a pilot.

Critical failures may be non-compensable: a high average cannot cancel a defined critical failure. Missing evidence is reported as missing, not silently treated as success. Human reviewers inspect evidence, uncertainty, and any threshold decision.

Proposed milestones could progress from **baseline** → **domain competence** → **robustness** → **human-reviewed guideline alignment**. Thresholds and evidence requirements remain to be approved. Passing a milestone is limited to a specified model/version, domain, date, rubric, dataset, harness, and conditions; it is not a production-readiness guarantee or safety certification.

---

## Slide 8 — Human council and advisory model council

### Proposed human council

The council would include appropriately qualified disciplinary and evaluation expertise. Its proposed responsibilities are to author, review, and approve versioned guidelines; oversee testing; resolve escalations; and approve rubric, roadmap, and milestone changes. Appointment criteria, membership, conflicts and recusals, recorded decisions and rationale, review, and appeal mechanisms still need to be established.

### Proposed advisory model council

Models may propose tests, critique evaluation plans, and offer roadmap options. The council is supervised by the human council; it cannot independently alter rubrics or scores, pass itself, grant access, or override human decisions. Cross-model agreement is not proof of truth or safety. Any evaluated model is kept out of decisions about its own result.

Neither council is represented as already formed.

---

## Slide 9 — Independence, evidence, and limitations

Because the startup plans to train its own evaluator while evaluating other models, impartiality requires explicit safeguards: evaluator/model-family conflicts are disclosed; evaluator training is separated from held-out assessment; rubrics and scoring changes are versioned and human-approved; and results include enough audit evidence for review.

For each run, a future system should record model and evaluator versions, prompts, dataset provenance and version, harness, rubric, settings, date, costs, missing data, and adjudication evidence. Held-out tests should be protected from training leakage and benchmark contamination. Data use requires appropriate rights and consent. External task reuse requires a separate license/permission and attribution check.

Scores are bounded observations under documented conditions. They cannot guarantee behavior outside those conditions, certify universal safety, or establish legal compliance. Evidence may inform governance review but cannot replace it.

---

## Slide 10 — Pilot plan and collaboration ask

**Proposed, dependency-gated phases; no dates or outcomes are promised.**

1. **Scope and permission:** select a narrow subject/task set; confirm expert availability, data rights, benchmark terms, safety needs, and evaluation access.
2. **Design and baseline:** have human reviewers approve the first rubric and test plan; establish model/version records, controls, held-out data, and a baseline.
3. **Run and review:** conduct reproducible evaluations and controlled interviews; document evidence, limitations, disagreement, and costs; review results with experts.
4. **Revise or stop:** publish or share findings only with agreed permissions; address appeals and lessons; let the human council approve any next phase.

We seek expert feedback on rubric design, public task-design methodology, subject-expert participation, and calibration against expert ratings. These are discussion topics, subject to mutual agreement, rights review, and clear separation of each party's work and materials.

See [Outlier collaboration proposal](outlier-collaboration-proposal.md) for a concise draft and outreach message. No external communication is included or sent by this document.

---

## Slide 11 — What exists, what is proposed

The repository contains application and documentation artifacts, including API and orchestration-related material. Their presence is evidence of files in this repository only; it does not establish deployment, production operation, benchmark results, customers, revenue, partnerships, or traction.

**Founder-reported context:** the founder reports just starting expert/PhD-level work with Outlier involving Terminal-Bench. This statement is not independent verification of qualifications, employment, endorsement, partnership, access rights, or completed work. No external work product or confidential material is assumed to be available to the startup.

**Roadmap, not current claims:** multidisciplinary suites, benchmark adapters, trained interviewer, weighted milestone system, human council, model council, and associated safeguards are proposed future work.

---

## Slide 12 — Funding discussion

**Proposed seed target: £3 million.** This is a fundraising target, not money raised. No validated financial forecast, pricing, market sizing, or compute estimate is asserted here.

An **illustrative allocation** for discussion (subject to expert review and budget validation):

| Use | Share | Amount |
|---|---:|---:|
| Compute and evaluation infrastructure | 35% | £1.05m |
| Expert review, domain task design, and evaluation governance | 25% | £0.75m |
| Product and engineering capacity | 25% | £0.75m |
| Data rights, security, legal, and operating needs | 10% | £0.30m |
| Reserve | 5% | £0.15m |
| **Total** | **100%** | **£3.00m** |

Actual scope, sequencing, staffing, compute needs, pricing, and runway require a researched plan and founder approval. This is not a quote, commitment, or validated operating budget.

---

## Decisions needed before a pilot or fundraising use

- Who will serve on the proposed human council, and what appointment and conflict rules will apply?
- Which domain rubric, weights, critical-failure gates, and milestone thresholds are approved?
- What funding, pricing, and budget assumptions are supported by research?
- What model access, external benchmark permissions, data rights, and pilot conditions are confirmed?
- What traction, deployment, and founder background claims can be independently substantiated and are approved for disclosure?

---

*Supreme Model T‑X · Discussion draft · October 22, 2026*
