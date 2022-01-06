using System;
using System.ComponentModel;

namespace Notes.Models
{
    public interface INote : INotifyPropertyChanged
    {

        bool IsTemp { get; }
        bool IsModified { get; }
        DateTime Date { get; }
        string Name { get; set; }
        string Text { get; set; }

        void Open(string path);
        void Save();
        void Close();
    }
}
