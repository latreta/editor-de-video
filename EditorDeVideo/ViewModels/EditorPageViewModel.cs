using Prism.Windows.Mvvm;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Media.Editing;
using Windows.Media.Effects;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml.Controls;

namespace EditorDeVideo.ViewModels
{
    public class EditorPageViewModel : ViewModelBase
    {
        private MediaComposition composition = new MediaComposition();
        private MediaStreamSource mediaStreamSource;
        public InkCanvas MyCanvas { get; set; }
        public IReadOnlyList<InkStroke> currentStrokes;

        private MediaPlayerElement mediaPlayer;
        public MediaPlayerElement MediaPlayer
        {
            get { return mediaPlayer; }
            set { SetProperty(ref mediaPlayer, value); }
        }

        private int selectedIndex = 0;
        public int SelectedIndex
        {
            get { return selectedIndex; }
            set { SetProperty(ref selectedIndex, value);}
        }

        private bool canvasActivated = false;
        public bool CanvasActivated
        {
            get { return canvasActivated; }
            set { SetProperty(ref canvasActivated, value); }
        }

        public ObservableCollection<StorageFile> AddedFiles { get; set; } = new ObservableCollection<StorageFile>();

        private void ApplyStrokesToClip()
        {
            VideoEffectDefinition videoEffect = new VideoEffectDefinition(typeof(VideoEffects.PenEffect).FullName,
                    new PropertySet() { { "InkStrokes", new List<InkStroke>(currentStrokes) } });

            composition.Clips.ElementAt(SelectedIndex).VideoEffectDefinitions.Add(videoEffect);
        }

        public void SetupCanvas(InkCanvas canvas)
        {
            MyCanvas = canvas;
            MyCanvas.InkPresenter.InputDeviceTypes = CoreInputDeviceTypes.Mouse | CoreInputDeviceTypes.Pen;
            InkDrawingAttributes drawAttributes = new InkDrawingAttributes();
            drawAttributes.Color = Colors.Red;
            drawAttributes.IgnorePressure = false;
            drawAttributes.FitToCurve = true;
            MyCanvas.InkPresenter.UpdateDefaultDrawingAttributes(drawAttributes);
            MyCanvas.InkPresenter.StrokesCollected += InkPresenter_StrokesCollected;
        }

        private void InkPresenter_StrokesCollected(InkPresenter sender, InkStrokesCollectedEventArgs args)
        {
            currentStrokes = sender.StrokeContainer.GetStrokes();
        }

        public async Task AddFileAsync()
        {
            var openPicker = new Windows.Storage.Pickers.FileOpenPicker();

            openPicker.FileTypeFilter.Add(".mp4");
            openPicker.FileTypeFilter.Add(".png");
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");

            IReadOnlyList<StorageFile> files = await openPicker.PickMultipleFilesAsync();

            if (files != null)
            {
                foreach (StorageFile file in files)
                {
                    MediaClip clip = await handleFile(file);

                    if (clip != null)
                    {
                        composition.Clips.Add(clip);
                        AddedFiles.Add(file);
                    }
                }
                RenderVideo();
            }
        }

        private async Task<MediaClip> handleFile(StorageFile file)
        {
            MediaClip clip = null;

            if (new List<string>() { ".jpeg", ".png", ".jpg" }.Contains(file.FileType))
            {
                clip = await MediaClip.CreateFromImageFileAsync(file, TimeSpan.FromSeconds(3));
            }
            else
            {
                clip = await MediaClip.CreateFromFileAsync(file);
            }

            return clip;
        }

        private void RenderVideo()
        {
            mediaStreamSource = composition.GeneratePreviewMediaStreamSource(
                (int)MediaPlayer.ActualWidth,
                (int)MediaPlayer.ActualHeight);
            MediaPlayer.Source = MediaSource.CreateFromMediaStreamSource(mediaStreamSource);
        }

        public async Task ExportToFileAsync()
        {
            var picker = new Windows.Storage.Pickers.FileSavePicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.VideosLibrary;
            picker.FileTypeChoices.Add("MP4 files", new List<string>() { ".mp4" });
            picker.SuggestedFileName = "NewComposition.mp4";

            Windows.Storage.StorageFile file = await picker.PickSaveFileAsync();
            
            var temefeito = (composition.Clips.ElementAt(0).VideoEffectDefinitions.FirstOrDefault(i => i.ActivatableClassId.Equals(typeof(VideoEffects.PenEffect).FullName)) != null);


            if (file != null) {
                _ = composition.RenderToFileAsync(file, MediaTrimmingPreference.Precise);
            }
        }

        public void Draw()
        {
            if (AddedFiles.Count <= 0)
                return;

            if (CanvasActivated)
            {
                ApplyStrokesToClip();
                RenderVideo();
                MyCanvas.InkPresenter.StrokeContainer.Clear();
                SelectedIndex = 0;
            }
            CanvasActivated = !CanvasActivated;
        }

        public void OnSelectedItem(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e){
            if (composition.Clips.Count > 0)
            {
                MediaPlayer.MediaPlayer.PlaybackSession.Position = composition.Clips.ElementAt(SelectedIndex).StartTimeInComposition;
            }
        }

    }
}
