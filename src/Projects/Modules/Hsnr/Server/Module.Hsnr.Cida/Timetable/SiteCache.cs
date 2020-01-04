using System.Collections.Generic;
using HtmlAgilityPack;
using Module.Hsnr.Timetable.Data;

namespace Module.Hsnr.Timetable
{
    public class SiteCache
    {
        private readonly TimetableConnector connector;
        
        public Dictionary<string, string> Lecturer { get; } = new Dictionary<string, string>();
        
        public Dictionary<string, string> BranchesOfStudy { get; } = new Dictionary<string, string>();
        
        public Dictionary<string, string> Rooms { get; } = new Dictionary<string, string>();

        public SiteCache(TimetableConnector connector)
        {
            this.connector = connector;
            this.Refresh();
        }

        public void Refresh()
        {
            this.LoadLecturers();
            this.LoadBranchOfStudies();
            this.LoadRooms();
        }

        private void LoadRooms()
        {
            this.Rooms.Clear();
            var formData = new FormData()
            {
                Calendar = CalendarType.Room,
            };

            var result = this.connector.PostDataAsync(formData).GetAwaiter().GetResult();
            var document = new HtmlDocument();
            document.LoadHtml(result);
            var element = document.GetElementbyId("select_R");
            foreach (var child in element.ChildNodes)
            {
                this.Rooms.Add(child.GetAttributeValue("value", string.Empty), child.InnerHtml);
            }
        }

        private void LoadBranchOfStudies()
        {
            this.BranchesOfStudy.Clear();
            var formData = new FormData()
            {
                Calendar = CalendarType.BranchOfStudy,
            };

            var result = this.connector.PostDataAsync(formData).GetAwaiter().GetResult();
            var document = new HtmlDocument();
            document.LoadHtml(result);
            var element = document.GetElementbyId("select_S");
            foreach (var child in element.ChildNodes)
            {
                this.BranchesOfStudy.Add(child.GetAttributeValue("value", string.Empty), child.InnerHtml);
            }
        }

        private void LoadLecturers()
        {
            this.Lecturer.Clear();
            var formData = new FormData()
            {
                Calendar = CalendarType.Lecturer,
            };

            var result = this.connector.PostDataAsync(formData).GetAwaiter().GetResult();
            var document = new HtmlDocument();
            document.LoadHtml(result);
            var element = document.GetElementbyId("select_D");
            foreach (var child in element.ChildNodes)
            {
                this.Lecturer.Add(child.GetAttributeValue("value", string.Empty), child.InnerHtml);
            }
        }
    }
}