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

namespace Tinkoff.Acquiring.Sdk
{
    /// <summary>
    /// Статус платежа.
    /// </summary>
    public enum PaymentStatus
    {
        /// <summary>
        /// Платеж зарегистрирован в шлюзе, но его обработка в процессинге не начата. 
        /// </summary>
        NEW,

        /// <summary>
        /// Платеж отменен Продавцом. 
        /// </summary>
        CANCELLED,

        /// <summary>
        /// Проверка платежных данных Покупателя.
        /// </summary>
        PREAUTHORIZING,

        /// <summary>
        /// Покупатель переправлен на страницу оплаты.
        /// </summary>
        FORMSHOWED,

        /// <summary>
        /// Покупатель начал аутентификацию. 
        /// </summary>
        AUTHORIZING,

        /// <summary>
        /// Покупатель начал аутентификацию по протоколу 3-D Secure. 
        /// </summary>
        DS_CHECKING,

        /// <summary>
        /// Покупатель завершил аутентификацию по протоколу 3-D Secure. 
        /// </summary>
        DS_CHECKED,

        /// <summary>
        /// Средства заблокированы, но не списаны. 
        /// </summary>
        AUTHORIZED,

        /// <summary>
        /// Начало отмены блокировки средств. 
        /// </summary>
        REVERSING,

        /// <summary>
        /// Денежные средства разблокированы. 
        /// </summary>
        REVERSED,

        /// <summary>
        /// Начало списания денежных средств. 
        /// </summary>
        CONFIRMING,

        /// <summary>
        /// Денежные средства списаны. 
        /// </summary>
        CONFIRMED,

        /// <summary>
        /// Начало возврата денежных средств. 
        /// </summary>
        REFUNDING,

        /// <summary>
        /// Произведен возврат денежных средств.
        /// </summary>
        REFUNDED,

        /// <summary>
        /// Платеж отклонен Банком. 
        /// </summary>
        REJECTED,

        /// <summary>
        /// Статус не определен. 
        /// </summary>
        UNKNOWN
    }
}
