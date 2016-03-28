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
using System.Reflection;
using Windows.UI.Xaml;
using Microsoft.Xaml.Interactivity;

namespace Tinkoff.Acquiring.UI.Behaviors
{
    abstract class Behavior<T> : DependencyObject, IBehavior where T : DependencyObject
    {
        #region Properties

        public T AssociatedObject { get; private set; }

        #endregion

        #region IBehavior Members

        DependencyObject IBehavior.AssociatedObject => AssociatedObject;

        void IBehavior.Attach(DependencyObject associatedObject)
        {
            if (associatedObject != null)
            {
                var type = associatedObject.GetType();
                if (type != typeof (T) && !type.GetTypeInfo().IsSubclassOf(typeof (T)))
                    throw new Exception("Invalid target type");
            }
            AssociatedObject = associatedObject as T;
            OnAttached();
        }

        void IBehavior.Detach()
        {
            OnDetached();
            AssociatedObject = null;
        }

        #endregion

        #region Protected Members

        protected abstract void OnAttached();

        protected abstract void OnDetached();

        #endregion
    }
}