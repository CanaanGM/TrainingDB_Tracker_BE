using AutoMapper;
using AutoMapper.QueryableExtensions;

using DataLibrary.Context;
using DataLibrary.Dtos;
using DataLibrary.Interfaces;

namespace DataLibrary.Services;
public class MuscleService : IMuscleService
{
    private readonly TrainingLogDbContext _context;
    private readonly IMapper _mapper;

    public MuscleService(TrainingLogDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public List<MuscleReadDto> Get()
    {
        return _context.Muscles
            .ProjectTo<MuscleReadDto>(_mapper.ConfigurationProvider)
            .ToList();
        ;


    }
}
