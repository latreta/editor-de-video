using EditorDeVideo.ViewModels;
using Windows.UI.Xaml.Controls;

namespace EditorDeVideo.Views
{
    public sealed partial class EditorPage : Page
    {

        public EditorPageViewModel ViewModel => DataContext as EditorPageViewModel;

        public EditorPage()
        {
            this.InitializeComponent();
            ViewModel.MediaPlayer = mediaPlayer;
            ViewModel.SetupCanvas(MyCanvas);
        }
    }
}
