using SharedLibrary.Dtos;

namespace DateLibraryTests.helpers;

public static class TrainingPlanDtoFactory
{
    public static TrainingPlanWriteDto CorrectPlanWithEquipment => new TrainingPlanWriteDto
    {
        Name = "New Training Plan",
        Description = "A new training plan for testing",
        Notes = "Some notes",


        TrainingWeeks = new List<TrainingWeekWriteDto>
        {
            new TrainingWeekWriteDto
            {
                Name = "Week 1",
                OrderNumber = 1,
                TrainingDays = new List<TrainingDaysWriteDto>
                {
                    new TrainingDaysWriteDto
                    {
                        Name = "Day 1",
                        OrderNumber = 1,
                        Blocks = new List<BlockWriteDto>
                        {
                            new BlockWriteDto
                            {
                                Name = "Block 1",
                                Sets = 3,
                                RestInSeconds = 60,
                                Instructions = "Some instructions",
                                OrderNumber = 1,
                                BlockExercises = new List<BlockExerciseWriteDto>
                                {
                                    new BlockExerciseWriteDto
                                    {
                                        ExerciseName = "dumbbell chest press - flat",
                                        Instructions = "Some notes",
                                        OrderNumber = 1,
                                        Repetitions = 10
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    };


    public static TrainingPlanWriteDto _correctPlanWithNoEquipment => new TrainingPlanWriteDto
    {
        Name = "New Training Plan",
        Description = "A new training plan for testing",
        Notes = "Some notes",


        TrainingWeeks = new List<TrainingWeekWriteDto>
        {
            new TrainingWeekWriteDto
            {
                Name = "Week 1",
                OrderNumber = 1,
                TrainingDays = new List<TrainingDaysWriteDto>
                {
                    new TrainingDaysWriteDto
                    {
                        Name = "Day 1",
                        OrderNumber = 1,
                        Blocks = new List<BlockWriteDto>
                        {
                            new BlockWriteDto
                            {
                                Name = "Block 1",
                                Sets = 3,
                                RestInSeconds = 60,
                                Instructions = "Some instructions",
                                OrderNumber = 1,
                                BlockExercises = new List<BlockExerciseWriteDto>
                                {
                                    new BlockExerciseWriteDto
                                    {
                                        ExerciseName = "dumbbell press - flat",
                                        Instructions = "Some notes",
                                        OrderNumber = 1,
                                        Repetitions = 10
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    };


    public static TrainingPlanWriteDto NoExercisePlan => new TrainingPlanWriteDto
    {
        Name = "Plan With Empty Exercises",
        Description = "A training plan with empty exercises",
        Notes = "Some notes",


        TrainingWeeks = new List<TrainingWeekWriteDto>
        {
            new TrainingWeekWriteDto
            {
                Name = "Week 1",
                OrderNumber = 1,
                TrainingDays = new List<TrainingDaysWriteDto>
                {
                    new TrainingDaysWriteDto
                    {
                        Name = "Day 1",
                        OrderNumber = 1,
                        Blocks = new List<BlockWriteDto>
                        {
                            new BlockWriteDto
                            {
                                Name = "Block 1",
                                Sets = 3,
                                RestInSeconds = 60,
                                Instructions = "Some instructions",
                                OrderNumber = 1,
                                BlockExercises = new List<BlockExerciseWriteDto>()
                            }
                        }
                    }
                }
            }
        }
    };

    public static TrainingPlanWriteDto MissingExerciesPlan => new TrainingPlanWriteDto
    {
        Name = "New Training Plan",
        Description = "A new training plan for testing",
        Notes = "Some notes",


        TrainingWeeks = new List<TrainingWeekWriteDto>
        {
            new TrainingWeekWriteDto
            {
                Name = "Week 1",
                OrderNumber = 1,
                TrainingDays = new List<TrainingDaysWriteDto>
                {
                    new TrainingDaysWriteDto
                    {
                        Name = "Day 1",
                        OrderNumber = 1,
                        Blocks = new List<BlockWriteDto>
                        {
                            new BlockWriteDto
                            {
                                Name = "Block 1",
                                Sets = 3,
                                RestInSeconds = 60,
                                Instructions = "Some instructions",
                                OrderNumber = 1,
                                BlockExercises = new List<BlockExerciseWriteDto>
                                {
                                    new BlockExerciseWriteDto
                                    {
                                        ExerciseName = "nargacuga",
                                        Instructions = "Some notes",
                                        OrderNumber = 1,
                                        Repetitions = 10
                                    },
                                    new BlockExerciseWriteDto
                                    {
                                        ExerciseName = "Tigrex",
                                        Instructions = "Some notes",
                                        OrderNumber = 1,
                                        Repetitions = 10
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    };


    public static TrainingPlanWriteDto CreateIncorrectUpdateDto => new TrainingPlanWriteDto
    {
        Name = "Updated Training Plan",
        Description = "Updated description",
        Notes = "Updated notes",


        TrainingWeeks = new List<TrainingWeekWriteDto>
        {
            new TrainingWeekWriteDto
            {
                Name = "Updated Week 1",
                OrderNumber = 1,
                TrainingDays = new List<TrainingDaysWriteDto>
                {
                    new TrainingDaysWriteDto
                    {
                        Name = "Updated Day 1",
                        OrderNumber = 1,
                        Blocks = new List<BlockWriteDto>
                        {
                            new BlockWriteDto
                            {
                                Name = "Updated Block 1",
                                Sets = 4,
                                RestInSeconds = 90,
                                Instructions = "Updated instructions",
                                OrderNumber = 1,
                                BlockExercises = new List<BlockExerciseWriteDto>
                                {
                                    new BlockExerciseWriteDto
                                    {
                                        ExerciseName = "NonExistentExercise",
                                        Instructions = "Updated notes",
                                        OrderNumber = 1,
                                        Repetitions = 12
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    };
    
    public static TrainingPlanWriteDto CreateValidUpdateDto = new TrainingPlanWriteDto
    {
        Name = "Updated Training Plan",
        Description = "Updated description",
        Notes = "Updated notes",
    
    
        TrainingWeeks = new List<TrainingWeekWriteDto>
        {
            new TrainingWeekWriteDto
            {
                Name = "Updated Week 1",
                OrderNumber = 1,
                TrainingDays = new List<TrainingDaysWriteDto>
                {
                    new TrainingDaysWriteDto
                    {
                        Name = "Updated Day 1",
                        OrderNumber = 1,
                        Blocks = new List<BlockWriteDto>
                        {
                            new BlockWriteDto
                            {
                                Name = "Updated Block 1",
                                Sets = 4,
                                RestInSeconds = 90,
                                Instructions = "Updated instructions",
                                OrderNumber = 1,
                                BlockExercises = new List<BlockExerciseWriteDto>
                                {
                                    new BlockExerciseWriteDto
                                    {
                                        ExerciseName = "dumbbell chest press - flat",
                                        Instructions = "Updated notes",
                                        OrderNumber = 1,
                                        Repetitions = 12
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    };
}