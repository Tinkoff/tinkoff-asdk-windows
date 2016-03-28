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

namespace Tinkoff.Acquiring.UI.Model
{
    class CardNumberField : ICardNumberField
    {
        #region Fields

        private State state = State.PartiallyValid;
        private string data = string.Empty;

        #endregion

        #region Ctor

        public CardNumberField()
        {
        }

        public CardNumberField(string maskedData)
        {
            Data = maskedData;
            FormattedData = FormatNumber(maskedData);
            state = State.Valid;
            IsReadOnly = true;
        }

        #endregion

        #region Properties

        public bool IsReadOnly { get; }

        public bool IsValid => state == State.Valid;

        public string Data
        {
            get { return data; }
            set
            {
                if (IsReadOnly) throw new InvalidOperationException("Field is immutable.");
                if (value == null) throw new ArgumentNullException(nameof(value));

                value = new string(value.Where(char.IsDigit).ToArray());

                if (Validate(value))
                {
                    data = value;
                    FormattedData = FormatNumber(data);
                }
                else
                {
                    throw new ArgumentException();
                }
            }
        }

        public string FormattedData { get; private set; } = string.Empty;

        #endregion

        #region ICardNumberField Members

        public PaymentSystem PaymentSystem => RecognizePaymentSystem(Data.FirstOrDefault());

        public int MaxLength => PaymentSystem == PaymentSystem.Maestro ? 19 : 16;

        #endregion

        #region Private Members

        private bool Validate(string cardNumber)
        {
            switch (RecognizePaymentSystem(cardNumber.FirstOrDefault()))
            {
                case PaymentSystem.MasterCard:
                case PaymentSystem.Visa:
                case PaymentSystem.Undefined:
                    if (cardNumber.Length <= 16)
                    {
                        if (cardNumber.Length == 16)
                        {
                            if (LuhnValidation(cardNumber))
                            {
                                state = State.Valid;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            state = State.PartiallyValid;
                        }
                        return true;
                    }
                    return false;
                case PaymentSystem.Maestro:
                    if (cardNumber.Length <= 19)
                    {
                        if (cardNumber.Length == 18)
                        {
                            if (LuhnValidation(cardNumber))
                            {
                                state = State.Valid;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else if (cardNumber.Length == 19)
                        {
                            if (LuhnValidation(cardNumber))
                            {
                                state = State.Valid;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            state = State.PartiallyValid;
                        }
                        return true;
                    }
                    return false;
            }
            return true;
        }

        private static PaymentSystem RecognizePaymentSystem(char firstChar)
        {
            switch (firstChar)
            {
                case '2':
                case '5':
                    return PaymentSystem.MasterCard;
                case '4':
                    return PaymentSystem.Visa;
                case '6':
                    return PaymentSystem.Maestro;
                default:
                    return PaymentSystem.Undefined;
            }
        }

        private static bool LuhnValidation(string cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber))
                return false;

            cardNumber = cardNumber.Replace(" ", string.Empty);
            var sum = 0;

            for (var i = cardNumber.Length - 1; i >= 0; i--)
            {
                var curChar = cardNumber[i].ToString();
                if (!string.IsNullOrEmpty(curChar))
                {
                    int value;
                    if (int.TryParse(curChar, out value))
                    {
                        var shouldBeDoubled = (cardNumber.Length - i)%2 == 0;

                        if (shouldBeDoubled)
                        {
                            value *= 2;
                            sum += value > 9 ? 1 + value%10 : value;
                        }
                        else
                            sum += value;
                    }
                    else
                        return false;
                }
            }

            return sum%10 == 0;
        }

        private string FormatNumber(string value)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < value.Length; ++i)
            {
                sb.Append(value[i]);

                if (PaymentSystem != PaymentSystem.Maestro && (i + 1) % 4 == 0 && (i + 1) != 16 ||
                    PaymentSystem == PaymentSystem.Maestro && (i + 1) == 8)
                {
                    sb.Append(' ');
                }
            }
            return sb.ToString();
        }

        #endregion
    }
}