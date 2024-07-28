// using System.Collections;
// using AutoMapper;
// using DataLibrary.Context;
// using DataLibrary.Core;
// using DataLibrary.Dtos;
// using DataLibrary.Helpers;
// using DataLibrary.Models;
// using DataLibrary.Services;
// using Microsoft.Extensions.Logging;
// using Moq;
// using TestSupport.EfHelpers;
//
// namespace DateLibraryTests.ServicesTests;
//
// public class EquipmentServiceTests
// {
//     private readonly DbContextOptionsDisposable<SqliteContext> options;
//     private readonly SqliteContext context;
//     private readonly Profiles myProfile;
//     private readonly MapperConfiguration configuration;
//     private readonly Mapper mapper;
//     private readonly Mock<ILogger<EquipmentService>> logger;
//     private readonly EquipmentService service;
//
//     public EquipmentServiceTests()
//     {
//         options = SqliteInMemory.CreateOptions<SqliteContext>();
//         context = new SqliteContext(options);
//         context.Database.EnsureCreated();
//         myProfile = new Profiles();
//         configuration = new MapperConfiguration(cfg => cfg.AddProfile(myProfile));
//         mapper = new Mapper(configuration);
//         logger = new Mock<ILogger<EquipmentService>>();
//         service = new EquipmentService(context, mapper, logger.Object);
//     }
//
//     [Fact]
//     public async Task GetAll_Empty_success()
//     {
//         var result = await service.GetAsync(new CancellationToken());
//         Assert.True(result.IsSuccess);
//         Assert.Empty(result.Value);
//     }
//
//     [Fact]
//     public async Task GetAllNotEmpty_Success()
//     {
//         Console.WriteLine($"{Directory.GetCurrentDirectory()}/training_log_v2.db");
//         Console.WriteLine($"{AppContext.BaseDirectory}/training_log_v2.db");
//         List<Equipment> equipments = new()
//         {
//             new Equipment()
//             {
//                 Name = "straight bar",
//                 WeightKg = 10,
//                 Description = "Home training bar"
//             },
//             new Equipment()
//             {
//                 Name = "swiggly bar",
//                 WeightKg = 7,
//                 Description = "Home training swiggly bar"
//             }
//         };
//         context.Equipment.AddRange(equipments);
//         context.SaveChanges();
//
//         var result = await service.GetAsync(new CancellationToken());
//         Assert.True(result.IsSuccess);
//         Assert.NotEmpty(result.Value);
//         Assert.Equal(equipments[0].Name, result.Value[0].Name);
//         Assert.Equal(equipments[0].WeightKg, result.Value[0].WeightKg);
//         Assert.Equal(equipments[0].Description, result.Value[0].Description);
//         Assert.Equal(DateTime.Now.Date, result.Value[0].CreatedAt?.Date);
//         Assert.Equal(equipments[1].Name, result.Value[1].Name);
//         Assert.Equal(equipments[1].WeightKg, result.Value[1].WeightKg);
//         Assert.Equal(equipments[1].Description, result.Value[1].Description);
//         Assert.Equal(DateTime.UtcNow.Date, result.Value[1].CreatedAt?.Date);
//     }
//
//     [Fact]
//     public async Task Upsert_CreationShouldReturnSucess()
//     {
//         var n = new EquipmentWriteDto()
//         {
//             Name = "straight bar",
//             WeightKg = 10,
//             Description = "Home training bar"
//         };
//
//
//         var result = await service.UpsertAsync(n, new CancellationToken());
//         Assert.True(result.IsSuccess);
//         Assert.True(result.Value >= 1);
//
//         var newEquip = context.Equipment.First();
//         Assert.Equal(newEquip.Name, n.Name);
//         Assert.Equal(newEquip.WeightKg, n.WeightKg);
//         Assert.Equal(newEquip.Description, n.Description);
//         Assert.Equal(DateTime.UtcNow.Day, newEquip.CreatedAt.Day);
//     }
//
//    public static IEnumerable<object[]> UpsertRecords() => new List<object[]>()
//     {
//         new[]
//         {
//             new Tuple<Equipment, EquipmentWriteDto>(new Equipment()
//             {
//                 Name = "straight bar",
//                 Description = "Home training bar",
//                 WeightKg = 10
//             }, new EquipmentWriteDto()
//             {
//                 Name = "straight bar",
//                 NewName = "swiggly bar",
//                 Description = "Home training bar, the non olympic version",
//                 WeightKg = 101
//             }),
//         },
//         new[]
//         {
//             new Tuple<Equipment, EquipmentWriteDto>(new Equipment()
//             {
//                 Name = "straight bar",
//                 Description = "Home training bar",
//                 WeightKg = 10
//             }, new EquipmentWriteDto()
//             {
//                 Name = "straight bar",
//                 Description = "Home training bar, the non olympic version"
//             }),
//         },
//         new[]
//         {
//             new Tuple<Equipment, EquipmentWriteDto>(new Equipment()
//             {
//                 Name = "straight bar",
//                 Description = "Home training bar",
//                 WeightKg = 10
//             }, new EquipmentWriteDto()
//             {
//                 Name = "Straight bar",
//                 WeightKg = 101
//             }),
//         },
//         new[]
//         {
//             new Tuple<Equipment, EquipmentWriteDto>(new Equipment()
//             {
//                 Name = "straight bar",
//                 Description = "Home training bar",
//                 WeightKg = 10
//             }, new EquipmentWriteDto()
//             {
//                 Name = "Straight Bar",
//                 WeightKg = 101
//             }),
//         },
//     };
//
//
//     [Theory]
//     [MemberData(nameof(UpsertRecords))]
//     public async Task Upsert_UpdateShouldReturnSucess(Tuple<Equipment, EquipmentWriteDto> data)
//     {
//         var eq = data.Item1;
//         context.Equipment.Add(eq);
//         context.SaveChanges();
//         context.ChangeTracker.Clear();
//         
//         var n = data.Item2;
//
//         var result = await service.UpsertAsync(n, new CancellationToken());
//         Assert.True(result.IsSuccess);
//         Assert.True(result.Value >= 1);
//
//         
//         var newEquip = context.Equipment.First();
//         if(n.NewName is not null)
//             Assert.Equal(newEquip.Name, Utils.NormalizeString(n.NewName));
//         if (n.Description is not null)
//             Assert.Equal(newEquip.Description, n.Description);
//         if (n.WeightKg is not null)
//             Assert.Equal(newEquip.WeightKg, n.WeightKg);
//         
//         Assert.Equal(DateTime.UtcNow.Day, newEquip.CreatedAt.Day); // the day should match, it's created today after all.
//     }   public static IEnumerable<object[]> CreationBulkFaulty() => new List<object[]>()
//     {
//         new[]
//         {
//             new List<EquipmentWriteDto> ()
//             {
//                 new EquipmentWriteDto()
//                 {
//                     Name = "straight bar",
//                     Description = "Home training bar",
//                     WeightKg = 10
//                 },
//                 new EquipmentWriteDto()
//                 {
//                     Name = "plastic dumbbell",
//                     Description = "plastic dumbbell",
//                     WeightKg = 0.75
//                 },new EquipmentWriteDto()
//                 {
//                     Name = "Swiggly bar",
//                     Description = "Home Swiggly training bar",
//                     WeightKg = 7.5
//                 },new EquipmentWriteDto()
//                 {
//                     Name = "iron dumbbell",
//                     Description = "iron dumbbell. daaauh",
//                     WeightKg = 1.5
//                 }
//             }
//         },
//     };
//     
//     [Theory]
//     [MemberData(nameof(CreationBulkFaulty))]
//     public async Task CreateBulk_ShouldReturnSucess(List<EquipmentWriteDto> data)
//     {
//         var result = await service.CreateBulkAsync(data, new CancellationToken());
//         Assert.True(result.IsSuccess);
//         Assert.True(result.Value);
//
//         Assert.Equal(data.Count, context.Equipment.Count());
//         
//     }
//     
//     [Fact]
//     public async Task Delete_NoRecords_Failure()
//     {
//         var equipmentName = "canaan";
//         var result = await service.DeleteAsync(equipmentName, new CancellationToken());
//         Assert.False(result.IsSuccess);
//         Assert.Equal($"Equipment with the name: {equipmentName}, does not exists", result.ErrorMessage);
//     }
//
//     [Fact]
//     public async Task Delete_ExistingRecords_Sucess()
//     {
//         var n = new Equipment()
//         {
//             Name = "canaan",
//             Description = "who is making this",
//             WeightKg = 82.5
//         };
//         context.Equipment.Add(n);
//         context.SaveChanges();
//         context.ChangeTracker.Clear();
//         
//         
//         var equipmentName = "canaan";
//         var result = await service.DeleteAsync(equipmentName, new CancellationToken());
//         Assert.True(result.IsSuccess);
//         Assert.True(result.Value);
//         
//         Assert.Empty(context.Equipment);
//     }
//     
//     [Theory]
//     [InlineData(" ")]
//     [InlineData("\n\t\r")]
//     [InlineData(null)]
//     
//     public async Task Delete_ExistingRecords_FaultyName_returns_failure(string name)
//     {
//         
//         var result = await service.DeleteAsync(name, new CancellationToken());
//         Assert.False(result.IsSuccess);
//         Assert.NotNull(result.Exception);
//         Assert.Equal("Name cannot be empty!", result.Exception.Message );
//         Assert.IsType<ArgumentException>(result.Exception);
//
//     }
//     
// }