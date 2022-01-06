using Notes.Controllers;
using Notes.Models;
using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;

namespace Notes
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static CultureInfo Language
        {
            get => System.Threading.Thread.CurrentThread.CurrentUICulture;
            set
            {
                if (value is null) 
                    throw new ArgumentNullException("value");
                if (value == System.Threading.Thread.CurrentThread.CurrentUICulture) 
                    return;

                System.Threading.Thread.CurrentThread.CurrentUICulture = value;

                ResourceDictionary newDict = new ResourceDictionary();
                newDict.Source = new Uri(String.Format("Resources/Languages/{0}.xaml", value.Name), UriKind.Relative);

                ResourceDictionary oldDict = Application.Current.Resources.MergedDictionaries
                    .First(dict => dict.Source != null
                     && dict.Source.OriginalString.StartsWith("Resources/Languages/"));

                Note.NewFileName = newDict["NewFileName"].ToString();

                if (oldDict != null)
                {
                    int index = Application.Current.Resources.MergedDictionaries.IndexOf(oldDict);
                    Application.Current.Resources.MergedDictionaries.Remove(oldDict);
                    Application.Current.Resources.MergedDictionaries.Insert(index, newDict);
                }
                else
                {
                    Application.Current.Resources.MergedDictionaries.Add(newDict);
                }
            }
        }
        public NoteMemoryStorage Collection { get; set; } = new NoteMemoryStorage();

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            LoadNotes();
        }
        private void LoadNotes()
        {
            string[] allfiles = Directory
                .GetFiles(Directory.GetCurrentDirectory() + "\\Notes\\")
                .Where(path => path.Contains(".note"))
                .ToArray();
            Collection.AddRange(allfiles);
        }
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.Message);
            App.Current.MainWindow.UpdateLayout();
            e.Handled = true;
        }
    }
}
