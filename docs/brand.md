# Supreme Model T‑X — Brand Story & T‑Series Hierarchy

> *T‑Series Autonomous Governance Models*

---

## Overview

**Supreme Model T‑X** is the flagship product of the T‑Series Autonomous Governance Models — a family of purpose-built AI engines designed for multi-model orchestration, auditable decision-making, and sovereign AI governance.

The T‑Series naming system reflects a modular, hierarchical architecture where each model layer has a distinct responsibility in the evaluation and governance pipeline. The "T" stands for *Tier*, representing a structured progression from raw judgment through to fully autonomous, evidence-backed governance.

---

## T‑Series Model Hierarchy

| Model | Name | Role | Status |
|-------|------|------|--------|
| **Model T‑101** | Judgment Engine | Core multi-model evaluation — fans prompts out to all AI providers in parallel, scores responses, and selects the winning answer with a confidence score and plain-English rationale | ✅ Live |
| **Model T‑201** | Governance Intelligence Layer | Policy enforcement and API governance — stamps every response with `X-Api-Version` and `X-Request-Id`, enforces rate limits, and produces structured audit log entries | ✅ Live |
| **Model T‑301** | Benchmark Engine | Performance benchmarking and model scoring — computes accuracy, latency, cost, hallucination rate, and safety scores across task suites; persists benchmark history | ✅ Live |
| **Model T‑501** | Evidence Layer | Audit trail and evidentiary record — long-term immutable audit storage for regulatory compliance and change-control evidence | 🔜 Future |
| **Model T‑X** | Elite / Flagship | All T‑Series capabilities unified — the full-stack autonomous governance platform; the version shipped to enterprise and public-sector customers | ✅ Live |

---

## Brand Identity

### Name
**Supreme Model T‑X** — pronounced "Supreme Model Tee-Ex"

### Tagline
*T‑Series Autonomous Governance Models*

### Sub-brand Shield
The T‑Series sub-brand is represented by a shield motif enclosing the "T" mark — symbolising protection, governance, and institutional trust.

### Logo Files

| File | Description | Use Case |
|------|-------------|----------|
| `branding/model-t/logo-primary.svg` | Model T‑X primary logo — dark background | Dark-mode UIs, README on dark, hero sections |
| `branding/model-t/logo-monochrome.svg` | Monochrome variant | Print, single-colour embossing |
| `branding/model-t/shield-tseries.svg` | T‑Series sub-brand shield | Sub-brand contexts, model badges, API docs |
| `branding/model-t/logo-darkmode.svg` | Dark-mode optimised variant | Dark-mode interfaces with reduced glow effects |

---

## Colour Palette

The T‑Series palette extends the existing Supreme brand colours with T‑Series-specific accents:

| Token | Hex | Usage |
|-------|-----|-------|
| T‑Gold | `#FFD700` | Primary crown/flagship accent — Model T‑X |
| T‑Emerald | `#059669` | Judgment Engine accent — Model T‑101 |
| T‑Cyan | `#00E5FF` | Governance Intelligence accent — Model T‑201 |
| T‑Violet | `#7C3AED` | Benchmark Engine accent — Model T‑301 |
| T‑Steel | `#94A3B8` | Evidence Layer accent — Model T‑501 |
| T‑Slate | `#1E293B` | Primary dark background |

---

## Usage Rules

1. **Always use "Supreme Model T‑X"** (with the non-breaking hyphen `‑`) when referring to the flagship product — never abbreviate to "T-X" in user-facing copy without context.
2. **Use the full model name** (e.g. "Model T‑101 Judgment Engine") on first reference in documentation; subsequent references may use the short form "T‑101".
3. **Do not alter routing paths** — all API endpoints remain under `/supreme/*` and `/api/ai/*` for backward compatibility.
4. **The crown wordmark** remains in the UI header SVG; it represents the flagship "supreme" positioning that predates and underpins the T‑Series launch.
5. **Governance banner copy**: when displaying the active decision mode in the UI, use *"Powered by Model T‑101 Judgment Engine"*.

---

## Architecture Mapping

```
Supreme Model T‑X (Flagship)
│
├── Model T‑101  Judgment Engine          ← POST /api/ai/supreme
│   Evaluates all models, scores, selects winner, returns rationale
│
├── Model T‑201  Governance Intelligence Layer  ← GovernanceMiddleware
│   X-Request-Id · X-Api-Version · Audit logs · Rate limiting
│
├── Model T‑301  Benchmark Engine         ← /supreme/judge · benchmark store
│   Accuracy · Latency · Cost · Hallucination · Safety scoring
│
└── Model T‑501  Evidence Layer           ← (future)
    Immutable audit trail · Regulatory evidence · GDPR/EU AI Act records
```

---

*See [`branding/brand-guidelines.md`](../branding/brand-guidelines.md) for full visual identity guidelines.*
