using DataLibrary.Core;
using DataLibrary.Dtos;

namespace DataLibrary.Services;

public interface IExerciseService
{
    Task<Result<int>> CreateAsync(ExerciseWriteDto newExerciseDto, CancellationToken cancellationToken);
    Task<Result<bool>> CreateBulkAsync(List<ExerciseWriteDto> newExerciseDtos, CancellationToken cancellationToken);
    Task<Result<bool>> DeleteExerciseAsync(int exerciseId, CancellationToken cancellationToken);
    Task<Result<ExerciseReadDto>> GetByNameAsync(string exerciseName, CancellationToken cancellationToken);
    Task<Result<bool>> UpdateAsync(int exerciseId, ExerciseWriteDto exerciseDto, CancellationToken cancellationToken);
    Task<Result<List<ExerciseSearchResultDto>>> SearchExercisesAsync(string searchTerm, CancellationToken cancellationToken);
    Task<Result<PaginatedList<ExerciseReadDto>>> GetAllAsync(
        ExerciseQueryOptions options,
        CancellationToken cancellationToken);
}