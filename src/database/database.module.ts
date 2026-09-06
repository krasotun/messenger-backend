import { Global, Module } from '@nestjs/common';
import { ConfigModule } from '@nestjs/config';
import { DATABASE_CONNECTION, databaseConnectionProvider } from './database.provider.js';

@Global()
@Module({
  imports: [ConfigModule],
  providers: [databaseConnectionProvider],
  exports: [DATABASE_CONNECTION],
})
export class DatabaseModule {}
