using DataLibrary.Context;
using DataLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace DateLibraryTests.helpers;

public static class DatabaseHelpers
{
    /// <summary>
    /// seeds the database with 3 languages : 1: _english_, 2: _arabic_, 3: japanese
    /// </summary>
    /// <param name="context"></param>
    public static void SeedLanguages(SqliteContext context)
    {
        // Ensure we do not duplicate entries if already seeded
        if (context.Languages.Any()) return;
        string insertLanguages = @"
                INSERT INTO language (id, code, name) VALUES
                (1, 'en', 'english'),
                (2, 'ar', 'arabic'),
                (3, 'jp', 'japanese');
            ";
        context.Database.ExecuteSqlRaw(insertLanguages);
        context.SaveChanges();
    }
    
    /// <summary>
    /// seeds the database with 3 equipment for each language, so obviously this depends on <see cref="SeedLanguages"/>
    /// </summary>
    /// <param name="context"></param>
    public static void SeedEquipmentAndAssociateThemWithLanguages(SqliteContext context)
    {
        // Ensure we do not duplicate entries if already seeded
        string insertEquipment = @"
        INSERT INTO equipment (id, weight_kg) VALUES
        (1, 10.0),
        (2, 15.5),
        (3, 20.0);
    ";

        // Seed localized equipment
        string insertLocalizedEquipment = @"
        INSERT INTO localized_equipment (equipment_id, language_id, name, description, how_to) VALUES
        (1, 1, 'dumbbell', 'A short barbell with a fixed weight.', 'Use for weight training and fitness.'),
        (1, 2, 'دمبل', 'شريط قصير بوزن ثابت.', 'استخدم لتدريبات الوزن واللياقة.'),
        (1, 3, 'ダンベル', '固定重量の短いバーベルです。', 'ウェイトトレーニングとフィットネスに使用します。'),

        (2, 1, 'kettlebell', 'A cast iron ball with a handle.', 'Used for swing exercises and strength training.'),
        (2, 2, 'كرة الحديد', 'كرة من الحديد الزهر مع مقبض.', 'تستخدم لتمارين السوينغ وتدريب القوة.'),
        (2, 3, 'ケトルベル', 'ハンドル付きの鋳鉄球。', 'スイングエクササイズと力トレーニングに使用されます。'),

        (3, 1, 'barbell', 'A long bar with weights at each end.', 'Utilized for bench press, squats, and deadlifts.'),
        (3, 2, 'الباربل', 'عارضة طويلة بأوزان على كل طرف.', 'يستخدم لتمارين البنش برس والقرفصاء والرفعة المميتة.'),
        (3, 3, 'バーベル', '両端に重りのある長いバー。', 'ベンチプレス、スクワット、デッドリフトに利用されます。');
    ";
        context.Database.ExecuteSqlRaw(insertEquipment);
        context.Database.ExecuteSqlRaw(insertLocalizedEquipment);

        context.SaveChanges();
    }

    /// <summary>
    /// creates 4 new users and assigns hashed passwords and salts.
    /// 1. Canaan, `كنعان لازم يتدرب !`,  salt:`wnGHc9SponguR0Givr2Zcvy7UI3Szs0y0lYfAwdatUE=`, hash:`iUXNkjg5RS/0/uEH8f6tasaKKKt8IVsZgTTtyGH3dmQ=`
    /// 2. Dante, `pizza is pizza!`, salt:`UqfeVl+kUvwURJ2Avx5nw2+qTBMKUUdrHE5m48RGFU0=`, hash:`BzMDx05wo6wjTriEEjxDsb9jZTa6QkbeFAr7B46CAwc=`
    /// 3. Alphrad, `sneaky snake`, salt:`ROtBSjEmAd8swaKpy0e7yV6n5IJl+K5i6yF/brfjTD0=`, hash:`wA+0773CBog+MlcEjrNcAmHS6pSd06ceminYzPU4wlI=`
    /// 4. Nero, `ろまである!`, salt:`a5MaAfB/7zpN8dd3+i10RgHGJ455lpoSIOrAXXXQbyY=`, hash:`Kw1vjlzykwUqruCdhzdMOQBuOkn4hljD3JLiOdAogWg=` 
    /// </summary>
    /// <param name="context"></param>
    public static void SeesDummyUsers(SqliteContext context)
    {
        string insertUsers = @"
       insert into user (username, email, height, gender) VALUES
            ('Canaan', 'canaan@test.com', 173, 'M'),
            ('Dante', 'dante@test.com', 200, 'M'),
            ('Alphrad', 'alphrad@test.com', 172, 'F'),
            ('Nero', 'nero@test.com', 156, 'F');      
            ";

        // other wise i need a loop, so noh!
        string insertUserPassword = @"
        insert into user_passwords(user_id, password_hash, password_salt)
        VALUES
            (1, 'iUXNkjg5RS/0/uEH8f6tasaKKKt8IVsZgTTtyGH3dmQ=', 'wnGHc9SponguR0Givr2Zcvy7UI3Szs0y0lYfAwdatUE='),
            (2, 'BzMDx05wo6wjTriEEjxDsb9jZTa6QkbeFAr7B46CAwc=', 'UqfeVl+kUvwURJ2Avx5nw2+qTBMKUUdrHE5m48RGFU0='),
            (3, 'wA+0773CBog+MlcEjrNcAmHS6pSd06ceminYzPU4wlI=', 'ROtBSjEmAd8swaKpy0e7yV6n5IJl+K5i6yF/brfjTD0='),
            (4, 'Kw1vjlzykwUqruCdhzdMOQBuOkn4hljD3JLiOdAogWg=', 'a5MaAfB/7zpN8dd3+i10RgHGJ455lpoSIOrAXXXQbyY=');";

        context.Database.ExecuteSqlRaw(insertUsers);
        context.Database.ExecuteSqlRaw(insertUserPassword);
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