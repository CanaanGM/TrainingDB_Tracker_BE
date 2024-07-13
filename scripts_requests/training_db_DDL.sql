-- Singular Muscle
create table if not exists muscle (
	id integer primary key autoincrement,
	name varchar(64),
	`function` text,
	wiki_page_url varchar(255)
);

-- A Group of Muscle; Glutes are Glutues maximus and the other one i forgot the name of, it's on top of the maximus
create table if not exists muscle_group(
	id integer primary key autoincrement,
	scientific_name varchar(64) not null,
	common_name varchar(64),
	`function` text,
	wiki_page_url varchar(255)
);

-- Juncture table for the relations of muscles 
create table if not exists muscle_group_muscle (
	id integer primary key autoincrement,
	muscle_group_id integer,
	muscle_id integer,
	unique (muscle_group_id, muscle_id),
	Foreign Key(muscle_group_id) references muscle_group(id),
	Foreign Key (muscle_id) references muscle(id)
);

-- singular exercise 
-- description: the description of the exercise
-- how to: how to perform the exercise
create table if not exists exercise(
	id integer primary key autoincrement,
	name varchar(64) not null unique,
	description text,
	how_to text
);




-- what muscles involves in an exercise
-- is_primary: if a muscle is the main target of an exercise
create table if not exists exercise_muscle(
	id integer primary key autoincrement,	
	is_primary boolean,
	muscle_id integer,
	exercise_id integer,
	unique (muscle_id,exercise_id),
	Foreign Key(muscle_id) references muscle(id),
	Foreign Key(exercise_id) references exercise(id)
);

-- a link to a video or a pic/gif of how to perform an exercise
create table if not exists exercise_how_to(
	id integer primary key autoincrement,
	exercise_id integer,
	name varchar(64) not null,
	url varchar(255) not null
);

-- equipment table
-- like dumbbell, echangable weights, 12kg
create table if not exists equipment (
	id integer primary key autoincrement,
	name varchar(64) not null,
	description text,
	weight real,
	created_at datetime default current_timestamp
);

-- what muscle does an equipment train
-- is_preimary: denotes the involvement of the muscle
create table if not exists equipment_muscle(
	id integer primary key autoincrement,
	equipment_id integer,
	muscle_id integer,
	is_primary boolean,
	unique(equipment_id,muscle_id),
	Foreign Key(equipment_id) references equipment (id),
	Foreign Key(muscle_id) references muscle(id)
);


-- singular exercise record
-- Dragon flag, 20,0,0,0, "Go Slower", 0, 2024-06-24 00:55:24
-- 0 weight = Body Weight
create table if not exists exercise_record (
	id integer primary key autoincrement,
	exercise_id integer,
	repetitions integer,
	timer_in_seconds integer,
	distance_in_meters integer,
	weight_used real,
	notes text,
	created_at datetime default current_timestamp,
	Foreign Key(exercise_id) references exercise(id)
);

-- represets a singular training session no matter how big or small (a day can have a lot)
-- 00:30:00, 495, "i love rope jumping", 2024-06-24 00:55:24
-- for storing data
-- as i don't really care about the order of the exercises (blocks), i only want to know what i did for data sience!.
create table if not exists training_session(
	id integer primary key autoincrement, 
	duration integer,
	calories integer,
	notes text, 
	created_at datetime default current_timestamp
);

-- all training records for a session, allowing this:
-- 00:30:00, 495,Rope jumping,  "i love rope jumping", 2024-06-24 00:55:24
-- 00:05:00, 12, Dragon Flag, 20,20,20, 2024-06-24 00:55:24. Join and grouped on exercise.name, the "20,20,20" are repetitions.
create table if not exists training_session_exercise_record(
	id integer primary key autoincrement,
	training_session_id integer,
	exercise_record_id integer,
	last_weight_used real, -- should be populated when reading the exercise
	created_at datetime default current_timestamp,
	Foreign Key(training_session_id) references training_session(id),
	Foreign Key(exercise_record_id) references exercise_record(id)
);

-- what type of training have i done
-- BodyBuilding, Strength Training, Athletics, Cardio, Yoga . . . etc
create table if not exists training_type(
	id integer primary key autoincrement,
	name varchar(64) not null
);

-- what types does an exercise have ; Dragon Flag, [BodyBuilding, Calisthincs, Athletics]
create table if not exists exercise_type(
	id integer primary key autoincrement,
	exercise_id integer,
	training_type_id integer,
	unique (exercise_id ,training_type_id),
	Foreign Key(exercise_id) references exercise (id),
	Foreign Key(training_type_id) references training_type (id)
);

