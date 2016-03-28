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
    class Journal
    {
        #region Fields

        private ILogger logger = new DefaultLogger();

        #endregion

        #region Properties

        public bool IsDebug { get; set; }

        #endregion

        #region Public Members

        public void Log(object value)
        {
            if (IsDebug && value != null) logger.Log(value.ToString());
        }

        public void Log(string message)
        {
            if (IsDebug)
                logger.Log(message);
        }

        public void Log(Exception e)
        {
            if (IsDebug)
                logger.Log(e);
        }

        public void SetLogger(ILogger logger)
        {
            this.logger = logger;
        }

        #endregion
    }
}