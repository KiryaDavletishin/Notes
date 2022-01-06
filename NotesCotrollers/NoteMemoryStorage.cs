using Notes.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Notes.Controllers
{
    public class NoteMemoryStorage : ObservableCollection<INote>
    {
        
        public void Add(string path)
        {
            var note = new Note(path);

            Add(note);
        }
        public void AddRange(string[] paths)
        {
            foreach(var path in paths)
            {
                Add(path);
            }
        }
        public bool IsMatchFilter(INote note, string filter)
        {
            string pattern = $@"\w*{Regex.Escape(filter)}\w*";
            var regex = new Regex(pattern);
            return regex.IsMatch(note.Name);
        }

        public void RemoveAll()
        {
            while (Count != 0)
                Remove(this.Last());
        }

        public void SaveNew(INote note, string newPath)
        {
            note.Name = newPath;
            note.Save();
        }
        public void Save(INote note)
        {
            note.Save();
        }
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if(e.Action == NotifyCollectionChangedAction.Remove)
            {
                for (int i = 0; i < e.OldItems.Count; i++)
                {
                    var deleted = e.OldItems[i] as INote;
                    deleted.Close();
                }
            }

            base.OnCollectionChanged(e);
        }
    }
}
