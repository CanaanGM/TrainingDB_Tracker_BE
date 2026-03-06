using AutoMapper;
using AutoMapper.QueryableExtensions;

using DataLibrary.Context;
using DataLibrary.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using SharedLibrary.Core;
using SharedLibrary.Dtos;
using SharedLibrary.Helpers;

namespace DataLibrary.Services;

public interface IUserService
{
    // Auth + users
    Task<Result<UserAuthDto>> CreateUserAsync(UserWriteDto userDto, CancellationToken cancellationToken);
    Task<Result<InternalUserAuthDto>> GetUserWithRolesByEmailAsync(string email, CancellationToken cancellationToken);
    Task<Result> CreateRefreshTokenForUser(string userEmail, string token, CancellationToken cancellationToken);
    Task<Result<InternalUserAuthDto>> RotateRefreshTokenAsync(string currentToken, RefreshToken newRefreshToken, CancellationToken cancellationToken);
    Task<Result> RevokeRefreshTokenAsync(string token, CancellationToken cancellationToken);

    // Plans
    Task<Result<TrainingPlanReadDto>> GetActiveTrainingPlanAsync(int userId, CancellationToken cancellationToken);
    Task<Result> SetActiveTrainingPlanAsync(int userId, int trainingPlanId, CancellationToken cancellationToken);
    Task<Result> SetActiveTrainingPlanDatesAsync(int userId, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken);
    Task<Result> StartTrainingPlanAsync(int userId, int trainingPlanId, DateTime? startDate, CancellationToken cancellationToken);
    Task<Result> FinishTrainingPlanAsync(int userId, int trainingPlanId, DateTime? endDate, CancellationToken cancellationToken);

    // Profile
    Task<Result<User>> GetUserProfileAsync(int userId, CancellationToken cancellationToken);
    Task<Result> UpdateUserProfileAsync(int userId, string? username, double? height, string? gender, CancellationToken cancellationToken);

    // Password
    Task<Result> CreatePasswordAsync(int userId, string newPassword, CancellationToken cancellationToken);
    Task<Result> UpdatePasswordAsync(int userId, string newPassword, CancellationToken cancellationToken);
    Task<Result> ChangePasswordAsync(int userId, string currentPassword, string newPassword, CancellationToken cancellationToken);

    // Images
    Task<Result<List<UserProfileImage>>> GetUserImagesAsync(int userId, CancellationToken cancellationToken);
    Task<Result<UserProfileImage>> AddUserImageAsync(int userId, string imageUrl, bool isPrimary, CancellationToken cancellationToken);
    Task<Result> UpdateUserImageAsync(int userId, int imageId, string? imageUrl, bool? isPrimary, CancellationToken cancellationToken);
    Task<Result> DeleteUserImageAsync(int userId, int imageId, CancellationToken cancellationToken);
    Task<Result> SetPrimaryUserImageAsync(int userId, int imageId, CancellationToken cancellationToken);

    // Muscles
    Task<Result<List<MuscleReadDto>>> GetUserMusclesAsync(int userId, CancellationToken cancellationToken);

    // Cool down + frequency
    Task<Result<double>> CalculateCoolDownMinutesAsync(int userId, CancellationToken cancellationToken);
    Task<Result<double>> GetTrainingFrequencyPerWeekAsync(int userId, int weeks, CancellationToken cancellationToken);

    // Exercises
    Task<Result<List<UserExercise>>> GetUserExercisesAsync(int userId, CancellationToken cancellationToken);
}

public class UserService : IUserService
{
    private readonly SqliteContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;
    private readonly IAuthService _authService;

