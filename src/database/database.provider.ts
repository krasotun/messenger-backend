import type { Provider } from '@nestjs/common';
import { ConfigService } from '@nestjs/config';
import { drizzle, type NodePgDatabase } from 'drizzle-orm/node-postgres';
import * as schema from './schema.js';

export const DATABASE_CONNECTION = 'DATABASE_CONNECTION';

export type Database = NodePgDatabase<typeof schema>;

export const databaseConnectionProvider: Provider = {
  provide: DATABASE_CONNECTION,
  inject: [ConfigService],
  useFactory: (config: ConfigService): Database => {
    const url = config.getOrThrow<string>('DATABASE_URL');

    // Пул подключается лениво, при первом запросе: старт приложения не зависит от доступности БД.
    return drizzle({ connection: { connectionString: url }, schema });
  },
};
