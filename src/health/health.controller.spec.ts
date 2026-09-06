import { Test, type TestingModule } from '@nestjs/testing';
import { HealthController } from './health.controller.js';

describe('HealthController', () => {
  let controller: HealthController;

  beforeEach(async () => {
    const module: TestingModule = await Test.createTestingModule({
      controllers: [HealthController],
    }).compile();

    controller = module.get(HealthController);
  });

  it('отвечает статусом ok', () => {
    expect(controller.check().status).toBe('ok');
  });

  it('отдает время работы процесса целым числом секунд', () => {
    const { uptime } = controller.check();

    expect(Number.isInteger(uptime)).toBe(true);
    expect(uptime).toBeGreaterThanOrEqual(0);
  });
});
