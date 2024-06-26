using AutoMapper;
using AutoMapper.QueryableExtensions;

using DataLibrary.Context;
using DataLibrary.Dtos;
using DataLibrary.Interfaces;
using DataLibrary.Models;

using Microsoft.EntityFrameworkCore;

namespace DataLibrary.Services;
internal class ExerciseService : IExerciseService
{
    private readonly TrainingLogDbContext _context;
    private readonly IMapper _mapper;

    public ExerciseService(TrainingLogDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }


    public List<ExerciseReadDto> Get()
    {
        return _context.Exercises
            .AsNoTracking()
            .ProjectTo<ExerciseReadDto>(_mapper.ConfigurationProvider)
            .ToList();
    }

    public ExerciseReadDto GetById(int id)
    {
        return _context.Exercises
            .AsNoTracking()
            .ProjectTo<ExerciseReadDto>(_mapper.ConfigurationProvider)
            .First();

        //.Where(x => x.Id == id)
        //.Include(x => x.ExerciseMuscles)
        //.Include(r => r.ExerciseTypes)
        //.FirstOrDefault();
    }

    public void AddExercise(ExerciseWriteDto exerciseDto)
    {
        List<string> exerciseMuscleGroups = exerciseDto.MuscleGroups;
        List<int> muscleGroupsIds = new();
        List<ExerciseHowTo> HowTos = _mapper.Map<List<ExerciseHowTo>>(exerciseDto.HowToLinks);
        List<int> exerciseTypes = new();
        // get the group's ids
        foreach (string muscleGroupName in exerciseMuscleGroups)
        {
            try
            {
                Models.MuscleGroup? muscleGroup = _context.MuscleGroups
                    .AsNoTracking()
                    .Where(x => x.CommonName == muscleGroupName)
                    .FirstOrDefault();
                if (muscleGroup is null)
                    throw new Exception("Change me later!!\nWrong Muscle Group Name!");

                muscleGroupsIds.Add(muscleGroup.Id);

            }
            catch (Exception)
            {
                // return a Result later!
                throw;
            }
        }

        foreach (string t in exerciseDto.Types)
        {
            try
            {
                TrainingType? type = _context.TrainingTypes
                    .AsNoTracking()
                    .SingleOrDefault(e => e.Name == t);
                if (type is null)
                    throw new Exception();

                exerciseTypes.Add(type.Id);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        Exercise newExercise = new Exercise
        {
            Name = exerciseDto.Name,
            Description = exerciseDto.Description,
            //ExerciseTypes

        };

    }
}
