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
using System.Linq;
using System.Text;
using Tinkoff.Acquiring.Sdk.Requests;

namespace Tinkoff.Acquiring.Sdk.Builders
{
    abstract class AcquiringRequestBuilder<T> where T : AcquiringRequest, new()
    {
        #region Fields

        private readonly string password;
        private readonly string terminalKey;
        private readonly Journal journal;

        #endregion

        #region Ctor

        protected AcquiringRequestBuilder(string password, string terminalKey, Journal journal)
        {
            this.password = password;
            this.terminalKey = terminalKey;
            this.journal = journal;
            Request = new T();
        }

        #endregion

        #region Properties

        protected T Request { get; }

        #endregion

        #region Public Members

        public T Build()
        {
            try
            {
                Validate();
            }
            catch (ArgumentException ex)
            {
                journal.Log(ex.Message);
                throw;
            }
            Request.TerminalKey = terminalKey;
            Request.Token = MakeToken();

            return Request;
        }

        #endregion

        #region Protected Members

        protected abstract void Validate();

        #endregion

        #region Private Members

        private string MakeToken()
        {
            var dictionary = Request.ToDictionary();
            dictionary.Remove(Fields.TOKEN);
            dictionary.Add(Fields.PASSWORD, password);

            var builder = new StringBuilder();
            foreach (var pair in dictionary.OrderBy(pair => pair.Key))
            {
                builder.Append(pair.Value);
            }

            return CryptoUtils.Sha256(builder.ToString());
        }

        #endregion
    }
}
