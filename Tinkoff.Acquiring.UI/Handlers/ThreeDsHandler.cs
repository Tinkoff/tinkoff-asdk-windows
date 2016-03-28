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
    /// Коллбек позволяющий получать уведомления о статусе прохождения 3DS.
    /// </summary>
    public class ThreeDsHandler
    {
        /// <summary>
        /// Вызывается в случае, если 3DS успешно пройден.
        /// </summary>
        public Action SucceededCallback { get; set; }

        /// <summary>
        /// Вызывается, если прохождение 3DS отменено пользователем.
        /// </summary>
        public Action CancelledCallback { get; set; }

        /// <summary>
        /// Вызывается, если во время проверки возникло исключение.
        /// </summary>
        public Action<Exception> FailedCallback { get; set; }
    }
}