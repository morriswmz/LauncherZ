using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LauncherZ.Controls
{
    /// <summary>
    /// A TextBox that exposes selection range as a dependency property.
    /// </summary>
    public class SelectionRangeAwareTextBox : TextBox
    {

        private bool _settingSelectionRange = false;

        static SelectionRangeAwareTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SelectionRangeAwareTextBox), new FrameworkPropertyMetadata(typeof(SelectionRangeAwareTextBox)));
        }

        public SelectionRangeAwareTextBox()
        {
            SelectionChanged += SelectionRangeAwareTextBox_SelectionChanged;
        }
        
        public SelectionRange SelectionRange
        {
            get { return (SelectionRange)GetValue(SelectionRangeProperty); }
            set { SetValue(SelectionRangeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectionRangePropertyRange.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectionRangeProperty =
            DependencyProperty.Register("SelectionRange", typeof (SelectionRange),
                typeof (SelectionRangeAwareTextBox),
                new FrameworkPropertyMetadata(
                    new SelectionRange(0, 0), FrameworkPropertyMetadataOptions.Journal,
                    SelectionRangeChangedCallback, null, true
                    ), SelectionRangeValidationCallback
                );

        private static bool SelectionRangeValidationCallback(object value)
        {
            var sr = (SelectionRange) value;
            return sr.Start >= 0;
        }

        private static void SelectionRangeChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tb = (SelectionRangeAwareTextBox) d;
            tb._settingSelectionRange = true;
            var sr = (SelectionRange) e.NewValue;
            // no need to update selection when range is not changed.
            // this happens when SelectionRange property is set by SelectionChanged event handlers,
            // and it is redundant to update selection in this case.
            if (sr.Start != tb.SelectionStart || sr.Length != tb.SelectionLength)
                tb.Select(sr.Start, sr.Length);
            tb._settingSelectionRange = false;
        }

        private void SelectionRangeAwareTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            // ignore changes raised by setting SelectionRange property
            if (_settingSelectionRange)
                return;
            SelectionRange = new SelectionRange(SelectionStart, SelectionLength);
        }
    }

    [TypeConverter(typeof(SelectionRangeConvertor))]
    public struct SelectionRange
    {
        public int Start { get; set; }
        public int Length { get; set; }

        public SelectionRange(int start, int length) : this()
        {
            Start = start;
            Length = length;
        }

        public bool Equals(SelectionRange other)
        {
            return Start == other.Start && Length == other.Length;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is SelectionRange && Equals((SelectionRange) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Start*397) ^ Length;
            }
        }

        public static bool operator ==(SelectionRange left, SelectionRange right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SelectionRange left, SelectionRange right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return string.Format("{0},{1}", Start, Length);
        }
    }

    public class SelectionRangeConvertor : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof (SelectionRange)
                   || destinationType == typeof (InstanceDescriptor)
                   || base.CanConvertTo(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var s = value as string;
            if (s != null)
            {
                string[] parts = s.Split(',');
                if (parts.Length == 2)
                {
                    return new SelectionRange(int.Parse(parts[0]), int.Parse(parts[1]));
                }
                throw GetConvertFromException(value);
            }
            
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value == null)
                throw new ArgumentNullException("value");
            if (destinationType == null)
                throw new ArgumentNullException("destinationType");

            var sr = (SelectionRange) value;
            if (destinationType == typeof (InstanceDescriptor))
            {
                ConstructorInfo ci = typeof (SelectionRange).GetConstructor(new Type[] {typeof (int), typeof (int)});
                return new InstanceDescriptor(ci, new object[] {sr.Start, sr.Length});
            }
            if (destinationType == typeof (string))
            {
                return sr.ToString();
            }
            throw GetConvertToException(value, destinationType);
        }
    }
}
