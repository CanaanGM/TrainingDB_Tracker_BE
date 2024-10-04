
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

create unique index idx_muscle_name on muscle(name);
create index idx_muscle_group on muscle(muscle_group);
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
create unique index idx_exercise_name on exercise(name);
create index idx_exercise_difficulty on exercise(difficulty);

create table if not exists exercise_image (
    id integer primary key autoincrement ,
    name text not null,
    url text not null,
    exercise_id integer not null,
    is_primary bit default 1, -- either 1 = true or 0 false.
    created_at datetime default current_timestamp, -- so the newest is the primary
    CONSTRAINT fk_exercise_image_exercise_id FOREIGN KEY (exercise_id) references exercise(id) on delete cascade
);
create index idx_exercise_image_name on exercise_image(name);
create index idx_exercise_image_status on exercise_image(is_primary);

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
create index idx_exercise_how_to on exercise_how_to (name);
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
(1, '$2a$11$hCdSz2IWtWhfSMu5HU1xe.YA6zrxged3TNHoZC/CycqNpaYS7ci4W', '$2a$11$hCdSz2IWtWhfSMu5HU1xe.'),
(2, '2a$11$v68jMQkfWr9OS4BHPe20keuztD79mByxoBc2OJFOvO0dBBXPlmQ4e', '$2a$11$v68jMQkfWr9OS4BHPe20ke'),
(3, '$2a$11$FHNqTyAalmLYbaOpwJ683OY7krQV58AT94Vc6cICI3ihcP4A2jIwG', '$2a$11$FHNqTyAalmLYbaOpwJ683O'),
(4, '$2a$11$YyB7Yu/pMRy/8xHEHlWJgOUfKUpJwBAq4Im.leW/gTWDzOatDvqai', '$2a$11$YyB7Yu/pMRy/8xHEHlWJgO');
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

--- INSERTION

insert into muscle (name, muscle_group, function, wiki_page_url)
values (
    'upper trapezius',
    'back',
    'elevates the scapula and extends the head at the neck.',
    'https://en.wikipedia.org/wiki/Trapezius_muscle'
  ),
  (
    'middle trapezius',
    'back',
    'retracts the scapula.',
    'https://en.wikipedia.org/wiki/Trapezius_muscle'
  ),
  (
    'lower trapezius',
    'back',
    'depresses the scapula.',
    'https://en.wikipedia.org/wiki/Trapezius_muscle'
  ),
  (
    'latissimus dorsi',
    'back',
    'extends, adducts, and medially rotates the arm.',
    'https://en.wikipedia.org/wiki/Latissimus_dorsi_muscle'
  ),
  (
    'rhomboid',
    'back',
    'retracts and elevates the scapula.',
    'https://en.wikipedia.org/wiki/Rhomboid_muscles'
  ),
  (
    'erector spinae',
    'back',
    'extends and laterally flexes the spine.',
    'https://en.wikipedia.org/wiki/Erector_spinae_muscles'
  ),
  (
    'infraspinatus',
    'back',
    'laterally rotates the arm and stabilizes the shoulder joint.',
    'https://en.wikipedia.org/wiki/Infraspinatus_muscle'
  ),
  (
    'teres major',
    'back',
    'adducts and medially rotates the arm.',
    'https://en.wikipedia.org/wiki/Teres_major_muscle'
  ),
  (
    'teres minor',
    'back',
    'laterally rotates the arm and stabilizes the shoulder joint.',
    'https://en.wikipedia.org/wiki/Teres_minor_muscle'
  ),
  (
    'thoracolumbar fascia',
    'back',
    'load transfer between the trunk and limb (it is tensioned by the action of the latissimus dorsi muscle, gluteus maximus muscle, and the hamstring muscles), and lifting.',
    'https://en.wikipedia.org/wiki/Thoracolumbar_fascia'
  ),
  (
    'sternocleidomastoid',
    'neck',
    'flexes and laterally rotates the head.',
    'https://en.wikipedia.org/wiki/Sternocleidomastoid_muscle'
  ),
  (
    'omohyoid',
    'neck',
    'depresses the hyoid bone and larynx.',
    'https://en.wikipedia.org/wiki/Omohyoid_muscle'
  ),
  (
    'sternohyoid',
    'neck',
    'depresses the hyoid bone after it has been elevated during swallowing.',
    'https://en.wikipedia.org/wiki/Sternohyoid_muscle'
  ),
  (
    'longus colli',
    'neck',
    'forward and lateral flexion of the neck, as well as rotation of the neck.',
    'https://en.wikipedia.org/wiki/Longus_colli_muscle'
  ),
  (
    'longus capitis',
    'neck',
    'bilateral contraction - head flexion; unilateral contraction - head rotation.',
    'https://en.wikipedia.org/wiki/Longus_capitis_muscle'
  ),
  (
    'scalenes',
    'neck',
    'the anterior and middle scalene muscles lift the first rib and bend the neck to the same side as the acting muscle the posterior scalene lifts the second rib and tilts the neck to the same side.',
    'https://en.wikipedia.org/wiki/Scalene_muscles'
  ),
  (
    'pectoralis major upper portion',
    'chest',
    'flexes, adducts, and medially rotates the arm.',
    'https://en.wikipedia.org/wiki/Pectoralis_major_muscle'
  ),
  (
    'pectoralis major lower portion',
    'chest',
    'flexes, adducts, and medially rotates the arm.',
    'https://en.wikipedia.org/wiki/Pectoralis_major_muscle'
  ),
  (
    'pectoralis minor',
    'chest',
    'stabilizes the scapula by drawing it inferiorly and anteriorly against the thoracic wall.',
    'https://en.wikipedia.org/wiki/Pectoralis_minor_muscle'
  ),
  (
    'deltoid anterior head',
    'shoulders',
    'flexes and medially rotates the arm.',
    'https://en.wikipedia.org/wiki/Deltoid_muscle#Anterior_part'
  ),
  (
    'deltoid posterior head',
    'shoulders',
    'extends and laterally rotates the arm.',
    'https://en.wikipedia.org/wiki/Deltoid_muscle#Posterior_part'
  ),
  (
    'deltoid middle head',
    'shoulders',
    'abducts the arm.',
    'https://en.wikipedia.org/wiki/Deltoid_muscle#Middle_part'
  ),
  (
    'biceps brachii short head',
    'biceps',
    'flexes the elbow joint and supinates the forearm.',
    'https://en.wikipedia.org/wiki/Biceps'
  ),
  (
    'biceps brachii long head',
    'biceps',
    'flexes the elbow joint and supinates the forearm.',
    'https://en.wikipedia.org/wiki/Biceps'
  ),
  (
    'brachialis',
    'biceps',
    'mucle in the upper arm the flexes the elbow.',
    'https://en.wikipedia.org/wiki/Brachialis_muscle'
  ),
  (
    'triceps brachii long head',
    'triceps',
    'employed when sustained force generation is demanded, or when there is a need for a synergistic control of the shoulder and elbow or both.',
    'https://en.wikipedia.org/wiki/Triceps'
  ),
  (
    'triceps brachii lateral head',
    'triceps',
    'used for movements requiring occasional high-intensity force.',
    'https://en.wikipedia.org/wiki/Triceps'
  ),
  (
    'triceps brachii medial head',
    'triceps',
    'enables more precise, low-force movements.',
    'https://en.wikipedia.org/wiki/Triceps'
  ),
  (
    'brachioradialis',
    'forearms',
    'flexes the forearms at the elbow joint.',
    'https://en.wikipedia.org/wiki/Brachioradialis_muscle'
  ),
  (
    'pronator teres',
    'forearms',
    'pronates the forearm and flexes the elbow joint.',
    'https://en.wikipedia.org/wiki/Pronator_teres_muscle'
  ),
  (
    'flexor carpi ulnaris',
    'forearms',
    'flexes and adducts the hand at the wrist.',
    'https://en.wikipedia.org/wiki/Flexor_carpi_ulnaris_muscle'
  ),
  (
    'flexor carpi radialis',
    'forearms',
    'flexes and abducts the hand at the wrist.',
    'https://en.wikipedia.org/wiki/Flexor_carpi_radialis_muscle'
  ),
  (
    'flexor digitorum superficialis',
    'forearms',
    'flexes the fingers at the proximal interphalangeal joints.',
    'https://en.wikipedia.org/wiki/Flexor_digitorum_superficialis_muscle'
  ),
  (
    'flexor digitorum profundus',
    'forearms',
    'flexes the fingers at the distal interphalangeal joints.',
    'https://en.wikipedia.org/wiki/Flexor_digitorum_profundus_muscle'
  ),
  (
    'biceps femoris',
    'hamstrings',
    'flexes the knee joint and laterally rotates the leg.',
    'https://en.wikipedia.org/wiki/Biceps_femoris_muscle'
  ),
  (
    'semimembranosus',
    'hamstrings',
    'flexes the knee joint and medially rotates the leg.',
    'https://en.wikipedia.org/wiki/Semimembranosus_muscle'
  ),
  (
    'semitendinosus',
    'hamstrings',
    'flexes the knee joint and medially rotates the leg.',
    'https://en.wikipedia.org/wiki/Semitendinosus_muscle'
  ),
  (
    'rectus femoris',
    'quadriceps',
    'flexes the hip and extends the knee.',
    'https://en.wikipedia.org/wiki/Rectus_femoris_muscle'
  ),
  (
    'vastus lateralis',
    'quadriceps',
    'extends the knee and stabilizes the patella.',
    'https://en.wikipedia.org/wiki/Vastus_lateralis_muscle'
  ),
  (
    'vastus medialis',
    'quadriceps',
    'extends the knee and stabilizes the patella.',
    'https://en.wikipedia.org/wiki/Vastus_medialis_muscle'
  ),
  (
    'vastus intermedius',
    'quadriceps',
    'extends the knee.',
    'https://en.wikipedia.org/wiki/Vastus_intermedius_muscle'
  ),
  (
    'gluteus maximus',
    'glutes',
    'extends and laterally rotates the hip.',
    'https://en.wikipedia.org/wiki/Gluteus_maximus_muscle'
  ),
  (
    'gluteus medius',
    'glutes',
    'abducts and medially rotates the hip.',
    'https://en.wikipedia.org/wiki/Gluteus_medius_muscle'
  ),
  (
    'gluteus minimus',
    'glutes',
    'abducts and medially rotates the hip.',
    'https://en.wikipedia.org/wiki/Gluteus_minimus_muscle'
  ),
  (
    'rectus abdominis',
    'core',
    'flexes the trunk and compresses the abdomen.',
    'https://en.wikipedia.org/wiki/Rectus_abdominis_muscle'
  ),
  (
    'external oblique',
    'core',
    'flexes the trunk, rotates the trunk, and compresses the abdomen.',
    'https://en.wikipedia.org/wiki/External_oblique_muscle'
  ),
  (
    'internal oblique',
    'core',
    'flexes the trunk, rotates the trunk, and compresses the abdomen.',
    'https://en.wikipedia.org/wiki/Internal_oblique_muscle'
  ),
  (
    'psoas major',
    'core',
    'flexion in the hip joint.',
    'https://en.wikipedia.org/wiki/Psoas_major_muscle'
  ),
  (
    'transversus abdominis',
    'core',
    'compresses the abdomen.',
    'https://en.wikipedia.org/wiki/Transversus_abdominis_muscle'
  ),
  (
    'gastrocnemius',
    'calves',
    'plantar flexes the ankle and flexes the knee.',
    'https://en.wikipedia.org/wiki/Gastrocnemius_muscle'
  ),
  (
    'soleus',
    'calves',
    'plantar flexes the ankle.',
    'https://en.wikipedia.org/wiki/Soleus_muscle'
  ),
  (
    'tibialis anterior',
    'lower legs',
    'dorsiflexes and inverts the foot.',
    'https://en.wikipedia.org/wiki/Tibialis_anterior_muscle'
  ),
  (
    'peroneus longus',
    'lower legs',
    'plantar flexes and everts the foot.',
    'https://en.wikipedia.org/wiki/Peroneus_longus_muscle'
  ),
  (
    'peroneus brevis',
    'lower legs',
    'plantar flexes and everts the foot.',
    'https://en.wikipedia.org/wiki/Peroneus_brevis_muscle'
  ),
  (
    'adductor longus',
    'adductor',
    'its main function is to adduct the thigh and it is innervated by the obturator nerve.',
    'https://en.wikipedia.org/wiki/Adductor_longus_muscle'
  ),
  (
    'adductor brevis',
    'adductor',
    'main function of the adductor brevis is to pull the thigh medially.',
    'https://en.wikipedia.org/wiki/Adductor_brevis_muscle'
  ),
  (
    'adductor magnus',
    'adductor',
    'the adductor magnus is a powerful adductor of the thigh, made especially active when the legs are moved from a wide spread position to one in which the legs parallel each other.',
    'https://en.wikipedia.org/wiki/Adductor_magnus_muscle'
  ),
  (
    'sartorius',
    'thigh',
    'can move the hip joint and the knee joint.',
    'https://en.wikipedia.org/wiki/Sartorius_muscle'
  ),
  (
    'pectineus',
    'thigh',
    'hip flection and adduction.',
    'https://en.wikipedia.org/wiki/Pectineus_muscle'
  ),
  (
    'external obturator',
    'thigh',
    'lateral rotator of the hip joint. as a short muscle around the hip joint, it stabilizes the hip joint as a postural muscle.',
    'https://en.wikipedia.org/wiki/External_obturator_muscle'
  ),
  (
    'gracilis',
    'thigh',
    'the muscle adducts, medially rotates (with hip flexion), laterally rotates, and flexes the hip as above, and also aids in flexion of the knee.',
    'https://en.wikipedia.org/wiki/Gracilis_muscle'
  ),
  (
    'iliacus',
    'thigh',
    'in open-chain exercises, as part of the iliopsoas, the iliacus is important for lifting (flexing) the femur forward (e.g. front scale). in closed-chain exercises, the iliopsoas bends the trunk forward and can lift the trunk from a lying posture.',
    'https://en.wikipedia.org/wiki/Iliacus_muscle'
  ),
  (
    'tensor fasciae latae',
    'thigh',
    'stabilize the hip in extension (assists gluteus maximus during hip extension).',
    'https://en.wikipedia.org/wiki/Tensor_fasciae_latae_muscle'
  ),
  (
    'serratus anterior',
    'core',
    'occasionally called the big swing muscle or boxer''s muscle because it is largely responsible for the protraction of the scapula — that is, the pulling of the scapula forward and around the rib cage that occurs when someone throws a punch.',
    'https://en.wikipedia.org/wiki/Serratus_anterior_muscle'
  ),
  (
    'levator scapulae',
    'neck',
    'elevates the scapula and tilts its glenoid cavity inferiorly by rotating the scapula; assists in neck extension and lateral neck flexion.',
    'https://en.wikipedia.org/wiki/Levator_scapulae_muscle'
  ),
  (
    'supraspinatus',
    'shoulders',
    'the supraspinatus muscle performs abduction of the arm, and pulls the head of the humerus medially towards the glenoid cavity.',
    'https://en.wikipedia.org/wiki/Supraspinatus_muscle'
  );

insert into training_type (name)
values ('bodybuilding'),
  ('strength'),
  ('yoga'),
  ('athletics'),
  ('karate'),
  ('stretch'),
  ('stability'),
  ('cardiovascular'),
  ('agility'),
  ('endurance'),
  ('rehabilitation'),
  ('mobility'),
  ('power'),
  ('speed'),
  ('flexibility'),
  ('calisthenics'),
  ('coordination'),
  ('functional'),
  ('warm-up'),
  ('plyometrics'),
  ('powerlifting');


