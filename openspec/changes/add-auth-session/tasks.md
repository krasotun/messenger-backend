## 1. Схема БД и миграция

- [ ] 1.1 Описать сущность `User` в `src/Messenger.Api/Data/`: `Id` (`int`,
      identity - контракт требует числовой `id`), `FirstName`, `SecondName`,
      `Login`, `Email`, `Phone`, `PasswordHash` - все обязательные;
      `DisplayName` и `Avatar` - nullable; `CreatedAt`/`UpdatedAt` со
      значением по умолчанию из БД. Уникальные индексы на `Login` и `Email`
      через `HasIndex(...).IsUnique()`. Проверка: `dotnet build` проходит.
- [ ] 1.2 Описать сущность `Session`: `Id` (`text`, первичный ключ), `UserId`
      (`int`, обязательный, внешний ключ на `User` с `OnDelete(Cascade)`),
      `CreatedAt`, `ExpiresAt` (`timestamptz`, обязательный), `Value` для
      сериализованного ticket, индекс по `UserId`. Проверка: `dotnet build`
      проходит.
- [ ] 1.3 Сгенерировать миграцию
      `dotnet ef migrations add AddUsersAndSessions --project src/Messenger.Api
--output-dir Data/Migrations`, прочитать ее глазами и накатить
      `dotnet ef database update` на поднятый `docker compose up -d postgres`.
      Проверка: в базе есть обе таблицы, оба уникальных индекса и внешний ключ
      с каскадом.
- [ ] 1.4 Дописать в `README.md` шаг «поднять базу и накатить миграции» перед
      `dotnet run` и `dotnet test`. Проверка: инструкция проходится с нуля на
      чистой базе.

## 2. Пароль и сессия

- [ ] 2.1 Подключить `Microsoft.Extensions.Identity.Core` и завести обертку
      над `PasswordHasher<User>`: хеширование и проверка, результат
      `SuccessRehashNeeded` трактуется как успех. Проверка: unit-тесты на «хеш
      не равен паролю», «два одинаковых пароля дают разные хеши», «проверка
      своего хеша истинна, чужого - ложна».
- [ ] 2.2 Добавить в обертку проверку по фиктивному хешу для случая
      «пользователь не найден», чтобы время ответа не выдавало занятые логины.
      Проверка: unit-тест на то, что метод отрабатывает и возвращает `false`.
- [ ] 2.3 Реализовать `ITicketStore` поверх таблицы `sessions`: `StoreAsync`
      создает запись с ключом `RandomNumberGenerator.GetBytes(32)` в
      `Base64Url` и `ExpiresAt` из `Session:TtlDays`, `RetrieveAsync` находит
      живую сессию, `RemoveAsync` удаляет одну, `RenewAsync` обновляет ticket;
      истекшая запись при обращении удаляется и не находится. Проверка:
      unit-тесты на создание, поиск, удаление, обновление и на то, что
      истекшая сессия не находится и удаляется.
- [ ] 2.4 Добавить типизированную конфигурацию секциями `Session`, `Cookie` и
      `Cors` (`CookieName`, `TtlDays`, `Secure`, `SameSite`, `Origins`) через
      `AddOptions<T>().Bind(...).ValidateOnStart()`; приложение не стартует с
      пустым `Cors:Origins`. Отразить значения в `appsettings.json`,
      `.env.example` и `docker-compose.yml`. Проверка: unit-тест на отказ
      стартовать с пустым `Cors:Origins`, приложение поднимается с
      заполненной конфигурацией.

## 3. Инфраструктура запроса

- [ ] 3.1 Подключить cookie-аутентификацию: `AddAuthentication().AddCookie()`
      с `SessionStore` из задачи 2.3, `Cookie.HttpOnly = true`, `Path = "/"`,
      имя, `Secure` и `SameSite` из конфигурации; `UseAuthentication()` и
      `UseAuthorization()` в конвейере. Проверка: `dotnet build` проходит,
      приложение стартует.
- [ ] 3.2 Задать `AuthorizationOptions.FallbackPolicy` с
      `RequireAuthenticatedUser()` и пометить `[AllowAnonymous]` health,
      `signup`, `signin` и `logout`. Проверка: e2e - `GET /api/v2/health`
      доступен гостю, а защищенный эндпоинт гостю отвечает `401` без тела и
      без заголовка `Location`.
