using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DataAccess;
using Microsoft.Win32;

namespace UserInterface
{
    public partial class ProvinciaUserControl : UserControl
    {
        private static readonly CultureInfo SpanishCulture = CultureInfo.GetCultureInfo("es-ES");
        private static readonly Brush ErrorBorderBrush = new SolidColorBrush(Color.FromRgb(220, 38, 38));
        private static readonly Brush DefaultBorderBrush = new SolidColorBrush(Color.FromRgb(209, 213, 219));

        private Provincia? provincia;

        public Provincia? Provincia
        {
            get => provincia;
            set
            {
                provincia = value;
                if (value != null)
                {
                    idTextBox.Text = value.ID.ToString();
                    nombreTextBox.Text = value.Nombre ?? string.Empty;
                    poblacionTextBox.Text = value.Poblacion?.ToString() ?? string.Empty;
                    superficieTextBox.Text = value.SuperficieKm2.HasValue
                        ? value.SuperficieKm2.Value.ToString("0.##", SpanishCulture)
                        : string.Empty;

                    UpdateEscudoImage();
                    UpdateMapaImage();
                }
                else
                {
                    idTextBox.Text = string.Empty;
                    nombreTextBox.Text = string.Empty;
                    poblacionTextBox.Text = string.Empty;
                    superficieTextBox.Text = string.Empty;
                    ClearImage(escudoImage, escudoPlaceholder);
                    ClearImage(mapaImage, mapaPlaceholder);
                }

                ClearError(poblacionTextBox);
                ClearError(superficieTextBox);
            }
        }

        public ProvinciaUserControl()
        {
            InitializeComponent();
        }

        public bool TryCommit(out string? errorMessage)
        {
            errorMessage = null;
            if (provincia == null)
            {
                return true;
            }

            // 1. Commit ID
            try
            {
                provincia.ID = Convert.ToInt32(idTextBox.Text.Trim());
            }
            catch (Exception)
            {
                provincia.ID = 0;
                idTextBox.Text = "0";
            }

            // 2. Commit Nombre
            provincia.Nombre = nombreTextBox.Text ?? string.Empty;

            // 3. Commit Población
            string pobText = poblacionTextBox.Text.Trim();
            if (string.IsNullOrEmpty(pobText))
            {
                provincia.Poblacion = null;
                ClearError(poblacionTextBox);
            }
            else
            {
                if (long.TryParse(pobText, NumberStyles.None, CultureInfo.InvariantCulture, out long p) && p >= 0)
                {
                    provincia.Poblacion = p;
                    ClearError(poblacionTextBox);
                }
                else
                {
                    SetError(poblacionTextBox, "Población debe ser un número entero mayor o igual a 0.");
                    errorMessage = $"La población de '{provincia.Nombre}' debe ser un número entero mayor o igual a 0.";
                    return false;
                }
            }

            // 4. Commit Superficie
            string supText = superficieTextBox.Text.Trim();
            if (string.IsNullOrEmpty(supText))
            {
                provincia.SuperficieKm2 = null;
                ClearError(superficieTextBox);
            }
            else
            {
                if (supText.Contains('.'))
                {
                    SetError(superficieTextBox, "Use coma decimal (ej. 1234,56). Los puntos no están permitidos.");
                    errorMessage = $"La superficie de '{provincia.Nombre}' no debe contener puntos. Use coma como separador decimal (ej. 1234,56).";
                    return false;
                }

                if (decimal.TryParse(supText, NumberStyles.AllowDecimalPoint, SpanishCulture, out decimal s) && s > 0)
                {
                    provincia.SuperficieKm2 = s;
                    ClearError(superficieTextBox);
                }
                else
                {
                    SetError(superficieTextBox, "Superficie debe ser un número positivo en km² (ej. 1234,56).");
                    errorMessage = $"La superficie de '{provincia.Nombre}' debe ser un número positivo mayor que 0.";
                    return false;
                }
            }

            return true;
        }

