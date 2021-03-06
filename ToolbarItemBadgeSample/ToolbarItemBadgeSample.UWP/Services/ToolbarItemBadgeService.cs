﻿using System.Reflection;
using ToolbarItemBadgeSample.Services;
using ToolbarItemBadgeSample.UWP.Services;
using ToolbarItemBadgeSample.UWP.Utils;
using Windows.UI.Xaml.Controls;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;
using Page = Xamarin.Forms.Page;

[assembly: Dependency(typeof(ToolbarItemBadgeService))]
namespace ToolbarItemBadgeSample.UWP.Services
{
    /// <summary>
    /// Represents service for toolbar item badge
    /// </summary>
    public class ToolbarItemBadgeService : IToolbarItemBadgeService
    {
        /// <summary>
        /// Set badge on toolbar item
        /// </summary>
        /// <param name="page">Page where toolbar item exists</param>
        /// <param name="item">Toolbar item</param>
        /// <param name="value">Value to show as badge</param>
        /// <param name="backgroundColor">Background color for badge</param>
        /// <param name="textColor">Foreground color for barge</param>
        public void SetBadge(Page page, ToolbarItem item, string value, Color backgroundColor, Color textColor)
        {
            Device.BeginInvokeOnMainThread(() => // perform the action in main thread
            {
                if (!(GetAppBarButton(page, item) is AppBarButton appBarButton)) return; // try getting app bar button from page
                appBarButton.AddBadge(item, value, backgroundColor, textColor); // update badge item
            });
        }

        private AppBarButton GetAppBarButton(Page page, ToolbarItem item)
        {
            if (page == null) return null; // if page is null, return null

            var renderer = Platform.GetRenderer(page); // get platform renderer of page
            if (renderer == null) // if renderer not found
            {
                renderer = Platform.CreateRenderer(page); // create a renderer
                Platform.SetRenderer(page, renderer); // set to Xamarin.Forms control
            }

            switch (renderer) // check type of renderer
            {
                // if platform renderer is of type MasterDetailPageRenderer and if platform (UWP) control is of type MasterDetailControl
                case MasterDetailPageRenderer masterDetailPageRenderer when masterDetailPageRenderer.Control is MasterDetailControl masterDetailControl:
                    return GetAppBarButton(page, item, masterDetailControl);
                // if platform renderer is of type NavigationPageRenderer and if platform (UWP) control is of type PageControl
                case NavigationPageRenderer navigationPageRenderer when navigationPageRenderer.ContainerElement is PageControl pageControl:
                    return GetAppBarButton(page, item, pageControl);
                // if platform renderer is of type PageRenderer and if platform (UWP) control is of type PageControl
                case PageRenderer pageRenderer when pageRenderer.Parent is PageControl pageControl:
                    return GetAppBarButton(page, item, pageControl);
                default: // if type not matched
                    return null; // return null
            }
        }

        /// <summary>
        /// Fetches App Bar Button from given page
        /// </summary>
        /// <typeparam name="T">Type of platform control</typeparam>
        /// <param name="page">Page</param>
        /// <param name="item">Toolbar item in page</param>
        /// <param name="platformControl">UWP equivalent control</param>
        /// <returns></returns>
        private AppBarButton GetAppBarButton<T>(Page page, ToolbarItem item, T platformControl)
        {
            var commandBarField = typeof(T).GetField("_commandBar",
                BindingFlags.Instance | BindingFlags.NonPublic |
                BindingFlags.GetField); // get "_commandBar" private field from UWP platformControl
            if (!(commandBarField?.GetValue(platformControl) is FormsCommandBar commandBar))
                return null; // try getting value, else return

            var primaryItemsControlField = typeof(FormsCommandBar).GetField("_primaryItemsControl",
                BindingFlags.Instance | BindingFlags.NonPublic |
                BindingFlags.GetField); // get "_primaryItemsControl" private field from UWP FormsCommandBar
            if (primaryItemsControlField?.GetValue(commandBar) is ItemsControl primaryItemsControl) // if primaryItemsControl of type ItemsControl
            {
                if (primaryItemsControl.Items?.Count > 0) // if control has any items
                {
                    var index = page.ToolbarItems.IndexOf(item); // get the toolbar item's index in the page
                    index = page.ToolbarItems.Count - index - 1; // get the index in reverse order
                    if (!(primaryItemsControl.Items?[index] is AppBarButton appBarButton))
                        return null; // get the toolbar item in uwp toolbar

                    return appBarButton; // return app bar button 
                }
            }

            if (!(page.Parent is Page parentPage)) return null; // if not fetched, try to get from its parent page

            return GetAppBarButton(parentPage, item); // fetch from parent page
        }
    }


}
