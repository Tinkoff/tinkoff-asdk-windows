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

namespace Tinkoff.Acquiring.Sdk
{
    class Assert
    {
        #region Public Members

        public static void IsNonNegative(int value, string field)
        {
            Assert.That(arg => arg >= 0, value, field);
        }

        public static void IsNonNegative(decimal value, string field)
        {
            Assert.That(arg => arg >= decimal.Zero, value, field);
        }

        public static void IsNonNullOrEmpty(string value, string field)
        {
            Assert.That(arg => !string.IsNullOrEmpty(arg), value, field);
        }

        #endregion

        #region Private Members
 

        private static void That<T>(Func<T, bool> func, T value, string field)
        {
            if (!func(value))
                throw new ArgumentException(string.Format("Unable to build request: field '{0}' is not valid", field));
        }

        #endregion
    }
}
