using Microsoft.EntityFrameworkCore;

namespace Messenger.Api.Data;

/// <summary>
/// Контекст базы мессенджера. Сущностей пока нет: таблицы заводятся вместе с
/// эндпоинтами, которые их читают, а не заранее. Контекст нужен уже сейчас,
/// чтобы цикл миграций был рабочим до первого домена.
/// </summary>
public sealed class MessengerDbContext(DbContextOptions<MessengerDbContext> options) : DbContext(options);
