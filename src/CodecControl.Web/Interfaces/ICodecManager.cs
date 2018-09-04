using System.Threading.Tasks;
using CodecControl.Client.Models;

namespace CodecControl.Web.Interfaces
{

    public interface ICodecManager
    {
        Task<bool> CallAsync(string sipAddress, string callee, string profileName);
        Task<bool> HangUpAsync(string sipAddress);
        Task<bool> CheckIfAvailableAsync(string sipAddress);
        Task<bool?> GetGpoAsync(string sipAddress, int gpio);
        Task<bool> SetGpoAsync(string sipAddress, int gpo, bool active);

        // GetInputEnabled, SetInputEnabled, GetInputGainLevel och SetInputGainLevel doesn't work on Quantum ST since it lacks controlable inputs.
        Task<bool> GetInputEnabledAsync(string sipAddress, int input);
        Task<bool> SetInputEnabledAsync(string sipAddress, int input, bool enabled);
        Task<int> GetInputGainLevelAsync(string sipAddress, int input);
        Task<int> SetInputGainLevelAsync(string sipAddress, int input, int gainLevel);
        Task<LineStatus> GetLineStatusAsync(string sipAddress, int line);
        Task<string> GetLoadedPresetNameAsync(string sipAddress, string lastPresetName);
        Task<VuValues> GetVuValuesAsync(string sipAddress);
        Task<AudioStatus> GetAudioStatusAsync(string sipAddress, int nrOfInputs, int nrOfGpos);
        Task<AudioMode> GetAudioModeAsync(string sipAddress);
        Task<bool> LoadPresetAsync(string sipAddress, string preset);
        Task<bool> RebootAsync(string sipAddress);
    }


}