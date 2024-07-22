using DataLibrary.Context;
using DataLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace DateLibraryTests.helpers;

public static class DatabaseHelpers
{
    public static void SeedDatabaseTrainingSessions(this SqliteContext context, int sessionNumber = 10)
    {
        // context.AddRange(CreateTrainingSessions(sessionNumber));
        context.SaveChanges();
    }

    /// <summary>
    /// Seeds the database with <strong>3</strong> muscles, <strong>4</strong> training types, <strong>3</strong> exercises <i> with their relations</i> to the muscles and types.
    /// <em>Making a session have <strong>4 training types</strong></em>.<br></br>
    /// </summary>
    public static void SeedTypesExercisesAndMuscles(SqliteContext context)
    {
        string insertMuscles = @"
            INSERT INTO muscle (name, muscle_group, ""function"", wiki_page_url) VALUES
            ('deltoid anterior head','shoulders','flexes and medially rotates the arm.','https://en.wikipedia.org/wiki/Deltoid_muscle#Anterior_part'),
            ('deltoid posterior head','shoulders','flexes and medially rotates the arm.','https://en.wikipedia.org/wiki/Deltoid_muscle#Anterior_part'),
            ('deltoid middle head','shoulders','flexes and medially rotates the arm.','https://en.wikipedia.org/wiki/Deltoid_muscle#Anterior_part');
            ";

        string insertTrainingTypes = @"
            INSERT INTO training_type (name) VALUES 
            ('strength'), 
            ('cardio'),
            ('bodybuilding'), 
            ('martial arts');
            ";

        string insertExercises = @"
            INSERT INTO exercise (name, description, how_to, difficulty) VALUES
            ('dragon flag', 'a flag', 'lie and cry', 4),
            ('rope jumping', 'jump', 'cant touch this!', 2),
            ('barbell curl', 'curl a barbell . . . duah!', 'one more! GO!', 1);
            ";

        string insertExerciseMuscles = @"
            INSERT INTO exercise_muscle (muscle_id, exercise_id) VALUES
            (1, 1), (2, 2), (3, 3);
            ";

        string insertExerciseTypes = @"
            INSERT INTO exercise_type (exercise_id, training_type_id) VALUES 
            (1, 1), (2, 2), (1, 3), (1, 4), (3, 3);
            ";

        context.Database.ExecuteSqlRaw(insertMuscles);
        context.Database.ExecuteSqlRaw(insertTrainingTypes);
        context.Database.ExecuteSqlRaw(insertExercises);
        context.Database.ExecuteSqlRaw(insertExerciseMuscles);
        context.Database.ExecuteSqlRaw(insertExerciseTypes);
    }

    /// <summary>
    /// Seeds the databse with <strong>3 Exercise recordss</strong> for the seeison's id given.<br></br>
    /// <strong>a session will have 3 <em>distinct</em> trainingSessionExerciseRecords</strong>.<br></br>
    /// this depends on <strong>SeedTypesExercisesAndMuscles</strong> to be called <strong>first!</strong>.
    /// </summary>
    /// <param name="sessionId">the session you want to create exercise record for.</param>
    /// <seealso cref="SeedTypesExercisesAndMuscles"/>
    public static void SeedWorkOutRecords(int sessionId, SqliteContext context)
    {
        string insertExerciseRecords1 = @"
                INSERT INTO exercise_record (exercise_id, repetitions, weight_used_kg) VALUES 
                (1, 20, 0), 
                (3, 20, 30);
                ";

        string insertExerciseRecords2 = @"
                INSERT INTO exercise_record (exercise_id, timer_in_seconds) VALUES 
                (2, 1800);
                ";

        string insertTrainingSessionExerciseRecords = @"
                INSERT INTO training_session_exercise_record (training_session_id, exercise_record_id, last_weight_used_kg) VALUES
                ({0}, 1, 0), 
                ({0}, 3, 30), 
                ({0}, 2, 0);
                ";
        string insertTrainingSessionTrainingTypes = @"
                INSERT INTO training_session_type (training_session_id, training_type_id) VALUES
                ({0}, 1), 
                ({0}, 2),
                ({0}, 3), 
                ({0}, 4); 
                ";
        context.Database.ExecuteSqlRaw(insertExerciseRecords1);
        context.Database.ExecuteSqlRaw(insertExerciseRecords2);
        context.Database.ExecuteSqlRaw(insertTrainingSessionExerciseRecords, sessionId);
        context.Database.ExecuteSqlRaw(insertTrainingSessionTrainingTypes, sessionId);
    }

