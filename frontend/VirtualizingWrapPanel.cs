using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Revalis
{
    public class VirtualizingWrapPanel : VirtualizingPanel, IScrollInfo
    {
        // Dependency properties for item sizing and margin
        public static readonly DependencyProperty ItemWidthProperty =
            DependencyProperty.Register(nameof(ItemWidth), typeof(double), typeof(VirtualizingWrapPanel),
                new FrameworkPropertyMetadata(200.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static readonly DependencyProperty ItemHeightProperty =
            DependencyProperty.Register(nameof(ItemHeight), typeof(double), typeof(VirtualizingWrapPanel),
                new FrameworkPropertyMetadata(300.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static readonly DependencyProperty ItemMarginProperty =
            DependencyProperty.Register(nameof(ItemMargin), typeof(Thickness), typeof(VirtualizingWrapPanel),
                new FrameworkPropertyMetadata(new Thickness(10), FrameworkPropertyMetadataOptions.AffectsMeasure));

        public double ItemWidth
        {
            get => (double)GetValue(ItemWidthProperty);
            set => SetValue(ItemWidthProperty, value);
        }

        public double ItemHeight
        {
            get => (double)GetValue(ItemHeightProperty);
            set => SetValue(ItemHeightProperty, value);
        }

        public Thickness ItemMargin
        {
            get => (Thickness)GetValue(ItemMarginProperty);
            set => SetValue(ItemMarginProperty, value);
        }

        // Internal layout/scroll state
        private Size _extent = new Size(0, 0);
        private Size _viewport = new Size(0, 0);
        private Point _offset;

        public bool CanHorizontallyScroll { get; set; }
        public bool CanVerticallyScroll { get; set; }
        public double ExtentHeight => _extent.Height;
        public double ExtentWidth => _extent.Width;
        public double ViewportHeight => _viewport.Height;
        public double ViewportWidth => _viewport.Width;
        public double HorizontalOffset => _offset.X;
        public double VerticalOffset => _offset.Y;
        public ScrollViewer ScrollOwner { get; set; }

        // Track realized indices for cleanup/arrange
        private static readonly DependencyProperty ItemIndexProperty =
            DependencyProperty.RegisterAttached("ItemIndex", typeof(int), typeof(VirtualizingWrapPanel),
                new FrameworkPropertyMetadata(-1));

        private static void SetItemIndex(UIElement element, int index) => element.SetValue(ItemIndexProperty, index);
        private static int GetItemIndex(UIElement element) => (int)element.GetValue(ItemIndexProperty);

        protected override void OnItemsChanged(object sender, ItemsChangedEventArgs args)
        {
            base.OnItemsChanged(sender, args);
            // When the underlying items change, reset layout and children
            InvalidateMeasure();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (ItemContainerGenerator == null)
                return availableSize;
            // Ensure finite viewport
            if (double.IsInfinity(availableSize.Width) || double.IsNaN(availableSize.Width))
                availableSize.Width = Math.Max(ActualWidth, 1);
            if (double.IsInfinity(availableSize.Height) || double.IsNaN(availableSize.Height))
                availableSize.Height = Math.Max(ActualHeight, 1);

            _viewport = availableSize;

            var itemsControl = ItemsControl.GetItemsOwner(this);
            int itemCount = itemsControl?.Items.Count ?? 0;

            var effItem = EffectiveItemSize();
            if (effItem.Height <= 0 || effItem.Width <= 0)
                return availableSize;
            int itemsPerRow = Math.Max(1, (int)(availableSize.Width / effItem.Width));
            int totalRows = itemCount == 0 ? 0 : (int)Math.Ceiling((double)itemCount / itemsPerRow);

            // Extent represents the full content size
            _extent = new Size(itemsPerRow * effItem.Width, totalRows * effItem.Height);
            ScrollOwner?.InvalidateScrollInfo();

            if (itemCount == 0)
            {
                if (InternalChildren.Count > 0)
                    RemoveInternalChildRange(0, InternalChildren.Count);
                return availableSize;
            }

            // Determine visible row range based on scroll offset
            int firstVisibleRow = (int)Math.Floor(VerticalOffset / effItem.Height);
            int lastVisibleRow = (int)Math.Floor((VerticalOffset + _viewport.Height) / effItem.Height);

            const int bufferRows = 1;
            int startRow = Math.Max(0, firstVisibleRow - bufferRows);
            int endRow = Math.Min(totalRows - 1, lastVisibleRow + bufferRows);

            int startIndex = startRow * itemsPerRow;
            int endIndex = Math.Min(itemCount - 1, (endRow + 1) * itemsPerRow - 1);

            IItemContainerGenerator generator = ItemContainerGenerator;
            GeneratorPosition startPos = generator.GeneratorPositionFromIndex(startIndex);

            using (generator.StartAt(startPos, GeneratorDirection.Forward, allowStartAtRealizedItem: true))
            {
                for (int itemIndex = startIndex; itemIndex <= endIndex; itemIndex++)
                {
                    bool newlyRealized;
                    var child = generator.GenerateNext(out newlyRealized) as UIElement;

                    if (child == null)
                        continue;

                    if (newlyRealized)
                    {
                        // Always append instead of inserting at a calculated index
                        AddInternalChild(child);
                        generator.PrepareItemContainer(child);
                    }

                    SetItemIndex(child, itemIndex);
                    child.Measure(new Size(ItemWidth, ItemHeight));
                }
            }

            CleanupChildrenOutsideRange(startIndex, endIndex);

            return availableSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var effItem = EffectiveItemSize();
            int itemsPerRow = Math.Max(1, (int)(finalSize.Width / effItem.Width));

            foreach (UIElement child in InternalChildren)
            {
                int itemIndex = GetItemIndex(child);
                if (itemIndex < 0) continue;

                int row = itemIndex / itemsPerRow;
                int col = itemIndex % itemsPerRow;

                double x = col * effItem.Width - HorizontalOffset;
                double y = row * effItem.Height - VerticalOffset;

                // Account for margins inside the cell (center the content with margins)
                var rect = new Rect(
                    x + ItemMargin.Left,
                    y + ItemMargin.Top,
                    Math.Max(0, effItem.Width - (ItemMargin.Left + ItemMargin.Right)),
                    Math.Max(0, effItem.Height - (ItemMargin.Top + ItemMargin.Bottom)));

                child.Arrange(rect);
            }

            return finalSize;
        }

        private void CleanupChildrenOutsideRange(int startIndex, int endIndex)
        {
            // Remove any realized child whose index is outside [startIndex, endIndex]
            for (int i = InternalChildren.Count - 1; i >= 0; i--)
            {
                var child = InternalChildren[i];
                int ix = GetItemIndex(child);
                if (ix < startIndex || ix > endIndex)
                {
                    RemoveInternalChildRange(i, 1);
                }
            }
        }

        private Size EffectiveItemSize()
        {
            double w = ItemWidth + ItemMargin.Left + ItemMargin.Right;
            double h = ItemHeight + ItemMargin.Top + ItemMargin.Bottom;
            return new Size(Math.Max(1, w), Math.Max(1, h));
        }

        // IScrollInfo implementation
        public void LineUp() => SetVerticalOffset(VerticalOffset - 20);
        public void LineDown() => SetVerticalOffset(VerticalOffset + 20);
        public void LineLeft() => SetHorizontalOffset(HorizontalOffset - 20);
        public void LineRight() => SetHorizontalOffset(HorizontalOffset + 20);
        public void MouseWheelUp() => LineUp();
        public void MouseWheelDown() => LineDown();
        public void MouseWheelLeft() => LineLeft();
        public void MouseWheelRight() => LineRight();
        public void PageUp() => SetVerticalOffset(VerticalOffset - ViewportHeight);
        public void PageDown() => SetVerticalOffset(VerticalOffset + ViewportHeight);
        public void PageLeft() => SetHorizontalOffset(HorizontalOffset - ViewportWidth);
        public void PageRight() => SetHorizontalOffset(HorizontalOffset + ViewportWidth);

        public void SetHorizontalOffset(double offset)
        {
            if (offset < 0 || ViewportWidth >= ExtentWidth) offset = 0;
            else if (offset + ViewportWidth >= ExtentWidth) offset = ExtentWidth - ViewportWidth;

            _offset.X = offset;
            InvalidateArrange();
            ScrollOwner?.InvalidateScrollInfo();
        }

        public void SetVerticalOffset(double offset)
        {
            if (offset < 0 || ViewportHeight >= ExtentHeight) offset = 0;
            else if (offset + ViewportHeight >= ExtentHeight) offset = ExtentHeight - ViewportHeight;

            _offset.Y = offset;
            InvalidateMeasure(); // Re-measure to realize the new visible range
            ScrollOwner?.InvalidateScrollInfo();
        }

        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            if (visual is UIElement child)
            {
                int itemIndex = GetItemIndex(child);
                if (itemIndex >= 0)
                    BringIndexIntoView(itemIndex);
            }
            return rectangle;
        }

        private void BringIndexIntoView(int index)
        {
            var effItem = EffectiveItemSize();
            int itemsPerRow = Math.Max(1, (int)(_viewport.Width / effItem.Width));
            int row = index / itemsPerRow;

            double targetTop = row * effItem.Height;
            // If target is above viewport
            if (targetTop < VerticalOffset)
            {
                SetVerticalOffset(targetTop);
                return;
            }

            // If target is below viewport
            double targetBottom = targetTop + effItem.Height;
            double viewportBottom = VerticalOffset + ViewportHeight;
            if (targetBottom > viewportBottom)
            {
                SetVerticalOffset(targetBottom - ViewportHeight);
            }
        }
    }
}

