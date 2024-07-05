﻿using AutoMapper;

using DataLibrary.Context;
using DataLibrary.Core;
using DataLibrary.Dtos;
using DataLibrary.Helpers;
using DataLibrary.Models;

using Microsoft.EntityFrameworkCore;

namespace DataLibrary.Services;
internal class MuscleSercive : IMuscleService
{
    private readonly SqliteContext _context;
    private readonly IMapper _mapper;

    public MuscleSercive(SqliteContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<List<MuscleReadDto>>> GetAllAsync(CancellationToken cancellationToken)
    {
        return Result<List<MuscleReadDto>>.Success(
            _mapper.Map<List<MuscleReadDto>>(
                await _context.Muscles
                    .AsNoTracking()
                    .ToListAsync(cancellationToken)
                )
            );
    }

    public async Task<Result<MuscleReadDto>> GetByNameAsync(string muscleName, CancellationToken cancellationToken)
    {
        return Result<MuscleReadDto>.Success(
        _mapper.Map<MuscleReadDto>(
            await _context.Muscles
                .AsNoTracking()
                .SingleOrDefaultAsync(muscle => muscle.Name == muscleName, cancellationToken)
            )
        );
    }

    public async Task<Result<List<MuscleReadDto>>> GetAllByGroupAsync(string muscleGroupName, CancellationToken cancellationToken)
    {
        return Result<List<MuscleReadDto>>.Success(
                _mapper.Map<List<MuscleReadDto>>(
                    await _context.Muscles
                        .AsNoTracking()
                        .Where(muscle => muscle.MuscleGroup == muscleGroupName)
                        .ToListAsync(cancellationToken)
        )
    );
    }

    public async Task<Result<bool>> CreateMuscleAsync(MuscleWriteDto newMuscle, CancellationToken cancellationToken)
    {
        // normalize the name, group name
        Muscle newMusce = _mapper.Map<Muscle>(newMuscle);
        newMusce.Name = Utils.NormalizeString(newMusce.Name!);
        newMusce.MuscleGroup = Utils.NormalizeString(newMusce.MuscleGroup!);
        newMusce.Function = Utils.NormalizeString(newMusce.Function!);

        try
        {
            await _context.Muscles.AddAsync(newMusce, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            // log meeeee!
            return Result<bool>.Failure(ex.Message, ex);
        }

        return Result<bool>.Success(true);

    }

    public async Task<Result<bool>> CreateBulkAsync(HashSet<MuscleWriteDto> newMuscles, CancellationToken cancellationToken)
    {

        using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            List<Muscle> normalizedMuscles = newMuscles.Select(
                newMuscleDto =>
                {
                    Muscle newMuscle = _mapper.Map<Muscle>(newMuscleDto);
                    newMuscle.Name = Utils.NormalizeString(newMuscle.Name!);
                    newMuscle.MuscleGroup = Utils.NormalizeString(newMuscle.MuscleGroup!);
                    newMuscle.Function = Utils.NormalizeString(newMuscle.Function!);
                    return newMuscle;
                }
                ).ToList();


            await _context.Muscles.AddRangeAsync(normalizedMuscles, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);

            // Log MEEEEEEEEEEE!
            return Result<bool>.Failure(ex.Message, ex);
        }

        return Result<bool>.Success(true);
    }


    public async Task<Result<bool>> UpdateAsync(int muscleId, MuscleWriteDto updatedMuscle, CancellationToken cancellationToken)
    {
        try
        {
            Muscle? oldMuscle = await _context.Muscles.SingleOrDefaultAsync(x => x.Id == muscleId, cancellationToken);
            if (oldMuscle is null)
                throw new Exception("Muscle not Found, double check the name!");

            _mapper.Map<Muscle>(updatedMuscle);
            return Result<bool>.Success(true);

        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(ex.Message, ex);
        }
    }

    public async Task<Result<bool>> DeleteAsync(int muscleId, CancellationToken cancellationToken)
    {
        try
        {
            Muscle? muscleToTear = await _context.Muscles.SingleOrDefaultAsync(m => m.Id == muscleId, cancellationToken);
            if (muscleToTear is null)
                throw new Exception("Muscle was resilient to tearing . . . CASUE IT'S NOT THERE!!");

            _context.Muscles.Remove(muscleToTear);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {

            return Result<bool>.Failure(ex.Message, ex);
        }
        return Result<bool>.Success(true);
    }



}