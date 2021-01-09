using fita.ui.Models;
using System;
using twentySix.Framework.Core.UI.Interfaces;

namespace fita.ui.Views
{
    public partial class CurrenciesView : IIsModelView
    {
        public CurrenciesView()
        {
            InitializeComponent();
        }

        public string View => nameof(CurrenciesView);

        public Type ModelType => typeof(Currencies);
    }
}
