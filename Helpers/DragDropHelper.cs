using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WpfListBox = System.Windows.Controls.ListBox;
using WpfPoint = System.Windows.Point;
using WpfDataObject = System.Windows.DataObject;
using WpfDragDrop = System.Windows.DragDrop;
using WpfDragDropEffects = System.Windows.DragDropEffects;
using WpfDataFormats = System.Windows.DataFormats;

namespace EasyTaskStarter.Helpers
{
    /// <summary>
    /// ListBox için sürükle-bırak sıralama desteği.
    /// Kullanım: DragDropHelper.Attach(listBox, onReordered);
    /// </summary>
    public static class DragDropHelper
    {
        private const string DragFormat = "EasyTaskStarter_DragItem";

        public static void Attach(
            WpfListBox listBox,
            System.Action onReordered,
            System.Action<string[]>? onFilesDropped = null)
        {
            WpfPoint _dragStart = default;
            object? _dragItem = null;
            bool _isDragging = false;

            listBox.PreviewMouseLeftButtonDown += (s, e) =>
            {
                _dragStart = e.GetPosition(null);
                _dragItem = GetItemUnderMouse(listBox, e.GetPosition(listBox));
                _isDragging = false;
            };

            listBox.PreviewMouseMove += (s, e) =>
            {
                if (e.LeftButton != MouseButtonState.Pressed || _dragItem == null) return;

                var pos = e.GetPosition(null);
                var diff = _dragStart - pos;

                if (!_isDragging &&
                    (System.Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                     System.Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
                {
                    _isDragging = true;
                    var data = new WpfDataObject(DragFormat, _dragItem);
                    WpfDragDrop.DoDragDrop(listBox, data, WpfDragDropEffects.Move);
                    _isDragging = false;
                    _dragItem = null;
                }
            };

            listBox.PreviewMouseLeftButtonUp += (s, e) =>
            {
                _dragItem = null;
                _isDragging = false;
            };

            listBox.DragOver += (s, e) =>
            {
                if (e.Data.GetDataPresent(WpfDataFormats.FileDrop))
                {
                    e.Effects = WpfDragDropEffects.Copy;
                    e.Handled = true;
                }
                else if (e.Data.GetDataPresent(DragFormat))
                {
                    e.Effects = WpfDragDropEffects.Move;
                    e.Handled = true;
                }
                else
                {
                    e.Effects = WpfDragDropEffects.None;
                }
            };

            listBox.Drop += (s, e) =>
            {
                // Dışarıdan dosya/klasör bırakıldıysa
                if (e.Data.GetDataPresent(WpfDataFormats.FileDrop))
                {
                    if (e.Data.GetData(WpfDataFormats.FileDrop) is string[] files && files.Length > 0)
                    {
                        onFilesDropped?.Invoke(files);
                        e.Handled = true;
                        return;
                    }
                }

                // Dahili sıralama sürüklemesi
                if (!e.Data.GetDataPresent(DragFormat)) return;

                var dragged = e.Data.GetData(DragFormat);
                var target = GetItemUnderMouse(listBox, e.GetPosition(listBox));

                if (dragged == null || target == null || dragged == target) return;

                // ItemsSource bir IList olmalı
                if (listBox.ItemsSource is System.Collections.IList list)
                {
                    int fromIndex = list.IndexOf(dragged);
                    int toIndex = list.IndexOf(target);
                    if (fromIndex < 0 || toIndex < 0) return;

                    list.RemoveAt(fromIndex);
                    list.Insert(toIndex, dragged);

                    listBox.ItemsSource = null;
                    listBox.ItemsSource = list;
                    listBox.SelectedItem = dragged;

                    onReordered();
                }

                e.Handled = true;
            };

            // Drop'un çalışması için AllowDrop gerekli
            listBox.AllowDrop = true;
        }

        private static object? GetItemUnderMouse(WpfListBox listBox, WpfPoint position)
        {
            var element = listBox.InputHitTest(position) as DependencyObject;
            while (element != null)
            {
                if (element is ListBoxItem lbi)
                    return lbi.DataContext;
                element = VisualTreeHelper.GetParent(element);
            }
            return null;
        }
    }
}