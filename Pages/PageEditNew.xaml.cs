using Microsoft.Win32;
using Shoes_shop.Database;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Shoes_shop.Pages
{
    public partial class PageEditNew : Page
    {
        private Products _product;
        private string _photoPath;

        public PageEditNew(Products product = null)
        {
            InitializeComponent();
            LoadComboBoxes();

            if (product != null)
            {
                _product = product;
                LoadProductData();
                TbArticle.IsReadOnly = true;
            }
            else
            {
                _product = new Products();
                TbArticle.IsReadOnly = false;
                TbArticle.Focus();
            }
        }

        private void LoadComboBoxes()
        {
            CbName.ItemsSource = ConnectOdb.conObj.ProductNamesDict.ToList();
            CbCategory.ItemsSource = ConnectOdb.conObj.ProductCategories.ToList();
            CbManufacturer.ItemsSource = ConnectOdb.conObj.Manufacturers.ToList();
            CbSupplier.ItemsSource = ConnectOdb.conObj.Suppliers.ToList();
        }

        private void LoadProductData()
        {
            TbArticle.Text = _product.Article;
            CbName.SelectedValue = _product.NameId;
            CbCategory.SelectedValue = _product.CategoryId;
            CbManufacturer.SelectedValue = _product.ManufacturerId;
            CbSupplier.SelectedValue = _product.SupplierId;
            TbPrice.Text = _product.Price.ToString("F2");
            TbUnit.Text = _product.Unit;
            TbQuantity.Text = _product.StockQuantity.ToString();
            TbDiscount.Text = _product.DiscountPercent.ToString();
            TbDescription.Text = _product.Description;
            _photoPath = _product.PhotoPath;

            LoadImage(_photoPath);
        }

        private void LoadImage(string path)
        {
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(path);
                    bitmap.DecodePixelWidth = 300;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    ImgProduct.Source = bitmap;
                }
                catch { }
            }
        }

        private void BtnLoadPhoto_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp",
                Title = "Выберите фото товара"
            };

            if (dialog.ShowDialog() != true) return;

            var imagesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
            Directory.CreateDirectory(imagesDir);

            var fileName = Path.GetFileName(dialog.FileName);
            _photoPath = Path.Combine(imagesDir, fileName);

            if (File.Exists(_photoPath))
            {
                var name = Path.GetFileNameWithoutExtension(fileName);
                var ext = Path.GetExtension(fileName);
                _photoPath = Path.Combine(imagesDir, $"{name}_{DateTime.Now:yyyyMMdd_HHmmss}{ext}");
            }

            File.Copy(dialog.FileName, _photoPath, true);
            LoadImage(_photoPath);
        }

        private bool TryGetDecimal(string text, out decimal result)
        {
            result = 0;
            return decimal.TryParse(text, out result) && result >= 0;
        }

        private bool TryGetInt(string text, out int result, int min = 0, int max = int.MaxValue)
        {
            result = 0;
            return int.TryParse(text, out result) && result >= min && result <= max;
        }

        private int GetId(ComboBox cb) => cb.SelectedValue is int id ? id : 0;

        private bool Validate()
        {
            var article = TbArticle.Text?.Trim();
            if (string.IsNullOrEmpty(article))
                return ShowError("Введите артикул");

            var existing = ConnectOdb.conObj.Products.Find(article);
            if (existing != null && _product.Article != article)
                return ShowError($"Артикул «{article}» уже занят");

            if (GetId(CbName) == 0)
                return ShowError("Выберите наименование");

            if (!TryGetDecimal(TbPrice.Text, out _))
                return ShowError("Некорректная цена");

            if (!TryGetInt(TbQuantity.Text, out _))
                return ShowError("Некорректное количество");

            if (!TryGetInt(TbDiscount.Text, out var disc, 0, 100))
                return ShowError("Скидка должна быть от 0 до 100");

            return true;
        }

        private bool ShowError(string message)
        {
            MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!Validate()) return;

            try
            {
                _product.Article = TbArticle.Text.Trim();
                _product.NameId = GetId(CbName);
                _product.CategoryId = GetId(CbCategory);
                _product.ManufacturerId = GetId(CbManufacturer);
                _product.SupplierId = GetId(CbSupplier);
                _product.Price = decimal.Parse(TbPrice.Text);
                _product.Unit = TbUnit.Text?.Trim() ?? "шт.";
                _product.StockQuantity = int.Parse(TbQuantity.Text);
                _product.DiscountPercent = int.Parse(TbDiscount.Text);
                _product.Description = TbDescription.Text?.Trim();
                _product.PhotoPath = _photoPath;

                if (ConnectOdb.conObj.Products.Find(_product.Article) == null)
                    ConnectOdb.conObj.Products.Add(_product);

                ConnectOdb.conObj.SaveChanges();

                MessageBox.Show("Товар сохранён ✓", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                FrameOdb.frameMain.Navigate(new PageProduct());
            }
            catch (Exception ex)
            {
                ShowError($"Не удалось сохранить: {ex.Message}");
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => FrameOdb.frameMain.Navigate(new PageProduct());

        private void NumberInput_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text, 0) && e.Text != "," && e.Text != ".";
        }

        private void TbPrice_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                var text = (e.DataObject.GetData(typeof(string)) as string)?.Replace('.', ',');
                if (!string.IsNullOrEmpty(text))
                    e.DataObject.SetData(text);
            }
        }
    }
}