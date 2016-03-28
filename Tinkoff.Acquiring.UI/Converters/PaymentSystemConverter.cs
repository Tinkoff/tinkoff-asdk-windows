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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Tinkoff.Acquiring.Sdk;

namespace Tinkoff.Acquiring.UI.Converters
{
    class PaymentSystemConverter : IValueConverter
    {
        #region Properties

        public ImageSource Visa { get; set; }

        public ImageSource MasterCard { get; set; }

        public ImageSource Maestro { get; set; }

        #endregion

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var card = value as Card;
            if (string.IsNullOrEmpty(card?.Pan)) return DependencyProperty.UnsetValue;
            switch (card.Pan[0])
            {
                case '4':
                    return Visa;
                case '2':
                case '5':
                    return MasterCard;
                case '6':
                    return Maestro;
                default:
                    return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
