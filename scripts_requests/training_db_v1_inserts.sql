

-- Muscle, Muscle groups and Their Relations
-- Inserting Back Muscle Group
INSERT INTO muscle_group (scientific_name, common_name, `function`, wiki_page_url) VALUES
('musculi_dorsi', 'Back', 'Group of muscles located on the back of the body that support the spine and facilitate movements like extension, lateral flexion, and rotation of the spine.', 'https://en.wikipedia.org/wiki/Back_muscles'),
('musculi_pectoralis', 'Pectoralis', 'Group of muscles located in the chest that contribute to shoulder and arm movements, such as flexion, adduction, and rotation.', 'https://en.wikipedia.org/wiki/Pectoralis_major_muscle'),
('musculi_colli', 'Neck', 'Group of muscles that support the neck and head, allowing for a range of movements including flexion, extension, and rotation.', 'https://en.wikipedia.org/wiki/Neck_muscles'),
('musculi_deltoidei', 'Deltoid', 'Group of muscles that form the rounded contour of the shoulder, responsible for various arm movements including abduction, flexion, and extension.', 'https://en.wikipedia.org/wiki/Deltoid_muscle'),
('musculi_bicipitis', 'Biceps', 'Group of muscles located in the upper arm that flex the elbow joint and supinate the forearm.', 'https://en.wikipedia.org/wiki/Biceps'),
('musculi_antibrachii', 'Forearm', 'Group of muscles located in the forearm responsible for various movements of the hand and wrist.', 'https://en.wikipedia.org/wiki/Forearm_muscles'),
('musculi_ischiofemorales', 'Hamstrings', 'Group of muscles located in the posterior thigh that flex the knee joint and extend the hip joint.', 'https://en.wikipedia.org/wiki/Hamstring'),
('musculi_quadriceps', 'Quadriceps', 'Group of muscles located in the front of the thigh that extend the knee joint.', 'https://en.wikipedia.org/wiki/Quadriceps_femoris_muscle'),
('musculi_glutei', 'Glutes', 'Group of muscles located in the buttocks that extend and abduct the hip joint.', 'https://en.wikipedia.org/wiki/Gluteal_muscles'),
('musculi_abdominis', 'Abs', 'Group of muscles located in the abdomen that support the trunk, flex the spine, and help with breathing.', 'https://en.wikipedia.org/wiki/Abdominal_muscles'),
('musculi_gastrocnemius', 'Calves', 'Group of muscles located in the back of the lower leg that flex the knee and plantar flex the ankle.', 'https://en.wikipedia.org/wiki/Gastrocnemius_muscle'),
('musculi_cruris', 'Lower Leg', 'Group of muscles located in the lower leg that move the foot and toes.', 'https://en.wikipedia.org/wiki/Lower_leg_muscles');
INSERT INTO muscle (name, `function`, wiki_page_url) VALUES
--  Back Muscles
('Upper Trapezius', 'Elevates the scapula and extends the head at the neck.', 'https://en.wikipedia.org/wiki/Trapezius_muscle'),
('Middle Trapezius', 'Retracts the scapula.', 'https://en.wikipedia.org/wiki/Trapezius_muscle'),
('Lower Trapezius', 'Depresses the scapula.', 'https://en.wikipedia.org/wiki/Trapezius_muscle'),
('Latissimus Dorsi', 'Extends, adducts, and medially rotates the arm.', 'https://en.wikipedia.org/wiki/Latissimus_dorsi_muscle'),
('Rhomboid Major', 'Retracts and elevates the scapula.', 'https://en.wikipedia.org/wiki/Rhomboid_major_muscle'),
('Rhomboid Minor', 'Retracts and elevates the scapula.', 'https://en.wikipedia.org/wiki/Rhomboid_minor_muscle'),
('Erector Spinae', 'Extends and laterally flexes the spine.', 'https://en.wikipedia.org/wiki/Erector_spinae_muscles'),
('Levator Scapulae', 'Elevates the scapula and tilts its glenoid cavity inferiorly by rotating the scapula.', 'https://en.wikipedia.org/wiki/Levator_scapulae_muscle'),
('Infraspinatus', 'Laterally rotates the arm and stabilizes the shoulder joint.', 'https://en.wikipedia.org/wiki/Infraspinatus_muscle'),
('Teres Major', 'Adducts and medially rotates the arm.', 'https://en.wikipedia.org/wiki/Teres_major_muscle'),
('Teres Minor', 'Laterally rotates the arm and stabilizes the shoulder joint.', 'https://en.wikipedia.org/wiki/Teres_minor_muscle'),
('Serratus Posterior Superior', 'Elevates the upper ribs during inhalation.', 'https://en.wikipedia.org/wiki/Serratus_posterior_superior_muscle'),
('Serratus Posterior Inferior', 'Depresses the lower ribs during exhalation.', 'https://en.wikipedia.org/wiki/Serratus_posterior_inferior_muscle'),
('Multifidus', 'Stabilizes the joints within the spine.', 'https://en.wikipedia.org/wiki/Multifidus_muscle'),
('Quadratus Lumborum', 'Lateral flexion of the vertebral column and stabilization of the pelvis.', 'https://en.wikipedia.org/wiki/Quadratus_lumborum_muscle'),
-- neck muscles
('Sternocleidomastoid', 'Flexes and laterally rotates the head.', 'https://en.wikipedia.org/wiki/Sternocleidomastoid_muscle'),
('Scalene Anterior', 'Elevates the first rib and flexes and laterally bends the neck.', 'https://en.wikipedia.org/wiki/Scalene_muscles#Anterior_scalene'),
('Scalene Middle', 'Elevates the first rib and flexes and laterally bends the neck.', 'https://en.wikipedia.org/wiki/Scalene_muscles#Middle_scalene'),
('Scalene Posterior', 'Elevates the second rib and flexes and laterally bends the neck.', 'https://en.wikipedia.org/wiki/Scalene_muscles#Posterior_scalene'),
('Levator Scapulae', 'Elevates the scapula and tilts its glenoid cavity inferiorly by rotating the scapula.', 'https://en.wikipedia.org/wiki/Levator_scapulae_muscle'),
('Splenius Capitis', 'Extends, rotates, and laterally flexes the head.', 'https://en.wikipedia.org/wiki/Splenius_capitis_muscle'),
('Splenius Cervicis', 'Extends and rotates the cervical spine.', 'https://en.wikipedia.org/wiki/Splenius_cervicis_muscle'),
('Longus Capitis', 'Flexes the head.', 'https://en.wikipedia.org/wiki/Longus_capitis_muscle'),
('Longus Colli', 'Flexes the neck and head.', 'https://en.wikipedia.org/wiki/Longus_colli_muscle'),
('Omohyoid', 'Depresses the hyoid bone and larynx.', 'https://en.wikipedia.org/wiki/Omohyoid_muscle'),
('Sternohyoid', 'Depresses the hyoid bone after it has been elevated during swallowing.', 'https://en.wikipedia.org/wiki/Sternohyoid_muscle'),
('Platysma', 'Tenses the skin of the neck.', 'https://en.wikipedia.org/wiki/Platysma_muscle'),
-- chest 
('Pectoralis Major', 'Flexes, adducts, and medially rotates the arm.', 'https://en.wikipedia.org/wiki/Pectoralis_major_muscle'),
('Pectoralis Minor', 'Stabilizes the scapula by drawing it inferiorly and anteriorly against the thoracic wall.', 'https://en.wikipedia.org/wiki/Pectoralis_minor_muscle'),
('Serratus Anterior', 'Protracts and stabilizes the scapula.', 'https://en.wikipedia.org/wiki/Serratus_anterior_muscle'),
-- deltoids
('Deltoid Anterior Head', 'Flexes and medially rotates the arm.', 'https://en.wikipedia.org/wiki/Deltoid_muscle#Anterior_part'),
('Deltoid Middle Head', 'Abducts the arm.', 'https://en.wikipedia.org/wiki/Deltoid_muscle#Middle_part'),
('Deltoid Posterior Head', 'Extends and laterally rotates the arm.', 'https://en.wikipedia.org/wiki/Deltoid_muscle#Posterior_part'),
-- biceps
('Biceps Brachii', 'Flexes the elbow joint and supinates the forearm.', 'https://en.wikipedia.org/wiki/Biceps_brachii_muscle'),
('Brachialis', 'Flexes the elbow joint.', 'https://en.wikipedia.org/wiki/Brachialis_muscle'),
('Brachioradialis', 'Flexes the elbow joint.', 'https://en.wikipedia.org/wiki/Brachioradialis_muscle'),
-- forearms
('Flexor Digitorum Profundus', 'Flexes the fingers at the distal interphalangeal joints.', 'https://en.wikipedia.org/wiki/Flexor_digitorum_profundus_muscle'),
('Flexor Digitorum Superficialis', 'Flexes the fingers at the proximal interphalangeal joints.', 'https://en.wikipedia.org/wiki/Flexor_digitorum_superficialis_muscle'),
('Flexor Carpi Radialis', 'Flexes and abducts the hand at the wrist.', 'https://en.wikipedia.org/wiki/Flexor_carpi_radialis_muscle'),
('Flexor Carpi Ulnaris', 'Flexes and adducts the hand at the wrist.', 'https://en.wikipedia.org/wiki/Flexor_carpi_ulnaris_muscle'),
('Pronator Teres', 'Pronates the forearm and flexes the elbow joint.', 'https://en.wikipedia.org/wiki/Pronator_teres_muscle'),
-- hamstrings
('Biceps Femoris', 'Flexes the knee joint and laterally rotates the leg.', 'https://en.wikipedia.org/wiki/Biceps_femoris_muscle'),
('Semimembranosus', 'Flexes the knee joint and medially rotates the leg.', 'https://en.wikipedia.org/wiki/Semimembranosus_muscle'),
('Semitendinosus', 'Flexes the knee joint and medially rotates the leg.', 'https://en.wikipedia.org/wiki/Semitendinosus_muscle'),
-- quads
('Rectus Femoris', 'Flexes the hip and extends the knee.', 'https://en.wikipedia.org/wiki/Rectus_femoris_muscle'),
('Vastus Lateralis', 'Extends the knee and stabilizes the patella.', 'https://en.wikipedia.org/wiki/Vastus_lateralis_muscle'),
('Vastus Medialis', 'Extends the knee and stabilizes the patella.', 'https://en.wikipedia.org/wiki/Vastus_medialis_muscle'),
('Vastus Intermedius', 'Extends the knee.', 'https://en.wikipedia.org/wiki/Vastus_intermedius_muscle'),
--glutes
('Gluteus Maximus', 'Extends and laterally rotates the hip.', 'https://en.wikipedia.org/wiki/Gluteus_maximus_muscle'),
('Gluteus Medius', 'Abducts and medially rotates the hip.', 'https://en.wikipedia.org/wiki/Gluteus_medius_muscle'),
('Gluteus Minimus', 'Abducts and medially rotates the hip.', 'https://en.wikipedia.org/wiki/Gluteus_minimus_muscle'),
-- abs
('Rectus Abdominis', 'Flexes the trunk and compresses the abdomen.', 'https://en.wikipedia.org/wiki/Rectus_abdominis_muscle'),
('External Oblique', 'Flexes the trunk, rotates the trunk, and compresses the abdomen.', 'https://en.wikipedia.org/wiki/External_oblique_muscle'),
('Internal Oblique', 'Flexes the trunk, rotates the trunk, and compresses the abdomen.', 'https://en.wikipedia.org/wiki/Internal_oblique_muscle'),
('Transversus Abdominis', 'Compresses the abdomen.', 'https://en.wikipedia.org/wiki/Transversus_abdominis_muscle'),
--calves
('Gastrocnemius', 'Plantar flexes the ankle and flexes the knee.', 'https://en.wikipedia.org/wiki/Gastrocnemius_muscle'),
('Soleus', 'Plantar flexes the ankle.', 'https://en.wikipedia.org/wiki/Soleus_muscle'),
-- lower legs
('Tibialis Anterior', 'Dorsiflexes and inverts the foot.', 'https://en.wikipedia.org/wiki/Tibialis_anterior_muscle'),
('Peroneus Longus', 'Plantar flexes and everts the foot.', 'https://en.wikipedia.org/wiki/Peroneus_longus_muscle'),
('Peroneus Brevis', 'Plantar flexes and everts the foot.', 'https://en.wikipedia.org/wiki/Peroneus_brevis_muscle');
INSERT INTO muscle_group_muscle (muscle_group_id, muscle_id) VALUES
-- back muscles
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_dorsi'), (SELECT id FROM muscle WHERE name = 'Upper Trapezius')),
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_dorsi'), (SELECT id FROM muscle WHERE name = 'Middle Trapezius')),
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_dorsi'), (SELECT id FROM muscle WHERE name = 'Lower Trapezius')),
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_dorsi'), (SELECT id FROM muscle WHERE name = 'Latissimus Dorsi')),
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_dorsi'), (SELECT id FROM muscle WHERE name = 'Rhomboid Major')),
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_dorsi'), (SELECT id FROM muscle WHERE name = 'Rhomboid Minor')),
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_dorsi'), (SELECT id FROM muscle WHERE name = 'Erector Spinae')),
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_dorsi'), (SELECT id FROM muscle WHERE name = 'Levator Scapulae')),
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_dorsi'), (SELECT id FROM muscle WHERE name = 'Infraspinatus')),
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_dorsi'), (SELECT id FROM muscle WHERE name = 'Teres Major')),
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_dorsi'), (SELECT id FROM muscle WHERE name = 'Teres Minor')),
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_dorsi'), (SELECT id FROM muscle WHERE name = 'Serratus Posterior Superior')),
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_dorsi'), (SELECT id FROM muscle WHERE name = 'Serratus Posterior Inferior')),
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_dorsi'), (SELECT id FROM muscle WHERE name = 'Multifidus')),
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_dorsi'), (SELECT id FROM muscle WHERE name = 'Quadratus Lumborum')),
-- neck
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_colli'), (SELECT id FROM muscle WHERE name = 'Sternocleidomastoid')),
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_colli'), (SELECT id FROM muscle WHERE name = 'Scalene Anterior')),
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_colli'), (SELECT id FROM muscle WHERE name = 'Scalene Middle')),
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_colli'), (SELECT id FROM muscle WHERE name = 'Scalene Posterior')),
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_colli'), (SELECT id FROM muscle WHERE name = 'Levator Scapulae')),
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_colli'), (SELECT id FROM muscle WHERE name = 'Splenius Capitis')),
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_colli'), (SELECT id FROM muscle WHERE name = 'Splenius Cervicis')),
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_colli'), (SELECT id FROM muscle WHERE name = 'Longus Capitis')),
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_colli'), (SELECT id FROM muscle WHERE name = 'Longus Colli')),
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_colli'), (SELECT id FROM muscle WHERE name = 'Omohyoid')),
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_colli'), (SELECT id FROM muscle WHERE name = 'Sternohyoid')),
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_colli'), (SELECT id FROM muscle WHERE name = 'Platysma')),
-- chest
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_pectoralis'), (SELECT id FROM muscle WHERE name = 'Pectoralis Major')),
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_pectoralis'), (SELECT id FROM muscle WHERE name = 'Pectoralis Minor')),
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_pectoris'), (SELECT id FROM muscle WHERE scientific_name = 'Serratus Anterior')),
-- deltoids
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_deltoidei'), (SELECT id FROM muscle WHERE name = 'Deltoid Anterior Head')),
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_deltoidei'), (SELECT id FROM muscle WHERE name = 'Deltoid Middle Head')),
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_deltoidei'), (SELECT id FROM muscle WHERE name = 'Deltoid Posterior Head')),
-- biceps
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_bicipitis'), (SELECT id FROM muscle WHERE name = 'Biceps Brachii')),
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_bicipitis'), (SELECT id FROM muscle WHERE name = 'Brachialis')),
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_bicipitis'), (SELECT id FROM muscle WHERE name = 'Brachioradialis')),
--forearms
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_antibrachii'), (SELECT id FROM muscle WHERE name = 'Flexor Digitorum Profundus')),
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_antibrachii'), (SELECT id FROM muscle WHERE name = 'Flexor Digitorum Superficialis')),
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_antibrachii'), (SELECT id FROM muscle WHERE name = 'Flexor Carpi Radialis')),
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_antibrachii'), (SELECT id FROM muscle WHERE name = 'Flexor Carpi Ulnaris')),
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_antibrachii'), (SELECT id FROM muscle WHERE name = 'Pronator Teres')),
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_antibrachii'), (SELECT id FROM muscle WHERE name = 'Brachioradialis')),
--hamstrings
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_ischiofemorales'), (SELECT id FROM muscle WHERE name = 'Biceps Femoris')),
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_ischiofemorales'), (SELECT id FROM muscle WHERE name = 'Semimembranosus')),
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_ischiofemorales'), (SELECT id FROM muscle WHERE name = 'Semitendinosus')),
--quads
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_quadriceps'), (SELECT id FROM muscle WHERE name = 'Rectus Femoris')),
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_quadriceps'), (SELECT id FROM muscle WHERE name = 'Vastus Lateralis')),
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_quadriceps'), (SELECT id FROM muscle WHERE name = 'Vastus Medialis')),
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_quadriceps'), (SELECT id FROM muscle WHERE name = 'Vastus Intermedius')),
--glutes
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_glutei'), (SELECT id FROM muscle WHERE name = 'Gluteus Maximus')),
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_glutei'), (SELECT id FROM muscle WHERE name = 'Gluteus Medius')),
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_glutei'), (SELECT id FROM muscle WHERE name = 'Gluteus Minimus')),
-- abs
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_abdominis'), (SELECT id FROM muscle WHERE name = 'Rectus Abdominis')),
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_abdominis'), (SELECT id FROM muscle WHERE name = 'External Oblique')),
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_abdominis'), (SELECT id FROM muscle WHERE name = 'Internal Oblique')),
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_abdominis'), (SELECT id FROM muscle WHERE name = 'Transversus Abdominis')),
--calves
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_gastrocnemius'), (SELECT id FROM muscle WHERE name = 'Gastrocnemius')),
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_gastrocnemius'), (SELECT id FROM muscle WHERE name = 'Soleus')),
--lower legs
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_cruris'), (SELECT id FROM muscle WHERE name = 'Tibialis Anterior')),
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_cruris'), (SELECT id FROM muscle WHERE name = 'Peroneus Longus')),
((SELECT id FROM muscle_group WHERE scientific_name = 'musculi_cruris'), (SELECT id FROM muscle WHERE name = 'Peroneus Brevis'));




SELECT * FROM muscle_group mg ;

SELECT m.name, mg.common_name FROM muscle m
	JOIN muscle_group_muscle mgm ON mgm.muscle_id = m.id
	JOIN muscle_group mg ON mgm.muscle_group_id = mg.id
WHere mg.common_name = "Hamstrings" 
;
SELECT e.name, m. FROM equipment e
	JOIN equipment_muscle em ON e.id = em.equipment_id
	JOIN muscle m ON em.muscle_id  = m.id 
;


