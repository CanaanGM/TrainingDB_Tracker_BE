// using AutoMapper;
//
// using DataLibrary.Context;
// using DataLibrary.Core;
// using DataLibrary.Dtos;
// using DataLibrary.Helpers;
// using DataLibrary.Services;
//
// using Microsoft.Extensions.Logging;
//
// using Moq;
//
// using TestSupport.EfHelpers;
//
// namespace DateLibraryTests.ServicesTests;
// public class TrainingTypesServiceTests
// {
//     DbContextOptionsDisposable<SqliteContext>? options;
//     Profiles? myProfile;
//     MapperConfiguration? configuration;
//     Mapper? mapper;
//     private Mock<ILogger<TrainingTypesService>> logger;
//
//     public TrainingTypesServiceTests()
//     {
//         options = SqliteInMemory.CreateOptions<SqliteContext>();
//         myProfile = new Profiles();
//         configuration = new MapperConfiguration(cfg => cfg.AddProfile(myProfile));
//         mapper = new Mapper(configuration);
//         logger = new Mock<ILogger<TrainingTypesService>>();
//
//     }
//
//     [Theory]
//     [InlineData("Cardio")]
//     public async Task Creating_Returns_Success(string @string)
//     {
//         using SqliteContext? context = new SqliteContext(options!);
//         context.Database.EnsureCreated();
//         var newTypeDto = new TrainingTypeWriteDto
//         {
//             Name = @string
//         };
//
//
//         var service = new TrainingTypesService(context, mapper!, logger.Object);
//         var creationResult = await service.UpdateAsync(newTypeDto, new CancellationToken());
//
//         Assert.True(creationResult.Value >= 1); // a new ID has been assigned
//         var newType = context.TrainingTypes.FirstOrDefault(x => x.Id == creationResult.Value);
//         Assert.NotNull(newType);
//         Assert.True(newType.Name == Utils.NormalizeString(@string)); // normalizing works
//
//     }
//
//     [Fact]
//     public async Task GettingAll_no_types_returns_Success()
//     {
//         using SqliteContext? context = new SqliteContext(options!);
//         context.Database.EnsureCreated();
//
//         var service = new TrainingTypesService(context, mapper!, logger.Object);
//         var result = await service.GetAllAsync(new CancellationToken());
//
//         Assert.True(result.IsSuccess);
//         Assert.NotNull(result.Value);
//         Assert.Empty(result.Value);
//
//     }
//
//     [Fact]
//     public async Task GetAll_returns_success()
//     {
//         using SqliteContext? context = new SqliteContext(options!);
//         context.Database.EnsureCreated();
//
//         context.TrainingTypes.Add(new DataLibrary.Models.TrainingType { Name = "cardio" });
//         context.TrainingTypes.Add(new DataLibrary.Models.TrainingType { Name = "strength" });
//         context.SaveChanges();
//
//         var service = new TrainingTypesService(context, mapper!, logger.Object);
//         var result = await service.GetAllAsync(new CancellationToken());
//
//         Assert.True(result.IsSuccess);
//         Assert.NotNull(result.Value);
//         Assert.NotEmpty(result.Value);
//         Assert.Equal(2, result.Value.Count());
//         Assert.Equal("cardio", result.Value[0].Name);
//         Assert.Equal("strength", result.Value[1].Name);
//     }
//
//     [Fact]
//     public async Task Update_record_returns_success()
//     {
//         using SqliteContext? context = new SqliteContext(options!);
//         context.Database.EnsureCreated();
//
//         context.TrainingTypes.Add(new DataLibrary.Models.TrainingType { Name = "cardio" });
//         context.SaveChanges();
//
//         var service = new TrainingTypesService(context, mapper!, logger.Object);
//         var result = await service.Update(1, new TrainingTypeWriteDto { Name = "strength" }, new CancellationToken());
//         Assert.True(result.IsSuccess);
//         var updatedType = context.TrainingTypes.FirstOrDefault(x => x.Name == "strength");
//         Assert.NotNull(updatedType);
//     }
//
//     [Fact]
//     public async Task Delete_type_returns_success()
//     {
//         using SqliteContext? context = new SqliteContext(options!);
//         context.Database.EnsureCreated();
//
//         context.TrainingTypes.Add(new DataLibrary.Models.TrainingType { Name = "cardio" });
//         context.SaveChanges();
//         context.ChangeTracker.Clear();
//         var service = new TrainingTypesService(context, mapper!, logger.Object);
//         var result = await service.DeleteAsync(1, new CancellationToken());
//
//         Assert.True(result.IsSuccess);
//         Assert.Null(context.TrainingTypes.FirstOrDefault(x => x.Name == "cardio"));
//     }
//
//
//     [Fact]
//     public async Task CreateBulk_returns_Success()
//     {
//         using SqliteContext? context = new SqliteContext(options!);
//         context.Database.EnsureCreated();
//
//
//         var listOfTypes = new List<TrainingTypeWriteDto> {
//             new TrainingTypeWriteDto{Name = "CaRdIo"},
//             new TrainingTypeWriteDto{Name = "STRENGTH"},
//             new TrainingTypeWriteDto{Name = "BodyBuilding"},
//             new TrainingTypeWriteDto{Name = "Mobility"},
//             new TrainingTypeWriteDto{Name = "Martial Arts"},
//             new TrainingTypeWriteDto{Name = "Yoga"},
//         };
//
//         var service = new TrainingTypesService(context, mapper!, logger.Object);
//         var result = await service.CreateBulkAsync(listOfTypes, new CancellationToken());
//
//
//         Assert.True(result.IsSuccess);
//         var createdTypes = context.TrainingTypes.ToList();
//         Assert.NotNull(createdTypes);
//         Assert.Equal(6, createdTypes.Count());
//         Assert.Equal("cardio", createdTypes[0].Name);
//         Assert.Equal("strength", createdTypes[1].Name);
//         Assert.Equal("martial arts", createdTypes[4].Name);
//     }
// }
