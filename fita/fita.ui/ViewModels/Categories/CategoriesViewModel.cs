﻿using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.WindowsUI;
using fita.data.Models;
using fita.services.Repositories;
using fita.ui.Common;
using fita.ui.Messages;
using fita.ui.Views.Categories;
using JetBrains.Annotations;
using twentySix.Framework.Core.Common;
using twentySix.Framework.Core.Extensions;
using twentySix.Framework.Core.Messages;
using twentySix.Framework.Core.UI.Enums;
using twentySix.Framework.Core.UI.ViewModels;

namespace fita.ui.ViewModels.Categories;

[POCOViewModel]
public class CategoriesViewModel : ComposedDocumentViewModelBase, IDesiredSize
{
    private bool _fireChangeNotification;

    public int Width => 500;

    public int Height => 600;

    [Import]
    public CategoryRepoService CategoryRepoService { get; set; }

    protected IDocumentManagerService DocumentManagerService =>
        this.GetRequiredService<IDocumentManagerService>("ModalWindowDocumentService");

    public virtual LockableCollection<Category> Data { get; set; } = new();

    [UsedImplicitly]
    public void Close()
    {
        if (_fireChangeNotification)
        {
            Messenger.Default.Send(new CategoriesChanged());
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
            var categories = await CategoryRepoService.GetAll();

            Data.AddRange(categories.OrderBy(c => c.Group).ThenBy(x => x.Name));
        }
        finally
        {
            Data.EndUpdate();
            IsBusy = false;
        }
    }

    [UsedImplicitly]
    public async Task Edit(Category category)
    {
        var viewModel = ViewModelSource.Create<CategoryDetailsViewModel>();
        viewModel.Entity = category ?? new Category();

        var document = DocumentManagerService.CreateDocument(nameof(CategoryDetailsView), viewModel, null, this);
        document.DestroyOnClose = true;
        document.Show();

        if (viewModel.Saved)
        {
            _fireChangeNotification = true;

            await RefreshData();
        }
    }

    [UsedImplicitly]
    public async Task Delete(Category category)
    {
        if (category is null)
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
            Messenger.Default.Send(await CategoryRepoService.Delete(category.CategoryId) == Result.Fail
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