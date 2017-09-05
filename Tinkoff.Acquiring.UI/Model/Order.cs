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

namespace Tinkoff.Acquiring.UI.Model
{
    /// <summary>
    /// Параметры заказа.
    /// </summary>
    public class Order
    {
        /// <summary>
        /// Идентификатор заказа в системе Продавца.
        /// </summary>
        public string OrderId { get; set; }

        /// <summary>
        /// Сумма покупки в копейках.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Заголовок окна.
        /// </summary>
        public string Title { get; set; } = "Оплата";

        /// <summary>
        /// Название товара.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Описание товара.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Выбранная по умолчанию карта.
        /// </summary>
        public string CardId { get; set; }

        /// <summary>
        /// Предзаполненный email покупателя для квитанции.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Идентификатор покупателя в системе Продавца
        /// </summary>
        public string CustomerKey { get; set; }

        /// <summary>
        /// Возвращает или устанавливает значение параметра, отвечающего за использование кастомной клавиатуры для ввода
        /// реквизитов карты.
        /// </summary>
        public bool CustomKeyboard { get; set; } = true;

        /// <summary>
        /// Возвращает или устанавливает JSON объект с данными чека.
        /// </summary>
        public string Receipt { get; set; }
    }
}