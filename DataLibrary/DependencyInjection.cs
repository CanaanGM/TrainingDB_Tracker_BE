using DataLibrary.Context;
using DataLibrary.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DataLibrary;
public static class DependencyInjection
{
    public static IServiceCollection AddDataLibrary(
        this IServiceCollection services,
        string connectionString = "Data Source = E:\\development\\c#\\TrainingDB_Integration\\training_log_v2.db"
        )
    {
        services.AddDbContext<SqliteContext>(opt => opt.UseSqlite(
            connectionString
            ));

        services.AddScoped<IMuscleService, MuscleService>();
        services.AddScoped<ITrainingTypesService, TrainingTypesService>();
        services.AddScoped<IExerciseService, ExerciseService>();
        services.AddScoped<IMeasurementsService, MeasurementsService>();
        services.AddScoped<IEquipmentService, EquipmentService>();
        services.AddScoped<IPlanService, PlanService>();
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        services.AddScoped<ITrainingSessionService, TrainingSessionService>();
        return services;
    }
}
