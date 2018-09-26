using CodecControl.Client.Models;
using CodecControl.Web.Helpers;

namespace CodecControl.Web.Models.Responses
{
    public class AudioModeResponse 
    {
        public AudioAlgorithm EncoderAudioMode { get; set; }
        public AudioAlgorithm DecoderAudioMode { get; set; }

        public string EncoderAudioModeDescription => EncoderAudioMode.Description();
        public string DecoderAudioModeDescription => DecoderAudioMode.Description();
    }
}