insert into exercise (name, description, how_to, difficulty)
values (
                'rope jumping',
                'Rope jumping, also known as skipping, is a cardiovascular exercise that involves jumping over a rope that is swung under the feet and over the head. It is a versatile exercise that can be performed almost anywhere and offers a wide range of benefits, including improved cardiovascular fitness, coordination, and agility.',
                '1. Hold the rope handles at each end, with the rope behind your feet.
2. Swing the rope over your head and jump over it as it reaches your feet.
3. Keep your jumps small and land softly on the balls of your feet.
4. Maintain a consistent rhythm and pace.',
                2
        ),
        (
                'squats',
                'Squats are a fundamental lower body exercise that primarily targets the muscles of the thighs, hips, and buttocks. They are a key exercise for building lower body strength, improving mobility, and enhancing overall athletic performance.',
                '1. Stand with your feet shoulder-width apart and your toes slightly turned out.
2. Keep your chest up and your core engaged as you lower your body by bending your knees and hips.
3. Descend until your thighs are parallel to the ground or as low as your mobility allows.
4. Push through your heels to return to the starting position, fully extending your hips and knees.',
                3
        ),
        (
                'high bar squats',
                'High bar squats are a variation of the traditional squat where the barbell is placed high on the traps. This position encourages a more upright torso during the squat, placing greater emphasis on the quadriceps while still engaging the glutes and hamstrings. It is commonly used in Olympic weightlifting and bodybuilding.',
                '1. Position the barbell high on your upper traps, just below the base of your neck.
2. Grip the barbell with your hands slightly wider than shoulder-width apart.
3. Stand with your feet shoulder-width apart and toes slightly turned out.
4. Keep your chest up and your core engaged as you lower your body by bending your knees and hips.
5. Descend until your thighs are parallel to the ground or as low as your mobility allows.
6. Push through your heels to return to the starting position, fully extending your hips and knees.',
                3
        ),
        (
                'bent over barbell row',
                'The barbell bent over row is a compound exercise that targets the back muscles, specifically the latissimus dorsi, rhomboids, and trapezius. It also engages the biceps and the muscles of the forearms. This exercise helps build a strong and muscular back, improving posture and upper body strength.',
                '1. Stand with your feet shoulder-width apart, holding a barbell with an overhand grip.
2. Bend your knees slightly and hinge at the hips, keeping your back straight and your chest up.
3. Lower the barbell to just below your knees, keeping your arms extended.
4. Pull the barbell towards your torso, squeezing your shoulder blades together at the top of the movement.
5. Lower the barbell back to the starting position with control.
6. Maintain a neutral spine throughout the exercise and avoid rounding your back.',
                3
        ),
        (
                'floor press',
                'The floor press is a variation of the bench press performed while lying on the floor. This exercise primarily targets the chest, triceps, and shoulders. It limits the range of motion, reducing stress on the shoulders and focusing on the lockout portion of the lift.',
                '1. Lie on the floor under a barbell rack or with a barbell placed on the floor behind you.
2. Position the barbell over your chest and grasp it with a slightly wider than shoulder-width grip.
3. Bend your knees and place your feet flat on the floor for stability.
4. Lift the barbell off the rack or floor and hold it above your chest with arms fully extended.
5. Lower the barbell slowly until your upper arms touch the floor, keeping your elbows at a 45-degree angle to your body.
6. Press the barbell back up to the starting position, fully extending your arms.
7. Maintain a tight core and avoid arching your back throughout the movement.',
                3
        ),
        (
                'machine shoulder press',
                'The machine shoulder press is a strength training exercise that targets the shoulder muscles, specifically the deltoids. Using a machine helps to stabilize the movement and ensure proper form, making it a great option for beginners or those recovering from injuries.',
                '1. Adjust the seat height so that the handles are at shoulder level.
2. Sit down and grasp the handles with an overhand grip, palms facing forward.
3. Keep your back against the pad and your feet flat on the floor.
4. Press the handles upward until your arms are fully extended but not locked out.
5. Slowly lower the handles back to the starting position.
6. Maintain a controlled movement throughout the exercise and avoid arching your back.',
                2
        ),
        (
                'narrow grip shoulder press machine',
                'The narrow grip shoulder press machine is a variation of the shoulder press that targets the deltoid muscles with a focus on the deltoid anterior heads. The narrower grip also engages the triceps more compared to the standard shoulder press. Using a machine helps stabilize the movement and ensure proper form.',
                '1. Adjust the seat height so that the handles are at shoulder level.
2. Sit down and grasp the handles with a narrow overhand grip, palms facing forward.
3. Keep your back against the pad and your feet flat on the floor.
4. Press the handles upward until your arms are fully extended but not locked out.
5. Slowly lower the handles back to the starting position.
6. Maintain a controlled movement throughout the exercise and avoid arching your back.',
                2
        ),
        (
                'bent over dumbbell rear delt raise',
                'The bent over dumbbell rear delt raise is an isolation exercise that targets the rear deltoid muscles. It helps to improve shoulder stability and strength, as well as enhance overall shoulder aesthetics.',
                '1. Hold a dumbbell in each hand with a neutral grip (palms facing each other).
2. Bend at the hips until your torso is nearly parallel to the floor, keeping a slight bend in your knees.
3. Let the dumbbells hang directly below your shoulders, with your elbows slightly bent.
4. Raise the dumbbells towards your back, squeezing your shoulder blades together at the top of the movement.
5. Slowly lower the dumbbells back to the starting position with control.
6. Keep your core engaged and maintain a straight back throughout the exercise.',
                2
        ),
        (
                'incline chest press machine',
                'The incline chest press machine is a strength training exercise that targets the upper portion of the pectoral muscles, as well as the triceps and shoulders. Using a machine helps to stabilize the movement and ensure proper form.',
                '1. Adjust the seat height so that the handles are at chest level when you are seated.
2. Sit down and grasp the handles with an overhand grip, palms facing forward.
3. Keep your back against the pad and your feet flat on the floor.
4. Press the handles upward and forward until your arms are fully extended but not locked out.
5. Slowly lower the handles back to the starting position with control.
6. Maintain a tight core and avoid arching your back throughout the exercise.',
                2
        ),
        (
                'standing calf raise machine',
                'The standing calf raise machine is a strength training exercise that targets the calf muscles, specifically the gastrocnemius and soleus. It helps to build calf muscle strength and definition.',
                '1. Position your shoulders under the pads and the balls of your feet on the edge of the platform.
2. Stand upright with your torso straight and your knees slightly bent.
3. Lower your heels as far as possible to stretch your calves.
4. Raise your heels as high as possible by extending your ankles and contracting your calf muscles.
5. Hold the top position for a moment before lowering your heels back to the starting position.
6. Maintain a controlled movement throughout the exercise and avoid bouncing.',
                2
        ),
        (
                'tibialis raises',
                'Tibialis raises are an exercise that targets the tibialis anterior muscle located at the front of the lower leg. This exercise helps to strengthen the shin muscles, improve ankle stability, and prevent injuries such as shin splints.',
                '1. Stand with your back against a wall and your heels a few inches away from the wall.
2. Keep your feet flat on the floor and your knees slightly bent.
3. Raise your toes towards your shins as high as possible, keeping your heels on the floor.
4. Hold the top position for a moment, feeling the contraction in your tibialis anterior.
5. Slowly lower your toes back to the starting position with control.
6. Repeat the movement for the desired number of repetitions.',
                1
        ),
        (
                'cable triceps pushdown',
                'The cable triceps pushdown is an isolation exercise that targets the triceps muscles. It helps to build strength and size in the triceps, which are essential for overall upper body strength and aesthetics.',
                '1. Attach a straight bar or rope to a high pulley on a cable machine.
2. Stand facing the machine with your feet shoulder-width apart and grasp the bar with an overhand grip.
3. Keep your elbows close to your sides and your upper arms stationary throughout the exercise.
4. Push the bar down by extending your elbows until your arms are fully extended.
5. Hold the contraction for a moment at the bottom of the movement.
6. Slowly return the bar to the starting position with control, keeping your elbows tight to your sides.
7. Repeat the movement for the desired number of repetitions.',
                2
        ),
        (
                'seated leg curl',
                'The seated leg curl is an isolation exercise that targets the hamstrings. It helps to build strength and size in the hamstrings, which are essential for overall lower body strength and stability.',
                '1. Adjust the machine so that the back pad is comfortable and the ankle pad rests just above your heels.
2. Sit on the machine and place your legs under the ankle pad with your feet pointed forward.
3. Grip the handles or sides of the seat for stability.
4. Curl your legs by flexing your knees and bringing your heels towards your buttocks.
5. Hold the contraction for a moment at the top of the movement.
6. Slowly return the weight to the starting position with control, extending your legs fully.
7. Repeat the movement for the desired number of repetitions.',
                2
        ),
        (
                'leg extension',
                'The leg extension is an isolation exercise that primarily targets the quadriceps muscles. It helps to build strength and size in the quadriceps, which are essential for overall lower body strength and knee stability.',
                '1. Adjust the machine so that the back pad is comfortable and the ankle pad rests just above your feet.
2. Sit on the machine and place your legs under the ankle pad with your feet pointed forward.
3. Grip the handles or sides of the seat for stability.
4. Extend your legs by straightening your knees and lifting the weight.
5. Hold the contraction for a moment at the top of the movement.
6. Slowly return the weight to the starting position with control, bending your knees fully.
7. Repeat the movement for the desired number of repetitions.',
                2
        ),
        (
                'seated hip adduction machine',
                'The seated hip adduction machine is an isolation exercise that targets the inner thigh muscles, specifically the adductors. It helps to strengthen the adductor muscles, improving lower body stability and coordination.',
                '1. Sit on the machine and place your legs on the outside of the pads.
2. Adjust the machine settings so that your legs are comfortably apart.
3. Grip the handles on the sides of the machine for stability.
4. Squeeze your legs together, bringing the pads inward as you contract your adductor muscles.
5. Hold the contraction for a moment at the end of the movement.
6. Slowly return the pads to the starting position with control.
7. Repeat the movement for the desired number of repetitions.',
                1
        ),
        (
                'cable wood choppers',
                'Cable wood choppers are a dynamic exercise that targets the core muscles, particularly the obliques. This exercise mimics the chopping motion and helps improve rotational strength and stability.',
                '1. Set a cable pulley to core height and attach a handle.
2. Stand sideways to the machine with your feet shoulder-width apart.
3. Grasp the handle with both hands, arms fully extended at core height.
4. Rotate your torso and pull the handle horizontally across your body to the opposite side, keeping your core engaged.
5. Allow your hips and knees to rotate naturally with the movement.
6. Return to the starting position with control.
7. Repeat for the desired number of repetitions, then switch sides.',
                2
        ),
        (
                'hyper extensions',
                'Hyper extensions are an exercise that primarily targets the lower back muscles, specifically the erector spinae. This exercise also engages the glutes and hamstrings, helping to improve lower back strength and stability.',
                '1. Adjust the hyperextension bench so that your hips rest on the pad and your feet are secured under the foot pads.
2. Cross your arms over your chest or place your hands behind your head.
3. Keeping your back straight, bend at the waist to lower your upper body towards the floor.
4. Lower until your torso is just above perpendicular to the floor.
5. Raise your upper body back to the starting position by extending your back and engaging your glutes and hamstrings.
6. Avoid hyperextending your back at the top of the movement.
7. Repeat for the desired number of repetitions.',
                2
        ),
        (
                'lat pulldown - wide grip',
                'The wide grip lat pulldown is a compound exercise that targets the back muscles, particularly the latissimus dorsi. It helps to build a wide and strong back, improving upper body strength and posture.',
                '1. Sit at a lat pulldown machine and adjust the knee pad to secure your legs.
2. Grasp the bar with a wide overhand grip, wider than shoulder-width apart.
3. Pull the bar down towards your upper chest, bringing your shoulder blades together and keeping your chest up.
4. Avoid leaning back excessively during the pull.
5. Slowly return the bar to the starting position with control, fully extending your arms.
6. Repeat for the desired number of repetitions.',
                2
        ),
        (
                'machine flye',
                'The machine flye is an isolation exercise that targets the chest muscles, specifically the pectoralis major. It helps to build chest strength and definition, and is performed using a machine to ensure proper form and stability.',
                '1. Adjust the seat height so that the handles are at chest level when you are seated.
2. Sit down and grasp the handles with an overhand grip, palms facing forward.
3. Keep your back against the pad and your feet flat on the floor.
4. Press your arms together in a hugging motion, bringing the handles in front of your chest.
5. Hold the contraction for a moment at the end of the movement.
6. Slowly return the handles to the starting position with control, allowing your chest to stretch.
7. Repeat the movement for the desired number of repetitions.',
                2
        ),
        (
                'seated hip abductor',
                'The seated hip abductor machine is an isolation exercise that targets the muscles of the outer thighs, specifically the gluteus medius and minimus. This exercise helps to strengthen the hip abductors, improving hip stability and overall lower body strength.',
                '1. Sit on the machine and place your legs on the inside of the pads.
2. Adjust the machine settings so that your legs are comfortably close together.
3. Grip the handles on the sides of the machine for stability.
4. Push your legs outward against the pads, spreading them apart as you contract your hip abductors.
5. Hold the contraction for a moment at the end of the movement.
6. Slowly return the pads to the starting position with control.
7. Repeat the movement for the desired number of repetitions.',
                1
        ),
        (
                'jogging',
                'Jogging is a cardiovascular exercise that involves running at a steady, moderate pace. It helps to improve cardiovascular fitness, build endurance, and maintain overall health.',
                '1. Start by standing upright with your feet shoulder-width apart.
2. Begin running at a steady, moderate pace.
3. Keep your head up, back straight, and arms bent at a 90-degree angle.
4. Swing your arms naturally with each stride, keeping your shoulders relaxed.
5. Land on the midfoot and push off with your toes, maintaining a smooth and consistent stride.
6. Breathe rhythmically and maintain a conversational pace.
7. Continue jogging for the desired duration or distance.',
                2
        ),
        (
                'cable crunch',
                'The rope crunch is an isolation exercise that targets the abdominal muscles, particularly the rectus abdominis. This exercise helps to build core strength and improve abdominal definition.',
                '1. Attach a rope handle or a straight bar to a high pulley on a cable machine.
2. Kneel in front of the machine, holding the rope handles with both hands, and position the rope behind your head.
3. Keep your hips stationary and contract your abs to pull your elbows towards your thighs.
4. Focus on curling your spine and bringing your chest towards your knees.
5. Hold the contraction for a moment at the bottom of the movement.
6. Slowly return to the starting position with control, keeping tension on your abs.
7. Repeat the movement for the desired number of repetitions.',
                2
        ),
        (
                'dumbbell stiff legged deadlift',
                'The dumbbell stiff legged deadlift is an exercise that primarily targets the hamstrings and glutes. It also engages the lower back muscles, helping to improve posterior chain strength and flexibility.',
                '1. Stand with your feet shoulder-width apart, holding a dumbbell in each hand with your palms facing your thighs.
2. Keep your knees slightly bent and your back straight.
3. Hinge at the hips to lower the dumbbells towards your feet, keeping them close to your body.
4. Lower the dumbbells until you feel a stretch in your hamstrings, keeping your back flat.
5. Squeeze your glutes and hamstrings to raise your torso back to the starting position.
6. Maintain a controlled movement throughout the exercise and avoid rounding your back.
7. Repeat the movement for the desired number of repetitions.',
                3
        ),
        (
                'fast walking',
                'Fast walking, also known as power walking, is a cardiovascular exercise that involves walking at a brisk pace. It helps to improve cardiovascular fitness, build endurance, and maintain overall health.',
                '1. Start by standing upright with your feet shoulder-width apart.
2. Begin walking at a brisk pace, swinging your arms naturally with each stride.
3. Keep your head up, back straight, and shoulders relaxed.
4. Maintain a steady and quick pace, aiming for a speed that is faster than a regular walk but slower than a jog.
5. Land on the heel of your foot and roll through to push off with your toes, maintaining a smooth and consistent stride.
6. Breathe rhythmically and maintain good posture throughout the walk.
7. Continue walking for the desired duration or distance.',
                1
        ),
        (
                'machine glute kickback',
                'The machine glute kickback is an isolation exercise that targets the gluteus maximus. It helps to build strength and size in the glutes, enhancing lower body strength and aesthetics.',
                '1. Adjust the machine so that the pad is positioned just above your ankle.
2. Stand facing the machine, hold onto the handles, and place one foot on the platform.
3. With a slight bend in your knee, kick your working leg back in a controlled motion, extending your hip and contracting your glute.
4. Hold the contraction for a moment at the top of the movement.
5. Slowly return your leg to the starting position with control.
6. Repeat for the desired number of repetitions, then switch legs.',
                2
        ),
        (
                'barbell french press',
                'The barbell French press, also known as the barbell skull crusher, is an isolation exercise that targets the triceps. It helps to build strength and size in the triceps, which are essential for overall upper body strength and aesthetics.',
                '1. Lie flat on a bench with your feet firmly planted on the floor.
2. Hold a barbell with a close grip, palms facing up, and extend your arms above your chest.
3. Lower the barbell slowly towards your forehead by bending your elbows, keeping your upper arms stationary.
4. Stop just before the barbell reaches your forehead.
5. Extend your elbows to press the barbell back to the starting position, fully extending your arms.
6. Maintain control throughout the movement and avoid flaring your elbows out.
7. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'dips',
                'Dips are a compound upper body exercise that primarily targets the triceps, chest, and shoulders. This exercise helps to build strength and muscle mass in the upper body.',
                '1. Grasp the parallel dip bars firmly and lift your body, keeping your arms fully extended.
2. Lean slightly forward to engage your chest, or keep your torso upright to focus on the triceps.
3. Lower your body by bending your elbows until your upper arms are parallel to the floor.
4. Push yourself back up to the starting position by extending your elbows, fully straightening your arms.
5. Keep your core engaged and avoid swinging your legs.
6. Repeat the movement for the desired number of repetitions.',
                3
        ),
        (
                'dumbbell chest press - slight incline',
                'The slight incline dumbbell press is a variation of the dumbbell bench press that targets the upper portion of the pectoral muscles. By using a slight incline, this exercise helps to build upper chest strength and improve overall chest definition.',
                '1. Adjust an incline bench to a slight incline, around 15-30 degrees.
2. Sit on the bench with a dumbbell in each hand, resting them on your thighs.
3. Lie back on the bench and position the dumbbells at shoulder level with your palms facing forward.
4. Press the dumbbells up and together, extending your arms fully without locking your elbows.
5. Slowly lower the dumbbells back to the starting position at shoulder level.
6. Keep your feet flat on the floor and your back pressed against the bench throughout the movement.
7. Repeat the movement for the desired number of repetitions.',
                2
        ),
        (
                'dumbbell chest press - flat',
                'The flat dumbbell press is a fundamental exercise that targets the chest muscles, specifically the pectoralis major. It also engages the triceps and shoulders, helping to build upper body strength and muscle mass.',
                '1. Lie flat on a bench with a dumbbell in each hand, resting them on your thighs.
2. Use your thighs to help lift the dumbbells as you lie back, positioning them at shoulder level with your palms facing forward.
3. Press the dumbbells up and together, fully extending your arms without locking your elbows.
4. Slowly lower the dumbbells back to the starting position at shoulder level.
5. Keep your feet flat on the floor and your back pressed against the bench throughout the movement.
6. Repeat the movement for the desired number of repetitions.',
                2
        ),
        (
                'hanging shrugs',
                'Hanging shrugs are an exercise that targets the upper trapezius muscles. This exercise helps to build strength and size in the traps, improving shoulder stability and posture.',
                '1. Hang from a pull-up bar with an overhand grip, hands shoulder-width apart.
2. Keep your arms fully extended and your body relaxed.
3. Without bending your elbows, lift your shoulders up towards your ears by contracting your trapezius muscles.
4. Hold the contraction for a moment at the top of the movement.
5. Slowly lower your shoulders back to the starting position with control.
6. Repeat the movement for the desired number of repetitions.',
                2
        ),
        (
                'wide pull-ups',
                'Wide pull-ups are a compound exercise that primarily targets the latissimus dorsi muscles, along with the upper back, shoulders, and arms. This exercise helps to build upper body strength and improve back width.',
                '1. Grasp a pull-up bar with a wide overhand grip, hands wider than shoulder-width apart.
2. Hang from the bar with your arms fully extended and your legs slightly bent.
3. Engage your core and pull your body up towards the bar, leading with your chest and keeping your elbows out to the sides.
4. Pull until your chin is above the bar, squeezing your shoulder blades together at the top of the movement.
5. Slowly lower your body back to the starting position with control, fully extending your arms.
6. Repeat the movement for the desired number of repetitions.',
                3
        ),
        (
                't-bar row',
                'The T-Bar Row is a compound exercise that primarily targets the middle and upper back muscles, including the latissimus dorsi, rhomboids, and trapezius. It also engages the biceps and forearms. This exercise helps to build back thickness and overall upper body strength.',
                '1. Load the desired weight onto the T-bar row machine or landmine attachment.
2. Stand over the bar with a shoulder-width stance and bend your knees slightly.
3. Grasp the handles or a V-bar attachment with a neutral grip.
4. Keeping your back straight, hinge at the hips and lower your torso until it''s almost parallel to the floor.
5. Pull the bar towards your chest, squeezing your shoulder blades together at the top of the movement.
6. Slowly lower the bar back to the starting position with control.
7. Maintain a tight core and avoid rounding your back throughout the exercise.
8. Repeat the movement for the desired number of repetitions.',
                3
        ),
        (
                'seated lat row',
                'The seated lat row, also known as the seated cable row, is a compound exercise that targets the muscles of the middle back, including the latissimus dorsi, rhomboids, and trapezius. It also engages the biceps and forearms. This exercise helps to build back thickness and improve upper body strength.',
                '1. Sit at a seated row machine and place your feet on the foot platform, knees slightly bent.
2. Grasp the handles or a V-bar attachment with a neutral grip, arms fully extended.
3. Keep your back straight and your chest up as you pull the handles towards your torso, squeezing your shoulder blades together at the top of the movement.
4. Slowly return the handles to the starting position with control, fully extending your arms.
5. Maintain a tight core and avoid leaning too far forward or backward throughout the exercise.
6. Repeat the movement for the desired number of repetitions.',
                2
        ),
        (
                'wide leg press',
                'The wide leg press is a compound exercise that targets the lower body muscles, particularly the quadriceps, hamstrings, and glutes. By using a wider stance, it also engages the inner thigh muscles (adductors). This exercise helps to build lower body strength and muscle mass.',
                '1. Sit on the leg press machine and place your feet on the platform in a wide stance, slightly wider than shoulder-width apart.
2. Keep your feet flat and your toes slightly pointed out.
3. Release the safety handles and extend your legs, but do not lock your knees.
4. Lower the platform by bending your knees and hips, bringing your thighs to a 90-degree angle.
5. Push the platform back up to the starting position by extending your knees and hips.
6. Maintain a controlled movement throughout the exercise and avoid locking your knees at the top.
7. Repeat the movement for the desired number of repetitions.',
                2
        ),
        (
                'dumbbell split squat - glutes',
                'The dumbbell split squat targets the glutes, quadriceps, and hamstrings. By emphasizing the glutes, this exercise helps to build lower body strength and improve balance, particularly focusing on the posterior chain.',
                'Hold a dumbbell in each hand, arms fully extended at your sides. Step one foot forward into a split stance, ensuring your front foot is far enough ahead that when you lower into the squat, your knee stays behind your toes. Lower your body until your back knee nearly touches the ground and your front thigh is parallel to the floor, focusing on engaging the glutes. Push through the heel of your front foot to return to the starting position. Repeat for the desired number of repetitions before switching legs.',
                3
        ),
        (
                'ez bar curl - wide grip',
                'The EZ bar curl with a wide grip targets the biceps, with an emphasis on the long head. The wide grip allows for a greater stretch and activation of the outer part of the biceps, helping to build strength and muscle size.',
                'Stand with your feet shoulder-width apart, holding an EZ bar with a wide grip (hands placed on the outer curves of the bar). Keep your elbows close to your torso as you curl the bar upwards towards your shoulders. Squeeze your biceps at the top of the movement, then slowly lower the bar back to the starting position. Repeat for the desired number of repetitions.',
                2
        ),
        (
                'lat row - incline neutral grip',
                'The incline neutral grip lat row targets the lats, rhomboids, and traps, with an emphasis on the middle back. The neutral grip allows for a strong contraction and helps to build thickness in the upper and middle back.',
                'Set an incline bench to about a 45-degree angle. Lie face down on the bench with a dumbbell in each hand, using a neutral grip (palms facing each other). Row the dumbbells up towards your hips, squeezing your shoulder blades together at the top of the movement. Lower the dumbbells back to the starting position with control. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'single arm dumbbell pullover',
                'The single arm dumbbell pullover targets the lats, chest, and serratus anterior muscles. This unilateral exercise helps to improve upper body strength, flexibility, and muscle balance.',
                'Lie on a flat bench with your head supported and feet flat on the floor. Hold a dumbbell with one hand, extending your arm above your chest. Lower the dumbbell in an arc behind your head until you feel a stretch in your lats and chest. Raise the dumbbell back to the starting position with control. Repeat for the desired number of repetitions, then switch arms.',
                3
        ),
        (
                'barrel press - slight incline',
                'The barrel press on a slight incline targets the pectoralis major, especially the upper portion, while also engaging the anterior deltoids and triceps. The unique grip and range of motion focus on enhancing upper chest development.',
                'Set an incline bench to a slight angle, around 15-30 degrees. Lie back on the bench with a neutral grip on the dumbbells, as if hugging a barrel. Keep your elbows slightly bent and press the dumbbells upwards in a controlled motion, maintaining the barrel-like grip. Lower the dumbbells back to the starting position with control. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'walking lunges - dumbbell',
                'Walking lunges with dumbbells are a compound exercise that targets the quadriceps, hamstrings, and glutes while also engaging the core for stability. This exercise is great for building lower body strength and improving balance.',
                'Hold a dumbbell in each hand, arms fully extended at your sides. Take a step forward with one leg and lower your body until both knees are bent at a 90-degree angle, with your back knee just above the ground. Push through the heel of your front foot to bring your back leg forward into the next lunge. Continue to alternate legs as you walk forward, maintaining good posture throughout. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'barbell static lunges',
                'Barbell static lunges are a compound exercise that targets the quadriceps, hamstrings, and glutes. This exercise helps to build lower body strength and improve balance.',
                'Place a barbell across your upper traps, similar to the position for a back squat. Step one foot forward into a lunge position. Lower your back knee towards the ground while keeping your front knee over your ankle. Push through the heel of your front foot to return to the starting position without stepping forward or backward. Repeat for the desired number of repetitions, then switch legs.',
                3
        ),
        (
                'dumbbell curls - incline single arm',
                'Incline single-arm dumbbell curls target the biceps, specifically emphasizing the long head. The incline position increases the stretch on the biceps, helping to build strength and muscle size.',
                'Set an incline bench to a 45-degree angle. Sit back on the bench with a dumbbell in one hand, allowing your arm to hang straight down. Keep your elbow close to your body as you curl the dumbbell up towards your shoulder. Squeeze your bicep at the top, then slowly lower the dumbbell back to the starting position. Repeat for the desired number of repetitions before switching arms.',
                2
        ),
        (
                'calf raises - seated garage style',
                'in a seated squat position, on a plate, you keep goining untill you cannot handle the burn anymore',
                'Sit in a squatted position on a plate, then add weight on your quads and go',
                2
        ),
        (
                'shrugs - seated dumbbell',
                'Seated dumbbell shrugs primarily target the upper trapezius muscles, helping to build strength and size in the upper traps. Performing this exercise seated helps to reduce momentum and focus more on the trap muscles.',
                'Sit on a bench with a dumbbell in each hand, arms fully extended at your sides. Keep your back straight and your core engaged. Shrug your shoulders upwards towards your ears, squeezing your traps at the top of the movement. Lower the dumbbells back to the starting position with control. Repeat for the desired number of repetitions.',
                2
        ),
        (
                'standing hyperextensions - barbell',
                'Standing hyperextensions with a barbell target the lower back, glutes, and hamstrings. This exercise helps to build strength in the posterior chain while also improving lower back stability.',
                'Stand with your feet shoulder-width apart, holding a barbell across your upper traps, similar to the position for a back squat. Keep your legs slightly bent and your back straight. Hinge at the hips to lower your torso forward until it is nearly parallel to the floor, feeling a stretch in your hamstrings and lower back. Engage your glutes and lower back muscles to raise your torso back to the starting position. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'bent over dumbbell row - high elbow',
                'The bent over dumbbell row with a high elbow targets the upper back, emphasizing the rhomboids, rear deltoids, and upper traps. The high elbow position shifts the focus towards the upper portion of the back.',
                'Stand with your feet shoulder-width apart, holding a dumbbell in each hand. Bend forward at the waist with a slight bend in your knees, keeping your back flat. With your elbows flared out to the side, row the dumbbells up towards your chest, squeezing your shoulder blades together at the top. Lower the dumbbells back to the starting position with control. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'skull crushers - incline',
                'Incline skull crushers target the triceps, particularly the long head, with the incline adding a unique angle to the exercise that increases the stretch and range of motion.',
                'Set an incline bench to a 30-45 degree angle. Lie back on the bench with an EZ curl bar or dumbbells held above your chest, arms fully extended. Keeping your upper arms stationary, bend your elbows to lower the weight towards your forehead, or just above your head. Extend your elbows to return to the starting position, engaging your triceps throughout the movement. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'barbell press - slight incline',
                'The slight incline barbell press primarily targets the upper portion of the pectoralis major, while also engaging the anterior deltoids and triceps. This exercise is effective for building strength and size in the upper chest with a focus on a slightly different angle than a standard incline press.',
                'Set an incline bench to a slight angle, around 15-30 degrees. Lie back on the bench and grasp the barbell with a grip slightly wider than shoulder-width apart. Unrack the barbell and position it directly above your upper chest. Lower the barbell slowly to your chest, then press it back up to the starting position by fully extending your arms. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'dumbbell chest press - incline',
                'The incline dumbbell press targets the upper portion of the pectoralis major, along with the anterior deltoids and triceps. This exercise helps to build strength and size in the upper chest.',
                'Set an incline bench to a 30-45 degree angle. Sit on the bench with a dumbbell in each hand, resting them on your thighs. Kick the dumbbells up as you lie back on the bench, positioning them at shoulder height with your palms facing forward. Press the dumbbells upwards until your arms are fully extended, bringing them together at the top. Lower the dumbbells back to the starting position with control. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'machine ab crunch',
                'The machine ab crunch is an isolation exercise that targets the abdominal muscles, specifically the rectus abdominis. This exercise helps to build core strength and improve abdominal definition using a machine to provide resistance.',
                '1. Sit on the ab crunch machine and adjust the seat height so that the pads rest comfortably against your chest and thighs.
2. Grasp the handles or place your hands on the pads provided.
3. Engage your core and begin the movement by curling your torso forward, bringing your chest towards your knees.
4. Focus on contracting your abdominal muscles as you crunch forward.
5. Hold the contraction for a moment at the bottom of the movement.
6. Slowly return to the starting position with control, extending your torso back to the upright position.
7. Repeat the movement for the desired number of repetitions.',
                2
        ),
        (
                'sissy squat - dumbbell',
                'The sissy squat is an advanced lower body exercise that primarily targets the quadriceps while also engaging the hip flexors. Using a dumbbell adds resistance, making this a challenging movement for building quad strength and size.',
                'Hold a dumbbell close to your chest with both hands. Stand with your feet shoulder-width apart. While keeping your upper body straight and your hips locked, push your knees forward and lean back as you lower your body down. Descend until your thighs are almost parallel to the ground or your knees are fully flexed. Push through your toes to return to the starting position. Repeat for the desired number of repetitions.',
                4
        ),
        (
                'sissy squat',
                'The sissy squat is an advanced lower body exercise that primarily targets the quadriceps, focusing on the rectus femoris, vastus lateralis, and vastus medialis. It is highly effective for building quad strength and definition.',
                'Stand with your feet shoulder-width apart, keeping your upper body straight. Hold onto a stable surface or extend your arms forward for balance. While keeping your hips locked and your upper body straight, push your knees forward and lean back as you lower your body down. Descend until your thighs are almost parallel to the ground or your knees are fully flexed. Push through your toes to return to the starting position. Repeat for the desired number of repetitions.',
                4
        ),
        (
                'sumo deadlifts',
                'Sumo deadlifts are a compound exercise that targets the glutes, hamstrings, quadriceps, and lower back. The wide stance emphasizes the inner thighs and reduces the range of motion, making it an effective lift for building lower body strength and power.',
                'Stand with your feet wider than shoulder-width apart and your toes slightly pointed outwards. Position the barbell over the middle of your feet. Bend at the hips and knees to grasp the bar with an overhand or mixed grip, keeping your hands inside your knees. Engage your core and drive through your heels to lift the bar by extending your hips and knees. Keep the bar close to your body as you stand up straight. Lower the bar back to the starting position with control. Repeat for the desired number of repetitions.',
                4
        ),
        (
                'front squat - barbell',
                'The barbell front squat is a compound exercise that primarily targets the quadriceps, while also engaging the glutes, hamstrings, and core. The front-loaded position places more emphasis on the quads and requires a strong, upright torso to maintain balance.',
                'Stand with your feet shoulder-width apart and position the barbell across the front of your shoulders. Cross your arms or use a clean grip to hold the barbell in place. Keep your chest up and elbows pointed forward as you lower your body into a squat, ensuring that your knees track over your toes. Descend until your thighs are parallel to the floor or lower, then push through your heels to return to the starting position. Maintain an upright torso throughout the movement. Repeat for the desired number of repetitions.',
                4
        ),
        (
                'dragon flag',
                'The dragon flag is an advanced core exercise that primarily targets the rectus abdominis, while also engaging the obliques, hip flexors, and lower back. It is a challenging movement that builds core strength, stability, and control.',
                'Lie on a flat bench or the floor and grasp a sturdy object behind your head, such as the bench or a post, for support. Begin with your legs extended and your body straight. Engage your core and lift your legs and hips off the bench, keeping your body in a straight line. Slowly lower your body back down, keeping it straight and avoiding any arching in your lower back. Lower until your body is just above the bench or floor, then lift back up. Repeat for the desired number of repetitions.',
                5
        ),
        (
                'pro shuttle',
                'The Pro Shuttle, also known as the 5-10-5 drill, is a dynamic agility exercise that focuses on speed, quickness, and change of direction. It is commonly used in athletic training to improve lateral movement and acceleration.',
                'Start in a three-point stance, with your hand on the ground and feet shoulder-width apart. At the start signal, sprint 5 yards to your right and touch the line with your hand. Then, sprint 10 yards to your left, touching the far line with your hand. Finally, sprint 5 yards back to the center line. Focus on quick, explosive movements and maintaining control throughout the drill.',
                3
        ),
        (
                'leg extensions',
                'Leg extensions are an isolation exercise that primarily targets the quadriceps muscles. This exercise is effective for building quad strength and muscle definition.',
                'Sit on a leg extension machine with your back pressed against the seat and your legs under the padded lever. Adjust the machine so that your knees are aligned with the machine''s pivot point. Grasp the handles on the sides of the seat for stability. Extend your legs fully by straightening your knees, lifting the padded lever upward. Hold the top position for a brief moment, then slowly lower the weight back to the starting position. Repeat for the desired number of repetitions.',
                2
        ),
        (
                'cable tricep extension with internal rotation',
                'The cable tricep extension with internal rotation is an isolation exercise that targets the triceps, with a unique twist that adds emphasis on the long head of the triceps. The internal rotation helps to increase the range of motion and muscle activation.',
                'Set the cable machine to a high pulley position and attach a rope or single handle. Stand facing away from the machine and grasp the handle with one hand, keeping your elbow bent and close to your head. Extend your arm forward, fully straightening your elbow while internally rotating your hand. Slowly return to the starting position with control. Repeat for the desired number of repetitions before switching to the other arm.',
                3
        ),
        (
                'seated lat row - close grip',
                'The seated lat row with a close grip is a compound exercise that targets the middle back, particularly the latissimus dorsi, rhomboids, and lower traps. The close grip emphasizes the inner back muscles and helps to build back thickness and strength.',
                'Sit on a seated row machine with your feet placed firmly on the foot platform and knees slightly bent. Grasp the close grip handle with both hands, keeping your back straight and chest up. Pull the handle towards your lower chest, squeezing your shoulder blades together at the end of the movement. Slowly return the handle to the starting position by extending your arms. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'dumbbell hammer curl',
                'Dumbbell hammer curls are an isolation exercise that targets the brachialis, brachioradialis, and biceps brachii. The neutral grip used in hammer curls emphasizes the forearms and helps build arm strength and size.',
                'Stand with your feet shoulder-width apart, holding a dumbbell in each hand with a neutral grip (palms facing each other). Keep your elbows close to your torso as you curl the dumbbells upwards towards your shoulders. Squeeze your biceps at the top of the movement, then slowly lower the dumbbells back to the starting position. Repeat for the desired number of repetitions.',
                2
        ),
        (
                'bench press - close grip',
                'The close grip bench press is a compound exercise that primarily targets the triceps, while also engaging the chest and shoulders. The close grip places more emphasis on the triceps, making it a great exercise for building arm strength and muscle mass.',
                'Lie flat on a bench with your feet planted firmly on the ground. Grasp the barbell with a close grip, hands positioned about shoulder-width apart. Unrack the barbell and lower it slowly to your lower chest, keeping your elbows close to your body. Press the barbell back up to the starting position, fully extending your arms while engaging your triceps. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'dumbbell curls - supinated',
                'Dumbbell curls with a supinated grip primarily target the biceps brachii, emphasizing both the short and long heads. This exercise helps to build arm strength and size by keeping the palms facing up throughout the movement.',
                'Stand with your feet shoulder-width apart, holding a dumbbell in each hand with a supinated grip (palms facing up). Keep your elbows close to your torso as you curl the dumbbells upwards towards your shoulders. Squeeze your biceps at the top of the movement, then slowly lower the dumbbells back to the starting position. Maintain the supinated grip throughout the exercise. Repeat for the desired number of repetitions.',
                2
        ),
        (
                'bench press - reverse grip',
                'The reverse grip bench press is a variation of the traditional bench press that targets the upper chest and triceps more intensely. The reverse grip shifts the emphasis towards the upper portion of the pectoralis major while reducing stress on the shoulders.',
                'Lie flat on a bench with your feet planted firmly on the ground. Grasp the barbell with an underhand (reverse) grip, hands positioned about shoulder-width apart. Unrack the barbell and lower it slowly to your lower chest, keeping your elbows close to your body. Press the barbell back up to the starting position, fully extending your arms while engaging your chest and triceps. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'barbell curls - reverse grip',
                'Reverse grip straight bar curls primarily target the brachialis and brachioradialis, while also engaging the biceps. This exercise helps to build strength in the forearms and the outer part of the upper arms.',
                'Stand with your feet shoulder-width apart, holding a straight bar with an overhand (reverse) grip, hands placed slightly wider than shoulder-width apart. Keep your elbows close to your torso as you curl the bar upwards towards your shoulders. Focus on squeezing your forearms and brachialis at the top of the movement, then slowly lower the bar back to the starting position. Repeat for the desired number of repetitions.',
                2
        ),
        (
                'dumbbell skull crushers - incline',
                'Incline dumbbell skull crushers target the triceps, specifically focusing on the long head. The incline position increases the stretch on the triceps, making this variation highly effective for building arm strength and size.',
                'Set an incline bench to a 30-45 degree angle. Sit back on the bench with a dumbbell in each hand, holding them above your chest with your arms fully extended. Keeping your upper arms stationary, lower the dumbbells towards your head by bending your elbows. Once your elbows reach about a 90-degree angle, press the dumbbells back up to the starting position. Ensure you engage your triceps throughout the movement. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'hanging knee raises - weighted',
                'Hanging knee raises are an effective core exercise that primarily targets the rectus abdominis, while also engaging the hip flexors and obliques. Adding weight increases the difficulty, making it a powerful move for building core strength and muscle definition.',
                'Hang from a pull-up bar with a weight between your feet, keeping your arms fully extended and your legs straight. Engage your core and bring your knees up towards your chest, maintaining control throughout the movement. Hold for a moment at the top, then slowly lower your legs back to the starting position. Repeat for the desired number of repetitions.',
                4
        ),
        (
                'pullovers - barbell',
                'Barbell pullovers are a compound exercise that targets the lats, chest, and serratus anterior muscles. This movement helps to improve upper body strength, flexibility, and muscle definition.',
                'Lie on a flat bench with your head supported and your feet flat on the floor. Hold a barbell with an overhand grip and extend your arms above your chest. Keeping your arms slightly bent, lower the barbell in an arc behind your head until you feel a stretch in your lats and chest. Engage your lats and chest to raise the barbell back to the starting position with control. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'one arm row - pronated',
                'The one-arm row with a pronated grip targets the upper and middle back, focusing on the lats, rhomboids, and traps. The pronated grip shifts some emphasis to the rear deltoids and helps to build strength and muscle definition in the back.',
                'Stand beside a bench with one knee and one hand on the bench for support, holding a dumbbell in the opposite hand with a pronated grip (palm facing down). Keep your back flat and your core engaged. Row the dumbbell up towards your hip, leading with your elbow and keeping your arm close to your body. Squeeze your shoulder blade at the top, then slowly lower the dumbbell back to the starting position. Repeat for the desired number of repetitions, then switch sides.',
                3
        ),
        (
                'dumbbell side lateral - scapular plane',
                'The dumbbell side lateral raise in the scapular plane targets the middle deltoid, with an emphasis on shoulder stability and function. Performing the exercise in the scapular plane (about 30 degrees forward) reduces strain on the shoulder joint and improves muscle activation.',
                'Stand with your feet shoulder-width apart, holding a dumbbell in each hand. Position your arms so they are slightly in front of your body, in the scapular plane (about 30 degrees forward from your sides). Keeping a slight bend in your elbows, raise the dumbbells out to the sides until they reach shoulder height. Lower the dumbbells back to the starting position with control. Repeat for the desired number of repetitions.',
                2
        ),
        (
                'dumbbell rear delt raise - incline',
                'The incline dumbbell rear delt raise targets the posterior deltoids, along with the rhomboids and traps. This exercise is effective for building strength and definition in the rear shoulders and upper back.',
                'Set an incline bench to about a 30-45 degree angle. Lie face down on the bench with a dumbbell in each hand, arms hanging straight down. Keeping a slight bend in your elbows, raise the dumbbells out to the sides until they reach shoulder height. Focus on squeezing your shoulder blades together at the top of the movement. Lower the dumbbells back to the starting position with control. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'barbell rear delt raise',
                'The barbell rear delt raise targets the posterior deltoids, rhomboids, and upper traps. This exercise helps to build strength and definition in the rear shoulders and upper back, promoting balanced shoulder development.',
                'Stand with your feet shoulder-width apart, holding a barbell with a pronated grip (palms facing down). Bend forward at the waist, keeping your back straight and a slight bend in your knees. With a slight bend in your elbows, raise the barbell out to your sides until it reaches shoulder height. Focus on squeezing your shoulder blades together at the top of the movement. Lower the barbell back to the starting position with control. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'dumbbell side lateral - fixed elbow',
                'The dumbbell side lateral raise with a fixed elbow primarily targets the middle deltoid, helping to build shoulder width and strength. Keeping a fixed elbow throughout the movement focuses on isolating the deltoid muscle.',
                'Stand with your feet shoulder-width apart, holding a dumbbell in each hand with your arms at your sides. With a slight bend in your elbows, lift the dumbbells out to the sides until they reach shoulder height, keeping your elbows fixed in place. Lower the dumbbells back to the starting position with control. Repeat for the desired number of repetitions.',
                2
        ),
        (
                'dumbbell shoulder press - seated hammer',
                'The seated hammer dumbbell press primarily targets the deltoids and triceps, with an emphasis on the middle and anterior heads of the deltoid. The hammer grip reduces strain on the shoulder joints and provides a strong, natural pressing motion.',
                'Sit on a bench with your back supported and feet flat on the ground. Hold a dumbbell in each hand with a neutral (hammer) grip, palms facing each other, and position the dumbbells at shoulder height. Press the dumbbells upward until your arms are fully extended overhead, then lower them back to the starting position with control. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'dumbbell curls - seated supinated',
                'Seated supinated dumbbell curls target the biceps, emphasizing both the short and long heads. The seated position helps to isolate the biceps, reducing momentum and improving muscle engagement.',
                'Sit on a bench with your back supported and feet flat on the ground. Hold a dumbbell in each hand with a supinated grip (palms facing up). Keep your elbows close to your torso as you curl the dumbbells upwards towards your shoulders. Squeeze your biceps at the top of the movement, then slowly lower the dumbbells back to the starting position. Repeat for the desired number of repetitions.',
                2
        ),
        (
                'skull crushers - 45 degree',
                'skull crushers target the triceps, specifically focusing on the long head. The incline position increases the stretch on the triceps, making this variation highly effective for building arm strength and size.',
                'lie on a flat bench. Sit back on the bench with a (ez/straight) bar, lower it untill it hang, then pull it in a 45 degree angle, stopping at the bottom. then repeat.',
                3
        ),
        (
                'pullovers - single arm dumbbell',
                'The single arm dumbbell pullover targets the lats, chest, and serratus anterior muscles. This unilateral exercise helps improve upper body strength, flexibility, and muscle balance.',
                'Lie on a flat bench with your head supported and feet flat on the floor. Hold a dumbbell in one hand, extending your arm above your chest. Keeping your arm slightly bent, lower the dumbbell in an arc behind your head until you feel a stretch in your lats and chest. Engage your lats and chest to raise the dumbbell back to the starting position with control. Repeat for the desired number of repetitions, then switch arms.',
                3
        ),
        (
                'shrugs - barbell wide grip',
                'Barbell shrugs with a wide grip primarily target the upper trapezius muscles, helping to build strength and size in the upper traps. The wide grip variation increases the range of motion and can provide a different stimulus to the muscle.',
                'Stand with your feet shoulder-width apart, holding a barbell with a wide overhand grip, hands placed wider than shoulder-width apart. Keep your arms fully extended at your sides. Shrug your shoulders upwards towards your ears, squeezing your traps at the top of the movement. Lower the barbell back to the starting position with control. Repeat for the desired number of repetitions.',
                2
        ),
        (
                'v-ups - dumbbell both hands & feet',
                'V-ups with a dumbbell held between both hands and feet target the entire core, with a focus on the rectus abdominis, hip flexors, and obliques. This advanced variation increases the difficulty by adding resistance, improving core strength and stability.',
                'Lie flat on your back with your legs fully extended and a dumbbell held between both hands above your head. Hold a second dumbbell between your feet. Simultaneously lift your upper body and legs towards each other, forming a ''V'' shape, and touch the dumbbells together at the top. Engage your core throughout the movement. Slowly lower your body back to the starting position with control. Repeat for the desired number of repetitions.',
                4
        ),
        (
                'plate calf raises - toes turned out',
                'Plate calf raises with toes turned out primarily target the gastrocnemius muscle, with an emphasis on the inner part of the calf. This variation helps to build calf strength and muscle definition.',
                'Stand with the balls of your feet on a weight plate or a slightly elevated surface, with your heels hanging off the edge. Position your feet so that your toes are turned outwards at an angle. Keeping your legs straight, raise your heels as high as possible, squeezing your calves at the top of the movement. Slowly lower your heels back down to the starting position. Repeat for the desired number of repetitions.',
                2
        ),
        (
                'plate calf raises - toes turned in',
                'Plate calf raises with toes turned in primarily target the gastrocnemius muscle, with an emphasis on the outer part of the calf. This variation helps to build calf strength and muscle definition, particularly on the outer calves.',
                'Stand with the balls of your feet on a weight plate or a slightly elevated surface, with your heels hanging off the edge. Position your feet so that your toes are turned inwards at an angle. Keeping your legs straight, raise your heels as high as possible, squeezing your calves at the top of the movement. Slowly lower your heels back down to the starting position. Repeat for the desired number of repetitions.',
                2
        ),
        (
                'dumbbell side lateral - seated',
                'Seated dumbbell side laterals primarily target the middle deltoid, helping to build shoulder width and strength. Performing the exercise seated helps to reduce momentum, increasing the focus on the deltoid muscles.',
                'Sit on a bench with your feet flat on the ground, holding a dumbbell in each hand at your sides. Keep a slight bend in your elbows as you lift the dumbbells out to the sides until they reach shoulder height. Ensure your movements are controlled, and focus on engaging your deltoid muscles. Lower the dumbbells back to the starting position with control. Repeat for the desired number of repetitions.',
                2
        ),
        (
                'upright row - wide grip',
                'The wide grip upright row primarily targets the upper trapezius and deltoids, with an emphasis on the middle deltoids and upper traps. The wide grip variation reduces stress on the shoulder joints and shifts some focus to the middle deltoids, helping to build shoulder width and upper back strength.',
                'Stand with your feet shoulder-width apart, holding a barbell or dumbbells with a wide overhand grip. Keep your hands wider than shoulder-width apart. Lift the weight vertically by raising your elbows out to the sides until your upper arms are parallel to the floor. Keep the barbell close to your body throughout the movement. Lower the weight back to the starting position with control. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'dumbbell shoulder press - seated hammer single hand',
                'The seated hammer single-hand dumbbell press targets the deltoids, with an emphasis on the anterior and middle heads, while also engaging the triceps. This variation, performed with one arm at a time, increases the demand on core stability and balance.',
                'Sit on a bench with your back supported and feet flat on the ground. Hold a dumbbell in one hand with a neutral (hammer) grip, palm facing inward, and position the dumbbell at shoulder height. Press the dumbbell upward until your arm is fully extended overhead, then lower it back to the starting position with control. Keep your core engaged to maintain balance throughout the movement. Repeat for the desired number of repetitions, then switch to the other arm.',
                3
        ),
        (
                'rear delt raise - around the world',
                'The rear delt raise ''Around the World'' variation targets the posterior deltoids, traps, and upper back muscles. This dynamic movement involves a circular motion that engages the rear delts and upper back more comprehensively, helping to build strength and definition.',
                'Stand with your feet shoulder-width apart, holding a dumbbell in each hand. Bend forward at the waist with a slight bend in your knees and keep your back flat. Start by raising the dumbbells out to your sides in a circular motion, bringing them above your head in a wide arc, and then lower them back down in front of you in a controlled manner. Focus on squeezing your shoulder blades together at the top of the movement. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'ez bar curl - close grip',
                'The EZ bar curl with a close grip targets the biceps, with an emphasis on the short head. The close grip allows for greater bicep activation, helping to build arm strength and size.',
                'Stand with your feet shoulder-width apart, holding an EZ bar with a close grip (hands placed on the inner part of the bar). Keep your elbows close to your torso as you curl the bar upwards towards your shoulders. Squeeze your biceps at the top of the movement, then slowly lower the bar back to the starting position. Repeat for the desired number of repetitions.',
                2
        ),
        (
                'barbell curls',
                'Barbell curls are a classic isolation exercise that primarily targets the biceps brachii. This exercise helps to build arm strength and size by focusing on the biceps, with some involvement from the brachialis and forearms.',
                'Stand with your feet shoulder-width apart, holding a barbell with an underhand grip (palms facing up), hands positioned slightly wider than shoulder-width apart. Keep your elbows close to your torso as you curl the barbell upwards towards your shoulders. Squeeze your biceps at the top of the movement, then slowly lower the barbell back to the starting position. Repeat for the desired number of repetitions.',
                2
        ),
        (
                'arnold rotations - slight incline lying',
                'Arnold rotations on a slight incline while lying down primarily target the rear deltoids, with additional emphasis on the middle deltoids and rotator cuff muscles. This variation of the exercise helps to improve shoulder stability, strength, and mobility, particularly focusing on the posterior deltoid.',
                'Set an incline bench to a slight angle, around 15-30 degrees, and lie face down on the bench with your chest supported. Hold a dumbbell in each hand with your palms facing you at shoulder height. As you rotate your arms upward, twist your wrists so that your palms face outward at the top of the movement, focusing on engaging your rear delts. Reverse the motion as you lower the dumbbells back to the starting position, rotating your palms to face you again. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'arnold rotations - slight incline',
                'Arnold rotations on a slight incline target the shoulders, particularly the anterior and middle deltoids, while also engaging the triceps. The rotating motion helps to improve shoulder stability and strength, with the incline adding a unique challenge.',
                'Set an incline bench to a slight angle, around 15-30 degrees. Sit back on the bench with a dumbbell in each hand, palms facing you at shoulder height. As you press the dumbbells upward, rotate your palms outward so that they face away from you at the top of the movement. Reverse the motion on the way down, bringing the dumbbells back to the starting position with palms facing you. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'bus drivers - incline',
                'Incline bus drivers target the deltoids, particularly the anterior and middle heads, while also engaging the upper chest and forearms. This exercise mimics the motion of turning a steering wheel, providing a unique rotational challenge that enhances shoulder stability and strength.',
                'Set an incline bench to a 30-45 degree angle and lie back with a weight plate held in both hands at arm''s length in front of your chest. Keep your arms slightly bent. Rotate the plate from side to side as if you were turning a steering wheel, focusing on controlled movements and engaging your shoulder muscles throughout. Perform the motion for the desired number of repetitions.',
                3
        ),
        (
                'bus driver',
                'The bus driver exercise targets the deltoids, particularly the anterior and middle heads, while also engaging the forearms. This movement mimics the action of turning a steering wheel, providing a rotational challenge that enhances shoulder stability, strength, and mobility.',
                'Stand with your feet shoulder-width apart and hold a weight plate with both hands at arm''s length in front of you. Keep your arms slightly bent. Rotate the plate from side to side as if you were turning a steering wheel, focusing on controlled movements and engaging your shoulder muscles throughout the exercise. Perform the motion for the desired number of repetitions.',
                2
        ),
        (
                '90/90 stretch',
                'The 90/90 stretch is a hip mobility exercise that targets the hip flexors, glutes, and external rotators. It helps improve hip flexibility and mobility, making it beneficial for athletes and anyone looking to enhance their range of motion in the hips.',
                'Sit on the floor with one leg bent in front of you at a 90-degree angle, with your shin parallel to your torso. Position your back leg behind you, also bent at a 90-degree angle. Keep your chest up and your back straight. Lean forward over your front leg to increase the stretch in your hip. Hold the stretch for 20-30 seconds, then switch sides. Focus on keeping your hips square and maintaining proper alignment throughout the stretch.',
                2
        ),
        (
                'a-skips',
                'A-skips are a dynamic warm-up exercise that targets the hip flexors, quadriceps, and calves while also improving coordination, rhythm, and running form. This drill is commonly used in athletic training to prepare the body for sprinting and other explosive movements.',
                'Stand upright with your feet shoulder-width apart. Begin by skipping forward, driving one knee up towards your chest while simultaneously hopping off the opposite foot. Swing your opposite arm forward in coordination with the lifted knee. Land softly on the ball of your foot and quickly transition to the next skip with the opposite leg. Maintain a rhythmic pattern, focusing on quick, controlled movements and keeping your posture tall. Repeat for the desired distance or number of repetitions.',
                2
        ),
        (
                'alternate bounding',
                'Alternate bounding is a plyometric exercise that targets the lower body, including the glutes, quadriceps, hamstrings, and calves. This exercise is great for improving explosive power, coordination, and running form.',
                'Start by standing upright with your feet shoulder-width apart. Begin by driving one knee up and forward while pushing off the opposite foot to leap forward. Swing your opposite arm in coordination with the bounding leg to maintain balance and rhythm. Land softly on the ball of your foot and immediately push off with the other leg to continue bounding forward. Maintain an upright posture and focus on powerful, controlled movements. Repeat for the desired distance or number of repetitions.',
                3
        ),
        (
                'alternating deadbug holding swiss ball',
                'The alternating deadbug holding a Swiss ball is a core stabilization exercise that targets the rectus abdominis, obliques, and hip flexors. This exercise helps improve core strength, stability, and coordination while engaging the entire core.',
                'Lie on your back with your knees bent at a 90-degree angle and your arms extended straight up, holding a Swiss ball between your knees and hands. Press the ball firmly between your hands and knees to engage your core. Begin by extending one leg out straight while lowering the opposite arm toward the floor, keeping the ball in place with the other arm and leg. Return to the starting position and repeat with the opposite arm and leg. Continue alternating sides for the desired number of repetitions, maintaining tension in your core throughout the movement.',
                3
        ),
        (
                'arnold press',
                'The Arnold Press is a shoulder exercise that targets the deltoids, with an emphasis on both the anterior and middle heads. The unique rotating movement helps to engage the shoulders more fully and adds an element of stability and control to the press.',
                'Sit on a bench with your back supported and feet flat on the ground. Hold a dumbbell in each hand at shoulder height with your palms facing towards you and your elbows bent. As you press the dumbbells overhead, rotate your wrists so that your palms face forward at the top of the movement. Reverse the motion on the way down, bringing the dumbbells back to the starting position with your palms facing you. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'backpack stretch',
                'The backpack stretch is a mobility exercise that primarily targets the upper back and shoulders, helping to relieve tension and improve flexibility. This stretch is particularly useful for those who carry heavy loads or experience tightness in the upper back and shoulders.',
                'Stand with your feet shoulder-width apart, holding a backpack or similar weight behind your back with both hands. Keep your arms extended and gently pull the backpack upward, stretching your shoulders and upper back. Hold the stretch for 20-30 seconds, then slowly lower the backpack. Maintain a straight posture throughout the stretch, and focus on breathing deeply to enhance the stretch. Repeat as needed.',
                1
        ),
        (
                'band lat stretch',
                'The band lat stretch is a mobility exercise that targets the latissimus dorsi, helping to improve flexibility and relieve tightness in the upper body. This stretch is beneficial for increasing range of motion and reducing tension in the back and shoulders.',
                'Secure a resistance band to a sturdy anchor point above head height. Grasp the band with one hand and step back to create tension in the band. Lean your body forward and slightly to the side, allowing the band to gently pull on your arm and stretch the latissimus dorsi. Hold the stretch for 20-30 seconds, feeling the stretch along your side and back. Repeat on the other side.',
                1
        ),
        (
                'band pec minor stretch',
                'The band pec minor stretch is a mobility exercise that targets the pectoralis minor muscle, helping to improve flexibility and relieve tightness in the chest and shoulders. This stretch is beneficial for increasing range of motion and reducing tension in the upper body.',
                'Secure a resistance band to a sturdy anchor point at shoulder height. Grasp the band with one hand and step forward to create tension in the band. Rotate your body away from the anchored band, allowing the band to gently pull on your arm and stretch the pectoralis minor. Hold the stretch for 20-30 seconds, feeling the stretch across your chest and the front of your shoulder. Repeat on the other side.',
                1
        ),
        (
                'barbell curls - close grip',
                'Barbell curls with a close grip primarily target the biceps, emphasizing the short head. This grip variation allows for greater bicep activation, helping to build arm strength and size.',
                'Stand with your feet shoulder-width apart, holding a barbell with a close underhand grip (hands placed closer than shoulder-width apart). Keep your elbows close to your torso as you curl the barbell upwards towards your shoulders. Squeeze your biceps at the top of the movement, then slowly lower the barbell back to the starting position. Repeat for the desired number of repetitions.',
                2
        ),
        (
                'barbell squats - close stance',
                'Barbell squats with a close stance primarily target the quadriceps, with additional engagement of the glutes and hamstrings. This stance variation increases the emphasis on the quads, helping to build lower body strength and size.',
                'Stand with your feet close together, about hip-width apart, holding a barbell across your upper back. Keep your chest up, core engaged, and your back straight. Lower your body into a squat by bending your knees and hips, keeping your knees in line with your toes. Descend until your thighs are parallel to the floor or lower, then push through your heels to return to the starting position. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'barbell stiff leg deadlift',
                'The barbell stiff leg deadlift is a compound exercise that primarily targets the hamstrings and glutes, while also engaging the lower back. This movement helps to build strength and flexibility in the posterior chain.',
                'Stand with your feet shoulder-width apart, holding a barbell with an overhand grip in front of your thighs. Keep your legs straight but not locked, and maintain a slight bend in your knees. Hinge at the hips to lower the barbell towards the floor, keeping your back flat and your core engaged. Lower the barbell until you feel a stretch in your hamstrings, then drive your hips forward to return to the starting position. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'bench press',
                'The bench press is a fundamental compound exercise that primarily targets the chest, while also engaging the shoulders and triceps. It is one of the most effective exercises for building upper body strength and muscle mass.',
                'Lie flat on a bench with your feet firmly planted on the ground. Grasp the barbell with a grip slightly wider than shoulder-width apart. Unrack the barbell and lower it slowly to your mid-chest, keeping your elbows at a 45-degree angle from your body. Press the barbell back up to the starting position, fully extending your arms while keeping your chest engaged. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'bench press - decline',
                'The decline bench press is a compound exercise that targets the lower portion of the chest, while also engaging the triceps and shoulders. The decline angle shifts the emphasis to the lower chest, making it a key exercise for building chest strength and definition.',
                'Set the bench to a decline angle and secure your feet in the footpads. Lie back on the bench and grasp the barbell with a grip slightly wider than shoulder-width apart. Unrack the barbell and lower it slowly to your lower chest, keeping your elbows at a 45-degree angle from your body. Press the barbell back up to the starting position, fully extending your arms while keeping your chest engaged. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'bench press - eccentric',
                'The eccentric bench press emphasizes the lowering phase of the movement, focusing on controlled, slow descent to build strength and muscle control. This variation is effective for increasing muscle hypertrophy and improving stability during the bench press.',
                'Lie flat on a bench with your feet firmly planted on the ground. Grasp the barbell with a grip slightly wider than shoulder-width apart. Unrack the barbell and slowly lower it to your mid-chest, taking 3-5 seconds to complete the descent. Keep your elbows at a 45-degree angle from your body. Once the barbell reaches your chest, press it back up to the starting position with normal speed. Repeat for the desired number of repetitions, focusing on the slow, controlled lowering phase.',
                4
        ),
        (
                'bench press - incline',
                'The incline bench press is a compound exercise that targets the upper portion of the chest, while also engaging the shoulders and triceps. The incline angle shifts the emphasis to the upper chest, making it a key exercise for balanced chest development.',
                'Set the bench to an incline angle of about 30-45 degrees. Lie back on the bench and grasp the barbell with a grip slightly wider than shoulder-width apart. Unrack the barbell and lower it slowly to your upper chest, keeping your elbows at a 45-degree angle from your body. Press the barbell back up to the starting position, fully extending your arms while keeping your chest engaged. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'bench press - isometric',
                'The isometric bench press involves holding the barbell in a static position, which helps to build strength and endurance in the chest, shoulders, and triceps. This variation increases time under tension, making it effective for developing muscular endurance and stability.',
                'Lie flat on a bench with your feet firmly planted on the ground. Grasp the barbell with a grip slightly wider than shoulder-width apart. Unrack the barbell and lower it to your chest. Instead of pressing the bar back up immediately, hold it in a static position just above your chest for a set amount of time, typically 3-10 seconds. After the hold, press the barbell back up to the starting position. Repeat for the desired number of repetitions or isometric holds.',
                3
        ),
        (
                'bent over barbell row - eccentric',
                'The eccentric bent over barbell row emphasizes the lowering phase of the movement, which helps to build strength, muscle control, and hypertrophy in the back muscles. This variation focuses on a slow and controlled descent, increasing time under tension.',
                'Stand with your feet shoulder-width apart, holding a barbell with a pronated grip (palms facing down). Bend at the hips and knees, keeping your back flat and torso nearly parallel to the floor. Pull the barbell towards your lower chest, squeezing your shoulder blades together at the top. Lower the barbell slowly, taking 3-5 seconds to return to the starting position. Focus on maintaining control and engaging your back muscles throughout the movement. Repeat for the desired number of repetitions.',
                4
        ),
        (
                'bent over dumbbell row',
                'The bent over dumbbell row is a compound exercise that targets the muscles of the upper and middle back, including the lats, rhomboids, and traps. This exercise also engages the biceps and core, making it effective for building back strength and improving posture.',
                'Stand with your feet shoulder-width apart, holding a dumbbell in each hand. Bend at the hips and knees, keeping your back flat and torso nearly parallel to the floor. With your arms extended downwards, pull the dumbbells towards your lower chest, squeezing your shoulder blades together at the top of the movement. Lower the dumbbells back to the starting position with control, maintaining a strong, flat back throughout the exercise. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'bicycle crunch with bands around ankles',
                'The bicycle crunch with bands around the ankles is an advanced core exercise that targets the rectus abdominis, obliques, and hip flexors. The resistance band adds extra tension, increasing the challenge and effectiveness of the exercise.',
                'Lie flat on your back with your hands behind your head and a resistance band looped around your ankles. Lift your legs off the ground and bend your knees to 90 degrees. Perform the bicycle motion by bringing one knee towards your chest while simultaneously extending the other leg out straight. As you do this, twist your torso to bring the opposite elbow towards the bent knee. Alternate sides in a controlled, rhythmic motion, maintaining tension in the band throughout the exercise. Repeat for the desired number of repetitions.',
                4
        ),
        (
                'board press',
                'The board press is a bench press variation that targets the triceps, chest, and shoulders. It involves placing a board on the chest to reduce the range of motion, making it particularly effective for building strength in the top half of the bench press.',
                'Lie flat on a bench with your feet firmly planted on the ground. Have a partner place a board on your chest, or hold the board yourself. Grasp the barbell with a grip slightly wider than shoulder-width apart. Unrack the barbell and lower it until it touches the board, keeping your elbows at a 45-degree angle from your body. Press the barbell back up to the starting position, fully extending your arms while keeping your chest engaged. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'box jump',
                'The box jump is a plyometric exercise that primarily targets the muscles of the lower body, including the quads, glutes, hamstrings, and calves. It is an explosive movement that helps to build power, agility, and coordination.',
                'Stand in front of a sturdy box or platform with your feet shoulder-width apart. Lower your body into a quarter squat to prepare for the jump. Swing your arms back, then explode upwards, extending your hips, knees, and ankles to jump onto the box. Land softly on the box with both feet, absorbing the impact by bending your knees slightly. Step down from the box carefully and repeat for the desired number of repetitions.',
                3
        ),
        (
                'box squat',
                'The box squat is a variation of the traditional squat that targets the lower body muscles, including the quadriceps, glutes, and hamstrings. By sitting back onto a box, this exercise helps improve squat form, depth control, and strength, while minimizing stress on the knees.',
                'Stand with your feet shoulder-width apart, with a box or bench positioned behind you. Hold a barbell across your upper back, keeping your chest up and core engaged. Begin the movement by pushing your hips back and bending your knees to lower your body towards the box. Once you touch the box with your glutes, pause briefly, then drive through your heels to stand back up, returning to the starting position. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'bounding',
                'Bounding is a plyometric exercise that targets the muscles of the lower body, including the quadriceps, hamstrings, glutes, and calves. It is an explosive movement that helps improve power, coordination, and running form, making it a key drill for athletes.',
                'Begin by standing upright with your feet shoulder-width apart. Push off with one leg to leap forward as far as possible, driving your opposite knee up towards your chest. Land softly on the opposite foot and immediately push off to leap forward again with the other leg. Maintain a powerful and controlled rhythm, focusing on driving your knees high and landing with control. Repeat for the desired distance or number of repetitions.',
                3
        ),
        (
                'bulgarian split squat',
                'The Bulgarian split squat is a unilateral leg exercise that targets the quadriceps, glutes, and hamstrings. By elevating the rear foot, this exercise increases the range of motion, making it effective for building strength, balance, and stability in the lower body.',
                'Stand a few feet in front of a bench or elevated surface. Place the top of your rear foot on the bench, ensuring your front foot is far enough forward to allow a deep squat. Lower your body by bending your front knee, keeping your torso upright. Lower until your front thigh is parallel to the ground or just below, then push through your front heel to return to the starting position. Repeat for the desired number of repetitions, then switch legs.',
                3
        ),
        (
                'cable crunch - standing',
                'Standing cable crunches target the rectus abdominis and obliques, helping to build core strength and definition. This exercise allows for a greater range of motion and resistance, making it effective for engaging the abdominal muscles.',
                'Attach a rope handle to a high pulley on a cable machine. Stand facing the machine with your feet shoulder-width apart and grasp the rope with both hands, placing your hands near your forehead. Keeping your elbows close to your head, engage your core and crunch forward, bringing your chest toward your knees. Focus on contracting your abs throughout the movement. Slowly return to the starting position with control. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'cable curl - alternating',
                'Cable curls with an alternating motion target the biceps, focusing on building strength and muscle definition. The alternating movement allows you to concentrate on one arm at a time, ensuring balanced development.',
                'Attach a single handle to each side of a cable machine. Stand between the pulleys with your feet shoulder-width apart, holding a handle in each hand. Curl one handle towards your shoulder while keeping the other arm extended. Lower the handle back down with control, then repeat with the opposite arm. Continue alternating for the desired number of repetitions.',
                2
        ),
        (
                'cable face pulls',
                'Cable face pulls target the rear deltoids, upper traps, and rhomboids. This exercise is excellent for improving shoulder stability, posture, and overall upper back strength.',
                'Attach a rope handle to a high pulley on a cable machine. Stand facing the machine with your feet shoulder-width apart and grasp the rope with both hands, palms facing each other. Pull the rope towards your face, flaring your elbows out to the sides and squeezing your shoulder blades together. Slowly return to the starting position. Repeat for the desired number of repetitions.',
                2
        ),
        (
                'cable fly - high angle',
                'The high-angle cable fly targets the upper portion of the chest, helping to build upper chest strength and definition. This variation emphasizes the clavicular head of the pectoralis major.',
                'Attach handles to the high pulleys of a cable machine. Stand between the pulleys with your feet shoulder-width apart and hold a handle in each hand. Step forward slightly to create tension in the cables. With a slight bend in your elbows, bring your arms together in front of you, squeezing your chest at the top of the movement. Slowly return to the starting position with control. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'cable fly - low angle',
                'The low-angle cable fly targets the lower portion of the chest, focusing on building strength and definition in the pectoralis major. This variation helps create a well-rounded chest.',
                'Attach handles to the low pulleys of a cable machine. Stand between the pulleys with your feet shoulder-width apart and hold a handle in each hand. Step forward slightly to create tension in the cables. With a slight bend in your elbows, bring your arms up and together in front of you, squeezing your chest at the top of the movement. Slowly return to the starting position with control. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'cable upright row',
                'Cable upright rows primarily target the shoulders and traps, helping to build strength and size in these areas. This exercise is effective for creating upper body width and improving posture.',
                'Attach a straight bar to a low pulley on a cable machine. Stand with your feet shoulder-width apart, holding the bar with an overhand grip. Pull the bar straight up towards your chin, keeping your elbows flared out to the sides. Lower the bar back to the starting position with control. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'cable woodchops',
                'Cable woodchops are a functional exercise that targets the core, especially the obliques, while also engaging the shoulders and hips. This movement is great for developing rotational strength and power.',
                'Attach a handle to a high pulley on a cable machine. Stand sideways to the machine with your feet shoulder-width apart. Grasp the handle with both hands and extend your arms fully. Pull the handle down and across your body in a chopping motion, rotating your torso as you do so. Control the movement as you return to the starting position. Repeat for the desired number of repetitions, then switch sides.',
                3
        ),
        (
                'c. y-cuffs',
                'The C. Y-Cuffs exercise targets the rotator cuff muscles, focusing on shoulder stability and mobility. This exercise is beneficial for preventing injuries and improving shoulder strength.',
                'Attach resistance bands or cables to a low pulley on a cable machine. Stand with your feet shoulder-width apart, holding the handles with your arms at your sides. Raise your arms out to form a ''Y'' shape, keeping your thumbs pointing upwards and your elbows slightly bent. Control the movement as you lower your arms back to the starting position. Repeat for the desired number of repetitions.',
                2
        ),
        (
                'chin ups',
                'Chin-ups are a compound upper body exercise that primarily targets the biceps and upper back, particularly the latissimus dorsi and biceps. This exercise is great for building upper body strength and muscle.',
                'Grasp a pull-up bar with an underhand grip (palms facing you) and your hands shoulder-width apart. Hang with your arms fully extended. Pull yourself up until your chin is above the bar, focusing on squeezing your back and biceps at the top of the movement. Lower yourself back to the starting position with control. Repeat for the desired number of repetitions.',
                4
        ),
        (
                'circle sprints',
                'Circle sprints are a high-intensity cardio exercise that improves agility, speed, and cardiovascular fitness. This exercise is excellent for enhancing athletic performance, particularly in sports that require quick direction changes.',
                'Set up a circle of cones or markers on the ground. Start by sprinting around the circle, maintaining a fast pace. Focus on quick footwork and tight turns. After completing a set number of laps in one direction, reverse the direction and repeat. Ensure you maintain proper running form and stay light on your feet throughout the exercise.',
                3
        ),
        (
                'couch stretch',
                'The couch stretch is a mobility exercise that targets the hip flexors and quadriceps, helping to improve flexibility and relieve tightness in the lower body. This stretch is particularly beneficial for those who sit for extended periods.',
                'Kneel on the floor in front of a couch or wall. Place one foot up on the couch or against the wall, with your shin vertical and your knee on the ground. Place the other foot flat on the floor in front of you to form a lunge position. Slowly shift your hips forward while keeping your back straight, feeling the stretch in your hip flexors and quads. Hold the stretch for 20-30 seconds, then switch sides.',
                2
        ),
        (
                'couch stretch series',
                'The couch stretch series is a comprehensive mobility routine that targets the hip flexors, quadriceps, and surrounding muscles. This series of stretches helps improve flexibility, reduce tightness, and enhance overall mobility in the lower body.',
                'Perform the standard couch stretch by kneeling in front of a couch or wall, placing one foot up on the couch or against the wall. After holding the stretch for 20-30 seconds, adjust your position to target different angles, such as leaning forward to stretch the hip flexors more deeply or leaning back to emphasize the quads. Repeat the sequence on the opposite side, ensuring you stretch each muscle group thoroughly.',
                2
        ),
        (
                'crunches on bosu ball',
                'Crunches on a Bosu ball enhance core activation by adding instability, targeting the rectus abdominis and obliques. This exercise is effective for building core strength and improving balance.',
                'Sit on a Bosu ball with your lower back positioned on the center of the ball. Place your feet flat on the ground and your hands behind your head. Engage your core and curl your torso upwards, bringing your chest towards your knees. Hold the contraction for a moment, then slowly lower yourself back to the starting position. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'deadlift',
                'The deadlift is a fundamental compound exercise that targets multiple muscle groups, including the glutes, hamstrings, quadriceps, lower back, and core. It is a key exercise for building overall strength, power, and muscle mass.',
                'Stand with your feet hip-width apart, with the barbell over the middle of your feet. Bend at the hips and knees to grasp the bar with an overhand or mixed grip, keeping your back straight and your chest up. Engage your core and drive through your heels to lift the bar by extending your hips and knees. Keep the bar close to your body as you stand up straight. Lower the bar back to the starting position with control, maintaining a flat back throughout the movement. Repeat for the desired number of repetitions.',
                4
        ),
        (
                'deadlift - dumbbell',
                'The dumbbell deadlift is a variation of the traditional deadlift that targets the same muscle groups, including the glutes, hamstrings, quadriceps, and lower back, while also engaging stabilizing muscles. This variation allows for greater range of motion and control.',
                'Stand with your feet hip-width apart, holding a dumbbell in each hand at your sides. Bend at the hips and knees, lowering the dumbbells towards the ground while keeping your back straight and chest up. Engage your core and drive through your heels to lift the dumbbells by extending your hips and knees. Stand up straight, then lower the dumbbells back to the starting position with control. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'deadlift - eccentric',
                'The eccentric deadlift focuses on the lowering phase of the movement, which helps to build strength, muscle control, and hypertrophy in the posterior chain muscles. This variation emphasizes a slow and controlled descent.',
                'Stand with your feet hip-width apart, with the barbell over the middle of your feet. Bend at the hips and knees to grasp the bar with an overhand or mixed grip, keeping your back straight and chest up. Engage your core and drive through your heels to lift the bar by extending your hips and knees. As you lower the bar back to the starting position, take 3-5 seconds to control the descent. Keep the bar close to your body, maintaining a flat back throughout the movement. Repeat for the desired number of repetitions.',
                4
        ),
        (
                'deadlift - isometric',
                'The isometric deadlift involves holding the barbell in a static position, which helps to build strength and endurance in the lower back, glutes, and hamstrings. This variation increases time under tension, making it effective for developing muscular endurance and stability.',
                'Stand with your feet hip-width apart, with the barbell over the middle of your feet. Bend at the hips and knees to grasp the bar with an overhand or mixed grip, keeping your back straight and chest up. Engage your core and drive through your heels to lift the bar just off the ground, holding the position for a set amount of time (typically 3-10 seconds). After the hold, lower the bar back to the starting position with control. Repeat for the desired number of repetitions or isometric holds.',
                4
        ),
        (
                'deadlift off small box',
                'The deadlift off a small box is a variation that increases the range of motion, placing greater emphasis on the lower back, glutes, and hamstrings. This exercise is particularly useful for developing strength from the bottom position of the deadlift.',
                'Stand on a small box or platform with your feet hip-width apart, with the barbell over the middle of your feet. Bend at the hips and knees to grasp the bar with an overhand or mixed grip, keeping your back straight and chest up. Engage your core and drive through your heels to lift the bar by extending your hips and knees. Keep the bar close to your body as you stand up straight. Lower the bar back to the starting position with control, maintaining a flat back throughout the movement. Repeat for the desired number of repetitions.',
                4
        ),
        (
                'decline cable fly',
                'The decline cable fly targets the lower portion of the chest, focusing on building strength and definition in the pectoralis major. This exercise helps to create a well-rounded chest by emphasizing the lower pecs.',
                'Attach handles to the high pulleys of a cable machine. Set a bench to a decline angle and lie back on it, holding a handle in each hand. With a slight bend in your elbows, bring your arms down and together in front of you, squeezing your chest at the bottom of the movement. Slowly return to the starting position with control. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'dumbbell chest press - hammer grip',
                'The dumbbell chest press with a hammer grip targets the chest, shoulders, and triceps while emphasizing stability and control. The neutral grip places less strain on the shoulders, making it a safer option for those with shoulder issues.',
                'Lie on a flat bench with a dumbbell in each hand, palms facing each other (neutral grip). Press the dumbbells up until your arms are fully extended. Lower the dumbbells slowly back to the starting position, keeping your elbows close to your body. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'dumbbell curl - alternating',
                'The alternating dumbbell curl is a bicep exercise that allows for isolated focus on each arm, promoting balanced development. This exercise targets both heads of the biceps and helps build arm strength and muscle definition.',
                'Stand with your feet shoulder-width apart, holding a dumbbell in each hand with your palms facing forward. Curl one dumbbell towards your shoulder while keeping the other arm extended. Lower the dumbbell back to the starting position with control, then repeat with the opposite arm. Continue alternating for the desired number of repetitions.',
                2
        ),
        (
                'dumbbell curls - incline',
                'Incline dumbbell curls target the biceps, especially the long head, which is emphasized due to the angle of the incline. This exercise helps build the peak of the biceps, creating a fuller appearance.',
                'Set an incline bench to a 45-degree angle and sit back with a dumbbell in each hand, arms fully extended down. With your palms facing forward, curl the dumbbells towards your shoulders, keeping your elbows close to your body. Lower the dumbbells back to the starting position with control. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'dumbbell curls - preacher',
                'Preacher curls with dumbbells isolate the biceps, minimizing the use of other muscles and ensuring a strict form. This exercise is effective for building both the short and long heads of the biceps, enhancing muscle definition and strength.',
                'Sit on a preacher bench with your upper arms resting on the pad and a dumbbell in each hand. With your palms facing up, curl the dumbbells towards your shoulders, keeping your upper arms stationary. Squeeze your biceps at the top of the movement, then slowly lower the dumbbells back to the starting position. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'dumbbell fly - decline',
                'The decline dumbbell fly targets the lower portion of the chest, helping to build strength and definition in the pectoralis major. This exercise is effective for enhancing the lower chest area, contributing to a well-rounded chest.',
                'Set a bench to a decline angle and lie back with a dumbbell in each hand, arms extended above your chest with a slight bend in your elbows. Slowly lower the dumbbells out to your sides in an arc, feeling the stretch in your chest. Bring the dumbbells back together at the top, squeezing your chest muscles. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'dumbbell fly - incline',
                'The incline dumbbell fly focuses on the upper portion of the chest, helping to build strength and size in the clavicular head of the pectoralis major. This exercise also engages the shoulders, making it an effective upper chest developer.',
                'Set a bench to an incline angle and lie back with a dumbbell in each hand, arms extended above your chest with a slight bend in your elbows. Slowly lower the dumbbells out to your sides in an arc, feeling the stretch in your upper chest. Bring the dumbbells back together at the top, squeezing your chest muscles. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'dumbbell front raise - seated',
                'The seated dumbbell front raise is an isolation exercise that targets the anterior deltoids. This exercise is excellent for building shoulder strength and enhancing the front part of the shoulders.',
                'Sit on a bench with a dumbbell in each hand, arms extended down by your sides with your palms facing your thighs. Keeping your back straight and core engaged, raise one or both dumbbells in front of you to shoulder height. Lower the dumbbells back down with control. Repeat for the desired number of repetitions.',
                2
        ),
        (
                'dumbbell hammer curl - alternating',
                'The alternating dumbbell hammer curl targets the biceps, brachialis, and brachioradialis. The neutral grip used in this exercise places emphasis on the forearms and helps build overall arm strength and size.',
                'Stand with your feet shoulder-width apart, holding a dumbbell in each hand with your palms facing your thighs (neutral grip). Curl one dumbbell towards your shoulder while keeping the other arm extended. Lower the dumbbell back to the starting position with control, then repeat with the opposite arm. Continue alternating for the desired number of repetitions.',
                2
        ),
        (
                'dumbbell overhead press - natural grip',
                'The dumbbell overhead press with a natural grip (palms facing each other) targets the shoulders, with an emphasis on the deltoids. The neutral grip places less strain on the shoulder joints, making it a safer option for overhead pressing.',
                'Stand or sit with your feet shoulder-width apart, holding a dumbbell in each hand at shoulder height with your palms facing each other. Press the dumbbells overhead until your arms are fully extended. Lower the dumbbells back to shoulder height with control. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'dumbbell press - close grip',
                'The close grip dumbbell press targets the triceps and chest, with an emphasis on the inner portion of the chest. This exercise is effective for building upper body strength and increasing muscle definition.',
                'Lie flat on a bench with a dumbbell in each hand, palms facing each other (neutral grip) and dumbbells close together. Press the dumbbells up until your arms are fully extended. Lower the dumbbells back to the starting position with control, keeping the dumbbells close together throughout the movement. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'dumbbell press - standing single arm',
                'The standing single arm dumbbell press is a unilateral exercise that targets the shoulders, helping to build strength and stability. This exercise also engages the core, improving overall balance and coordination.',
                'Stand with your feet shoulder-width apart, holding a dumbbell in one hand at shoulder height with your palm facing inward (neutral grip). Press the dumbbell overhead until your arm is fully extended. Lower the dumbbell back to shoulder height with control. Repeat for the desired number of repetitions, then switch arms.',
                3
        ),
        (
                'dumbbell row - alternating',
                'The alternating dumbbell row targets the upper and middle back muscles, including the lats, rhomboids, and traps. The alternating motion allows for focused engagement of each side of the back, promoting balanced development.',
                'Stand with your feet shoulder-width apart, holding a dumbbell in each hand. Bend at the hips and knees, keeping your back flat and torso nearly parallel to the floor. Pull one dumbbell towards your lower chest, squeezing your shoulder blade at the top. Lower the dumbbell back to the starting position with control, then repeat with the opposite arm. Continue alternating for the desired number of repetitions.',
                3
        ),
        (
                'dumbbell row - incline',
                'The incline dumbbell row is a variation that targets the upper and middle back muscles, including the lats, rhomboids, and traps, with the added benefit of chest support, which reduces strain on the lower back.',
                'Set an incline bench to a 45-degree angle and lie face down with a dumbbell in each hand. With your arms hanging down, pull the dumbbells towards your lower chest, squeezing your shoulder blades together at the top. Lower the dumbbells back to the starting position with control. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'dumbbell shoulder press - hammer grip',
                'The dumbbell shoulder press with a hammer grip (neutral grip) targets the deltoids and triceps, placing less strain on the shoulder joints compared to other grip variations. This exercise helps to build overall shoulder strength and stability.',
                'Sit on a bench with a back support, holding a dumbbell in each hand at shoulder height with your palms facing each other (neutral grip). Press the dumbbells overhead until your arms are fully extended. Lower the dumbbells back to shoulder height with control. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'dumbbell shoulder press - seated alternating',
                'The seated alternating dumbbell shoulder press targets the deltoids and triceps while allowing you to focus on one arm at a time, helping to build balanced strength and muscle development in the shoulders.',
                'Sit on a bench with back support, holding a dumbbell in each hand at shoulder height with your palms facing forward. Press one dumbbell overhead until your arm is fully extended, while keeping the other dumbbell at shoulder height. Lower the dumbbell back to shoulder height, then repeat with the opposite arm. Continue alternating for the desired number of repetitions.',
                3
        ),
        (
                'dumbbell side bends',
                'Dumbbell side bends are an isolation exercise that targets the obliques, helping to build core strength and stability. This exercise also engages the quadratus lumborum, which supports the lower back.',
                'Stand with your feet shoulder-width apart, holding a dumbbell in one hand by your side. Place your opposite hand on your hip for support. Slowly bend to the side, lowering the dumbbell towards the floor while keeping your torso straight. Return to the starting position by contracting your obliques. Repeat for the desired number of repetitions, then switch sides.',
                2
        ),
        (
                'dumbbell side lateral - negative',
                'The negative dumbbell side lateral raise focuses on the eccentric phase of the movement, targeting the deltoids with increased time under tension. This exercise is effective for building shoulder strength and muscle hypertrophy.',
                'Stand with your feet shoulder-width apart, holding a dumbbell in each hand by your sides. Raise the dumbbells out to the sides to shoulder height, keeping a slight bend in your elbows. Slowly lower the dumbbells back to the starting position, taking 3-5 seconds to complete the descent. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'dumbbell side lateral - standing',
                'The standing dumbbell side lateral raise targets the deltoid muscles, particularly the middle deltoids, helping to build shoulder width and definition. This exercise is key for developing the rounded appearance of the shoulders.',
                'Stand with your feet shoulder-width apart, holding a dumbbell in each hand by your sides. Raise the dumbbells out to the sides to shoulder height, keeping a slight bend in your elbows. Lower the dumbbells back to the starting position with control. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'skull crushers - dumbbell',
                'Dumbbell skull crushers target the triceps, specifically the long head, helping to build arm strength and size. This exercise also engages the muscles of the upper back to stabilize the shoulders.',
                'Lie flat on a bench with a dumbbell in each hand, arms extended straight above your chest. Keeping your upper arms stationary, bend your elbows to lower the dumbbells towards your forehead in a controlled manner. Once your elbows are at a 90-degree angle, press the dumbbells back up to the starting position by extending your elbows. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'dumbbell upright row',
                'The dumbbell upright row primarily targets the shoulders and traps, helping to build upper body strength and definition. This exercise is also effective for improving posture and shoulder stability.',
                'Stand with your feet shoulder-width apart, holding a dumbbell in each hand with your palms facing your thighs. Pull the dumbbells straight up towards your chin, keeping your elbows flared out to the sides. Lower the dumbbells back to the starting position with control. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'dump the buckets',
                'Dump the Buckets is a shoulder exercise that emphasizes the deltoid muscles, particularly the lateral and posterior heads. This movement helps to improve shoulder stability and strength.',
                'Stand with your feet shoulder-width apart, holding a dumbbell in each hand. Start with the dumbbells at your sides, palms facing in. As you raise the dumbbells out to your sides to shoulder height, rotate your wrists as if pouring out water from a bucket, with your thumbs pointing downward. Lower the dumbbells back to the starting position with control. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'reverse lunge - elevated',
                'The elevated reverse lunge targets the quadriceps, glutes, and hamstrings, helping to build lower body strength and stability. Elevating the front foot increases the range of motion, making this exercise more challenging.',
                'Stand on an elevated surface, such as a step or platform, with your feet shoulder-width apart. Step back with one leg into a reverse lunge, lowering your back knee towards the ground while keeping your front knee aligned with your ankle. Push through your front heel to return to the starting position. Repeat for the desired number of repetitions, then switch legs.',
                3
        ),
        (
                'ez bar cable curl',
                'The EZ bar cable curl targets the biceps, providing a continuous tension throughout the movement. The EZ bar''s angled grip places less strain on the wrists, making it a joint-friendly option for bicep training.',
                'Attach an EZ bar to a low pulley on a cable machine. Stand with your feet shoulder-width apart and grasp the bar with an underhand grip, keeping your elbows close to your body. Curl the bar towards your shoulders, focusing on contracting your biceps. Slowly lower the bar back to the starting position with control. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'ez bar cable tricep extension',
                'The EZ bar cable tricep extension targets all three heads of the triceps, helping to build arm strength and size. The use of a cable machine provides constant tension throughout the movement, enhancing muscle activation.',
                'Attach an EZ bar to a high pulley on a cable machine. Stand with your feet shoulder-width apart, grasping the bar with an overhand grip. Keep your elbows close to your head and extend your arms fully to press the bar downwards. Slowly return to the starting position with control, ensuring your upper arms remain stationary. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'ez curls - reverse grip',
                'Reverse EZ curls target the brachioradialis and biceps, helping to build arm and forearm strength. The reverse grip places more emphasis on the forearms, making it a valuable exercise for overall arm development.',
                'Stand with your feet shoulder-width apart, holding an EZ bar with a reverse (overhand) grip. Keep your elbows close to your body and curl the bar towards your shoulders, focusing on contracting your forearms and biceps. Lower the bar back to the starting position with control. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'face pull',
                'Face pulls target the rear deltoids, upper traps, and rhomboids, making it an excellent exercise for improving shoulder health, posture, and upper back strength. This exercise helps to counteract the effects of poor posture and build stability in the shoulder joint.',
                'Attach a rope handle to a high pulley on a cable machine. Stand facing the machine with your feet shoulder-width apart and grasp the rope with both hands, palms facing each other. Pull the rope towards your face, flaring your elbows out to the sides and squeezing your shoulder blades together. Slowly return to the starting position. Repeat for the desired number of repetitions.',
                2
        ),
        (
                'farmer carry - single arm',
                'The single-arm farmer carry is a unilateral exercise that targets the core, particularly the obliques, while also engaging the shoulders, traps, and grip strength. This exercise is excellent for improving stability and balance.',
                'Stand with your feet shoulder-width apart, holding a heavy dumbbell or kettlebell in one hand by your side. Keep your torso upright and shoulders square as you walk forward for a set distance or time. Focus on engaging your core to prevent leaning to one side. Switch hands and repeat on the opposite side.',
                3
        ),
        (
                'farmer walk',
                'The farmer walk is a full-body exercise that primarily targets the grip, forearms, shoulders, traps, and core. It is an excellent exercise for building strength, endurance, and stability while promoting good posture.',
                'Stand with your feet shoulder-width apart, holding a heavy dumbbell or kettlebell in each hand by your sides. Keep your torso upright and shoulders square as you walk forward for a set distance or time. Maintain a strong grip on the weights and focus on engaging your core throughout the movement.',
                3
        ),
        (
                'figure 8 drill',
                'The Figure 8 Drill is a dynamic agility exercise that improves coordination, speed, and footwork. It is commonly used in sports training to enhance an athlete''s ability to change direction quickly while maintaining balance and control.',
                'Set up two cones or markers about 5 to 10 feet apart. Start by running in a figure-8 pattern around the cones, focusing on quick, sharp turns and maintaining a low center of gravity. Stay light on your feet and use your arms for balance. Repeat for the desired number of repetitions or time.',
                3
        ),
        (
                'fire hydrants',
                'Fire hydrants are a bodyweight exercise that targets the glutes, specifically the gluteus medius, helping to improve hip stability and strength. This exercise also engages the core and helps to build a strong lower body.',
                'Start on all fours with your hands directly under your shoulders and your knees under your hips. Keeping your knee bent, lift one leg out to the side, stopping at hip height. Hold for a moment, then lower your leg back to the starting position. Repeat for the desired number of repetitions, then switch sides.',
                2
        ),
        (
                'flying 20''s',
                'Flying 20''s is a sprinting drill designed to improve speed and acceleration. This exercise is often used in athletic training to enhance running mechanics and explosive power.',
                'Start by jogging for 10 meters to build momentum. As you reach the 10-meter mark, accelerate into a full sprint for the next 20 meters. Focus on maintaining proper running form, with quick turnover and powerful strides. After the sprint, decelerate gradually and walk back to the starting point to recover. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'flutter kicks holding dumbbell high',
                'Flutter kicks with a dumbbell held high is a core exercise that targets the lower abs, while also engaging the shoulders and upper body for stability. This exercise helps to build core strength and improve overall stability.',
                'Lie on your back with your legs extended and a dumbbell held with both hands above your chest. Lift your head, shoulders, and legs off the ground. Keep your legs straight and perform a flutter kicking motion, alternating your feet up and down. Maintain tension in your core and keep the dumbbell steady throughout the exercise. Repeat for the desired number of repetitions or time.',
                3
        ),
        (
                'frog stretch',
                'The frog stretch is a deep hip and groin stretch that helps to improve flexibility and mobility in the hips. This stretch is particularly beneficial for those who experience tightness in the inner thighs and hip flexors.',
                'Start on all fours, with your knees wide apart and your feet together behind you. Slowly push your hips back towards your heels, keeping your chest low to the ground. You should feel a deep stretch in your inner thighs and hips. Hold the stretch for 20-30 seconds, breathing deeply and relaxing into the position.',
                2
        ),
        (
                'full sit ups with feet under weight',
                'Full sit-ups with feet under weight target the rectus abdominis and hip flexors, helping to build core strength and stability. Anchoring the feet allows for a greater range of motion, making this exercise more challenging.',
                'Lie on your back with your knees bent and feet anchored under a weight or heavy object. Place your hands behind your head or across your chest. Engage your core and sit up, bringing your chest towards your knees. Lower yourself back down with control. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'glute bridge',
                'The glute bridge is a fundamental exercise that targets the glutes, biceps femoris, semitendinosus, and semimembranosus. It helps to strengthen the posterior chain, improve hip stability, and enhance lower body strength.',
                'Lie on your back with your knees bent and feet flat on the ground, hip-width apart. Keep your arms at your sides with palms facing down. Engage your core and squeeze your glutes as you lift your hips off the ground until your body forms a straight line from your shoulders to your knees. Hold the position for a moment, then lower your hips back down with control. Repeat for the desired number of repetitions.',
                2
        ),
        (
                'glute bridge - barbell',
                'The barbell glute bridge is an advanced variation of the standard glute bridge that adds resistance to the movement, targeting the glutes, biceps femoris, semitendinosus, and semimembranosus more intensely. This exercise is effective for building strength and muscle mass in the lower body.',
                'Lie on your back with your knees bent and feet flat on the ground, hip-width apart. Place a barbell across your hips, holding it with both hands to keep it stable. Engage your core and squeeze your glutes as you lift your hips off the ground, pressing the barbell upwards until your body forms a straight line from your shoulders to your knees. Hold the position for a moment, then lower your hips back down with control. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'glute bridge with medball on lap - 3 sec holds',
                'This variation of the glute bridge involves holding a medball on your lap and performing the bridge with a 3-second hold at the top. This exercise targets the glutes, biceps femoris, semitendinosus, and semimembranosus, enhancing muscle endurance and strength.',
                'Lie on your back with your knees bent and feet flat on the ground, hip-width apart. Place a medball on your lap and hold it in place with your hands. Engage your core and squeeze your glutes as you lift your hips off the ground, holding the top position for 3 seconds. Lower your hips back down with control and repeat for the desired number of repetitions.',
                3
        ),
        (
                'glute/ham raise',
                'The glute/ham raise is an advanced exercise that targets the biceps femoris, semitendinosus, semimembranosus, glutes, and lower back. This movement is highly effective for building strength and muscle mass in the posterior chain while improving hamstring flexibility.',
                'Position yourself on a glute/ham raise machine with your feet secured under the footplate and your thighs resting on the pad. Start with your body in a straight line, then lower yourself forward by bending at the knees, keeping your back straight. Once you reach the bottom position, contract your hamstrings and glutes to pull your body back up to the starting position. Repeat for the desired number of repetitions.',
                4
        ),
        (
                'good mornings',
                'Good mornings are a compound exercise that targets the biceps femoris, semitendinosus, semimembranosus, glutes, and lower back. This exercise is effective for building strength in the posterior chain and improving hip hinge mechanics.',
                'Stand with your feet shoulder-width apart, holding a barbell across your upper back. With a slight bend in your knees, hinge at the hips to lower your torso towards the ground while keeping your back straight. Lower until your torso is nearly parallel to the floor, then engage your hamstrings and glutes to return to the starting position. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'groiners',
                'Groiners are a dynamic stretch that targets the hip flexors, biceps femoris, semitendinosus, semimembranosus, and groin. This exercise is excellent for warming up the lower body and improving mobility in the hips and lower back.',
                'Start in a high plank position with your hands under your shoulders and your body in a straight line. Step your right foot outside of your right hand, keeping your hips low. Pause for a moment, then return to the plank position and repeat on the left side. Continue alternating for the desired number of repetitions.',
                2
        ),
        (
                'ground starts',
                'Ground starts are a sprinting drill that focuses on explosive power and acceleration from a prone position. This exercise is commonly used in athletic training to improve starting speed and quickness.',
                'Start in a prone position on the ground with your hands under your shoulders and your body flat. When ready, push off the ground explosively, transitioning into a sprint as quickly as possible. Focus on powerful strides and maintaining good running form as you accelerate. Repeat for the desired number of repetitions or time.',
                3
        ),
        (
                'hanging knee raises',
                'Hanging knee raises target the lower abdominal muscles while also engaging the hip flexors and stabilizing the upper body. This exercise is great for building core strength and improving overall abdominal definition.',
                'Hang from a pull-up bar with your arms fully extended and your legs straight. Engage your core and slowly lift your knees towards your chest, keeping your back straight and your movement controlled. Lower your knees back to the starting position without swinging. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'hanging l sit',
                'The hanging L sit is an advanced core exercise that targets the entire abdominal region while also engaging the hip flexors and upper body stabilizers. This static hold builds core strength, stability, and endurance.',
                'Hang from a pull-up bar with your arms fully extended and legs straight. Lift your legs in front of you until they form a 90-degree angle with your torso, creating an ''L'' shape. Hold this position for as long as possible, keeping your core tight and your legs parallel to the ground. Lower your legs back down with control. Repeat for the desired number of repetitions or hold duration.',
                4
        ),
        (
                'hanging leg raises',
                'Hanging leg raises target the lower abdominal muscles while also engaging the hip flexors and stabilizing the upper body. This exercise is effective for building core strength and improving overall abdominal definition.',
                'Hang from a pull-up bar with your arms fully extended and your legs straight. Engage your core and slowly lift your legs towards your chest, keeping your back straight and your movement controlled. Lower your legs back to the starting position without swinging. Repeat for the desired number of repetitions.',
                4
        ),
        (
                'hanging oblique crunch',
                'Hanging oblique crunches target the obliques and lower abs, helping to build a strong and defined core. This exercise is effective for improving core stability and enhancing overall abdominal strength.',
                'Hang from a pull-up bar with your arms fully extended and your legs straight. Engage your core and twist your torso to one side as you lift your knees towards your chest. Lower your legs back to the starting position with control, then repeat on the opposite side. Continue alternating sides for the desired number of repetitions.',
                4
        ),
        (
                'heavy sled drag',
                'The heavy sled drag is a functional exercise that targets the lower body muscles, including the glutes, quadriceps, and hamstrings, while also engaging the core and upper body. This exercise is excellent for building strength, power, and endurance.',
                'Attach a sled to a harness around your waist or hold the sled straps in your hands. Lean forward slightly and begin walking or running, dragging the sled behind you. Maintain a steady pace and keep your core engaged throughout the movement. Continue for a set distance or time.',
                4
        ),
        (
                'heavy sled drag or kittlebell swing',
                'This combo exercise alternates between heavy sled drags and kettlebell swings, targeting the entire body, particularly the lower body, core, and shoulders. This combination is effective for building strength, power, and endurance.',
                'Start with a heavy sled drag for a set distance or time, focusing on engaging your lower body and core. Immediately transition into kettlebell swings, standing with your feet shoulder-width apart, holding a kettlebell with both hands. Swing the kettlebell between your legs and thrust your hips forward to bring it to chest height. Repeat the sequence for the desired number of rounds.',
                4
        ),
        (
                'hex press - incline',
                'The incline hex press targets the chest, particularly the upper portion, while also engaging the shoulders and triceps. This exercise is effective for building strength and muscle mass in the upper body.',
                'Lie on an incline bench with a dumbbell in each hand, palms facing each other (neutral grip). Press the dumbbells together and press them upwards until your arms are fully extended. Lower the dumbbells back to your chest with control, keeping them pressed together throughout the movement. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'hip lifts',
                'Hip lifts, also known as glute bridges, target the glutes, biceps femoris, semitendinosus, and semimembranosus. This exercise helps to strengthen the posterior chain, improve hip stability, and enhance lower body strength.',
                'Lie on your back with your knees bent and feet flat on the ground, hip-width apart. Keep your arms at your sides with palms facing down. Engage your core and squeeze your glutes as you lift your hips off the ground until your body forms a straight line from your shoulders to your knees. Hold the position for a moment, then lower your hips back down with control. Repeat for the desired number of repetitions.',
                2
        ),
        (
                'hurdle jump',
                'Hurdle jumps are a plyometric exercise that targets the lower body muscles, particularly the glutes, quadriceps, hamstrings, and calves. This exercise is effective for building explosive power, agility, and coordination.',
                'Set up hurdles or other barriers at an appropriate height. Stand with your feet shoulder-width apart, facing the hurdles. Bend your knees and explode off the ground, jumping over the hurdle. Land softly on the balls of your feet, absorbing the impact with your legs. Continue jumping over the hurdles for the desired number of repetitions or time.',
                4
        ),
        (
                'hyperextension situp',
                'The hyperextension situp is a core exercise that targets the rectus abdominis and hip flexors while also engaging the lower back. This exercise is effective for building core strength and improving overall stability.',
                'Lie back on a hyperextension bench with your feet secured and your body extended. Cross your arms over your chest or place your hands behind your head. Engage your core and sit up, bringing your chest towards your knees. Lower yourself back down with control, keeping tension in your core throughout the movement. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'hyperextensions',
                'Hyperextensions target the lower back muscles, including the erector spinae, while also engaging the glutes and hamstrings. This exercise is effective for building strength and stability in the posterior chain.',
                'Position yourself on a hyperextension bench with your feet secured and your body extended. Cross your arms over your chest or place your hands behind your head. Engage your core and lower your torso towards the ground by bending at the hips. Lift your torso back up to the starting position by contracting your lower back, glutes, and hamstrings. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'incline cable fly',
                'The incline cable fly targets the upper portion of the pectoralis major while also engaging the shoulders. This exercise helps to build strength and definition in the upper chest.',
                'Set the pulleys on a cable machine to the low position and attach handles. Lie back on an incline bench set at a 45-degree angle, holding a handle in each hand. With a slight bend in your elbows, bring your arms together in front of your chest, squeezing the upper chest muscles at the top. Slowly return to the starting position with control. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'incline curl',
                'Incline curls target the biceps, particularly the long head, which is emphasized due to the angle of the incline. This exercise helps build the peak of the biceps, creating a fuller appearance.',
                'Set an incline bench to a 45-degree angle and sit back with a dumbbell in each hand, arms fully extended down. With your palms facing forward, curl the dumbbells towards your shoulders, keeping your elbows close to your body. Lower the dumbbells back to the starting position with control. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'inverted row',
                'The inverted row is a bodyweight exercise that targets the upper back, specifically the latissimus dorsi, rhomboids, and traps. It also engages the biceps and core for stabilization. This exercise is effective for building upper body strength and improving posture.',
                'Set a bar at about waist height. Lie underneath the bar and grasp it with an overhand grip, hands shoulder-width apart. Keep your body in a straight line from head to heels, and pull your chest towards the bar by squeezing your shoulder blades together. Lower yourself back to the starting position with control. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'iso fire hydrant',
                'The iso fire hydrant is an isometric variation of the fire hydrant exercise, targeting the glutes, particularly the gluteus medius. It also engages the core and hips for stabilization, improving hip stability and lower body strength.',
                'Start on all fours with your hands under your shoulders and your knees under your hips. Lift one leg out to the side, keeping your knee bent at a 90-degree angle. Hold the position at the top for the desired duration, keeping your core engaged and your hips stable. Lower your leg back down with control and repeat on the opposite side.',
                2
        ),
        (
                'iso kick backs',
                'Iso kick backs are an isometric variation of the traditional kick back exercise, targeting the glutes, particularly the gluteus maximus, and hamstrings. This exercise helps improve hip stability and lower body strength.',
                'Start on all fours with your hands under your shoulders and your knees under your hips. Extend one leg straight back, keeping your foot flexed and your hips square to the ground. Hold the position at the top, squeezing your glutes. Lower your leg back down with control and repeat on the opposite side.',
                2
        ),
        (
                'kettlebell swings',
                'Kettlebell swings are a dynamic full-body exercise that targets the glutes, hamstrings, core, and shoulders. This exercise is excellent for building strength, power, and cardiovascular endurance.',
                'Stand with your feet shoulder-width apart, holding a kettlebell with both hands in front of you. Hinge at the hips to swing the kettlebell back between your legs, then thrust your hips forward to swing the kettlebell up to shoulder height. Keep your arms straight and core engaged throughout the movement. Lower the kettlebell back down and repeat for the desired number of repetitions.',
                3
        ),
        (
                'landmine press',
                'The landmine press is a shoulder exercise that also targets the chest, triceps, and core. This exercise is effective for building upper body strength and improving shoulder stability.',
                'Position one end of a barbell in a landmine attachment or securely in a corner. Stand with your feet shoulder-width apart, holding the free end of the barbell with one hand at shoulder height. Press the barbell forward and up until your arm is fully extended. Lower the barbell back to the starting position with control. Repeat for the desired number of repetitions, then switch arms.',
                3
        ),
        (
                'landmine press - alternating',
                'The alternating landmine press is a variation that involves switching arms during the press, targeting the shoulders, chest, triceps, and core. This exercise helps improve unilateral strength and shoulder stability.',
                'Position one end of a barbell in a landmine attachment or securely in a corner. Stand with your feet shoulder-width apart, holding the free end of the barbell with one hand at shoulder height. Press the barbell forward and up with one arm, then lower it back to the starting position. Quickly switch hands and repeat the press with the opposite arm. Continue alternating for the desired number of repetitions.',
                3
        ),
        (
                'lat pulldown',
                'The lat pulldown targets the upper back muscles, specifically the latissimus dorsi, while also engaging the biceps and shoulders. This exercise is effective for building upper body strength and improving posture.',
                'Sit at a lat pulldown machine with your knees secured under the pad. Grasp the bar with a wide overhand grip and pull it down towards your chest while keeping your back straight. Squeeze your shoulder blades together at the bottom of the movement, then slowly return the bar to the starting position. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'lat pulldown - close grip',
                'The close grip lat pulldown is a variation that targets the latissimus dorsi while also engaging the biceps and middle back muscles. The close grip places more emphasis on the lower part of the lats and biceps.',
                'Sit at a lat pulldown machine with your knees secured under the pad. Grasp the bar with a close neutral grip (palms facing each other) and pull it down towards your chest while keeping your back straight. Squeeze your shoulder blades together at the bottom of the movement, then slowly return the bar to the starting position. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'leg press',
                'The leg press is a compound lower body exercise that targets the quadriceps, glutes, and hamstrings. This exercise is effective for building leg strength and muscle mass while minimizing the load on the lower back.',
                'Sit on the leg press machine with your feet shoulder-width apart on the platform. Lower the safety bars and slowly lower the platform by bending your knees. Press the platform back up by extending your legs, focusing on pushing through your heels. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'leg press calf raise',
                'The leg press calf raise is an isolation exercise that targets the calf muscles, particularly the gastrocnemius and soleus. This exercise helps to build strength and size in the lower legs.',
                'Sit on the leg press machine with your feet positioned low on the platform, about shoulder-width apart. Press the platform up by extending your legs. From this position, push through the balls of your feet to raise your heels, flexing your calves at the top. Lower your heels back down with control. Repeat for the desired number of repetitions.',
                2
        ),
        (
                'leg raise - decline bench',
                'The decline bench leg raise targets the lower abdominal muscles, hip flexors, and helps to build core strength and stability. The decline angle increases the intensity of the exercise.',
                'Lie on a decline bench with your head higher than your hips and your hands gripping the bench behind your head for support. With your legs straight, lift them up towards the ceiling, keeping your lower back pressed against the bench. Slowly lower your legs back down without touching the bench. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'leg raise - med ball',
                'The med ball leg raise is a core exercise that targets the lower abs while adding resistance with a medicine ball. This exercise helps to build core strength and stability.',
                'Lie flat on your back with your legs straight and a medicine ball held between your feet. Place your hands under your lower back or sides for support. Engage your core and lift your legs towards the ceiling, keeping the medicine ball secure. Lower your legs back down with control, without letting the medicine ball touch the ground. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'leverage incline press',
                'The leverage incline press targets the upper chest, shoulders, and triceps. This exercise is effective for building strength and muscle mass in the upper body, particularly the upper pectoral muscles.',
                'Sit on the incline press machine with your back pressed against the pad and your feet flat on the ground. Grasp the handles with an overhand grip and press them upwards until your arms are fully extended. Lower the handles back down with control, keeping your elbows slightly bent at the bottom. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'lying passthroughs',
                'Lying passthroughs target the core, particularly the lower abs, while also engaging the hip flexors. This exercise helps to improve core stability and strength, particularly in the lower abdominal region.',
                'Lie flat on your back with your legs extended and a small object or dumbbell held in your hands above your head. Raise your legs and arms simultaneously, passing the object from your hands to between your feet. Lower your legs and arms back down with control, then raise them again to pass the object back to your hands. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'machine abductions',
                'Machine abductions target the outer thighs, particularly the gluteus medius, gluteus minimus, and tensor fasciae latae. This exercise helps to strengthen the hip abductors and improve hip stability.',
                'Sit on the abduction machine with your feet positioned on the footrests and your thighs against the pads. Grasp the handles on the sides for support. Push your legs outward against the resistance of the machine, squeezing your glutes at the top of the movement. Slowly return to the starting position with control. Repeat for the desired number of repetitions.',
                2
        ),
        (
                'machine fly - negative seated',
                'The machine fly negative seated is a chest exercise that emphasizes the eccentric (lowering) portion of the movement, targeting the pectoral muscles and improving strength and muscle control.',
                'Sit on the machine fly with your back pressed against the pad and your feet flat on the floor. Grasp the handles with your arms slightly bent and bring them together in front of your chest. Slowly allow your arms to open back out, focusing on the negative (eccentric) part of the movement. Control the descent, then bring your arms back together to complete the repetition. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'mckenzie pressup',
                'The McKenzie pressup is a spinal extension exercise that targets the lower back muscles, particularly the erector spinae. This movement is often used for rehabilitation to help relieve lower back pain and improve spinal mobility.',
                'Lie face down on the floor with your hands positioned under your shoulders. Keeping your lower body relaxed, press through your hands to lift your chest off the ground, extending your spine. Hold the position for a moment, then slowly lower yourself back down. Repeat for the desired number of repetitions.',
                2
        ),
        (
                'medicine ball dumbbell chest press',
                'The medicine ball dumbbell chest press is a compound exercise that targets the chest, shoulders, and triceps. This exercise adds an element of instability, engaging the core and stabilizer muscles.',
                'Lie on a flat bench, holding a dumbbell in each hand with a medicine ball between them. Press the dumbbells and medicine ball up towards the ceiling, fully extending your arms. Lower the dumbbells back to your chest with control, keeping the medicine ball stable between them. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'medicine ball chest pass',
                'The medicine ball chest pass is a power exercise that targets the chest, shoulders, and triceps. It is effective for building explosive strength and improving upper body coordination.',
                'Stand facing a wall or a partner, holding a medicine ball at chest height with both hands. Push the ball forward explosively, throwing it against the wall or to your partner. Catch the ball on the rebound or after your partner returns it, and immediately repeat the movement. Perform for the desired number of repetitions or time.',
                2
        ),
        (
                'medicine ball leg raise',
                'The medicine ball leg raise targets the lower abs and hip flexors while adding resistance with a medicine ball. This exercise is effective for building core strength and stability.',
                'Lie flat on your back with your legs straight and a medicine ball held between your feet. Place your hands under your lower back or sides for support. Engage your core and lift your legs towards the ceiling, keeping the medicine ball secure. Lower your legs back down with control, without letting the medicine ball touch the ground. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'mini pro shuttle',
                'The mini pro shuttle is an agility drill that targets the lower body muscles, particularly the glutes, quadriceps, and hamstrings. It is effective for improving speed, quickness, and coordination.',
                'Set up three cones or markers in a straight line, about 5-10 feet apart. Start at the middle cone, sprint to the right cone, touch it, then sprint to the far-left cone, touch it, and finally return to the middle cone. Repeat for the desired number of rounds, focusing on quick changes of direction and maintaining a low center of gravity.',
                3
        ),
        (
                'monster walk',
                'The monster walk is an exercise that targets the glutes, particularly the gluteus medius, and helps to strengthen the hip abductors and improve lower body stability. It is often used as a warm-up or in rehabilitation settings.',
                'Place a resistance band around your legs, just above your knees. Stand with your feet shoulder-width apart, and slightly bend your knees. Step forward and out to the side with one foot, followed by the other foot, maintaining tension in the band. Continue walking in this manner for the desired distance or time, then reverse the direction.',
                2
        ),
        (
                'mountain climbers',
                'Mountain climbers are a full-body exercise that primarily targets the core, hip flexors, and shoulders while also engaging the cardiovascular system. This exercise is effective for building endurance, agility, and overall strength.',
                'Start in a high plank position with your hands under your shoulders and your body in a straight line from head to heels. Quickly alternate driving your knees towards your chest, keeping your core engaged and your back flat. Continue the movement at a rapid pace for the desired number of repetitions or time.',
                3
        ),
        (
                'one arm row',
                'The one-arm row is an exercise that targets the back muscles, particularly the latissimus dorsi, rhomboids, and traps. It also engages the biceps and core for stabilization. This exercise helps to build upper body strength and improve muscular balance.',
                'Place one knee and the same-side hand on a bench, with the opposite foot planted on the ground. Hold a dumbbell in your free hand, letting it hang towards the floor. Keep your back flat and core engaged as you pull the dumbbell up towards your ribcage, squeezing your shoulder blade at the top of the movement. Lower the dumbbell back down with control. Repeat for the desired number of repetitions, then switch sides.',
                3
        ),
        (
                'one arm t-bar row',
                'The one-arm T-bar row is an effective exercise for targeting the upper back, including the latissimus dorsi, rhomboids, and traps, while also engaging the biceps. This unilateral exercise helps to correct muscular imbalances and build strength.',
                'Stand beside a T-bar row machine or secure one end of a barbell in a corner. Grasp the free end of the barbell with one hand, keeping your back straight and core engaged. Pull the barbell up towards your hip, squeezing your shoulder blade at the top of the movement. Lower the barbell back down with control. Repeat for the desired number of repetitions, then switch sides.',
                3
        ),
        (
                'opposite arm to foot v-up on swiss ball',
                'The opposite arm to foot V-up on a Swiss ball is a challenging core exercise that targets the rectus abdominis, obliques, and hip flexors while also engaging the stabilizing muscles. This exercise improves core strength, stability, and coordination.',
                'Lie on your back on a Swiss ball with your arms extended overhead and your legs straight. Lift one arm and the opposite leg towards each other, crunching your torso as you reach to touch your foot. Lower back down with control and repeat on the other side. Continue alternating for the desired number of repetitions.',
                4
        ),
        (
                'overhead cable curls',
                'Overhead cable curls target the biceps, specifically the long head, helping to build the peak of the bicep muscle. This exercise also engages the shoulders and forearms, improving overall upper arm definition.',
                'Set the pulleys on a cable machine to the highest position and attach handles. Stand in the middle of the machine with your arms extended to the sides, gripping the handles with an underhand grip. Curl the handles towards your head, focusing on squeezing your biceps at the top. Slowly return to the starting position with control. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'overhead med ball backwards toss',
                'The overhead med ball backwards toss is a power exercise that targets the posterior chain, including the glutes, hamstrings, and lower back. This exercise helps to build explosive strength and improve athletic performance.',
                'Stand with your feet shoulder-width apart, holding a medicine ball in both hands. Lift the ball overhead, then explosively throw it backwards over your head, using your hips and legs to generate power. Let the ball land behind you, then retrieve it and repeat for the desired number of repetitions.',
                3
        ),
        (
                'overhead tricep extension',
                'The overhead tricep extension is a compound exercise that targets the triceps, particularly the long head. This exercise helps to build strength and muscle mass in the upper arms.',
                'Stand or sit with your feet shoulder-width apart, holding a dumbbell with both hands above your head. Keep your upper arms close to your ears and elbows pointing forward as you lower the dumbbell behind your head. Extend your arms to lift the dumbbell back to the starting position. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'overhead tricep extension - dumbbell',
                'The overhead tricep extension with a dumbbell focuses on the triceps, especially the long head. This exercise helps to enhance arm strength and hypertrophy by allowing for a full stretch of the triceps.',
                'Stand or sit with your feet shoulder-width apart, holding a dumbbell in both hands behind your head, elbows pointing upward. Keep your upper arms stationary as you extend your elbows to lift the dumbbell overhead. Lower the dumbbell back down with control, allowing a full stretch in the triceps. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'overhead tricep extension - rope cable',
                'The overhead tricep extension with a rope cable is a triceps-focused exercise that targets all three heads of the triceps. The rope attachment allows for a greater range of motion and muscle activation, leading to improved triceps development.',
                'Attach a rope to the high pulley of a cable machine. Stand with your back to the machine and grasp the rope with both hands. Step forward to create tension in the cable, then extend your arms overhead, keeping your elbows close to your ears. Pull the rope apart as you extend your arms, squeezing the triceps at the top. Slowly return to the starting position. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'overhead tricep extension - seated dumbbell',
                'The seated dumbbell overhead tricep extension is a variation that emphasizes stability and allows for a more controlled movement, targeting the triceps, particularly the long head. This exercise is effective for increasing arm strength and muscle size.',
                'Sit on a bench with a back support, holding a dumbbell with both hands behind your head, elbows pointing upward. Keep your upper arms stationary as you extend your elbows to lift the dumbbell overhead. Lower the dumbbell back down with control, allowing a full stretch in the triceps. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'pallof press',
                'The Pallof press is a core stability exercise that targets the rectus abdominis, obliques, and transverse abdominis. This anti-rotation exercise helps to improve core strength, stability, and control.',
                'Attach a handle to a cable machine at chest height. Stand perpendicular to the machine and grasp the handle with both hands, holding it close to your chest. Step away from the machine to create tension in the cable, then extend your arms straight out in front of you, resisting the pull of the cable. Hold the position for a moment, then return the handle to your chest. Repeat for the desired number of repetitions, then switch sides.',
                3
        ),
        (
                'pigeon stretch',
                'The pigeon stretch is a deep hip-opening stretch that targets the glutes, hip flexors, and piriformis muscles. This stretch helps to improve hip mobility, alleviate lower back pain, and release tension in the hips.',
                'Start in a tabletop position on your hands and knees. Bring your right knee forward and place it behind your right wrist, with your shin parallel to the front of the mat. Extend your left leg straight back behind you, keeping your hips square to the floor. Lower your upper body over your right leg, resting on your forearms or chest. Hold the stretch for the desired time, then switch sides.',
                2
        ),
        (
                'plank',
                'The plank is a fundamental core exercise that targets the rectus abdominis, transverse abdominis, and obliques, while also engaging the shoulders and glutes. This exercise helps to build core strength and stability.',
                'Start in a forearm plank position with your elbows directly under your shoulders and your body in a straight line from head to heels. Engage your core, squeeze your glutes, and hold the position without letting your hips sag or rise. Maintain steady breathing and hold for the desired amount of time.',
                2
        ),
        (
                'plank - weighted',
                'The weighted plank is an advanced variation of the plank exercise, adding resistance to increase the challenge for the core, shoulders, and glutes. This exercise helps to build core strength, stability, and endurance.',
                'Start in a forearm plank position with your elbows directly under your shoulders and your body in a straight line from head to heels. Have a partner place a weight plate on your lower back, or use a weighted vest. Engage your core, squeeze your glutes, and hold the position without letting your hips sag or rise. Maintain steady breathing and hold for the desired amount of time.',
                3
        ),
        (
                'plank on swiss ball',
                'The plank on a Swiss ball is a core stability exercise that targets the rectus abdominis, transverse abdominis, and obliques, while also engaging the shoulders and glutes. The instability of the Swiss ball adds an extra challenge, improving core strength and stability.',
                'Place your forearms on a Swiss ball and extend your legs behind you, balancing on your toes. Engage your core, squeeze your glutes, and hold your body in a straight line from head to heels. Maintain the position without letting your hips sag or rise. Hold for the desired amount of time, focusing on maintaining stability.',
                3
        ),
        (
                'plank on swiss ball then stir the pot',
                'This variation of the plank on a Swiss ball, called ''stir the pot,'' adds a dynamic element to the exercise by incorporating circular movements. It targets the core muscles, including the rectus abdominis, transverse abdominis, and obliques, while also engaging the shoulders and glutes.',
                'Place your forearms on a Swiss ball and extend your legs behind you, balancing on your toes. Engage your core, squeeze your glutes, and hold your body in a straight line. Begin making small circular motions with your forearms on the Swiss ball, as if stirring a pot. Perform the movements in one direction, then switch to the opposite direction. Continue for the desired amount of time.',
                4
        ),
        (
                'plate external rotation - incline',
                'The plate external rotation on an incline bench targets the rotator cuff muscles, particularly the infraspinatus and teres minor, which are essential for shoulder stability and injury prevention.',
                'Lie face down on an incline bench, holding a weight plate in one hand with your arm hanging straight down. Rotate your arm outward, keeping your elbow close to your side, until your hand is level with your shoulder. Slowly return to the starting position with control. Repeat for the desired number of repetitions, then switch sides.',
                3
        ),
        (
                'power skips',
                'Power skips are a plyometric exercise that targets the lower body muscles, particularly the glutes, quadriceps, and calves, while also engaging the core. This exercise helps to build explosive strength, coordination, and cardiovascular endurance.',
                'Stand with your feet shoulder-width apart. Begin by skipping forward, driving one knee up towards your chest as you simultaneously push off the opposite foot to gain height. Swing your arms to generate momentum and balance. Land softly on the balls of your feet and immediately repeat the movement with the opposite leg. Continue skipping forward for the desired distance or time.',
                3
        ),
        (
                'preacher curls',
                'Preacher curls are an isolation exercise that targets the biceps, particularly the brachialis, and biceps brachii. This exercise helps to build arm strength and size by isolating the biceps and minimizing the use of other muscles.',
                'Sit on a preacher bench with your arms resting on the pad and your chest pressed against it. Hold a barbell or an EZ curl bar with an underhand grip. Curl the bar up towards your shoulders, keeping your upper arms stationary on the pad. Squeeze your biceps at the top of the movement, then slowly lower the bar back down with control. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'prone cobra',
                'The prone cobra is an isometric exercise that targets the muscles of the lower back, shoulders, and upper back, including the erector spinae, rhomboids, and traps. This exercise helps to improve posture, spinal stability, and upper back strength.',
                'Lie face down on the floor with your arms extended along your sides, palms facing down. Engage your lower back muscles to lift your chest off the ground while simultaneously lifting your arms, squeezing your shoulder blades together. Keep your gaze slightly forward and hold the position for the desired time. Lower yourself back down with control and repeat as needed.',
                2
        ),
        (
                'pull ups',
                'Pull ups are a fundamental upper body exercise that primarily targets the back muscles, particularly the latissimus dorsi, while also engaging the biceps, shoulders, and core. This exercise is excellent for building upper body strength and muscle mass.',
                'Grasp a pull-up bar with an overhand grip, hands slightly wider than shoulder-width apart. Hang with your arms fully extended and your feet off the ground. Pull your body up towards the bar by engaging your back and biceps, keeping your core tight. Continue pulling until your chin is above the bar, then lower yourself back down with control. Repeat for the desired number of repetitions.',
                4
        ),
        (
                'pull ups - negative',
                'Negative pull ups focus on the eccentric phase of the pull up, which emphasizes the lowering portion of the movement. This exercise is great for building strength in the back, biceps, and shoulders, particularly for those working towards performing full pull ups.',
                'Start at the top position of a pull up with your chin above the bar, either by jumping or using a step. Slowly lower your body down with control, resisting gravity as much as possible. Focus on engaging your back and biceps during the descent. Once your arms are fully extended, step back up to the starting position and repeat for the desired number of repetitions.',
                3
        ),
        (
                'pull ups - weighted',
                'Weighted pull ups are an advanced variation of the pull up that adds resistance to the exercise, making it more challenging. This exercise significantly increases the strength and size of the upper back, biceps, and shoulders.',
                'Attach a weight belt with a plate or use a weighted vest. Grasp a pull-up bar with an overhand grip, hands slightly wider than shoulder-width apart. Hang with your arms fully extended and your feet off the ground. Pull your body up towards the bar, engaging your back and biceps. Continue pulling until your chin is above the bar, then lower yourself back down with control. Repeat for the desired number of repetitions.',
                5
        ),
        (
                'pull ups - rope/towel',
                'Rope or towel pull ups are a variation that increases the difficulty by requiring a stronger grip and greater stabilization. This exercise targets the back, biceps, forearms, and core.',
                'Drape a rope or towel over a pull-up bar and grasp each end with your hands. Hang with your arms fully extended and your feet off the ground. Pull your body up towards the bar, engaging your back, biceps, and forearms. Continue pulling until your chin is above the bar, then lower yourself back down with control. Repeat for the desired number of repetitions.',
                4
        ),
        (
                'pullover - dumbbell with isometric hold',
                'The dumbbell pullover with an isometric hold targets the chest, lats, and triceps, while also engaging the core. The isometric hold increases time under tension, leading to greater strength and muscle gains.',
                'Lie on a bench with your upper back supported and your feet planted on the ground. Hold a dumbbell with both hands above your chest, arms slightly bent. Lower the dumbbell behind your head, keeping your arms slightly bent, until you feel a stretch in your chest and lats. Hold the position for a few seconds, then return to the starting position with control. Repeat for the desired number of repetitions.',
                4
        ),
        (
                'push ups',
                'Push ups are a classic bodyweight exercise that targets the chest, shoulders, and triceps, while also engaging the core and lower body for stability. This exercise is fundamental for building upper body strength and endurance.',
                'Start in a high plank position with your hands slightly wider than shoulder-width apart. Keep your body in a straight line from head to heels, and lower your chest towards the ground by bending your elbows. Push through your palms to return to the starting position. Repeat for the desired number of repetitions.',
                2
        ),
        (
                'push ups - diamond',
                'Diamond push ups are a push-up variation that places more emphasis on the triceps and inner chest. This exercise helps to build upper body strength and muscle definition, particularly in the arms.',
                'Start in a high plank position with your hands close together under your chest, forming a diamond shape with your thumbs and index fingers. Keep your body in a straight line from head to heels as you lower your chest towards the ground by bending your elbows. Push through your palms to return to the starting position. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'push ups - stability',
                'Stability push ups are a challenging variation that increases core and shoulder engagement by performing the exercise on an unstable surface, such as a BOSU ball or Swiss ball. This exercise improves balance, stability, and overall upper body strength.',
                'Place your hands on an unstable surface like a BOSU ball or Swiss ball and assume a high plank position with your body in a straight line from head to heels. Lower your chest towards the ball by bending your elbows, maintaining balance and control. Push through your palms to return to the starting position. Repeat for the desired number of repetitions.',
                4
        ),
        (
                'push ups - wide',
                'Wide push ups are a variation that places more emphasis on the chest muscles, particularly the pectoralis major. This exercise helps to build upper body strength and improve muscle definition.',
                'Start in a high plank position with your hands placed wider than shoulder-width apart. Keep your body in a straight line from head to heels as you lower your chest towards the ground by bending your elbows. Push through your palms to return to the starting position. Repeat for the desired number of repetitions.',
                2
        ),
        (
                'quadruped thoracic rotation',
                'The quadruped thoracic rotation is a mobility exercise that targets the thoracic spine, helping to improve spinal mobility, posture, and overall movement quality. This exercise is beneficial for alleviating upper back stiffness and enhancing rotational movement.',
                'Start on all fours with your hands under your shoulders and your knees under your hips. Place one hand behind your head, with your elbow pointing out to the side. Rotate your upper body to bring your elbow towards the opposite knee, then rotate in the opposite direction, bringing your elbow up towards the ceiling. Repeat for the desired number of repetitions, then switch sides.',
                2
        ),
        (
                'reactive vertical tuck jump',
                'The reactive vertical tuck jump is a plyometric exercise that targets the lower body muscles, particularly the quadriceps, glutes, and calves, while also engaging the core. This exercise helps to build explosive power, agility, and coordination.',
                'Stand with your feet shoulder-width apart. Bend your knees and explode upwards, tucking your knees towards your chest at the peak of the jump. Land softly on the balls of your feet and immediately jump again, focusing on quick, reactive movements. Continue for the desired number of repetitions or time.',
                4
        ),
        (
                'rear delt raise',
                'The rear delt raise is an isolation exercise that targets the posterior deltoids (rear delts) and the muscles of the upper back, particularly the rhomboids and traps. This exercise helps to build shoulder stability and improve posture.',
                'Hold a pair of dumbbells and bend forward at the hips until your torso is nearly parallel to the floor. With a slight bend in your elbows, raise the dumbbells out to the sides until your arms are parallel with the ground. Squeeze your shoulder blades together at the top of the movement. Lower the dumbbells back to the starting position with control. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'reverse lunges - barbell',
                'Barbell reverse lunges are a lower body exercise that targets the quadriceps, hamstrings, and glutes, while also engaging the core for stability. This exercise helps to build strength, balance, and muscle mass in the legs.',
                'Stand with a barbell across your upper back, feet hip-width apart. Step one foot back into a lunge position, lowering your hips until both knees are bent at a 90-degree angle. Push through the front heel to return to the starting position. Repeat for the desired number of repetitions, then switch legs.',
                4
        ),
        (
                'reverse crunch',
                'The reverse crunch is a core exercise that targets the lower abdominal muscles, helping to build strength and stability in the core. This exercise is effective for improving core control and reducing the risk of lower back injuries.',
                'Lie flat on your back with your arms at your sides and your legs bent at a 90-degree angle. Engage your core and lift your hips off the ground, bringing your knees towards your chest. Lower your hips back down with control, keeping your lower back pressed into the floor. Repeat for the desired number of repetitions.',
                2
        ),
        (
                'reverse lunges - dumbbell',
                'Dumbbell reverse lunges are a lower body exercise that targets the quadriceps, hamstrings, and glutes, while also engaging the core for stability. This exercise helps to build strength, balance, and muscle mass in the legs.',
                'Stand with a dumbbell in each hand, feet hip-width apart. Step one foot back into a lunge position, lowering your hips until both knees are bent at a 90-degree angle. Push through the front heel to return to the starting position. Repeat for the desired number of repetitions, then switch legs.',
                3
        ),
        (
                'rollover reach',
                'The rollover reach is a dynamic stretch that targets the hamstrings, lower back, and glutes, while also improving hip mobility and flexibility. This exercise is useful for warming up the posterior chain and enhancing range of motion.',
                'Lie flat on your back with your arms extended overhead and your legs straight. Lift your legs up and over your head, reaching your toes towards the floor behind you. Then, lower your legs back down and reach forward with your hands towards your toes. Repeat the movement, focusing on a smooth and controlled motion.',
                2
        ),
        (
                'rope cable face pull',
                'Rope cable face pulls are an upper body exercise that targets the posterior deltoids, rhomboids, and traps. This exercise helps to improve shoulder stability, posture, and upper back strength.',
                'Attach a rope to the high pulley of a cable machine. Stand facing the machine, holding the ends of the rope with an overhand grip. Pull the rope towards your face, leading with your elbows and keeping your upper arms parallel to the ground. Squeeze your shoulder blades together at the top of the movement, then slowly return to the starting position. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'rope tricep extension',
                'The rope tricep extension is an isolation exercise that targets the triceps, particularly the long and lateral heads. This exercise helps to build strength and muscle mass in the upper arms.',
                'Attach a rope to the high pulley of a cable machine. Stand facing the machine, holding the ends of the rope with an overhand grip. Keep your elbows close to your sides as you extend your arms downwards, spreading the rope apart at the bottom of the movement. Squeeze your triceps at the bottom, then slowly return to the starting position. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'russian twist - seated',
                'The seated Russian twist is a core exercise that targets the obliques and rectus abdominis. This exercise helps to build rotational strength and improve core stability.',
                'Sit on the floor with your knees bent and your feet off the ground. Lean back slightly to engage your core. Hold a weight or medicine ball with both hands in front of your chest. Twist your torso to the right, bringing the weight towards the floor beside your hip, then twist to the left. Continue alternating sides for the desired number of repetitions.',
                3
        ),
        (
                'russian twists',
                'Russian twists are a core exercise that targets the obliques and rectus abdominis. This exercise helps to build rotational strength and improve core stability.',
                'Sit on the floor with your knees bent and your feet off the ground. Lean back slightly to engage your core. Hold a weight or medicine ball with both hands in front of your chest. Twist your torso to the right, bringing the weight towards the floor beside your hip, then twist to the left. Continue alternating sides for the desired number of repetitions.',
                3
        ),
        (
                'russian twists - barbell',
                'Russian twists with a barbell are a core exercise that targets the obliques and rectus abdominis, with the added challenge of the barbell increasing resistance. This exercise helps to build rotational strength and improve core stability.',
                'Sit on the floor with your knees bent and your feet off the ground. Lean back slightly to engage your core. Hold one end of a barbell with both hands in front of your chest. Twist your torso to the right, bringing the barbell towards the floor beside your hip, then twist to the left. Continue alternating sides for the desired number of repetitions.',
                4
        ),
        (
                'russian twists - dumbbell',
                'Russian twists with a dumbbell are a core exercise that targets the obliques and rectus abdominis, using a dumbbell to add resistance. This exercise helps to build rotational strength and improve core stability.',
                'Sit on the floor with your knees bent and your feet off the ground. Lean back slightly to engage your core. Hold a dumbbell with both hands in front of your chest. Twist your torso to the right, bringing the dumbbell towards the floor beside your hip, then twist to the left. Continue alternating sides for the desired number of repetitions.',
                3
        ),
        (
                'russian twists - medball',
                'Russian twists with a medicine ball are a core exercise that targets the obliques and rectus abdominis, with the added challenge of a medicine ball to increase resistance. This exercise helps to build rotational strength and improve core stability.',
                'Sit on the floor with your knees bent and your feet off the ground. Lean back slightly to engage your core. Hold a medicine ball with both hands in front of your chest. Twist your torso to the right, bringing the medicine ball towards the floor beside your hip, then twist to the left. Continue alternating sides for the desired number of repetitions.',
                3
        ),
        (
                'scarecrows',
                'Scarecrows are a shoulder exercise that targets the rotator cuff muscles, particularly the infraspinatus and teres minor, as well as the posterior deltoids. This exercise helps to improve shoulder stability, mobility, and prevent injuries.',
                'Hold a pair of light dumbbells with your arms bent at a 90-degree angle, elbows at shoulder height. Rotate your arms upward, so that your forearms are parallel to the ground, then rotate your arms downward, bringing the forearms back to the starting position. Focus on slow and controlled movements to maximize muscle engagement. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'scissor kicks holding dumbbell high',
                'Scissor kicks with a dumbbell held overhead is a core exercise that targets the lower abdominals and hip flexors while also engaging the shoulders and upper body for stabilization. This exercise improves core strength and stability.',
                'Lie flat on your back with a dumbbell held straight above your chest. Lift your legs a few inches off the ground and perform a scissor-like motion, alternating legs up and down. Keep your lower back pressed into the floor and maintain the dumbbell steady above you. Continue the motion for the desired number of repetitions or time.',
                3
        ),
        (
                'seal walks',
                'Seal walks are a core and upper body exercise that targets the shoulders, chest, and core muscles. This exercise involves moving in a plank position, emphasizing shoulder stability and core control.',
                'Start in a plank position with your forearms on the ground and your body in a straight line. Keeping your core engaged and hips level, walk your body forward by moving your forearms and dragging your legs behind you. Continue walking forward for the desired distance or time, maintaining a stable and controlled movement.',
                4
        ),
        (
                'seated groin stretch',
                'The seated groin stretch is a flexibility exercise that targets the inner thighs, particularly the adductor muscles. This stretch helps to improve flexibility, mobility, and reduce the risk of groin injuries.',
                'Sit on the floor with your knees bent and the soles of your feet pressed together. Hold your feet with your hands and gently press your knees towards the ground, feeling a stretch in your inner thighs. Hold the position for the desired time, breathing deeply and relaxing into the stretch.',
                1
        ),
        (
                'seated lat row - wide grip',
                'The seated lat row with a wide grip targets the back muscles, particularly the latissimus dorsi and rhomboids, while also engaging the biceps and shoulders. This exercise helps to build upper body strength and muscle mass.',
                'Sit on a seated row machine with your feet firmly on the footrests. Grasp the wide grip handle with both hands, keeping your back straight. Pull the handle towards your torso, squeezing your shoulder blades together at the end of the movement. Slowly return to the starting position with control. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'seated row - wide neutral grip',
                'The seated row with a wide neutral grip targets the back muscles, particularly the latissimus dorsi, rhomboids, and traps. This exercise also engages the biceps and forearms, helping to build upper body strength and improve posture.',
                'Sit on a seated row machine with your feet firmly on the footrests. Grasp the wide neutral grip handle with both hands, keeping your back straight. Pull the handle towards your torso, squeezing your shoulder blades together at the end of the movement. Slowly return to the starting position with control. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'shrugs - barbell',
                'Barbell shrugs are an isolation exercise that targets the upper trapezius muscles, helping to build neck and shoulder strength and improve upper body posture.',
                'Stand with your feet shoulder-width apart, holding a barbell in front of your thighs with an overhand grip. Keep your arms straight and lift your shoulders towards your ears, squeezing your traps at the top of the movement. Lower your shoulders back down with control. Repeat for the desired number of repetitions.',
                2
        ),
        (
                'shrugs - dumbbell',
                'Dumbbell shrugs are an isolation exercise that targets the upper trapezius muscles, helping to build neck and shoulder strength and improve upper body posture.',
                'Stand with your feet shoulder-width apart, holding a dumbbell in each hand at your sides with your palms facing your body. Keep your arms straight and lift your shoulders towards your ears, squeezing your traps at the top of the movement. Lower your shoulders back down with control. Repeat for the desired number of repetitions.',
                2
        ),
        (
                'single arm cable fly - front',
                'The single arm cable fly (front) is an isolation exercise that targets the chest muscles, particularly the pectoralis major. This exercise helps to improve muscle symmetry and build strength in the upper body.',
                'Set a cable machine to the high pulley position and attach a single handle. Stand with your side facing the machine, holding the handle in the hand farthest from the machine. With a slight bend in your elbow, bring the handle across your body, focusing on squeezing your chest at the end of the movement. Slowly return to the starting position with control. Repeat for the desired number of repetitions, then switch sides.',
                3
        ),
        (
                'single arm cable fly - lateral',
                'The single arm cable fly (lateral) is an isolation exercise that targets the chest muscles, particularly the pectoralis major, with an emphasis on the outer portion of the chest. This exercise helps to build strength and definition in the chest.',
                'Set a cable machine to the low pulley position and attach a single handle. Stand with your side facing the machine, holding the handle in the hand farthest from the machine. With a slight bend in your elbow, bring the handle across your body, focusing on squeezing your chest at the end of the movement. Slowly return to the starting position with control. Repeat for the desired number of repetitions, then switch sides.',
                3
        ),
        (
                'single leg glute bridge on bosu ball',
                'The single leg glute bridge on a Bosu ball is a lower body exercise that targets the glutes, hamstrings, and core, while also challenging balance and stability. This exercise helps to build strength in the posterior chain and improve hip stability.',
                'Lie on your back with one foot on the flat side of a Bosu ball and the other leg extended straight up. Push through the foot on the Bosu ball to lift your hips off the ground, engaging your glutes and hamstrings. Hold the top position for a moment, then lower your hips back down with control. Repeat for the desired number of repetitions, then switch legs.',
                4
        ),
        (
                'sit ups',
                'Sit ups are a classic core exercise that targets the rectus abdominis, with some engagement of the hip flexors. This exercise helps to build core strength and improve abdominal muscle tone.',
                'Lie flat on your back with your knees bent and feet flat on the floor. Cross your arms over your chest or place your hands behind your head. Engage your core and lift your upper body towards your knees, bringing your chest up to your thighs. Lower yourself back down with control. Repeat for the desired number of repetitions.',
                2
        ),
        (
                'sit ups - weighted',
                'Weighted sit ups are an advanced variation of the traditional sit up, adding resistance to increase the challenge for the abdominal muscles. This exercise helps to build greater core strength and muscle endurance.',
                'Lie flat on your back with your knees bent and feet flat on the floor. Hold a weight plate or dumbbell against your chest. Engage your core and lift your upper body towards your knees, bringing your chest up to your thighs. Lower yourself back down with control. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'skull crushers',
                'Skull crushers are a triceps isolation exercise that targets all three heads of the triceps, particularly the long head. This exercise is effective for building upper arm strength and muscle mass.',
                'Lie on a bench with a barbell or EZ curl bar held above your chest, arms fully extended. Keep your upper arms stationary as you lower the bar towards your forehead by bending your elbows. Extend your arms back to the starting position, squeezing your triceps at the top. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'skull crushers - ez bar',
                'EZ curl bar skull crushers are a triceps isolation exercise that targets all three heads of the triceps, with the EZ curl bar providing a more comfortable grip. This exercise helps to build upper arm strength and muscle definition.',
                'Lie on a bench with an EZ curl bar held above your chest, arms fully extended. Keep your upper arms stationary as you lower the bar towards your forehead by bending your elbows. Extend your arms back to the starting position, squeezing your triceps at the top. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'skull crushers - lying',
                'Lying skull crushers are a triceps isolation exercise that targets all three heads of the triceps, particularly the long head. This exercise helps to build upper arm strength and muscle mass.',
                'Lie on a bench with a barbell or EZ curl bar held above your chest, arms fully extended. Keep your upper arms stationary as you lower the bar towards your forehead by bending your elbows. Extend your arms back to the starting position, squeezing your triceps at the top. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'smith machine squat',
                'Smith machine squats are a lower body exercise that targets the quadriceps, hamstrings, and glutes, with the added stability of the Smith machine. This exercise helps to build leg strength and muscle mass while minimizing the need for balance.',
                'Stand under the bar of a Smith machine with your feet shoulder-width apart. Position the bar across your upper back and shoulders, and unhook it from the safety locks. Lower your body into a squat by bending your knees and hips, keeping your chest up and back straight. Push through your heels to return to the starting position. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'spider curls',
                'Spider curls are a bicep isolation exercise that targets the brachialis and biceps brachii, particularly the short head. This exercise helps to build arm strength and muscle size by focusing on the contraction of the biceps.',
                'Lie face down on an incline bench with your arms hanging straight down. Hold a barbell or EZ curl bar with an underhand grip. Curl the weight up towards your shoulders, keeping your upper arms stationary. Squeeze your biceps at the top of the movement, then slowly lower the weight back down with control. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'sprint',
                'Sprinting is a high-intensity cardiovascular exercise that primarily targets the lower body muscles, including the quadriceps, hamstrings, and glutes, while also engaging the core. This exercise helps to build explosive power, speed, and cardiovascular endurance.',
                'Begin by standing in a starting position, then explode forward by driving your knees up and pumping your arms. Focus on maintaining a strong stride, with quick turnover and full extension of the hips. Continue sprinting for the desired distance or time, then decelerate gradually to avoid injury.',
                4
        ),
        (
                'squat',
                'The squat is a fundamental lower body exercise that targets the quadriceps, hamstrings, and glutes, while also engaging the core for stability. This exercise helps to build overall leg strength, muscle mass, and functional fitness.',
                'Stand with your feet shoulder-width apart, toes pointed slightly outward. Keep your chest up and back straight as you lower your hips down into a squat by bending your knees and hips. Push through your heels to return to the starting position. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'squat - eccentric',
                'Eccentric squats focus on the lowering phase of the squat, emphasizing control and muscle tension. This exercise targets the quadriceps, hamstrings, and glutes, helping to build strength and muscle mass while improving control and stability.',
                'Stand with your feet shoulder-width apart, toes pointed slightly outward. Keep your chest up and back straight as you slowly lower your hips down into a squat, taking 3-5 seconds to descend. Push through your heels to return to the starting position. Repeat for the desired number of repetitions.',
                4
        ),
        (
                'squat - isometric',
                'Isometric squats involve holding a squat position to build strength and endurance in the quadriceps, hamstrings, and glutes. This exercise helps to improve muscle endurance, stability, and control.',
                'Stand with your feet shoulder-width apart, toes pointed slightly outward. Lower your hips down into a squat, stopping when your thighs are parallel to the ground. Hold this position for the desired amount of time, keeping your chest up and core engaged. Push through your heels to return to the starting position.',
                4
        ),
        (
                'squat jumps - dumbbell',
                'Dumbbell squat jumps are a plyometric exercise that targets the quadriceps, hamstrings, glutes, and calves while also engaging the core. This exercise helps to build explosive power, strength, and cardiovascular endurance.',
                'Hold a dumbbell in each hand at your sides. Stand with your feet shoulder-width apart, lower into a squat, and then explosively jump up, pushing through your heels. Land softly and immediately go into the next squat jump. Repeat for the desired number of repetitions.',
                4
        ),
        (
                'standing cable oblique crunch bends',
                'Standing cable oblique crunch bends target the oblique muscles along the sides of the abdomen, helping to build strength and definition in the core while also improving balance and stability.',
                'Attach a single handle to the high pulley of a cable machine. Stand sideways to the machine, holding the handle with the hand closest to the machine. With your other hand on your hip, bend your torso sideways away from the machine, squeezing your . Return to the starting position with control. Repeat for the desired number of repetitions, then switch sides.',
                3
        ),
        (
                'standing cable rotation',
                'Standing cable rotations are a core exercise that targets the  and transverse abdominis. This exercise helps to build rotational strength and stability, improving core control and balance.',
                'Attach a single handle to a cable machine at chest height. Stand sideways to the machine, holding the handle with both hands. With your feet shoulder-width apart, rotate your torso away from the machine, pulling the handle across your body. Keep your arms straight and core engaged. Return to the starting position with control. Repeat for the desired number of repetitions, then switch sides.',
                3
        ),
        (
                'standing chest press - close grip',
                'The standing chest press with a close grip targets the pectoral muscles, triceps, and shoulders, helping to build upper body strength and muscle mass. The standing position also engages the core for stability.',
                'Stand facing a cable machine with a handle or bar set at chest height. Hold the handle with a close grip, keeping your elbows close to your sides. Push the handle forward, extending your arms fully while squeezing your chest. Return to the starting position with control. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'standing medium high cable fly',
                'The standing medium high cable fly is an isolation exercise that targets the pectoral muscles, particularly the upper chest. This exercise helps to build chest strength and muscle definition.',
                'Set the pulleys of a cable machine to a medium-high position. Stand with your feet shoulder-width apart, holding a handle in each hand. With a slight bend in your elbows, bring your arms together in front of your chest, focusing on squeezing your pectorals. Slowly return to the starting position with control. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'stir the pot',
                'Stir the pot is a core stabilization exercise that targets the rectus abdominis, , and transverse abdominis, while also engaging the shoulders. This exercise improves core strength, stability, and overall body control.',
                'Place your forearms on a Swiss ball and assume a plank position with your body in a straight line. Keeping your core tight, slowly move your forearms in a circular motion as if stirring a pot. Perform the circles in one direction for the desired number of repetitions, then switch to the other direction.',
                4
        ),
        (
                'straight arm pulldown',
                'The straight arm pulldown is an isolation exercise that targets the latissimus dorsi muscles, while also engaging the core. This exercise helps to build back strength and muscle definition.',
                'Stand facing a cable machine with a straight bar attachment set at the high pulley. Hold the bar with an overhand grip, arms straight. Pull the bar down towards your thighs while keeping your arms straight, focusing on engaging your lats. Slowly return to the starting position with control. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'supermans',
                'Supermans are a bodyweight exercise that targets the lower back, glutes, and hamstrings. This exercise helps to improve spinal stability, strengthen the posterior chain, and enhance overall core strength.',
                'Lie face down on the floor with your arms extended in front of you and your legs straight. Simultaneously lift your arms, chest, and legs off the ground, squeezing your glutes and lower back muscles. Hold the top position for a moment, then slowly lower back down with control. Repeat for the desired number of repetitions.',
                2
        ),
        (
                'swiss ball crunch',
                'Swiss ball crunches are a core exercise that targets the rectus abdominis while engaging the  and deep core muscles for stability. This exercise helps to build core strength and improve abdominal muscle definition.',
                'Sit on a Swiss ball and walk your feet forward until your lower back is supported on the ball. Place your hands behind your head, elbows out to the sides. Engage your core and lift your upper body towards your knees, bringing your chest up and forward. Lower yourself back down with control. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'swiss ball hamstring curl',
                'Swiss ball hamstring curls are a lower body exercise that targets the hamstrings and glutes while also engaging the core for stability. This exercise helps to build strength and muscle mass in the posterior chain.',
                'Lie on your back with your heels resting on top of a Swiss ball, arms at your sides. Lift your hips off the ground, forming a straight line from your shoulders to your heels. Bend your knees to roll the ball towards your glutes, engaging your hamstrings. Slowly extend your legs to roll the ball back to the starting position. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'swiss ball pikes',
                'Swiss ball pikes are an advanced core exercise that targets the rectus abdominis, , and hip flexors, while also engaging the shoulders and arms. This exercise helps to build core strength, stability, and control.',
                'Start in a plank position with your feet on a Swiss ball and your hands on the floor. Engage your core and use your abdominal muscles to lift your hips towards the ceiling, rolling the ball towards your chest. Keep your legs straight as you form an inverted V shape. Slowly lower your hips back down to the starting position with control. Repeat for the desired number of repetitions.',
                4
        ),
        (
                'toes to ball',
                'Toes to ball is a core exercise that primarily targets the lower abdominals while also engaging the hip flexors. This exercise helps to build core strength and improve muscle tone.',
                'Lie flat on your back with your arms extended overhead and a Swiss ball between your feet. Lift your legs towards the ceiling, bringing the ball towards your hands while keeping your lower back pressed into the floor. Lower the ball back down with control, keeping your legs straight. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'back squat',
                'The back squat is a foundational lower body exercise that targets the quadriceps, hamstrings, glutes, and core. This exercise helps to build overall leg strength, muscle mass, and functional fitness.',
                'Position a barbell on your upper back and shoulders, standing with your feet shoulder-width apart. Lower your hips down into a squat by bending your knees and hips, keeping your chest up and back straight. Push through your heels to return to the starting position. Repeat for the desired number of repetitions.',
                4
        ),
        (
                'band shoulder mobility series',
                'The band shoulder mobility series is a collection of exercises that target the shoulder muscles and joints, helping to improve mobility, flexibility, and range of motion. This series is beneficial for warming up and reducing the risk of shoulder injuries.',
                'Using a resistance band, perform a series of shoulder mobility exercises such as band pull-aparts, overhead band stretches, and band dislocates. Each exercise should be performed with controlled movements, focusing on proper form and range of motion. Repeat each exercise for the desired number of repetitions.',
                2
        ),
        (
                'tricep extension',
                'Tricep extensions are an isolation exercise that targets all three heads of the triceps, helping to build upper arm strength and muscle mass.',
                'Hold a dumbbell or cable attachment with both hands above your head, arms fully extended. Keep your upper arms stationary as you lower the weight behind your head by bending your elbows. Extend your arms back to the starting position, squeezing your triceps at the top. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'tricep extension - band',
                'Band tricep extensions are an isolation exercise that targets all three heads of the triceps, using a resistance band to build upper arm strength and muscle mass.',
                'Secure a resistance band to a high anchor point. Stand facing away from the anchor, holding the band with both hands above your head, arms fully extended. Keep your upper arms stationary as you lower the band behind your head by bending your elbows. Extend your arms back to the starting position, squeezing your triceps at the top. Repeat for the desired number of repetitions.',
                2
        ),
        (
                'tricep kickbacks - cable',
                'Cable tricep kickbacks are an isolation exercise that targets all three heads of the triceps, helping to build upper arm strength and muscle mass.',
                'Attach a handle to a low pulley of a cable machine. Stand with your torso bent forward at the hips, holding the handle with one hand, and keep your upper arm stationary as you extend your forearm backwards, fully contracting your triceps at the top of the movement. Slowly return to the starting position. Repeat for the desired number of repetitions, then switch arms.',
                3
        ),
        (
                'tricep pushdown - straight bar attachment',
                'The tricep pushdown with a straight bar attachment is an isolation exercise that targets all three heads of the triceps, helping to build upper arm strength and muscle mass.',
                'Attach a straight bar to a high pulley of a cable machine. Stand facing the machine, holding the bar with an overhand grip. Keep your elbows close to your sides as you extend your arms downward, fully contracting your triceps at the bottom of the movement. Slowly return to the starting position. Repeat for the desired number of repetitions.',
                2
        ),
        (
                'tricep stretch',
                'The tricep stretch targets the triceps and shoulder muscles, helping to improve flexibility and reduce the risk of injury. This stretch is beneficial for cooling down after upper body workouts.',
                'Stand or sit with one arm extended overhead. Bend your elbow to lower your hand behind your head, reaching down towards the opposite shoulder blade. Use your opposite hand to gently press on your bent elbow, deepening the stretch. Hold for the desired amount of time, then switch sides.',
                1
        ),
        (
                'triple jump',
                'The triple jump is a dynamic plyometric exercise that targets the quadriceps, hamstrings, glutes, and calves, while also engaging the core for stability. This exercise helps to build explosive power, coordination, and agility.',
                'Start by performing a hop on one leg, followed by a bound with the opposite leg, and finish with a jump landing on both feet. Focus on generating power and maintaining balance throughout each phase of the movement. Repeat for the desired number of repetitions or distance.',
                4
        ),
        (
                'twist mountain climbers',
                'Twist mountain climbers are a core exercise that targets the obliques, rectus abdominis, and hip flexors, while also engaging the shoulders and legs. This exercise helps to build core strength, stability, and cardiovascular endurance.',
                'Start in a plank position with your hands on the floor and your body in a straight line. Drive one knee towards the opposite elbow, twisting your torso as you bring the knee across your body. Alternate legs in a running motion, maintaining a quick pace. Repeat for the desired number of repetitions or time.',
                3
        ),
        (
                'upright row',
                'The upright row is an upper body exercise that targets the shoulders, particularly the deltoids, and the trapezius muscles. This exercise helps to build shoulder strength and improve upper body posture.',
                'Hold a barbell or dumbbells with an overhand grip, hands shoulder-width apart. Stand with your feet hip-width apart, keeping the barbell or dumbbells close to your body as you lift them to shoulder height, leading with your elbows. Lower the weights back down with control. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'v-ups isometric holds',
                'V-up isometric holds are a core stabilization exercise that targets the rectus abdominis and hip flexors, helping to build core strength and endurance.',
                'Lie flat on your back with your legs straight and arms extended overhead. Engage your core and lift your legs and upper body off the ground, reaching your hands towards your toes, forming a V shape with your body. Hold this position for the desired amount of time, maintaining tension in your core.',
                3
        ),
        (
                'v-ups on weight bench',
                'V-ups on a weight bench are a core exercise that targets the rectus abdominis and hip flexors, with the added challenge of performing the movement on an unstable surface.',
                'Sit on the edge of a weight bench with your legs extended and your hands gripping the sides of the bench for support. Lean back slightly and lift your legs towards your chest, bringing your upper body and legs together to form a V shape. Lower yourself back down with control. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'walking lunge',
                'The walking lunge is a lower body exercise that targets the quadriceps, hamstrings, glutes, and calves while also engaging the core for stability. This exercise helps to build leg strength, muscle mass, and balance.',
                'Stand with your feet hip-width apart. Step forward with one foot into a lunge position, lowering your hips until both knees are bent at a 90-degree angle. Push through the front heel to bring your back foot forward into the next lunge. Continue alternating legs as you move forward. Repeat for the desired number of repetitions or distance.',
                3
        ),
        (
                'walking lunge - barbell',
                'The barbell walking lunge is a lower body exercise that targets the quadriceps, hamstrings, glutes, and calves while also engaging the core for stability. This exercise helps to build leg strength, muscle mass, and balance.',
                'Position a barbell on your upper back and shoulders, standing with your feet hip-width apart. Step forward with one foot into a lunge position, lowering your hips until both knees are bent at a 90-degree angle. Push through the front heel to bring your back foot forward into the next lunge. Continue alternating legs as you move forward. Repeat for the desired number of repetitions or distance.',
                4
        ),
        (
                'walking lunges - medicine ball',
                'Walking lunges with a medicine ball are a lower body exercise that targets the quadriceps, hamstrings, glutes, and calves while also engaging the core and upper body. This exercise helps to build leg strength, muscle mass, and balance.',
                'Hold a medicine ball at chest height, standing with your feet hip-width apart. Step forward with one foot into a lunge position, lowering your hips until both knees are bent at a 90-degree angle. Push through the front heel to bring your back foot forward into the next lunge. Continue alternating legs as you move forward, holding the medicine ball steady. Repeat for the desired number of repetitions or distance.',
                3
        ),
        (
                'wall lat stretch',
                'The wall lat stretch is a mobility exercise that targets the latissimus dorsi muscles, helping to improve flexibility and reduce tension in the upper back and shoulders. This stretch is beneficial for improving shoulder mobility and reducing the risk of injury.',
                'Stand facing a wall with your hands placed on the wall at shoulder height. Step back slightly and lean forward, allowing your chest to drop towards the ground while keeping your arms straight. You should feel a stretch along your sides and upper back. Hold for the desired amount of time, breathing deeply and relaxing into the stretch.',
                1
        ),
        (
                'wall slides',
                'Wall slides are a mobility exercise that targets the shoulders and upper back, helping to improve shoulder mobility, flexibility, and range of motion. This exercise is beneficial for warming up and reducing the risk of shoulder injuries.',
                'Stand with your back against a wall, feet slightly away from the wall. Press your lower back, upper back, and head against the wall. Raise your arms to form a W shape with your elbows bent at a 90-degree angle. Slowly slide your arms up the wall into a Y shape, keeping your arms and back in contact with the wall. Lower your arms back down to the W position. Repeat for the desired number of repetitions.',
                2
        ),
        (
                'windshield wipers',
                'Windshield wipers are a core exercise that targets the obliques, rectus abdominis, and hip flexors, while also engaging the lower back. This exercise helps to build core strength, stability, and rotational control.',
                'Lie flat on your back with your arms extended out to the sides for stability. Lift your legs towards the ceiling, keeping them straight. Slowly lower your legs to one side, keeping your shoulders and back pressed into the ground. Raise your legs back to the center, then lower them to the other side. Repeat for the desired number of repetitions.',
                4
        ),
        (
                'y,t,w''s',
                'Y, T, W''s are a series of shoulder and upper back exercises that target the rotator cuff muscles, trapezius, and deltoids. This exercise helps to improve shoulder stability, mobility, and posture.',
                'Lie face down on an incline bench or stability ball with your arms hanging straight down. Perform the following movements in sequence: Raise your arms to form a Y shape, then lower them back down. Raise your arms out to the sides to form a T shape, then lower them. Finally, raise your arms to form a W shape by bending your elbows, then lower them. Repeat the sequence for the desired number of repetitions.',
                3
        ),
        (
                '10 meters back pedal into 10 meters sprint',
                'This drill combines a backpedal with a sprint to improve agility, acceleration, and overall athleticism. It targets the lower body muscles, including the quadriceps, hamstrings, glutes, and calves, while also engaging the core for stability.',
                'Start in an athletic stance with your feet shoulder-width apart. Begin by backpedaling for 10 meters, staying low and maintaining a strong, balanced position. Once you reach the 10-meter mark, quickly transition into a forward sprint for the next 10 meters. Focus on accelerating as fast as possible during the sprint portion. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'calf raises - standing',
                'Standing calf raises are an isolation exercise that targets the calf muscles, specifically the gastrocnemius and soleus. This exercise helps to build calf strength and improve ankle stability.',
                'Stand with your feet hip-width apart, toes pointed forward. Push through the balls of your feet to raise your heels off the ground, lifting your body as high as possible. Hold the top position for a moment, then slowly lower your heels back down. Repeat for the desired number of repetitions.',
                2
        ),
        (
                'side plank on knee',
                'The side plank on knee is a core stabilization exercise that targets the obliques, rectus abdominis, and glutes. This exercise helps to build core strength, stability, and balance.',
                'Lie on your side with your bottom knee bent and your top leg extended straight. Prop yourself up on your forearm, keeping your elbow directly under your shoulder. Lift your hips off the ground, forming a straight line from your head to your top foot. Hold this position for the desired amount of time, then switch sides.',
                2
        ),
        (
                'lizard stretch',
                'The lizard stretch is a deep hip and groin stretch that targets the hip flexors, hamstrings, and groin muscles. This stretch helps to improve hip mobility, flexibility, and range of motion.',
                'Start in a push-up position. Step your right foot outside your right hand, lowering your hips towards the ground. Keep your back leg extended straight behind you and your front foot flat on the ground. Hold this position, breathing deeply and relaxing into the stretch. Switch sides after holding for the desired amount of time.',
                2
        ),
        (
                'side plank',
                'The side plank is a core stabilization exercise that targets the obliques, rectus abdominis, and glutes. This exercise helps to build core strength, stability, and balance.',
                'Lie on your side with your legs extended straight and your feet stacked on top of each other. Prop yourself up on your forearm, keeping your elbow directly under your shoulder. Lift your hips off the ground, forming a straight line from your head to your feet. Hold this position for the desired amount of time, then switch sides.',
                3
        ),
        (
                'apley''s scratch test',
                'Apley''s scratch test is a mobility assessment that evaluates shoulder range of motion and flexibility. This test helps to identify any limitations in shoulder mobility.',
                'Reach one arm over your shoulder to touch your upper back and the other arm behind your back to touch your lower back. Try to touch your fingertips together. Repeat on the other side. This test assesses shoulder mobility by comparing the distance between the fingertips on each side.',
                1
        ),
        (
                'broad jumps',
                'Broad jumps are a plyometric exercise that targets the quadriceps, hamstrings, glutes, and calves while also engaging the core. This exercise helps to build explosive power, leg strength, and coordination.',
                'Stand with your feet shoulder-width apart. Lower into a squat position, then explosively jump forward as far as you can, swinging your arms for momentum. Land softly with your knees slightly bent and immediately go into the next jump. Repeat for the desired number of repetitions or distance.',
                4
        ),
        (
                'dumbbell side lateral',
                'Dumbbell side laterals are an isolation exercise that targets the deltoid muscles, particularly the middle head. This exercise helps to build shoulder strength and improve upper body posture.',
                'Stand with your feet shoulder-width apart, holding a dumbbell in each hand at your sides with your palms facing your body. Lift your arms out to the sides until they are parallel to the floor, keeping a slight bend in your elbows. Lower the dumbbells back down with control. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'ball drops',
                'Ball drops are a coordination and reaction drill that targets the lower body muscles and core while improving hand-eye coordination and reflexes. This exercise helps to develop agility and quickness.',
                'Stand in an athletic stance with a partner or coach holding a tennis ball or similar object. When the ball is dropped, react quickly by sprinting forward to catch it before it bounces a second time. Repeat for the desired number of repetitions.',
                2
        ),
        (
                'lateral lunge',
                'Lateral lunges are a lower body exercise that targets the quadriceps, hamstrings, glutes, and adductors. This exercise helps to build leg strength, improve lateral movement, and increase flexibility.',
                'Stand with your feet shoulder-width apart. Take a large step to the side with one leg, bending that knee and lowering your hips while keeping the other leg straight. Push through the heel of the bent leg to return to the starting position. Repeat for the desired number of repetitions, then switch sides.',
                3
        ),
        (
                'kettlebell swing',
                'The kettlebell swing is a dynamic exercise that targets the posterior chain, including the glutes, hamstrings, and lower back, while also engaging the core and shoulders. This exercise helps to build power, endurance, and functional strength.',
                'Stand with your feet shoulder-width apart, holding a kettlebell with both hands in front of your hips. Hinge at your hips, swinging the kettlebell back between your legs, then explosively thrust your hips forward to swing the kettlebell up to shoulder height. Allow the kettlebell to swing back down between your legs as you prepare for the next repetition. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'front plate raise',
                'The front plate raise is an isolation exercise that targets the deltoid muscles, particularly the anterior head. This exercise helps to build shoulder strength and improve upper body posture.',
                'Stand with your feet shoulder-width apart, holding a weight plate with both hands at your thighs. Keeping your arms straight, raise the plate in front of you until your arms are parallel to the ground. Lower the plate back down with control. Repeat for the desired number of repetitions.',
                2
        ),
        (
                '3 cone drill',
                'The 3 cone drill is an agility and speed drill that targets the lower body muscles and core while improving coordination, quickness, and change of direction. This drill is commonly used in athletic training to enhance overall performance.',
                'Set up three cones in an L-shape, with 5 yards between each cone. Start at the first cone, sprint to the second cone, touch the line, and sprint back to the first cone. Then, sprint around the outside of the second cone to the third cone, circle it, and sprint back through the second cone to finish. Focus on maintaining speed and control throughout the drill.',
                3
        ),
        (
                'cable tricep extension',
                'Cable tricep extensions are an isolation exercise that targets all three heads of the triceps, helping to build upper arm strength and muscle mass.',
                'Attach a straight bar or rope attachment to a high pulley of a cable machine. Stand facing the machine, holding the bar or rope with an overhand grip. Keep your elbows close to your sides as you extend your arms downward, fully contracting your triceps at the bottom of the movement. Slowly return to the starting position. Repeat for the desired number of repetitions.',
                2
        ),
        (
                'high plank with leg raise on bosu ball',
                'This exercise combines a high plank with a leg raise on a Bosu ball, targeting the core, glutes, and shoulders while improving balance and stability.',
                'Start in a high plank position with your hands on the flat side of a Bosu ball and your feet together. Engage your core and lift one leg towards the ceiling while keeping your hips level. Lower the leg back down and repeat on the other side. Continue alternating legs for the desired number of repetitions.',
                4
        ),
        (
                'cable curls - rope',
                'Cable rope curls are an isolation exercise that targets the biceps, helping to build upper arm strength and muscle mass.',
                'Attach a rope handle to a low pulley of a cable machine. Stand facing the machine, holding the rope with an underhand grip. Keep your elbows close to your sides as you curl the rope towards your shoulders, squeezing your biceps at the top. Slowly lower the rope back down to the starting position. Repeat for the desired number of repetitions.',
                3
        ),
        (
                'step ups - dumbbell',
                'Dumbbell step ups are a lower body exercise that targets the quadriceps, hamstrings, and glutes while also engaging the core for stability. This exercise helps to build leg strength, muscle mass, and balance.',
                'Stand in front of a bench or step with a dumbbell in each hand. Step up onto the bench with one foot, driving through the heel to lift your body up. Bring your other foot up to meet the first, then step back down with the same foot. Repeat on the other side, continuing to alternate legs for the desired number of repetitions.',
                3
        ),
        (
                'oblique crunches on hyper extension',
                'Oblique crunches on a hyperextension bench target the obliques, helping to build core strength and improve lateral stability.',
                'Position yourself sideways on a hyperextension bench with your hips supported and your feet secured. Place your hands behind your head. Lower your upper body towards the ground, then engage your obliques to lift your torso back up, twisting slightly at the top. Repeat for the desired number of repetitions, then switch sides.',
                3
        ),
        (
                'lateral line hops',
                'Lateral line hops are a plyometric exercise that targets the lower body muscles, particularly the calves and ankles, while also improving coordination and agility.',
                'Stand with your feet together next to a line or object on the ground. Hop side to side over the line, keeping your feet together and landing softly. Continue hopping back and forth for the desired number of repetitions or time.',
                2
        ),
        (
                'side plank - dumbbell reach',
                'This variation of the side plank adds a dumbbell reach, targeting the obliques, shoulders, and core while also improving stability and coordination.',
                'Start in a side plank position with a dumbbell in your top hand. Reach the dumbbell towards the ceiling, then slowly lower it down in front of you, passing it under your torso. Return to the starting position and repeat for the desired number of repetitions, then switch sides.',
                4
        ),
        (
                'shoulder press - supinated barbell',
                'The supinated barbell shoulder press is an upper body exercise that targets the shoulders, triceps, and upper chest. This exercise helps to build shoulder strength and improve upper body posture.',
                'Hold a barbell with a supinated (underhand) grip, hands shoulder-width apart. Press the barbell overhead until your arms are fully extended, then lower it back down to your shoulders with control. Repeat for the desired number of repetitions.',
                4
        ),
        (
                'high to low cable chops',
                'High to low cable chops are a rotational core exercise that targets the obliques, rectus abdominis, and shoulders. This exercise helps to build core strength and improve rotational power.',
                'Attach a handle to a high pulley of a cable machine. Stand sideways to the machine, holding the handle with both hands. Pull the handle down and across your body towards your opposite hip, rotating your torso as you move. Return to the starting position with control. Repeat for the desired number of repetitions, then switch sides.',
                3
        ),
        (
                'b-skips',
                'B-skips are a dynamic warm-up exercise that targets the hip flexors, quadriceps, and hamstrings while improving coordination and running form.',
                'Start by skipping forward, driving one knee up while extending your other leg forward and downward in a kicking motion. Alternate legs with each skip, focusing on maintaining rhythm and form. Continue for the desired distance or time.',
                2
        ),
        (
                'push ups - clapping',
                'Clapping push-ups are an explosive upper body exercise that targets the chest, triceps, and shoulders while also improving power and coordination.',
                'Start in a push-up position with your hands shoulder-width apart. Lower your body towards the ground, then explosively push yourself up, clapping your hands together before landing back in the push-up position. Repeat for the desired number of repetitions.',
                4
        ),
        (
                'ab roller',
                'The ab roller is a core exercise that targets the rectus abdominis, obliques, and hip flexors, helping to build core strength, stability, and endurance.',
                'Kneel on the ground with the ab roller in front of you. Slowly roll the ab roller forward, extending your body into a straight line. Engage your core and roll back to the starting position. Repeat for the desired number of repetitions.',
                4
        );