-- training session types
create table if not exists training_session_type(
	id integer primary key autoincrement,
	training_session_id integer,
	training_type_id integer,
	Foreign Key(training_session_id) references training_session(id),
	Foreign Key(training_type_id) references training_type(id)
);

-- overall training plan that you follow for a set of weeks
create table if not exists training_plan(
	id integer primary key autoincrement,
	name varchar(64),
	training_weeks integer not null, -- 4 weeks for example
	training_days_per_week integer not null, -- 5 days per week of training for example
	description text,
	notes text,
	created_at datetime default current_timestamp
);

-- types of training plan ; [BodyBuilding, Strength Training] or [BodyBuilding] or [Strength Training]
create table if not exists training_plan_type(
	id integer primary key autoincrement,
	training_plan_id integer,
	training_type_id integer,
	Foreign Key(training_plan_id) references training_plan(id),
	Foreign Key(training_type_id) references training_type(id)
);


-- what equipment does the training plan needs/ require
create table if not exists training_plan_equipment(
	id integer primary key autoincrement,
	training_plan_id integer,
	equipment_id integer,
	Foreign key (training_plan_id) references training_plan(id),
	Foreign Key (equipment_id) references equipment(id)
);

-- training week record
-- meso_cycle: number of weeks on a meso cycle (4-6 weeks or training, where the last week is recovery focused)
create table if not exists training_week(
	id integer primary key autoincrement,
	name varchar(64) not null,
	mesocycle integer, -- maybe creata a different table for this ¯\_(ツ)_/¯
	order_number integer not null,
	created_at datetime default current_timestamp 
); 

-- the weeks in a training plan, if it has 5 weeks of trining, it should have 5 weeks referenced to it...duh
create table if not exists training_plan_weeks (
	id integer primary key autoincrement,
	training_plan_id integer,
	training_week_id integer,
	created_at datetime default current_timestamp ,
	unique (training_plan_id,training_week_id),
	Foreign Key(training_plan_id) references training_plan(id),
	Foreign Key(training_week_id) references training_week(id) 
);

-- a day of training, holds the blocks
-- for viewing the exercises in an ordered manner
create table if not exists training_day(
	id integer primary key autoincrement,
	name varchar(64) not null,
	notes text,
	created_at datetime default current_timestamp 
);

-- training days of a week
-- order_number denotes the ordering of the days
create table if not exists training_week_days (
	id integer primary key autoincrement,
	training_week_id integer,
	training_day_id integer,
	order_number integer,
	unique (training_week_id,training_day_id),
	Foreign Key(training_week_id) references training_week(id),
	Foreign Key(training_day_id) references training_day(id) 
);

-- what muscle groups did i hit in a day of training
create table if not exists training_day_muscle_groups (
	id integer primary key autoincrement,
	training_day_id integer,
	muscle_group_id integer,
	unique (training_day_id,muscle_group_id),
	Foreign Key (training_day_id) references training_day(id),
	Foreign Key (muscle_group_id) references muscle_group(id) 
);

-- a block holds many exercises and denotes their order
-- like a super set of exercises. 
-- or a drop set
create table if not exists block(
	id integer primary key autoincrement,
	name varchar(64) not null,
	`sets` integer,
	rest_in_seconds integer,
	instrcustions text,
	created_at datetime default current_timestamp
);

-- all the blocks a training day has
-- order_number denotes the ordering of the block
create table if not exists training_day_blocks(
	id integer primary key autoincrement,
	training_day_id integer,
	block_id integer,
	order_number integer,
	created_at datetime default current_timestamp,
	unique (training_day_id,block_id),
	Foreign Key(block_id) references block(id),
	Foreign Key(training_day_id) references training_day(id)
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
	Foreign Key(block_id) references block(id),
	Foreign Key(exercise_id) references exercise(id)
);


create table if not exists author (
	id integer primary key autoincrement ,
	name varchar(64) not null,
	description text
);

create table if not exists author_links(
	id integer primary key autoincrement ,
	author_id integer,
	url varchar(255),
	created_at datetime default current_timestamp 
);


create table if not exists `user` (
	id text primary key ,
	username varchar(255) not null unique,
	email varchar(255) not null unique,
	created_at datetime default current_timestamp 
);

create table if not exists user_passwords(
	id text primary key,
	user_id text,
	password text not null,
	is_active boolean,
	created_at datetime default current_timestamp,
	Foreign Key(user_id) references `user`(id)
);

create table if not exists user_images(
	id integer primary key autoincrement,
	user_id text,
	url varchar(255) not null,
	is_primary boolean,
	created_at datetime default current_timestamp, 
	Foreign Key(user_id) references `user`(id)
);
