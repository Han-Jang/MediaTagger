using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
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
using System.Windows.Threading;
using TagLib.Ogg;

namespace MediaTagger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        TagLib.File tagFile;

        private bool mediaPlayerIsPlaying = false;
        private bool userIsDraggingSlider = false;

        public MainWindow()
        {
            InitializeComponent();

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();
        }

        //close the program
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown(99);
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if ((myPlayer.Source != null) && (myPlayer.NaturalDuration.HasTimeSpan) && (!userIsDraggingSlider))
            {
                sliProgress.Minimum = 0;
                sliProgress.Maximum = myPlayer.NaturalDuration.TimeSpan.TotalSeconds; //Loads the length of the music
                sliProgress.Value = myPlayer.Position.TotalSeconds; //show you how much you are listening
            }
        }

        //Load file information (title, artist, album name, year)
        private void inputInfo()
        {
            var title = tagFile.Tag.Title;
            var artist = tagFile.Tag.Performers[0];
            var album = tagFile.Tag.Album;
            var year = tagFile.Tag.Year.ToString();

            //MainWindow.xaml for view text
            myMeta.Title.Content = title;
            myMeta.Artist.Content = artist;
            myMeta.Album.Content = album + " (" + year + ")";

            //MetaData.xaml for Edit text
            Title.Text = title;
            Artist.Text = artist;
            Album.Text = album;
            Year.Text = year;
        }

        //Load album art
        private void albumArt()
        {
            TagLib.IPicture picture = tagFile.Tag.Pictures[0];
            var sys = new System.IO.MemoryStream(picture.Data.Data);
            sys.Seek(0, System.IO.SeekOrigin.Begin);

            BitmapImage albumArt = new BitmapImage();

            albumArt.BeginInit();
            albumArt.StreamSource = sys;
            albumArt.EndInit();

            albumImage.Source = albumArt;
        }

        //openable only when there is a source
        private void Open_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        //open the mp3 file
        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Media files (*.mp3)|*.mp3|All files (*.*)|*.*";

                if (openFileDialog.ShowDialog() == true)
                {
                    tagFile = TagLib.File.Create(openFileDialog.FileName);
                    myPlayer.Source = new Uri(openFileDialog.FileName);

                    inputInfo();

                    if (tagFile.Tag.Pictures != null && tagFile.Tag.Pictures.Length > 0)
                    {
                        albumArt();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //editable only when there is a source
        private void Edit_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = myPlayer.Source != null;
        }

        //Stops song, stops myPlayer from using it, saves tag info, regives it to myPlayer, restarts song
        private void Edit_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (mediaPlayerIsPlaying == true)
            {
                try
                {
                    var temp = myPlayer.Source;
                    myPlayer.Source = null;

                    myPlayer.Stop();
                    mediaPlayerIsPlaying = false;

                    tagFile.Tag.Title = Title.Text;
                    tagFile.Tag.Album = Album.Text;
                    tagFile.Tag.Year = uint.Parse(Year.Text);

                    tagFile.Save();
                    myPlayer.Source = temp;

                    inputInfo();

                    editWindow.Visibility = Visibility.Hidden;
                    myMeta.Visibility = Visibility.Visible;

                    myPlayer.Play();
                    mediaPlayerIsPlaying = true;

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                try
                {
                    var temp = myPlayer.Source;
                    myPlayer.Source = null;

                    tagFile.Tag.Title = Title.Text;
                    tagFile.Tag.Album = Album.Text;
                    tagFile.Tag.Year = uint.Parse(Year.Text);

                    tagFile.Save();
                    myPlayer.Source = temp;

                    inputInfo();

                    editWindow.Visibility = Visibility.Hidden;
                    myMeta.Visibility = Visibility.Visible;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        //playable only when there is a source
        private void Play_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (myPlayer != null) && (myPlayer.Source != null);
        }

        //Play music
        private void Play_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            myPlayer.Play();
            mediaPlayerIsPlaying = true;
        }

        //Pauseable only when there is a source
        private void Pause_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = mediaPlayerIsPlaying;
        }

        //Pause music
        private void Pause_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            myPlayer.Pause();
        }

        //Stoppable  only when there is a source
        private void Stop_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = mediaPlayerIsPlaying;
        }

        //stop music
        private void Stop_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            myPlayer.Stop();
            mediaPlayerIsPlaying = false;
        }

        //start dragging the slider
        private void sliProgress_DragStarted(object sender, DragStartedEventArgs e)
        {
            userIsDraggingSlider = true;
        }

        //finish dragging the slider
        private void sliProgress_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            userIsDraggingSlider = false;
            myPlayer.Position = TimeSpan.FromSeconds(sliProgress.Value);
        }

        //show music time
        private void sliProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            lblProgressStatus.Text = TimeSpan.FromSeconds(sliProgress.Value).ToString(@"hh\:mm\:ss");
        }

        //Edit button behavior
        private void edit_Click(object sender, RoutedEventArgs e)
        {
            if (editWindow.Visibility == Visibility.Hidden)
            {
                editWindow.Visibility = Visibility.Visible;
                myMeta.Visibility = Visibility.Hidden;
            }
            else
            {
                editWindow.Visibility = Visibility.Hidden;
                myMeta.Visibility = Visibility.Visible;
            }
        }

    }
}
