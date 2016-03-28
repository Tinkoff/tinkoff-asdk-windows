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

namespace Tinkoff.Acquiring.UI.Handlers
{
    /// <summary>
    /// Коллбек позволяющий получать уведомления о статусе проведения платежа.
    /// </summary>
    public class PaymentHandler
    {
        /// <summary>
        /// Вызывается в случае, если платеж прошел успешно.
        /// </summary>
        public Action<string> SucceededCallback { get; set; }

        /// <summary>
        /// Вызывается, если платеж отменен пользователем.
        /// </summary>
        public Action CancelledCallback { get; set; }

        /// <summary>
        /// Вызывается, если не удалось совершить платеж.
        /// </summary>
        public Action<Exception> FailedCallback { get; set; }
    }
}