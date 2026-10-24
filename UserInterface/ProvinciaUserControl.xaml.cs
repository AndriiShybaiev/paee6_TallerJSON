using System;
using System.Windows;
using System.Windows.Controls;
using DataAccess;

namespace UserInterface
{
    public partial class ProvinciaUserControl : UserControl
    {
        private Provincia? provincia;

        public Provincia? Provincia
        {
            set
            {
                this.provincia = value;
                if (value != null)
                {
                    this.idTextBox.Text = value.ID.ToString();
                    this.nombreTextBox.Text = value.Nombre;
                }
                else
                {
                    this.idTextBox.Text = string.Empty;
                    this.nombreTextBox.Text = string.Empty;
                }
            }
            get
            {
                return this.provincia;
            }
        }

        public ProvinciaUserControl()
        {
            InitializeComponent();
        }

        private void idTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (provincia == null) return;
            try
            {
                provincia.ID = Convert.ToInt32(idTextBox.Text);
            }
            catch (FormatException)
            {
                provincia.ID = 0;
                idTextBox.Text = "0";
            }
        }

        private void nombreTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (provincia != null)
            {
                provincia.Nombre = nombreTextBox.Text;
            }
        }
    }
}
