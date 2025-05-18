-- I. Таблица для данных, шардированных по userId
-- Эта таблица будет хранить связи пользователей с IP-адресами

CREATE TABLE IF NOT EXISTS user_connections_user_shard (
    user_id BIGINT NOT NULL,        -- Идентификатор пользователя
    ip_address INET NOT NULL,       -- IP-адрес (IPv4 или IPv6)
    last_seen_at TIMESTAMPTZ NOT NULL, -- Время последнего обнаружения этой пары (user_id, ip_address)

    -- Первичный ключ обеспечивает уникальность пары (user_id, ip_address)
    -- и является основным индексом для поиска IP-адресов конкретного пользователя.
    PRIMARY KEY (user_id, ip_address)
);

-- Индекс для быстрого поиска N последних подключений пользователя (или самого последнего).
-- DESC по last_seen_at позволяет быстро получить самые свежие записи.
CREATE INDEX IF NOT EXISTS idx_user_connections_user_shard_user_id_last_seen_at
ON user_connections_user_shard (user_id, last_seen_at DESC);

COMMENT ON TABLE user_connections_user_shard IS 'Хранит информацию о подключениях пользователей (IP-адреса и время последнего визита). Шардируется по user_id.';
COMMENT ON COLUMN user_connections_user_shard.user_id IS 'Идентификатор пользователя.';
COMMENT ON COLUMN user_connections_user_shard.ip_address IS 'IP-адрес пользователя (IPv4 или IPv6).';
COMMENT ON COLUMN user_connections_user_shard.last_seen_at IS 'Время последнего обнаружения пользователя с данного IP-адреса (с часовым поясом).';


-- II. Таблица для данных, шардированных по IP-адресу
-- Эта таблица будет использоваться для поиска пользователей по IP-адресу или его префиксу.

CREATE TABLE IF NOT EXISTS ip_to_user_registry_ip_shard (
    ip_address INET NOT NULL,       -- IP-адрес (IPv4 или IPv6)
    user_id BIGINT NOT NULL,        -- Идентификатор пользователя, связанного с этим IP
    last_seen_at TIMESTAMPTZ NOT NULL, -- Время последнего обнаружения этой пары (ip_address, user_id)
    ip_address_search_text TEXT NOT NULL,    -- Нормализованное текстовое представление IP для префиксного поиска (например, полностью развернутый IPv6)

    -- Первичный ключ обеспечивает уникальность пары (ip_address, user_id)
    -- Для одного IP может быть несколько пользователей,
    -- и для каждого из них будет храниться свое last_seen_at.
    PRIMARY KEY (ip_address, user_id)
);

-- Индекс по ip_address для поддержки операторов типа network_ops (например, <<= для префиксного поиска).
-- GIST-индекс хорошо подходит для типа INET и его операторов.
CREATE INDEX IF NOT EXISTS idx_ip_to_user_registry_ip_shard_ip_address_gist
ON ip_to_user_registry_ip_shard USING GIST (ip_address inet_ops);

-- Индекс по ip_address_search_text для быстрого префиксного поиска (LIKE 'prefix%').
-- text_pattern_ops используется для оптимизации таких запросов.
CREATE INDEX IF NOT EXISTS idx_ip_to_user_registry_ip_shard_search_text
ON ip_to_user_registry_ip_shard (ip_address_search_text text_pattern_ops);

-- Дополнительный индекс может быть полезен, если часто ищут всех пользователей для IP, и затем сортируют по времени:
CREATE INDEX IF NOT EXISTS idx_ip_to_user_registry_ip_shard_ip_last_seen_user
ON ip_to_user_registry_ip_shard (ip_address, last_seen_at DESC, user_id);


COMMENT ON TABLE ip_to_user_registry_ip_shard IS 'Хранит информацию о пользователях, замеченных с определенных IP-адресов. Шардируется по IP-адресу.';
COMMENT ON COLUMN ip_to_user_registry_ip_shard.ip_address IS 'IP-адрес (IPv4 или IPv6).';
COMMENT ON COLUMN ip_to_user_registry_ip_shard.user_id IS 'Идентификатор пользователя, связанного с этим IP-адресом.';
COMMENT ON COLUMN ip_to_user_registry_ip_shard.last_seen_at IS 'Время последнего обнаружения пользователя с данного IP-адреса.';
COMMENT ON COLUMN ip_to_user_registry_ip_shard.ip_address_search_text IS 'Нормализованное текстовое представление IP-адреса для эффективного префиксного поиска (например, полностью развернутый IPv6 без "::" в нижнем регистре). Заполняется приложением.';

-- Примечания по UPSERT логике (будет реализована в приложении):

-- Для user_connections_user_shard:
-- INSERT INTO user_connections_user_shard (user_id, ip_address, last_seen_at)
-- VALUES ($1, $2, $3)
-- ON CONFLICT (user_id, ip_address) DO UPDATE
-- SET last_seen_at = EXCLUDED.last_seen_at
-- WHERE EXCLUDED.last_seen_at > user_connections_user_shard.last_seen_at;

-- Для ip_to_user_registry_ip_shard:
-- INSERT INTO ip_to_user_registry_ip_shard (ip_address, user_id, last_seen_at, ip_address_search_text)
-- VALUES ($1, $2, $3, $4) -- $4 - это подготовленное приложением текстовое представление IP
-- ON CONFLICT (ip_address, user_id) DO UPDATE
-- SET last_seen_at = EXCLUDED.last_seen_at
-- WHERE EXCLUDED.last_seen_at > ip_to_user_registry_ip_shard.last_seen_at;
-- Обратите внимание: если last_seen_at не изменился, то и ip_address_search_text не требует обновления, так как он зависит только от ip_address.

-- По поводу `ip_address_search_text`:
-- Его формирование (например, полное раскрытие IPv6) должно происходить на стороне сервиса обработки перед записью в БД.
-- Пример для IPv4: '192.168.1.5'
-- Пример для IPv6 '2001:0db8:0000:0000:0000:0000:0000:0001' (вместо '2001:db8::1')
-- Это значение должно быть консистентным, чтобы поиск по префиксу работал корректно.

