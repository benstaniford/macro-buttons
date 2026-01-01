using MacroButtons.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MacroButtons.Views;

/// <summary>
/// Interaction logic for ConfigEditorWindow.xaml
/// </summary>
public partial class ConfigEditorWindow : Window
{
    private Point _dragStartPoint;
    private bool _isDragging;
    private ButtonTileEditorViewModel? _draggedTile;

    public ConfigEditorWindow(ConfigEditorViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void Tile_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _dragStartPoint = e.GetPosition(null);
        _isDragging = false;
        
        if (sender is Border border && border.DataContext is ButtonTileEditorViewModel tile)
        {
            _draggedTile = tile;
        }
    }

    private void Tile_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed && !_isDragging && _draggedTile != null)
        {
            Point currentPosition = e.GetPosition(null);
            Vector diff = _dragStartPoint - currentPosition;

            // Start drag if moved enough distance
            if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                // Don't allow dragging the BACK button
                if (_draggedTile.IsBackButton)
                    return;

                _isDragging = true;
                _draggedTile.IsDragging = true;

                DataObject dragData = new DataObject("TileData", _draggedTile);
                DragDrop.DoDragDrop((DependencyObject)sender, dragData, DragDropEffects.Move);
                
                _draggedTile.IsDragging = false;
                _isDragging = false;
                _draggedTile = null;
            }
        }
    }

    private void Tile_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        _isDragging = false;
        if (_draggedTile != null)
        {
            _draggedTile.IsDragging = false;
            _draggedTile = null;
        }
    }

    private void Tile_Drop(object sender, DragEventArgs e)
    {
        if (sender is Border border)
        {
            // Remove drop indicator
            var dropIndicator = FindVisualChild<Border>(border, "DropIndicator");
            if (dropIndicator != null)
            {
                dropIndicator.BorderThickness = new Thickness(0);
            }

            if (border.DataContext is ButtonTileEditorViewModel targetTile && 
                e.Data.GetData("TileData") is ButtonTileEditorViewModel sourceTile &&
                DataContext is ConfigEditorViewModel viewModel)
            {
                viewModel.MoveTile(sourceTile, targetTile);
            }
        }
    }

    private void Tile_DragOver(object sender, DragEventArgs e)
    {
        if (sender is Border border && 
            border.DataContext is ButtonTileEditorViewModel targetTile &&
            !targetTile.IsBackButton)
        {
            // Show drop indicator
            var dropIndicator = FindVisualChild<Border>(border, "DropIndicator");
            if (dropIndicator != null)
            {
                dropIndicator.BorderThickness = new Thickness(3);
            }

            e.Effects = DragDropEffects.Move;
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
        e.Handled = true;
    }

    private void Tile_DragLeave(object sender, DragEventArgs e)
    {
        if (sender is Border border)
        {
            // Remove drop indicator
            var dropIndicator = FindVisualChild<Border>(border, "DropIndicator");
            if (dropIndicator != null)
            {
                dropIndicator.BorderThickness = new Thickness(0);
            }
        }
    }

    // Helper method to find child elements by name
    private T? FindVisualChild<T>(DependencyObject parent, string name) where T : FrameworkElement
    {
        for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
            
            if (child is T typedChild && typedChild.Name == name)
            {
                return typedChild;
            }

            var result = FindVisualChild<T>(child, name);
            if (result != null)
                return result;
        }
        return null;
    }
}
