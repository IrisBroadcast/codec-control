using CodecControl.Client.Models;
using CodecControl.Web.Helpers;

namespace CodecControl.Web.Models
{
    public class AudioModeResponse 
    {
        public AudioAlgorithm EncoderAudioMode { get; set; }
        public AudioAlgorithm DecoderAudioMode { get; set; }

        public string EncoderAudioModeString => EncoderAudioMode.Description();
        public string DecoderAudioModeString => DecoderAudioMode.Description();
    }
}