using DevExpress.Blazor;
using Microsoft.AspNetCore.Components;
using PocCommon.Models;
using System.Collections.ObjectModel;
using WebImageLibPoc.Infra;

namespace WebImageLibPoc.Pages
{
    public partial class ToDo
    {
        [Inject] private IToDoService? ToDoService { get; set; }
        private TaskModelStatus _selectedValue;
        private List<TaskModel>? _toDoList;
        private ObservableCollection<TaskModel>? _toDoCollection;
        
        private static GridEditMode CurrentEditMode => GridEditMode.EditForm;

        private IGrid? _grid;

        protected override async Task OnInitializedAsync()
            => await ResetAsync();

        private void Filter()
        {
            if (_toDoList == null)
            {
                return;
            }

            _toDoCollection =
                new ObservableCollection<TaskModel>(_toDoList.Where(x => x.Status == _selectedValue.ToString()));
        }

        private async Task ResetAsync()
        {
            if (ToDoService == null)
            {
                return;
            }

            var result = await ToDoService.GetTaskModelsAsync();
            _toDoList = result.ToList();
            _toDoCollection = new ObservableCollection<TaskModel>(_toDoList);
        }

        private static void GridEdit(GridCustomizeEditModelEventArgs e)
        {
            //do nothing
        }

        private async Task GridEditingAsync(GridEditModelSavingEventArgs e)
        {
            if (ToDoService == null)
            {
                return;
            }

            var editableTask = (TaskModel)e.EditModel;

            if (e.IsNew)
            {
                await ToDoService.CreateTaskModelsAsync(editableTask);
            }
            else
            {
                await ToDoService.UpdateTaskModelsAsync(editableTask);
            }

            await ResetAsync();
        }

        private async Task GridDeletingAsync(GridDataItemDeletingEventArgs e)
        {
            if (ToDoService == null)
            {
                return;
            }

            var deleteTask = (TaskModel)e.DataItem;
            await ToDoService.DeleteTaskModelsAsync(deleteTask);
            await ResetAsync();
        }
    }

    public enum TaskModelStatus
    {
        New,
        Active,
        Complete
    }
}