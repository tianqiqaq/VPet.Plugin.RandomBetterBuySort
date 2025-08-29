using System.Reflection;
using System.Windows;
using System.Windows.Media;

namespace VPet.Plugin.RandomBetterBuySort;

public static class WindowsControlHelper
{
    public static T? TryGetControl<T>(DependencyObject parent, string name) where T : DependencyObject
    {
        if (parent == null) return null;

        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);

            if (child is T target && ((FrameworkElement)child).Name == name)
            {
                return (T)child;
            }

            var result = TryGetControl<T>(child, name);
            if (result != null) return result;
        }

        return null;
    }

    public static T? TryGetPrivateField<T>(Window window, string fieldName)
    {
        try
        {
            var field = window.GetType().GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance);

            if (field != null && field.FieldType == typeof(T))
            {
                var value = field.GetValue(window);
                if (value != null)
                {
                    return (T)value;
                }
            }
        }
        catch (Exception ex)
        {
            // 处理异常
            Console.WriteLine($"获取 {fieldName} 失败: {ex.Message}");
        }

        return default;
    }
}