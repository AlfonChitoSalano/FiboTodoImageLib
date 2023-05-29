using PocCommon.Models;
using WebImageLibPoc.Pages;

namespace WebImageLibPoc.Infra
{
    public class ToDoService : IToDoService
    {
        private static readonly Dictionary<int, TaskModel> SourceFromRemoteDb = new()
        {
            {
                1, new TaskModel
                {
                    Id = 1,
                    Name = "Can this be a feature or bug?",
                    Status = TaskModelStatus.New.ToString()
                }
            },
            {
                2, new TaskModel
                {
                    Id = 2,
                    Name = "This is a what task?",
                    Status = TaskModelStatus.Active.ToString()
                }
            },
            {
                3, new TaskModel
                {
                    Id = 3,
                    Name = "No this is not complete. A bug found.",
                    Status = TaskModelStatus.Complete.ToString()
                }
            },
            {
                4, new TaskModel
                {
                    Id = 4,
                    Name = "The front end got issue",
                    Status = TaskModelStatus.Active.ToString()
                }
            }
        };

        public Task<IEnumerable<TaskModel>> GetTaskModelsAsync()
        {
            return Task.FromResult<IEnumerable<TaskModel>>(SourceFromRemoteDb.Values.ToList());
        }

        public Task UpdateTaskModelsAsync(TaskModel updatedModel)
        {
            SourceFromRemoteDb[updatedModel.Id] = updatedModel;
            return Task.CompletedTask;
        }

        public Task DeleteTaskModelsAsync(TaskModel updatedModel)
        {
            SourceFromRemoteDb.Remove(updatedModel.Id);
            return Task.CompletedTask;
        }

        public Task CreateTaskModelsAsync(TaskModel newModel)
        {
            var lastId = SourceFromRemoteDb.Keys.Max();
            newModel.Id = lastId + 1;
            SourceFromRemoteDb.Add(newModel.Id, newModel);
            return Task.CompletedTask;
        }
    }
}