# SupremeAI — Brand Guidelines

## Overview

This document defines the visual identity and communication standards for **SupremeAI** — the AI-powered DevOps platform. Consistent application of these guidelines ensures a coherent, professional brand presence across all touchpoints.

---

## 1. Brand Personality

SupremeAI embodies four core attributes:

| Attribute | Description |
|-----------|-------------|
| **Intelligent** | Decisions backed by AI; every interaction feels smart and purposeful |
| **Supreme** | Category-defining quality — the highest standard in AI-powered DevOps |
| **Reliable** | Enterprise-grade stability and trust |
| **Bold** | Confident, forward-looking, unafraid to redefine norms |

---

## 2. Color Palette

### Primary Colors

| Name | Hex | RGB | Usage |
|------|-----|-----|-------|
| **Void** | `#0D0D1A` | 13, 13, 26 | Dark backgrounds, hero sections |
| **Deep Indigo** | `#1E1040` | 30, 16, 64 | Secondary dark surfaces |
| **Supreme Purple** | `#7C3AED` | 124, 58, 237 | Primary brand color, CTAs |
| **Electric Cyan** | `#00E5FF` | 0, 229, 255 | Accents, highlights, AI indicators |

### Secondary Colors

| Name | Hex | RGB | Usage |
|------|-----|-----|-------|
| **Sapphire** | `#0891B2` | 8, 145, 178 | Links, interactive elements |
| **Violet** | `#A78BFA` | 167, 139, 250 | Gradient midpoints, hover states |
| **Emerald** | `#10B981` | 16, 185, 129 | Success states, positive metrics |
| **Amber** | `#F59E0B` | 245, 158, 11 | Warnings, attention indicators |
| **Crimson** | `#EF4444` | 239, 68, 68 | Error states, critical alerts |

### Neutral Colors

| Name | Hex | RGB | Usage |
|------|-----|-----|-------|
| **Snow** | `#F8F9FF` | 248, 249, 255 | Light backgrounds |
| **Slate 100** | `#F1F5F9` | 241, 245, 249 | Light surface |
| **Slate 600** | `#475569` | 71, 85, 105 | Secondary text (light) |
| **Slate 800** | `#1E293B` | 30, 41, 59 | Primary text (light) |
| **White** | `#FFFFFF` | 255, 255, 255 | Primary text (dark) |

### Gradient Definitions

| Name | Definition | Usage |
|------|-----------|-------|
| **Brand Gradient** | `linear-gradient(135deg, #00E5FF 0%, #7C3AED 100%)` | Logo, hero text, key UI elements |
| **Dark Surface** | `linear-gradient(135deg, #0D0D1A 0%, #1E1040 100%)` | Card backgrounds, icon backgrounds |
| **Glow Cyan** | `radial-gradient(circle, #00E5FF33 0%, transparent 70%)` | AI activity indicators, hover glows |

---

## 3. Typography

### Font Stack

| Role | Typeface | Fallback | Weight |
|------|----------|---------|--------|
| **Display / Heading** | Inter | Segoe UI, Arial, sans-serif | 700, 800 |
| **Body** | Inter | Segoe UI, Arial, sans-serif | 400, 500 |
| **UI Labels / Caps** | Inter | Segoe UI, Arial, sans-serif | 600 (all-caps + letter-spacing) |
| **Code / Monospace** | JetBrains Mono | Fira Code, Consolas, monospace | 400, 500 |

