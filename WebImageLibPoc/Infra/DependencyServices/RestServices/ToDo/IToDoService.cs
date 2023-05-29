using PocCommon.Models;

namespace WebImageLibPoc.Infra
{
    public interface IToDoService
    {
        Task<IEnumerable<TaskModel>> GetTaskModelsAsync();
        Task UpdateTaskModelsAsync(TaskModel updatedModel);
        Task DeleteTaskModelsAsync(TaskModel updatedModel);
        Task CreateTaskModelsAsync(TaskModel newModel);
    }
}
