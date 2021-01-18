using fita.ui.Models;
using System;
using twentySix.Framework.Core.UI.Interfaces;

namespace fita.ui.Views
{
    public partial class CategoriesView : IIsModelView
    {
        public CategoriesView()
        {
            InitializeComponent();
        }

        public string View => nameof(CategoriesView);

        public Type ModelType => typeof(Categories);
    }
}