> Inter is available free at [fonts.google.com/specimen/Inter](https://fonts.google.com/specimen/Inter).  
> JetBrains Mono is available free at [fonts.google.com/specimen/JetBrains+Mono](https://fonts.google.com/specimen/JetBrains+Mono).

### Type Scale

| Token | Size | Line Height | Usage |
|-------|------|-------------|-------|
| `display-xl` | 56px / 3.5rem | 1.1 | Hero headlines |
| `display-lg` | 48px / 3rem | 1.15 | Section titles |
| `h1` | 36px / 2.25rem | 1.2 | Page headings |
| `h2` | 28px / 1.75rem | 1.25 | Subsection headings |
| `h3` | 22px / 1.375rem | 1.3 | Card titles |
| `h4` | 18px / 1.125rem | 1.4 | Component headings |
| `body-lg` | 18px / 1.125rem | 1.6 | Lead paragraphs |
| `body` | 16px / 1rem | 1.6 | Body copy |
| `body-sm` | 14px / 0.875rem | 1.5 | Captions, secondary text |
| `label` | 12px / 0.75rem | 1.4 | Labels (uppercase, +1.5 letter-spacing) |
| `code` | 14px / 0.875rem | 1.6 | Code blocks, terminal output |

---

## 4. Logo

### Concept

The SupremeAI logo combines a **crown mark** — representing supremacy and excellence — with a **circuit/node motif** beneath it, symbolising AI intelligence and connectivity. Together they communicate the product's core promise: the supreme intelligence layer for DevOps.

### Files

| File | Background | Format | Use Cases |
|------|-----------|--------|-----------|
| `logos/logo-dark.svg` | Dark / transparent-dark | SVG | Dark-mode UIs, dark hero sections, README on dark |
| `logos/logo-light.svg` | Light / transparent-light | SVG | Light-mode UIs, print, documentation |
| `logos/icon.svg` | Dark background, rounded square | SVG | App icons, avatars, social media profile images |
| `favicon.svg` | Dark background, rounded square | SVG | Browser tab favicon |

### Clear Space

Maintain a minimum clear space equal to **the height of the crown mark** on all four sides of the logo. Never crowd the logo with other elements.

### Minimum Size

| Format | Minimum Width |
|--------|--------------|
| Wordmark (logo-dark / logo-light) | 160 px |
| Icon | 24 px |
| Favicon | 16 px |

### Incorrect Usage

- ❌ Do not recolor the logo outside the approved palette.
- ❌ Do not stretch or skew the logo.
- ❌ Do not place the dark logo on a light background (use `logo-light.svg`).
- ❌ Do not add drop shadows or extra effects.
- ❌ Do not rotate the logo.
- ❌ Do not place the logo on visually busy backgrounds without sufficient contrast.

---

## 5. Iconography

Use **outline-style icons** at 1.5 px stroke weight for UI contexts. Preferred icon library: [Heroicons](https://heroicons.com/) (MIT licence).

- Use **Electric Cyan** (`#00E5FF`) for AI-specific feature icons.
- Use **Supreme Purple** (`#7C3AED`) for core navigation and action icons.
- Use **Slate 600** (`#475569`) for secondary/supplementary icons.

---

## 6. Voice & Tone

| Context | Tone | Example |
|---------|------|---------|
| Marketing copy | Confident, visionary | "The DevOps intelligence layer your team has been waiting for." |
| Product UI | Concise, helpful | "Pipeline optimized. Build time reduced by 23 %." |
| Error messages | Direct, non-blaming | "Deployment failed. View the run log for details." |
| Documentation | Clear, instructive | "Run `supreme deploy` to trigger a release." |
| Release notes | Technical, celebratory | "v2.0 ships auto-scaling inference — 3× faster cold starts." |

### Writing Rules

- Write in **sentence case** for UI copy (not Title Case).
- Use the **active voice** wherever possible.
- Refer to the product as **SupremeAI** (one word, capital S, capital A, capital I).
- Avoid jargon unless writing for a technical audience who expects it.
- Use numbers for metrics (e.g., "3×", "99.9 %").

---

## 7. Imagery & Illustration Style

- **Photography:** Dark, high-contrast environments; server rooms with cool lighting; engineers in focused concentration.
- **Illustrations:** Geometric, vector-based; use brand gradients sparingly as accents.
- **Data visualisations:** Dark backgrounds with Electric Cyan and Violet data series; minimal grid lines; bold axis labels.
- **Terminal / code screenshots:** Dark terminal theme (Void background, Electric Cyan prompt, white code text).

---

## 8. Motion & Animation

- **Transitions:** 150–250 ms ease-in-out for micro-interactions; 400 ms for page transitions.
- **AI indicators:** Pulsing glow effect using `Glow Cyan` radial gradient, 1.5 s loop.
- **Loading states:** Indeterminate progress bar using Brand Gradient, 800 ms sweep.
- Respect `prefers-reduced-motion` — provide static fallbacks for all animations.

---

## 9. Accessibility

- All text on `Void` (`#0D0D1A`) backgrounds must meet WCAG AA contrast (minimum 4.5:1 for body, 3:1 for large text).
- **White** on `Void` achieves **21:1** ✓
- **Electric Cyan** on `Void` achieves **10.8:1** ✓
- **Supreme Purple** on `Snow` achieves **5.8:1** ✓
- Interactive elements must have a visible focus ring (2 px `Electric Cyan` outline).
- Never convey information through colour alone; pair with text labels or icons.

---

*Last updated: March 2026*
