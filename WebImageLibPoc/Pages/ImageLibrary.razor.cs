using DevExpress.Blazor;
using WebImageLibPoc.Infra;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using PocCommon.Models;
using System.Collections.ObjectModel;

namespace WebImageLibPoc.Pages
{
    public partial class ImageLibrary
    {
        [Inject] private IImageService? ImageService { get; set; }
        private IGrid? _myGrid;
        private ObservableCollection<ImageModel>? _imageModels = new();
        private static GridEditMode CurrentEditMode => GridEditMode.EditForm;

        protected override async Task OnInitializedAsync()
        {
            if (ImageService == null)
            {
                return;
            }

            _imageModels = new ObservableCollection<ImageModel>(await ImageService.GetAllImageModelsAsync());
        }

        private async void UploadImage(InputFileChangeEventArgs e)
        {
            var fileToUpload = e.File;

            if (ImageService == null || fileToUpload == null)
            {
                return;
            }

            var imageModel = await ImageService.UploadToBlobAndSaveImageModelAsync(fileToUpload, fileToUpload.Name);
            _imageModels?.Add(imageModel);
        }

        private async Task RemoveImage(GridDataItemDeletingEventArgs el)
        {
            var imageModel = el.DataItem as ImageModel;

            if (ImageService == null || string.IsNullOrEmpty(imageModel?.Id))
            {
                return;
            }

            await ImageService.DeleteImageModelAsync(imageModel.Id);
            _imageModels?.Remove(imageModel);
        }

        private async Task UpdateImageModel(GridEditModelSavingEventArgs e)
        {
            var editModel = (ImageModel)e.EditModel;

            if (ImageService == null)
            {
                return;
            }

            _ = await ImageService.SaveImageModelAsync(editModel);
            _imageModels = new ObservableCollection<ImageModel>(await ImageService.GetAllImageModelsAsync());
        }
    }

    public enum ImageCategory
    {
        JPG,
        PNG
    }
}