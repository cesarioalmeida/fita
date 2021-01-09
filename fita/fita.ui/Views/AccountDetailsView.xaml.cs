using fita.ui.Models;
using System;
using System.ComponentModel.Composition;
using twentySix.Framework.Core.UI.Interfaces;

namespace fita.ui.Views
{
    [Export(typeof(IIsModelView))]
    public partial class AccountDetailsView : IIsModelView
    {
        public AccountDetailsView()
        {
            InitializeComponent();
        }

        public string View => nameof(AccountDetailsView);

        public Type ModelType => typeof(AccountDetails);
    }
}
