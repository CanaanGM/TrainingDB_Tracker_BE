
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
    username TEXT NOT NULL,
    email TEXT UNIQUE NOT NULL,
    height REAL,
    gender CHAR(1),
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    CHECK (gender IN ('F', 'M', 'U')) -- U -> un specified . . . yet !
);
CREATE INDEX idx_user_username ON user(username);
CREATE UNIQUE INDEX idx_user_email ON user(email);
-- muscles muscles muscles WHAT ? !
create table if not exists muscle (
	id integer primary key autoincrement,
    name TEXT unique NOT NULL,
    muscle_group TEXT NOT NULL,
    function TEXT,
    wiki_page_url VARCHAR
);
-- singular exercise
-- description: the description of the exercise
-- how to: how to perform the exercise
create table if not exists exercise(
	id integer primary key autoincrement,
	difficulty integer default 0,
    name TEXT UNIQUE not NULL,
    description TEXT,
    how_to TEXT,
	CHECK (difficulty >= 0 AND difficulty <= 5)
);
-- what type of training have i done
-- why no enums ??!
-- BodyBuilding, Strength Training, Athletics, Cardio, Yoga . . . etc
create table if not exists training_type(
	id integer primary key autoincrement,
    name TEXT NOT NULL unique
);
create unique index idx_training_type_name on training_type(name);
-- what types does an exercise have ; Dragon Flag, [BodyBuilding, Calisthenics, Athletics]
create table if not exists exercise_type(
	exercise_id integer,
	training_type_id integer,
	CONSTRAINT pk_exercise_type_id primary key (exercise_id ,training_type_id),
	CONSTRAINT fk_exercise_type_exercise_id Foreign Key(exercise_id) references exercise (id) ON DELETE cascade ,
	CONSTRAINT fk_exercise_type_training_type_id Foreign Key(training_type_id) references training_type (id) ON DELETE cascade
);
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
    weight_kg real not null default 0,
    created_at datetime default current_timestamp,
    name TEXT unique NOT NULL,
    description TEXT,
    how_to TEXT
);
create index idx_equipment_weight on equipment(weight_kg);
create index idx_equipemnt_name on equipment(name);
-- what equipment with what exercise
-- dumbbell curls needs a dumbbell, dragon flag a bench or something to hold onto
create table if not exists exercise_equipment (
    exercise_id integer,
    equipment_id integer,
    CONSTRAINT fk_exercise_equipment_exercise_id FOREIGN KEY (exercise_id) references exercise(id) on delete cascade,
    CONSTRAINT fk_exercise_equipment_equipment_id FOREIGN KEY (equipment_id) references equipment(id) on delete cascade,
    CONSTRAINT pk_exercise_equipment_id primary key (exercise_id, equipment_id)
);
-- for metadata about the exercises i used
create table if not exists user_exercise(
    id integer primary key autoincrement ,
    user_id integer,
    exercise_id integer,
    use_count integer not null default 0, -- how many times the exercise was logged 'used', count of unique exercise name in list
    -- for bodyBuilding type exercises
    best_weight real not null default 0, -- the largest weight ever, MAX
    average_weight real not null default 0, -- the AVG weight of overall records used
    last_used_weight_kg real not null default 0, -- should be in a history table, but this is fine for now
    -- for cardio based exercises
    average_timer_in_seconds real,
    average_heart_rate real,
    average_kcal_burned real,
    average_distance real,
    average_speed real,
    average_rate_of_perceived_exertion real,
    created_at datetime default current_timestamp,
    CONSTRAINT fk_user_exercise_user_id FOREIGN KEY (user_id) references user(id) on delete cascade ,
    CONSTRAINT fk_user_exercise_exercise_id FOREIGN KEY (exercise_id) references exercise(id) on delete cascade
);
create table if not exists exercise_record (
    id integer primary key autoincrement,
	exercise_id integer,
    user_id integer,
	repetitions integer not null default 1, -- you always did at least one, hence u created a record
	mood integer not null default 5 , -- 1 ~ 10
	timer_in_seconds integer default null,
	weight_used_kg real not null default 0, -- 0 is body weight
    -- effort, perceived cause you observe it (￣﹃￣), 10: grueling, 9: really hard, 8: hard . . . etc
    rate_of_perceived_exertion real not null default 1,
    rest_in_seconds integer default 10,
    KcalBurned integer not null default 1,
	distance_in_meters integer default null,
	notes text default null,
    incline integer default null,
    speed integer default null,
    heart_rate_avg integer default null,
	created_at datetime default current_timestamp,
	CHECK (mood >= 0 AND mood <= 10),
	CHECK (rate_of_perceived_exertion >= 0 AND rate_of_perceived_exertion <= 10),
	CONSTRAINT fk_exercise_record_exercise_id Foreign Key(exercise_id) references exercise(id) ON DELETE cascade,
    CONSTRAINT fk_exercise_record_user_id foreign key (user_id) references user(id) on delete cascade
);
create table if not exists training_session(
    id integer primary key autoincrement,
    calories integer not null default 0,
    duration_in_seconds real not null default 0,
    mood integer not null default 0,
    feeling text not null default 'good',
    total_kg_moved real,
    total_repetitions real,
    average_rate_of_perceived_exertion real,
    notes text,
    user_id integer,
	created_at datetime default current_timestamp,
    CONSTRAINT fk_training_session_user_id foreign key (user_id) references user(id)
);
create table if not exists training_session_exercise_record(
    training_session_id integer,
    exercise_record_id integer,
    CONSTRAINT pk_training_session_exercise_record_id primary key (training_session_id, exercise_record_id),
    CONSTRAINT fk_training_session_exercise_record_training_session_id foreign key (training_session_id) references training_session(id),
    CONSTRAINT fk_training_session_exercise_record_exercise_record_id foreign key (exercise_record_id) references exercise_record(id)
);
create index idx_exercise_record_rate_of_perceived_exertion on exercise_record(rate_of_perceived_exertion);
create index idx_exercise_record_created_at on exercise_record (created_at);
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
	CONSTRAINT fk_training_week_training_plan_id Foreign Key (training_plan_id ) references training_plan (id) ON DELETE CASCADE,
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
	CONSTRAINT fk_block_training_day_id Foreign Key(training_day_id) references training_day(id) ON DELETE CASCADE,
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
	CONSTRAINT fk_block_exercises_block_id Foreign Key(block_id) references block(id) ON DELETE cascade ,
	CONSTRAINT fk_block_exercises_exercise_id Foreign Key(exercise_id) references exercise(id) ON DELETE CASCADE,
	check(order_number >= 1)
);
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
create table if not exists role(
    id integer primary key autoincrement,
    name text unique not null
);
create table if not exists user_roles(
    user_id integer not null,
    role_id integer not null,
    created_at datetime default current_timestamp,
    CONSTRAINT pk_user_roles_id PRIMARY KEY (user_id, role_id),
    CONSTRAINT fk_user_roles_user_id FOREIGN KEY (user_id) references user(id) on delete cascade,
    CONSTRAINT fk_user_roles_role_id FOREIGN KEY (role_id) references role(id) on delete cascade
);
create index idx_role_name on role(name);
create table if not exists refresh_token(
    id integer primary key autoincrement,
    user_id integer,
    token text not null,
    expires datetime default (DATETIME('now', '+7 days')),
    revoked datetime,
    active bit default 0, -- 1 active, 0 not
    CONSTRAINT fk_refresh_token_user_id FOREIGN KEY (user_id) references user(id)
);
create index idx_refresh_token_token on refresh_token(token);
-- TODO: REMOVE LATER!
insert into user (username, email, height, gender) VALUES
    ('Canaan', 'canaan@test.com', 173, 'M'),
    ('Dante', 'dante@test.com', 200, 'M'),
    ('Alphrad', 'alphrad@test.com', 172, 'F'),
    ('Nero', 'nero@test.com', 156, 'F');
insert into user_passwords(user_id, password_hash, password_salt)
        VALUES
(1, 'iUXNkjg5RS/0/uEH8f6tasaKKKt8IVsZgTTtyGH3dmQ=', 'wnGHc9SponguR0Givr2Zcvy7UI3Szs0y0lYfAwdatUE='),
(2, 'BzMDx05wo6wjTriEEjxDsb9jZTa6QkbeFAr7B46CAwc=', 'UqfeVl+kUvwURJ2Avx5nw2+qTBMKUUdrHE5m48RGFU0='),
(3, 'wA+0773CBog+MlcEjrNcAmHS6pSd06ceminYzPU4wlI=', 'ROtBSjEmAd8swaKpy0e7yV6n5IJl+K5i6yF/brfjTD0='),
(4, 'Kw1vjlzykwUqruCdhzdMOQBuOkn4hljD3JLiOdAogWg=', 'a5MaAfB/7zpN8dd3+i10RgHGJ455lpoSIOrAXXXQbyY=');
insert into role(name) values
('user'),
('admin'),
('owner'),
('thieves');
insert into user_roles(user_id, role_id) VALUES
 (1, 1),
 (1,2),
 (1,3),
 (2,1),
 (3,1),
 (4,1);
