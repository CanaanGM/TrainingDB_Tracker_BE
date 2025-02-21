using DataLibrary.Context;
using Microsoft.EntityFrameworkCore;

namespace TrainingTests.helpers;

public static class ProductionDatabaseHelpers
{
	private static string ReadSqlFile(string fileName)
	{
		return File.ReadAllText($"{PathHelpers.GetSolutionRoot()}\\Docs\\Sql\\{fileName}.sql");
	}
	public static void SeedProductionData(SqliteContext context)
	{

		context.Database.ExecuteSqlRaw(ReadSqlFile("muscle"));
		context.Database.ExecuteSqlRaw(ReadSqlFile("exercise"));
		context.Database.ExecuteSqlRaw(ReadSqlFile("training_type"));
		context.Database.ExecuteSqlRaw(ReadSqlFile("exercise_muscle"));
		context.Database.ExecuteSqlRaw(ReadSqlFile("exercise_how_to"));
		// context.Database.ExecuteSqlRaw(insertEquipment);
		context.Database.ExecuteSqlRaw(ReadSqlFile("exercise_type"));
	}

	/// <summary>
	/// creates 4 new users and assigns hashed passwords and salts.
	/// 1. Canaan, `كنعان لازم يتدرب !`,  salt:`$2a$11$hCdSz2IWtWhfSMu5HU1xe.`, hash:`$2a$11$hCdSz2IWtWhfSMu5HU1xe.YA6zrxged3TNHoZC/CycqNpaYS7ci4W`
	/// 2. Dante, `pizza is pizza!`, salt:`$2a$11$v68jMQkfWr9OS4BHPe20ke`, hash:`2a$11$v68jMQkfWr9OS4BHPe20keuztD79mByxoBc2OJFOvO0dBBXPlmQ4e`
	/// 3. Alphrad, `sneaky snake`, salt:`$2a$11$FHNqTyAalmLYbaOpwJ683O`, hash:`$2a$11$FHNqTyAalmLYbaOpwJ683OY7krQV58AT94Vc6cICI3ihcP4A2jIwG`
	/// 4. Nero, `ろまである!`, salt:`$2a$11$YyB7Yu/pMRy/8xHEHlWJgO`, hash:`$2a$11$YyB7Yu/pMRy/8xHEHlWJgOUfKUpJwBAq4Im.leW/gTWDzOatDvqai` 
	/// </summary>
	/// <param name="context"></param>
	public static void SeedDummyUsers(SqliteContext context)
	{
		context.Database.ExecuteSqlRaw(ReadSqlFile("users_and_roles"));
	}

	public static void SeedRoles(SqliteContext context)
	{
		context.Database.ExecuteSqlRaw(ReadSqlFile("roles"));
	}

	public static void SeedMeasurements(SqliteContext context)
	{
		context.Database.ExecuteSqlRaw(ReadSqlFile("measurements"));
	}
}