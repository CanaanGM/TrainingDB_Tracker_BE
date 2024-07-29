
/*
 * why is it like this ?
 *
 * MUSCLE:
 * muscles and muscle groups are inter-changeable, and the will not change over time, so simplifying queries are best, space is a trade i took!
 * and it will eliminate complex joins with Equipments and Exercises.
 * one table > 3.
 *
 * Exercise:
 * difficulty is a number from 1 ~ 5, maybe enforced by an upper layer. i would've used an enum if i was not using Sqlite.
 *
 * Exercise Muscle:
 * What muscles are trained in an exercise, and if its the primary focus of it.
 *
 * Equipment:
 * Nice to keep track of what equipment a plan requires for example, anything more than that is not really necessary.
 * primarily to keep track of what i have at home, hence the "weight".
 *
 * Training Session:
 * mood: from 1 ~ 10, depending on it will be a spectrum of emojis or something similar, something FUN!
 *
 * Storing and Viewing data:
 *
 * a day will have multiple session which has multiple exercise records
 *
 * muscles and muscles groups are a small number even when you have 3 languages
 * 64 muscles * 3 = 192
 * 19 group * 3 = 57
 * so no need for separation of tables for localization
 * */
 -- User table
CREATE TABLE IF NOT EXISTS user (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    username TEXT UNIQUE NOT NULL, -- TODO: Not unique, i want 2 Canaans!
    email TEXT UNIQUE NOT NULL,
    height REAL,
    gender CHAR(1),
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    CHECK (gender IN ('F', 'M'))
);
CREATE INDEX idx_user_username ON user(username);
CREATE INDEX idx_user_email ON user(email);
--
 create table if not exists language (
     id integer primary key autoincrement,
     code text not null, -- en, ar, jp
     name text not null -- english, arabic . . .
 );
