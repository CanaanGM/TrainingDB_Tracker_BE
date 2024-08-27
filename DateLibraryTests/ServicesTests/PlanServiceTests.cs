using DataLibrary.Models;
using DataLibrary.Services;
using DateLibraryTests.helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using SharedLibrary.Dtos;
using SharedLibrary.Helpers;

namespace DateLibraryTests.ServicesTests;

public class PlanServiceTests : BaseTestClass
{
    private PlanService service;
    private Mock<ILogger<PlanService>> logger;


    public PlanServiceTests()
    {
        logger = new Mock<ILogger<PlanService>>();
        service = new PlanService(_context, _mapper, logger.Object);
    }


  

    public static IEnumerable<object[]> GeneratePlans ()
    {
        var plans =  PlanHelpers.GeneratePlansDtos().Result;
        return  new List<object[]>
        {
           new object[] { plans[0] },
           new object[] { plans[1] },
           new object[] { plans[2] },
           new object[] { plans[3] },
           new object[] { plans[4] },
        };
        
    }
    
    [Theory]
    [MemberData(nameof(GeneratePlans))]
    public async Task CreateAsync_ShouldCreateTrainingPlanSuccessfully(TrainingPlanWriteDto plan)
    {
        // Arrange
        ProductionDatabaseHelpers.SeedProductionData(_context);

        // Act
        var result = await service.CreateAsync(plan, new CancellationToken());

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value > 0);

        var createdPlan = await GetCreatedPlan(result.Value);
        AssertPlanCreatedSuccessfully(plan, createdPlan);
    }

    [Fact]
    public async Task CreateBulk_ShouldReturnSuccess()
    {
        ProductionDatabaseHelpers.SeedProductionData(_context);
        var plans = await PlanHelpers.GenerateBulkPlan();
        var result = await service.CreateBulkAsync(plans, new CancellationToken());
        
        Assert.True(result.IsSuccess);
        Assert.Equal(plans.Count, _context.TrainingPlans.Count());
        foreach (var plan in plans)
        {
            var createdPlan = await _context.TrainingPlans
                .Include(tp => tp.TrainingWeeks)
                .ThenInclude(tw => tw.TrainingDays)
                .ThenInclude(td => td.Blocks)
                .ThenInclude(b => b.BlockExercises)
                .ThenInclude(be => be.Exercise)
                .FirstOrDefaultAsync(tp => tp.Name == plan.Name);

            AssertPlanCreatedSuccessfully(plan, createdPlan);

        }
                    
    }
    private async Task<TrainingPlan> GetCreatedPlan(int planId)
    {
        return await _context.TrainingPlans
            .Include(tp => tp.TrainingWeeks)
            .ThenInclude(tw => tw.TrainingDays)
            .ThenInclude(td => td.Blocks)
            .ThenInclude(b => b.BlockExercises)
            .ThenInclude(be => be.Exercise)
            .FirstOrDefaultAsync(tp => tp.Id == planId);
    }
    
 private void AssertPlanCreatedSuccessfully(TrainingPlanWriteDto newPlanDto, TrainingPlan createdPlan)
{
    Assert.NotNull(createdPlan);
    Assert.Equal(newPlanDto.Name, createdPlan.Name);
    Assert.Equal(newPlanDto.Description, createdPlan.Description);
    Assert.Equal(newPlanDto.Notes, createdPlan.Notes);

    // Use a loop instead of indexing
    Assert.Equal(newPlanDto.TrainingWeeks.Count, createdPlan.TrainingWeeks.Count);
    var actualWeeksList = createdPlan.TrainingWeeks.ToList();
    for (int i = 0; i < newPlanDto.TrainingWeeks.Count; i++)
    {
        var expectedWeek = newPlanDto.TrainingWeeks[i];
        var actualWeek = actualWeeksList[i];

        Assert.Equal(expectedWeek.Name, actualWeek.Name);
        Assert.Equal(expectedWeek.OrderNumber, actualWeek.OrderNumber);
        Assert.Equal(expectedWeek.TrainingDays.Count, actualWeek.TrainingDays.Count);

        var actualDaysList = actualWeek.TrainingDays.ToList();
        for (int j = 0; j < expectedWeek.TrainingDays.Count; j++)
        {
            var expectedDay = expectedWeek.TrainingDays[j];
            var actualDay = actualDaysList[j];

            Assert.Equal(expectedDay.Name, actualDay.Name);
            Assert.Equal(expectedDay.OrderNumber, actualDay.OrderNumber);
            Assert.Equal(expectedDay.Notes, actualDay.Notes);
            Assert.Equal(expectedDay.Blocks.Count, actualDay.Blocks.Count);

            var actualBlocksList = actualDay.Blocks.ToList();
            for (int k = 0; k < expectedDay.Blocks.Count; k++)
            {
                var expectedBlock = expectedDay.Blocks[k];
                var actualBlock = actualBlocksList[k];

                Assert.Equal(expectedBlock.Name, actualBlock.Name);
                Assert.Equal(expectedBlock.OrderNumber, actualBlock.OrderNumber);
                Assert.Equal(expectedBlock.Sets, actualBlock.Sets);
                Assert.Equal(expectedBlock.RestInSeconds, actualBlock.RestInSeconds);
                Assert.Equal(expectedBlock.Instructions, actualBlock.Instructions);
                Assert.Equal(expectedBlock.BlockExercises.Count, actualBlock.BlockExercises.Count);

                var actualExercisesList = actualBlock.BlockExercises.ToList();
                for (int l = 0; l < expectedBlock.BlockExercises.Count; l++)
                {
                    var expectedExercise = expectedBlock.BlockExercises[l];
                    var actualExercise = actualExercisesList[l];

                    Assert.Equal(expectedExercise.ExerciseName, actualExercise.Exercise.Name);
                    Assert.Equal(expectedExercise.OrderNumber, actualExercise.OrderNumber);
                    Assert.Equal(expectedExercise.Repetitions, actualExercise.Repetitions);

                    if (expectedExercise.TimerInSeconds.HasValue)
                    {
                        Assert.Equal(expectedExercise.TimerInSeconds.Value, actualExercise.TimerInSeconds);
                    }
                    else
                    {
                        Assert.Null(actualExercise.TimerInSeconds);
                    }

                    if (expectedExercise.DistanceInMeters.HasValue)
                    {
                        Assert.Equal(expectedExercise.DistanceInMeters.Value, actualExercise.DistanceInMeters);
                    }
                    else
                    {
                        Assert.Null(actualExercise.DistanceInMeters);
                    }

                    Assert.Equal(expectedExercise.Instructions, actualExercise.Instructions);
                }
            }
        }
    }
}

