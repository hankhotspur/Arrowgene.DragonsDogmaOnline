CREATE TABLE IF NOT EXISTS "ddon_reward_mission_progress" (
    "character_id"        INTEGER NOT NULL,
    "mission_id"          INTEGER NOT NULL,
    "progress"            INTEGER NOT NULL DEFAULT 0,
    "state"               TINYINT NOT NULL DEFAULT 1,
    CONSTRAINT "pk_ddon_reward_mission_progress" PRIMARY KEY ("character_id", "mission_id"),
    CONSTRAINT "fk_ddon_reward_mission_progress_character_id" FOREIGN KEY ("character_id") REFERENCES "ddon_character" ("character_id") ON DELETE CASCADE
);