- [ ] 3.3 Включить `UnmappedMemberHandling = Disallow` в настройках JSON.
      Проверка: e2e - запрос с лишним полем `role` в теле получает `400` с
      телом `{ reason }`.
- [ ] 3.4 Включить CORS со списком origin из `Cors:Origins` и
      `AllowCredentials()`. Проверка: e2e - запрос с разрешенным `Origin`
      несет `Access-Control-Allow-Origin` и `Access-Control-Allow-Credentials`,
      с посторонним - не несет.

## 4. Регистрация и вход

- [ ] 4.1 Завести `src/Messenger.Api/IdentityAccess/` с контроллером `auth` и
      DTO `SignUpRequest` / `SignInRequest` на DataAnnotations; регулярки
      `email` и `phone` взять из `docs/api/swagger.json`. Проверка: unit-тесты
      DTO на все случаи валидации из спеки (нет поля, кривой email, кривой
      телефон, пустая строка).
- [ ] 4.2 Реализовать `POST /auth/signup`: создание пользователя с хешем
      пароля, `display_name` и `avatar` пустые, `SignInAsync` на созданную
      сессию, ответ `200` с `{ id }` и установкой cookie. Проверка: e2e на
      успешную регистрацию с последующим `GET /auth/user` по выданной cookie.
- [ ] 4.3 Обработать нарушение уникальности (`PostgresException` со `SqlState`
      `23505`) как `400` с `reason` `Login already exists` или
      `Email already exists` - по имени нарушенного индекса. Проверка: e2e на
      занятый логин и на занятый email, в обоих случаях второй пользователь не
      создан и cookie не выдана.
- [ ] 4.4 Реализовать `POST /auth/signin`: проверка пароля, `SignInAsync`,
      ответ `200` телом `OK` и установкой cookie; неверные учетные данные -
      `400` с `{ reason: 'Login or password is incorrect' }`, одинаковым для
      неизвестного логина и неверного пароля. Проверка: e2e на успешный вход,
      неверный пароль и несуществующий логин.
- [ ] 4.5 Завершать прежнюю сессию при входе с уже живой cookie. Проверка:
      e2e - вход вторым пользователем поверх сессии первого делает прежнюю
      cookie неопознаваемой.

## 5. Текущий пользователь и выход

- [ ] 5.1 Реализовать `GET /auth/user`: ответ `200` ровно восемью полями
      контракта, `display_name` и `avatar` как `null`, когда не заданы; хеш
      пароля и служебные поля сессии не отдаются. Проверка: e2e сверяет набор
      ключей ответа с `UserResponse` из `swagger.json` и убеждается, что поля
      с паролем нет.
- [ ] 5.2 Убедиться, что гость получает `401` без тела - без cookie, с cookie
      несуществующей сессии и с cookie истекшей сессии. Проверка: три
      e2e-теста; истекшая сессия готовится записью с прошедшим `ExpiresAt`.
- [ ] 5.3 Реализовать `POST /auth/logout`: `SignOutAsync` удаляет сессию и
      сбрасывает cookie, ответ `200` с пустым телом; гостю тоже `200`.
      Проверка: e2e на выход текущего пользователя с последующим `401` по
      прежней cookie и на выход без cookie.
- [ ] 5.4 Убедиться, что выход не трогает другие сессии того же пользователя.
      Проверка: e2e с двумя `HttpClient` - после выхода в одном второй
      продолжает получать `200` на `GET /auth/user`.
- [ ] 5.5 Проверить атрибуты cookie: `HttpOnly`, `Path=/`, `Secure` и
      `SameSite` из конфигурации, а также что сессия не принимается через
      заголовок `Authorization`. Проверка: e2e на заголовок `Set-Cookie` и на
      `401` при передаче идентификатора сессии заголовком.

## 6. Сдача блока

- [ ] 6.1 Прогнать `dotnet format --verify-no-changes`, `dotnet build` и
      `dotnet test` на поднятой базе. Проверка: все зеленые.
- [ ] 6.2 Проверить контракт по `docs/api/swagger.json`: пути, методы, коды и
      состав полей всех четырех эндпоинтов совпадают. Проверка: расхождений
      нет; если нашлось - это отклонение, оно заводится отдельным change, а не
      правится молча.
- [ ] 6.3 Поднять `docker compose up --build` и пройти сценарий вручную:
      регистрация, вход, перезагрузка страницы фронта с подмененным
      `apiBaseUrl`, выход. Проверка: фронт проходит все четыре шага.
