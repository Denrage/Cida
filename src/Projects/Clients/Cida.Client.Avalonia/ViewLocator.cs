// Copyright (c) The Avalonia Project. All rights reserved.
// Licensed under the MIT license. See licence.md file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Cida.Client.Avalonia.Api;

namespace Cida.Client.Avalonia
{
    public class ViewLocator : IDataTemplate
    {
        // Hack: Don't use static
        public static List<Assembly> Assemblies = new List<Assembly>(new [] {Assembly.GetExecutingAssembly()});
        public bool SupportsRecycling => false;

        public IControl Build(object data)
        {
            var name = data.GetType().FullName.Replace("ViewModel", "View");
            Type type = null;
            foreach (var assembly in Assemblies)
            {
                type = assembly.GetType(name);
                if (type != null)
                {
                    break;
                }
            }

            if (type != null)
            {
                return (Control)Activator.CreateInstance(type);
            }
            else
            {
                return new TextBlock { Text = "Not Found: " + name };
            }
        }

        public bool Match(object data)
        {
            return data is ViewModelBase;
        }
    }
}