    public static void SeedExtendedTypesExercisesAndMuscles(SqliteContext context)
    {
        string insertMuscles = @"
        INSERT INTO muscle (name, muscle_group, ""function"", wiki_page_url) VALUES
        ('deltoid anterior head','shoulders','flexes and medially rotates the arm.','https://en.wikipedia.org/wiki/Deltoid_muscle#Anterior_part'),
        ('deltoid posterior head','shoulders','flexes and medially rotates the arm.','https://en.wikipedia.org/wiki/Deltoid_muscle#Anterior_part'),
        ('deltoid middle head','shoulders','flexes and medially rotates the arm.','https://en.wikipedia.org/wiki/Deltoid_muscle#Anterior_part'),
        ('biceps brachii','arms','flexes the elbow.','https://en.wikipedia.org/wiki/Biceps_brachii'),
        ('triceps brachii','arms','extends the elbow.','https://en.wikipedia.org/wiki/Triceps_brachii'),
        ('rectus abdominis','core','flexes the lumbar spine.','https://en.wikipedia.org/wiki/Rectus_abdominis'),
        ('obliques','core','rotates the trunk.','https://en.wikipedia.org/wiki/Abdominal_external_oblique_muscle'),
        ('quadriceps','legs','extends the knee.','https://en.wikipedia.org/wiki/Quadriceps_femoris'),
        ('hamstrings','legs','flexes the knee.','https://en.wikipedia.org/wiki/Hamstring'),
        ('gastrocnemius','calves','plantar flexes the foot.','https://en.wikipedia.org/wiki/Gastrocnemius_muscle');
    ";

        string insertTrainingTypes = @"
        INSERT INTO training_type (name) VALUES 
        ('strength'), 
        ('cardio'),
        ('bodybuilding'), 
        ('martial arts'),
        ('calisthenics'),
        ('endurance'),
        ('flexibility');
    ";

        string insertExercises = @"
        INSERT INTO exercise (name, description, how_to, difficulty) VALUES
        ('dragon flag', 'a flag', 'lie and cry', 4),
        ('rope jumping', 'jump', 'cant touch this!', 2),
        ('barbell curl', 'curl a barbell . . . duah!', 'one more! GO!', 1),
        ('shoulder press', 'press overhead', 'press up', 3),
        ('push up', 'a basic push-up', 'push up from the ground', 2),
        ('pull up', 'a basic pull-up', 'pull up to the bar', 3),
        ('squat', 'basic squat', 'squat down and up', 3),
        ('deadlift', 'basic deadlift', 'lift the bar from the ground', 4),
        ('bench press', 'basic bench press', 'press the bar from your chest', 4),
        ('plank', 'hold a plank position', 'stay in plank position', 2);
    ";

        string insertExerciseMuscles = @"
        INSERT INTO exercise_muscle (muscle_id, exercise_id) VALUES
        (1, 1), (2, 2), (3, 3), (1, 4), (4, 5), (5, 6), (6, 7), (7, 8), (8, 9), (9, 10);
    ";

        string insertExerciseTypes = @"
        INSERT INTO exercise_type (exercise_id, training_type_id) VALUES 
        (1, 1), (1, 3), (1, 4),
        (2, 2), (2, 5), (2, 6),
        (3, 1), (3, 3), (3, 7),
        (4, 1), (4, 3), (4, 4),
        (5, 2), (5, 5), (5, 6),
        (6, 1), (6, 2), (6, 5),
        (7, 1), (7, 3), (7, 6),
        (8, 1), (8, 3), (8, 4),
        (9, 1), (9, 3), (9, 7),
        (10, 2), (10, 5), (10, 6);
    ";

        string insertExerciseHowTos = @"
        INSERT INTO exercise_how_to (exercise_id, name, url) VALUES
        (1, 'Tutorial 1', 'http://example.com/1'),
        (2, 'Tutorial 2', 'http://example.com/2'),
        (3, 'Tutorial 3', 'http://example.com/3'),
        (4, 'Tutorial 4', 'http://example.com/4'),
        (5, 'Tutorial 5', 'http://example.com/5'),
        (6, 'Tutorial 6', 'http://example.com/6'),
        (7, 'Tutorial 7', 'http://example.com/7'),
        (8, 'Tutorial 8', 'http://example.com/8'),
        (9, 'Tutorial 9', 'http://example.com/9'),
        (10, 'Tutorial 10', 'http://example.com/10');
    ";

        context.Database.ExecuteSqlRaw(insertMuscles);
        context.Database.ExecuteSqlRaw(insertTrainingTypes);
        context.Database.ExecuteSqlRaw(insertExercises);
        context.Database.ExecuteSqlRaw(insertExerciseMuscles);
        context.Database.ExecuteSqlRaw(insertExerciseTypes);
        context.Database.ExecuteSqlRaw(insertExerciseHowTos);
    }


}