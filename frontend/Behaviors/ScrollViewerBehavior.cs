using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Revalis.Behaviors
{
    public static class ScrollViewerBehavior
    {
        public static readonly DependencyProperty LoadMoreCommandProperty =
            DependencyProperty.RegisterAttached(
                "LoadMoreCommand",
                typeof(ICommand),
                typeof(ScrollViewerBehavior),
                new PropertyMetadata(null, OnLoadMoreCommandChanged));

        public static void SetLoadMoreCommand(DependencyObject obj, ICommand value)
            => obj.SetValue(LoadMoreCommandProperty, value);

        public static ICommand GetLoadMoreCommand(DependencyObject obj)
            => (ICommand)obj.GetValue(LoadMoreCommandProperty);

        private static void OnLoadMoreCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollViewer scrollViewer)
            {
                scrollViewer.ScrollChanged += (s, args) =>
                {
                    if (scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight)
                    {
                        var command = GetLoadMoreCommand(scrollViewer);
                        if (command?.CanExecute(null) == true)
                            command.Execute(null);
                    }
                };
            }
        }
    }
}
