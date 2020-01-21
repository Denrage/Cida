using Grpc.Core;
using Hsnr;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Module.Hsnr.Wpf
{
    public partial class TimetableView : Window, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion

        #region bindables

        private SemesterType _semester = SemesterType.SummerSemester; //Set SemesterType according to current Semester?
        public SemesterType Semester
        {
            get => _semester;
            set
            {
                if (_semester == value)
                    return;

                _semester = value;
                UpdateTimetable();
                OnPropertyChanged(nameof(Semester));
            }
        }

        private CalendarType _calendar = CalendarType.BranchOfStudy;
        public CalendarType Calendar
        {
            get => _calendar;
            set
            {
                if (_calendar == value)
                    return;

                _calendar = value;
                UpdateTimetable();
                OnPropertyChanged(nameof(Calendar));
            }
        }

        private string _branchOfStudy = "KB15";
        public string BranchOfStudy
        {
            get => _branchOfStudy;
            set
            {
                if (_branchOfStudy == value)
                    return;

                _branchOfStudy = value;
                UpdateTimetable();
                OnPropertyChanged(nameof(BranchOfStudy));
            }
        }

        private string _lecturer = "Re";
        public string Lecturer
        {
            get => _lecturer;
            set
            {
                if (_lecturer == value)
                    return;

                _lecturer = value;
                UpdateTimetable();
                OnPropertyChanged(nameof(Lecturer));
            }
        }

        private string _room = "B111";
        public string Room
        {
            get => _room;
            set
            {
                if (_room == value)
                    return;

                _room = value;
                UpdateTimetable();
                OnPropertyChanged(nameof(Room));
            }
        }

        private ObservableCollection<string> _branchesOfStudy = new ObservableCollection<string> { "KB15" };
        public ObservableCollection<string> BranchesOfStudy
        {
            get => _branchesOfStudy;
            set
            {
                if (_branchesOfStudy == value)
                    return;

                _branchesOfStudy = value;
                OnPropertyChanged(nameof(BranchesOfStudy));
            }
        }

        private ObservableCollection<string> _rooms = new ObservableCollection<string> { "B111" };
        public ObservableCollection<string> Rooms
        {
            get => _rooms;
            set
            {
                if (_rooms == value)
                    return;

                _rooms = value;
                OnPropertyChanged(nameof(Rooms));
            }
        }

        private ObservableCollection<string> _lecturers = new ObservableCollection<string> { "Re" };
        public ObservableCollection<string> Lecturers
        {
            get => _lecturers;
            set
            {
                if (_lecturers == value)
                    return;

                _lecturers = value;
                OnPropertyChanged(nameof(Lecturers));
            }
        }

        #endregion

        private HsnrTimetableService.HsnrTimetableServiceClient _client;

        public TimetableView()
        {
            InitializeComponent();

            var channel = new Channel("ipv4:127.0.0.1:31564,127.0.0.2:31564", ChannelCredentials.Insecure, new[] { new ChannelOption("grpc.lb_policy_name", "round_robin") });
            _client = new HsnrTimetableService.HsnrTimetableServiceClient(channel);

            ResetTimetable();
            UpdateTimetable();
        }

        public void ResetTimetable()
        {
            TimetableGrid.Children.Clear();
            TimetableGrid.RowDefinitions.Clear();
            TimetableGrid.ColumnDefinitions.Clear();

            TimetableGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(20) });
            TimetableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            for (int i = 0; i < 13; i++)
            {
                TimetableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
                var timeTxt = new TextBlock();
                timeTxt.Text = $"{i + 8}-{i + 9}";
                Grid.SetRow(timeTxt, 0);
                Grid.SetColumn(timeTxt, i + 1);
                TimetableGrid.Children.Add(timeTxt);
            }
        }

        public void UpdateTimetable()
        {
            TimetableResponse timetable;
            try
            {
                timetable = _client.Timetable(new TimetableRequest()
                {
                    Semester = _semester,
                    Calendar = _calendar,
                    BranchOfStudy = BranchOfStudy,
                    Room = Room,
                    Lecturer = Lecturer,
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            ResetTimetable();

            foreach (var day in timetable.Result.WeekDays)
            {
                AddSeperator();

                int rowStart = TimetableGrid.RowDefinitions.Count;
                LayoutTimetable(day.Subjects.ToList());
                int rowEnd = TimetableGrid.RowDefinitions.Count;

                var dayTxt = new TextBlock();
                dayTxt.Text = Enum.GetName(typeof(TimetableResponse.Types.Timetable.Types.WeekDay.Types.Days), day.Day);
                Grid.SetRow(dayTxt, rowStart);
                Grid.SetRowSpan(dayTxt, rowEnd - rowStart);
                Grid.SetColumn(dayTxt, 0);
                TimetableGrid.Children.Add(dayTxt);
            }
        }

        public void AddSeperator()
        {
            TimetableGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(5) }); //Seperator
            var rect = new Rectangle();
            rect.Fill = new SolidColorBrush(Colors.Black);
            Grid.SetRow(rect, TimetableGrid.RowDefinitions.Count - 1);
            Grid.SetColumn(rect, 0);
            Grid.SetColumnSpan(rect, TimetableGrid.ColumnDefinitions.Count);
            TimetableGrid.Children.Add(rect);
        }

        public void LayoutTimetable(List<TimetableResponse.Types.Timetable.Types.WeekDay.Types.Subject> subjects)
        {
            TimetableGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(30) });

            if (subjects.Any())
                return;

            for (int i = 8; i <= 22;)
            {
                var subject = subjects.FirstOrDefault(x => x.Start == i);
                if (subject != null)
                {
                    var subjectView = new SubjectView(subject);
                    Grid.SetRow(subjectView, TimetableGrid.RowDefinitions.Count - 1);
                    Grid.SetColumn(subjectView, subject.Start - (8 - 1));
                    Grid.SetColumnSpan(subjectView, subject.End - subject.Start);
                    TimetableGrid.Children.Add(subjectView);

                    i = subject.End;
                    subjects.Remove(subject);
                }
                else
                {
                    i++;
                }
            }

            if (subjects.Any())
                LayoutTimetable(subjects);
        }

        public void Refresh(object sender, RoutedEventArgs e)
        {
            UpdateTimetable();
        }
    }
}