Create INDEX idx_language_name on language(name);
Create INDEX idx_language_code on language(code);
--
create table if not exists muscle (
	id integer primary key autoincrement
);
create table if not exists muscle_group(
    id integer primary key autoincrement
);
create table if not exists muscle_group_muscle(
    muscle_id integer,
    muscle_group_id integer,
    CONSTRAINT fk_muscle_group_muscle_id FOREIGN KEY (muscle_id) references muscle(id) on delete cascade ,
    CONSTRAINT fk_muscle_group_muscle_group_id FOREIGN KEY (muscle_group_id) references muscle_group(id) on delete cascade,
    Constraint pk_muscle_group_muscle_id PRIMARY KEY (muscle_id, muscle_group_id)
);
create table if not exists localized_muscle(
    muscle_id integer,
    language_id integer,
    name TEXT NOT NULL,
    function TEXT,
    wiki_page_url VARCHAR,
    CONSTRAINT fk_localized_muscle_muscle_id FOREIGN KEY (muscle_id) references muscle(id) on delete cascade ,
    CONSTRAINT fk_localized_muscle_language_id FOREIGN KEY (language_id) references language(id) on delete cascade,
    CONSTRAINT pk_localized_muscle_id primary key (muscle_id, language_id)
);
create table if not exists localized_muscle_group(
    muscle_group integer,
    language_id integer,
    name text not null,
    function text ,
    wiki_page_url text,
    CONSTRAINT fk_localized_muscle_muscle_id FOREIGN KEY (muscle_group) references muscle_group(id) on delete cascade,
    CONSTRAINT fk_localized_muscle_group_language_id foreign key (language_id) references language(id) on delete cascade,
    CONSTRAINT pk_localized_muscle_group_id primary key (muscle_group, language_id)
);
-- to know/keep track of which group was trainined and the cooldown ??!
create table if not exists user_muscle(
    user_id integer,
    muscle_group_id integer,
    muscle_cooldown integer, -- TODO: calculate this based on age and rpe from the records (by user_id and records for today)
    frequency integer, -- calculated from the sessions that has this muscle trained
    training_volume integer, -- calculated from how many reps i did per an exercise that trains this muscle group
    CONSTRAINT fk_user_muscle_user_id FOREIGN KEY (user_id) references user(id),
    CONSTRAINT fk_user_muscle_group_id FOREIGN KEY (muscle_group_id) references muscle_group(id)
);
-- singular exercise
-- description: the description of the exercise
-- how to: how to perform the exercise
create table if not exists exercise(
	id integer primary key autoincrement,
	difficulty integer default 0,
	CHECK (difficulty >= 0 AND difficulty <= 5)
);
create index idx_exercise_difficulty on exercise (difficulty);
-- localization
create table if not exists localized_exercise(
    exercise_id INTEGER NOT NULL,
    language_id INTEGER NOT NULL,
    name TEXT NOT NULL,
    description TEXT,
    how_to TEXT,
    CONSTRAINT fk_localized_exercise_exercise_id FOREIGN KEY (exercise_id) REFERENCES Exercise(id) ON DELETE CASCADE,
    CONSTRAINT fk_localized_exercise_language_id FOREIGN KEY (language_id) REFERENCES language(id) ON DELETE CASCADE,
    CONSTRAINT pk_localized_exercise_id primary key (exercise_id, language_id)
);
CREATE INDEX idx_localized_exercise_name ON localized_exercise(name);
-- what muscles involves in an exercise
-- is_primary: if a muscle is the main target of an exercise
create table if not exists exercise_muscle(
	is_primary bit DEFAULT 1,
	muscle_id integer,
	exercise_id integer,
	constraint pk_exercise_muscle_id primary key (muscle_id,exercise_id),
	CONSTRAINT fk_exercise_muscle_muscle_id Foreign Key(muscle_id) references muscle(id) ON DELETE cascade,
	CONSTRAINT fk_exercise_muscle_exercise_id Foreign Key(exercise_id) references exercise(id) ON DELETE cascade
);
create index idx_exercise_muscle_is_primary on exercise_muscle(is_primary);
-- a link to a video or a pic/gif of how to perform an exercise
-- no need to localize this either
create table if not exists exercise_how_to(
	id integer primary key autoincrement,
	exercise_id integer,
	name text not null,
	url text not NULL,
	CONSTRAINT fk_exercise_how_to_exercise_id Foreign KEY(exercise_id) REFERENCES exercise (id) ON DELETE cascade
);
-- equipment table
-- like dumbbell, changeable weights, 12kg
create table if not exists equipment (
    id integer primary key autoincrement,
    weight_kg real,
    created_at datetime default current_timestamp
);
create table if not exists localized_equipment(
    equipment_id INTEGER NOT NULL,
    language_id INTEGER NOT NULL,
    name TEXT NOT NULL,
    description TEXT,
    how_to TEXT,
    CONSTRAINT fk_localized_equipment_equipment_id FOREIGN KEY (equipment_id) REFERENCES equipment(id) ON DELETE CASCADE,
    CONSTRAINT fk_localized_equipment_language_id FOREIGN KEY (language_id) REFERENCES language(id) ON DELETE CASCADE,
    CONSTRAINT pk_localized_equipment_id PRIMARY KEY (equipment_id, language_id)
);
create index idx_localized_equipment_name on localized_equipment(name);
create index idx_equipment_weight on equipment(weight_kg);
-- what equipment with what exercise
-- dumbbell curls needs a dumbbell, dragon flag a bench or something to hold onto
create table if not exists exercise_equipment (
    exercise_id integer,
    equipment_id integer,
    CONSTRAINT fk_exercise_equipment_exercise_id FOREIGN KEY (exercise_id) references exercise(id) on delete cascade,
    CONSTRAINT fk_exercise_equipment_equipment_id FOREIGN KEY (equipment_id) references equipment(id) on delete cascade,
    CONSTRAINT pk_exercise_equipment_id primary key (exercise_id, equipment_id)
);
create table if not exists user_exercise(
    id integer primary key autoincrement ,
    user_id integer,
    exercise_id integer,
    use_count integer , -- how many times the exercise was logged 'used'
    best_weight real , -- the largest weight ever MAX
    average_weight real, -- the AVG weight of overall records used
    created_at datetime default current_timestamp,
    CONSTRAINT fk_user_exercise_user_id FOREIGN KEY (user_id) references user(id) on delete cascade ,
    CONSTRAINT fk_user_exercise_exercise_id FOREIGN KEY (exercise_id) references exercise(id) on delete cascade
);
-- singular exercise record
-- Dragon flag, 20,0,0,0, "Go Slower", 0, 2024-06-24 00:55:24
-- 0 weight = Body Weight
-- for localization, no need to do anything here
create table if not exists exercise_record (
    user_id integer,
	user_exercise_id integer,
	repetitions integer,
	timer_in_seconds integer,
	distance_in_meters integer,
	weight_used_kg real,
	notes text,
    rest_in_seconds integer,
    incline integer,
    speed integer,
    heart_rate_avg integer,
    KcalBurned integer,
	mood integer,
    -- effort, perceived cause you observe it (￣﹃￣), 10: grueling, 9: really hard, 8: hard . . . etc
    rate_of_perceived_exertion real,
	created_at datetime default current_timestamp,
	CHECK (mood >= 0 AND mood <= 10),
	CHECK (rate_of_perceived_exertion >= 0 AND rate_of_perceived_exertion <= 10),
	CONSTRAINT fk_exercise_record_user_exercise_id Foreign Key(user_exercise_id) references user_exercise(id) ON DELETE cascade,
    CONSTRAINT fk_exercise_record_user_id foreign key (user_id) references user(id) on delete cascade,
    CONSTRAINT pk_exercise_record_id primary key (user_id, user_exercise_id)
);
create index idx_exercise_record_created_at on exercise_record (created_at);
-- represents a singular training session no matter how big or small (a day can have a lot)
-- 00:30:00, 495, "i love rope jumping", 2024-06-24 00:55:24
-- for storing data
-- as i don't really care about the order of the exercises (blocks), i only want to know what i did for data science!.
-- it's types should be gotten from the exercises, for example i did rope jumping and dragon flags
--  -> types: [ Cardio, Calisthenics]
create table if not exists training_session(
	id integer primary key autoincrement,
	duration_in_seconds integer,
	total_calories_burned integer,
	notes text,
	mood integer, -- is this redundant with rpe ?
	feeling text, -- my legs were dead from last time, or something similar
    rate_of_perceived_exertion_avg integer, -- average feeling from all records
	created_at datetime default current_timestamp,
    user_id integer,
	CHECK (mood >= 0 AND mood <= 10),
    CONSTRAINT fk_training_session_user_id foreign key (user_id) references user(id) on delete cascade
);
create index idx_training_session_created_at on training_session (created_at);
-- all training records for a session, allowing this:
-- 00:30:00, 495,Rope jumping,  "i love rope jumping", 2024-06-24 00:55:24
-- 00:05:00, 12, Dragon Flag, 20,20,20, 2024-06-24 00:55:24. Join and grouped on exercise.name, the "20,20,20" are repetitions.
create table if not exists training_session_exercise_record(
	training_session_id integer,
	exercise_record_id integer,
	last_weight_used_kg real, -- should be populated when reading the exercise
	created_at datetime default current_timestamp,
	CONSTRAINT fk_training_session_exercise_record_training_session_id  Foreign Key(training_session_id) references training_session(id) ON DELETE cascade,
	CONSTRAINT fk_training_session_exercise_record_exercise_record_id Foreign Key(exercise_record_id) references exercise_record(id) ON DELETE CASCADE,
    CONSTRAINT pk_training_session_exercise_record_id primary key (training_session_id, exercise_record_id)
);
create index idx_training_session_exercise_record_created_at on training_session_exercise_record( created_at );
-- what type of training have i done
-- why no enums ??!
-- BodyBuilding, Strength Training, Athletics, Cardio, Yoga . . . etc
create table if not exists training_type(
	id integer primary key autoincrement,
    language_id INTEGER NOT NULL,
    name TEXT NOT NULL unique,
    CONSTRAINT fk_training_type_language_id FOREIGN KEY (language_id) REFERENCES language(id) on delete NO ACTION
);
-- what types does an exercise have ; Dragon Flag, [BodyBuilding, Calisthenics, Athletics]
create table if not exists exercise_type(
	exercise_id integer,
	training_type_id integer,
	CONSTRAINT pk_exercise_type_id primary key (exercise_id ,training_type_id),
	CONSTRAINT fk_exercise_type_exercise_id Foreign Key(exercise_id) references exercise (id) ON DELETE cascade ,
	CONSTRAINT fk_exercise_type_training_type_id Foreign Key(training_type_id) references training_type (id) ON DELETE cascade
);
-- overall training plan that you follow for a set of weeks
-- both the type and equipment are aggregated from the exercises inside the plan, not thru a table
-- training weeks and days per week prop can be an aggregate query
create table if not exists training_plan(
	id integer primary key autoincrement,
	name text,
	description text,
	notes text,
	created_at datetime default current_timestamp
);
-- training week record
-- meso_cycle: number of weeks on a meso cycle (4-6 weeks or training, where the last week is recovery focused)
create table if not exists training_week(
	id integer primary key autoincrement,
	name text not null,
	-- mesocycle integer, -- maybe create a different table for this ¯\_(ツ)_/¯
	order_number integer not null,
	created_at datetime default current_timestamp,
	training_plan_id integer,
	CONSTRAINT fk_training_plan_id Foreign Key (training_plan_id ) references training_plan (id) ON DELETE CASCADE,
	check(order_number >= 1)
);
-- a day of training, holds the blocks
-- for viewing the exercises in an ordered manner
-- i can know which muscle was trained via the logged session
create table if not exists training_day(
	id integer primary key autoincrement,
	name text not null,
	notes text,
	order_number integer,
	created_at datetime default current_timestamp,
	training_week_id integer,
	CONSTRAINT fk_training_week_id Foreign Key(training_week_id) references training_week(id) ON DELETE CASCADE,
	check(order_number >= 1)
);
-- a block holds many exercises and denotes their order
-- like a super set of exercises.
-- or a drop set
create table if not exists block(
	id integer primary key autoincrement,
	name text not null,
	`sets` integer,
	rest_in_seconds integer,
	instructions text,
	order_number integer,
	training_day_id integer,
	created_at datetime default current_timestamp,
	CONSTRAINT fk_training_day_id Foreign Key(training_day_id) references training_day(id) ON DELETE CASCADE,
	check(order_number >= 1)
);
-- all exercises in a block
create table if not exists block_exercises(
	id integer primary key autoincrement,
	block_id integer,
	exercise_id integer,
	order_number integer,
	instructions text,
	repetitions integer, -- if it has a rep range to go thru
	timer_in_seconds integer, -- for timed sets
	distance_in_meters integer, -- for walking bullshit exercises, like walking lunges
	created_at datetime default current_timestamp,
	CONSTRAINT fk_block_id Foreign Key(block_id) references block(id) ON DELETE cascade ,
	CONSTRAINT fk_exercise_id Foreign Key(exercise_id) references exercise(id) ON DELETE CASCADE,
	check(order_number >= 1)
);
-- table to keep track of muscle measurements
CREATE TABLE IF NOT EXISTS measurements (
	id integer primary KEY autoincrement ,
	hip real,
	chest real,
	waist_under_belly real,
    waist_on_belly real,
	left_thigh real,
	right_thigh real,
	left_calf real,
	right_calf real,
	left_upper_arm real,
	left_forearm real,
	right_upper_arm real,
	right_forearm real,
	neck real,
    -- InBody Tests
    minerals real,
    protein real,
    total_body_water real,
    body_fat_mass real,
    body_weight real,
    body_fat_percentage real,
    skeletal_muscle_mass real,
    in_body_score real,
    body_mass_index real,
    basal_metabolic_rate integer,
    visceral_fat_level integer,
	created_at datetime default current_timestamp,
    -- one user per measurement, unless the user is a clone :-O
    user_id integer ,
    CONSTRAINT fk_user_id foreign key (user_id) references user(id) on delete cascade
);
create index idx__measurement_date on measurements(created_at);
-- User profile images table
CREATE TABLE IF NOT EXISTS user_profile_images (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    user_id INTEGER,
    is_primary BIT,
    url varchar(255),
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_user_profile_images_user_id FOREIGN KEY (user_id) REFERENCES user(id) ON DELETE CASCADE
);
CREATE INDEX idx_user_profile_images_user_id ON user_profile_images(user_id);
-- User passwords table
CREATE TABLE IF NOT EXISTS user_passwords (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    user_id INTEGER,
    password_hash TEXT NOT NULL,
    password_salt TEXT NOT NULL,
    is_current BIT DEFAULT 1,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_user_passwords_id FOREIGN KEY (user_id) REFERENCES user(id) ON DELETE CASCADE
);
CREATE INDEX idx_user_passwords_user_id ON user_passwords(user_id);
CREATE INDEX idx_user_passwords_is_current ON user_passwords(is_current);
-- user related tables

