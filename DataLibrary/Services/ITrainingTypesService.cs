using DataLibrary.Core;
using DataLibrary.Dtos;

namespace DataLibrary.Services;

public interface ITrainingTypesService
{
    Task<Result<bool>> CreateAsync(TrainingTypeWriteDto newTrainingType, CancellationToken cancellationToken);
    Task<Result<bool>> CreateBulkAsync(HashSet<TrainingTypeWriteDto> newTypes, CancellationToken cancellationToken);
    Task<Result<List<TrainingTypeReadDto>>> GetAllAsync(CancellationToken cancellationToken);
    Task<Result<bool>> Update(int typeId, TrainingTypeWriteDto updatedType, CancellationToken cancellationToken);
}