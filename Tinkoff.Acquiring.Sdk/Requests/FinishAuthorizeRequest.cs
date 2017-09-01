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

namespace Tinkoff.Acquiring.Sdk.Requests
{
    /// <summary>
    /// Подтверждает инициированный платеж передачей карточных данных.
    /// </summary>
    sealed class FinishAuthorizeRequest : AcquiringRequest
    {
        #region Overriders of AcquiringRequest

        /// <summary>
        /// Вовзвращает имя опреации.
        /// </summary>
        public override string Operation => "FinishAuthorize";

        #endregion

        #region Properties

        /// <summary>
        /// Возвращает уникальный идентификатор транзакции в системе Банка.
        /// </summary>
        public string PaymentId { get; set; }

        /// <summary>
        /// Возвращает параметр, который определяет отравлять email с квитанцией или нет.
        /// </summary>
        public bool SendEmail { get; set; }

        /// <summary>
        /// Возвращает зашифрованные данные карты.
        /// </summary>
        public string CardData { get; set; }

        /// <summary>
        /// Возвращает электронный адрес на который будет отправлена квитанция об оплате.
        /// </summary>
        public string InfoEmail { get; set; }

        #endregion

        #region Methods

        public override IDictionary<string, string> ToDictionary()
        {
            var dictionary = base.ToDictionary();
            if (!string.IsNullOrEmpty(PaymentId))
                dictionary.Add(Fields.PAYMENTID, PaymentId);
            //if (SendEmail && !string.IsNullOrEmpty(InfoEmail))
            {
                dictionary.Add(Fields.SENDEMAIL, SendEmail.ToString().ToLower());
                dictionary.Add(Fields.INFOEMAIL, InfoEmail);
            }
            if (!string.IsNullOrEmpty(CardData))
                dictionary.Add(Fields.CARDDATA, CardData);
            return dictionary;
        }

        #endregion
    }
}
