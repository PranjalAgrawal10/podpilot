# Branding

Visual language aligned with the React app (`web/src/App.css`, `ThemeContext`).

## Theme modes

`ThemeContext` persists `podpilot_theme` as `light` or `dark` and sets `data-theme` on `<html>`.

## Colors (App shell)

| Token | Light | Dark |
|-------|-------|------|
| `--bg-primary` | `#f8f9fc` | `#0f1117` |
| `--bg-secondary` | `#ffffff` | `#1a1d29` |
| `--bg-sidebar` | `#1a1d29` | `#12141c` |
| `--text-primary` | `#1a1d29` | `#e9ecef` |
| `--text-secondary` | `#6c757d` | `#adb5bd` |
| `--text-sidebar` | `#e9ecef` | `#e9ecef` |
| `--border-color` | `#e3e6f0` | `#2d3142` |
| `--accent-color` | `#4e73df` | `#6c8cff` |
| `--accent-hover` | `#3d5fc4` | `#5a7af0` |
| `--shadow` | soft blue-gray | soft black |

## Marketing / Vite starter accents (`index.css`)

Optional accent for landing-style pages: `#aa3bff` (light) / `#c084fc` (dark). Prefer the App shell blue accent for product UI consistency.

## Typography

```css
font-family: 'Segoe UI', system-ui, -apple-system, sans-serif;
```

Use clear hierarchy: sidebar labels muted, page titles `text-primary`, links/actions `accent-color`.

## Motion

Theme transitions: `background-color` / `color` ≈ `0.3s`. Prefer subtle UI transitions over decorative glow.

## Logo / product name

Product name: **PodPilot**. In UI, weight the wordmark in the sidebar header; do not replace accent blue with generic purple-on-white marketing templates for the console.
