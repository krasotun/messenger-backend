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
docker compose up --build
```

Поднимаются два сервиса: `postgres` (порт 5432 проброшен на хост) и `app`
(порт 3001). Значения переменных `app` заданы прямо в `docker-compose.yml`.

Разработка на хосте, база в докере:

```sh
docker compose up -d postgres
dotnet run --project src/Messenger.Api
```

Локально настройки берутся из `appsettings.Development.json` и подходят к базе
из compose. Перекрыть их можно переменными окружения - образец с пояснением про
формат строки подключения лежит в `.env.example`; файла `.env` приложение не
читает, значения экспортируются в оболочку.

Сервер поднимается на `http://localhost:3001`, все пути под префиксом
`/api/v2`. Порт 3001, а не 3000: 3000 занят `mock-backend` фронта, и они должны
уживаться одновременно.

Приложение не требует базы на старте: соединение открывается лениво, на первом
запросе к БД. Поднятый с остановленным Postgres сервер отвечает на health.

Проверка живости:

```sh
curl http://localhost:3001/api/v2/health
```

## Миграции

Схема описывается кодом в `src/Messenger.Api/Data/`, миграции генерирует EF
Core по разнице моделей. `dotnet-ef` - локальный инструмент из
`dotnet-tools.json`, отдельно его ставить не нужно: он приезжает с
`dotnet tool restore`.

```sh
docker compose up -d postgres
dotnet ef migrations add <Name> --project src/Messenger.Api --output-dir Data/Migrations
dotnet ef database update --project src/Messenger.Api
```

Сгенерированную миграцию читают глазами до наката: EF Core прячет SQL, и
увидеть его больше негде.

Строку подключения `dotnet ef` берет из `appsettings.Development.json` -
инструменты запускают приложение в окружении Development.

Как мигрировать на сервере - вопрос к issue про деплой, здесь он не решен.

## Команды

| Команда                              | Что делает                    |
| ------------------------------------ | ----------------------------- |
| `dotnet build`                       | Сборка решения                |
| `dotnet run --project src/Messenger.Api` | Запуск приложения          |
| `dotnet test`                        | Тесты - unit и e2e            |
| `dotnet format`                      | Форматирование по `.editorconfig` |
| `dotnet format --verify-no-changes`  | Проверка формата без правок   |
| `dotnet ef migrations add <Name>`    | Сгенерировать миграцию по модели |
| `dotnet ef database update`          | Применить миграции            |
| `openspec list`                      | Активные changes              |

e2e-тесты поднимают приложение в процессе через `WebApplicationFactory` и ходят
в него настоящим HTTP.

## Структура

- `src/Messenger.Api/` - проект API: контроллеры, конфигурация, точка входа;
- `src/Messenger.Api/Data/` - `DbContext` и миграции EF Core;
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
