using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection.Emit;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace MyFileWpfFileExplorer
{
    [ValueConversion(typeof(DirectoryItemType), typeof(BitmapSource))]
    //public class FileToGetIconConverter : IMultiValueConverter
    //{
    //    //public static FileToGetIconConverter Instance = new FileToGetIconConverter();
    //    public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        if (value == null) return null;
    //        var file = (DirectoryItem)value[0];
    //        if (file.Type == DirectoryItemType.Drive)
    //        {
    //            var icon = "Images/HardDrive.png";
    //            return new BitmapImage(new Uri($"pack://application:,,,/{icon}"));
    //        }
    //        else if (file.Type == DirectoryItemType.Folder)
    //        {
    //            var icon = file.IsExpanded? "Images/folder-open.png": "Images/folder-closed.png";
    //            return new BitmapImage(new Uri($"pack://application:,,,/{icon}"));
    //        }
    //        else
    //        {
    //            var sysicon = Icon.ExtractAssociatedIcon(file.FullPath);
    //            var image = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
    //                  sysicon.Handle,
    //                  System.Windows.Int32Rect.Empty,
    //                  BitmapSizeOptions.FromEmptyOptions());
    //            sysicon.Dispose();
    //            return image;
    //        }
    //    }

    //    public object[] ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    public class FileToGetIconConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null) return null;
            var type = (DirectoryItemType)values[0];
            var isExpended = (bool)values[1];
            var fullPath = (string)values[2];
            if (type == DirectoryItemType.Drive)
            {
                var icon = "Images/HardDrive.png";
                return new BitmapImage(new Uri($"pack://application:,,,/{icon}"));
            }
            else if (type == DirectoryItemType.Folder)
            {
                var icon = isExpended ? "Images/folder-open.png" : "Images/folder-closed.png";
                return new BitmapImage(new Uri($"pack://application:,,,/{icon}"));
            }
            else
            {
                var sysicon = Icon.ExtractAssociatedIcon(fullPath);
                var image = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                      sysicon.Handle,
                      System.Windows.Int32Rect.Empty,
                      BitmapSizeOptions.FromEmptyOptions());
                sysicon.Dispose();
                return image;
            }


         
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

