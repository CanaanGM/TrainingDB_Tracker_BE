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
 *
 * */
create table if not exists muscle
(
    id
    integer
    primary
    key
    autoincrement,
    name
    text
    unique
    NOT
    NULL,
    muscle_group
    text
    NOT
    null,
    `function`
    text,
    wiki_page_url
    varchar
(
    255
)
    );
CREATE INDEX idx_muscle_name ON muscle (name);
CREATE INDEX idx_muscle_group ON muscle (muscle_group);
-- singular exercise
-- description: the description of the exercise
-- how to: how to perform the exercise
create table if not exists exercise
(
    id
    integer
    primary
    key
    autoincrement,
    name
    text
    not
    null
    unique,
    description
    text,
    how_to
    text,
    difficulty
    integer
    default
    0,
    CHECK
(
    difficulty
    >=
    0
    AND
    difficulty
    <=
    5
)
    );
create index idx_exercise_name on exercise (name);
create index idx_exercise_difficulty on exercise (difficulty);
-- what muscles involves in an exercise
-- is_primary: if a muscle is the main target of an exercise
create table if not exists exercise_muscle
(
    is_primary
    boolean
    DEFAULT
    false,
    muscle_id
    integer,
    exercise_id
    integer,
    primary
    key
(
    muscle_id,
    exercise_id
),
    CONSTRAINT fk_muscle_id Foreign Key
(
    muscle_id
) references muscle
(
    id
) ON DELETE cascade,
    CONSTRAINT fk_exercise_id Foreign Key
(
    exercise_id
) references exercise
(
    id
)
  ON DELETE cascade
    );
create index idx_exercise_muscle_is_primary on exercise_muscle (is_primary);
-- a link to a video or a pic/gif of how to perform an exercise
create table if not exists exercise_how_to
(
    id
    integer
    primary
    key
    autoincrement,
    exercise_id
    integer,
    name
    text
    not
    null,
    url
    varchar
(
    255
) not NULL,
    CONSTRAINT fk_exercise_id Foreign KEY
(
    exercise_id
) REFERENCES exercise
(
    id
) ON DELETE cascade
    );
-- equipment table
-- like dumbbell, changeable weights, 12kg
create table if not exists equipment
(
    id
    integer
    primary
    key
    autoincrement,
    name
    text
    not
    null,
    description
    text,
    weight_kg
    real,
    created_at
    datetime
    default
    current_timestamp
);
-- singular exercise record
-- Dragon flag, 20,0,0,0, "Go Slower", 0, 2024-06-24 00:55:24
-- 0 weight = Body Weight
create table if not exists exercise_record
(
    id
    integer
    primary
    key
    autoincrement,
    exercise_id
    integer,
    repetitions
    integer,
    timer_in_seconds
    integer,
    distance_in_meters
    integer,
    weight_used_kg
    real,
    notes
    text,
    rest_in_seconds
    integer,
    incline
    integer,
    speed
    integer,
    heart_rate_avg
    integer,
    KcalBurned
    integer,
    mood
    integer,
    rate_of_perceived_exertion
    int, -- effort
    created_at
    datetime
    default
    current_timestamp,
    CHECK
(
    mood
    >=
    0
    AND
    mood
    <=
    10
),
    CONSTRAINT fk_exercise_id Foreign Key
(
    exercise_id
) references exercise
(
    id
) ON DELETE cascade
    );
create index idx_exercise_record_created_at on exercise_record (created_at);
-- represents a singular training session no matter how big or small (a day can have a lot)
-- 00:30:00, 495, "i love rope jumping", 2024-06-24 00:55:24
-- for storing data
-- as i don't really care about the order of the exercises (blocks), i only want to know what i did for data science!.
create table if not exists training_session
(
    id
    integer
    primary
    key
    autoincrement,
    duration_in_seconds
    integer,
    total_calories_burned
    integer,
    notes
    text,
    mood
    integer,
    feeling
    text, -- my legs were dead from last time, or something similar
    created_at
    datetime
    default
    current_timestamp,
    CHECK
(
    mood
    >=
    0
    AND
    mood
    <=
    10
)
    );
create index idx_training_session_created_at on training_session (created_at);
-- all training records for a session, allowing this:
-- 00:30:00, 495,Rope jumping,  "i love rope jumping", 2024-06-24 00:55:24
-- 00:05:00, 12, Dragon Flag, 20,20,20, 2024-06-24 00:55:24. Join and grouped on exercise.name, the "20,20,20" are repetitions.
create table if not exists training_session_exercise_record
(
    id
    integer
    primary
    key
    autoincrement,
    training_session_id
    integer,
    exercise_record_id
    integer,
    last_weight_used_kg
    real, -- should be populated when reading the exercise
    created_at
    datetime
    default
    current_timestamp,
    CONSTRAINT
    fk_training_session_id
    Foreign
    Key
(
    training_session_id
) references training_session
(
    id
) ON DELETE cascade,
    CONSTRAINT fk_exercise_record_id Foreign Key
(
    exercise_record_id
) references exercise_record
(
    id
)
  ON DELETE CASCADE
    );
