using Microsoft.Win32;
using Notes.Controllers;
using Notes.Models;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Notes
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NoteMemoryStorage _storage;

        public MainWindow()
        {
            _storage = (App.Current as App).Collection;
            InitializeComponent();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var source = Resources["ListingDataView"] as CollectionViewSource;

            if (SearchTextBox.Text == "")
                source.View.Filter = null;
            else
                source.View.Filter += (obj) =>
                {
                    var note = obj as INote;
                    return _storage.IsMatchFilter(note, SearchTextBox.Text);
                };
        }
        private void LangChanged(object sender, RoutedEventArgs e)
        {
            var obj = sender as Control;
            App.Language = new System.Globalization.CultureInfo(obj.Tag.ToString());
        }

        #region Commands

        private void NewCmd_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        private void NewCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            AddNewNote();
        }

        private void OpenCmd_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        private void OpenCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Directory.GetCurrentDirectory() + "\\Notes\\";
            openFileDialog.Filter = "note files (*.note)|*.note";
            openFileDialog.Multiselect = true;

            if(openFileDialog.ShowDialog() == true)
            {
                _storage.AddRange(openFileDialog.FileNames);
            }
        }

        private void SaveCmd_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _storage.Count > 0 && !(NotesListBox.SelectedItem is null);
        }
        private void SaveCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            INote note = NotesListBox.SelectedItem as INote;

            if (note.IsTemp)
            {
                _storage.SaveNew(note, GetSavePath(note.Name));
            }
            else
                _storage.Save(note);
        }

        private void SaveAsCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            INote note = NotesListBox.SelectedItem as INote;
            _storage.SaveNew(note, GetSavePath(note.Name));
        }
        
        private void FindCmd_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _storage.Count > 0;
        }
        private void FindCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SearchTextBox.Focus();
        }

        private void DeleteCmd_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _storage.Count > 0 && !(NotesListBox.SelectedItem is null);
        }
        private void DeleteCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RemoveCurrentNote();
        }

        #endregion

        private string GetSavePath(string name)
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialDirectory = Directory.GetCurrentDirectory() + "\\Notes\\";
            saveFileDialog.FileName = name;
            saveFileDialog.Filter = "note files (*.note)|*.note";
            if (saveFileDialog.ShowDialog() == true)
            {
                return saveFileDialog.FileName;
            }
            return string.Empty;
        }

        private void AddNewNote()
        {
            _storage.Add(new Note());
        }
        private void RemoveCurrentNote()
        {
            _storage.Remove(NotesListBox.SelectedItem as INote);
        }
    }
}
