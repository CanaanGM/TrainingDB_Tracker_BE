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
    'iUXNkjg5RS/0/uEH8f6tasaKKKt8IVsZgTTtyGH3dmQ=',
    'wnGHc9SponguR0Givr2Zcvy7UI3Szs0y0lYfAwdatUE='
  ),
  (
    2,
    'BzMDx05wo6wjTriEEjxDsb9jZTa6QkbeFAr7B46CAwc=',
    'UqfeVl+kUvwURJ2Avx5nw2+qTBMKUUdrHE5m48RGFU0='
  ),
  (
    3,
    'wA+0773CBog+MlcEjrNcAmHS6pSd06ceminYzPU4wlI=',
    'ROtBSjEmAd8swaKpy0e7yV6n5IJl+K5i6yF/brfjTD0='
  ),
  (
    4,
    'Kw1vjlzykwUqruCdhzdMOQBuOkn4hljD3JLiOdAogWg=',
    'a5MaAfB/7zpN8dd3+i10RgHGJ455lpoSIOrAXXXQbyY='
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
-- and salts.// / 1.Canaan,
-- `كنعان لازم يتدرب !`,
-- salt :`wnGHc9SponguR0Givr2Zcvy7UI3Szs0y0lYfAwdatUE=`,
-- hash :`iUXNkjg5RS/0/uEH8f6tasaKKKt8IVsZgTTtyGH3dmQ=` // / 2.Dante,
-- `pizza is pizza!`,
-- salt :`UqfeVl+kUvwURJ2Avx5nw2+qTBMKUUdrHE5m48RGFU0=`,
-- hash :`BzMDx05wo6wjTriEEjxDsb9jZTa6QkbeFAr7B46CAwc=` // / 3.Alphrad,
-- `sneaky snake`,
-- salt :`ROtBSjEmAd8swaKpy0e7yV6n5IJl+K5i6yF/brfjTD0=`,
-- hash :`wA+0773CBog+MlcEjrNcAmHS6pSd06ceminYzPU4wlI=` // / 4.Nero,
-- `ろまである!`,
-- salt :`a5MaAfB/7zpN8dd3+i10RgHGJ455lpoSIOrAXXXQbyY=`,
-- hash :`Kw1vjlzykwUqruCdhzdMOQBuOkn4hljD3JLiOdAogWg=`