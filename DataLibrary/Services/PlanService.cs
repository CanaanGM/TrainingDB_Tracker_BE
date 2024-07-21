using DataLibrary.Models;

namespace DataLibrary.Services;

public class PlanService
{
    public async Task CreateAsync(CancellationToken cancellationToken)
    {
        var plan = new TrainingPlan
        {
            Name = "V3-Advanced",
            Description = " a body building plan that requires the gym",
            Notes = "Choose a weight that is 70~75% of a weight you can do once, if you can left 20kg once, train with 15~16kg.",
            Equipment = new List<Equipment>(),
            TrainingTypes = new List<TrainingType>(),
            TrainingWeeksNavigation = new List<TrainingWeek>(),
            TrainingWeeks = 4,
            TrainingDaysPerWeek = 5,
        };
    }
}