insert into exercise_type (exercise_id, training_type_id)
values (1, 8),
  (1, 9),
  (1, 10),
  (2, 1),
  (2, 2),
  (2, 18),
  (3, 1),
  (3, 2),
  (3, 18),
  (4, 1),
  (4, 2),
  (4, 18),
  (5, 1),
  (5, 2),
  (5, 18),
  (6, 1),
  (6, 2),
  (6, 18),
  (7, 1),
  (7, 2),
  (7, 18),
  (8, 1),
  (8, 2),
  (8, 18),
  (9, 1),
  (9, 2),
  (9, 18),
  (10, 1),
  (10, 2),
  (10, 18),
  (11, 2),
  (11, 18),
  (12, 1),
  (12, 2),
  (12, 18),
  (13, 1),
  (13, 2),
  (13, 18),
  (14, 1),
  (14, 2),
  (14, 18),
  (15, 1),
  (15, 2),
  (15, 18),
  (16, 2),
  (16, 18),
  (17, 1),
  (17, 2),
  (17, 18),
  (18, 1),
  (18, 2),
  (18, 18),
  (19, 1),
  (19, 2),
  (19, 18),
  (20, 1),
  (20, 2),
  (20, 18),
  (21, 8),
  (21, 10),
  (21, 18),
  (22, 1),
  (22, 2),
  (22, 18),
  (23, 1),
  (23, 2),
  (23, 18),
  (24, 8),
  (24, 10),
  (24, 18),
  (25, 1),
  (25, 2),
  (25, 18),
  (26, 1),
  (26, 2),
  (27, 1),
  (27, 2),
  (28, 1),
  (28, 2),
  (28, 18),
  (29, 1),
  (29, 2),
  (29, 18),
  (30, 1),
  (30, 2),
  (30, 18),
  (31, 1),
  (31, 2),
  (31, 18),
  (32, 1),
  (32, 2),
  (32, 18),
  (33, 1),
  (33, 2),
  (33, 18),
  (34, 1),
  (34, 2),
  (34, 18),
  (35, 1),
  (35, 2),
  (36, 1),
  (36, 2),
  (37, 1),
  (37, 2),
  (38, 1),
  (38, 2),
  (39, 1),
  (39, 2),
  (40, 1),
  (40, 2),
  (40, 18),
  (41, 1),
  (41, 2),
  (42, 1),
  (42, 2),
  (43, 1),
  (43, 2),
  (44, 1),
  (44, 2),
  (45, 1),
  (45, 2),
  (45, 18),
  (46, 1),
  (46, 2),
  (47, 1),
  (47, 2),
  (48, 1),
  (48, 2),
  (49, 1),
  (49, 2),
  (50, 1),
  (50, 2),
  (50, 18),
  (51, 1),
  (51, 2),
  (52, 1),
  (52, 2),
  (53, 1),
  (53, 2),
  (54, 1),
  (54, 2),
  (55, 1),
  (55, 2),
  (55, 18),
  (56, 9),
  (56, 14),
  (56, 18),
  (57, 1),
  (57, 2),
  (58, 1),
  (58, 2),
  (59, 1),
  (59, 2),
  (60, 1),
  (60, 2),
  (61, 1),
  (61, 2),
  (62, 1),
  (62, 2),
  (63, 1),
  (63, 2),
  (64, 1),
  (64, 2),
  (65, 1),
  (65, 2),
  (66, 1),
  (66, 2),
  (66, 18),
  (67, 1),
  (67, 2),
  (68, 1),
  (68, 2),
  (69, 1),
  (69, 2),
  (70, 1),
  (70, 2),
  (71, 1),
  (71, 2),
  (72, 1),
  (72, 2),
  (73, 1),
  (73, 2),
  (74, 1),
  (74, 2),
  (75, 1),
  (75, 2),
  (76, 1),
  (76, 2),
  (77, 1),
  (77, 2),
  (78, 1),
  (78, 2),
  (78, 18),
  (79, 1),
  (79, 2),
  (80, 1),
  (80, 2),
  (81, 1),
  (81, 2),
  (82, 1),
  (82, 2),
  (83, 1),
  (83, 2),
  (84, 1),
  (84, 2),
  (85, 1),
  (85, 2),
  (86, 1),
  (86, 2),
  (87, 1),
  (87, 2),
  (88, 1),
  (88, 2),
  (89, 1),
  (89, 2),
  (89, 18),
  (90, 1),
  (90, 2),
  (90, 18),
  (91, 12),
  (91, 18),
  (92, 9),
  (92, 18),
  (92, 19),
  (93, 9),
  (93, 18),
  (93, 20),
  (94, 2),
  (94, 18),
  (95, 1),
  (95, 2),
  (96, 12),
  (96, 18),
  (97, 12),
  (97, 18),
  (98, 12),
  (98, 18),
  (99, 1),
  (99, 2),
  (100, 1),
  (100, 2),
  (101, 1),
  (101, 2),
  (102, 1),
  (102, 2),
  (103, 1),
  (103, 2),
  (104, 1),
  (104, 2),
  (105, 1),
  (105, 2),
  (106, 1),
  (106, 2),
  (107, 1),
  (107, 2),
  (108, 1),
  (108, 2),
  (109, 1),
  (109, 2),
  (109, 18),
  (110, 1),
  (110, 2),
  (110, 21),
  (111, 2),
  (111, 9),
  (111, 18),
  (111, 20),
  (112, 1),
  (112, 2),
  (112, 21),
  (113, 2),
  (113, 9),
  (113, 18),
  (113, 20),
  (114, 1),
  (114, 2),
  (114, 18),
  (115, 1),
  (115, 2),
  (115, 18),
  (116, 1),
  (116, 2),
  (117, 1),
  (117, 2),
  (117, 18),
  (118, 1),
  (118, 2),
  (119, 1),
  (119, 2),
  (120, 1),
  (120, 2),
  (121, 2),
  (121, 18),
  (122, 2),
  (122, 11),
  (122, 18),
  (123, 1),
  (123, 2),
  (124, 8),
  (124, 9),
  (124, 18),
  (125, 12),
  (125, 18),
  (126, 12),
  (126, 18),
  (127, 1),
  (127, 2),
  (127, 18),
  (128, 1),
  (128, 2),
  (128, 18),
  (128, 21),
  (129, 1),
  (129, 2),
  (129, 18),
  (130, 1),
  (130, 2),
  (130, 18),
  (130, 21),
  (131, 1),
  (131, 2),
  (131, 18),
  (131, 21),
  (132, 1),
  (132, 2),
  (132, 18),
  (132, 21),
  (133, 1),
  (133, 2),
  (134, 1),
  (134, 2),
  (135, 1),
  (135, 2),
  (136, 1),
  (136, 2),
  (137, 1),
  (137, 2),
  (138, 1),
  (138, 2),
  (139, 1),
  (139, 2),
  (140, 1),
  (140, 2),
  (141, 1),
  (141, 2),
  (142, 1),
  (142, 2),
  (143, 1),
  (143, 2),
  (144, 1),
  (144, 2),
  (144, 18),
  (145, 1),
  (145, 2),
  (146, 1),
  (146, 2),
  (147, 1),
  (147, 2),
  (148, 1),
  (148, 2),
  (149, 1),
  (149, 2),
  (149, 18),
  (150, 1),
  (150, 2),
  (151, 1),
  (151, 2),
  (152, 1),
  (152, 2),
  (153, 1),
  (153, 2),
  (154, 1),
  (154, 2),
  (155, 1),
  (155, 2),
  (155, 18),
  (156, 1),
  (156, 2),
  (157, 1),
  (157, 2),
  (158, 1),
  (158, 2),
  (159, 1),
  (159, 2),
  (159, 18),
  (160, 2),
  (160, 18),
  (161, 2),
  (161, 18),
  (162, 8),
  (162, 9),
  (162, 18),
  (163, 2),
  (163, 18),
  (164, 8),
  (164, 9),
  (164, 18),
  (165, 2),
  (165, 18),
  (166, 12),
  (166, 18),
  (167, 2),
  (167, 18),
  (168, 2),
  (168, 18),
  (169, 1),
  (169, 2),
  (170, 2),
  (170, 18),
  (171, 1),
  (171, 2),
  (172, 1),
  (172, 2),
  (172, 18),
  (173, 12),
  (173, 18),
  (174, 8),
  (174, 9),
  (174, 18),
  (175, 2),
  (175, 18),
  (176, 2),
  (176, 18),
  (177, 2),
  (177, 18),
  (178, 2),
  (178, 18),
  (179, 2),
  (179, 13),
  (179, 18),
  (180, 2),
  (180, 13),
  (180, 18),
  (181, 1),
  (181, 2),
  (182, 2),
  (182, 18),
  (183, 9),
  (183, 13),
  (183, 18),
  (184, 2),
  (184, 18),
  (185, 2),
  (185, 18),
  (186, 1),
  (186, 2),
  (187, 1),
  (187, 2),
  (188, 2),
  (188, 18),
  (189, 2),
  (189, 18),
  (190, 2),
  (190, 18),
  (191, 2),
  (191, 8),
  (191, 18),
  (192, 2),
  (192, 18),
  (193, 2),
  (193, 18),
  (194, 1),
  (194, 2),
  (195, 1),
  (195, 2),
  (196, 1),
  (196, 2),
  (197, 1),
  (197, 2),
  (198, 2),
  (198, 18),
  (199, 2),
  (199, 18),
  (200, 1),
  (200, 2),
  (201, 2),
  (201, 18),
  (202, 2),
  (202, 18),
  (203, 1),
  (203, 2),
  (204, 12),
  (204, 18),
  (205, 2),
  (205, 18),
  (206, 13),
  (206, 18),
  (207, 2),
  (207, 18),
  (208, 9),
  (208, 18),
  (209, 2),
  (209, 18),
  (210, 8),
  (210, 18),
  (211, 1),
  (211, 2),
  (212, 1),
  (212, 2),
  (213, 2),
  (213, 18),
  (214, 1),
  (214, 2),
  (215, 13),
  (215, 18),
  (216, 1),
  (216, 2),
  (217, 1),
  (217, 2),
  (218, 1),
  (218, 2),
  (219, 1),
  (219, 2),
  (220, 2),
  (220, 18),
  (221, 12),
  (221, 18),
  (222, 2),
  (222, 18),
  (223, 2),
  (223, 18),
  (224, 2),
  (224, 18),
  (225, 2),
  (225, 18),
  (226, 2),
  (226, 18),
  (227, 8),
  (227, 13),
  (227, 18),
  (228, 1),
  (228, 2),
  (229, 2),
  (229, 18),
  (230, 1),
  (230, 2),
  (230, 18),
  (231, 1),
  (231, 2),
  (231, 18),
  (232, 1),
  (232, 2),
  (232, 18),
  (233, 2),
  (233, 18),
  (234, 1),
  (234, 2),
  (235, 1),
  (235, 2),
  (235, 18),
  (236, 1),
  (236, 2),
  (236, 18),
  (237, 2),
  (237, 18),
  (238, 1),
  (238, 2),
  (239, 12),
  (239, 18),
  (240, 13),
  (240, 18),
  (241, 1),
  (241, 2),
  (242, 1),
  (242, 2),
  (242, 18),
  (243, 2),
  (243, 18),
  (244, 1),
  (244, 2),
  (244, 18),
  (245, 12),
  (245, 18),
  (246, 2),
  (246, 18),
  (247, 1),
  (247, 2),
  (248, 2),
  (248, 18),
  (249, 2),
  (249, 18),
  (250, 2),
  (250, 18),
  (251, 2),
  (251, 18),
  (252, 2),
  (252, 18),
  (253, 2),
  (253, 18),
  (254, 2),
  (254, 18),
  (255, 2),
  (255, 18),
  (256, 12),
  (256, 18),
  (257, 1),
  (257, 2),
  (258, 1),
  (258, 2),
  (259, 1),
  (259, 2),
  (260, 1),
  (260, 2),
  (261, 1),
  (261, 2),
  (262, 1),
  (262, 2),
  (263, 2),
  (263, 18),
  (264, 2),
  (264, 18),
  (265, 1),
  (265, 2),
  (266, 1),
  (266, 2),
  (267, 1),
  (267, 2),
  (268, 1),
  (268, 2),
  (269, 1),
  (269, 2),
  (270, 1),
  (270, 2),
  (271, 8),
  (271, 13),
  (272, 1),
  (272, 2),
  (272, 18),
  (273, 1),
  (273, 2),
  (273, 18),
  (274, 1),
  (274, 2),
  (274, 18),
  (275, 2),
  (275, 8),
  (275, 13),
  (276, 2),
  (276, 18),
  (277, 2),
  (277, 18),
  (278, 1),
  (278, 2),
  (278, 18),
  (279, 1),
  (279, 2),
  (280, 2),
  (280, 18),
  (281, 1),
  (281, 2),
  (282, 2),
  (282, 18),
  (283, 2),
  (283, 18),
  (284, 2),
  (284, 18),
  (285, 2),
  (285, 18),
  (286, 2),
  (286, 18),
  (287, 1),
  (287, 2),
  (287, 18),
  (288, 12),
  (288, 18),
  (289, 1),
  (289, 2),
  (290, 1),
  (290, 2),
  (290, 18),
  (291, 1),
  (291, 2),
  (292, 1),
  (292, 2),
  (293, 12),
  (293, 18),
  (294, 8),
  (294, 13),
  (294, 18),
  (295, 2),
  (295, 8),
  (295, 18),
  (296, 1),
  (296, 2),
  (297, 2),
  (297, 18),
  (298, 2),
  (298, 18),
  (299, 1),
  (299, 2),
  (299, 18),
  (300, 1),
  (300, 2),
  (300, 18),
  (301, 1),
  (301, 2),
  (301, 18),
  (302, 12),
  (302, 18),
  (303, 12),
  (303, 18),
  (304, 2),
  (304, 18),
  (305, 2),
  (305, 12),
  (305, 18),
  (306, 8),
  (306, 13),
  (306, 18),
  (307, 1),
  (307, 2),
  (308, 2),
  (308, 18),
  (309, 12),
  (309, 18),
  (310, 2),
  (310, 18),
  (311, 12),
  (312, 8),
  (312, 13),
  (312, 18),
  (313, 1),
  (313, 2),
  (314, 8),
  (314, 18),
  (315, 2),
  (315, 18),
  (316, 8),
  (316, 13),
  (316, 18),
  (317, 1),
  (317, 2),
  (318, 8),
  (318, 18),
  (319, 1),
  (319, 2),
  (320, 2),
  (320, 18),
  (321, 1),
  (321, 2),
  (322, 1),
  (322, 2),
  (322, 18),
  (323, 2),
  (323, 18),
  (324, 8),
  (324, 18),
  (325, 2),
  (325, 18),
  (326, 1),
  (326, 2),
  (327, 2),
  (327, 18),
  (328, 8),
  (328, 18),
  (329, 2),
  (329, 13),
  (330, 2),
  (330, 18);

