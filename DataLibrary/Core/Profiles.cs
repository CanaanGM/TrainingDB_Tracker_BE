﻿using AutoMapper;

using DataLibrary.Dtos;
using DataLibrary.Helpers;
using DataLibrary.Models;

namespace DataLibrary.Core;
public class Profiles : Profile
{
    public Profiles()
    {
        CreateMap<Muscle, MuscleReadDto>()
            .ForMember(x => x.MuscleName, src => src.MapFrom(w => w.Name))
            .ReverseMap();


        CreateMap<Muscle, MuscleWriteDto>().ReverseMap();
        CreateMap<MuscleUpdateDto, Muscle>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));


        CreateMap<ExerciseMuscle, MuscleExerciseReadDto>()
            .ForMember(dt => dt.Id, opt => opt.MapFrom(src => src.Muscle.Id))
            .ForMember(dt => dt.Name, opt => opt.MapFrom(src => src.Muscle.Name))
            .ForMember(dt => dt.Function, opt => opt.MapFrom(src => src.Muscle.Function))
            .ForMember(dt => dt.MuscleGroup, opt => opt.MapFrom(src => src.Muscle.MuscleGroup))
            .ForMember(dt => dt.WikiPageUrl, opt => opt.MapFrom(src => src.Muscle.WikiPageUrl));


        CreateMap<TrainingType, TrainingTypeReadDto>().ReverseMap();
        CreateMap<TrainingType, TrainingTypeWriteDto>().ReverseMap();


        CreateMap<Exercise, ExerciseReadDto>()
            .ForMember(s => s.HowTos, opt => opt.MapFrom(src => src.ExerciseHowTos));
        CreateMap<Exercise, ExerciseWriteDto>().ReverseMap();
        CreateMap<Exercise, ExerciseSearchResultDto>();

        CreateMap<ExerciseHowTo, ExerciseHowToReadDto>();
        CreateMap<ExerciseHowTo, ExerciseHowToWriteDto>().ReverseMap();


        CreateMap<TrainingSession, TrainingSessionReadDto>()
            .ForMember(dt => dt.TrainingTypes
            , src => src.MapFrom(o => o.TrainingTypes))
            .ForMember(dt => dt.ExerciseRecords, src => src.MapFrom(r => r.TrainingSessionExerciseRecords.Select(r => r.ExerciseRecord)))
            ;

        CreateMap<TrainingSessionWriteDto, TrainingSession>()
            .ForMember(dt => dt.DurationInSeconds, src => src.MapFrom(r => Utils.DurationSecondsFromMinutes(r.DurationInMinutes)))
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<ExerciseRecord, ExerciseRecordReadDto>()
            .ForMember(dt => dt.ExerciseName
                , src => src
                    .MapFrom(o => o.Exercise!.Name))

            .ForMember(dt => dt.MuscleGroup
                , src => src
                    .MapFrom(o => o.Exercise!.ExerciseMuscles
                        .Select(x => x.Muscle.MuscleGroup).Distinct()
                        )
                    //.Select(x => x.Muscle.Name))
                    );
        CreateMap<ExerciseRecordWriteDto, Exercise>()
            .ForMember(dt => dt.Name, src => src.MapFrom(x => x.ExerciseName));



    }
}
