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
using System.Runtime.Serialization;

namespace Tinkoff.Acquiring.Sdk.Responses
{
    /// <summary>
    /// Ответ на запрос подтверждения платежа карточными данными.
    /// </summary>
    [DataContract]
    sealed class FinishAuthorizeResponse : AcquiringResponse
    {
        #region Fields

        [DataMember(Name = "Status")]
        private string status;

        #endregion

        #region Properties

        /// <summary>
        /// Возвращает уникальный идентификатор транзакции в системе Банка.
        /// </summary>
        [DataMember]
        public string PaymentId { get; internal set; }

        /// <summary>
        /// Возвращает номер заказа в системе Продавца.
        /// </summary>
        [DataMember]
        public string OrderId { get; internal set; }

        /// <summary>
        /// Вовращает сумму в копейках.
        /// </summary>
        [DataMember]
        public decimal Amount { get; internal set; }

        /// <summary>
        /// Возвращает статус транзакции.
        /// </summary>
        [IgnoreDataMember]
        public PaymentStatus Status { get; internal set; }

        /// <summary>
        /// Возвращает Access Control Server Url.
        /// </summary>
        [DataMember]
        public string ACSUrl { get; internal set; }

        /// <summary>
        /// Возвращает данные продавца.
        /// </summary>
        [DataMember]
        public string MD { get; internal set; }

        /// <summary>
        /// Возвращает запрос на аутентификацию плательщика.
        /// </summary>
        [DataMember]
        public string PaReq { get; internal set; }

        #endregion

        #region Internal Members

        [OnDeserialized]
        internal void OnDeserialized(StreamingContext context)
        {
            if (status == "3DS_CHECKING")
            {
                Status = PaymentStatus.DS_CHECKING;
                return;
            }
            if (status == "3DS_CHECKED")
            {
                Status = PaymentStatus.DS_CHECKED;
                return;
            }
            PaymentStatus value;
            Enum.TryParse(status, out value);
            Status = value;
        }

        #endregion
    }
}
