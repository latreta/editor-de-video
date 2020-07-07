using Microsoft.Graphics.Canvas;
using System.Collections.Generic;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.Graphics.DirectX.Direct3D11;
using Windows.Media.Effects;
using Windows.Media.MediaProperties;
using Windows.UI.Input.Inking;

namespace VideoEffects
{
    public sealed class PenEffect : IBasicVideoEffect
    {
        private CanvasDevice canvasDevice;
        private IPropertySet configuration;
        private IReadOnlyList<InkStroke> myInkStrokes;

        public void SetEncodingProperties(VideoEncodingProperties encodingProperties, IDirect3DDevice device)
        {
            canvasDevice = CanvasDevice.CreateFromDirect3D11Device(device);
            myInkStrokes = (IReadOnlyList<InkStroke>)configuration["InkStrokes"];
        }

        public void ProcessFrame(ProcessVideoFrameContext context)
        {
            using (CanvasBitmap inputBitmap = CanvasBitmap.CreateFromDirect3D11Surface(canvasDevice, context.InputFrame.Direct3DSurface))
            using (CanvasRenderTarget renderTarget = CanvasRenderTarget.CreateFromDirect3D11Surface(canvasDevice, context.OutputFrame.Direct3DSurface))
            using (CanvasDrawingSession ds = renderTarget.CreateDrawingSession())
            {
                ds.DrawImage(inputBitmap);
                ds.DrawInk(myInkStrokes);
            }

        }

        public void Close(MediaEffectClosedReason reason)
        {
        }

        public void DiscardQueuedFrames()
        {
        }

        public bool IsReadOnly { get { return true; } }

        public IReadOnlyList<VideoEncodingProperties> SupportedEncodingProperties
        {
            get
            {
                var encodingProperties = new VideoEncodingProperties();
                encodingProperties.Subtype = "ARGB32";
                return new List<VideoEncodingProperties>() { encodingProperties };
            }
        }

        public MediaMemoryTypes SupportedMemoryTypes { 
            get { 
                return MediaMemoryTypes.Gpu; 
            } 
        }

        public bool TimeIndependent
        {
            get
            {
                return true;
            } 
        }

        public void SetProperties(IPropertySet configuration)
        {
            this.configuration = configuration;
        }
    }
}
