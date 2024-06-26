using DataLibrary.Dtos;

namespace DataLibrary.Interfaces;
public interface ITrainingTypesService
{
    List<TypeReadDto> Get();
    Task InsertBulkAsync(List<TypeWriteDto> types, CancellationToken cancellationToken);
}