        private void idTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (provincia == null) return;
            try
            {
                provincia.ID = Convert.ToInt32(idTextBox.Text.Trim());
            }
            catch (Exception)
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

        private void poblacionTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (provincia == null) return;
            string text = poblacionTextBox.Text.Trim();
            if (string.IsNullOrEmpty(text))
            {
                provincia.Poblacion = null;
                ClearError(poblacionTextBox);
            }
            else if (long.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out long p) && p >= 0)
            {
                provincia.Poblacion = p;
                ClearError(poblacionTextBox);
            }
            else
            {
                SetError(poblacionTextBox, "Población debe ser un número entero mayor o igual a 0.");
            }
        }

        private void superficieTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (provincia == null) return;
            string text = superficieTextBox.Text.Trim();
            if (string.IsNullOrEmpty(text))
            {
                provincia.SuperficieKm2 = null;
                ClearError(superficieTextBox);
            }
            else if (!text.Contains('.') && decimal.TryParse(text, NumberStyles.AllowDecimalPoint, SpanishCulture, out decimal s) && s > 0)
            {
                provincia.SuperficieKm2 = s;
                ClearError(superficieTextBox);
            }
            else
            {
                SetError(superficieTextBox, "Superficie debe ser un número decimal mayor a 0 con coma (ej. 1234,56).");
            }
        }

        private void selectEscudoButton_Click(object sender, RoutedEventArgs e)
        {
            if (provincia == null) return;
            string? path = ChooseImageFile("Seleccionar escudo de " + provincia.Nombre);
            if (path != null)
            {
                provincia.EscudoPath = path;
                UpdateEscudoImage();
            }
        }

        private void clearEscudoButton_Click(object sender, RoutedEventArgs e)
        {
            if (provincia == null) return;
            provincia.EscudoPath = null;
            UpdateEscudoImage();
        }

        private void selectMapaButton_Click(object sender, RoutedEventArgs e)
        {
            if (provincia == null) return;
            string? path = ChooseImageFile("Seleccionar mapa de " + provincia.Nombre);
            if (path != null)
            {
                provincia.MapaPath = path;
                UpdateMapaImage();
            }
        }

        private void clearMapaButton_Click(object sender, RoutedEventArgs e)
        {
            if (provincia == null) return;
            provincia.MapaPath = null;
            UpdateMapaImage();
        }

        private string? ChooseImageFile(string title)
        {
            var dlg = new OpenFileDialog
            {
                Title = title,
                Filter = "Archivos de imagen (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|Todos los archivos (*.*)|*.*"
            };

            return dlg.ShowDialog() == true ? dlg.FileName : null;
        }

        private void UpdateEscudoImage()
        {
            SetImage(escudoImage, escudoPlaceholder, provincia?.EscudoPath);
        }

        private void UpdateMapaImage()
        {
            SetImage(mapaImage, mapaPlaceholder, provincia?.MapaPath);
        }

        private void SetImage(Image imgControl, TextBlock placeholder, string? path)
        {
            BitmapImage? bmp = LoadBitmapImage(path);
            if (bmp != null)
            {
                imgControl.Source = bmp;
                placeholder.Visibility = Visibility.Collapsed;
            }
            else
            {
                imgControl.Source = null;
                placeholder.Visibility = Visibility.Visible;
            }
        }

        private void ClearImage(Image imgControl, TextBlock placeholder)
        {
            imgControl.Source = null;
            placeholder.Visibility = Visibility.Visible;
        }

        private static BitmapImage? LoadBitmapImage(string? path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return null;
            }

            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(Path.GetFullPath(path), UriKind.Absolute);
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
            catch
            {
                return null;
            }
        }

        private static void SetError(TextBox textBox, string toolTip)
        {
            textBox.BorderBrush = ErrorBorderBrush;
            textBox.ToolTip = toolTip;
        }

        private static void ClearError(TextBox textBox)
        {
            textBox.BorderBrush = DefaultBorderBrush;
            textBox.ToolTip = null;
        }
    }
}
