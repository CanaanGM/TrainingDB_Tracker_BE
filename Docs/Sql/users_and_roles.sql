-- Insert users
insert into user (username, email, height, gender)
VALUES ('Canaan', 'canaan@test.com', 173, 'M'),
  ('Dante', 'dante@test.com', 200, 'M'),
  ('Alphrad', 'alphrad@test.com', 172, 'F'),
  ('Nero', 'nero@test.com', 156, 'F');
-- Insert user passwords
insert into user_passwords(user_id, password_hash, password_salt)
VALUES (
    1,
    '$2a$11$hCdSz2IWtWhfSMu5HU1xe.YA6zrxged3TNHoZC/CycqNpaYS7ci4W',
    '$2a$11$hCdSz2IWtWhfSMu5HU1xe.'
  ),
  (
    2,
    '$2a$11$v68jMQkfWr9OS4BHPe20keuztD79mByxoBc2OJFOvO0dBBXPlmQ4e',
    '$2a$11$v68jMQkfWr9OS4BHPe20ke'
  ),
  (
    3,
    '$2a$11$FHNqTyAalmLYbaOpwJ683OY7krQV58AT94Vc6cICI3ihcP4A2jIwG',
    '$2a$11$FHNqTyAalmLYbaOpwJ683O'
  ),
  (
    4,
    '$2a$11$YyB7Yu/pMRy/8xHEHlWJgOUfKUpJwBAq4Im.leW/gTWDzOatDvqai',
    '$2a$11$YyB7Yu/pMRy/8xHEHlWJgO'
  );
-- Insert roles
insert into role(name)
values ('user'),
  ('admin'),
  ('owner'),
  ('thieves');
-- Assign roles to users
insert into user_roles(user_id, role_id)
VALUES (1, 1),
  (1, 2),
  (1, 3),
  (2, 1),
  (3, 1),
  (4, 1);
-- Insert refresh tokens for each user, with varying expiration dates within a week
insert into refresh_token(user_id, token, expires, active)
VALUES (1, 'token1_canaan', DATETIME('now', '+1 day'), 1),
  (1, 'token2_canaan', DATETIME('now', '+2 day'), 0),
  (1, 'token3_canaan', DATETIME('now', '+3 day'), 0),
  (2, 'token1_dante', DATETIME('now', '+1 day'), 1),
  (2, 'token2_dante', DATETIME('now', '+2 day'), 0),
  (2, 'token3_dante', DATETIME('now', '+3 day'), 0),
  (
    3,
    'token1_alphrad',
    DATETIME('now', '+1 day'),
    1
  ),
  (
    3,
    'token2_alphrad',
    DATETIME('now', '+2 day'),
    0
  ),
  (
    3,
    'token3_alphrad',
    DATETIME('now', '+3 day'),
    0
  ),
  (4, 'token1_nero', DATETIME('now', '+1 day'), 1),
  (4, 'token2_nero', DATETIME('now', '+2 day'), 0),
  (4, 'token3_nero', DATETIME('now', '+3 day'), 0);


-- creates 4 new users
-- and assigns hashed passwords
-- and salts.
--  1.Canaan,
-- `كنعان لازم يتدرب !`,
-- salt :`$2a$11$hCdSz2IWtWhfSMu5HU1xe.`
-- hash :`$2a$11$hCdSz2IWtWhfSMu5HU1xe.YA6zrxged3TNHoZC/CycqNpaYS7ci4W`,
--  2.Dante,
-- `pizza is pizza!`,
-- salt :`$2a$11$v68jMQkfWr9OS4BHPe20ke`
-- hash :`2a$11$v68jMQkfWr9OS4BHPe20keuztD79mByxoBc2OJFOvO0dBBXPlmQ4e`,
--  3.Alphrad,
-- `sneaky snake`,
-- salt :`$2a$11$FHNqTyAalmLYbaOpwJ683O`
-- hash :`$2a$11$FHNqTyAalmLYbaOpwJ683OY7krQV58AT94Vc6cICI3ihcP4A2jIwG`,
--  4.Nero,
-- `ろまである!`,
-- salt :`$2a$11$YyB7Yu/pMRy/8xHEHlWJgO`,
-- hash :`$2a$11$YyB7Yu/pMRy/8xHEHlWJgOUfKUpJwBAq4Im.leW/gTWDzOatDvqai`