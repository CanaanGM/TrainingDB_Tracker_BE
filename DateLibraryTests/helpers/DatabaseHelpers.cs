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
    public static void SeedDummyUsers(SqliteContext context)
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


    public static void SeedEnglishMuscles(SqliteContext context)
    {
        string createMuscle = "insert into muscle default values;";

        for (int i = 0; i <= 65; i++)
            context.Database.ExecuteSqlRaw(createMuscle);

        string createLocalizedMuscles = @"
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (1 , 1, 'adductor', 'adductor brevis', 'main function of the adductor brevis is to pull the thigh medially.', 'https://en.wikipedia.org/wiki/Adductor_brevis_muscle');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (2, 1, 'adductor', 'adductor longus', 'its main function is to adduct the thigh and it is innervated by the obturator nerve.', 'https://en.wikipedia.org/wiki/Adductor_longus_muscle');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (3 , 1, 'adductor', 'adductor magnus', 'the adductor magnus is a powerful adductor of the thigh, made especially active when the legs are moved from a wide spread position to one in which the legs parallel each other.', 'https://en.wikipedia.org/wiki/Adductor_magnus_muscle');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (4 , 1, 'back', 'erector spinae', 'extends and laterally flexes the spine.', 'https://en.wikipedia.org/wiki/Erector_spinae_muscles');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (5 , 1, 'back', 'infraspinatus', 'laterally rotates the arm and stabilizes the shoulder joint.', 'https://en.wikipedia.org/wiki/Infraspinatus_muscle');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (6 , 1, 'back', 'latissimus dorsi', 'extends, adducts, and medially rotates the arm.', 'https://en.wikipedia.org/wiki/Latissimus_dorsi_muscle');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (7 , 1, 'back', 'lower trapezius', 'depresses the scapula.', 'https://en.wikipedia.org/wiki/Trapezius_muscle');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (8 , 1, 'back', 'middle trapezius', 'retracts the scapula.', 'https://en.wikipedia.org/wiki/Trapezius_muscle');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (9 , 1, 'back', 'rhomboid', 'retracts and elevates the scapula.', 'https://en.wikipedia.org/wiki/Rhomboid_muscles');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (10 , 1, 'back', 'teres major', 'adducts and medially rotates the arm.', 'https://en.wikipedia.org/wiki/Teres_major_muscle');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (11 , 1, 'back', 'teres minor', 'laterally rotates the arm and stabilizes the shoulder joint.', 'https://en.wikipedia.org/wiki/Teres_minor_muscle');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (12 , 1, 'back', 'thoracolumbar fascia', 'load transfer between the trunk and limb (it is tensioned by the action of the latissimus dorsi muscle, gluteus maximus muscle, and the hamstring muscles), and lifting.', 'https://en.wikipedia.org/wiki/Thoracolumbar_fascia');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (13 , 1, 'back', 'upper trapezius', 'elevates the scapula and extends the head at the neck.', 'https://en.wikipedia.org/wiki/Trapezius_muscle');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (14 , 1, 'biceps', 'biceps brachii long head', 'flexes the elbow joint and supinates the forearm.', 'https://en.wikipedia.org/wiki/Biceps');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (15 , 1, 'biceps', 'biceps brachii short head', 'flexes the elbow joint and supinates the forearm.', 'https://en.wikipedia.org/wiki/Biceps');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (16 , 1, 'biceps', 'brachialis', 'mucle in the upper arm the flexes the elbow.', 'https://en.wikipedia.org/wiki/Brachialis_muscle');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (17 , 1, 'calves', 'gastrocnemius', 'plantar flexes the ankle and flexes the knee.', 'https://en.wikipedia.org/wiki/Gastrocnemius_muscle');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (18 , 1, 'calves', 'soleus', 'plantar flexes the ankle.', 'https://en.wikipedia.org/wiki/Soleus_muscle');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (19 , 1, 'chest', 'pectoralis major lower portion', 'flexes, adducts, and medially rotates the arm.', 'https://en.wikipedia.org/wiki/Pectoralis_major_muscle');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (20 , 1, 'chest', 'pectoralis major upper portion', 'flexes, adducts, and medially rotates the arm.', 'https://en.wikipedia.org/wiki/Pectoralis_major_muscle');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (21 , 1, 'chest', 'pectoralis minor', 'stabilizes the scapula by drawing it inferiorly and anteriorly against the thoracic wall.', 'https://en.wikipedia.org/wiki/Pectoralis_minor_muscle');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (22 , 1, 'core', 'external oblique', 'flexes the trunk, rotates the trunk, and compresses the abdomen.', 'https://en.wikipedia.org/wiki/External_oblique_muscle');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (23 , 1, 'core', 'internal oblique', 'flexes the trunk, rotates the trunk, and compresses the abdomen.', 'https://en.wikipedia.org/wiki/Internal_oblique_muscle');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (24 , 1, 'core', 'psoas major', 'flexion in the hip joint.', 'https://en.wikipedia.org/wiki/Psoas_major_muscle');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (25 , 1, 'core', 'rectus abdominis', 'flexes the trunk and compresses the abdomen.', 'https://en.wikipedia.org/wiki/Rectus_abdominis_muscle');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (26 , 1, 'core', 'transversus abdominis', 'compresses the abdomen.', 'https://en.wikipedia.org/wiki/Transversus_abdominis_muscle');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (27 , 1, 'forearms', 'brachioradialis', 'flexes the forearms at the elbow joint.', 'https://en.wikipedia.org/wiki/Brachioradialis_muscle');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (28 , 1, 'forearms', 'flexor carpi radialis', 'flexes and abducts the hand at the wrist.', 'https://en.wikipedia.org/wiki/Flexor_carpi_radialis_muscle');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (29 , 1, 'forearms', 'flexor carpi ulnaris', 'flexes and adducts the hand at the wrist.', 'https://en.wikipedia.org/wiki/Flexor_carpi_ulnaris_muscle');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (30 , 1, 'forearms', 'flexor digitorum profundus', 'flexes the fingers at the distal interphalangeal joints.', 'https://en.wikipedia.org/wiki/Flexor_digitorum_profundus_muscle');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (31 , 1, 'forearms', 'flexor digitorum superficialis', 'flexes the fingers at the proximal interphalangeal joints.', 'https://en.wikipedia.org/wiki/Flexor_digitorum_superficialis_muscle');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (32 , 1, 'forearms', 'pronator teres', 'pronates the forearm and flexes the elbow joint.', 'https://en.wikipedia.org/wiki/Pronator_teres_muscle');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (33 , 1, 'glutes', 'gluteus maximus', 'extends and laterally rotates the hip.', 'https://en.wikipedia.org/wiki/Gluteus_maximus_muscle');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (34 , 1, 'glutes', 'gluteus medius', 'abducts and medially rotates the hip.', 'https://en.wikipedia.org/wiki/Gluteus_medius_muscle');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (35 , 1, 'glutes', 'gluteus minimus', 'abducts and medially rotates the hip.', 'https://en.wikipedia.org/wiki/Gluteus_minimus_muscle');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (36 , 1, 'hamstrings', 'biceps femoris', 'flexes the knee joint and laterally rotates the leg.', 'https://en.wikipedia.org/wiki/Biceps_femoris_muscle');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (37 , 1, 'hamstrings', 'semimembranosus', 'flexes the knee joint and medially rotates the leg.', 'https://en.wikipedia.org/wiki/Semimembranosus_muscle');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (38 , 1, 'hamstrings', 'semitendinosus', 'flexes the knee joint and medially rotates the leg.', 'https://en.wikipedia.org/wiki/Semitendinosus_muscle');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (39 , 1, 'neck', 'levator scapulae', 'When the spine is fixed, levator scapulae elevates the scapula nad rotates it.', 'https://en.wikipedia.org/wiki/Levator_scapulae_muscle');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (40 , 1, 'neck', 'longus capitis', 'bilateral contraction - head flexion; unilateral contraction - head rotation.', 'https://en.wikipedia.org/wiki/Longus_capitis_muscle');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (41 , 1, 'neck', 'longus colli', 'forward and lateral flexion of the neck, as well as rotation of the neck.', 'https://en.wikipedia.org/wiki/Longus_colli_muscle');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (42 , 1, 'neck', 'omohyoid', 'depresses the hyoid bone and larynx.', 'https://en.wikipedia.org/wiki/Omohyoid_muscle');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (43 , 1, 'neck', 'scalenes', 'the anterior and middle scalene muscles lift the first rib and bend the neck to the same side as the acting muscle the posterior scalene lifts the second rib and tilts the neck to the same side.', 'https://en.wikipedia.org/wiki/Scalene_muscles');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (44 , 1, 'neck', 'sternocleidomastoid', 'flexes and laterally rotates the head.', 'https://en.wikipedia.org/wiki/Sternocleidomastoid_muscle');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (45 , 1, 'neck', 'sternohyoid', 'depresses the hyoid bone after it has been elevated during swallowing.', 'https://en.wikipedia.org/wiki/Sternohyoid_muscle');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (46 , 1, 'quadriceps', 'rectus femoris', 'flexes the hip and extends the knee.', 'https://en.wikipedia.org/wiki/Rectus_femoris_muscle');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (47 , 1, 'quadriceps', 'vastus intermedius', 'extends the knee.', 'https://en.wikipedia.org/wiki/Vastus_intermedius_muscle');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (48 , 1, 'quadriceps', 'vastus lateralis', 'extends the knee and stabilizes the patella.', 'https://en.wikipedia.org/wiki/Vastus_lateralis_muscle');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (49 , 1, 'quadriceps', 'vastus medialis', 'extends the knee and stabilizes the patella.', 'https://en.wikipedia.org/wiki/Vastus_medialis_muscle');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (50 , 1, 'shins', 'fibularis brevis', 'It acts to tilt the sole of the foot away from the midline of the body (eversion) and to extend the foot downward away from the body at the ankle (plantar flexion).', 'https://en.wikipedia.org/wiki/Fibularis_brevis');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (51 , 1, 'shins', 'fibularis longus ', 'It acts to tilt the sole of the foot away from the midline of the body (eversion) and to extend the foot downward away from the body (plantar flexion) at the ankle. ', 'https://en.wikipedia.org/wiki/Fibularis_longus');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (52 , 1, 'shins', 'fibularis tertius ', 'It acts to tilt the sole of the foot away from the midline of the body (eversion) and to pull the foot upward toward the body (dorsiflexion)', 'https://en.wikipedia.org/wiki/Fibularis_tertius');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (53 , 1, 'shins', 'tibialis anterior', 'The muscle helps maintain the medial longitudinal arch of the foot.[2] It draws up and holds the toe in a locked position. The tibialis anterior aids in any activity that requires moving the leg or keeping the leg vertical. It functions to stabilize the ankle as the foot hits the ground during the contact phase of walking (eccentric contraction) and acts later to pull the foot clear of the ground during the swing phase (concentric contraction). It also functions to ''lock'' the ankle, as in toe-kicking a ball, when held in an isometric contraction.', 'https://en.wikipedia.org/wiki/Tibialis_anterior_muscle');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (54 , 1, 'shoulders', 'deltoid anterior head', 'flexes and medially rotates the arm.', 'https://en.wikipedia.org/wiki/Deltoid_muscle#Anterior_part');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (55 , 1, 'shoulders', 'deltoid middle head', 'abducts the arm.', 'https://en.wikipedia.org/wiki/Deltoid_muscle#Middle_part');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (56 , 1, 'shoulders', 'deltoid posterior head', 'extends and laterally rotates the arm.', 'https://en.wikipedia.org/wiki/Deltoid_muscle#Posterior_part');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (57 , 1, 'thigh', 'external obturator', 'lateral rotator of the hip joint. as a short muscle around the hip joint, it stabilizes the hip joint as a postural muscle.', 'https://en.wikipedia.org/wiki/External_obturator_muscle');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (58 , 1, 'thigh', 'gracilis', 'the muscle adducts, medially rotates (with hip flexion), laterally rotates, and flexes the hip as above, and also aids in flexion of the knee.', 'https://en.wikipedia.org/wiki/Gracilis_muscle');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (59 , 1, 'thigh', 'iliacus', 'In open-chain exercises, as part of the iliopsoas, the iliacus is important for lifting (flexing) the femur forward (e.g. front scale). In closed-chain exercises, the iliopsoas bends the trunk forward and can lift the trunk from a lying posture.', 'https://en.wikipedia.org/wiki/Iliacus_muscle');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (60 , 1, 'thigh', 'pectineus', 'hip flection and adduction.', 'https://en.wikipedia.org/wiki/Pectineus_muscle');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (61 , 1, 'thigh', 'sartorius', 'can move the hip joint and the knee joint.', 'https://en.wikipedia.org/wiki/Sartorius_muscle');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (62 , 1, 'thigh', 'tensor fasciae latae', 'stabilize the hip in extension (assists gluteus maximus during hip extension).', 'https://en.wikipedia.org/wiki/Tensor_fasciae_latae_muscle');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (63 , 1, 'triceps', 'triceps brachii lateral head', 'used for movements requiring occasional high-intensity force.', 'https://en.wikipedia.org/wiki/Triceps');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (64 , 1, 'triceps', 'triceps brachii long head', 'employed when sustained force generation is demanded, or when there is a need for a synergistic control of the shoulder and elbow or both.', 'https://en.wikipedia.org/wiki/Triceps');
            INSERT INTO localized_muscle (muscle_id, language_id, muscle_group, name, function, wiki_page_url) VALUES (65 , 1, 'triceps', 'triceps brachii medial head', 'enables more precise, low-force movements.', 'https://en.wikipedia.org/wiki/Triceps');
        ";
        context.Database.ExecuteSqlRaw(createLocalizedMuscles);
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