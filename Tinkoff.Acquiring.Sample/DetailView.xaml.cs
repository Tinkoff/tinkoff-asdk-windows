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
using System.Linq;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using Tinkoff.Acquiring.Sample.Models;
using Tinkoff.Acquiring.UI;
using Tinkoff.Acquiring.UI.Handlers;
using Tinkoff.Acquiring.UI.Model;

namespace Tinkoff.Acquiring.Sample
{
    public sealed partial class DetailView
    {
        private Item item;
        private Color? oldStatusBarBackground;
        private double oldStatusBarOpacity;
        private Color? oldStatusBarForeground;

        public DetailView()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            item = e.Parameter as Item;
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            DataContext = item;

            this.SaveStatusBarColors(ref oldStatusBarBackground, ref oldStatusBarOpacity, ref oldStatusBarForeground);
            this.RecolorStatusBar((Color)Application.Current.Resources["Cerulean"], 1, Colors.White);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.RecolorStatusBar(oldStatusBarBackground, oldStatusBarOpacity, oldStatusBarForeground);
        }

        private void OnBasketClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(BasketView));
        }

        private void OnAboutClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AboutView));
        }

        private async void OnAddClick(object sender, RoutedEventArgs e)
        {
            MessageDialog dialog;
            var book = App.Basket.FirstOrDefault(store => store.Item.Id == item.Id);
            if (book != null)
            {
                book.Quantity++;
                dialog = new MessageDialog(string.Format("Книга '{0}' повторно добавлена в корзину", item.VolumeInfo.Title), "Интернет магазин книг");
            }
            else
            {
                App.Basket.Add(new BasketItem {Item = item, Quantity = 1});
                dialog = new MessageDialog(string.Format("Книга '{0}' добавлена в корзину", item.VolumeInfo.Title), "Интернет магазин книг");
            }
            dialog.Commands.Add(new UICommand("Закрыть"));
            dialog.CancelCommandIndex = 0;
            await dialog.ShowAsync();
        }

        private async void OnPayClick(object sender, RoutedEventArgs e)
        {
            var product = new Order
            {
                OrderId = Guid.NewGuid().ToString(),
                Amount = item.SaleInfo.Price * 100,
                Title = item.VolumeInfo.Title,
                Description = item.VolumeInfo.Description,
                CustomerKey = App.CustomerKey,
            };

            var paymentListener = new PaymentHandler
            {
                SucceededCallback = paymentId =>
                {
                    Frame.Navigate(typeof (ConfirmationView), item.SaleInfo.Price);
                },
                CancelledCallback = () => { },
                FailedCallback = async exception =>
                {
                    var messageDialog = new MessageDialog("Платёж отклонён банком. Убедитесь, что на карте достаточно средств и попробуйте снова.", "Платёж отклонён");
                    await messageDialog.ShowAsync();
                }
            };

            await AcquiringUI.ShowPaymentViewAsync(product, paymentListener);
        }
    }
}
