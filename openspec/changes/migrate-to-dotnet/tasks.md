## 1. Инструменты и каркас проекта

- [x] 1.1 Установить .NET 10 SDK и зафиксировать версию в `global.json` в корне
      репозитория. Проверка: `dotnet --list-sdks` показывает установленную
      версию, `dotnet --version` в корне отдает версию из `global.json`.
- [x] 1.2 Завести solution и проект веб-API `src/Messenger.Api/` с
      контроллерами. Проверка: `dotnet build` проходит, `dotnet run` поднимает
      приложение.
- [x] 1.3 Завести тестовый проект `test/Messenger.Api.Tests/` на xUnit с
      `Microsoft.AspNetCore.Mvc.Testing` и ссылкой на проект API. Проверка:
      `dotnet test` проходит на пустом наборе тестов.
- [x] 1.4 Обновить `.gitignore` под .NET (`bin/`, `obj/`) и убрать из него
      правила, относившиеся к сборке Node. Проверка: `git status` чист после
      `dotnet build`.

## 2. Конфигурация, сериализация и ошибки

- [x] 2.1 Задать префикс `api/v2` для всех контроллеров. Проверка: маршрут
      health доступен по `/api/v2/health` и недоступен по `/health`.
- [x] 2.2 Включить глобальную политику именования JSON
      `JsonNamingPolicy.SnakeCaseLower` на сериализацию и десериализацию.
      Проверка: unit-тест сериализует DTO со свойством `FirstName` и получает
      ключ `first_name`, и разбирает `first_name` обратно.
- [x] 2.3 Реализовать обработчик исключений, приводящий `4xx` и `5xx` к телу
      `{ "reason": <текст> }`, и отключить автоматический `ProblemDetails` для
      ошибок валидации модели; `401` остается без тела; необработанная ошибка
      логируется целиком, наружу идет `500` с обезличенным текстом. Проверка:
      тесты на все три случая, в теле ответа нет полей `type`, `title`,
      `status` из `ProblemDetails`.
- [x] 2.4 Перенести конфигурацию в `appsettings.json` с перекрытием из
      переменных окружения; добавить валидацию на старте с падением и внятным
      сообщением при отсутствии строки подключения. Проверка: приложение не
      стартует без строки подключения и стартует с ней.
- [x] 2.5 Обновить `.env.example`: строка подключения в формате Npgsql вместо
      URI, комментарий о смене формата. Проверка: значение из `.env.example`
      подходит к базе из `docker compose up postgres`.

## 3. Подключение к Postgres

- [ ] 3.1 Подключить `Npgsql.EntityFrameworkCore.PostgreSQL` и
      `Microsoft.EntityFrameworkCore.Design`, завести `DbContext` без сущностей
      и зарегистрировать его через `UseNpgsql`. Проверка: `dotnet build`
      проходит, приложение стартует с поднятой базой.
- [ ] 3.2 Проверить цикл миграций от начала до конца на пустом `DbContext`:
      `dotnet ef migrations add` создает миграцию, `dotnet ef database update`
      накатывает ее на базу из `docker compose up postgres`. Проверка: в базе
      появилась таблица истории миграций EF Core; миграция прочитана глазами
      перед накатом.
- [ ] 3.3 Убедиться, что старт приложения не зависит от доступности базы -
      как сейчас у ленивого пула Drizzle. Проверка: приложение поднимается при
      остановленном контейнере Postgres и отвечает на health.

## 4. Health и снос старого стека

- [x] 4.1 Реализовать `GET /api/v2/health`, отвечающий `200` и телом
      `{ "status": "ok", "uptime": <целое число секунд> }` - тем же, что
      отдает снимаемый Nest-эндпоинт. Проверка: e2e через
      `WebApplicationFactory` сверяет код, состав полей, `status` и то, что
      `uptime` - неотрицательное целое.
