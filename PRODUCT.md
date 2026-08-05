# Product

<!-- impeccable:product-schema 1 -->

## Platform

web

## Stack

delegated: React + Vite + TypeScript + Tailwind CSS for the frontend (confirmed earlier by the user with the existing C# ASP.NET backend). Backend remains the Acervo API (PsiArtigos.Api project) + PostgreSQL.

## Users

Primary user: a learner/researcher (initially framed around someone who struggles to find academic articles online) who wants to discover, organize, and understand papers without opening five academic search sites.

Situation: starting from a topic or curiosity (“quero aprender X”), not from a known DOI.

Job: find relevant articles quickly, read them comfortably, save them, and get AI help (summary, beginner explanation, learning path).

## Product Purpose

Acervo is a unified academic discovery and reading product — a “Netflix of articles” — that aggregates scholarly sources into one elegant interface and adds AI-assisted understanding and learning trails.

Success: the user can go from a topic prompt to a curated set of articles (and optionally a progressive learning trail) and read/save/summarize without leaving the app.

## Positioning

Unlike Google Scholar or PubMed alone, Acervo combines multi-source search, a reader-first experience, personal library (favorites/collections), and an AI learning trail that sequences articles from beginner to recent research — in one product surface.

## Operating Context

- Web app consuming Acervo API (`/api/search`, articles, library, learning-trails, insights, reading, recommendations)
- Dev auth via `X-User-Id` header until real auth lands
- PostgreSQL via Docker Compose for local data
- Academic sources currently: OpenAlex, arXiv, Crossref

## Capabilities and Constraints

Confirmed V1 capabilities to surface in the frontend:
- Home search: “O que você quer aprender hoje?” with topic suggestions
- Unified search results with citation/year/source signals
- Filters (year, language, citations, sources)
- Article detail + clean reader (dark mode, font size, highlights, favorites)
- In-app readable text: extract full text from PDF (or HTML/abstract fallback), cache, and render as typographic reading experience (`/api/articles/{id}/content`)
- AI: summarize, explain for beginners, translate (uses extracted text when available)
- Learning trail from natural-language prompt
- Favorites, collections, search history
- Citation export (APA, ABNT, BibTeX)
- Recommendations list (API-backed; quality depends on backend seeding)

Constraints / not V1 frontend blockers:
- Real auth / multi-device sync polish
- Citation graph visualization
- Semantic “similar articles” (backend not ready)
- Live LLM (local AI fallback acceptable for V1 demo)
- OCR for scanned PDFs; paywalled HTML bodies may not extract

## Brand Commitments

- Product name: **Acervo**
- Visual brief from user: clean, sophisticated **light** theme and matching refined **dark** theme; elegant, presentable, not generic AI-purple aesthetics
- Concept metaphor: Netflix-like discovery for academic articles (browse/search → select → immerse)

## Evidence on Hand

- Working backend API and domain model in `src/`
- Product vision from the founding conversation (Netflix of articles + AI trail)
- No final logo asset yet; wordmark typography may carry brand in V1
- Demo/synthetic article content may appear when APIs are empty; label clearly if fabricated for UI states

## Product Principles

1. Discovery before jargon — start from curiosity, not database syntax.
2. One surface for many sources — never send the user to five tabs to begin.
3. Reading is a first-class product, not an afterthought.
4. AI assists understanding and sequencing; it does not replace the paper.
5. Calm sophistication — the interface should feel like a modern private library, not a noisy dashboard.

## Accessibility & Inclusion

Target WCAG 2.1 AA contrast for text/controls in both themes; keyboard-reachable primary actions; respect `prefers-reduced-motion`.
