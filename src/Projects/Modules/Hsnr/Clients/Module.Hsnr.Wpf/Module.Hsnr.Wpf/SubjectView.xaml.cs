using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Hsnr;
using static Hsnr.TimetableResponse.Types.Timetable.Types.WeekDay.Types;

namespace Module.Hsnr.Wpf
{
    /// <summary>
    /// Interaction logic for SubjectView.xaml
    /// </summary>
    public partial class SubjectView : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        private Subject _subject;
        public Subject Subject
        {
            get => _subject;
            set
            {
                if (_subject == value)
                    return;

                _subject = value;
                OnPropertyChanged(nameof(Subject));
            }
        }


        public SubjectView(Subject subject)
        {
            InitializeComponent();
            this.Subject = subject;
        }

    }
}
