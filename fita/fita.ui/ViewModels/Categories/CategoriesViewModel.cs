using System.Threading.Tasks;
using System.Windows;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.WindowsUI;
using fita.data.Models;
using fita.services;
using fita.services.Repositories;
using fita.ui.Common;
using fita.ui.Views.Categories;
using twentySix.Framework.Core.Extensions;
using twentySix.Framework.Core.Messages;
using twentySix.Framework.Core.UI.Enums;
using twentySix.Framework.Core.UI.ViewModels;

namespace fita.ui.ViewModels.Categories
{
    [POCOViewModel]
    public class CategoriesViewModel : ComposedDocumentViewModelBase, IDesiredSize
    {
        private bool fireChangeNotification;

        public int Width => 500;

        public int Height => 600;

        public CategoryRepoService CategoryRepoService { get; set; }

        protected IDocumentManagerService DocumentManagerService =>
            this.GetRequiredService<IDocumentManagerService>("ModalWindowDocumentService");

        public virtual LockableCollection<Category> Data { get; set; } = new();

        public void Close()
        {
            if (fireChangeNotification)
            {
            }

            Data.Clear();
            DocumentOwner?.Close(this);
        }

        public async Task RefreshData()
        {
            IsBusy = true;

            Data.BeginUpdate();
            Data.Clear();

            try
            {
                var categories = await CategoryRepoService.AllAsync();

                Data.AddRange(categories);
            }
            finally
            {
                Data.EndUpdate();
                IsBusy = false;
            }
        }

        public async Task Edit(Category category)
        {
            var viewModel = ViewModelSource.Create<CategoryDetailsViewModel>();
            viewModel.Entity = category ?? new Category();

            var document =
                this.DocumentManagerService.CreateDocument(nameof(CategoryDetailsView), viewModel, null, this);
            document.DestroyOnClose = true;
            document.Show();

            if (viewModel.Saved)
            {
                fireChangeNotification = true;

                await RefreshData();
            }
        }

        public async Task Delete(Category category)
        {
            if (category == null)
            {
                return;
            }

            if (WinUIMessageBox.Show(
                $"Are you sure you want to delete the category {category.Name}?",
                "Delete category",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Question) != MessageBoxResult.OK)
            {
                return;
            }

            IsBusy = true;

            try
            {
                Messenger.Default.Send(await CategoryRepoService.DeleteAsync(category.CategoryId) == Result.Fail
                    ? new NotificationMessage("Failed to delete category.", NotificationStatusEnum.Error)
                    : new NotificationMessage($"Category {category.Name} deleted.", NotificationStatusEnum.Success));

                await RefreshData();
            }
            finally
            {
                IsBusy = false;
            }
        }
        
    }
}