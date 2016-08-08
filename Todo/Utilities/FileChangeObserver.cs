using Splat;
using System;
using System.IO;
using System.Threading;

namespace Seemon.Todo.Utilities
{
    class FileChangeObserver : IDisposable, IEnableLogger
    {
        private UserSettings settings = null;
        private string fileName = string.Empty;
        private FileSystemWatcher watcher = null;

        public delegate void FileChanged();
        public event FileChanged OnFileChanged;

        public FileChangeObserver()
        {
            this.settings = Locator.Current.GetService<UserSettings>();
        }

        public void ObserveFile(string filename)
        {
            if(!this.settings.AutomaticallyRefreshTaskListFromFile)
            {
                if(this.watcher != null)
                {
                    this.watcher.EnableRaisingEvents = false;
                    this.watcher.Dispose();
                    this.watcher = null;
                    this.fileName = string.Empty;
                }
                return;
            }

            if (this.fileName != filename)
            {
                this.fileName = filename;
                if (this.watcher != null)
                    this.watcher.Dispose();

                this.watcher = new FileSystemWatcher();
                this.watcher.Path = Path.GetDirectoryName(filename);
                this.watcher.Filter = Path.GetFileName(filename);
                this.watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size;

                this.watcher.Changed += OnChanged;
                this.watcher.EnableRaisingEvents = true;
            }
            else if (!this.settings.AutomaticallyRefreshTaskListFromFile && this.watcher == null)
                this.watcher.EnableRaisingEvents = false;
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            this.Log().Debug("File Changed");
            Thread.Sleep(1000);

            if (this.OnFileChanged != null)
                this.OnFileChanged();
        }

        public void Dispose()
        {
            if(this.watcher != null)
            {
                this.watcher.Dispose();
                this.fileName = string.Empty;
            }
        }
    }
}