[Fact]
public async Task CreateAndUpdatePlan_ReverseWeeks_Success()
{
    // Arrange
    ProductionDatabaseHelpers.SeedProductionData(_context);
    
    // Load the plan from the excel.json file
    var excelPlan = await PlanHelpers.ReadPlanFile("excel");

    // Act: Create the plan
    var creationResult = await service.CreateAsync(excelPlan, new CancellationToken());
    Assert.True(creationResult.IsSuccess);

    // Retrieve the created plan
    var createdPlanId = creationResult.Value;
    var createdPlan = await _context.TrainingPlans
        .Include(tp => tp.TrainingWeeks)
        .FirstOrDefaultAsync(tp => tp.Id == createdPlanId);

    Assert.NotNull(createdPlan);
    Assert.Equal(excelPlan.TrainingWeeks.Count, createdPlan.TrainingWeeks.Count);

    // Act: Reverse the weeks
    var reversedWeeks = excelPlan.TrainingWeeks.AsEnumerable().Reverse().ToList();
    excelPlan.TrainingWeeks = reversedWeeks;

    // Update the plan with reversed weeks
    var updateResult = await service.UpdateAsync(createdPlanId, excelPlan, new CancellationToken());
    Assert.True(updateResult.IsSuccess);

    // Retrieve the updated plan
    var updatedPlan = await _context.TrainingPlans
        .Include(tp => tp.TrainingWeeks)
        .ThenInclude(tw => tw.TrainingDays)
        .ThenInclude(td => td.Blocks)
        .ThenInclude(b => b.BlockExercises)
        .ThenInclude(be => be.Exercise)
        .FirstOrDefaultAsync(tp => tp.Id == createdPlanId);

    Assert.NotNull(updatedPlan);
    AssertPlanCreatedSuccessfully(excelPlan, updatedPlan);
}


    [Fact]
    public async Task CreateAsync_MissingExercises_ShouldReturnErrorWithAllMissingExercises()
    {
        // Arrange
        ProductionDatabaseHelpers.SeedProductionData(_context);
        var newPlanDto = TrainingPlanDtoFactory.MissingExerciesPlan;
        // Act
        var result = await service.CreateAsync(newPlanDto, new CancellationToken());
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("nargacuga\ntigrex", result.ErrorMessage);
        Assert.Empty(_context.TrainingPlans);
    }
    
    
    [Fact]
    public async Task CreateAsync_EmptyExercises_ShouldReturnError()
    {
        // Arrange
        ProductionDatabaseHelpers.SeedProductionData(_context);
    
        var newPlanDto =TrainingPlanDtoFactory.NoExercisePlan;
    
        // Act
        var result = await service.CreateAsync(newPlanDto, new CancellationToken());
    
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("The training plan must have at least one week with one day and one exercise.",
            result.ErrorMessage);
        Assert.Empty(_context.TrainingPlans);
    }
    
    
    [Fact]
    public async Task CreateAsync_EmptyPlan_ShouldReturnError()
    {
        // Arrange
        ProductionDatabaseHelpers.SeedProductionData(_context);
    
        var newPlanDto = new TrainingPlanWriteDto
        {
            Name = "Empty Plan",
            Description = "An empty training plan",
            Notes = "Some notes",
    
            TrainingWeeks = new List<TrainingWeekWriteDto>()
        };
    
        // Act
        var result = await service.CreateAsync(newPlanDto, new CancellationToken());
    
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("The training plan must have at least one week with one day and one exercise.",
            result.ErrorMessage);
        Assert.Empty(_context.TrainingPlans);
    }
    

    
    [Fact]
    public async Task UpdateAsync_MissingExercises_ShouldReturnFailure()
    {
        // Arrange
        ProductionDatabaseHelpers.SeedProductionData(_context);
    
        var initialPlan = TrainingPlanDtoFactory.CorrectPlanWithEquipment;
        var initialResult = await service.CreateAsync(initialPlan, new CancellationToken());
        Assert.True(initialResult.IsSuccess);

        var updateDto = TrainingPlanDtoFactory.CreateIncorrectUpdateDto;
        // Act
        var updateResult = await service.UpdateAsync(initialResult.Value, updateDto, new CancellationToken());
    
        // Assert
        Assert.False(updateResult.IsSuccess);
        Assert.Equal("The following exercises do not exist: nonexistentexercise", updateResult.ErrorMessage);
    }
    //
    [Fact]
    public async Task UpdateAsync_FullUpdate_ShouldUpdateTrainingPlanSuccessfully()
    {
        // Arrange
        ProductionDatabaseHelpers.SeedProductionData(_context);
    
        var initialPlan = TrainingPlanDtoFactory.CorrectPlanWithEquipment;
        var initialResult = await service.CreateAsync(initialPlan, new CancellationToken());
        Assert.True(initialResult.IsSuccess);

        var updateDto = TrainingPlanDtoFactory.CreateValidUpdateDto;
    
        // Act
        var updateResult = await service.UpdateAsync(initialResult.Value, updateDto, new CancellationToken());
    
        // Assert
        Assert.True(updateResult.IsSuccess);
    
        var updatedPlan = await _context.TrainingPlans
            .Include(tp => tp.TrainingWeeks)
            .ThenInclude(tw => tw.TrainingDays)
            .ThenInclude(td => td.Blocks)
            .ThenInclude(b => b.BlockExercises)
            .ThenInclude(be => be.Exercise)
            .FirstOrDefaultAsync(tp => tp.Id == initialResult.Value);
    
        Assert.NotNull(updatedPlan);
        Assert.Equal(Utils.NormalizeString(updateDto.Name), updatedPlan.Name);
        Assert.Equal(updateDto.Description, updatedPlan.Description);
        Assert.Equal(updateDto.Notes, updatedPlan.Notes);
        Assert.NotEmpty(updatedPlan.TrainingWeeks);
        Assert.Equal("updated block 1", updatedPlan.TrainingWeeks.First().TrainingDays.First().Blocks.First().Name);
    }
    
    
    [Fact]
    public async Task UpdateAsync_PartialUpdate_ShouldUpdateTrainingPlanSuccessfully()
    {
        // Arrange
        ProductionDatabaseHelpers.SeedProductionData(_context);
    
        var initialPlan = TrainingPlanDtoFactory.CorrectPlanWithEquipment;
        var initialResult = await service.CreateAsync(initialPlan, new CancellationToken());
        Assert.True(initialResult.IsSuccess);

        var partialUpdateDto = TrainingPlanDtoFactory.CreateValidUpdateDto;
    
        // Act
        var updateResult = await service.UpdateAsync(initialResult.Value, partialUpdateDto, new CancellationToken());
    
        // Assert
        Assert.True(updateResult.IsSuccess);
    
        var updatedPlan = await _context.TrainingPlans
            .Include(tp => tp.TrainingWeeks)
            .ThenInclude(tw => tw.TrainingDays)
            .ThenInclude(td => td.Blocks)
            .ThenInclude(b => b.BlockExercises)
            .ThenInclude(be => be.Exercise)
            .FirstOrDefaultAsync(tp => tp.Id == initialResult.Value);
    
        Assert.NotNull(updatedPlan);
        Assert.Equal(Utils.NormalizeString(partialUpdateDto.Name), updatedPlan.Name);
        Assert.Equal(partialUpdateDto.Description, updatedPlan.Description);
        Assert.Equal(partialUpdateDto.Notes, updatedPlan.Notes);
        Assert.NotEmpty(updatedPlan.TrainingWeeks);
        Assert.Equal("updated block 1", updatedPlan.TrainingWeeks.First().TrainingDays.First().Blocks.First().Name);
    }
    
    
    [Fact]
    public async Task UpdateAsync_NoChanges_ShouldNotChangeTrainingPlan()
    {
        // Arrange
        ProductionDatabaseHelpers.SeedProductionData(_context);
    
        var initialPlan = TrainingPlanDtoFactory.CorrectPlanWithEquipment;
        var initialResult = await service.CreateAsync(initialPlan, new CancellationToken());
        Assert.True(initialResult.IsSuccess);
    
        // Act
        var updateResult = await service.UpdateAsync(initialResult.Value, initialPlan, new CancellationToken());
    
        // Assert
        Assert.True(updateResult.IsSuccess);
    
        var updatedPlan = await _context.TrainingPlans
            .Include(tp => tp.TrainingWeeks)
            .ThenInclude(tw => tw.TrainingDays)
            .ThenInclude(td => td.Blocks)
            .ThenInclude(b => b.BlockExercises)
            .ThenInclude(be => be.Exercise)
            .FirstOrDefaultAsync(tp => tp.Id == initialResult.Value);
    
        Assert.NotNull(updatedPlan);
        Assert.Equal(Utils.NormalizeString(initialPlan.Name), updatedPlan.Name);
        Assert.Equal(initialPlan.Description, updatedPlan.Description);
        Assert.Equal(initialPlan.Notes, updatedPlan.Notes);
        Assert.NotEmpty(updatedPlan.TrainingWeeks);
    
        Assert.Equal("block 1", updatedPlan.TrainingWeeks.First().TrainingDays.First().Blocks.First().Name);
    }
    
    
    [Fact]
    public async Task UpdateAsync_InvalidExerciseName_ShouldReturnError()
    {
        // Arrange
        ProductionDatabaseHelpers.SeedProductionData(_context);
    
        var initialPlan = TrainingPlanDtoFactory.CorrectPlanWithEquipment;
        var initialResult = await service.CreateAsync(initialPlan, new CancellationToken());
        Assert.True(initialResult.IsSuccess);

        var updateDto = TrainingPlanDtoFactory.CreateIncorrectUpdateDto;
    
        // Act
        var updateResult = await service.UpdateAsync(initialResult.Value, updateDto, new CancellationToken());
    
        // Assert
        Assert.False(updateResult.IsSuccess);
        Assert.Equal("The following exercises do not exist: nonexistentexercise", updateResult.ErrorMessage);
    }
    
    
    [Fact]
    public async Task UpdateAsync_EmptyTrainingPlan_ShouldReturnError()
    {
        // Arrange
        ProductionDatabaseHelpers.SeedProductionData(_context);
    
        var initialPlan = TrainingPlanDtoFactory.CorrectPlanWithEquipment;
        var initialResult = await service.CreateAsync(initialPlan, new CancellationToken());
        Assert.True(initialResult.IsSuccess);
    
        var updateDto = new TrainingPlanWriteDto
        {
            Name = "Updated Plan",
            Description = "Updated Description",
            Notes = "Updated Notes",
    
            TrainingWeeks = new List<TrainingWeekWriteDto>()
        };
    
        // Act
        var updateResult = await service.UpdateAsync(initialResult.Value, updateDto, new CancellationToken());
    
        // Assert
        Assert.False(updateResult.IsSuccess);
        Assert.Equal("The training plan must have at least one week with one day and one exercise.",
            updateResult.ErrorMessage);
    }
    
    
    [Fact]
    public async Task UpdateAsync_RemoveAllWeeks_ShouldReturnError()
    {
        // Arrange
        ProductionDatabaseHelpers.SeedProductionData(_context);
    
        var initialPlan = TrainingPlanDtoFactory.CorrectPlanWithEquipment;
        var initialResult = await service.CreateAsync(initialPlan, new CancellationToken());
        Assert.True(initialResult.IsSuccess);
    
        var updateDto = new TrainingPlanWriteDto
        {
            Name = initialPlan.Name,
            Description = initialPlan.Description,
            Notes = initialPlan.Notes,
            TrainingWeeks = new List<TrainingWeekWriteDto>()
        };
    
        // Act
        var updateResult = await service.UpdateAsync(initialResult.Value, updateDto, new CancellationToken());
    
        // Assert
        Assert.False(updateResult.IsSuccess);
        Assert.Equal("The training plan must have at least one week with one day and one exercise.",
            updateResult.ErrorMessage);
    }
    
    
    [Fact]
          public async Task UpdateAsync_PartiallyRemoveBlocks_ShouldUpdateSuccessfully()
          {
              // Arrange
              ProductionDatabaseHelpers.SeedProductionData(_context);
          
              var initialPlan = TrainingPlanDtoFactory.CorrectPlanWithEquipment;
              var initialResult = await service.CreateAsync(initialPlan, new CancellationToken());
              Assert.True(initialResult.IsSuccess);
          
              var updateDto = new TrainingPlanWriteDto
              {
                  Name = initialPlan.Name,
                  Description = initialPlan.Description,
                  Notes = initialPlan.Notes,
          
          
                  TrainingWeeks = initialPlan.TrainingWeeks.Select(w => new TrainingWeekWriteDto
                  {
                      Name = w.Name,
                      OrderNumber = w.OrderNumber,
                      TrainingDays = w.TrainingDays.Select(d => new TrainingDaysWriteDto
                      {
                          Name = d.Name,
                          OrderNumber = d.OrderNumber,
                          Blocks = d.Blocks.Take(1).ToList() // Take only the first block
                      }).ToList()
                  }).ToList()
              };
          
              // Act
              var updateResult = await service.UpdateAsync(initialResult.Value, updateDto, new CancellationToken());
          
              // Assert
              Assert.True(updateResult.IsSuccess);
          
              var updatedPlan = await _context.TrainingPlans
                  .Include(tp => tp.TrainingWeeks)
                  .ThenInclude(tw => tw.TrainingDays)
                  .ThenInclude(td => td.Blocks)
                  .FirstOrDefaultAsync(tp => tp.Id == initialResult.Value);
          
              Assert.NotNull(updatedPlan);
              Assert.All(updatedPlan.TrainingWeeks,
                  week => { Assert.All(week.TrainingDays, day => { Assert.Single(day.Blocks); }); });
          }
          
          
    [Fact]
    public async Task UpdateAsync_UpdatePlanNameOnly_ShouldUpdateSuccessfully()
    {
        // Arrange
        ProductionDatabaseHelpers.SeedProductionData(_context);
    
        var initialPlan = TrainingPlanDtoFactory.CorrectPlanWithEquipment;
        var initialResult = await service.CreateAsync(initialPlan, new CancellationToken());
        Assert.True(initialResult.IsSuccess);
    
        var updateDto = new TrainingPlanWriteDto
        {
            Name = "Updated Plan Name",
            Description = initialPlan.Description,
            Notes = initialPlan.Notes,
    
    
            TrainingWeeks = initialPlan.TrainingWeeks
        };
    
        // Act
        var updateResult = await service.UpdateAsync(initialResult.Value, updateDto, new CancellationToken());
    
        // Assert
        Assert.True(updateResult.IsSuccess);
    
        var updatedPlan = await _context.TrainingPlans
            .Include(tp => tp.TrainingWeeks)
            .ThenInclude(tw => tw.TrainingDays)
            .ThenInclude(td => td.Blocks)
            .FirstOrDefaultAsync(tp => tp.Id == initialResult.Value);
    
        Assert.NotNull(updatedPlan);
        Assert.Equal("updated plan name", updatedPlan.Name);
        Assert.Equal(initialPlan.Description, updatedPlan.Description);
        Assert.Equal(initialPlan.Notes, updatedPlan.Notes);
    }
    
    [Fact]
    public async Task UpdateAsync_UpdatePlanDescriptionOnly_ShouldUpdateSuccessfully()
    {
        // Arrange
        ProductionDatabaseHelpers.SeedProductionData(_context);
    
        var initialPlan = TrainingPlanDtoFactory.CorrectPlanWithEquipment;
        var initialResult = await service.CreateAsync(initialPlan, new CancellationToken());
        Assert.True(initialResult.IsSuccess);
    
        var updateDto = new TrainingPlanWriteDto
        {
            Name = initialPlan.Name,
            Description = "Updated Description",
            Notes = initialPlan.Notes,
    
    
            TrainingWeeks = initialPlan.TrainingWeeks
        };
    
        // Act
        var updateResult = await service.UpdateAsync(initialResult.Value, updateDto, new CancellationToken());
    
        // Assert
        Assert.True(updateResult.IsSuccess);
    
        var updatedPlan = await _context.TrainingPlans
            .Include(tp => tp.TrainingWeeks)
            .ThenInclude(tw => tw.TrainingDays)
            .ThenInclude(td => td.Blocks)
            .FirstOrDefaultAsync(tp => tp.Id == initialResult.Value);
    
        Assert.NotNull(updatedPlan);
        Assert.Equal(Utils.NormalizeString(initialPlan.Name), updatedPlan.Name);
        Assert.Equal("Updated Description", updatedPlan.Description);
        Assert.Equal(initialPlan.Notes, updatedPlan.Notes);
    }
    
    [Fact]
    public async Task UpdateAsync_UpdatePlanNotesOnly_ShouldUpdateSuccessfully()
    {
        // Arrange
        ProductionDatabaseHelpers.SeedProductionData(_context);
    
        var initialPlan = TrainingPlanDtoFactory.CorrectPlanWithEquipment;
        var initialResult = await service.CreateAsync(initialPlan, new CancellationToken());
        Assert.True(initialResult.IsSuccess);
    
        var updateDto = new TrainingPlanWriteDto
        {
            Name = initialPlan.Name,
            Description = initialPlan.Description,
            Notes = "Updated Notes",
            TrainingWeeks = initialPlan.TrainingWeeks
        };
    
        // Act
        var updateResult = await service.UpdateAsync(initialResult.Value, updateDto, new CancellationToken());
    
        // Assert
        Assert.True(updateResult.IsSuccess);
    
        var updatedPlan = await _context.TrainingPlans
            .Include(tp => tp.TrainingWeeks)
            .ThenInclude(tw => tw.TrainingDays)
            .ThenInclude(td => td.Blocks)
            .FirstOrDefaultAsync(tp => tp.Id == initialResult.Value);
    
        Assert.NotNull(updatedPlan);
        Assert.Equal(Utils.NormalizeString(initialPlan.Name), updatedPlan.Name);
        Assert.Equal(initialPlan.Description, updatedPlan.Description);
        Assert.Equal("Updated Notes", updatedPlan.Notes);
    }
    
    
    [Fact]
    public async Task DeleteAsync_ValidPlanId_ShouldDeleteSuccessfully()
    {
        // Arrange
        ProductionDatabaseHelpers.SeedProductionData(_context);
    
        var newPlanDto = TrainingPlanDtoFactory.CorrectPlanWithEquipment;
        var createResult = await service.CreateAsync(newPlanDto, new CancellationToken());
        Assert.True(createResult.IsSuccess);
    
        // Act
        var deleteResult = await service.DeleteAsync(createResult.Value, new CancellationToken());
    
        // Assert
        Assert.True(deleteResult.IsSuccess);
        var deletedPlan = await _context.TrainingPlans
            .FirstOrDefaultAsync(tp => tp.Id == createResult.Value);
        Assert.Null(deletedPlan);
    }
    
    [Fact]
    public async Task DeleteAsync_InvalidPlanId_ShouldReturnFailure()
    {
        // Act
        var deleteResult = await service.DeleteAsync(999, new CancellationToken());
    
        // Assert
        Assert.False(deleteResult.IsSuccess);
        Assert.Equal("Training plan not found.", deleteResult.ErrorMessage);
    }
    
    [Fact]
    public async Task DeleteAsync_PlanWithAssociatedEntities_ShouldDeleteAllRelatedEntities()
    {
        // Arrange
        ProductionDatabaseHelpers.SeedProductionData(_context);
    
        var newPlanDto = TrainingPlanDtoFactory.CorrectPlanWithEquipment;
        var createResult = await service.CreateAsync(newPlanDto, new CancellationToken());
        Assert.True(createResult.IsSuccess);
    
        // Act
        var deleteResult = await service.DeleteAsync(createResult.Value, new CancellationToken());
    
        // Assert
        Assert.True(deleteResult.IsSuccess);
    
        var deletedPlan = await _context.TrainingPlans
            .Include(tp => tp.TrainingWeeks)
            .ThenInclude(tw => tw.TrainingDays)
            .ThenInclude(td => td.Blocks)
            .ThenInclude(b => b.BlockExercises)
            .FirstOrDefaultAsync(tp => tp.Id == createResult.Value);
        Assert.Null(deletedPlan);
    
        var weeks = await _context.TrainingWeeks
            .Where(tw => tw.TrainingPlanId == createResult.Value)
            .ToListAsync();
        Assert.Empty(weeks);
    
        var days = await _context.TrainingDays
            .Where(td => td.TrainingWeek.TrainingPlanId == createResult.Value)
            .ToListAsync();
        Assert.Empty(days);
    
        var blocks = await _context.Blocks
            .Where(b => b.TrainingDay.TrainingWeek.TrainingPlanId == createResult.Value)
            .ToListAsync();
        Assert.Empty(blocks);
    
        var blockExercises = await _context.BlockExercises
            .Where(be => be.Block.TrainingDay.TrainingWeek.TrainingPlanId == createResult.Value)
            .ToListAsync();
        Assert.Empty(blockExercises);
    }
    
    [Fact]
    public async Task DeleteAsync_ShouldDeleteAllRelatedEntities()
    {
        // Arrange
        ProductionDatabaseHelpers.SeedProductionData(_context);
    
        var newPlanDto = TrainingPlanDtoFactory.CorrectPlanWithEquipment;
        var createResult = await service.CreateAsync(newPlanDto, new CancellationToken());
        Assert.True(createResult.IsSuccess);
    
        // Act
        var deleteResult = await service.DeleteAsync(createResult.Value, new CancellationToken());
    
        // Assert
        Assert.True(deleteResult.IsSuccess);
    
        // Check if the plan is deleted
        var deletedPlan = await _context.TrainingPlans
            .FirstOrDefaultAsync(tp => tp.Id == createResult.Value);
        Assert.Null(deletedPlan);
    
        // Check if all related weeks are deleted
        var deletedWeeks = await _context.TrainingWeeks
            .Where(tw => tw.TrainingPlanId == createResult.Value)
            .ToListAsync();
        Assert.Empty(deletedWeeks);
    
        // Check if all related days are deleted
        var deletedDays = await _context.TrainingDays
            .Where(td => td.TrainingWeek.TrainingPlanId == createResult.Value)
            .ToListAsync();
        Assert.Empty(deletedDays);
    
        // Check if all related blocks are deleted
        var deletedBlocks = await _context.Blocks
            .Where(b => b.TrainingDay.TrainingWeek.TrainingPlanId == createResult.Value)
            .ToListAsync();
        Assert.Empty(deletedBlocks);
    
        // Check if all related block exercises are deleted
        var deletedBlockExercises = await _context.BlockExercises
            .Where(be => be.Block.TrainingDay.TrainingWeek.TrainingPlanId == createResult.Value)
            .ToListAsync();
        Assert.Empty(deletedBlockExercises);
    }
    
    
    [Fact]
    public async Task GetByIdAsync_ShouldReturnTrainingPlan_WhenTrainingPlanExists()
    {
        ProductionDatabaseHelpers.SeedProductionData(_context);
        // Arrange
        var newPlanDto = TrainingPlanDtoFactory.CorrectPlanWithEquipment;
        var createResult = await service.CreateAsync(newPlanDto, new CancellationToken());
    
        // Act
        var result = await service.GetByIdAsync(createResult.Value, new CancellationToken());
    
        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(createResult.Value, result.Value.Id);
        Assert.NotEmpty(result.Value.TrainingWeeks);
    }
    
    [Fact]
    public async Task GetByIdAsync_ShouldReturnFailure_WhenTrainingPlanDoesNotExist()
    {
        // Arrange
        var nonExistentPlanId = 999;
    
        // Act
        var result = await service.GetByIdAsync(nonExistentPlanId, new CancellationToken());
    
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Training plan not found.", result.ErrorMessage);
    }
    
    [Fact]
    public async Task GetByIdAsync_ShouldReturnFailure_WhenExceptionOccurs()
    {
        // Arrange
        var newPlanDto = TrainingPlanDtoFactory.CorrectPlanWithEquipment;
        var createResult = await service.CreateAsync(newPlanDto, new CancellationToken());
    
        _context.Dispose(); // Force an exception by disposing the _context
    
        // Act
        var result = await service.GetByIdAsync(createResult.Value, new CancellationToken());
    
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Failed to retrieve training plan", result.ErrorMessage);
    }
    
    
    [Fact]
    public async Task CreatePlan_WithInvalidOrderNumbers_ShouldFail()
    {
        // Arrange
        var planWithInvalidOrderNumbers = new TrainingPlanWriteDto
        {
            Name = "Invalid Order Numbers",
            Description = "Plan with non-sequential order numbers.",
            Notes = "Invalid Order Numbers",
            TrainingWeeks = new List<TrainingWeekWriteDto>
            {
                new TrainingWeekWriteDto
                {
                    Name = "Week 1",
                    OrderNumber = 1,
                    TrainingDays = PlanHelpers.GenerateTrainingDays(5)
                },
                new TrainingWeekWriteDto
                {
                    Name = "Week 2",
                    OrderNumber = 10,  // Invalid order number
                    TrainingDays = PlanHelpers.GenerateTrainingDays(5)
                }
            }
        };

        // Act
        var result = await service.CreateAsync(planWithInvalidOrderNumbers, new CancellationToken());

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Training weeks must have sequential order numbers starting from 1.", result.ErrorMessage);
    }


    //////
    ///
    ///
    ///
    
    [Fact]
    public async Task GetPaginatedPlansAsync_LastPage_ReturnsRemainingPlans()
    {
        // Arrange
        ProductionDatabaseHelpers.SeedProductionData(_context);
        var plans = await PlanHelpers.GenerateBulkPlan();
        await service.CreateBulkAsync(plans, new CancellationToken());

        // Act
        var result = await service.GetPaginatedPlansAsync(2, 3, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Items.Count); // Assuming 5 plans total and 3 items per page
    }
    
    [Fact]
    public async Task GetPaginatedPlansAsync_ReturnsCorrectPlans()
    {
        // Arrange
        ProductionDatabaseHelpers.SeedProductionData(_context);
        var plans = await PlanHelpers.GenerateBulkPlan();
        await service.CreateBulkAsync(plans, new CancellationToken());

        // Act
        var result = await service.GetPaginatedPlansAsync(1, 5, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(5, result.Value.Items.Count);
        Assert.Equal("excel", result.Value.Items.First().Name);
    }

    [Fact]
    public async Task GetPaginatedPlansAsync_WithEmptyDatabase_ReturnsEmptyList()
    {
        // Act
        var result = await service.GetPaginatedPlansAsync(1, 5, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Items);
    }

    [Fact]
    public async Task GetPaginatedPlansAsync_WithInvalidPageNumber_ReturnsEmptyList()
    {
        // Arrange
        ProductionDatabaseHelpers.SeedProductionData(_context);
        var plans = await PlanHelpers.GenerateBulkPlan();
        await service.CreateBulkAsync(plans, new CancellationToken());
        // Act
        var result = await service.GetPaginatedPlansAsync(100, 5, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Items);
    }

    [Fact]
    public async Task GetPaginatedPlansAsync_PageSizeLargerThanAvailablePlans_ReturnsAllPlans()
    {
        // Arrange
        ProductionDatabaseHelpers.SeedProductionData(_context);
        var plans = await PlanHelpers.GenerateBulkPlan();
        await service.CreateBulkAsync(plans, new CancellationToken());
        // Act
        var result = await service.GetPaginatedPlansAsync(1, 10, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(plans.Count, result.Value.Items.Count);
    }

}