using DataLibrary.Context;
using DataLibrary.Models;

namespace DateLibraryTests.helpers;

public static class TestData
{
    public static void SeedDatabaseTrainingSessions(this SqliteContext context, int sessionNumber = 10)
    {
        // context.AddRange(CreateTrainingSessions(sessionNumber));
        context.SaveChanges();
    }



    public static void SeedMuscles(this SqliteContext context)
    {
        context.Muscles.AddRange(new List<Muscle> {
      new Muscle{
        Name= "upper trapezius",
        MuscleGroup= "back",
        Function= "elevates the scapula and extends the head at the neck.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Trapezius_muscle"
      },
      new Muscle{
        Name= "middle trapezius",
        MuscleGroup= "back",
        Function= "retracts the scapula.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Trapezius_muscle"
      },
      new Muscle{
        Name= "lower trapezius",
        MuscleGroup= "back",
        Function= "depresses the scapula.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Trapezius_muscle"
      },
      new Muscle{
        Name= "latissimus dorsi",
        MuscleGroup= "back",
        Function= "extends, adducts, and medially rotates the arm.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Latissimus_dorsi_muscle"
      },
      new Muscle{
        Name= "rhomboid",
        MuscleGroup= "back",
        Function= "retracts and elevates the scapula.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Rhomboid_muscles"
      },
      new Muscle{
        Name= "erector spinae",
        MuscleGroup= "back",
        Function= "extends and laterally flexes the spine.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Erector_spinae_muscles"
      },
      new Muscle{
        Name= "infraspinatus",
        MuscleGroup= "back",
        Function= "laterally rotates the arm and stabilizes the shoulder joint.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Infraspinatus_muscle"
      },
      new Muscle{
        Name= "teres major",
        MuscleGroup= "back",
        Function= "adducts and medially rotates the arm.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Teres_major_muscle"
      },
      new Muscle{
        Name= "teres minor",
        MuscleGroup= "back",
        Function= "laterally rotates the arm and stabilizes the shoulder joint.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Teres_minor_muscle"
      },
      new Muscle{
        Name= "thoracolumbar fascia",
        MuscleGroup= "back",
        Function= "load transfer between the trunk and limb (it is tensioned by the action of the latissimus dorsi muscle, gluteus maximus muscle, and the hamstring muscles), and lifting.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Thoracolumbar_fascia"
      },
      new Muscle{
        Name= "sternocleidomastoid",
        MuscleGroup= "neck",
        Function= "flexes and laterally rotates the head.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Sternocleidomastoid_muscle"
      },
      new Muscle{
        Name= "omohyoid",
        MuscleGroup= "neck",
        Function= "depresses the hyoid bone and larynx.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Omohyoid_muscle"
      },
      new Muscle{
        Name= "sternohyoid",
        MuscleGroup= "neck",
        Function= "depresses the hyoid bone after it has been elevated during swallowing.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Sternohyoid_muscle"
      },
      new Muscle{
        Name= "longus colli",
        MuscleGroup= "neck",
        Function= "forward and lateral flexion of the neck, as well as rotation of the neck.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Longus_colli_muscle"
      },
      new Muscle{
        Name= "longus capitis",
        MuscleGroup= "neck",
        Function= "bilateral contraction - head flexion; unilateral contraction - head rotation.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Longus_capitis_muscle"
      },
      new Muscle{
        Name= "scalenes",
        MuscleGroup= "neck",
        Function= "the anterior and middle scalene muscles lift the first rib and bend the neck to the same side as the acting muscle the posterior scalene lifts the second rib and tilts the neck to the same side.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Scalene_muscles"
      },
      new Muscle{
        Name= "pectoralis major upper portion",
        MuscleGroup= "chest",
        Function= "flexes, adducts, and medially rotates the arm.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Pectoralis_major_muscle"
      },
      new Muscle{
        Name= "pectoralis major lower portion",
        MuscleGroup= "chest",
        Function= "flexes, adducts, and medially rotates the arm.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Pectoralis_major_muscle"
      },
      new Muscle{
        Name= "pectoralis minor",
        MuscleGroup= "chest",
        Function= "stabilizes the scapula by drawing it inferiorly and anteriorly against the thoracic wall.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Pectoralis_minor_muscle"
      },
      new Muscle{
        Name= "deltoid anterior head",
        MuscleGroup= "shoulders",
        Function= "flexes and medially rotates the arm.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Deltoid_muscle#Anterior_part"
      },
      new Muscle{
        Name= "deltoid posterior head",
        MuscleGroup= "shoulders",
        Function= "extends and laterally rotates the arm.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Deltoid_muscle#Posterior_part"
      },
      new Muscle{
        Name= "deltoid middle head",
        MuscleGroup= "shoulders",
        Function= "abducts the arm.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Deltoid_muscle#Middle_part"
      },
      new Muscle{
        Name= "biceps brachii short head",
        MuscleGroup= "biceps",
        Function= "flexes the elbow joint and supinates the forearm.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Biceps"
      },
      new Muscle{
        Name= "biceps brachii long head",
        MuscleGroup= "biceps",
        Function= "flexes the elbow joint and supinates the forearm.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Biceps"
      },
      new Muscle{
        Name= "brachialis",
        MuscleGroup= "biceps",
        Function= "mucle in the upper arm the flexes the elbow.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Brachialis_muscle"
      },
      new Muscle{
        Name= "triceps brachii long head",
        MuscleGroup= "triceps",
        Function= "employed when sustained force generation is demanded, or when there is a need for a synergistic control of the shoulder and elbow or both.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Triceps"
      },
      new Muscle{
        Name= "triceps brachii lateral head",
        MuscleGroup= "triceps",
        Function= "used for movements requiring occasional high-intensity force.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Triceps"
      },
      new Muscle{
        Name= "triceps brachii medial head",
        MuscleGroup= "triceps",
        Function= "enables more precise, low-force movements.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Triceps"
      },
      new Muscle{
        Name= "brachioradialis",
        MuscleGroup= "forearms",
        Function= "flexes the forearms at the elbow joint.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Brachioradialis_muscle"
      },
      new Muscle{
        Name= "pronator teres",
        MuscleGroup= "forearms",
        Function= "pronates the forearm and flexes the elbow joint.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Pronator_teres_muscle"
      },
      new Muscle{
        Name= "flexor carpi ulnaris",
        MuscleGroup= "forearms",
        Function= "flexes and adducts the hand at the wrist.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Flexor_carpi_ulnaris_muscle"
      },
      new Muscle{
        Name= "flexor carpi radialis",
        MuscleGroup= "forearms",
        Function= "flexes and abducts the hand at the wrist.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Flexor_carpi_radialis_muscle"
      },
      new Muscle{
        Name= "flexor digitorum superficialis",
        MuscleGroup= "forearms",
        Function= "flexes the fingers at the proximal interphalangeal joints.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Flexor_digitorum_superficialis_muscle"
      },
      new Muscle{
        Name= "flexor digitorum profundus",
        MuscleGroup= "forearms",
        Function= "flexes the fingers at the distal interphalangeal joints.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Flexor_digitorum_profundus_muscle"
      },
      new Muscle{
        Name= "biceps femoris",
        MuscleGroup= "hamstrings",
        Function= "flexes the knee joint and laterally rotates the leg.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Biceps_femoris_muscle"
      },
      new Muscle{
        Name= "semimembranosus",
        MuscleGroup= "hamstrings",
        Function= "flexes the knee joint and medially rotates the leg.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Semimembranosus_muscle"
      },
      new Muscle{
        Name= "semitendinosus",
        MuscleGroup= "hamstrings",
        Function= "flexes the knee joint and medially rotates the leg.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Semitendinosus_muscle"
      },
      new Muscle{
        Name= "rectus femoris",
        MuscleGroup= "quadriceps",
        Function= "flexes the hip and extends the knee.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Rectus_femoris_muscle"
      },
      new Muscle{
        Name= "vastus lateralis",
        MuscleGroup= "quadriceps",
        Function= "extends the knee and stabilizes the patella.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Vastus_lateralis_muscle"
      },
      new Muscle{
        Name= "vastus medialis",
        MuscleGroup= "quadriceps",
        Function= "extends the knee and stabilizes the patella.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Vastus_medialis_muscle"
      },
      new Muscle{
        Name= "vastus intermedius",
        MuscleGroup= "quadriceps",
        Function= "extends the knee.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Vastus_intermedius_muscle"
      },
      new Muscle{
        Name= "gluteus maximus",
        MuscleGroup= "glutes",
        Function= "extends and laterally rotates the hip.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Gluteus_maximus_muscle"
      },
      new Muscle{
        Name= "gluteus medius",
        MuscleGroup= "glutes",
        Function= "abducts and medially rotates the hip.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Gluteus_medius_muscle"
      },
      new Muscle{
        Name= "gluteus minimus",
        MuscleGroup= "glutes",
        Function= "abducts and medially rotates the hip.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Gluteus_minimus_muscle"
      },
      new Muscle{
        Name= "rectus abdominis",
        MuscleGroup= "core",
        Function= "flexes the trunk and compresses the abdomen.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Rectus_abdominis_muscle"
      },
      new Muscle{
        Name= "external oblique",
        MuscleGroup= "core",
        Function= "flexes the trunk, rotates the trunk, and compresses the abdomen.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/External_oblique_muscle"
      },
      new Muscle{
        Name= "internal oblique",
        MuscleGroup= "core",
        Function= "flexes the trunk, rotates the trunk, and compresses the abdomen.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Internal_oblique_muscle"
      },
      new Muscle{
        Name= "transversus abdominis",
        MuscleGroup= "core",
        Function= "compresses the abdomen.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Transversus_abdominis_muscle"
      },
      new Muscle{
        Name= "gastrocnemius",
        MuscleGroup= "calves",
        Function= "plantar flexes the ankle and flexes the knee.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Gastrocnemius_muscle"
      },
      new Muscle{
        Name= "soleus",
        MuscleGroup= "calves",
        Function= "plantar flexes the ankle.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Soleus_muscle"
      },
      new Muscle{
        Name= "tibialis anterior",
        MuscleGroup= "lower legs",
        Function= "dorsiflexes and inverts the foot.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Tibialis_anterior_muscle"
      },
      new Muscle{
        Name= "peroneus longus",
        MuscleGroup= "lower legs",
        Function= "plantar flexes and everts the foot.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Peroneus_longus_muscle"
      },
      new Muscle{
        Name= "peroneus brevis",
        MuscleGroup= "lower legs",
        Function= "plantar flexes and everts the foot.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Peroneus_brevis_muscle"
      },
      new Muscle{
        Name= "adductor longus",
        MuscleGroup= "adductor",
        Function= "its main function is to adduct the thigh and it is innervated by the obturator nerve.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Adductor_longus_muscle"
      },
      new Muscle{
        Name= "adductor brevis",
        MuscleGroup= "adductor",
        Function= "main function of the adductor brevis is to pull the thigh medially.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Adductor_brevis_muscle"
      },
      new Muscle{
        Name= "adductor magnus",
        MuscleGroup= "adductor",
        Function= "the adductor magnus is a powerful adductor of the thigh, made especially active when the legs are moved from a wide spread position to one in which the legs parallel each other.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Adductor_magnus_muscle"
      },
      new Muscle{
        Name= "sartorius",
        MuscleGroup= "thigh",
        Function= "can move the hip joint and the knee joint.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Sartorius_muscle"
      },
      new Muscle{
        Name= "pectineus",
        MuscleGroup= "thigh",
        Function= "hip flection and adduction.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Pectineus_muscle"
      },
      new Muscle{
        Name= "external obturator",
        MuscleGroup= "thigh",
        Function= "lateral rotator of the hip joint. as a short muscle around the hip joint, it stabilizes the hip joint as a postural muscle.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/External_obturator_muscle"
      },
      new Muscle{
        Name= "gracilis",
        MuscleGroup= "thigh",
        Function= "the muscle adducts, medially rotates (with hip flexion), laterally rotates, and flexes the hip as above, and also aids in flexion of the knee.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Gracilis_muscle"
      },
      new Muscle{
        Name= "psoas major",
        MuscleGroup= "core",
        Function= "flexion in the hip joint.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Psoas_major_muscle"
      },
      new Muscle{
        Name= "iliacus",
        MuscleGroup= "thigh",
        Function= "In open-chain exercises, as part of the iliopsoas, the iliacus is important for lifting (flexing) the femur forward (e.g. front scale). In closed-chain exercises, the iliopsoas bends the trunk forward and can lift the trunk from a lying posture.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Iliacus_muscle"
      },
      new Muscle{
        Name= "tensor fasciae latae",
        MuscleGroup= "thigh",
        Function= "stabilize the hip in extension (assists gluteus maximus during hip extension).",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Tensor_fasciae_latae_muscle"
      }

     });
    }

