using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NetProgPractExam
{

    enum Statuses
    {
        NONE,
        DOWNLOADING,
        STOPED,
        COMPLETED,
        DELETED

    }
    class FileInfo : INotifyPropertyChanged
    {
        private string name;
        private string source;
        private string folderPath;
        private float progress;
        private float bytesProgress;
        private float size;
        private Statuses status;
        private bool isDownloading;

        public string Name { get { return name; } set { if (value != name) { name = value; OnPropertyChanged(); } } }
        public string Source { get { return source; } set { if (value != source) { source = value; OnPropertyChanged(); } } }
        public string FolderPath { get { return folderPath; } set { if (value != folderPath) { folderPath = value; OnPropertyChanged(); } } }
        public float Progress { get { return progress; } set { if (value > 100) value = 100; if (value != progress) { progress = value; OnPropertyChanged(); } } }
        public float BytesProgress { get { return bytesProgress; } set {  if (value != bytesProgress) { bytesProgress = value; OnPropertyChanged(); } } }

        public float Size { get { return size; } set { if (value != size) { size = value; OnPropertyChanged(); } } }
        public Statuses Status { get { return status; } set { if (value != status) { status = value; OnPropertyChanged(); } } }
        public bool IsDownloading { get { return isDownloading; } set { if (value != isDownloading) { isDownloading = value; OnPropertyChanged(); } } }



        private readonly ICollection<string> tags = new ObservableCollection<string>();
        public IEnumerable<string> Tags => tags;


        public void ClearTags()
        {
            tags.Clear();
        }
        public void AddTag(string tag)
        {
            bool res = false;
            foreach (var item in tags)
            {
                if (item == tag)
                {
                    res = true;
                    break;
                }

            }
            if (!res)
                tags.Add(tag);
        }
        public void RemoveTag(string tag)
        {
            tags.Remove(tag);
        }
        public FileInfo Clone()
        {
            FileInfo copy = new FileInfo() { source = source, folderPath = folderPath, progress = progress, size = size, bytesProgress=bytesProgress };
            foreach (var item in tags)
            {
                copy.tags.Add(item);
            }
            return copy;
        }
        

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