insert into exercise_muscle (is_primary, muscle_id, exercise_id)
values (0, 1, 6),
  (0, 1, 7),
  (1, 1, 30),
  (1, 1, 44),
  (0, 1, 46),
  (1, 2, 4),
  (0, 2, 30),
  (1, 2, 32),
  (1, 2, 33),
  (1, 2, 37),
  (0, 2, 44),
  (1, 2, 46),
  (0, 3, 4),
  (0, 3, 18),
  (0, 3, 31),
  (0, 3, 32),
  (0, 3, 33),
  (1, 3, 37),
  (1, 4, 4),
  (0, 4, 16),
  (1, 4, 18),
  (1, 4, 31),
  (1, 4, 32),
  (1, 4, 33),
  (1, 4, 37),
  (1, 4, 38),
  (0, 4, 46),
  (1, 5, 4),
  (0, 5, 8),
  (0, 5, 18),
  (0, 5, 31),
  (1, 5, 32),
  (1, 5, 33),
  (1, 5, 37),
  (1, 5, 46),
  (0, 6, 2),
  (0, 6, 3),
  (0, 6, 4),
  (1, 6, 17),
  (0, 6, 23),
  (0, 6, 32),
  (0, 6, 33),
  (1, 6, 45),
  (0, 7, 8),
  (0, 8, 18),
  (0, 8, 31),
  (0, 8, 32),
  (0, 8, 33),
  (0, 9, 8),
  (1, 17, 5),
  (1, 17, 9),
  (1, 17, 19),
  (0, 17, 27),
  (1, 17, 28),
  (0, 17, 29),
  (1, 17, 38),
  (1, 17, 39),
  (1, 17, 48),
  (1, 17, 49),
  (1, 18, 5),
  (1, 18, 9),
  (1, 18, 19),
  (0, 18, 27),
  (0, 18, 28),
  (1, 18, 29),
  (0, 19, 19),
  (1, 20, 5),
  (1, 20, 6),
  (1, 20, 7),
  (1, 20, 9),
  (0, 20, 19),
  (0, 20, 27),
  (0, 20, 28),
  (0, 20, 29),
  (1, 20, 39),
  (1, 20, 48),
  (1, 20, 49),
  (0, 21, 6),
  (0, 21, 7),
  (1, 21, 8),
  (0, 21, 37),
  (1, 21, 46),
  (1, 22, 6),
  (1, 22, 7),
  (0, 23, 4),
  (0, 23, 18),
  (0, 23, 31),
  (0, 23, 32),
  (0, 23, 33),
  (0, 23, 36),
  (0, 23, 42),
  (0, 24, 4),
  (0, 24, 18),
  (0, 24, 31),
  (0, 24, 32),
  (0, 24, 33),
  (1, 24, 36),
  (1, 24, 42),
  (0, 25, 4),
  (0, 25, 5),
  (0, 25, 18),
  (0, 25, 31),
  (0, 25, 32),
  (0, 25, 33),
  (0, 25, 36),
  (0, 25, 42),
  (1, 26, 5),
  (0, 26, 6),
  (1, 26, 7),
  (1, 26, 9),
  (1, 26, 12),
  (1, 26, 26),
  (1, 26, 27),
  (0, 26, 28),
  (0, 26, 29),
  (0, 26, 39),
  (1, 26, 47),
  (0, 26, 48),
  (0, 26, 49),
  (1, 27, 5),
  (0, 27, 6),
  (1, 27, 7),
  (0, 27, 9),
  (1, 27, 12),
  (1, 27, 26),
  (1, 27, 27),
  (0, 27, 28),
  (0, 27, 29),
  (0, 27, 39),
  (0, 27, 47),
  (0, 27, 48),
  (0, 27, 49),
  (1, 28, 5),
  (0, 28, 6),
  (1, 28, 7),
  (0, 28, 9),
  (1, 28, 12),
  (1, 28, 26),
  (1, 28, 27),
  (0, 28, 28),
  (0, 28, 29),
  (0, 28, 39),
  (0, 28, 47),
  (0, 28, 48),
  (0, 28, 49),
  (0, 29, 4),
  (0, 29, 5),
  (0, 29, 32),
  (0, 29, 33),
  (0, 35, 1),
  (0, 35, 2),
  (0, 35, 3),
  (1, 35, 13),
  (0, 35, 17),
  (1, 35, 21),
  (1, 35, 23),
  (1, 35, 24),
  (0, 35, 25),
  (1, 35, 34),
  (0, 35, 35),
  (0, 35, 40),
  (0, 35, 41),
  (1, 35, 45),
  (0, 36, 1),
  (0, 36, 2),
  (0, 36, 3),
  (1, 36, 13),
  (0, 36, 17),
  (1, 36, 21),
  (1, 36, 23),
  (1, 36, 24),
  (0, 36, 25),
  (1, 36, 34),
  (0, 36, 40),
  (0, 36, 41),
  (0, 36, 45),
  (0, 37, 1),
  (0, 37, 2),
  (0, 37, 3),
  (1, 37, 13),
  (0, 37, 17),
  (1, 37, 21),
  (1, 37, 23),
  (1, 37, 24),
  (0, 37, 25),
  (1, 37, 34),
  (0, 37, 40),
  (0, 37, 41),
  (0, 37, 45),
  (0, 38, 1),
  (1, 38, 2),
  (1, 38, 3),
  (1, 38, 14),
  (1, 38, 21),
  (1, 38, 24),
  (1, 38, 34),
  (0, 38, 35),
  (1, 38, 40),
  (1, 38, 41),
  (0, 39, 1),
  (1, 39, 2),
  (1, 39, 3),
  (1, 39, 14),
  (1, 39, 21),
  (1, 39, 24),
  (1, 39, 34),
  (0, 39, 35),
  (1, 39, 40),
  (1, 39, 41),
  (0, 40, 1),
  (1, 40, 2),
  (1, 40, 3),
  (1, 40, 14),
  (1, 40, 21),
  (1, 40, 24),
  (1, 40, 34),
  (0, 40, 35),
  (1, 40, 40),
  (1, 40, 41),
  (0, 41, 1),
  (1, 41, 2),
  (1, 41, 3),
  (1, 41, 14),
  (1, 41, 21),
  (1, 41, 24),
  (1, 41, 34),
  (0, 42, 1),
  (1, 42, 2),
  (1, 42, 3),
  (0, 42, 16),
  (1, 42, 17),
  (1, 42, 21),
  (1, 42, 23),
  (1, 42, 24),
  (1, 42, 25),
  (1, 42, 34),
  (1, 42, 35),
  (1, 42, 40),
  (1, 42, 41),
  (1, 42, 45),
  (0, 43, 2),
  (0, 43, 3),
  (1, 43, 20),
  (0, 43, 25),
  (0, 44, 2),
  (0, 44, 3),
  (1, 44, 20),
  (0, 44, 25),
  (0, 45, 1),
  (0, 45, 2),
  (0, 45, 3),
  (0, 45, 16),
  (1, 45, 21),
  (1, 45, 22),
  (1, 45, 24),
  (1, 45, 50),
  (0, 46, 1),
  (0, 46, 2),
  (0, 46, 3),
  (1, 46, 16),
  (1, 46, 21),
  (0, 46, 22),
  (1, 46, 24),
  (0, 46, 50),
  (0, 47, 1),
  (0, 47, 2),
  (0, 47, 3),
  (1, 47, 16),
  (1, 47, 21),
  (0, 47, 22),
  (1, 47, 24),
  (0, 47, 50),
  (0, 49, 1),
  (0, 49, 2),
  (0, 49, 3),
  (0, 49, 16),
  (1, 49, 21),
  (0, 49, 22),
  (1, 49, 24),
  (0, 49, 50),
  (1, 50, 1),
  (1, 50, 10),
  (0, 50, 13),
  (1, 50, 21),
  (1, 50, 24),
  (1, 50, 43),
  (1, 51, 1),
  (1, 51, 10),
  (1, 51, 21),
  (1, 51, 24),
  (0, 51, 43),
  (0, 52, 10),
  (1, 52, 11),
  (0, 53, 11),
  (0, 54, 11),
  (1, 55, 15),
  (0, 55, 16),
  (0, 55, 34),
  (1, 56, 15),
  (0, 56, 34),
  (1, 57, 15),
  (0, 57, 34),
  (0, 59, 15),
  (0, 61, 15),
  (0, 63, 20),
  (0, 64, 38),
  (0, 65, 30),
  (1, 38, 51),
  (1, 39, 51),
  (1, 40, 51),
  (1, 41, 51),
  (0, 48, 51),
  (1, 38, 52),
  (1, 39, 52),
  (1, 40, 52),
  (1, 41, 52),
  (0, 48, 52),
  (0, 6, 53),
  (1, 35, 53),
  (1, 36, 53),
  (1, 37, 53),
  (0, 38, 53),
  (0, 39, 53),
  (1, 42, 53),
  (0, 57, 53),
  (0, 35, 54),
  (0, 36, 54),
  (0, 37, 54),
  (1, 38, 54),
  (1, 39, 54),
  (1, 40, 54),
  (0, 42, 54),
  (1, 45, 55),
  (0, 46, 55),
  (0, 47, 55),
  (0, 48, 55),
  (0, 49, 55),
  (0, 62, 55),
  (0, 35, 56),
  (0, 36, 56),
  (0, 37, 56),
  (1, 38, 56),
  (1, 39, 56),
  (1, 40, 56),
  (1, 42, 56),
  (0, 50, 56),
  (0, 51, 56),
  (1, 38, 57),
  (1, 39, 57),
  (1, 40, 57),
  (1, 41, 57),
  (1, 26, 58),
  (0, 27, 58),
  (0, 28, 58),
  (1, 2, 59),
  (1, 3, 59),
  (1, 4, 59),
  (1, 5, 59),
  (0, 8, 59),
  (0, 23, 59),
  (0, 23, 60),
  (0, 24, 60),
  (1, 25, 60),
  (1, 29, 60),
  (0, 18, 61),
  (0, 20, 61),
  (1, 26, 61),
  (1, 27, 61),
  (1, 28, 61),
  (1, 23, 62),
  (1, 24, 62),
  (0, 25, 62),
  (1, 17, 63),
  (0, 20, 63),
  (1, 26, 63),
  (1, 27, 63),
  (1, 28, 63),
  (0, 23, 64),
  (0, 24, 64),
  (1, 25, 64),
  (1, 29, 64),
  (1, 26, 65),
  (0, 27, 65),
  (0, 28, 65),
  (1, 45, 66),
  (0, 46, 66),
  (0, 47, 66),
  (0, 48, 66),
  (0, 62, 66),
  (1, 4, 67),
  (1, 17, 67),
  (0, 64, 67),
  (1, 2, 68),
  (1, 4, 68),
  (1, 5, 68),
  (0, 8, 68),
  (0, 21, 68),
  (0, 20, 69),
  (1, 22, 69),
  (0, 66, 69),
  (1, 2, 70),
  (1, 5, 70),
  (0, 7, 70),
  (1, 21, 70),
  (1, 2, 71),
  (1, 5, 71),
  (0, 7, 71),
  (1, 21, 71),
  (0, 20, 72),
  (1, 22, 72),
  (1, 20, 73),
  (1, 22, 73),
  (0, 26, 73),
  (0, 27, 73),
  (0, 28, 73),
  (1, 23, 74),
  (1, 24, 74),
  (0, 25, 74),
  (1, 26, 75),
  (0, 27, 75),
  (0, 28, 75),
  (1, 4, 76),
  (1, 17, 76),
  (0, 64, 76),
  (1, 1, 77),
  (0, 2, 77),
  (1, 45, 78),
  (0, 46, 78),
  (0, 47, 78),
  (0, 48, 78),
  (0, 62, 78),
  (1, 50, 79),
  (0, 51, 79),
  (1, 50, 80),
  (0, 51, 80),
  (0, 20, 81),
  (1, 22, 81),
  (1, 1, 82),
  (0, 2, 82),
  (0, 20, 82),
  (1, 22, 82),
  (1, 20, 83),
  (1, 22, 83),
  (0, 26, 83),
  (0, 27, 83),
  (0, 28, 83),
  (1, 2, 84),
  (1, 5, 84),
  (0, 7, 84),
  (1, 21, 84),
  (1, 23, 85),
  (0, 24, 85),
  (0, 25, 85),
  (1, 23, 86),
  (1, 24, 86),
  (0, 25, 86),
  (0, 29, 86),
  (0, 7, 87),
  (0, 9, 87),
  (1, 21, 87),
  (1, 22, 87),
  (1, 20, 88),
  (1, 22, 88),
  (0, 26, 88),
  (0, 27, 88),
  (0, 17, 89),
  (1, 20, 89),
  (1, 22, 89),
  (0, 29, 89),
  (1, 20, 90),
  (1, 22, 90),
  (0, 29, 90),
  (1, 42, 91),
  (1, 43, 91),
  (1, 48, 91),
  (1, 62, 91),
  (1, 38, 92),
  (1, 48, 92),
  (1, 50, 92),
  (0, 51, 92),
  (1, 62, 92),
  (1, 35, 93),
  (1, 38, 93),
  (1, 39, 93),
  (1, 40, 93),
  (1, 42, 93),
  (0, 45, 93),
  (1, 50, 93),
  (0, 51, 93),
  (1, 45, 94),
  (1, 46, 94),
  (0, 47, 94),
  (0, 48, 94),
  (0, 49, 94),
  (0, 62, 94),
  (1, 20, 95),
  (1, 22, 95),
  (0, 26, 95),
  (0, 27, 95),
  (1, 1, 96),
  (1, 2, 96),
  (0, 5, 96),
  (0, 21, 96),
  (1, 4, 97),
  (0, 8, 97),
  (0, 9, 97),
  (0, 17, 98),
  (0, 18, 98),
  (1, 19, 98),
  (1, 23, 99),
  (0, 24, 99),
  (0, 25, 99),
  (0, 29, 99),
  (0, 35, 100),
  (1, 38, 100),
  (1, 39, 100),
  (1, 40, 100),
  (0, 42, 100),
  (0, 6, 101),
  (1, 35, 101),
  (1, 36, 101),
  (1, 37, 101),
  (0, 42, 101),
  (1, 17, 102),
  (1, 18, 102),
  (0, 20, 102),
  (0, 26, 102),
  (0, 17, 103),
  (1, 18, 103),
  (0, 20, 103),
  (0, 26, 103),
  (1, 17, 104),
  (1, 18, 104),
  (0, 20, 104),
  (0, 26, 104),
  (1, 17, 105),
  (0, 18, 105),
  (0, 20, 105),
  (0, 26, 105),
  (1, 17, 106),
  (1, 18, 106),
  (0, 20, 106),
  (0, 26, 106),
  (1, 2, 107),
  (0, 3, 107),
  (1, 4, 107),
  (1, 5, 107),
  (0, 8, 107),
  (0, 23, 107),
  (1, 2, 108),
  (1, 4, 108),
  (1, 5, 108),
  (0, 6, 108),
  (0, 8, 108),
  (0, 23, 108),
  (1, 45, 109),
  (1, 46, 109),
  (0, 47, 109),
  (0, 48, 109),
  (0, 62, 109),
  (0, 17, 110),
  (1, 18, 110),
  (0, 20, 110),
  (1, 26, 110),
  (0, 35, 111),
  (0, 36, 111),
  (0, 37, 111),
  (1, 38, 111),
  (1, 39, 111),
  (1, 40, 111),
  (1, 42, 111),
  (1, 50, 111),
  (0, 51, 111),
  (0, 35, 112),
  (0, 36, 112),
  (0, 37, 112),
  (1, 38, 112),
  (1, 39, 112),
  (1, 40, 112),
  (1, 42, 112),
  (1, 35, 113),
  (1, 36, 113),
  (1, 37, 113),
  (1, 38, 113),
  (1, 39, 113),
  (1, 40, 113),
  (1, 42, 113),
  (1, 50, 113),
  (0, 51, 113),
  (0, 35, 114),
  (0, 36, 114),
  (0, 37, 114),
  (1, 38, 114),
  (1, 39, 114),
  (1, 40, 114),
  (1, 42, 114),
  (1, 45, 115),
  (1, 46, 115),
  (0, 47, 115),
  (1, 1, 120),
  (1, 2, 117),
  (0, 2, 120),
  (1, 5, 117),
  (1, 17, 118),
  (1, 18, 119),
  (0, 20, 118),
  (0, 20, 119),
  (0, 20, 121),
  (1, 21, 117),
  (1, 22, 120),
  (1, 23, 116),
  (0, 24, 116),
  (0, 25, 116),
  (0, 45, 121),
  (1, 46, 121),
  (1, 47, 121),
  (1, 4, 123),
  (0, 5, 123),
  (1, 7, 122),
  (0, 8, 123),
  (1, 9, 122),
  (0, 21, 122),
  (0, 23, 123),
  (1, 24, 123),
  (1, 38, 124),
  (1, 38, 125),
  (1, 38, 126),
  (0, 39, 125),
  (0, 39, 126),
  (0, 40, 125),
  (0, 40, 126),
  (1, 42, 124),
  (1, 45, 127),
  (1, 46, 127),
  (0, 47, 127),
  (1, 48, 125),
  (1, 48, 126),
  (1, 50, 124),
  (0, 51, 124),
  (1, 62, 125),
  (1, 62, 126),
  (0, 6, 128),
  (0, 6, 129),
  (0, 6, 130),
  (0, 6, 131),
  (0, 6, 132),
  (0, 17, 134),
  (1, 18, 133),
  (1, 18, 134),
  (0, 20, 133),
  (0, 20, 134),
  (1, 23, 135),
  (1, 23, 136),
  (1, 23, 137),
  (1, 24, 135),
  (1, 24, 136),
  (1, 24, 137),
  (0, 25, 135),
  (0, 25, 136),
  (0, 25, 137),
  (0, 26, 134),
  (1, 35, 128),
  (1, 35, 129),
  (1, 35, 130),
  (1, 35, 131),
  (1, 35, 132),
  (1, 36, 128),
  (1, 36, 129),
  (1, 36, 130),
  (1, 36, 131),
  (1, 36, 132),
  (1, 37, 128),
  (1, 37, 129),
  (1, 37, 130),
  (1, 37, 131),
  (1, 37, 132),
  (0, 38, 128),
  (0, 38, 129),
  (0, 38, 130),
  (0, 38, 131),
  (0, 38, 132),
  (0, 39, 128),
  (0, 39, 129),
  (0, 39, 130),
  (0, 39, 131),
  (0, 39, 132),
  (0, 40, 128),
  (0, 40, 129),
  (0, 40, 130),
  (0, 40, 131),
  (0, 40, 132),
  (1, 42, 128),
  (1, 42, 129),
  (1, 42, 130),
  (1, 42, 131),
  (1, 42, 132),
  (0, 45, 128),
  (0, 45, 129),
  (0, 45, 130),
  (0, 45, 131),
  (0, 45, 132),
  (1, 2, 145),
  (1, 2, 146),
  (1, 4, 145),
  (1, 4, 146),
  (1, 5, 145),
  (1, 5, 146),
  (0, 6, 145),
  (0, 6, 146),
  (0, 8, 145),
  (0, 8, 146),
  (0, 17, 138),
  (1, 17, 139),
  (0, 17, 140),
  (0, 17, 143),
  (1, 18, 138),
  (0, 18, 139),
  (1, 18, 143),
  (0, 20, 138),
  (0, 20, 139),
  (1, 20, 140),
  (1, 20, 142),
  (0, 20, 143),
  (1, 20, 144),
  (1, 20, 147),
  (1, 20, 148),
  (0, 20, 150),
  (0, 20, 151),
  (0, 21, 150),
  (0, 21, 151),
  (1, 22, 142),
  (1, 22, 144),
  (1, 22, 147),
  (1, 22, 148),
  (1, 22, 150),
  (1, 22, 151),
  (0, 23, 141),
  (0, 23, 145),
  (0, 23, 146),
  (0, 24, 141),
  (1, 25, 141),
  (0, 26, 142),
  (0, 26, 143),
  (0, 26, 144),
  (0, 26, 147),
  (0, 26, 148),
  (1, 29, 141),
  (0, 45, 144),
  (1, 46, 149),
  (1, 47, 149),
  (1, 1, 153),
  (0, 2, 153),
  (1, 2, 159),
  (1, 5, 159),
  (0, 20, 154),
  (1, 21, 154),
  (1, 21, 159),
  (1, 22, 153),
  (1, 22, 154),
  (1, 23, 156),
  (1, 23, 158),
  (1, 24, 156),
  (0, 25, 156),
  (0, 25, 158),
  (1, 26, 152),
  (1, 26, 157),
  (0, 27, 152),
  (1, 27, 157),
  (0, 28, 152),
  (1, 28, 157),
  (1, 29, 158),
  (0, 35, 155),
  (0, 36, 155),
  (0, 37, 155),
  (1, 38, 155),
  (1, 39, 155),
  (1, 40, 155),
  (1, 42, 155),
  (0, 1, 160),
  (1, 1, 161),
  (0, 2, 160),
  (1, 2, 161),
  (0, 20, 165),
  (0, 29, 160),
  (1, 29, 161),
  (1, 38, 162),
  (1, 38, 164),
  (0, 38, 165),
  (1, 42, 162),
  (0, 42, 163),
  (1, 42, 164),
  (1, 43, 163),
  (0, 44, 163),
  (1, 45, 160),
  (1, 45, 161),
  (0, 45, 162),
  (0, 45, 163),
  (0, 45, 164),
  (1, 45, 165),
  (1, 45, 167),
  (1, 46, 160),
  (1, 46, 161),
  (0, 46, 165),
  (0, 46, 167),
  (1, 47, 160),
  (1, 47, 161),
  (0, 47, 167),
  (0, 48, 166),
  (0, 48, 167),
  (1, 49, 165),
  (1, 50, 162),
  (1, 50, 164),
  (0, 51, 162),
  (0, 51, 164),
  (1, 55, 166),
  (1, 57, 166),
  (0, 62, 166),
  (0, 62, 167),
  (0, 6, 171),
  (0, 6, 172),
  (0, 35, 168),
  (0, 35, 169),
  (0, 35, 170),
  (1, 35, 171),
  (1, 35, 172),
  (1, 35, 173),
  (0, 36, 168),
  (0, 36, 169),
  (0, 36, 170),
  (1, 36, 171),
  (1, 36, 172),
  (1, 36, 173),
  (0, 37, 168),
  (0, 37, 169),
  (0, 37, 170),
  (1, 37, 171),
  (1, 37, 172),
  (1, 37, 173),
  (1, 38, 174),
  (1, 42, 168),
  (1, 42, 169),
  (1, 42, 170),
  (0, 42, 171),
  (0, 42, 172),
  (0, 42, 173),
  (1, 42, 174),
  (0, 43, 168),
  (0, 43, 169),
  (0, 43, 170),
  (0, 45, 168),
  (0, 45, 169),
  (0, 45, 170),
  (0, 45, 171),
  (0, 45, 172),
  (0, 45, 174),
  (1, 48, 173),
  (1, 50, 174),
  (0, 51, 174),
  (0, 55, 173),
  (1, 62, 173),
  (0, 6, 184),
  (1, 6, 185),
  (1, 17, 181),
  (1, 17, 186),
  (0, 20, 180),
  (0, 20, 181),
  (0, 20, 186),
  (1, 23, 187),
  (1, 24, 187),
  (0, 25, 187),
  (0, 26, 181),
  (0, 35, 179),
  (1, 35, 180),
  (0, 35, 182),
  (1, 35, 183),
  (0, 35, 185),
  (0, 36, 179),
  (1, 36, 180),
  (0, 36, 182),
  (1, 36, 183),
  (0, 36, 185),
  (0, 37, 179),
  (1, 37, 180),
  (0, 37, 182),
  (1, 37, 183),
  (0, 37, 185),
  (0, 38, 176),
  (1, 38, 179),
  (1, 38, 180),
  (1, 38, 183),
  (0, 39, 179),
  (0, 40, 179),
  (1, 42, 179),
  (1, 42, 180),
  (1, 42, 182),
  (1, 42, 183),
  (0, 42, 185),
  (0, 43, 182),
  (1, 45, 175),
  (1, 45, 176),
  (1, 45, 177),
  (0, 45, 178),
  (0, 45, 179),
  (0, 45, 180),
  (0, 45, 182),
  (1, 45, 184),
  (1, 46, 178),
  (1, 47, 178),
  (1, 48, 175),
  (1, 48, 176),
  (1, 48, 177),
  (0, 48, 178),
  (1, 48, 184),
  (1, 50, 183),
  (0, 51, 183),
  (0, 62, 175),
  (0, 62, 176),
  (0, 62, 177),
  (0, 62, 178),
  (0, 62, 184),
  (1, 2, 188),
  (0, 2, 194),
  (0, 2, 195),
  (1, 4, 188),
  (1, 4, 194),
  (1, 4, 195),
  (1, 5, 188),
  (0, 5, 194),
  (0, 5, 195),
  (0, 6, 188),
  (0, 17, 192),
  (0, 17, 193),
  (1, 17, 200),
  (0, 20, 191),
  (1, 20, 192),
  (1, 20, 193),
  (0, 20, 200),
  (0, 23, 188),
  (0, 23, 194),
  (0, 23, 195),
  (0, 26, 192),
  (0, 26, 193),
  (0, 26, 200),
  (0, 35, 190),
  (1, 35, 191),
  (0, 35, 196),
  (0, 36, 190),
  (1, 36, 191),
  (0, 36, 196),
  (0, 37, 190),
  (1, 37, 191),
  (0, 37, 196),
  (1, 38, 196),
  (1, 39, 196),
  (1, 40, 196),
  (0, 42, 189),
  (1, 42, 190),
  (1, 42, 191),
  (0, 42, 196),
  (1, 43, 189),
  (0, 43, 190),
  (0, 44, 189),
  (0, 45, 189),
  (0, 45, 191),
  (0, 45, 192),
  (0, 45, 193),
  (1, 45, 198),
  (1, 45, 199),
  (1, 45, 201),
  (1, 48, 198),
  (1, 48, 199),
  (0, 48, 201),
  (1, 49, 201),
  (1, 50, 197),
  (1, 51, 197),
  (0, 62, 198),
  (0, 62, 199),
  (0, 62, 201),
  (1, 6, 204),
  (1, 17, 203),
  (1, 17, 205),
  (1, 17, 206),
  (1, 18, 203),
  (1, 18, 205),
  (1, 18, 206),
  (0, 19, 203),
  (0, 20, 205),
  (0, 20, 206),
  (0, 20, 210),
  (0, 26, 205),
  (0, 26, 206),
  (1, 35, 208),
  (1, 36, 208),
  (1, 37, 208),
  (1, 38, 208),
  (0, 38, 210),
  (1, 42, 208),
  (1, 43, 202),
  (1, 43, 209),
  (1, 44, 202),
  (0, 44, 209),
  (1, 45, 207),
  (1, 45, 210),
  (1, 48, 207),
  (1, 48, 210),
  (0, 50, 208),
  (0, 62, 207),
  (0, 62, 210),
  (1, 63, 202),
  (0, 63, 209),
  (1, 2, 211),
  (1, 2, 212),
  (1, 4, 211),
  (1, 4, 212),
  (1, 5, 211),
  (1, 5, 212),
  (0, 6, 211),
  (0, 6, 212),
  (0, 6, 215),
  (0, 23, 211),
  (0, 23, 212),
  (0, 23, 214),
  (1, 24, 214),
  (0, 25, 214),
  (1, 26, 216),
  (1, 26, 217),
  (1, 26, 218),
  (1, 26, 219),
  (0, 27, 216),
  (0, 27, 217),
  (0, 27, 218),
  (0, 27, 219),
  (0, 28, 216),
  (0, 28, 217),
  (0, 28, 218),
  (0, 28, 219),
  (0, 29, 214),
  (1, 35, 215),
  (1, 36, 215),
  (1, 37, 215),
  (1, 42, 215),
  (1, 45, 213),
  (1, 45, 220),
  (0, 46, 213),
  (0, 46, 220),
  (0, 47, 213),
  (0, 47, 220),
  (0, 48, 213),
  (0, 49, 220),
  (0, 62, 213),
  (1, 2, 229),
  (0, 3, 229),
  (1, 5, 229),
  (1, 6, 229),
  (1, 7, 226),
  (0, 7, 229),
  (1, 9, 226),
  (0, 20, 222),
  (0, 20, 223),
  (0, 20, 224),
  (0, 20, 225),
  (0, 21, 226),
  (1, 23, 228),
  (1, 24, 228),
  (1, 25, 228),
  (0, 29, 228),
  (1, 38, 227),
  (0, 39, 227),
  (0, 40, 227),
  (1, 42, 221),
  (0, 42, 222),
  (0, 42, 223),
  (0, 42, 224),
  (0, 42, 225),
  (1, 42, 227),
  (0, 43, 221),
  (1, 45, 222),
  (1, 45, 223),
  (1, 45, 224),
  (1, 45, 225),
  (0, 45, 227),
  (0, 46, 222),
  (0, 46, 223),
  (0, 46, 224),
  (0, 46, 225),
  (0, 47, 222),
  (0, 47, 223),
  (0, 47, 224),
  (0, 47, 225),
  (0, 48, 221),
  (1, 49, 222),
  (1, 49, 223),
  (1, 49, 224),
  (1, 49, 225),
  (0, 50, 227),
  (0, 51, 227),
  (0, 62, 221),
  (1, 2, 230),
  (1, 2, 231),
  (1, 2, 232),
  (1, 2, 233),
  (1, 4, 230),
  (1, 4, 231),
  (1, 4, 232),
  (1, 4, 233),
  (1, 4, 234),
  (1, 5, 230),
  (1, 5, 231),
  (1, 5, 232),
  (1, 5, 233),
  (0, 6, 230),
  (0, 6, 231),
  (0, 6, 232),
  (0, 6, 233),
  (1, 10, 239),
  (1, 17, 234),
  (1, 17, 235),
  (1, 17, 236),
  (1, 17, 237),
  (1, 17, 238),
  (0, 18, 234),
  (1, 18, 235),
  (1, 18, 236),
  (1, 18, 237),
  (1, 18, 238),
  (1, 20, 235),
  (0, 20, 236),
  (0, 20, 237),
  (1, 20, 238),
  (0, 21, 230),
  (0, 21, 231),
  (0, 21, 232),
  (0, 21, 233),
  (0, 23, 230),
  (0, 23, 231),
  (0, 23, 232),
  (0, 23, 233),
  (0, 26, 234),
  (0, 26, 235),
  (1, 26, 236),
  (0, 26, 237),
  (0, 26, 238),
  (0, 29, 233),
  (1, 38, 240),
  (1, 39, 240),
  (1, 40, 240),
  (0, 42, 235),
  (0, 42, 236),
  (0, 42, 238),
  (0, 42, 240),
  (0, 45, 235),
  (0, 45, 236),
  (1, 45, 237),
  (0, 45, 238),
  (1, 45, 239),
  (0, 45, 240),
  (0, 46, 239),
  (0, 47, 239),
  (1, 49, 237),
  (0, 50, 240),
  (0, 51, 240),
  (0, 64, 234),
  (1, 2, 241),
  (1, 2, 246),
  (1, 5, 241),
  (1, 5, 246),
  (0, 6, 245),
  (0, 7, 241),
  (0, 7, 246),
  (1, 21, 241),
  (1, 21, 246),
  (1, 26, 247),
  (1, 27, 247),
  (0, 28, 247),
  (1, 35, 242),
  (1, 35, 244),
  (1, 35, 245),
  (1, 36, 245),
  (1, 37, 245),
  (1, 38, 242),
  (1, 38, 244),
  (1, 39, 242),
  (1, 39, 244),
  (1, 40, 242),
  (1, 40, 244),
  (1, 42, 242),
  (1, 42, 244),
  (0, 42, 245),
  (0, 45, 242),
  (1, 45, 243),
  (0, 45, 244),
  (0, 45, 248),
  (0, 45, 249),
  (0, 45, 250),
  (0, 45, 251),
  (0, 45, 252),
  (1, 46, 248),
  (1, 46, 249),
  (1, 46, 250),
  (1, 46, 251),
  (1, 46, 252),
  (1, 47, 248),
  (1, 47, 249),
  (1, 47, 250),
  (1, 47, 251),
  (1, 47, 252),
  (0, 48, 243),
  (1, 49, 243),
  (0, 49, 248),
  (0, 49, 249),
  (0, 49, 250),
  (0, 49, 251),
  (0, 49, 252),
  (0, 62, 243),
  (1, 1, 259),
  (1, 1, 260),
  (1, 2, 257),
  (1, 2, 258),
  (0, 2, 259),
  (0, 2, 260),
  (1, 4, 257),
  (1, 4, 258),
  (1, 5, 257),
  (1, 5, 258),
  (0, 6, 257),
  (0, 6, 258),
  (1, 7, 253),
  (1, 9, 253),
  (1, 17, 255),
  (1, 17, 261),
  (1, 17, 262),
  (1, 18, 261),
  (1, 18, 262),
  (0, 19, 261),
  (0, 19, 262),
  (0, 20, 254),
  (0, 20, 255),
  (0, 20, 261),
  (0, 20, 262),
  (1, 21, 253),
  (0, 23, 257),
  (0, 23, 258),
  (1, 35, 263),
  (1, 36, 263),
  (1, 37, 263),
  (1, 42, 263),
  (1, 45, 254),
  (1, 45, 255),
  (0, 45, 263),
  (1, 48, 254),
  (1, 49, 255),
  (1, 55, 256),
  (1, 56, 256),
  (1, 57, 256),
  (0, 59, 256),
  (0, 61, 256),
  (0, 62, 254),
  (0, 65, 259),
  (0, 65, 260),
  (1, 23, 270),
  (1, 24, 270),
  (1, 25, 270),
  (1, 26, 266),
  (1, 26, 267),
  (1, 26, 268),
  (0, 27, 266),
  (0, 27, 267),
  (0, 27, 268),
  (0, 28, 266),
  (0, 28, 267),
  (0, 28, 268),
  (0, 29, 270),
  (0, 35, 269),
  (1, 35, 271),
  (0, 35, 272),
  (0, 35, 273),
  (0, 35, 274),
  (1, 36, 271),
  (1, 37, 271),
  (1, 38, 269),
  (1, 38, 271),
  (1, 38, 272),
  (1, 38, 273),
  (1, 38, 274),
  (1, 39, 269),
  (1, 39, 271),
  (1, 39, 272),
  (1, 39, 273),
  (1, 39, 274),
  (1, 40, 269),
  (1, 40, 271),
  (1, 40, 272),
  (1, 40, 273),
  (1, 40, 274),
  (1, 42, 269),
  (1, 42, 271),
  (1, 42, 272),
  (1, 42, 273),
  (1, 42, 274),
  (1, 45, 264),
  (1, 45, 265),
  (0, 45, 271),
  (0, 48, 264),
  (0, 48, 265),
  (0, 49, 264),
  (0, 49, 265),
  (0, 50, 271),
  (0, 62, 264),
  (0, 62, 265),
  (1, 4, 281),
  (1, 6, 282),
  (0, 8, 281),
  (1, 17, 278),
  (1, 17, 279),
  (0, 17, 281),
  (1, 18, 278),
  (1, 18, 279),
  (0, 20, 278),
  (0, 20, 279),
  (0, 20, 280),
  (0, 20, 285),
  (0, 21, 282),
  (1, 26, 278),
  (1, 35, 282),
  (1, 35, 284),
  (1, 36, 282),
  (1, 36, 284),
  (1, 37, 282),
  (1, 37, 284),
  (1, 38, 275),
  (1, 39, 275),
  (1, 40, 275),
  (1, 42, 275),
  (1, 42, 282),
  (1, 42, 284),
  (0, 45, 275),
  (0, 45, 276),
  (0, 45, 277),
  (0, 45, 278),
  (1, 45, 280),
  (0, 45, 281),
  (1, 45, 283),
  (0, 45, 284),
  (1, 45, 285),
  (1, 45, 286),
  (1, 46, 276),
  (1, 46, 277),
  (1, 46, 280),
  (0, 46, 283),
  (0, 46, 285),
  (1, 47, 276),
  (1, 47, 277),
  (1, 47, 280),
  (0, 47, 283),
  (0, 47, 285),
  (0, 48, 286),
  (0, 49, 277),
  (1, 49, 280),
  (1, 49, 283),
  (1, 49, 285),
  (1, 49, 286),
  (0, 50, 275),
  (0, 51, 275),
  (0, 62, 286),
  (1, 1, 296),
  (0, 1, 303),
  (0, 2, 296),
  (1, 2, 305),
  (0, 3, 305),
  (1, 4, 302),
  (0, 6, 304),
  (0, 7, 288),
  (0, 7, 303),
  (1, 7, 305),
  (0, 8, 302),
  (0, 9, 288),
  (0, 9, 303),
  (0, 9, 305),
  (0, 10, 302),
  (0, 17, 288),
  (1, 20, 288),
  (0, 20, 295),
  (1, 20, 296),
  (1, 20, 303),
  (1, 21, 288),
  (0, 21, 293),
  (1, 21, 303),
  (1, 21, 305),
  (1, 22, 288),
  (1, 22, 296),
  (1, 26, 289),
  (1, 26, 290),
  (1, 26, 291),
  (1, 26, 292),
  (1, 26, 293),
  (0, 27, 289),
  (0, 27, 290),
  (0, 27, 291),
  (1, 27, 292),
  (0, 27, 293),
  (0, 28, 289),
  (0, 28, 290),
  (0, 28, 291),
  (0, 28, 292),
  (0, 28, 293),
  (0, 35, 287),
  (1, 35, 294),
  (0, 35, 299),
  (0, 35, 300),
  (0, 35, 301),
  (1, 38, 287),
  (1, 38, 294),
  (1, 38, 299),
  (1, 38, 300),
  (1, 38, 301),
  (1, 39, 287),
  (1, 39, 294),
  (1, 39, 299),
  (1, 39, 300),
  (1, 39, 301),
  (1, 40, 287),
  (1, 40, 294),
  (1, 40, 299),
  (1, 40, 300),
  (1, 40, 301),
  (1, 42, 287),
  (1, 42, 294),
  (1, 42, 299),
  (1, 42, 300),
  (1, 42, 301),
  (0, 45, 294),
  (1, 45, 295),
  (1, 45, 297),
  (1, 45, 298),
  (0, 45, 299),
  (0, 45, 300),
  (0, 45, 301),
  (1, 45, 304),
  (1, 46, 295),
  (1, 46, 304),
  (1, 47, 295),
  (1, 47, 304),
  (0, 48, 297),
  (0, 48, 298),
  (1, 49, 297),
  (1, 49, 298),
  (0, 49, 304),
  (0, 50, 294),
  (0, 62, 297),
  (0, 62, 298),
  (1, 35, 306),
  (1, 38, 306),
  (1, 39, 306),
  (1, 40, 306),
  (1, 42, 306),
  (0, 45, 306),
  (0, 50, 306),
  (0, 1, 313),
  (1, 6, 316),
  (0, 7, 311),
  (0, 9, 311),
  (0, 17, 317),
  (1, 20, 311),
  (0, 20, 313),
  (0, 20, 316),
  (1, 20, 317),
  (1, 21, 311),
  (0, 21, 313),
  (1, 22, 313),
  (0, 22, 317),
  (1, 26, 319),
  (1, 27, 319),
  (0, 28, 319),
  (1, 35, 309),
  (1, 35, 312),
  (1, 35, 315),
  (1, 35, 316),
  (1, 35, 318),
  (1, 36, 309),
  (1, 36, 312),
  (1, 36, 315),
  (1, 36, 316),
  (1, 36, 318),
  (1, 37, 309),
  (1, 37, 312),
  (1, 37, 315),
  (1, 37, 316),
  (1, 37, 318),
  (1, 38, 312),
  (1, 38, 314),
  (1, 38, 315),
  (1, 38, 318),
  (1, 39, 312),
  (1, 39, 314),
  (1, 39, 315),
  (1, 39, 318),
  (1, 40, 312),
  (1, 40, 314),
  (1, 40, 315),
  (1, 40, 318),
  (1, 42, 312),
  (0, 42, 314),
  (1, 42, 315),
  (1, 42, 316),
  (1, 42, 318),
  (0, 43, 308),
  (0, 43, 310),
  (0, 45, 308),
  (0, 45, 310),
  (0, 45, 312),
  (0, 45, 314),
  (0, 45, 316),
  (0, 45, 318),
  (1, 46, 308),
  (1, 46, 310),
  (1, 47, 308),
  (1, 47, 310),
  (1, 48, 309),
  (1, 50, 307),
  (0, 50, 312),
  (1, 51, 307),
  (0, 57, 309),
  (1, 57, 315),
  (1, 62, 309),
  (0, 17, 326),
  (1, 17, 329),
  (1, 18, 329),
  (0, 20, 320),
  (0, 20, 325),
  (1, 20, 326),
  (0, 20, 327),
  (0, 20, 329),
  (0, 22, 325),
  (1, 22, 326),
  (1, 23, 321),
  (1, 24, 321),
  (0, 25, 321),
  (1, 26, 326),
  (1, 26, 329),
  (0, 27, 326),
  (0, 27, 329),
  (1, 35, 322),
  (1, 35, 328),
  (1, 36, 322),
  (1, 36, 328),
  (1, 37, 322),
  (1, 37, 328),
  (1, 38, 322),
  (0, 38, 324),
  (1, 38, 328),
  (1, 39, 322),
  (1, 39, 328),
  (1, 40, 322),
  (1, 40, 328),
  (1, 42, 320),
  (1, 42, 322),
  (0, 42, 328),
  (0, 43, 325),
  (1, 45, 320),
  (0, 45, 323),
  (0, 45, 325),
  (0, 45, 327),
  (0, 45, 328),
  (1, 45, 330),
  (1, 46, 323),
  (1, 46, 325),
  (1, 46, 327),
  (0, 46, 330),
  (1, 47, 323),
  (1, 47, 325),
  (1, 47, 327),
  (0, 47, 330),
  (0, 48, 330),
  (1, 49, 320),
  (1, 49, 330),
  (1, 50, 324),
  (0, 51, 324);

