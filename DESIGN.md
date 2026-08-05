# Design

<!-- impeccable:design-schema 1 -->

## Surface

Acervo web app (Operate mode with Read immersion on article pages)

## Visual world

Private academic lending library — calm paper surfaces, deep forest accent, gold citation signals. Discovery feels curated, not dashboard-like.

## Typography

- Display / UI: Bricolage Grotesque
- Reading body: Source Serif 4
- Measure ~68ch in the reader

## Color

### Light
- Canvas `#F1F3F0`, surface `#FCFDFB`, ink `#1C1F1E`
- Accent forest `#1F4D3A`, atmosphere green glows only; signal gold `#8A6A1F` for badges

### Dark
- Canvas `#121314`, surface `#1A1B1D`, ink `#EEEBE6` (neutral graphite)
- Accent fill same as light `#1F4D3A` / fg `#F4F8F5`; readable ink `#A8C9B8`
- Atmosphere glows green-only (`#1E2823` / `#243029`); signal gold kept for citation badges only

## Motion

Page enter (blur + lift), staggered list rise. Respects `prefers-reduced-motion`.

## Product mapping

Home search → results with citation/review/recent signals → reader (extracted full text as typographic reading, theme, highlights, AI on body, citations) → trails → library (favorites, collections, history, recommendations).
