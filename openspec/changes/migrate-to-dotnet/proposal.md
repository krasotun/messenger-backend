## Why

Issue: #15

Платформа бэкенда меняется с NestJS на ASP.NET Core. Причина не техническая:
есть намерение со временем работать фулстеком, и этот проект - место, где
вторая платформа осваивается всерьез, на настоящей задаче, а не на учебном
примере. Фронт уже написан на Angular, и .NET рядом с ним дает связку, которая
встречается на рынке целиком.

Делать это надо сейчас. В `src/` и `test/` лежит 132 строки: health-контроллер,
пустая схема Drizzle и провайдер пула. Доменного кода нет, миграций нет, CI нет,
`add-auth-session` еще не реализован. Каждый следующий смерженный блок повышает
цену переезда, а первым же блоком #1 в репозиторий приходят схема БД, миграции
и механизм сессии - переписывать их потом дороже, чем не писать сейчас.

Половина того, что `add-auth-session` планировал реализовывать руками, в
ASP.NET Core есть из коробки: сессионная cookie и хеширование пароля.

## What Changes

- Рантайм: NestJS 12 на Node → ASP.NET Core на .NET 10.
- Доступ к БД: Drizzle → EF Core с провайдером Npgsql; `drizzle-kit generate`
  и `drizzle-kit migrate` → `dotnet ef migrations add` и `dotnet ef database
update`. Каталог `drizzle/` уступает место каталогу миграций EF Core.
- Тесты: Vitest и supertest → xUnit и `WebApplicationFactory`; unit-тесты
  переезжают из файлов рядом с кодом в отдельный тестовый проект.
- Линт и формат: oxlint и prettier → `dotnet format` с `.editorconfig` и
  анализаторами.
- Сборка: `Dockerfile` и сервис `app` в `docker-compose.yml` переводятся на
  образы .NET. Сервис `postgres` не меняется вовсе.
- `openspec/config.yaml`: блок `context` описывает стек и структуру каталогов -
  переписывается целиком.
- `package.json`, `package-lock.json`, `node_modules/`, `.husky/` и
  `commitlint.config.js` удаляются. Инструменты процесса переезжают: husky,
  lint-staged и commitlint заменяет Husky.Net как локальный dotnet-инструмент,
  OpenSpec CLI ставится через `brew install openspec`. Node перестает быть и
  рантаймом приложения, и инструментом репозитория.
- `GET /api/v2/health` продолжает отвечать `200` и `{ "status": "ok" }` -
  единственное существующее поведение сохраняется без изменений.

**Это не ломающее изменение.** В терминах этого репозитория ломающим считается
отклонение от контракта, требующее синхронного PR во фронт. Контракт не
меняется: те же пути, методы, коды и состав полей, тот же префикс `api/v2`.
Фронт не знает и не должен знать, какой рантайм за `apiBaseUrl`. Синхронной
правки `../messenger` этот change не требует.

## Capabilities

### New Capabilities

Нет.

### Modified Capabilities

Нет. Внешнее поведение не меняется, поэтому change помечен `skip_specs: true` в
своем `.openspec.yaml`. Единственное описанное поведение - health-эндпоинт -
сохраняется дословно, а требования `add-auth-session` живут в своем change и
переезда не касаются: спека написана про поведение, а не про платформу.

## Impact

- Код: `src/` и `test/` удаляются целиком и заводятся заново на .NET.
- Процесс: `openspec/config.yaml` (блок `context`), `README.md` (запуск,
  миграции, тесты), `.env.example`, `Dockerfile`, `docker-compose.yml`,
  `.gitignore`. Хуки переезжают с husky и `lint-staged` на Husky.Net, формат
  коммитов - с commitlint на регулярку в `.csx`.
- Смежный change: у `add-auth-session` `design.md` и `tasks.md` написаны под
  Nest, Drizzle и Vitest и становятся неактуальными в момент мержа этого
  change. `proposal.md` теряет только блок Impact, `spec.md` остается верным
  целиком. Обновляются отдельно через `/opsx:update` - до начала реализации #1.
- Зависимости: npm-дерево уходит целиком - `@nestjs/*`, `drizzle-orm`,
  `drizzle-kit`, `pg`, `vitest`, `oxlint`, `prettier`, `supertest`, husky и
  commitlint. Приходят `Npgsql.EntityFrameworkCore.PostgreSQL`,
  `Microsoft.EntityFrameworkCore.Design`, `xunit`,
  `Microsoft.AspNetCore.Mvc.Testing` и Husky.Net в `dotnet-tools.json`.
- Инструменты разработчика: OpenSpec CLI переезжает на `brew install
openspec`; prettier для markdown и JSON пропадает без замены.
- Окружение: `DATABASE_URL` в формате URI уступает строке подключения Npgsql -
  формат другой, значение то же. `PORT` заменяется на механизм конфигурации
  ASP.NET Core.
- Инструменты разработчика: на машине нет `dotnet`, установка SDK - первый шаг.
- Контракт и фронт: не затронуты.