create index idx_training_session_exercise_record_created_at on training_session_exercise_record (created_at);
-- what type of training have i done
-- why no enums ??!
-- BodyBuilding, Strength Training, Athletics, Cardio, Yoga . . . etc
create table if not exists training_type
(
    id
    integer
    primary
    key
    autoincrement,
    name
    text
    not
    NULL
    unique
);
CREATE unique index idx_training_type_name on training_type (name);
-- what types does an exercise have ; Dragon Flag, [BodyBuilding, Calisthenics, Athletics]
create table if not exists exercise_type
(
    exercise_id
    integer,
    training_type_id
    integer,
    primary
    key
(
    exercise_id,
    training_type_id
),
    CONSTRAINT fk_exercise_id Foreign Key
(
    exercise_id
) references exercise
(
    id
) ON DELETE cascade ,
    CONSTRAINT fk_training_type_id Foreign Key
(
    training_type_id
) references training_type
(
    id
)
  ON DELETE cascade
    );
-- training session types
create table if not exists training_session_type
(
    training_session_id
    integer,
    training_type_id
    integer,
    primary
    key
(
    training_session_id,
    training_type_id
),
    CONSTRAINT fk_training_session_id Foreign Key
(
    training_session_id
) references training_session
(
    id
) ON DELETE cascade,
    CONSTRAINT fk_training_type_id Foreign Key
(
    training_type_id
) references training_type
(
    id
)
  ON DELETE cascade
    );
-- overall training plan that you follow for a set of weeks
create table if not exists training_plan
(
    id
    integer
    primary
    key
    autoincrement,
    name
    text,
    training_weeks
    integer
    not
    null, -- 4 weeks for example
    training_days_per_week
    integer
    not
    null, -- 5 days per week of training for example
    description
    text,
    notes
    text,
    created_at
    datetime
    default
    current_timestamp
);
-- types of training plan ; [BodyBuilding, Strength Training] or [BodyBuilding] or [Strength Training]
create table if not exists training_plan_type
(
    training_plan_id
    integer,
    training_type_id
    integer,
    primary
    key
(
    training_plan_id,
    training_type_id
),
    CONSTRAINT fk_training_plan_id Foreign Key
(
    training_plan_id
) references training_plan
(
    id
) ON DELETE cascade,
    CONSTRAINT fk_training_type_id Foreign Key
(
    training_type_id
) references training_type
(
    id
)
  ON DELETE cascade
    );
-- what equipment does the training plan needs/ require
create table if not exists training_plan_equipment
(
    training_plan_id
    integer,
    equipment_id
    integer,
    primary
    key
(
    training_plan_id,
    equipment_id
),
    CONSTRAINT fk_training_plan_id Foreign key
(
    training_plan_id
) references training_plan
(
    id
) ON DELETE cascade,
    CONSTRAINT fk_equipment_id Foreign Key
(
    equipment_id
) references equipment
(
    id
)
  ON DELETE cascade
    );
-- training week record
-- meso_cycle: number of weeks on a meso cycle (4-6 weeks or training, where the last week is recovery focused)
create table if not exists training_week
(
    id
    integer
    primary
    key
    autoincrement,
    name
    text
    not
    null,
    -- mesocycle integer, -- maybe create a different table for this ¯\_(ツ)_/¯
    order_number
    integer
    not
    null,
    created_at
    datetime
    default
    current_timestamp,
    training_plan_id
    integer,
    CONSTRAINT
    fk_training_plan_id
    Foreign
    Key
(
    training_plan_id
) references training_plan
(
    id
) ON DELETE CASCADE,
    check
(
    order_number
    >=
    1
)
    );
-- a day of training, holds the blocks
-- for viewing the exercises in an ordered manner
create table if not exists training_day
(
    id
    integer
    primary
    key
    autoincrement,
    name
    text
    not
    null,
    notes
    text,
    order_number
    integer,
    created_at
    datetime
    default
    current_timestamp,
    training_week_id
    integer,
    CONSTRAINT
    fk_training_week_id
    Foreign
    Key
(
    training_week_id
) references training_week
(
    id
) ON DELETE CASCADE,
    check
(
    order_number
    >=
    1
)
    );
