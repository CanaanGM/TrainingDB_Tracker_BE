# Training Records & Plans Keeper

## Obviously a work in progrss

![Build Status](https://github.com/CanaanGM/TrainingDB_Tracker_BE/actions/workflows/dotnet.yml/badge.svg)



---

### made this so i can

1. log my workouts
1. see all my plans
1. create new plans
1. keep a record of exercises and how they relate to muscles
1. leep a record of my training equipment

---

## Architecure

## To throw or to not throw ?

maybe imma use the result pattern ? will have to test performance, cause i kinda like throwing, what kinda training
session wont have throwing in it ?!

## TODO:

- [ ] facilitate more things to to
- [ ] ORM records
- [ ] consider upsert
- [ ] consider upsert plain sql for creation/updating

### dataBase

- the [schema](https://dbdiagram.io/d/workout-tracker-65bf3a4dac844320ae64ab02)
    - on going design. . .

#### Exercises

> Primary Movement - Modifier (if any) - Equipment (if any)
>

```txt

Primary Movement: Bench Press
Modifier: Reverse Grip
Equipment: Barbell (if specified)
Using this structure, you would name the exercise as:

-> Bench Press - Reverse Grip
```

using sqlite, cause i want to use the database in a standalone mobile application that has **no** access to the
internet. _the gym is in its own dimention it seems_.

some tables are normalized some are not, some relate with a juncture table some directly reference their parent. all for
the sake of simplicity, i can always remodel if the need arise.

no stored procedures or json in the database layer.

### DataLibrary project

meant to be a _stand alone layer_ that interacts with the database and handles all things related to it. **it's made so
i can pluck and add to another solution with minimum hassle**, hence it's the way it is.

---

## Commands

- scaffold the database models

```bash
dotnet ef dbcontext scaffold "Data Source=E:/development/c#/TrainingDB_Integration/training_log_v2.db" Microsoft.EntityFrameworkCore.Sqlite --output-dir ModelsV2 -p .\DataLibrary\  --context-dir Context --no-build
```

then copy the `modelsV2` models into `models`.

- migrations

```bash
dotnet ef database update -p .\DataLibrary\ -s .\DataLibrary\ --no-build -c SqliteContext --connection "Data Source = E:\development\c#\TrainingDB_Integration\training_log_v2.db"

```

- Run

```bash
dotnet watch -p ./API
```
