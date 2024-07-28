using AutoMapper;
using AutoMapper.QueryableExtensions;
using DataLibrary.Context;
using DataLibrary.Core;
using DataLibrary.Dtos;
using DataLibrary.Helpers;
using DataLibrary.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataLibrary.Services;

public interface ILanguageService
{
    Task<Result> CreateBulkAsync(List<LanguageWriteDto> newLanguagesDtos, CancellationToken cancellationToken);
    Task<Result<List<LanguageReadDto>>> GetAllAsync(CancellationToken cancellationToken);
    Task<Result> UpdateAsync(LanguageReadDto languageDto, CancellationToken cancellationToken);
    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken);
}

public class LanguageService(SqliteContext context, ILogger<LanguageService> logger, IMapper mapper) : ILanguageService
{
    public async Task<Result> CreateBulkAsync(List<LanguageWriteDto> newLanguagesDtos, CancellationToken cancellationToken)
    {
        if (newLanguagesDtos is null || newLanguagesDtos.Any(dto => !IsValid(dto)))
        {
            logger.LogWarning("Invalid language DTOs provided.");
            return Result.Failure("Invalid language DTOs provided.");
        }
        
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var newLanguages = newLanguagesDtos.Select(x => new Language
                {
                    Name = Utils.NormalizeString(x.Name),
                    Code = Utils.NormalizeString(x.Code)
                })
            .ToList();

            await context.Languages.AddRangeAsync(newLanguages, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            logger.LogInformation("Successfully created new languages.");
            return Result.Success();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            logger.LogError($"[ERROR]: could not create new languages: {ex}");
            return Result.Failure($"could not create new languages:{ex.Message}", ex);
        }
    }
    
    public async Task<Result<List<LanguageReadDto>>> GetAllAsync(CancellationToken cancellationToken)
    {
        try
        {
            var languages = await context.Languages
                .ProjectTo<LanguageReadDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return Result<List<LanguageReadDto>>.Success(languages);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving all languages.");
            return Result<List<LanguageReadDto>>.Failure("Error retrieving all languages.", ex);
        }
    }
       public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var language = await context.Languages.FindAsync(new object[] { id }, cancellationToken);
            if (language == null)
            {
                logger.LogWarning("Language with ID {Id} not found.", id);
                return Result.Failure("Language not found.");
            }

            context.Languages.Remove(language);
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            logger.LogInformation("Successfully deleted language with ID {Id}.", id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            logger.LogError(ex, "Error deleting language with ID {Id}.", id);
            return Result.Failure("Error deleting language.", ex);
        }
    }

    public async Task<Result> UpdateAsync(LanguageReadDto languageDto, CancellationToken cancellationToken)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var language = await context.Languages.FindAsync(new object[] { languageDto.Id }, cancellationToken);
            if (language == null)
            {
                logger.LogWarning("Language with ID {Id} not found.", languageDto.Id);
                return Result.Failure("Language not found.");
            }

            language.Code = Utils.NormalizeString(languageDto.Code);
            language.Name = Utils.NormalizeString(languageDto.Name);

            context.Languages.Update(language);
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            logger.LogInformation("Successfully updated language with ID {Id}.", languageDto.Id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            logger.LogError(ex, "Error updating language with ID {Id}.", languageDto.Id);
            return Result.Failure("Error updating language.", ex);
        }
    }
    private static bool IsValid(LanguageWriteDto dto)
    {
        return !string.IsNullOrWhiteSpace(dto.Code) && dto.Code.Length is >= 2 and <= 20 &&
               !string.IsNullOrWhiteSpace(dto.Name) && dto.Name.Length is >= 3 and <= 50;
    }
}