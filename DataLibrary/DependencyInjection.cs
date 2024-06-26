using DataLibrary.Context;
using DataLibrary.Interfaces;
using DataLibrary.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DataLibrary;
public static class DependencyInjection
{
    public static IServiceCollection AddDataLibrary(this IServiceCollection services)
    {
        services.AddDbContext<TrainingLogDbContext>(
            opt => opt.UseSqlite("Data Source=E://development//databases//training_log_db")
            );
        services.AddScoped<IPlanService, PlanService>();
        services.AddScoped<IMuscleService, MuscleService>();
        services.AddScoped<IExerciseService, ExerciseService>();
        services.AddScoped<ITrainingTypesService, TrainingTypesService>();

        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        return services;
    }

}
