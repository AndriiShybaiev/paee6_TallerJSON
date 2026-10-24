using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DataAccess;
using ExportPDF;
using Microsoft.Win32;

namespace UserInterface
{
    public partial class MainWindow : Window
    {
        public enum SortCriteria
        {
            None,
            Id,
            Nombre
        }

        private const int PageSize = 5;

        private ProvinciasJSON? provinciasJSON;
        private List<Provincia>? allProvincias;
        private List<Provincia> filteredSortedProvincias = new();
        private SortCriteria currentSort = SortCriteria.None;
        private string appliedSearchQuery = string.Empty;
        private int currentPage = 0;
        private int totalPages = 0;

        public MainWindow()
        {
            InitializeComponent();
            Closing += Window_Closing;
            UpdatePaginationUI();
        }

        public bool TryCommitVisibleEdits()
        {
            foreach (var control in provinciasStackPanel.Children.OfType<ProvinciaUserControl>())
            {
                if (!control.TryCommit(out string? error))
                {
                    if (!string.IsNullOrEmpty(error))
                    {
                        MessageBox.Show(error, "Error de validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    return false;
                }
            }
            return true;
        }

        private void cargarButton_Click(object sender, RoutedEventArgs e)
        {
            if (allProvincias != null && !TryCommitVisibleEdits())
            {
                return;
            }

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Archivo JSON|*.json";
            if (openFileDialog.ShowDialog() == true)
            {
                provinciasJSON = new ProvinciasJSON(openFileDialog.FileName);
                allProvincias = provinciasJSON.getProvincias();
                if (allProvincias != null)
                {
                    currentSort = SortCriteria.None;
                    appliedSearchQuery = string.Empty;
                    searchTextBox.Text = string.Empty;
                    RecomputePipeline(resetToFirstPage: true);
                    activarBotones(true);
                    statusText.Text = openFileDialog.FileName;
                }
                else
                {
                    allProvincias = null;
                    filteredSortedProvincias.Clear();
                    provinciasStackPanel.Children.Clear();
                    UpdatePaginationUI();
                    activarBotones(false);
                    MessageBox.Show("Error cargando datos", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    statusText.Text = "Error cargando datos";
                }
            }
        }

        private void RecomputePipeline(bool resetToFirstPage = false)
        {
            if (allProvincias == null)
            {
                filteredSortedProvincias.Clear();
                provinciasStackPanel.Children.Clear();
                searchResultsText.Text = string.Empty;
                UpdatePaginationUI();
                return;
            }

            // 1. Filter
            IEnumerable<Provincia> query = allProvincias;
            if (!string.IsNullOrWhiteSpace(appliedSearchQuery))
            {
                string term = appliedSearchQuery.Trim();
                bool isInt = int.TryParse(term, out int searchId);

                var compareInfo = CultureInfo.GetCultureInfo("es-ES").CompareInfo;
                query = query.Where(p =>
                    (p.Nombre != null && compareInfo.IndexOf(p.Nombre, term, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0)
                    || (isInt && p.ID == searchId)
                );
            }

            // 2. Sort
            query = currentSort switch
            {
                SortCriteria.Id => query.OrderBy(p => p.ID),
                SortCriteria.Nombre => query.OrderBy(p => p.Nombre, StringComparer.CurrentCultureIgnoreCase),
                _ => query
            };

            filteredSortedProvincias = query.ToList();
            searchResultsText.Text = $"Encontradas: {filteredSortedProvincias.Count} de {allProvincias.Count}";

            if (resetToFirstPage)
            {
                currentPage = filteredSortedProvincias.Count > 0 ? 1 : 0;
            }

            UpdatePaginationUI();
            RenderCurrentPage();
        }

        private void UpdatePaginationUI()
        {
            int n = filteredSortedProvincias.Count;
            if (allProvincias == null || n == 0)
            {
                currentPage = 0;
                totalPages = 0;
                rangeIndicatorText.Text = "Mostrando 0–0 de 0";
                pageIndicatorText.Text = "Página 0 de 0";
                primeraButton.IsEnabled = false;
                anteriorButton.IsEnabled = false;
                siguienteButton.IsEnabled = false;
                ultimaButton.IsEnabled = false;

                emptyStateText.Visibility = Visibility.Visible;
                emptyStateText.Text = allProvincias == null
                    ? "Fichero no cargado. Pulse 'Cargar' para abrir un archivo de provincias."
                    : "No se encontraron provincias coincidentes.";
            }
            else
            {
                totalPages = (int)Math.Ceiling(n / (double)PageSize);
                if (currentPage < 1) currentPage = 1;
                if (currentPage > totalPages) currentPage = totalPages;

                int start = (currentPage - 1) * PageSize + 1;
                int end = Math.Min(currentPage * PageSize, n);

                rangeIndicatorText.Text = $"Mostrando {start}–{end} de {n}";
                pageIndicatorText.Text = $"Página {currentPage} de {totalPages}";

                primeraButton.IsEnabled = currentPage > 1;
                anteriorButton.IsEnabled = currentPage > 1;
                siguienteButton.IsEnabled = currentPage < totalPages;
                ultimaButton.IsEnabled = currentPage < totalPages;

                emptyStateText.Visibility = Visibility.Collapsed;
            }
        }

        private void RenderCurrentPage()
        {
            provinciasStackPanel.Children.Clear();
            if (filteredSortedProvincias.Count == 0 || currentPage == 0)
            {
                return;
            }

            var pageItems = filteredSortedProvincias
                .Skip((currentPage - 1) * PageSize)
                .Take(PageSize);

            foreach (var provincia in pageItems)
            {
                var uc = new ProvinciaUserControl
                {
                    Provincia = provincia
                };
                provinciasStackPanel.Children.Add(uc);
            }
        }

        private void activarBotones(bool activar)
        {
            idButton.IsEnabled = activar;
            nombreButton.IsEnabled = activar;
            guardarButton.IsEnabled = activar;
            guardarComoButton.IsEnabled = activar;
            buscarButton.IsEnabled = activar;
            limpiarButton.IsEnabled = activar;
            exportarButton.IsEnabled = activar;
            exportarCsvButton.IsEnabled = activar;
            exportarXmlButton.IsEnabled = activar;
        }

        private void idButton_Click(object sender, RoutedEventArgs e)
        {
            if (!TryCommitVisibleEdits()) return;
            currentSort = SortCriteria.Id;
            RecomputePipeline(resetToFirstPage: false);
        }

        private void nombreButton_Click(object sender, RoutedEventArgs e)
        {
            if (!TryCommitVisibleEdits()) return;
            currentSort = SortCriteria.Nombre;
            RecomputePipeline(resetToFirstPage: false);
        }

        private void buscarButton_Click(object sender, RoutedEventArgs e)
        {
            if (!TryCommitVisibleEdits()) return;
            appliedSearchQuery = searchTextBox.Text;
            RecomputePipeline(resetToFirstPage: true);
        }

        private void limpiarButton_Click(object sender, RoutedEventArgs e)
        {
            if (!TryCommitVisibleEdits()) return;
            searchTextBox.Text = string.Empty;
            appliedSearchQuery = string.Empty;
            RecomputePipeline(resetToFirstPage: true);
        }

        private void searchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                buscarButton_Click(sender, e);
            }
        }

        private void primeraButton_Click(object sender, RoutedEventArgs e)
        {
            if (!TryCommitVisibleEdits()) return;
            currentPage = 1;
            UpdatePaginationUI();
            RenderCurrentPage();
        }

        private void anteriorButton_Click(object sender, RoutedEventArgs e)
        {
            if (!TryCommitVisibleEdits()) return;
            if (currentPage > 1)
            {
                currentPage--;
                UpdatePaginationUI();
                RenderCurrentPage();
            }
        }

        private void siguienteButton_Click(object sender, RoutedEventArgs e)
        {
            if (!TryCommitVisibleEdits()) return;
            if (currentPage < totalPages)
            {
                currentPage++;
                UpdatePaginationUI();
                RenderCurrentPage();
            }
        }

        private void ultimaButton_Click(object sender, RoutedEventArgs e)
        {
            if (!TryCommitVisibleEdits()) return;
            currentPage = totalPages;
            UpdatePaginationUI();
            RenderCurrentPage();
        }

        private bool guardar()
        {
            if (provinciasJSON == null || allProvincias == null)
            {
                MessageBox.Show("Error guardando datos", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!TryCommitVisibleEdits())
            {
                return false;
            }

            bool res = provinciasJSON.saveProvincias(allProvincias);
            if (!res)
            {
                MessageBox.Show("Error guardando datos", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            else
            {
                RenderCurrentPage();
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
            if (allProvincias == null) return;
            if (!TryCommitVisibleEdits()) return;

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Archivo JSON|*.json";
            if (saveFileDialog.ShowDialog() == true)
            {
                if (provinciasJSON == null)
                {
                    provinciasJSON = new ProvinciasJSON(saveFileDialog.FileName);
                }

                bool res = provinciasJSON.saveProvincias(allProvincias, saveFileDialog.FileName);
                if (res)
                {
                    statusText.Text = saveFileDialog.FileName;
                    RenderCurrentPage();
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
            if (allProvincias != null)
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

        public List<Provincia> GetExportDataset()
        {
            if (allProvincias == null) return new List<Provincia>();

            // exportScopeComboBox: 0 = "Todas", 1 = "Resultados"
            if (exportScopeComboBox.SelectedIndex == 1)
            {
                return filteredSortedProvincias.ToList();
            }

            IEnumerable<Provincia> query = allProvincias;
            query = currentSort switch
            {
                SortCriteria.Id => query.OrderBy(p => p.ID),
                SortCriteria.Nombre => query.OrderBy(p => p.Nombre, StringComparer.CurrentCultureIgnoreCase),
                _ => query
            };

            return query.ToList();
        }

        private void exportarButton_Click(object sender, RoutedEventArgs e)
        {
            if (allProvincias == null) return;
            if (!TryCommitVisibleEdits()) return;

            var exportList = GetExportDataset();
            if (exportList.Count == 0)
            {
                MessageBox.Show("No hay datos para exportar", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Archivo PDF|*.pdf";
            if (saveFileDialog.ShowDialog() == true)
            {
                ProvinciasPDF provinciasPDF = new ProvinciasPDF();
                string scopeDescription = exportScopeComboBox.SelectedIndex == 1 && !string.IsNullOrWhiteSpace(appliedSearchQuery)
                    ? $"Resultados de búsqueda: \"{appliedSearchQuery}\""
                    : "Todas las provincias";

                if (provinciasPDF.SavePDF(exportList, saveFileDialog.FileName, scopeDescription))
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
                else
                {
                    MessageBox.Show("Error exportando datos a PDF", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void exportarCsvButton_Click(object sender, RoutedEventArgs e)
        {
            if (allProvincias == null) return;
            if (!TryCommitVisibleEdits()) return;

            var exportList = GetExportDataset();
            if (exportList.Count == 0)
            {
                MessageBox.Show("No hay datos para exportar", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Archivo CSV|*.csv";
            if (saveFileDialog.ShowDialog() == true)
            {
                if (ProvinciasExport.SaveCSV(exportList, saveFileDialog.FileName))
                {
                    MessageBox.Show("Datos exportados correctamente a CSV en " + saveFileDialog.FileName,
                        "Exportación correcta", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Error exportando datos a CSV", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void exportarXmlButton_Click(object sender, RoutedEventArgs e)
        {
            if (allProvincias == null) return;
            if (!TryCommitVisibleEdits()) return;

            var exportList = GetExportDataset();
            if (exportList.Count == 0)
            {
                MessageBox.Show("No hay datos para exportar", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Archivo XML|*.xml";
            if (saveFileDialog.ShowDialog() == true)
            {
                if (ProvinciasExport.SaveXML(exportList, saveFileDialog.FileName))
                {
                    MessageBox.Show("Datos exportados correctamente a XML en " + saveFileDialog.FileName,
                        "Exportación correcta", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Error exportando datos a XML", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
