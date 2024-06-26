using DataLibrary.Context;
using DataLibrary.Interfaces;
using DataLibrary.Models;

namespace DataLibrary.Services;
public class PlanService : IPlanService
{
    private readonly TrainingLogDbContext _context;

    public PlanService(TrainingLogDbContext context)
    {
        _context = context;
    }
    public List<TrainingPlan> Get()
    {
        return _context.TrainingPlans.ToList();
    }
}
