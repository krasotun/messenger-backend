# messenger-backend

Бэкенд веб-мессенджера [`messenger`](https://github.com/krasotun/messenger).
Реализует контракт «Chat & OAuth API» один в один - фронту для перехода
достаточно подменить `apiBaseUrl`.

Стек: .NET 10, ASP.NET Core (контроллеры), PostgreSQL, xUnit.

## Требования

- [.NET SDK 10](https://dotnet.microsoft.com/download) - версия зафиксирована
  в `global.json`
- Docker с плагином Compose
- [OpenSpec CLI](https://openspec.dev/) - `brew install openspec`

PostgreSQL в систему не ставится: он живет в контейнере, см. ниже.

После клонирования нужно один раз восстановить локальные инструменты и
поставить git-хуки:

```sh
dotnet tool restore
dotnet husky install
```

## Запуск

Весь стенд в докере:

```sh
cp .env.example .env
docker compose up --build
```

Поднимаются два сервиса: `postgres` (порт 5432 проброшен на хост) и `app`
(порт 3001).

Разработка на хосте, база в докере:

```sh
docker compose up -d postgres
dotnet run --project src/Messenger.Api
```

Сервер поднимается на `http://localhost:3001`, все пути под префиксом
`/api/v2`. Порт 3001, а не 3000: 3000 занят `mock-backend` фронта, и они должны
уживаться одновременно.

Проверка живости:

```sh
curl http://localhost:3001/api/v2/health
```

## Команды

| Команда                              | Что делает                    |
| ------------------------------------ | ----------------------------- |
| `dotnet build`                       | Сборка решения                |
| `dotnet run --project src/Messenger.Api` | Запуск приложения          |
| `dotnet test`                        | Тесты - unit и e2e            |
| `dotnet format`                      | Форматирование по `.editorconfig` |
| `dotnet format --verify-no-changes`  | Проверка формата без правок   |
| `openspec list`                      | Активные changes              |

e2e-тесты поднимают приложение в процессе через `WebApplicationFactory` и ходят
в него настоящим HTTP.

## Структура

- `src/Messenger.Api/` - проект API: контроллеры, конфигурация, точка входа;
- `test/Messenger.Api.Tests/` - тесты;
- `docs/api/` - контракт: `swagger.json` и протокол переписки;
- `openspec/` - спецификации и changes.

Префикс `api/v2` задается один раз в `Controllers/ApiControllerBase.cs` и
повторяет форму контракта.

## Процесс

Разработка по OpenSpec: сначала спецификация в `openspec/`, потом код. Правила
работы - в `CLAUDE.md`, контекст проекта - в `openspec/config.yaml`, контракт -
в `docs/api/`.

Формат коммитов - Conventional Commits, проверяется хуком `commit-msg` через
Husky.Net: скрипт `.husky/csx/commit-lint.csx`.

Словарь предметной области общий с фронтом и живет в `../messenger/CONTEXT.md`.
