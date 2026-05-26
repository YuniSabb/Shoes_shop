using Shoes_shop.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Shoes_shop.Pages
{
    public partial class PageEditOrder : Page
    {
        private Orders _currentOrder;

        public PageEditOrder(Orders order = null)
        {
            InitializeComponent();
            LoadComboBoxes();

            if (order != null)
            {
                _currentOrder = order;
                LoadOrderData();
            }
        }

        private void LoadComboBoxes()
        {
            // Пункты выдачи
            CbPickupPoint.ItemsSource = ConnectOdb.conObj.PickupPoints.ToList();

            // Клиенты: фильтрация по RoleId с защитой от null
                var clients = ConnectOdb.conObj.Users
                    .Where(u => u.RoleId == 1)
                    .ToList();
        
                CbCustomer.ItemsSource = clients;
        
            CbCustomer.SelectedValuePath = "Id";

            // Статусы заказа
            CbStatus.ItemsSource = ConnectOdb.conObj.OrderStatuses.ToList();
            CbStatus.DisplayMemberPath = "StatusName";
        }

        private void LoadOrderData()
        {
            TbOrderNumber.Text = _currentOrder.OrderNumber.ToString();
            DpOrderDate.SelectedDate = _currentOrder.OrderDate;
            DpDeliveryDate.SelectedDate = _currentOrder.DeliveryDate;

            CbPickupPoint.SelectedValue = _currentOrder.PickupPointId;
            CbCustomer.SelectedValue = _currentOrder.CustomerId;
            CbStatus.SelectedValue = _currentOrder.StatusId;

            TbPickupCode.Text = _currentOrder.PickupCode;
        }


        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_currentOrder == null)
                {
                    _currentOrder = new Orders();
                    ConnectOdb.conObj.Orders.Add(_currentOrder);
                }

                _currentOrder.OrderDate = DpOrderDate.SelectedDate ?? DateTime.Today;
                _currentOrder.DeliveryDate = DpDeliveryDate.SelectedDate;
                _currentOrder.PickupPointId = ((PickupPoints)CbPickupPoint.SelectedItem)?.Id ?? 0;
                _currentOrder.CustomerId = ((Users)CbCustomer.SelectedItem)?.Id ?? 0;
                _currentOrder.StatusId = ((OrderStatuses)CbStatus.SelectedItem)?.Id ?? 0;
                _currentOrder.PickupCode = TbPickupCode.Text;

                ConnectOdb.conObj.SaveChanges();

                MessageBox.Show("Заказ сохранён", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                FrameOdb.frameMain.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            FrameOdb.frameMain.GoBack();
        }
    }
}