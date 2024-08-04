using AutoMapper;
using DataLibrary.Dtos;
using DataLibrary.Helpers;
using DataLibrary.Models;

namespace DataLibrary.Core;

public class Profiles : Profile
{
    public Profiles()
    {
        // TODO: move what's related into it's own file for clarity.

        CreateMap<Muscle, MuscleReadDto>()
            .ForMember(x => x.MuscleName, src =>
                src.MapFrom(w => w.Name))
            .ReverseMap();


        CreateMap<Muscle, MuscleWriteDto>().ReverseMap();
        CreateMap<MuscleUpdateDto, Muscle>()
            .ForAllMembers(opts =>
                opts.Condition((src, dest, srcMember) => srcMember != null));


        CreateMap<ExerciseMuscle, MuscleExerciseReadDto>()
            .ForMember(dt => dt.Id, opt =>
                opt.MapFrom(src => src.Muscle.Id))
            .ForMember(dt => dt.Name, opt =>
                opt.MapFrom(src => src.Muscle.Name))
            .ForMember(dt => dt.Function, opt =>
                opt.MapFrom(src => src.Muscle.Function))
            .ForMember(dt => dt.MuscleGroup, opt =>
                opt.MapFrom(src => src.Muscle.MuscleGroup))
            .ForMember(dt => dt.WikiPageUrl, opt =>
                opt.MapFrom(src => src.Muscle.WikiPageUrl));


        CreateMap<TrainingType, TrainingTypeReadDto>().ReverseMap();
        CreateMap<TrainingType, TrainingTypeWriteDto>().ReverseMap();


        CreateMap<Exercise, ExerciseReadDto>()
            .ForMember(s => s.HowTos,
                opt => opt
                    .MapFrom(src => src.ExerciseHowTos)
            );
        CreateMap<Exercise, ExerciseWriteDto>().ReverseMap();
        CreateMap<Exercise, ExerciseSearchResultDto>();

        CreateMap<ExerciseHowTo, ExerciseHowToReadDto>();
        CreateMap<ExerciseHowTo, ExerciseHowToWriteDto>().ReverseMap();

        CreateMap<ExerciseRecordWriteDto, Exercise>()
            .ForMember(dt => dt.Name, src =>
                src.MapFrom(x => x.ExerciseName));


        CreateMap<Measurement, MeasurementsReadDto>();
        CreateMap<MeasurementsWriteDto, Measurement>();


        CreateMap<Equipment, EquipmentReadDto>();
        CreateMap<EquipmentWriteDto, Equipment>();


        CreateMap< BlockExercise, BlockExerciseWriteDto>()
            .ForMember(x => x.ExerciseName,
                dst =>
                    dst.MapFrom(yt => yt.Exercise.Name));
        CreateMap<BlockExerciseReadDto, BlockExercise>().ReverseMap();

        CreateMap<Block, BlockReadDto>();
        CreateMap<BlockWriteDto, Block>();

        CreateMap<TrainingDaysWriteDto, TrainingDay>();
        CreateMap<TrainingDaysReadDto, TrainingDay>().ReverseMap();

        CreateMap<TrainingWeekWriteDto, TrainingWeek>();
        CreateMap<TrainingWeekReadDto, TrainingWeek>().ReverseMap();

        CreateMap<TrainingPlanWriteDto, TrainingPlan>();
        CreateMap<TrainingPlan, TrainingPlanReadDto>()
            ;
        
    }


}