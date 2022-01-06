using Microsoft.Win32;
using Notes.Controllers;
using Notes.Controllers.Interfaces;
using Notes.Controllers.Utility;
using Notes.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Notes
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ToggleButton CurrentNoteUIElement = null; //DONT FUCKING TOUCH
        private INote CurrentNoteLink;
        private INoteStorage NoteStorage;

        private void ChangeCurrentNote(ToggleButton currentNote)
        {
            CurrentNoteUIElement = currentNote;
            if (CurrentNoteUIElement is null)
            {
                NoteName.IsEnabled = false;
                NoteContent.IsEnabled = false;
            }
            else
            {
                NoteName.IsEnabled = true;
                NoteContent.IsEnabled = true;
            }
        }

        private void AddNewNoteToUI(string name, string date)
        {
            ToggleButton note = new ToggleButton();
            DockPanel noteInfo = new DockPanel();
            TextBlock noteName = new TextBlock();
            TextBlock noteDate = new TextBlock();


            noteName.Text = name;
            noteName.HorizontalAlignment = HorizontalAlignment.Left;
            noteName.Margin = new Thickness(5, 5, 5, 5);

            noteDate.Text = date;
            noteDate.VerticalAlignment = VerticalAlignment.Bottom;
            noteDate.HorizontalAlignment = HorizontalAlignment.Right;
            noteDate.Margin = new Thickness(5, 5, 5, 5);

            noteInfo.Children.Add(noteName);
            noteInfo.Children.Add(noteDate);

            DockPanel.SetDock(noteName, Dock.Top);
            DockPanel.SetDock(noteDate, Dock.Bottom);

            note.Height = 75;
            note.Margin = new Thickness(10, 5, 10, 5);
            note.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            note.VerticalContentAlignment = VerticalAlignment.Stretch;
            note.Content = noteInfo;
            note.Checked += new RoutedEventHandler(NoteListClick);
            NoteListStackPanel.Children.Add(note);
            note.IsChecked = true;
            ChangeCurrentNote(note);
        }
        private void UploadAllNotesToUI()
        {  
            List<INote> notes = NoteStorage.Cache.ToList();
            foreach (INote note in notes)
                AddNewNoteToUI(note.Name, note.Date.ToShortDateString().ToString());
        }
        private void SelectNote(ToggleButton note)
        {
            if (CurrentNoteUIElement != null)
            {
                CurrentNoteUIElement.IsChecked = false;
            }
            ToggleButton currentNote = note;
            ChangeCurrentNote(currentNote);
            currentNote.IsChecked = true;
            String currentNoteName = ((TextBlock)((DockPanel)currentNote.Content).Children[0]).Text;
            foreach (INote noteLink in NoteStorage.Cache.ToList())
                if (noteLink.Name == currentNoteName)
                {
                    CurrentNoteLink = noteLink;
                    break;
                }
            NoteName.Text = CurrentNoteLink.Name;
            NoteContent.Document.Blocks.Clear();
            NoteContent.AppendText(CurrentNoteLink.Text);
        }
        private void DeleteCurrentNoteFromUI()
        {
            if (CurrentNoteUIElement != null)
            {
                NoteListStackPanel.Children.Remove(CurrentNoteUIElement);
                ChangeCurrentNote(null);
                NoteName.Clear();
                NoteContent.Document.Blocks.Clear();
            }
            else
                NoteName.Text = "";
        }
        private void ChangeCurrentNoteNameInUI()
        {
            if (CurrentNoteUIElement != null)
            {
                NoteName.Foreground = Brushes.Black;
                TextBlock txt = (TextBlock)((DockPanel)CurrentNoteUIElement.Content).Children[0];
                txt.Text = NoteName.Text;
            }
            else
            {
                NoteName.Text = "Имя заметки";
                NoteName.Foreground = Brushes.Black;
            }
        }
        private void OnRemoveUnsafed(object sender, RemoveEventArgs e)
        {
            INote note = e.Current;
            note.Name = Directory.GetCurrentDirectory() + "\\Notes\\" + note.Name + ".note";
            note.Save();
        }
        public MainWindow()
        {
            NoteStorage = new NoteMemoryStorage();
            NoteStorage.OnRemoveModified += OnRemoveUnsafed;
            InitializeComponent();
            if (CurrentNoteUIElement is null)
                NoteName.IsEnabled = false;
            RestoreDisk();
            UploadAllNotesToUI();
        }

        private void RestoreDisk()
        {
            string[] allfiles = Directory.GetFiles(Directory.GetCurrentDirectory() + "\\Notes\\");
            NoteStorage.AddRange(allfiles);
        }
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (NoteStorage.Cache.Count == 0)
                NoteStorage.Add("Новая заметка");
            else
                NoteStorage.Add($"Новая заметка({NoteStorage.Cache.Count})");
            CurrentNoteLink = NoteStorage.Cache.Last();
            AddNewNoteToUI(CurrentNoteLink.Name, CurrentNoteLink.Date.ToShortDateString().ToString());
        }
        private void NoteListClick(object sender, EventArgs e)
        {
            SelectNote(sender as ToggleButton);
        }
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            DeleteCurrentNoteFromUI();
            NoteStorage.Remove(CurrentNoteLink);
        }
        private void NoteName_TextChanged(object sender, TextChangedEventArgs e)
        {
            ChangeCurrentNoteNameInUI();
        }
        private void SearchTextBox_IsKeyboardFocusedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (SearchTextBox.Text == "Найти..." || SearchTextBox.Text == "")
            {
                if (SearchTextBox.IsKeyboardFocused)
                {
                    SearchTextBox.Text = "";
                    SearchTextBox.Foreground = Brushes.Black;
                }
                else if (SearchTextBox.Text == "")
                {
                    SearchTextBox.Text = "Найти...";
                    SearchTextBox.Foreground = Brushes.DarkGray;
                }
            }
            if (IsKeyboardFocused == false)
                NoteName.IsEnabled = false;
        }
        private void NoteName_IsKeyboardFocusedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (NoteName.Text == "Имя заметки" || NoteName.Text == "")
            {
                if (NoteName.IsKeyboardFocused)
                {
                    NoteName.Text = "";
                    NoteName.Foreground = Brushes.Black;
                }
                else if (NoteName.Text == "")
                {
                    NoteName.Text = "Имя заметки";
                    NoteName.Foreground = Brushes.DarkGray;
                }
            }
            else if (NoteName.IsKeyboardFocused == false)
            {
                if (CurrentNoteLink != null)
                    CurrentNoteLink.Name = NoteName.Text;
            }
        }
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

            if (CurrentNoteUIElement != null)
            {
                SearchTextBox.Foreground = Brushes.Black;
                TextBlock txt = (TextBlock)((DockPanel)CurrentNoteUIElement.Content).Children[0];
                txt.Text = NoteName.Text;
            }
            else
            {
                SearchTextBox.Text = "Найти...";
                SearchTextBox.Foreground = Brushes.Black;
            }

            if (SearchTextBox.Text != "" && SearchTextBox.Text != "Найти...")
                UpdateNoteListUI(NoteStorage.Find(SearchTextBox.Text));
            else if (SearchTextBox.Text == "Найти..." || SearchTextBox.Text == "")
                UpdateNoteListUI(NoteStorage.Cache);
        }
        private void UpdateNoteListUI(IEnumerable<INote> Notes)
        {
            if (NoteListStackPanel != null)
                while (NoteListStackPanel.Children.Count > 0)
                    NoteListStackPanel.Children.RemoveAt(NoteListStackPanel.Children.Count - 1);
            foreach (INote note in Notes)
                AddNewNoteToUI(note.Name, note.Date.ToShortDateString().ToString());
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            NoteStorage.RemoveAll();
        }
        private void NoteContent_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CurrentNoteLink != null)
                CurrentNoteLink.Text = new TextRange(NoteContent.Document.ContentStart, NoteContent.Document.ContentEnd).Text;
        }
    }
}
