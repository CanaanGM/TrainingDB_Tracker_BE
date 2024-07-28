using DataLibrary.Context;
using DataLibrary.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DataLibrary;
public static class DependencyInjection
{
    public static IServiceCollection AddDataLibrary(this IServiceCollection services)
    {
        services.AddDbContext<SqliteContext>(opt => opt.UseSqlite(
                "Data Source=E:/development/databases/training_log_v2.db"
            ));

        services.AddScoped<ILanguageService, LanguageService>();
        services.AddScoped<IEquipmentService, EquipmentService>();
        // services.AddScoped<IMuscleService, MuscleService>();
        // services.AddScoped<ITrainingTypesService, TrainingTypesService>();
        // services.AddScoped<IExerciseService, ExerciseService>();
        // services.AddScoped<IMeasurementsService, MeasurementsService>();
        // services.AddScoped<IPlanService, PlanService>();
        // services.AddScoped<ITrainingSessionService, TrainingSessionService>();
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        return services;
    }
}
