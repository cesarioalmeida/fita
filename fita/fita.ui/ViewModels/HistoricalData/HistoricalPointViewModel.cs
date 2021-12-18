using DevExpress.Mvvm.DataAnnotations;
using fita.data.Models;
using fita.ui.Common;
using JetBrains.Annotations;
using twentySix.Framework.Core.UI.ViewModels;

namespace fita.ui.ViewModels.HistoricalData
{
    [POCOViewModel]
    public class HistoricalPointViewModel : ComposedDocumentViewModelBase, IDesiredSize, IHasSaved
    {
        public int Width => 300;

        public int Height => 300;

        public virtual HistoricalDataPoint Point { get; set; }

        public bool Saved { get; private set; }

        [UsedImplicitly]
        public void Cancel() => DocumentOwner?.Close(this);

        [UsedImplicitly]
        public void Save()
        {
            Saved = true;
            DocumentOwner?.Close(this);
        }
    }
}