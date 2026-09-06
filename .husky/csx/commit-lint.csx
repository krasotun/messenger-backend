/// Conventional Commits для messenger-backend.
///
/// Повторяет снятый commitlint.config.js: типы из config-conventional, скоупы
/// из scope-enum. Скоупы доменов общие с фронтом ../messenger - меняется
/// список, правится в двух местах, автоматической сверки между репозиториями
/// нет.
///
/// LINQ здесь не используется: хост .csx его не подключает.

using System;
using System.IO;
using System.Text.RegularExpressions;

var types = "feat|fix|docs|refactor|test|chore|build|ci|perf|style|revert";
var scopes = "identity-access|chats|core|deployment|infra|process";

var header = File.ReadAllLines(Args[0])[0].Trim();

// commitlint по умолчанию пропускал служебные коммиты - повторяем, иначе
// merge-коммит в ветку блока будет отклонен.
string[] ignored = { "Merge ", "Revert ", "fixup!", "squash!" };
foreach (var prefix in ignored)
{
    if (header.StartsWith(prefix, StringComparison.Ordinal))
        return 0;
}

var pattern = $@"^(?:{types})(?:\((?:{scopes})\))?!?: [a-z0-9](?:[^\n]*[^.\s])?$";

if (header.Length <= 100 && Regex.IsMatch(header, pattern))
    return 0;

Console.ForegroundColor = ConsoleColor.Red;
Console.WriteLine($"Неверный заголовок коммита: {header}");
Console.ResetColor();
Console.WriteLine("Формат: <type>(<scope>): <description>");
Console.WriteLine($"Типы:   {types.Replace("|", ", ")}");
Console.WriteLine($"Скоупы: {scopes.Replace("|", ", ")} - необязателен");
Console.WriteLine("Описание: английский, нижний регистр, без точки в конце, до 100 символов.");
return 1;
