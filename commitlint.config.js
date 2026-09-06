// @ts-check

/** @type {import('@commitlint/types').UserConfig} */
export default {
  extends: ['@commitlint/config-conventional'],
  rules: {
    // Скоуп необязателен, но если указан - только из этого списка.
    // Домены совпадают с доменами фронта: словарь предметной области общий.
    'scope-enum': [
      2,
      'always',
      ['identity-access', 'chats', 'core', 'deployment', 'infra', 'process'],
    ],
  },
};
