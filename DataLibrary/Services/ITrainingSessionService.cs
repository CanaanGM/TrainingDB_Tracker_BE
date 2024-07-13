using DataLibrary.Core;
using DataLibrary.Dtos;

namespace DataLibrary.Services;

public interface ITrainingSessionService
{
    Task<Result<int>> CreateSessionAsync(TrainingSessionWriteDto newSession, CancellationToken cancellationToken);
    Task<Result<bool>> DeleteSessionAsync(int sessionId, CancellationToken cancellationToken);
    Task<Result<List<TrainingSessionReadDto>>> GetTrainingSessionsAsync(string? startDate, string? endDate, CancellationToken cancellationToken);
    Task<Result<bool>> UpdateSessionAsync(int sessionId, TrainingSessionWriteDto updateDto, CancellationToken cancellationToken);
}