-- what muscle  did i hit in a day of training
create table if not exists training_day_muscle
(
    training_day_id
    integer,
    muscle_id
    integer,
    primary
    key
(
    training_day_id,
    muscle_id
),
    CONSTRAINT fk_training_day_muscle_training_day_id Foreign Key
(
    training_day_id
) references training_day
(
    id
),
    CONSTRAINT fk_training_day_muscle_muscle_id Foreign Key
(
    muscle_id
) references muscle
(
    id
)
    );
-- a block holds many exercises and denotes their order
-- like a super set of exercises.
-- or a drop set
create table if not exists block
(
    id
    integer
    primary
    key
    autoincrement,
    name
    text
    not
    null,
    `sets`
    integer,
    rest_in_seconds
    integer,
    instructions
    text,
    order_number
    integer,
    training_day_id
    integer,
    created_at
    datetime
    default
    current_timestamp,
    CONSTRAINT
    fk_training_day_id
    Foreign
    Key
(
    training_day_id
) references training_day
(
    id
) ON DELETE CASCADE,
    check
(
    order_number
    >=
    1
)
    );
-- all exercises in a block
create table if not exists block_exercises
(
    id
    integer
    primary
    key
    autoincrement,
    block_id
    integer,
    exercise_id
    integer,
    order_number
    integer,
    instructions
    text,
    repetitions
    integer, -- if it has a rep range to go thru
    timer_in_seconds
    integer, -- for timed sets
    distance_in_meters
    integer, -- for walking bullshit exercises, like walking lunges
    created_at
    datetime
    default
    current_timestamp,
    CONSTRAINT
    fk_block_id
    Foreign
    Key
(
    block_id
) references block
(
    id
) ON DELETE cascade ,
    CONSTRAINT fk_exercise_id Foreign Key
(
    exercise_id
) references exercise
(
    id
)
  ON DELETE CASCADE,
    check
(
    order_number
    >=
    1
)
    );
-- to keep track of the weight history for a certain exercise
-- i have this in the relation table
CREATE TABLE if NOT EXISTS exercise_last_used_weight
(
    id
    integer
    primary
    KEY
    autoincrement,
    exercise_id
    integer,
    last_used_weight
    REAL
    NOT
    NULL,
    created_at
    datetime
    default
    current_timestamp,
    Foreign
    Key
(
    exercise_id
) references exercise
(
    id
)
    );
CREATE INDEX idx_exercise_last_used_weight ON exercise_last_used_weight (created_at);
-- table to keep track of muscle measurements
CREATE TABLE IF NOT EXISTS measurements
(
    id
    integer
    primary
    KEY
    autoincrement,
    hip
    integer,
    chest
    integer,
    waist
    integer,
    left_thigh
    integer,
    right_thigh
    integer,
    left_calf
    integer,
    right_calf
    integer,
    left_upper_arm
    integer,
    left_lower_arm
    integer,
    right_upper_arm
    integer,
    right_lower_arm
    integer,
    neck
    integer,
    created_at
    datetime
    default
    current_timestamp
);



INSERT INTO muscle (name, muscle_group, "function", wiki_page_url)
VALUES ('deltoid anterior head', 'shoulders', 'flexes and medially rotates the arm.',
        'https://en.wikipedia.org/wiki/Deltoid_muscle#Anterior_part'),
       ('deltoid posterior head', 'shoulders', 'flexes and medially rotates the arm.',
        'https://en.wikipedia.org/wiki/Deltoid_muscle#Anterior_part'),
       ('deltoid middle head', 'shoulders', 'flexes and medially rotates the arm.',
        'https://en.wikipedia.org/wiki/Deltoid_muscle#Anterior_part'),;
INSERT INTO training_type (name)
VALUES ('strength'),
       ('cardio'),
       ('bodyBuilding'),
       ('martial arts');
INSERT INTO exercise (name, description, how_to, difficulty)
VALUES ('dragon flag', 'a flag', 'lie and cry', 4),
       ('rope jumping', 'jump', 'cant touch this!', 4),
       ('barbell curl', 'curl a barbell . . . duh!', 'one more! GO!', 4);
INSERT INTO exercise_muscle (muscle_id, exercise_id)
VALUES (1, 1),
       (2, 2),
       (3, 3);
INSERT INTO exercise_type (exercise_id, training_type_id)
VALUES (1, 1),
       (2, 2),
       (1, 3),
       (1, 4),
       (3, 3);



INSERT INTO exercise_record (exercise_id, repetitions, weight_used_kg)
VALUES (1, 20, 0),
       (1, 20, 30);
INSERT INTO exercise_record (exercise_id, timer_in_seconds)
VALUES (1, 1800);



select *
from training_session_exercise_record;


select e.*, eh.url
from exercise e
         join exercise_how_to eh on e.id = eh.exercise_id
where e.name = 'floor press'
;






























