using AutoMapper;

using DataLibrary.Context;
using DataLibrary.Dtos;
using DataLibrary.Interfaces;
using DataLibrary.Models;

namespace DataLibrary.Services;
internal class TrainingTypesService : ITrainingTypesService
{
    private readonly TrainingLogDbContext _context;
    private readonly IMapper _mapper;

    public TrainingTypesService(
        TrainingLogDbContext context,
        IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }


    // get all 
    public List<TypeReadDto> Get()
    {
        return _mapper.Map<List<TypeReadDto>>(_context.TrainingTypes.ToList());
    }
    // get one
    // update one
    // delete one
    // insert one
    // insert bulk

    public async Task InsertBulkAsync(List<TypeWriteDto> types, CancellationToken cancellationToken)
    {
        // 2 steps are easier to debug!
        List<TrainingType> trainingTypes = _mapper.Map<List<TrainingType>>(types);

        try
        {
            await _context.TrainingTypes.AddRangeAsync(trainingTypes, cancellationToken);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {

            throw ex;
        }

    }
}
