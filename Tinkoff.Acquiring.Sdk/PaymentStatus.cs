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

using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Tinkoff.Acquiring.Sdk
{
    /// <summary>
    /// Статус платежа.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PaymentStatus
    {
        /// <summary>
        /// Платеж зарегистрирован в шлюзе, но его обработка в процессинге не начата. 
        /// </summary>
        [EnumMember(Value = "NEW")]
        NEW,

        /// <summary>
        /// Платеж отменен Продавцом. 
        /// </summary>
        [EnumMember(Value = "CANCELLED")]
        CANCELLED,

        /// <summary>
        /// Проверка платежных данных Покупателя.
        /// </summary>
        [EnumMember(Value = "PREAUTHORIZING")]
        PREAUTHORIZING,

        /// <summary>
        /// Покупатель переправлен на страницу оплаты.
        /// </summary>
        [EnumMember(Value = "FORMSHOWED")]
        FORMSHOWED,

        /// <summary>
        /// Покупатель начал аутентификацию. 
        /// </summary>
        [EnumMember(Value = "AUTHORIZING")]
        AUTHORIZING,

        /// <summary>
        /// Покупатель начал аутентификацию по протоколу 3-D Secure. 
        /// </summary>
        [EnumMember(Value = "3DS_CHECKING")]
        DS_CHECKING,

        /// <summary>
        /// Покупатель завершил аутентификацию по протоколу 3-D Secure. 
        /// </summary>
        [EnumMember(Value = "3DS_CHECKED")]
        DS_CHECKED,

        /// <summary>
        /// Средства заблокированы, но не списаны. 
        /// </summary>
        [EnumMember(Value = "AUTHORIZED")]
        AUTHORIZED,

        /// <summary>
        /// Начало отмены блокировки средств. 
        /// </summary>
        [EnumMember(Value = "REVERSING")]
        REVERSING,

        /// <summary>
        /// Денежные средства разблокированы. 
        /// </summary>
        [EnumMember(Value = "REVERSED")]
        REVERSED,

        /// <summary>
        /// Начало списания денежных средств. 
        /// </summary>
        [EnumMember(Value = "CONFIRMING")]
        CONFIRMING,

        /// <summary>
        /// Денежные средства списаны. 
        /// </summary>
        [EnumMember(Value = "CONFIRMED")]
        CONFIRMED,

        /// <summary>
        /// Начало возврата денежных средств. 
        /// </summary>
        [EnumMember(Value = "REFUNDING")]
        REFUNDING,

        /// <summary>
        /// Произведен возврат денежных средств.
        /// </summary>
        [EnumMember(Value = "REFUNDED")]
        REFUNDED,

        /// <summary>
        /// Платеж отклонен Банком. 
        /// </summary>
        [EnumMember(Value = "REJECTED")]
        REJECTED,

        /// <summary>
        /// Статус не определен. 
        /// </summary>
        UNKNOWN
    }
}