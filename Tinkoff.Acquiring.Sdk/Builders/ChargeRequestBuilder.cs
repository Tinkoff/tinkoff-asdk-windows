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

using Tinkoff.Acquiring.Sdk.Requests;

namespace Tinkoff.Acquiring.Sdk.Builders
{
    class ChargeRequestBuilder : AcquiringRequestBuilder<ChargeRequest>
    {
        #region Ctor

        public ChargeRequestBuilder(string password, string terminalKey, Journal journal)
            : base(password, terminalKey, journal)
        { }

        #endregion

        #region Base Members

        protected override void Validate()
        {
            Assert.IsNonNullOrEmpty(Request.PaymentId, Fields.PAYMENTID);
            Assert.IsNonNullOrEmpty(Request.RebillId, Fields.REBILLID);
        }

        #endregion

        #region Public Members

        /// <summary>
        /// Устанавливает уникальный идентификатор транзакции в системе Банка.
        /// </summary>
        public ChargeRequestBuilder SetPaymentId(string value)
        {
            Request.PaymentId = value;
            return this;
        }

        /// <summary>
        /// Устанавливает идентификатор рекуррентного платежа.
        /// </summary>
        public ChargeRequestBuilder SetRebillId(string value)
        {
            Request.RebillId = value;
            return this;
        }

        #endregion
    }
}
