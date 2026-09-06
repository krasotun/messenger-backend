# Контракт «Chat & OAuth API»

`swagger.json` - спецификация API, которое реализует этот бэкенд. Снята с
учебного API Яндекс.Практикума: фронт `messenger` написан под нее, и бэкенд
повторяет ее один в один.

Это источник правды по контракту для обоих репозиториев. Фронт держит копию в
своем `docs/api/swagger.json`; расходиться копии не должны.

`websocket.md` - разведанный протокол переписки. В `swagger.json` он не описан:
HTTP отдает только токен, обмен идет по WebSocket.

## Отклонения от контракта

Отклонение ломает фронт, поэтому заводится отдельным change и правится в двух
репозиториях синхронно. Список принятых отклонений ведется здесь; пока их нет.

## Обновление копии с сервера Практикума

На сервере отдельного `swagger.json` нет, спека вшита в скрипт Swagger UI:

```sh
curl -s https://ya-praktikum.tech/api/v2/swagger/swagger-ui-init.js -o /tmp/swagger-ui-init.js
python3 -c '
import json
s = open("/tmp/swagger-ui-init.js", encoding="utf-8").read()
doc, _ = json.JSONDecoder().raw_decode(s[s.index("{", s.index("\"swaggerDoc\"")):])
json.dump(doc, open("docs/api/swagger.json", "w", encoding="utf-8"), ensure_ascii=False, indent=2)
'
```

Человекочитаемая версия - https://ya-praktikum.tech/api/v2/swagger/

Снято 2026-09-03: Swagger 2.0, «Chat & OAuth API» 2.0.0, 35 путей.
