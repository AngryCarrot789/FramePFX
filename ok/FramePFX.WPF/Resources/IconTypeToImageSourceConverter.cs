using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FramePFX.WPF.Resources {
    /// <summary>
    /// A converter for converting an <see cref="IconType"/> into an <see cref="ImageSource"/>
    /// </summary>
    [ValueConversion(typeof(IconType), typeof(ImageSource))]
    public class IconTypeToImageSourceConverter : IValueConverter {
        public static IconTypeToImageSourceConverter Instance { get; } = new IconTypeToImageSourceConverter();

        public static Dictionary<IconType, Uri> UriMap { get; } = new Dictionary<IconType, Uri>();

        static IconTypeToImageSourceConverter() {
            // Copied from MCNBTEditor, leaving here so i don't forget how to add icons lel
            // UriMap[IconType.ITEM_TAG_End]                = GetUri("Icons/FileIcon-TagEnd.png");
            // UriMap[IconType.ITEM_TAG_Byte]               = GetUri("Icons/FileIcon-TagByte8.png");
            // UriMap[IconType.ITEM_TAG_Short]              = GetUri("Icons/FileIcon-TagShort16.png");
            // UriMap[IconType.ITEM_TAG_Int]                = GetUri("Icons/FileIcon-TagInt32.png");
            // UriMap[IconType.ITEM_TAG_Long]               = GetUri("Icons/FileIcon-TagLong64.png");
            // UriMap[IconType.ITEM_TAG_Float]              = GetUri("Icons/FileIcon-TagFloat328.png");
            // UriMap[IconType.ITEM_TAG_Double]             = GetUri("Icons/FileIcon-TagDouble64.png");
            // UriMap[IconType.ITEM_TAG_String]             = GetUri("Icons/FileIcon-TagString.png");
            // UriMap[IconType.ITEM_TAG_ByteArray]          = GetUri("Icons/FileIcon-TagByteArray.png");
            // UriMap[IconType.ITEM_TAG_IntArray]           = GetUri("Icons/FileIcon-TagIntArray.png");
            // UriMap[IconType.ITEM_TAG_LongArray]          = GetUri("Icons/FileIcon-TagLongArray.png");
            // UriMap[IconType.ITEM_TAG_List]               = GetUri("Icons/icons8-bulleted-list-48.png");
            // UriMap[IconType.ITEM_TAG_Compound_Closed]    = GetUri("Icons/icons8-closed-box-48.png");
            // UriMap[IconType.ITEM_TAG_Compound_OpenFull]  = GetUri("Icons/icons8-open-box-48.png");
            // UriMap[IconType.ITEM_TAG_Compound_OpenEmpty] = GetUri("Icons/icons8-empty-box-48.png");
            // UriMap[IconType.ITEM_DATFile]                = GetUri("Icons/icons8-closed-box-48.png");
            // UriMap[IconType.ITEM_RegionFile]             = GetUri("Icons/FileIcon-Region.png");
            // UriMap[IconType.ACTION_TAG_CopyName]         = null;
            // UriMap[IconType.ACTION_TAG_CopyValue]        = null;
            // UriMap[IconType.ACTION_TAG_CopyBinary]       = GetUri("Icons/UIGeneral/icons8-copy-48.png");
            // UriMap[IconType.ACTION_TAG_PasteBinary]      = GetUri("Icons/UIGeneral/icons8-paste-48.png");
            // UriMap[IconType.ACTION_TAG_Delete]           = null;
            // UriMap[IconType.ACTION_TAG_Rename]           = null;
            // UriMap[IconType.ACTION_TAG_EditGeneral]      = GetUri("Icons/UIGeneral/icons8-edit-48.png");
            // UriMap[IconType.ACTION_ITEM_Refresh]         = GetUri("Icons/UIGeneral/icons8-sync-48.png");
        }

        private IconTypeToImageSourceConverter() {
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null || value == DependencyProperty.UnsetValue)
                return DependencyProperty.UnsetValue;

            if (!(value is IconType type))
                throw new Exception($"Expected {nameof(IconType)}, got {value}");

            return IconTypeToImageSource(type);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        private static string GetResourcePath(string fileInResources) {
            return $"/FramePFX;component/Resources/{fileInResources}";
        }

        private static Uri GetUri(string fileInResources) {
            return new Uri(GetResourcePath(fileInResources), UriKind.RelativeOrAbsolute);
        }

        public static Uri IconTypeToUri(IconType type) {
            return UriMap.TryGetValue(type, out Uri uri) ? uri : null;
        }

        public static ImageSource IconTypeToImageSource(IconType type) {
            Uri uri = IconTypeToUri(type);
            return uri == null ? null : new BitmapImage(uri);
        }
    }
}