using System.Threading.Tasks;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using fita.data.Models;
using fita.services;
using fita.services.Repositories;
using fita.ui.Common;
using JetBrains.Annotations;
using twentySix.Framework.Core.Messages;
using twentySix.Framework.Core.UI.Enums;
using twentySix.Framework.Core.UI.ViewModels;

namespace fita.ui.ViewModels.Categories
{
    [POCOViewModel]
    public class CategoryDetailsViewModel : ComposedDocumentViewModelBase, IDesiredSize, IHasSaved
    {
        public int Width => 300;

        public int Height => 300;

        public CategoryRepoService RepoService { get; set; }

        public Category Entity { get; set; }

        public bool Saved { get; private set; }

        [UsedImplicitly]
        public void Cancel() => DocumentOwner?.Close(this);

        [UsedImplicitly]
        public async Task Save()
        {
            IsBusy = true;

            try
            {
                Messenger.Default.Send(await RepoService.SaveAsync(Entity) == Result.Fail
                    ? new NotificationMessage("Failed to save category.", NotificationStatusEnum.Error)
                    : new NotificationMessage($"Category {Entity.Name} saved.", NotificationStatusEnum.Success));

                Saved = true;
                DocumentOwner?.Close(this);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}