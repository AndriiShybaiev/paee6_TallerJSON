using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DataAccess;
using ExportPDF;
using Microsoft.Win32;

namespace UserInterface
{
    public partial class MainWindow : Window
    {
        private ProvinciasJSON? provinciasJSON;
        private List<Provincia>? provincias;

        public MainWindow()
        {
            InitializeComponent();
            Closing += Window_Closing;
        }

        private void cargarButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Archivo JSON|*.json";
            if (openFileDialog.ShowDialog() == true)
            {
                provinciasJSON = new ProvinciasJSON(openFileDialog.FileName);
                provincias = provinciasJSON.getProvincias();
                if (provincias != null)
                {
                    showProvincias();
                    activarBotones(true);
                    statusText.Text = openFileDialog.FileName;
                }
                else
                {
                    provinciasStackPanel.Children.Clear();
                    activarBotones(false);
                    MessageBox.Show("Error cargando datos", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    statusText.Text = "Error cargando datos";
                }
            }
        }

        private void showProvincias()
        {
            provinciasStackPanel.Children.Clear();
            if (provincias == null) return;
            foreach (Provincia provincia in provincias)
            {
                ProvinciaUserControl provinciaUserControl = new ProvinciaUserControl
                {
                    Provincia = provincia,
                };
                provinciasStackPanel.Children.Add(provinciaUserControl);
            }
        }

        private void activarBotones(bool activar)
        {
            foreach (Button b in botonesStackPanel.Children.OfType<Button>())
            {
                if (b.Name != "cargarButton")
                    b.IsEnabled = activar;
            }
        }

        private void orderProvincias(bool nombre)
        {
            if (provincias == null) return;
            if (nombre)
                provincias = provincias.OrderBy(o => o.Nombre).ToList();
            else
                provincias = provincias.OrderBy(o => o.ID).ToList();
        }

        private void idButton_Click(object sender, RoutedEventArgs e)
        {
            orderProvincias(false);
            showProvincias();
        }

        private void nombreButton_Click(object sender, RoutedEventArgs e)
        {
            orderProvincias(true);
            showProvincias();
        }

        private bool guardar()
        {
            if (provinciasJSON == null || provincias == null)
            {
                MessageBox.Show("Error guardando datos", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            bool res = provinciasJSON.saveProvincias(provincias);
            if (!res)
            {
                MessageBox.Show("Error guardando datos", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            else
            {
                MessageBox.Show("Datos guardados correctamente en " + provinciasJSON.Path,
                    "Datos guardados", MessageBoxButton.OK, MessageBoxImage.Information);
                return true;
            }
        }

        private void guardarButton_Click(object sender, RoutedEventArgs e)
        {
            guardar();
        }

        private void guardarComoButton_Click(object sender, RoutedEventArgs e)
        {
            if (provincias == null) return;

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Archivo JSON|*.json";
            if (saveFileDialog.ShowDialog() == true)
            {
                if (provinciasJSON == null)
                {
                    provinciasJSON = new ProvinciasJSON(saveFileDialog.FileName);
                }
                else
                {
                    provinciasJSON.Path = saveFileDialog.FileName;
                }

                bool res = provinciasJSON.saveProvincias(provincias);
                if (res)
                {
                    statusText.Text = saveFileDialog.FileName;
                    MessageBox.Show("Datos guardados correctamente en " + saveFileDialog.FileName,
                        "Datos guardados", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Error guardando datos en " + saveFileDialog.FileName, "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (provincias != null)
            {
                MessageBoxResult result = MessageBox.Show(
                    "¿Quiere guardar los datos antes de salir?", "Saliendo de la aplicación",
                    MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    if (!guardar())
                        e.Cancel = true;
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }

        private void exportarButton_Click(object sender, RoutedEventArgs e)
        {
            if (provincias == null) return;

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Archivo PDF|*.pdf";
            if (saveFileDialog.ShowDialog() == true)
            {
                ProvinciasPDF provinciasPDF = new ProvinciasPDF();
                if (provinciasPDF.SavePDF(provincias, saveFileDialog.FileName))
                {
                    if (MessageBox.Show("Datos exportados a PDF en " + saveFileDialog.FileName
                        + ".\n\n ¿Desea abrirlo?", "Exportación correcta",
                        MessageBoxButton.YesNo, MessageBoxImage.Information)
                        == MessageBoxResult.Yes)
                    {
                        var p = new Process();
                        p.StartInfo = new ProcessStartInfo(saveFileDialog.FileName)
                        {
                            UseShellExecute = true
                        };
                        p.Start();
                    }
                }
            }
        }
    }
}
