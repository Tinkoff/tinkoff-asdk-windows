﻿#region License

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
using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using Tinkoff.Acquiring.Sdk.Requests;

namespace Tinkoff.Acquiring.Sdk.Builders
{
    class InitRequestBuilder : AcquiringRequestBuilder<InitRequest>
    {
        #region Ctor

        public InitRequestBuilder(string password, string terminalKey, Journal journal)
            : base(password, terminalKey, journal)
        { }

        #endregion

        #region Overrides of AcquiringRequestBuilder<InitRequest>

        protected override void Validate()
        {
            Assert.IsNonNegative(Request.Amount, Fields.AMOUNT);
            Assert.IsNonNullOrEmpty(Request.OrderId, Fields.ORDERID);
        }

        #endregion

        #region Public Members

        /// <summary>
        /// Устанавливает сумму в копейках.
        /// </summary>
        public InitRequestBuilder SetAmount(decimal value)
        {
            Request.Amount = value;

            return this;
        }

        /// <summary>
        /// Устанавливает номер заказа в системе продавца.
        /// </summary>
        public InitRequestBuilder SetOrderId(string value)
        {
            Request.OrderId = value;

            return this;
        }

        /// <summary>
        /// Устанавливает краткое описание.
        /// </summary>
        public InitRequestBuilder SetDescription(string value)
        {
            Request.Description = value;

            return this;
        }

        /// <summary>
        /// Устанавливает название шаблона формы оплаты продавца.
        /// </summary>
        public InitRequestBuilder SetPayForm(string value)
        {
            Request.PayForm = value;

            return this;
        }

        /// <summary>
        /// Устанавливает идентификатор покупателя в системе Продавца.
        /// </summary>
        public InitRequestBuilder SetCustomerKey(string value)
        {
            Request.CustomerKey = value;

            return this;
        }

        /// <summary>
        /// Устанавливает параметр, который определяет регистрировать платеж как рекуррентный или нет.
        /// </summary>
        public InitRequestBuilder SetRecurrent(bool value)
        {
            Request.Recurrent = value;

            return this;
        }

        /// <summary>
        /// Устанавливает JSON объект с данными чека.
        /// </summary>
        public InitRequestBuilder SetReceipt(string value)
        {
            Request.Receipt = new JRaw(value);

            return this;
        }

        public InitRequestBuilder AddData([NotNull] string key, [CanBeNull] string value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (Request.Data.Count >= 20) throw new InvalidOperationException("You can not pass more than 20 key-value pairs.");
            if (key.Length > 20) throw new ArgumentOutOfRangeException(nameof(key), "Key can not be longer than 20.");
            if (value?.Length > 100) throw new ArgumentOutOfRangeException(nameof(value), "Value can not be longer than 100.");

            Request.Data.Add(key, value);

            return this;
        }

        public InitRequestBuilder AddData([NotNull] IReadOnlyList<KeyValuePair<string, string>> data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (Request.Data.Count + data.Count > 20) throw new InvalidOperationException("You can not pass more than 20 key-value pairs.");

            foreach (var pair in data)
            {
                if (pair.Key.Length > 20) throw new ArgumentOutOfRangeException(nameof(pair.Key), "Key can not be longer than 20.");
                if (pair.Value?.Length > 100) throw new ArgumentOutOfRangeException(nameof(pair.Value), "Value can not be longer than 100.");

                Request.Data.Add(pair.Key, pair.Value);
            }

            return this;
        }

        #endregion
    }
}