insert into exercise_how_to (exercise_id, name, url)
values (
    3,
    'youtube',
    'https://www.youtube.com/watch?v=2-o_-HTPhv8'
  ),
  (
    4,
    'youtube',
    'https://www.youtube.com/watch?v=o2qSVwxiFk4'
  ),
  (
    5,
    'youtube',
    'https://www.youtube.com/watch?v=9vYCwtHkWgI'
  ),
  (
    6,
    'youtube',
    'https://www.youtube.com/watch?v=WvLMauqrnK8'
  ),
  (
    7,
    'youtube',
    'https://www.youtube.com/watch?v=x-8iKS2Tcxk'
  ),
  (
    8,
    'youtube',
    'https://train.getswole.app/exercises/bent-over-dumbbell-rear-delt-raise-garage-2-0/'
  ),
  (
    9,
    'youtube',
    'https://www.youtube.com/watch?v=NwzUje3z0qY'
  ),
  (
    10,
    'youtube',
    'https://www.youtube.com/watch?v=ul0Z_skxUeI'
  ),
  (
    11,
    'youtube',
    'https://www.youtube.com/watch?v=VzIcGAgBiaM'
  ),
  (
    12,
    'youtube',
    'https://www.youtube.com/watch?v=6Fzep104f0s'
  ),
  (
    13,
    'youtube',
    'https://www.youtube.com/watch?v=Orxowest56U'
  ),
  (
    14,
    'youtube',
    'https://www.youtube.com/watch?v=m0FOpMEgero'
  ),
  (
    15,
    'youtube',
    'https://www.youtube.com/watch?v=MujQbvXr2vE'
  ),
  (
    16,
    'youtube',
    'https://www.youtube.com/watch?v=pAplQXk3dkU'
  ),
  (
    17,
    'youtube',
    'https://www.youtube.com/watch?v=5_ejbGfdAQE'
  ),
  (
    18,
    'youtube',
    'https://www.youtube.com/watch?v=YCKPD4BSD2E'
  ),
  (19, 'youtube', 'https://youtu.be/FDay9wFe5uE'),
  (
    20,
    'youtube',
    'https://www.youtube.com/watch?v=G_8LItOiZ0Q'
  ),
  (
    21,
    'youtube',
    'https://www.youtube.com/watch?v=brFHyOtTwH4'
  ),
  (
    22,
    'youtube',
    'https://www.youtube.com/watch?v=6GMKPQVERzw'
  ),
  (
    23,
    'youtube',
    'https://www.youtube.com/watch?v=cYKYGwcg0U8'
  ),
  (
    24,
    'youtube',
    'https://www.youtube.com/watch?v=ZS2poP_Wc04'
  ),
  (
    25,
    'youtube',
    'https://www.youtube.com/watch?v=NLDBFtSNhqg'
  ),
  (
    26,
    'youtube',
    'https://www.youtube.com/watch?v=q5X9thiKofE'
  ),
  (
    27,
    'youtube',
    'https://www.youtube.com/watch?v=flmMqsi3rl0'
  ),
  (
    28,
    'youtube',
    'https://www.youtube.com/watch?v=efDlxayx234'
  ),
  (
    29,
    'youtube',
    'https://www.youtube.com/watch?v=YQ2s_Y7g5Qk'
  ),
  (
    30,
    'youtube',
    'https://www.youtube.com/watch?v=q7H4AQPQAz0'
  ),
  (
    31,
    'youtube',
    'https://www.youtube.com/watch?v=7IV729pBFUc'
  ),
  (
    32,
    'youtube',
    'https://www.youtube.com/watch?v=GG6EjB3ZVf4'
  ),
  (33, 'youtube', 'https://youtu.be/UCXxvVItLoM'),
  (34, 'youtube', 'https://youtu.be/fvtbQuAG1sg'),
  (
    36,
    'youtube',
    'https://www.youtube.com/watch?v=EK747VC37yE'
  ),
  (
    37,
    'youtube',
    'https://www.youtube.com/watch?v=tZUYS7X50so'
  ),
  (
    38,
    'youtube',
    'https://www.youtube.com/watch?v=cEaIlD7BCW0'
  ),
  (
    39,
    'youtube',
    'https://www.youtube.com/watch?v=sExoXhSZzNQ'
  ),
  (
    40,
    'youtube',
    'https://www.youtube.com/watch?v=I34ysEkPK7w'
  ),
  (
    41,
    'youtube',
    'https://www.youtube.com/watch?v=YRg2hyPjp9E'
  ),
  (
    42,
    'youtube',
    'https://www.youtube.com/watch?v=isMOXCRkywQ'
  ),
  (
    44,
    'youtube',
    'https://www.youtube.com/watch?v=zgToz5FiI-E'
  ),
  (
    46,
    'youtube',
    'https://www.youtube.com/watch?v=oVq4fkh5pFs'
  ),
  (
    47,
    'youtube',
    'https://www.youtube.com/watch?v=HKbDU850mbE'
  ),
  (
    49,
    'youtube',
    'https://www.youtube.com/watch?v=8iPEnn-ltC8'
  ),
  (
    50,
    'youtube',
    'https://www.youtube.com/watch?v=_O1xunCfYEM'
  ),
  (
    51,
    'youtube',
    'https://www.youtube.com/watch?v=h8cVvPpleCA'
  ),
  (
    52,
    'youtube',
    'https://www.youtube.com/watch?v=bMBxJsBgTao'
  ),
  (
    53,
    'youtube',
    'https://www.youtube.com/watch?v=pfSMst14EFk'
  ),
  (
    54,
    'youtube',
    'https://www.youtube.com/watch?v=HHxNbhP16UE'
  ),
  (
    55,
    'youtube',
    'https://www.youtube.com/watch?v=U5pviWt7sMo'
  ),
  (
    56,
    'youtube',
    'https://www.youtube.com/watch?v=TTFDQnOLq6Y'
  ),
  (
    57,
    'youtube',
    'https://www.youtube.com/watch?v=m0FOpMEgero'
  ),
  (
    58,
    'youtube',
    'https://www.youtube.com/watch?v=SxW1G0ahSBc'
  ),
  (
    59,
    'youtube',
    'https://www.youtube.com/watch?v=SLOtxuaStoQ'
  ),
  (
    60,
    'youtube',
    'https://www.youtube.com/watch?v=P5sXHLmXmBM'
  ),
  (
    61,
    'youtube',
    'https://www.youtube.com/watch?v=FiQUzPtS90E'
  ),
  (
    62,
    'youtube',
    'https://www.youtube.com/watch?v=48ORDkT24tY'
  ),
  (
    64,
    'youtube',
    'https://www.youtube.com/watch?v=SQOsKWSHTMo'
  ),
  (
    65,
    'youtube',
    'https://www.youtube.com/watch?v=osHzkwG-9JE'
  ),
  (
    66,
    'youtube',
    'https://www.youtube.com/watch?v=RD_A-Z15ER4'
  ),
  (
    67,
    'youtube',
    'https://www.youtube.com/watch?v=U1OanW26I78'
  ),
  (
    68,
    'youtube',
    'https://www.youtube.com/watch?v=h2sXIR3S-hU'
  ),
  (
    69,
    'youtube',
    'https://www.youtube.com/watch?v=h2sXIR3S-hU'
  ),
  (
    70,
    'youtube',
    'https://www.youtube.com/watch?v=h2sXIR3S-hU'
  ),
  (
    71,
    'youtube',
    'https://www.youtube.com/watch?v=h2sXIR3S-hU'
  ),
  (
    72,
    'youtube',
    'https://www.youtube.com/watch?v=h2sXIR3S-hU'
  ),
  (
    73,
    'youtube',
    'https://www.youtube.com/watch?v=h2sXIR3S-hU'
  ),
  (
    74,
    'youtube',
    'https://www.youtube.com/watch?v=h2sXIR3S-hU'
  ),
  (
    77,
    'youtube',
    'https://www.youtube.com/watch?v=h2sXIR3S-hU'
  ),
  (
    82,
    'youtube',
    'https://www.youtube.com/watch?v=h2sXIR3S-hU'
  ),
  (
    84,
    'youtube',
    'https://youtu.be/kmbzQJoRMUU?t=125'
  ),
  (85, 'youtube', 'https://youtu.be/gRIX9uEYVZk'),
  (
    86,
    'youtube',
    'https://www.youtube.com/watch?v=JnLFSFurrqQ'
  ),
  (
    91,
    'youtube',
    'https://www.youtube.com/watch?v=F1XdXdCjERk'
  ),
  (
    92,
    'youtube',
    'https://www.youtube.com/watch?v=-O9H15yVc8s'
  ),
  (
    93,
    'youtube',
    'https://www.youtube.com/watch?v=LwsQ-AGc8JU'
  ),
  (
    94,
    'youtube',
    'https://www.youtube.com/watch?v=IZE8woIduAU'
  ),
  (
    95,
    'youtube',
    'https://www.youtube.com/watch?v=pQDrcNoDNVM'
  ),
  (
    96,
    'youtube',
    'https://www.youtube.com/watch?v=pXPyinlhx5U'
  ),
  (
    97,
    'youtube',
    'https://www.youtube.com/watch?v=x91Yog8fDHA'
  ),
  (
    98,
    'youtube',
    'https://www.youtube.com/watch?v=0bq4iUDuU9g'
  ),
  (
    99,
    'youtube',
    'https://www.youtube.com/watch?v=pUS6HBQjRmc'
  ),
  (
    100,
    'youtube',
    'https://www.youtube.com/watch?v=1IIPcUCKxcE'
  ),
  (
    101,
    'youtube',
    'https://www.youtube.com/watch?v=CN_7cz3P-1U'
  ),
  (
    102,
    'youtube',
    'https://www.youtube.com/watch?v=SCVCLChPQFY'
  ),
  (
    103,
    'youtube',
    'https://www.youtube.com/watch?v=Kxt_5D2Rpgg'
  ),
  (
    104,
    'youtube',
    'https://www.youtube.com/watch?v=WHTUFFy1Afc'
  ),
  (
    105,
    'youtube',
    'https://www.youtube.com/watch?v=lJ2o89kcnxY'
  ),
  (
    106,
    'youtube',
    'https://www.youtube.com/watch?v=VP1FvbLqRuk'
  ),
  (
    107,
    'youtube',
    'https://www.youtube.com/watch?v=XyhNXJ7wYFs'
  ),
  (
    108,
    'youtube',
    'https://www.youtube.com/watch?v=5PoEksoJNaw'
  ),
  (
    109,
    'youtube',
    'https://www.youtube.com/watch?v=lCtWt9VyIgI'
  ),
  (
    110,
    'youtube',
    'https://www.youtube.com/watch?v=iIBp-my013I'
  ),
  (
    111,
    'youtube',
    'https://www.youtube.com/watch?v=NBY9-kTuHEk'
  ),
  (
    112,
    'youtube',
    'https://www.youtube.com/watch?v=7iw2gLZKZ0w'
  ),
  (
    113,
    'youtube',
    'https://www.youtube.com/watch?v=b3124L0KK3Q'
  ),
  (
    114,
    'youtube',
    'https://www.youtube.com/watch?v=vgn7bSXkgkA'
  ),
  (
    115,
    'youtube',
    'https://www.youtube.com/watch?v=wBzU6_YJPEU'
  );

