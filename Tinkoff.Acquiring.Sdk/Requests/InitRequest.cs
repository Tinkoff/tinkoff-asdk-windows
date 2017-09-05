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

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Tinkoff.Acquiring.Sdk.Requests
{
    /// <summary>
    /// Инициирует платежную сессию.
    /// </summary>
    sealed class InitRequest : AcquiringRequest
    {
        #region Properties

        /// <summary>
        /// Возвращает сумму в копейках.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Возвращает номер заказа в системе продавца.
        /// </summary>
        public string OrderId { get; set; }

        /// <summary>
        /// Возвращает краткое описание.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Возвращает название шаблона формы оплаты продавца.
        /// </summary>
        public string PayForm { get; set; }

        /// <summary>
        /// Возвращает идентификатор покупателя в системе Продавца.
        /// </summary>
        public string CustomerKey { get; set; }

        /// <summary>
        /// Возвращает параметр, который определяет регистрировать платеж как рекуррентный или нет.
        /// </summary>
        public bool Recurrent { get; set; }

        /// <summary>
        /// Возвращает	JSON объект с данными чека.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public JRaw Receipt { get; set; }

        #endregion

        #region Overrides of AcquiringRequest

        /// <summary>
        /// Вовзвращает имя опреации.
        /// </summary>
        public override string Operation => "Init";

        public override IDictionary<string, string> ToDictionary()
        {
            var dictionary = base.ToDictionary();
            dictionary.Add(Fields.AMOUNT, string.Format("{0:0}", Amount));

            if (!string.IsNullOrEmpty(OrderId))
                dictionary.Add(Fields.ORDERID, OrderId);

            if (!string.IsNullOrEmpty(Description))
                dictionary.Add(Fields.DESCRIPTION, Description);

            if (!string.IsNullOrEmpty(PayForm))
                dictionary.Add(Fields.PAYFORM, PayForm);

            if (!string.IsNullOrEmpty(CustomerKey))
                dictionary.Add(Fields.CUSTOMERKEY, CustomerKey);

            if (Recurrent)
                dictionary.Add(Fields.RECURRENT, "Y");

            return dictionary;
        }

        #endregion
    }
}
