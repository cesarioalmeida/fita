// using DevExpress.Mvvm.DataAnnotations;
// using fita.core.Models;
// using fita.ui.Services;
// using System.Collections.ObjectModel;
// using System.Linq;
// using System.Windows;
// using DevExpress.Mvvm.POCO;
// using DevExpress.Xpf.WindowsUI;
// using twentySix.Framework.Core.Extensions;
// using twentySix.Framework.Core.UI.ViewModels;
//
// namespace fita.ui.ViewModels
// {
//     [POCOViewModel]
//     public class CategoriesViewModel : ComposedDocumentViewModelBase
//     {
//         public override object Title { get; set; } = "Categories";
//         
//         public virtual ObservableCollection<Category> Categories { get; set; }
//
//         public virtual Category SelectedCategory { get; set; }
//         
//         public virtual HistoricalData.HistoricalDataPoint SelectedRow { get; set; }
//
//         [ServiceProperty(Key = "NameFocusService")]
//         public virtual IFocusService NameFocusService => null;
//
//         public PersistenceService PersistenceService { get; set; }
//
//         public void Cancel()
//         {
//             Categories.Clear();
//             DocumentOwner?.Close(this);
//         }
//
//         public async void Ok()
//         {
//             IsBusy = true;
//
//             try
//             {
//                 // foreach (var category in Categories)
//                 // {
//                 //     // ignore currencies already defined
//                 //     if (currency.SavedState == null && Currencies.Except(new[] {currency}).Select(x => x.Name.ToUpper())
//                 //         .Contains(currency.Name.ToUpper()))
//                 //     {
//                 //         continue;
//                 //     }
//                 //
//                 //     await PersistenceService.SaveCurrency(currency);
//                 // }
//             }
//             finally
//             {
//                 IsBusy = false;
//             }
//
//             DocumentOwner?.Close(this);
//         }
//
//         public bool CanOk()
//         {
//             // return Categories.All(x => !x.HasError(_ => _.Name) && !x.HasError(_ => _.Symbol));
//             return true;
//         }
//
//         public void DeleteRow()
//         {
//             // if (WinUIMessageBox.Show(
//             //     $"Are you sure you want to delete the date\n{SelectedRow.Date.ToShortDateString()}?",
//             //     "delete data",
//             //     MessageBoxButton.OKCancel,
//             //     MessageBoxImage.Question) != MessageBoxResult.OK)
//             // {
//             //     return;
//             // }
//             //
//             // SelectedCurrency.ExchangeData.Delete(SelectedRow);
//         }
//         
//         public bool CanDeleteRow()
//         {
//             return SelectedRow != null;
//         }
//
//         public async void RefreshData()
//         {
//             IsBusy = true;
//
//             Categories = new ObservableCollection<Category>();
//
//             try
//             {
//                 // var currencies = await PersistenceService.GetCurrencies();
//                 //
//                 // if (currencies != null)
//                 // {
//                 //     currencies.ForEach(x => x.SaveState());
//                 //     Currencies.AddRange(currencies);
//                 // }
//                 //
//                 // SelectedCurrency = currencies?.FirstOrDefault();
//             }
//             finally
//             {
//                 IsBusy = false;
//             }
//         }
//
//         public void Add()
//         {
//             // var currency = new Currency();
//             // Currencies.Add(currency);
//             // SelectedCurrency = currency;
//             //
//             // NameFocusService?.Focus();
//         }
//     }
// }