using NAudio.Wave;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;

namespace veDDDa
{
    public class HandleFFT
    {
        // MICROPHONE ANALYSIS SETTINGS
        private int RATE = 44100; // sample rate of the sound card
        private int BUFFERSIZE = (int)Math.Pow(2, 11); // must be a multiple of 2
        private Image<Rgba32> _image;
        private byte[] _buffer;
        public int _texture;
        public HandleFFT()
        {
            _image = new Image<Rgba32>(BUFFERSIZE, 1); // no vertical flip as only 1px height

            //Use the CopyPixelDataTo function from ImageSharp to copy all of the bytes from the image into an array that we can give to OpenGL.
            _buffer = new byte[4 * _image.Width * _image.Height];

            // creating a texture
            _texture = 0;
            GL.GenTextures(1, out _texture);
            UpdateGLTexture();
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

        }
        public void UpdateGLTexture()
        {
            _image.CopyPixelDataTo(_buffer);
            GL.Enable(EnableCap.Texture2D);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, _image.Width, _image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, _buffer);

        }

        public BufferedWaveProvider bwp;

        // Taken from swharden here : https://github.com/swharden/Csharp-Data-Visualization
        public double[] FFT(double[] data)
        {
            double[] fft = new double[data.Length];
            System.Numerics.Complex[] fftComplex = new System.Numerics.Complex[data.Length];
            for (int i = 0; i < data.Length; i++)
                fftComplex[i] = new System.Numerics.Complex(data[i], 0.0);
            Accord.Math.FourierTransform.FFT(fftComplex, Accord.Math.FourierTransform.Direction.Forward);
            for (int i = 0; i < data.Length; i++)
                fft[i] = fftComplex[i].Magnitude;
            return fft;
        }
        void AudioDataAvailable(object sender, WaveInEventArgs e)
        {
            bwp.AddSamples(e.Buffer, 0, e.BytesRecorded);
        }
        // Taken from swharden here : https://github.com/swharden/Csharp-Data-Visualization
        public void StartListeningToMicrophone(int audioDeviceNumber = 0)
        {
            WaveIn wi = new WaveIn();
            wi.DeviceNumber = audioDeviceNumber;
            wi.WaveFormat = new NAudio.Wave.WaveFormat(RATE, 1);
            wi.BufferMilliseconds = (int)((double)BUFFERSIZE / (double)RATE * 1000.0);
            wi.DataAvailable += new EventHandler<WaveInEventArgs>(AudioDataAvailable);
            bwp = new BufferedWaveProvider(wi.WaveFormat);
            bwp.BufferLength = BUFFERSIZE * 2;
            bwp.DiscardOnBufferOverflow = true;
            try
            {
                wi.StartRecording();
            }
            catch
            {
                string msg = "Could not record from audio device!\n\n";
                msg += "Is your microphone plugged in?\n";
                msg += "Is it set as your default recording device?";
                MessageBox.Show(msg, "ERROR");
            }
        }

        public static float Lerp(float A, float B, float lerpValue)
        {
            var result = A * (1 - lerpValue) + B * lerpValue;

            return result;
        }
        public static float Saturate(float val)
        {
            return Math.Max(Math.Min(val, 1.0f), 0.0f);
        }
        public void PlotSignal()
        {
            // check the incoming microphone audio
            int frameSize = BUFFERSIZE;
            var audioBytes = new byte[frameSize];
            bwp.Read(audioBytes, 0, frameSize);

            // return if there's nothing new to plot
            if (audioBytes.Length == 0)
                return;
            if (audioBytes[frameSize - 2] == 0)
                return;

            // incoming data is 16-bit (2 bytes per audio point)
            int BYTES_PER_POINT = 2;

            // create a (32-bit) int array ready to fill with the 16-bit data
            int graphPointCount = audioBytes.Length / BYTES_PER_POINT;

            // create double arrays to hold the data we will graph
            double[] pcm = new double[graphPointCount];
            double[] fftReal = new double[graphPointCount / 2];

            // populate Xs and Ys with double data
            for (int i = 0; i < graphPointCount; i++)
            {
                // read the int16 from the two bytes
                var val = BitConverter.ToInt16(audioBytes, i * 2);

                // store the value in Ys as a percent (+/- 100% = 200%)
                pcm[i] = (double)(val) / Math.Pow(2, 14) * 200.0;
            }
            var fft = FFT(pcm);
            for (int i = 0; i < _image.Width; ++i)
            {
                float fi = (float)i;
                float factor = fi / (float)_image.Width;
                float sampleId = factor * fft.Length*0.5f + fft.Length*0.5f;
                float val = (float)fft[(int)Math.Floor(sampleId)];
                val *= 1.2f;
                val = Saturate(val);
                var prev = (float)_image[i, 0].R / 255.0f;
                val = Lerp(val, prev, 0.7f);
                _image[i, 0] = new Color(new Rgba32(val, val, val));
            }
            UpdateGLTexture();
        }
    }
}