- [x] 4.2 Удалить `src/main.ts`, `src/app.module.ts`, `src/database/`,
      `src/health/`, `test/app.e2e-spec.ts`, `nest-cli.json`,
      `tsconfig*.json`, `vitest.config.ts`, `vitest.config.e2e.ts`,
      `oxlint.json`, `.prettierrc`, `drizzle.config.ts`. Проверка: `dotnet
test` зеленый, в репозитории не осталось `.ts`-файлов приложения.
- [x] 4.3 Поставить OpenSpec через `brew install openspec` и убедиться, что
      `openspec validate migrate-to-dotnet --strict` зеленый без `npx`.
      Проверка: команда отрабатывает при удаленном `node_modules/`.
- [x] 4.4 Завести `dotnet-tools.json` с Husky.Net, хук `commit-msg` на
      `.csx`-скрипт с регуляркой Conventional Commits и перечнем скоупов из
      снимаемого `commitlint.config.js`, хук `pre-commit` на `dotnet format`
      по `${staged}`. Проверка: коммит с неверным заголовком отклоняется, с
      верным - проходит; скоуп вне перечня отклоняется.
- [x] 4.5 Удалить `package.json`, `package-lock.json`, `node_modules/`,
      `.husky/` и `commitlint.config.js`. Проверка: в репозитории не осталось
      файлов npm, хуки и `openspec` продолжают работать.
- [x] 4.6 Настроить `.editorconfig` и анализаторы, заменяющие oxlint.
      Проверка: `dotnet format --verify-no-changes` зеленый на всей кодовой
      базе.

## 5. Сборка и запуск

- [x] 5.1 Переписать `Dockerfile` на образы .NET: сборка в SDK-образе,
      рантайм в `aspnet`-образе. Проверка: `docker build .` проходит, образ не
      содержит Node.
- [x] 5.2 Обновить сервис `app` в `docker-compose.yml` под новые переменные и
      порт; сервис `postgres` и том не трогать. Проверка: `docker compose up
--build` поднимает оба сервиса, `GET /api/v2/health` из контейнера
      отвечает `200` и `{ "status": "ok" }`.
- [x] 5.3 Обновить `.dockerignore` под .NET (`bin/`, `obj/`). Проверка: в
      контекст сборки не попадают артефакты локальной сборки.

## 6. Процесс и документация

- [x] 6.1 Переписать блок `context` в `openspec/config.yaml`: стек, структура
      каталогов, команды проверок. Проверка: описание совпадает с тем, что
      реально лежит в репозитории, и не упоминает Nest, Drizzle, Vitest и
      oxlint.
- [ ] 6.2 Обновить `README.md`: установка SDK, поднятие базы, миграции,
      запуск, команды тестов и формата. Проверка: инструкция проходится с нуля
      на чистой машине без обращения к другим источникам.
- [x] 6.3 Обновить `CLAUDE.md`: внешние инструменты для Context7 (ASP.NET
      Core и EF Core вместо NestJS и Drizzle), команды проверок блока вместо
      `npm run lint` и `npm run test:*`, перечень скоупов вместо ссылки на
      удаленный `commitlint.config.js`, вызов `openspec` вместо `npx openspec`.
      Проверка: в файле не осталось ссылок на снятый стек и на файлы npm.
- [x] 6.4 Сверить `docs/api/swagger.json` и `docs/api/README.md`: они не
      меняются. Проверка: `git diff` по каталогу `docs/api/` пуст.

## 7. Сдача блока

- [ ] 7.1 Прогнать полный набор проверок: `dotnet format
--verify-no-changes`, `dotnet test` на unit и на e2e с поднятой базой.
      Проверка: все зеленые.
- [x] 7.2 Пройти сценарий вручную: `docker compose up --build` с нуля на
      чистом томе, миграции, `GET /api/v2/health`. Проверка: отвечает `200` и
      `{ "status": "ok" }`.
- [ ] 7.3 Обновить `add-auth-session` через `/opsx:update`: `design.md` и
      `tasks.md` под .NET, блок Impact в `proposal.md`; `spec.md` не трогать.
      Проверка: `npx openspec validate add-auth-session --strict` зеленый и в
      его артефактах не осталось упоминаний Nest, Drizzle и Vitest.
