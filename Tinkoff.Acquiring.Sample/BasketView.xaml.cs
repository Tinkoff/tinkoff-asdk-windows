#region License

// Copyright © 2016 Tinkoff Bank
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//  
//     http://www.apache.org/licenses/LICENSE-2.0
//  
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Tinkoff.Acquiring.Sample.Models;
using Tinkoff.Acquiring.UI;
using Tinkoff.Acquiring.UI.Handlers;
using Tinkoff.Acquiring.UI.Model;

namespace Tinkoff.Acquiring.Sample
{
    public sealed partial class BasketView
    {
        private Color? oldStatusBarBackground;
        private double oldStatusBarOpacity;
        private Color? oldStatusBarForeground;
        private decimal total;

        private readonly NumberFormatInfo numberFormatInfo = NumberFormatInfo.GetInstance(new CultureInfo("ru"));

        public ObservableCollection<BasketItem> Items => App.Basket;  

        public BasketView()
        {
            InitializeComponent();

            ListView.ItemsSource = Items;
            UpdateTotal();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.SaveStatusBarColors(ref oldStatusBarBackground, ref oldStatusBarOpacity, ref oldStatusBarForeground);
            this.RecolorStatusBar((Color)Application.Current.Resources["Cerulean"], 1, Colors.White);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.RecolorStatusBar(oldStatusBarBackground, oldStatusBarOpacity, oldStatusBarForeground);
        }

        private void OnRemoveClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var id = button?.Tag?.ToString();
            if(string.IsNullOrEmpty(id)) return;

            var book = Items.FirstOrDefault(item => item.Item.Id == id);
            if (book == null) return;

            if (book.Quantity == 1)
            {
                Items.Remove(book);
            }
            else
            {
                var index = Items.IndexOf(book);
                Items[index] = new BasketItem {Item = book.Item, Quantity = book.Quantity - 1};
            }
            UpdateTotal();
        }

        private void OnAboutClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AboutView));
        }

        private async void OnPayClick(object sender, RoutedEventArgs e)
        {
            var items = Items;
            if (!items.Any()) return;

            var order = new Order
            {
                OrderId = Guid.NewGuid().ToString(),
                Amount = total * 100,
                Title = items.Count == 1 ? items.First().Item.VolumeInfo.Title : "Покупка книг",
                Description = string.Join(", ", items.Select(item => item.Item.VolumeInfo.Description)),
                CustomerKey = App.CustomerKey
            };

            var paymentListener = new PaymentHandler
            {
                SucceededCallback = paymentId =>
                {
                    Items.Clear();
                    Frame.Navigate(typeof (ConfirmationView), total);
                },
                CancelledCallback = () => { },
                FailedCallback = async exception =>
                {
                    var messageDialog = new MessageDialog("Платёж отклонён банком. Убедитесь, что на карте достаточно средств и попробуйте снова.", "Платёж отклонён");
                    await messageDialog.ShowAsync();
                }
            };

            await AcquiringUI.ShowPaymentViewAsync(order, paymentListener);
        }

        private void UpdateTotal()
        {
            total = Items.Sum(item => item.Item.SaleInfo.Price * item.Quantity);
            TotalAmount.Text = string.Format(numberFormatInfo, "{0:C2}", total);
        }

    }
}
