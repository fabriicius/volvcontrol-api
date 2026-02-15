-- Garante que o email seja único nas tabelas users e client.
-- Execute após conferir que não há emails duplicados (veja consultas abaixo).

-- Opcional: conferir duplicados antes de aplicar
-- SELECT email, COUNT(*) FROM users GROUP BY email HAVING COUNT(*) > 1;
-- SELECT email, COUNT(*) FROM client GROUP BY email HAVING COUNT(*) > 1;

-- Tabela users: email único
ALTER TABLE users
  ADD UNIQUE INDEX uk_users_email (email);

-- Tabela client: email único
ALTER TABLE client
  ADD UNIQUE INDEX uk_client_email (email);
