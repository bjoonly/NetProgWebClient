using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace NetProgPractExam
{

    class ViewModel : INotifyPropertyChanged
    {

        public ViewModel()
        {

            currentFileInfo = new FileInfo() { Source = "http://212.183.159.230/50MB.zip", FolderPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) };

            downloadCommand = new DelegateCommand(Download);
            selectFolderCommand = new DelegateCommand(SelectFolder);

            moveFileCommand = new DelegateCommand(Move, () => SelectedFileInfo != null);
            renameFileCommand = new DelegateCommand(Rename,()=>SelectedFileInfo!=null && !String.IsNullOrEmpty(Name));
            deleteFileCommand = new DelegateCommand(DeleteFile, () => SelectedFileInfo != null);


            addTagCommand = new DelegateCommand(AddTag,()=>!String.IsNullOrEmpty(Tag));
            removeTagCommand = new DelegateCommand(RemoveTag,()=>!String.IsNullOrEmpty(Tag));
            searchByTagCommand = new DelegateCommand(SearchByTag);

            deleteDownloadCommand = new DelegateCommand(DeleteDownload, () => SelectedFileInfo != null);

            stopDownloadCommand = new DelegateCommand(Stop, () => SelectedFileInfo != null);

            PropertyChanged += (sender, args) =>
            {


                if (args.PropertyName == nameof(SelectedFileInfo))
                {
                    CurrentFileInfo = SelectedFileInfo.Clone();
                    renameFileCommand.RaiseCanExecuteChanged();
                    deleteFileCommand.RaiseCanExecuteChanged();
                    deleteDownloadCommand.RaiseCanExecuteChanged();
                    stopDownloadCommand.RaiseCanExecuteChanged();
                    moveFileCommand.RaiseCanExecuteChanged();
                }

                else if (args.PropertyName == nameof(Name))
                {
                    
                    renameFileCommand.RaiseCanExecuteChanged();
                }
                else if (args.PropertyName == nameof(Tag))
                {

                    addTagCommand.RaiseCanExecuteChanged();
                    removeTagCommand.RaiseCanExecuteChanged();
                 
                }
            };

           
        }

        #region Prop
        private string tag;
        private string name;
        private FileInfo selectedFileInfo;
        private FileInfo currentFileInfo;

        
        public string Tag { get { return tag; } set { if (value != tag) { tag = value; OnPropertyChanged(); } } }
        public string Name { get { return name; } set { if (value != name) { name = value; OnPropertyChanged(); } } }
        public FileInfo SelectedFileInfo { get { return selectedFileInfo; } set { if (value != selectedFileInfo) { selectedFileInfo = value; OnPropertyChanged(); } } }
        public FileInfo CurrentFileInfo { get { return currentFileInfo; } set { if (value != currentFileInfo) { currentFileInfo = value; OnPropertyChanged(); } } }

        #endregion

        private List<FileInfo> allFileInfos = new List<FileInfo>();
        private List<WebClient> webClients = new List<WebClient>();
        private readonly ICollection<FileInfo> fileInfos = new ObservableCollection<FileInfo>();

        public IEnumerable<FileInfo> FileInfos => fileInfos;


        #region Commands
        private Command downloadCommand;
        private Command selectFolderCommand;

        private Command moveFileCommand;
        private Command deleteFileCommand;
        private Command renameFileCommand;


        private Command addTagCommand;
        private Command removeTagCommand;
        private Command searchByTagCommand;

        private Command deleteDownloadCommand;

        private Command stopDownloadCommand;

        public ICommand DownloadCommand => downloadCommand;
        public ICommand SelectFolderCommand => selectFolderCommand;

        public ICommand MoveFileCommand => moveFileCommand;
        public ICommand DeleteFileCommand => deleteFileCommand;
        public ICommand RenameFileCommand => renameFileCommand;

        public ICommand AddTagCommand => addTagCommand;
        public ICommand RemoveTagCommand => removeTagCommand;
        public ICommand SearchByTagCommand => searchByTagCommand;

        public ICommand DeleteDownloadCommand => deleteDownloadCommand;
        public ICommand StopDownloadCommand => stopDownloadCommand;
        #endregion

        public string FreePath(string path)
        {
            int index = 0;
            string tmp;
            do
            {
                tmp = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + (index == 0 ? null : $"({index})") + Path.GetExtension(path));
                index++;
            } while (File.Exists(tmp));
            return tmp;
        }
        public void SelectFolder()
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                CurrentFileInfo.FolderPath = fbd.SelectedPath;

            }
        }

        public void DeleteDownload()
        {
            if (SelectedFileInfo != null && SelectedFileInfo.Status != Statuses.DOWNLOADING)
            {
                fileInfos.Remove(SelectedFileInfo);
                allFileInfos.Remove(SelectedFileInfo);
            }
        }
        public void AddTag()
        {
            if (!String.IsNullOrEmpty(Tag))
                currentFileInfo.AddTag(Tag);
        }
        public void RemoveTag()
        {
            if (!String.IsNullOrEmpty(Tag))
                currentFileInfo.RemoveTag(Tag);
        }

        public void SearchByTag()
        {
            fileInfos.Clear();
            if (String.IsNullOrEmpty(Tag))
            {
                foreach (var item in allFileInfos)
                {
                    fileInfos.Add(item);
                }
            }
            else
            {
                foreach (var item in allFileInfos)
                {
                    if (item.Tags.Contains(Tag))
                        fileInfos.Add(item);
                }
            }
        }
        public void Move()
        {
            if (SelectedFileInfo != null && SelectedFileInfo.Status == Statuses.COMPLETED)
            {
                string res = FreePath(Path.Combine(currentFileInfo.FolderPath, selectedFileInfo.Name));
                File.Move(Path.Combine(selectedFileInfo.FolderPath, selectedFileInfo.Name), res);
                SelectedFileInfo.Name = Path.GetFileName(res);
                SelectedFileInfo.FolderPath = Path.GetDirectoryName(res);
            }
        }
        public void Rename()
        {
            if (SelectedFileInfo != null && SelectedFileInfo.Status == Statuses.COMPLETED)
            {
                System.IO.FileInfo fi = new System.IO.FileInfo(Path.Combine(selectedFileInfo.FolderPath, selectedFileInfo.Name));
                string path = Path.Combine(selectedFileInfo.FolderPath, name + Path.GetExtension(selectedFileInfo.Name));
                path = FreePath(path);
                if (!File.Exists(path) && File.Exists(Path.Combine(selectedFileInfo.FolderPath, selectedFileInfo.Name)))
                {
                  
                    fi.MoveTo(Path.Combine(selectedFileInfo.FolderPath, Path.GetFileNameWithoutExtension(path) + Path.GetExtension(path)));
                }
                SelectedFileInfo.Name = Path.GetFileName(path);
                Name = String.Empty;
            }
        }
        public void DeleteFile()
        {
            if (SelectedFileInfo.Status == Statuses.COMPLETED)
            {
                File.Delete(Path.Combine(selectedFileInfo.FolderPath, selectedFileInfo.Name));
                SelectedFileInfo.Status = Statuses.DELETED;

            }
        }
        public void Stop()
        {
            SelectedFileInfo.IsDownloading = false;
            SelectedFileInfo.Status = Statuses.STOPED;
        }
        public void Download()
        {

            FileInfo fi = currentFileInfo.Clone();
            string tmp;
            tmp = Path.Combine(fi.FolderPath, Path.GetFileName(fi.Source));


            fi.Name = Path.GetFileName(FreePath(tmp));
            fi.Status = Statuses.DOWNLOADING;
            allFileInfos.Add(fi);
            fileInfos.Add(fi);
            fi.IsDownloading = true;
            Task.Run(() => DownloadFile(fi));

            
            CurrentFileInfo.Name = String.Empty;
            CurrentFileInfo.Progress = 0;
            CurrentFileInfo.Size = 0;
            CurrentFileInfo.ClearTags();
            CurrentFileInfo.IsDownloading = true;

            Tag = String.Empty;

        }
        public async void DownloadFile(FileInfo info)
        {
            try
            {
                using (WebClient client = new WebClient())
                {

                    client.DownloadProgressChanged += (o, e) =>
                    {
                        if (!info.IsDownloading)
                        {
                            client.CancelAsync();

                            return;
                        }

                        info.Progress = e.ProgressPercentage;
                        info.BytesProgress = e.BytesReceived / 1024/1024 ;
                        info.Size = e.TotalBytesToReceive / 1024 / 1024;

                    };
                    webClients.Add(client);

                    client.DownloadFileCompleted += (o, e) =>
                    {
                        webClients.Remove(client);
                        if (!e.Cancelled)
                        {
                            
                        info.Status = Statuses.COMPLETED;
                        
                        }
                        
                    };

                    await client.DownloadFileTaskAsync(new Uri(info.Source), Path.Combine(info.FolderPath, info.Name));
                }
            }
            catch { }

        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
