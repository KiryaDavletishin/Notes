using System;
using System.ComponentModel;
using System.IO;

namespace Notes.Models
{
    public class Note : INote, IDisposable, IEquatable<Note>
    {
        public static string NewFileName { get; set; } = "Новая заметка";
        
        private FileStream _fstream;
        private string _content;
        private bool _isTemp, _isModified = false;

        //При открытии файла
        public Note(string path)
        {
            Open(path);
        }
        //При создании нового
        public Note()
        {
            Name = NewFileName;
        }
        ~Note()
        {
            Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsTemp
        {
            get => _isTemp;
            private set
            {
                _isTemp = value;
                OnPropertyChanged("IsTemp");
            }
        }
        public bool IsModified
        {
            get => _isModified;
            private set
            {
                _isModified = value;
                OnPropertyChanged("IsModified");
            }
        }
        public DateTime Date
        {
            get => File.GetLastAccessTime(_fstream.Name);
        }
        public string Name
        {
            get
            {
                if (_fstream is null)
                    throw new FileNotFoundException("Файл отсутствует!");

                return Path.GetFileNameWithoutExtension(_fstream.Name);
            }
            set
            {
                string path = value,
                    extension = ".note";

                if (_fstream is null)
                {
                    path = Path.Combine(Path.GetTempPath(), value + extension);
                    IsTemp = true;
                }
                else
                {
                    _fstream.Close();

                    if (!Path.IsPathRooted(value))
                    {
                        path = Path.Combine(Path.GetDirectoryName(_fstream.Name), value + extension);
                    }
                    else
                        IsTemp = false;
                }

                ChangeFile(path);
                OnPropertyChanged("Name");
            }
        }
        public string Text {
            set
            {
                IsModified = true;
                _content = value;
                OnPropertyChanged("Text");
            }
            get
            {
                if (_content is null)
                {
                    if (_fstream is null)
                        return string.Empty;

                    if (!_isModified)
                        Read();
                }

                return _content;
            }
        }

        public void Open(string path)
        {
            _fstream = File.Open(path, FileMode.Open, FileAccess.ReadWrite);
        }
        public void Save()
        {
            IsModified = false;
            ClearFile();
            var writer = new StreamWriter(_fstream);
            writer.Write(_content);
            writer.Flush();
            _fstream.Flush();
            _fstream.Position = 0;
        }
        public void Close()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            _fstream.Dispose();

            if (_isTemp)
            {
                File.Delete(_fstream.Name);
            }
        }

        public bool Equals(Note other)
        {
            return this.GetHashCode() == other.GetHashCode();
        }
        public override int GetHashCode()
        {
            return this._fstream.Name.GetHashCode();
        }

        private void Read()
        {
            var reader = new StreamReader(_fstream);
            _content = reader.ReadToEnd();
            _fstream.Position = 0;
        }

        private void OnPropertyChanged(string status)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(status));
            }
        }

        private void ChangeFile(string path)
        {
            if (File.Exists(path))
            {
                string fileName = Path.GetFileNameWithoutExtension(path);
                string directory = Path.GetDirectoryName(path);
                int fileIndex = 1;

                do
                {
                    string tempName = fileName + $"({fileIndex++})";
                    path = Path.Combine(directory, tempName + ".note");
                }
                while (File.Exists(path));
            }

            if (!(_fstream is null))
                File.Move(_fstream.Name, path);

            _fstream = File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        }

        private void ClearFile()
        {
            _fstream.Close();
            _fstream = File.Open(_fstream.Name, FileMode.Create, FileAccess.ReadWrite);
        }
    }
}
