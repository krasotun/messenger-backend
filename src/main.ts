import { NestFactory } from '@nestjs/core';
import { AppModule } from './app.module.js';

async function bootstrap() {
  const app = await NestFactory.create(AppModule);

  // Контракт Chat & OAuth API живет под /api/v2 - фронту достаточно подменить apiBaseUrl.
  app.setGlobalPrefix('api/v2');

  await app.listen(process.env.PORT ?? 3001);
}

await bootstrap();