    private static string HashToken(string token)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var hash = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(token));
        return System.Convert.ToBase64String(hash).Replace('+','-').Replace('/','_').Replace("=", "");
    }

    public UserService(SqliteContext context, IMapper mapper, ILogger<UserService> logger, IAuthService authService)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _authService = authService;
    }

    // Back-compat for tests/new callers not yet updated
    public UserService(SqliteContext context, IMapper mapper, ILogger<UserService> logger) : this(context, mapper, logger, new AuthService(context, mapper))
    {
    }
    public Task<Result<UserAuthDto>> CreateUserAsync(UserWriteDto userDto, CancellationToken cancellationToken) => _authService.CreateUserAsync(userDto, cancellationToken);
    public Task<Result<InternalUserAuthDto>> GetUserWithRolesByEmailAsync(string email, CancellationToken cancellationToken) => _authService.GetUserWithRolesByEmailAsync(email, cancellationToken);
    public Task<Result> CreateRefreshTokenForUser(string userEmail, string token, CancellationToken cancellationToken) => _authService.CreateRefreshTokenForUser(userEmail, token, cancellationToken);
    public Task<Result<InternalUserAuthDto>> RotateRefreshTokenAsync(string currentToken, RefreshToken newRefreshToken, CancellationToken cancellationToken) => _authService.RotateRefreshTokenAsync(currentToken, newRefreshToken, cancellationToken);
    // user plans
    public async Task<Result<TrainingPlanReadDto>> GetActiveTrainingPlanAsync(int userId, CancellationToken cancellationToken)
    {
        try
        {
            var link = await _context.UserTrainingPlans.AsNoTracking()
                .Where(x => x.UserId == userId && x.IsActive == true && (x.IsFinished == null || x.IsFinished == false))
                .OrderByDescending(x => x.EnrolledDate)
                .FirstOrDefaultAsync(cancellationToken);

            if (link is null)
                return Result<TrainingPlanReadDto>.Failure("no active plan for user");

            var dto = await _context.TrainingPlans
                .Where(p => p.Id == link.TrainingPlanId)
                .ProjectTo<TrainingPlanReadDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

            return dto is null
                ? Result<TrainingPlanReadDto>.Failure("plan not found")
                : Result<TrainingPlanReadDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "error in {Method}", nameof(GetActiveTrainingPlanAsync));
            return Result<TrainingPlanReadDto>.Failure("failed to get active plan", ex);
        }
    }

    public async Task<Result> SetActiveTrainingPlanAsync(int userId, int trainingPlanId, CancellationToken cancellationToken)
    {
        try
        {
            await using var tx = await _context.Database.BeginTransactionAsync(cancellationToken);

            var plan = await _context.TrainingPlans.AsNoTracking().FirstOrDefaultAsync(p => p.Id == trainingPlanId, cancellationToken);
            if (plan is null) return Result.Failure("training plan not found");

            var links = await _context.UserTrainingPlans.Where(x => x.UserId == userId).ToListAsync(cancellationToken);
            foreach (var l in links)
            {
                l.IsActive = false;
            }

            var existing = links.FirstOrDefault(x => x.TrainingPlanId == trainingPlanId);
            if (existing is null)
            {
                var newLink = new UserTrainingPlan
                {
                    UserId = userId,
                    TrainingPlanId = trainingPlanId,
                    IsActive = true,
                    IsFinished = false,
                    EnrolledDate = DateTime.UtcNow
                };
                _context.UserTrainingPlans.Add(newLink);
            }
            else
            {
                existing.IsActive = true;
                existing.IsFinished = false;
                existing.EnrolledDate ??= DateTime.UtcNow;
                _context.UserTrainingPlans.Update(existing);
            }

            await _context.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "error in {Method}", nameof(SetActiveTrainingPlanAsync));
            return Result.Failure("failed to set active plan", ex);
        }
    }

    public async Task<Result> SetActiveTrainingPlanDatesAsync(int userId, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken)
    {
        try
        {
            var link = await _context.UserTrainingPlans.FirstOrDefaultAsync(x => x.UserId == userId && x.IsActive == true, cancellationToken);
            if (link is null) return Result.Failure("no active plan to update dates for");
            link.StartDate = startDate;
            link.EndDate = endDate;
            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "error in {Method}", nameof(SetActiveTrainingPlanDatesAsync));
            return Result.Failure("failed to update plan dates", ex);
        }
    }

    public async Task<Result> StartTrainingPlanAsync(int userId, int trainingPlanId, DateTime? startDate, CancellationToken cancellationToken)
    {
        try
        {
            await using var tx = await _context.Database.BeginTransactionAsync(cancellationToken);
            var link = await _context.UserTrainingPlans.FirstOrDefaultAsync(x => x.UserId == userId && x.TrainingPlanId == trainingPlanId, cancellationToken);
            if (link is null)
            {
                link = new UserTrainingPlan
                {
                    UserId = userId,
                    TrainingPlanId = trainingPlanId,
                    EnrolledDate = DateTime.UtcNow
                };
                _context.UserTrainingPlans.Add(link);
            }
            link.IsActive = true;
            link.IsFinished = false;
            link.StartDate = startDate ?? DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "error in {Method}", nameof(StartTrainingPlanAsync));
            return Result.Failure("failed to start plan", ex);
        }
    }

    public async Task<Result> FinishTrainingPlanAsync(int userId, int trainingPlanId, DateTime? endDate, CancellationToken cancellationToken)
    {
        try
        {
            var link = await _context.UserTrainingPlans.FirstOrDefaultAsync(x => x.UserId == userId && x.TrainingPlanId == trainingPlanId, cancellationToken);
            if (link is null) return Result.Failure("user not enrolled in plan");
            link.IsActive = false;
            link.IsFinished = true;
            link.EndDate = endDate ?? DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "error in {Method}", nameof(FinishTrainingPlanAsync));
            return Result.Failure("failed to finish plan", ex);
        }
    }
    // user profile
    public async Task<Result<User>> GetUserProfileAsync(int userId, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
            return user is null ? Result<User>.Failure("user not found") : Result<User>.Success(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "error in {Method}", nameof(GetUserProfileAsync));
            return Result<User>.Failure("failed to get user profile", ex);
        }
    }

    public async Task<Result> UpdateUserProfileAsync(int userId, string? username, double? height, string? gender, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
            if (user is null) return Result.Failure("user not found");
            if (!string.IsNullOrWhiteSpace(username)) user.Username = username!;
            if (height.HasValue) user.Height = height;
            if (!string.IsNullOrWhiteSpace(gender)) user.Gender = gender;
            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "error in {Method}", nameof(UpdateUserProfileAsync));
            return Result.Failure("failed to update user profile", ex);
        }
    }
    // user password
    public async Task<Result> CreatePasswordAsync(int userId, string newPassword, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _context.Users.Include(u => u.UserPasswords).FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
            if (user is null) return Result.Failure("user not found");
            if (user.UserPasswords.Any(up => up.IsCurrent == true))
                return Result.Failure("password already set; use change or update");

            var salt = SecurityHelpers.GenerateSalt();
            var hashed = SecurityHelpers.HashPassword(newPassword, salt);
            if (!hashed.IsSuccess) return Result.Failure(hashed.ErrorMessage!);

            user.UserPasswords.Add(new UserPassword
            {
                IsCurrent = true,
                CreatedAt = DateTime.UtcNow,
                PasswordHash = hashed.Value!,
                PasswordSalt = salt,
                User = user
            });
            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "error in {Method}", nameof(CreatePasswordAsync));
            return Result.Failure("failed to create password", ex);
        }
    }

    public async Task<Result> UpdatePasswordAsync(int userId, string newPassword, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _context.Users.Include(u => u.UserPasswords).FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
            if (user is null) return Result.Failure("user not found");

            foreach (var p in user.UserPasswords) p.IsCurrent = false;
            var salt = SecurityHelpers.GenerateSalt();
            var hashed = SecurityHelpers.HashPassword(newPassword, salt);
            if (!hashed.IsSuccess) return Result.Failure(hashed.ErrorMessage!);

            user.UserPasswords.Add(new UserPassword
            {
                IsCurrent = true,
                CreatedAt = DateTime.UtcNow,
                PasswordHash = hashed.Value!,
                PasswordSalt = salt,
                User = user
            });
            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "error in {Method}", nameof(UpdatePasswordAsync));
            return Result.Failure("failed to update password", ex);
        }
    }

    public async Task<Result> ChangePasswordAsync(int userId, string currentPassword, string newPassword, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _context.Users.Include(u => u.UserPasswords.Where(x => x.IsCurrent == true)).FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
            if (user is null) return Result.Failure("user not found");
            var current = user.UserPasswords.FirstOrDefault();
            if (current is null) return Result.Failure("no current password set");
            var verified = SecurityHelpers.VerifyPassword(currentPassword, current.PasswordHash);
            if (!verified.IsSuccess) return Result.Failure("invalid current password");

            foreach (var p in user.UserPasswords) p.IsCurrent = false;
            var salt = SecurityHelpers.GenerateSalt();
            var hashed = SecurityHelpers.HashPassword(newPassword, salt);
            if (!hashed.IsSuccess) return Result.Failure(hashed.ErrorMessage!);

            user.UserPasswords.Add(new UserPassword
            {
                IsCurrent = true,
                CreatedAt = DateTime.UtcNow,
                PasswordHash = hashed.Value!,
                PasswordSalt = salt,
                User = user
            });
            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "error in {Method}", nameof(ChangePasswordAsync));
            return Result.Failure("failed to change password", ex);
        }
    }
    // user images
    public async Task<Result<List<UserProfileImage>>> GetUserImagesAsync(int userId, CancellationToken cancellationToken)
    {
        try
        {
            var images = await _context.UserProfileImages.Where(i => i.UserId == userId).OrderByDescending(i => i.IsPrimary == true).ThenByDescending(i => i.CreatedAt).ToListAsync(cancellationToken);
            return Result<List<UserProfileImage>>.Success(images);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "error in {Method}", nameof(GetUserImagesAsync));
            return Result<List<UserProfileImage>>.Failure("failed to get images", ex);
        }
    }

    public async Task<Result<UserProfileImage>> AddUserImageAsync(int userId, string imageUrl, bool isPrimary, CancellationToken cancellationToken)
    {
        try
        {
            await using var tx = await _context.Database.BeginTransactionAsync(cancellationToken);
            if (isPrimary)
            {
                var existingPrimary = await _context.UserProfileImages.Where(i => i.UserId == userId && i.IsPrimary == true).ToListAsync(cancellationToken);
                foreach (var img in existingPrimary) img.IsPrimary = false;
            }
            var imgToAdd = new UserProfileImage { UserId = userId, Url = imageUrl, IsPrimary = isPrimary, CreatedAt = DateTime.UtcNow };
            _context.UserProfileImages.Add(imgToAdd);
            await _context.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
            return Result<UserProfileImage>.Success(imgToAdd);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "error in {Method}", nameof(AddUserImageAsync));
            return Result<UserProfileImage>.Failure("failed to add image", ex);
        }
    }

    public async Task<Result> UpdateUserImageAsync(int userId, int imageId, string? imageUrl, bool? isPrimary, CancellationToken cancellationToken)
    {
        try
        {
            await using var tx = await _context.Database.BeginTransactionAsync(cancellationToken);
            var image = await _context.UserProfileImages.FirstOrDefaultAsync(i => i.Id == imageId && i.UserId == userId, cancellationToken);
            if (image is null) return Result.Failure("image not found");

            if (!string.IsNullOrWhiteSpace(imageUrl)) image.Url = imageUrl;
            if (isPrimary.HasValue && isPrimary.Value)
            {
                var existingPrimary = await _context.UserProfileImages.Where(i => i.UserId == userId && i.IsPrimary == true && i.Id != imageId).ToListAsync(cancellationToken);
                foreach (var img in existingPrimary) img.IsPrimary = false;
                image.IsPrimary = true;
            }
            await _context.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "error in {Method}", nameof(UpdateUserImageAsync));
            return Result.Failure("failed to update image", ex);
        }
    }

    public async Task<Result> DeleteUserImageAsync(int userId, int imageId, CancellationToken cancellationToken)
    {
        try
        {
            var image = await _context.UserProfileImages.FirstOrDefaultAsync(i => i.Id == imageId && i.UserId == userId, cancellationToken);
            if (image is null) return Result.Failure("image not found");
            _context.UserProfileImages.Remove(image);
            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "error in {Method}", nameof(DeleteUserImageAsync));
            return Result.Failure("failed to delete image", ex);
        }
    }

    public async Task<Result> SetPrimaryUserImageAsync(int userId, int imageId, CancellationToken cancellationToken)
    {
        try
        {
            await using var tx = await _context.Database.BeginTransactionAsync(cancellationToken);
            var image = await _context.UserProfileImages.FirstOrDefaultAsync(i => i.Id == imageId && i.UserId == userId, cancellationToken);
            if (image is null) return Result.Failure("image not found");

            var existingPrimary = await _context.UserProfileImages.Where(i => i.UserId == userId && i.IsPrimary == true && i.Id != imageId).ToListAsync(cancellationToken);
            foreach (var img in existingPrimary) img.IsPrimary = false;
            image.IsPrimary = true;
            await _context.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "error in {Method}", nameof(SetPrimaryUserImageAsync));
            return Result.Failure("failed to set primary image", ex);
        }
    }
    // user muscles
    public async Task<Result<List<MuscleReadDto>>> GetUserMusclesAsync(int userId, CancellationToken cancellationToken)
    {
        try
        {
            var exerciseIds = await _context.UserExercises
                .Where(ue => ue.UserId == userId && ue.ExerciseId != null)
                .Select(ue => ue.ExerciseId!.Value)
                .Distinct()
                .ToListAsync(cancellationToken);

            if (exerciseIds.Count == 0)
                return Result<List<MuscleReadDto>>.Success(new List<MuscleReadDto>());

            var muscles = await _context.ExerciseMuscles
                .Include(em => em.Muscle)
                .Where(em => exerciseIds.Contains(em.ExerciseId) && em.Muscle != null)
                .Select(em => em.Muscle!)
                .ToListAsync(cancellationToken);

            var distinct = muscles
                .GroupBy(m => m.Id)
                .Select(g => g.First())
                .ToList();

            var dto = _mapper.Map<List<MuscleReadDto>>(distinct);
            return Result<List<MuscleReadDto>>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "error in {Method}", nameof(GetUserMusclesAsync));
            return Result<List<MuscleReadDto>>.Failure("failed to get user muscles", ex);
        }
    }

    // cool down calculation (simple heuristic)
    //! need more insight here 
    public async Task<Result<double>> CalculateCoolDownMinutesAsync(int userId, CancellationToken cancellationToken)
    {
        try
        {
            var sessions = await _context.TrainingSessions
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .Take(5)
                .ToListAsync(cancellationToken);

            if (sessions.Count == 0) return Result<double>.Success(5d);

            var avgRpe = sessions.Average(s => s.AverageRateOfPerceivedExertion ?? 5);
            var avgDurationMin = sessions.Average(s => s.DurationInSeconds) / 60.0;
            var cooldown = Math.Clamp((avgRpe / 10.0) * 10.0 + Math.Min(avgDurationMin / 30.0 * 5.0, 10.0), 5.0, 20.0);
            return Result<double>.Success(Math.Round(cooldown, 1));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "error in {Method}", nameof(CalculateCoolDownMinutesAsync));
            return Result<double>.Failure("failed to calculate cooldown", ex);
        }
    }

    // frequency: sessions per week over a window
    public async Task<Result<double>> GetTrainingFrequencyPerWeekAsync(int userId, int weeks, CancellationToken cancellationToken)
    {
        try
        {
            if (weeks <= 0) weeks = 4;
            var since = DateTime.UtcNow.AddDays(-7 * weeks);
            var count = await _context.TrainingSessions.CountAsync(s => s.UserId == userId && s.CreatedAt >= since, cancellationToken);
            var freq = count / (double)weeks;
            return Result<double>.Success(Math.Round(freq, 2));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "error in {Method}", nameof(GetTrainingFrequencyPerWeekAsync));
            return Result<double>.Failure("failed to compute frequency", ex);
        }
    }

    // user exercises summary
    public async Task<Result<List<UserExercise>>> GetUserExercisesAsync(int userId, CancellationToken cancellationToken)
    {
        try
        {
            var list = await _context.UserExercises
                .Include(ue => ue.Exercise)
                .Where(ue => ue.UserId == userId)
                .OrderByDescending(ue => ue.UseCount)
                .ToListAsync(cancellationToken);
            return Result<List<UserExercise>>.Success(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "error in {Method}", nameof(GetUserExercisesAsync));
            return Result<List<UserExercise>>.Failure("failed to get user exercises", ex);
        }
    }

    public Task<Result> RevokeRefreshTokenAsync(string token, CancellationToken cancellationToken) => _authService.RevokeRefreshTokenAsync(token, cancellationToken);
}





