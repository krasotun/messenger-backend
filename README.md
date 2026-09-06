# messenger-backend

Бэкенд веб-мессенджера [`messenger`](https://github.com/krasotun/messenger).
Реализует контракт «Chat & OAuth API» один в один - фронту для перехода
достаточно подменить `apiBaseUrl`.

Стек: NestJS 12 (ESM), TypeScript, PostgreSQL через Drizzle ORM, Vitest.

## Требования

- Node.js 22+ (проверено на 26)
- Docker с плагином Compose

PostgreSQL в систему не ставится - он живет в контейнере, см. ниже.

## Запуск

Весь стенд в докере:

```sh
cp .env.example .env
docker compose up --build
```

Поднимаются два сервиса: `postgres` (порт 5432 проброшен на хост) и `app`
(порт 3001). Приложение стартует только после того, как БД прошла healthcheck.

Разработка с watch-режимом - приложение на хосте, база в докере:

```sh
docker compose up -d postgres
npm ci
npm run start:dev
```

Миграции пока гоняются с хоста - `drizzle-kit` лежит в dev-зависимостях, а в
production-образ они не попадают:

```sh
npm run db:migrate
```

Как мигрировать на сервере - вопрос к issue про деплой, здесь он не решен.

Сервер поднимается на `http://localhost:3001`, все пути под префиксом
`/api/v2`. Порт 3001, а не 3000: 3000 занят `mock-backend` фронта, и они должны
уживаться одновременно.

Проверка живости:

```sh
curl http://localhost:3001/api/v2/health
```

## Команды

| Команда               | Что делает                              |
| --------------------- | --------------------------------------- |
| `npm run build`       | Сборка в `dist/`                        |
| `npm run start:dev`   | Запуск в watch-режиме                   |
| `npm run lint`        | oxlint по `src/` и `test/`              |
| `npm run test:ci`     | Юнит-тесты (Vitest)                     |
| `npm run test:e2e`    | e2e-тесты (Vitest + supertest)          |
| `npm run db:generate` | Сгенерировать миграцию по схеме Drizzle |
| `npm run db:migrate`  | Применить миграции                      |

## Процесс

Разработка по OpenSpec: сначала спецификация в `openspec/`, потом код. Правила
работы - в `CLAUDE.md`, контекст проекта - в `openspec/config.yaml`, контракт -
в `docs/api/`.

Словарь предметной области общий с фронтом и живет в `../messenger/CONTEXT.md`.