create table if not exists user_training_plan(
    user_id integer,
    training_plan_id integer,
    start_date datetime,
    end_date datetime,
    is_active BIT DEFAULT 1, -- active
    is_finished BIT default 0, -- not finished
    enrolled_date datetime default current_timestamp,
    constraint pk_user_training_plan_id  primary key (user_id, training_plan_id),
    check ( start_date < end_date )
);
create index idx_user_training_plan_status on user_training_plan(is_finished);


insert into user (username, email, height, gender) VALUES
('Canaan', 'canaan@test.com', 173, 'M'),
('Dante', 'dante@test.com', 200, 'M'),
('Alphrad', 'alphrad@test.com', 172, 'F'),
('Nero', 'nero@test.com', 156, 'F');

insert into user_passwords(user_id, password_hash, password_salt)
VALUES
    (1, '3sWPJCAzV2mDKsWdXLaHkxfli05NH5dAsQJ4U9teYXM=', 'pKHWv1AkNTGeBtkbsm6fIqjJJdYDFIotxXtMupqXm+0='),
    (2, 'BzMDx05wo6wjTriEEjxDsb9jZTa6QkbeFAr7B46CAwc=', 'UqfeVl+kUvwURJ2Avx5nw2+qTBMKUUdrHE5m48RGFU0='),
    (3, 'wA+0773CBog+MlcEjrNcAmHS6pSd06ceminYzPU4wlI=', 'ROtBSjEmAd8swaKpy0e7yV6n5IJl+K5i6yF/brfjTD0='),
    (4, 'wTyWjvmC0bLE1Cqjav3N7UUB62BxMh4X+fkYJclk3h4=', 'yifKALQU7Uw//BXENsSVeZNXc14/qDEkW9V7MqrkVWQ=');