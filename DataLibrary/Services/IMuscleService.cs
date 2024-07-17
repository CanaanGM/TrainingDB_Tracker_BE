using DataLibrary.Core;
using DataLibrary.Dtos;

namespace DataLibrary.Services;

public interface IMuscleService
{
    Task<Result<bool>> CreateBulkAsync(ICollection<MuscleWriteDto> newMuscles, CancellationToken cancellationToken);
    Task<Result<int>> CreateMuscleAsync(MuscleWriteDto newMuscle, CancellationToken cancellationToken);
    Task<Result<bool>> DeleteAsync(int muscleId, CancellationToken cancellationToken);
    Task<Result<List<MuscleReadDto>>> GetAllAsync(CancellationToken cancellationToken);
    Task<Result<List<MuscleReadDto>>> GetAllByGroupAsync(string muscleGroupName, CancellationToken cancellationToken);
    Task<Result<MuscleReadDto>> GetByNameAsync(string muscleName, CancellationToken cancellationToken);
    Task<Result<bool>> UpdateAsync(int muscleId, MuscleUpdateDto updatedMuscle, CancellationToken cancellationToken);

    Task<Result<List<MuscleReadDto>>> SearchMuscleAsync(string searchTerm,
        CancellationToken cancellationToken);
}