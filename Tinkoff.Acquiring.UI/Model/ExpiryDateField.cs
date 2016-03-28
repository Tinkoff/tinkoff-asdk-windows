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
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Tinkoff.Acquiring.UI.Model
{
    class ExpiryDateField : ICardModelField
    {
        #region Fields

        private const string Format = @"MM\/yy";
        private readonly Regex completedInputRegex = new Regex("^[0-9]{2}/[0-9]{2}$");
        private string data = string.Empty;
        private State state;

        #endregion

        #region Ctor

        public ExpiryDateField()
        {
        }

        public ExpiryDateField(string maskedData)
        {
            data = maskedData;
            state = State.Valid;
            IsReadOnly = true;
        }

        #endregion


        #region Properties

        public bool IsReadOnly { get; }
        public bool IsValid => state == State.Valid;

        public string Data
        {
            get
            {
                return data;
            }
            set
            {
                if (IsReadOnly) throw new InvalidOperationException("Field is immutable.");
                if (value == null) throw new ArgumentNullException(nameof(value));

                value = new string(value.Where(char.IsDigit).Take(4).ToArray());

                if (Validate(value))
                {
                    data = value;
                }
                else
                {
                    throw new ArgumentException();
                }
            }
        }

        public string FormattedData => Data.Length > 1 && !IsReadOnly ? Data.Insert(2, "/") : Data;

        #endregion

        #region Private Members

        private bool Validate(string text)
        {
            switch (text.Length)
            {
                case 0:
                case 1:
                case 2:
                    state = State.PartiallyValid;
                    return true;
                case 3:
                case 4:
                    text = text.Insert(2, "/");
                    if (text.Length == 5)
                        goto case 5;

                    state = State.PartiallyValid;
                    return true;
                case 5:
                    if (completedInputRegex.IsMatch(text))
                    {
                        DateTime temp;
                        if (DateTime.TryParseExact(text, Format, null, DateTimeStyles.None, out temp))
                        {
                            if (temp.Year < 2000)
                            {
                                temp = temp.AddYears(100);
                            }
                            var now = DateTime.Now;
                            if (temp.Year == now.Year && temp.Month >= now.Month ||
                                temp.Year > now.Year && temp.Year - now.Year <= 20)
                            {
                                state = State.Valid;
                                return true;
                            }
                        }
                        state = State.PartiallyValid;
                        return true;
                    }
                    return false;
            }
            return false;
        }

        #endregion
    }
}