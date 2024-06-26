using DataLibrary.Dtos;

namespace DataLibrary.Interfaces;

public interface IMuscleService
{
    List<MuscleReadDto> Get();
}