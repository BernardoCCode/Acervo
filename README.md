# Acervo

Busca, leitura e organização de artigos acadêmicos em um só lugar — com leitor tipográfico, favoritos, coleções e trilhas de aprendizado assistidas por IA.

> Nome do produto na UI: **Acervo**. Repositório histórico: PsiArtigos.

## Demo

| | |
|---|---|
| **URL** | _em breve — cole o link do deploy aqui_ |
| **Conta de teste** | _email / senha após o deploy_ |
| **Sem conta** | Na tela `/entrar`, clique em **Explorar sem conta** |

O modo visitante entra com um usuário demo compartilhado — ideal pra recruta testar rápido. Para histórico/favoritos pessoais, use criar conta.

## O problema

Encontrar papers bons costuma significar abrir várias abas (Scholar, PubMed, arXiv…), cair em paywall e ler PDF ruim no navegador. O Acervo agrega fontes de acesso aberto, extrai texto legível e deixa salvar/organizar o que importa.

## Stack

| Camada | Tecnologia |
|--------|------------|
| Frontend | React 19, TypeScript, Vite, Tailwind CSS 4, React Router |
| Backend | ASP.NET Core (.NET 10), Clean Architecture |
| Banco | PostgreSQL 17 (Docker) |
| Cache | Redis opcional (fallback em memória) |
| Fontes | OpenAlex, Europe PMC, Semantic Scholar, arXiv |
| IA | OpenAI-compatible (opcional; sem chave usa fallback local) |

## Funcionalidades

- Busca multi-fonte com filtros (ano, idioma, citações, fontes)
- Leitor in-app a partir de PDF aberto (destaques, progresso, tema, tamanho de fonte)
- Favoritos, coleções, histórico e recomendações
- Trilhas de aprendizado a partir de um prompt
- Resumo / explicação para iniciantes / tradução (só aparece na UI com chave de IA)
- Citações APA, ABNT e BibTeX
- Auth JWT (registro e login)

## Como rodar local

### Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### 1. Infra (Postgres + Redis)

Na raiz do repositório:

```bash
cp .env.example .env
docker compose -f docker-compose.dev.yml up -d
```

### 2. API

```bash
dotnet run --project src/PsiArtigos.Api
```

API em `http://localhost:5096`. O banco migra automaticamente na subida.

**IA (opcional):**

```bash
dotnet user-secrets set "AI:ApiKey" "sk-..." --project src/PsiArtigos.Api
```

### 3. Frontend

```bash
cd frontend
npm install
npm run dev
```

App em `http://localhost:5173` (proxy `/api` → API).

## Arquitetura (visão rápida)

```
frontend/          SPA React (Vite)
src/
  PsiArtigos.Api/            Controllers, auth, middleware
  PsiArtigos.Application/    Casos de uso, DTOs
  PsiArtigos.Domain/         Aggregates e regras
  PsiArtigos.Infrastructure/ EF Core, clientes de busca, PDF, IA
```

## Screenshots

_Adicione 2–3 prints aqui (home, busca, leitor) antes de publicar no portfólio._

<!--
![Home](docs/screenshots/home.png)
![Busca](docs/screenshots/search.png)
![Leitor](docs/screenshots/reader.png)
-->

## Limitações honestas

- Só artigos com **PDF em acesso aberto** entram na lista (paywall fica de fora).
- Sem `AI:ApiKey`, resumos/trilhas usam fallback local (qualidade de demo).
- Deploy completo precisa de frontend + API + Postgres (não é só Vercel).

## Licença

Projeto pessoal / portfólio. Uso sob consulta do autor.