    public static void SeedTrainingTypes(this SqliteContext context)
    {
        var trainingTypeList = new List<TrainingType>{
      new TrainingType{
        Name= "bodybuilding"
      },
      new TrainingType{
        Name= "strength"
      },
      new TrainingType{
        Name= "yoga"
      },
      new TrainingType{
        Name= "athletics"
      },
      new TrainingType{
        Name= "karate"
      },
      new TrainingType{
        Name= "stretch"
      },
      new TrainingType{
        Name= "stability"
      },
      new TrainingType{
        Name= "cardiovascular"
      },
      new TrainingType{
        Name= "agility"
      },
      new TrainingType{
        Name= "endurance"
      },
      new TrainingType{
        Name= "rehabilitation"
      },
      new TrainingType{
        Name= "mobility"
      },
      new TrainingType{
        Name= "power"
      },
      new TrainingType{
        Name= "speed"
      },
      new TrainingType{
        Name= "flexibility"
      },
      new TrainingType{
        Name= "calisthenics"
      },
      new TrainingType{
        Name= "coordination"
      }
    };
        context.TrainingTypes.AddRange(trainingTypeList);
    }

    public static void SeedExercises(this SqliteContext context)
    {
        var trainingTypeList = new List<TrainingType>{
      new TrainingType{
        Name= "bodybuilding"
      },
      new TrainingType{
        Name= "strength"
      },
      new TrainingType{
        Name= "yoga"
      },
      new TrainingType{
        Name= "athletics"
      },
      new TrainingType{
        Name= "karate"
      },
      new TrainingType{
        Name= "stretch"
      },
      new TrainingType{
        Name= "stability"
      },
      new TrainingType{
        Name= "cardiovascular"
      },
      new TrainingType{
        Name= "agility"
      },
      new TrainingType{
        Name= "endurance"
      },
      new TrainingType{
        Name= "rehabilitation"
      },
      new TrainingType{
        Name= "mobility"
      },
      new TrainingType{
        Name= "power"
      },
      new TrainingType{
        Name= "speed"
      },
      new TrainingType{
        Name= "flexibility"
      },
      new TrainingType{
        Name= "calisthenics"
      },
      new TrainingType{
        Name= "coordination"
      }
    };
        var muscles = new List<Muscle> {
      new Muscle{
        Name= "upper trapezius",
        MuscleGroup= "back",
        Function= "elevates the scapula and extends the head at the neck.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Trapezius_muscle"
      },
      new Muscle{
        Name= "middle trapezius",
        MuscleGroup= "back",
        Function= "retracts the scapula.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Trapezius_muscle"
      },
      new Muscle{
        Name= "lower trapezius",
        MuscleGroup= "back",
        Function= "depresses the scapula.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Trapezius_muscle"
      },
      new Muscle{
        Name= "latissimus dorsi",
        MuscleGroup= "back",
        Function= "extends, adducts, and medially rotates the arm.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Latissimus_dorsi_muscle"
      },
      new Muscle{
        Name= "rhomboid",
        MuscleGroup= "back",
        Function= "retracts and elevates the scapula.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Rhomboid_muscles"
      },
      new Muscle{
        Name= "erector spinae",
        MuscleGroup= "back",
        Function= "extends and laterally flexes the spine.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Erector_spinae_muscles"
      },
      new Muscle{
        Name= "infraspinatus",
        MuscleGroup= "back",
        Function= "laterally rotates the arm and stabilizes the shoulder joint.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Infraspinatus_muscle"
      },
      new Muscle{
        Name= "teres major",
        MuscleGroup= "back",
        Function= "adducts and medially rotates the arm.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Teres_major_muscle"
      },
      new Muscle{
        Name= "teres minor",
        MuscleGroup= "back",
        Function= "laterally rotates the arm and stabilizes the shoulder joint.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Teres_minor_muscle"
      },
      new Muscle{
        Name= "thoracolumbar fascia",
        MuscleGroup= "back",
        Function= "load transfer between the trunk and limb (it is tensioned by the action of the latissimus dorsi muscle, gluteus maximus muscle, and the hamstring muscles), and lifting.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Thoracolumbar_fascia"
      },
      new Muscle{
        Name= "sternocleidomastoid",
        MuscleGroup= "neck",
        Function= "flexes and laterally rotates the head.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Sternocleidomastoid_muscle"
      },
      new Muscle{
        Name= "omohyoid",
        MuscleGroup= "neck",
        Function= "depresses the hyoid bone and larynx.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Omohyoid_muscle"
      },
      new Muscle{
        Name= "sternohyoid",
        MuscleGroup= "neck",
        Function= "depresses the hyoid bone after it has been elevated during swallowing.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Sternohyoid_muscle"
      },
      new Muscle{
        Name= "longus colli",
        MuscleGroup= "neck",
        Function= "forward and lateral flexion of the neck, as well as rotation of the neck.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Longus_colli_muscle"
      },
      new Muscle{
        Name= "longus capitis",
        MuscleGroup= "neck",
        Function= "bilateral contraction - head flexion; unilateral contraction - head rotation.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Longus_capitis_muscle"
      },
      new Muscle{
        Name= "scalenes",
        MuscleGroup= "neck",
        Function= "the anterior and middle scalene muscles lift the first rib and bend the neck to the same side as the acting muscle the posterior scalene lifts the second rib and tilts the neck to the same side.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Scalene_muscles"
      },
      new Muscle{
        Name= "pectoralis major upper portion",
        MuscleGroup= "chest",
        Function= "flexes, adducts, and medially rotates the arm.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Pectoralis_major_muscle"
      },
      new Muscle{
        Name= "pectoralis major lower portion",
        MuscleGroup= "chest",
        Function= "flexes, adducts, and medially rotates the arm.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Pectoralis_major_muscle"
      },
      new Muscle{
        Name= "pectoralis minor",
        MuscleGroup= "chest",
        Function= "stabilizes the scapula by drawing it inferiorly and anteriorly against the thoracic wall.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Pectoralis_minor_muscle"
      },
      new Muscle{
        Name= "deltoid anterior head",
        MuscleGroup= "shoulders",
        Function= "flexes and medially rotates the arm.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Deltoid_muscle#Anterior_part"
      },
      new Muscle{
        Name= "deltoid posterior head",
        MuscleGroup= "shoulders",
        Function= "extends and laterally rotates the arm.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Deltoid_muscle#Posterior_part"
      },
      new Muscle{
        Name= "deltoid middle head",
        MuscleGroup= "shoulders",
        Function= "abducts the arm.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Deltoid_muscle#Middle_part"
      },
      new Muscle{
        Name= "biceps brachii short head",
        MuscleGroup= "biceps",
        Function= "flexes the elbow joint and supinates the forearm.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Biceps"
      },
      new Muscle{
        Name= "biceps brachii long head",
        MuscleGroup= "biceps",
        Function= "flexes the elbow joint and supinates the forearm.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Biceps"
      },
      new Muscle{
        Name= "brachialis",
        MuscleGroup= "biceps",
        Function= "mucle in the upper arm the flexes the elbow.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Brachialis_muscle"
      },
      new Muscle{
        Name= "triceps brachii long head",
        MuscleGroup= "triceps",
        Function= "employed when sustained force generation is demanded, or when there is a need for a synergistic control of the shoulder and elbow or both.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Triceps"
      },
      new Muscle{
        Name= "triceps brachii lateral head",
        MuscleGroup= "triceps",
        Function= "used for movements requiring occasional high-intensity force.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Triceps"
      },
      new Muscle{
        Name= "triceps brachii medial head",
        MuscleGroup= "triceps",
        Function= "enables more precise, low-force movements.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Triceps"
      },
      new Muscle{
        Name= "brachioradialis",
        MuscleGroup= "forearms",
        Function= "flexes the forearms at the elbow joint.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Brachioradialis_muscle"
      },
      new Muscle{
        Name= "pronator teres",
        MuscleGroup= "forearms",
        Function= "pronates the forearm and flexes the elbow joint.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Pronator_teres_muscle"
      },
      new Muscle{
        Name= "flexor carpi ulnaris",
        MuscleGroup= "forearms",
        Function= "flexes and adducts the hand at the wrist.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Flexor_carpi_ulnaris_muscle"
      },
      new Muscle{
        Name= "flexor carpi radialis",
        MuscleGroup= "forearms",
        Function= "flexes and abducts the hand at the wrist.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Flexor_carpi_radialis_muscle"
      },
      new Muscle{
        Name= "flexor digitorum superficialis",
        MuscleGroup= "forearms",
        Function= "flexes the fingers at the proximal interphalangeal joints.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Flexor_digitorum_superficialis_muscle"
      },
      new Muscle{
        Name= "flexor digitorum profundus",
        MuscleGroup= "forearms",
        Function= "flexes the fingers at the distal interphalangeal joints.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Flexor_digitorum_profundus_muscle"
      },
      new Muscle{
        Name= "biceps femoris",
        MuscleGroup= "hamstrings",
        Function= "flexes the knee joint and laterally rotates the leg.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Biceps_femoris_muscle"
      },
      new Muscle{
        Name= "semimembranosus",
        MuscleGroup= "hamstrings",
        Function= "flexes the knee joint and medially rotates the leg.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Semimembranosus_muscle"
      },
      new Muscle{
        Name= "semitendinosus",
        MuscleGroup= "hamstrings",
        Function= "flexes the knee joint and medially rotates the leg.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Semitendinosus_muscle"
      },
      new Muscle{
        Name= "rectus femoris",
        MuscleGroup= "quadriceps",
        Function= "flexes the hip and extends the knee.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Rectus_femoris_muscle"
      },
      new Muscle{
        Name= "vastus lateralis",
        MuscleGroup= "quadriceps",
        Function= "extends the knee and stabilizes the patella.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Vastus_lateralis_muscle"
      },
      new Muscle{
        Name= "vastus medialis",
        MuscleGroup= "quadriceps",
        Function= "extends the knee and stabilizes the patella.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Vastus_medialis_muscle"
      },
      new Muscle{
        Name= "vastus intermedius",
        MuscleGroup= "quadriceps",
        Function= "extends the knee.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Vastus_intermedius_muscle"
      },
      new Muscle{
        Name= "gluteus maximus",
        MuscleGroup= "glutes",
        Function= "extends and laterally rotates the hip.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Gluteus_maximus_muscle"
      },
      new Muscle{
        Name= "gluteus medius",
        MuscleGroup= "glutes",
        Function= "abducts and medially rotates the hip.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Gluteus_medius_muscle"
      },
      new Muscle{
        Name= "gluteus minimus",
        MuscleGroup= "glutes",
        Function= "abducts and medially rotates the hip.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Gluteus_minimus_muscle"
      },
      new Muscle{
        Name= "rectus abdominis",
        MuscleGroup= "core",
        Function= "flexes the trunk and compresses the abdomen.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Rectus_abdominis_muscle"
      },
      new Muscle{
        Name= "external oblique",
        MuscleGroup= "core",
        Function= "flexes the trunk, rotates the trunk, and compresses the abdomen.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/External_oblique_muscle"
      },
      new Muscle{
        Name= "internal oblique",
        MuscleGroup= "core",
        Function= "flexes the trunk, rotates the trunk, and compresses the abdomen.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Internal_oblique_muscle"
      },
      new Muscle{
        Name= "transversus abdominis",
        MuscleGroup= "core",
        Function= "compresses the abdomen.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Transversus_abdominis_muscle"
      },
      new Muscle{
        Name= "gastrocnemius",
        MuscleGroup= "calves",
        Function= "plantar flexes the ankle and flexes the knee.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Gastrocnemius_muscle"
      },
      new Muscle{
        Name= "soleus",
        MuscleGroup= "calves",
        Function= "plantar flexes the ankle.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Soleus_muscle"
      },
      new Muscle{
        Name= "tibialis anterior",
        MuscleGroup= "lower legs",
        Function= "dorsiflexes and inverts the foot.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Tibialis_anterior_muscle"
      },
      new Muscle{
        Name= "peroneus longus",
        MuscleGroup= "lower legs",
        Function= "plantar flexes and everts the foot.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Peroneus_longus_muscle"
      },
      new Muscle{
        Name= "peroneus brevis",
        MuscleGroup= "lower legs",
        Function= "plantar flexes and everts the foot.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Peroneus_brevis_muscle"
      },
      new Muscle{
        Name= "adductor longus",
        MuscleGroup= "adductor",
        Function= "its main function is to adduct the thigh and it is innervated by the obturator nerve.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Adductor_longus_muscle"
      },
      new Muscle{
        Name= "adductor brevis",
        MuscleGroup= "adductor",
        Function= "main function of the adductor brevis is to pull the thigh medially.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Adductor_brevis_muscle"
      },
      new Muscle{
        Name= "adductor magnus",
        MuscleGroup= "adductor",
        Function= "the adductor magnus is a powerful adductor of the thigh, made especially active when the legs are moved from a wide spread position to one in which the legs parallel each other.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Adductor_magnus_muscle"
      },
      new Muscle{
        Name= "sartorius",
        MuscleGroup= "thigh",
        Function= "can move the hip joint and the knee joint.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Sartorius_muscle"
      },
      new Muscle{
        Name= "pectineus",
        MuscleGroup= "thigh",
        Function= "hip flection and adduction.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Pectineus_muscle"
      },
      new Muscle{
        Name= "external obturator",
        MuscleGroup= "thigh",
        Function= "lateral rotator of the hip joint. as a short muscle around the hip joint, it stabilizes the hip joint as a postural muscle.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/External_obturator_muscle"
      },
      new Muscle{
        Name= "gracilis",
        MuscleGroup= "thigh",
        Function= "the muscle adducts, medially rotates (with hip flexion), laterally rotates, and flexes the hip as above, and also aids in flexion of the knee.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Gracilis_muscle"
      },
      new Muscle{
        Name= "psoas major",
        MuscleGroup= "core",
        Function= "flexion in the hip joint.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Psoas_major_muscle"
      },
      new Muscle{
        Name= "iliacus",
        MuscleGroup= "thigh",
        Function= "In open-chain exercises, as part of the iliopsoas, the iliacus is important for lifting (flexing) the femur forward (e.g. front scale). In closed-chain exercises, the iliopsoas bends the trunk forward and can lift the trunk from a lying posture.",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Iliacus_muscle"
      },
      new Muscle{
        Name= "tensor fasciae latae",
        MuscleGroup= "thigh",
        Function= "stabilize the hip in extension (assists gluteus maximus during hip extension).",
        WikiPageUrl= "https://en.wikipedia.org/wiki/Tensor_fasciae_latae_muscle"
      }

     };

        var exercises = new List<Exercise> {
      new Exercise{
        Name= "Dragon Flag",
        Difficulty = 9,
        TrainingTypes = new List<TrainingType> {
          new TrainingType{
            Name= "bodybuilding",
          },
          new TrainingType{
            Name= "strength",
          }
        }
        }
      };

        var exerciseMuscles = new List<ExerciseMuscle> {
          new ExerciseMuscle{
            Exercise = exercises[0],
            IsPrimary = true,
            Muscle = muscles[0]
          }


  };
        exercises[0].ExerciseMuscles.Add(exerciseMuscles[0]);
        context.Exercises.AddRange(exercises);
    }


}