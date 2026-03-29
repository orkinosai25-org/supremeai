# SupremeAI Gemstone Color Palette

Generated from the color wheel, each color corresponds to a gemstone used in the brand.

## Primary Gemstone Colors

| Gemstone   | Hex       | RGB                | Usage                        |
|------------|-----------|--------------------|------------------------------|
| Emerald    | `#50C878` | rgb(80, 200, 120)  | **S** lettermark, primary CTA|
| Diamond    | `#E8F4F8` | rgb(232, 244, 248) | "ai" lettermark, highlights  |
| Gold       | `#FFD700` | rgb(255, 215, 0)   | Crown, borders, premium accents|
| Silver     | `#C0C0C0` | rgb(192, 192, 192) | Crown body, metallic text    |

## Secondary Gemstone Colors

| Gemstone   | Hex       | RGB                | Usage                        |
|------------|-----------|--------------------|------------------------------|
| Ruby       | `#E0115F` | rgb(224, 17, 95)   | Alerts, errors, "u" letter   |
| Sapphire   | `#0F52BA` | rgb(15, 82, 186)   | Links, info states, "p" letter|
| Amethyst   | `#9B59B6` | rgb(155, 89, 182)  | Backgrounds, accents         |
| Topaz      | `#FFC87C` | rgb(255, 200, 124) | Warm highlights              |
| Aquamarine | `#7FFFD4` | rgb(127, 255, 212) | Success states, teal accents |
| Rose Quartz| `#F4A7B9` | rgb(244, 167, 185) | Soft pink accents            |
| Tanzanite  | `#4B0082` | rgb(75, 0, 130)    | Deep purple backgrounds      |

## Gradient Definitions

```css
/* Mascot / Rainbow Gradient */
--gradient-rainbow: linear-gradient(135deg, #00B4DB, #7C3AED, #EC4899, #F472B6);

/* Gold Crown Gradient */
--gradient-gold: linear-gradient(180deg, #FFD700 0%, #FFA500 50%, #B8860B 100%);

/* Silver Crown Gradient */
--gradient-silver: linear-gradient(180deg, #E8E8E8 0%, #C0C0C0 50%, #808080 100%);

/* Diamond / Crystal Gradient */
--gradient-diamond: linear-gradient(135deg, #E8F4F8 0%, #A8D8EA 40%, #E8F4F8 70%, #FFFFFF 100%);

/* Dark Background */
--gradient-dark-bg: radial-gradient(ellipse at top, #0D1B2A 0%, #050A0E 100%);
```

## Semantic Color Tokens

```css
/* Brand */
--color-primary:   #50C878;   /* Emerald — brand green */
--color-secondary: #FFD700;   /* Gold    — premium */
--color-tertiary:  #9B59B6;   /* Amethyst — accent */

/* Background */
--color-bg-dark:   #070D14;
--color-bg-mid:    #0D1B2A;
--color-bg-light:  #F8FAFB;

/* Surface */
--color-surface-dark:  #111827;
--color-surface-mid:   #1E293B;
--color-surface-light: #FFFFFF;

/* Text */
--color-text-primary:   #F1F5F9;
--color-text-secondary: #94A3B8;
--color-text-on-light:  #0F172A;

/* State */
--color-success: #50C878;   /* Emerald */
--color-warning: #FFC87C;   /* Topaz   */
--color-error:   #E0115F;   /* Ruby    */
--color-info:    #0F52BA;   /* Sapphire */
```
