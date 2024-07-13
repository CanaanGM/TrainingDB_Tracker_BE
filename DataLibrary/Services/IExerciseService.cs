using DataLibrary.Core;
using DataLibrary.Dtos;

namespace DataLibrary.Services;

public interface IExerciseService
{
    Task<Result<bool>> CreateAsync(ExerciseWriteDto newExerciseDto, CancellationToken cancellationToken);
    Task<Result<bool>> CreateBulkAsync(List<ExerciseWriteDto> newExerciseDtos, CancellationToken cancellationToken);
    Task<Result<bool>> DeleteExerciseAsync(int exerciseId, CancellationToken cancellationToken);
    Task<Result<PaginatedList<ExerciseReadDto>>> GetAsync(ExerciseQueryOptions options, CancellationToken cancellationToken);
    Task<Result<Dictionary<string, List<ExerciseReadDto>>>> GetByGroupAsync(ExerciseQueryOptions options, CancellationToken cancellationToken = default);
    Task<Result<ExerciseReadDto>> GetByNameAsync(string exerciseName, CancellationToken cancellationToken);
    Task<Result<Dictionary<string, List<ExerciseReadDto>>>> GetExercisesGroupedByTrainingTypeAsync(ExerciseQueryOptions options, CancellationToken cancellationToken);
    Task<Result<bool>> UpdateAsync(int exerciseId, ExerciseWriteDto exerciseDto, CancellationToken cancellationToken);
}