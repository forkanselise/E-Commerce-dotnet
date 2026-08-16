---
name: nexus-design-system
description: Guidelines and tokens for implementing the Glassmorphic Artisan + Tech aesthetic for Nexus Bakery & Tech.
---

# Nexus Bakery & Tech — Design System & Tokens

## Core Aesthetic: Glassmorphic Artisan + Tech
Blend warm artisan culinary warmth (glowing amber, crust gold, warm darks) with sleek high-tech precision electronics aesthetics (deep obsidian, cyan/emerald glow accents, frosted glass layers).

### 1. Color Palette & CSS Variables
```css
:root {
  /* Backgrounds */
  --bg-base: #0B0D13;
  --bg-surface: #131722;
  --bg-surface-elevated: #1A2030;
  
  /* Glassmorphism */
  --glass-bg: rgba(19, 23, 34, 0.75);
  --glass-bg-hover: rgba(26, 32, 48, 0.85);
  --glass-border: rgba(255, 255, 255, 0.08);
  --glass-border-glow: rgba(245, 158, 11, 0.25);
  --glass-blur: blur(16px);
  
  /* Artisan Amber / Gold (Warm Culinary) */
  --color-amber-400: #FBBF24;
  --color-amber-500: #F59E0B;
  --color-amber-600: #D97706;
  --color-gold-glow: rgba(245, 158, 11, 0.35);
  
  /* Tech Cyan & Indigo Accents */
  --color-cyan-400: #22D3EE;
  --color-cyan-500: #06B6D4;
  --color-indigo-500: #6366F1;
  --color-emerald-400: #34D399;
  
  /* Typography & Text */
  --text-primary: #F9FAFB;
  --text-secondary: #9CA3AF;
  --text-muted: #6B7280;
  
  /* Shadows & Elevation */
  --shadow-sm: 0 2px 8px rgba(0, 0, 0, 0.3);
  --shadow-lg: 0 10px 30px rgba(0, 0, 0, 0.5);
  --shadow-glow: 0 0 25px rgba(245, 158, 11, 0.15);
  
  /* Borders & Radius */
  --radius-sm: 8px;
  --radius-md: 12px;
  --radius-lg: 20px;
  --radius-full: 9999px;
}
```

### 2. UI Component Rules
- **Buttons**: Rounded-md/full with subtle gradient overlays, hover translateY(-2px), and glowing borders.
- **Cards**: Translucent backgrounds with backdrop-filter: blur(16px), 1px subtle white/gold border.
- **Badges**: Pill-shaped with colored glows for stock status (`In Stock`, `Low Stock`, `Subscriber Only`).
- **Icons**: Use `lucide-react` with consistent 18px / 20px sizes and accent colors.
- **Animations**: Smooth 200ms-300ms ease-out cubic-bezier transitions on all hover states.
