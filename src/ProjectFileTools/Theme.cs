﻿using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualStudio.Shell;

namespace ProjectFileTools
{
    public static class Theme
    {
        private static ResourceDictionary BuildThemeResources()
        {
            ResourceDictionary allResources = new ResourceDictionary();
            ResourceDictionary shellResources = (ResourceDictionary) Application.LoadComponent(new Uri("Microsoft.VisualStudio.Platform.WindowManagement;component/Themes/ThemedDialogDefaultStyles.xaml", UriKind.Relative));
            ResourceDictionary scrollStyleContainer = (ResourceDictionary) Application.LoadComponent(new Uri("Microsoft.VisualStudio.Shell.UI.Internal;component/Styles/ScrollBarStyle.xaml", UriKind.Relative));
            allResources.MergedDictionaries.Add(shellResources);
            allResources.MergedDictionaries.Add(scrollStyleContainer);
            allResources[typeof (ScrollViewer)] = new Style
            {
                TargetType = typeof (ScrollViewer),
                BasedOn = (Style) scrollStyleContainer[VsResourceKeys.ScrollViewerStyleKey]
            };
            return allResources;
        }

        private static ResourceDictionary ThemeResources { get; } = BuildThemeResources();

        public static void ShouldBeThemed(this FrameworkElement control)
        {
            if (control.Resources == null)
            {
                control.Resources = ThemeResources;
            }
            else if(control.Resources != ThemeResources)
            {
                ResourceDictionary d = new ResourceDictionary();
                d.MergedDictionaries.Add(ThemeResources);
                d.MergedDictionaries.Add(control.Resources);
                control.Resources = null;
                control.Resources = d;
            }
        }
    }
}
