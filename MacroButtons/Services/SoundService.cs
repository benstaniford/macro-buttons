using NAudio.Wave;
using System.IO;
using System.Windows;

namespace MacroButtons.Services;

/// <summary>
/// Service for playing sound effects using NAudio.
/// Supports playing sounds from embedded resources or file paths.
/// </summary>
public class SoundService : IDisposable
{
    private readonly LoggingService? _loggingService;
    private WaveOutEvent? _waveOut;
    private Mp3FileReader? _mp3Reader;
    private MemoryStream? _soundStream;
    private bool _soundEnabled = true;
    private float _volume = 0.5f; // Default 50% volume

    public SoundService(LoggingService? loggingService = null)
    {
        _loggingService = loggingService;
        LoadDefaultSound();
    }

    /// <summary>
    /// Enable or disable sound playback.
    /// </summary>
    public bool SoundEnabled
    {
        get => _soundEnabled;
        set => _soundEnabled = value;
    }

    /// <summary>
    /// Set the playback volume (0.0 to 1.0).
    /// </summary>
    public float Volume
    {
        get => _volume;
        set
        {
            _volume = Math.Clamp(value, 0.0f, 1.0f);
            if (_waveOut != null)
                _waveOut.Volume = _volume;
        }
    }

    /// <summary>
    /// Load the default button click sound from embedded resources.
    /// </summary>
    private void LoadDefaultSound()
    {
        try
        {
            // Load the embedded sound resource
            var resourceUri = new Uri("pack://application:,,,/MacroButtons;component/Resources/button-click.mp3");
            var resourceInfo = Application.GetResourceStream(resourceUri);

            if (resourceInfo != null)
            {
                // Copy to MemoryStream so we can reuse it
                _soundStream = new MemoryStream();
                resourceInfo.Stream.CopyTo(_soundStream);
                _soundStream.Position = 0;

                _loggingService?.LogInfo("Sound effect loaded successfully");
            }
            else
            {
                _loggingService?.LogWarning("Sound effect resource not found");
            }
        }
        catch (Exception ex)
        {
            _loggingService?.LogError("Failed to load sound effect", ex);
        }
    }

    /// <summary>
    /// Play the button click sound effect.
    /// </summary>
    public void PlayButtonClick()
    {
        if (!_soundEnabled || _soundStream == null)
            return;

        try
        {
            // Stop any currently playing sound
            StopSound();

            // Reset stream position
            _soundStream.Position = 0;

            // Create new readers for this playback
            _mp3Reader = new Mp3FileReader(_soundStream, waveFormat => new Mp3FileReader.Mp3FrameDecompressor(waveFormat));
            _waveOut = new WaveOutEvent();
            _waveOut.Volume = _volume;

            // Set up event to dispose readers when playback completes
            _waveOut.PlaybackStopped += (sender, args) =>
            {
                _mp3Reader?.Dispose();
                _mp3Reader = null;
                _waveOut?.Dispose();
                _waveOut = null;
            };

            _waveOut.Init(_mp3Reader);
            _waveOut.Play();
        }
        catch (Exception ex)
        {
            _loggingService?.LogError("Failed to play sound effect", ex);
        }
    }

    /// <summary>
    /// Play a custom sound from a file path.
    /// </summary>
    public void PlaySound(string filePath)
    {
        if (!_soundEnabled || !File.Exists(filePath))
            return;

        try
        {
            // Stop any currently playing sound
            StopSound();

            // Create new readers for this playback
            _mp3Reader = new Mp3FileReader(filePath);
            _waveOut = new WaveOutEvent();
            _waveOut.Volume = _volume;

            // Set up event to dispose readers when playback completes
            _waveOut.PlaybackStopped += (sender, args) =>
            {
                _mp3Reader?.Dispose();
                _mp3Reader = null;
                _waveOut?.Dispose();
                _waveOut = null;
            };

            _waveOut.Init(_mp3Reader);
            _waveOut.Play();
        }
        catch (Exception ex)
        {
            _loggingService?.LogError($"Failed to play sound from {filePath}", ex);
        }
    }

    /// <summary>
    /// Stop the currently playing sound.
    /// </summary>
    private void StopSound()
    {
        if (_waveOut != null)
        {
            _waveOut.Stop();
            _waveOut.Dispose();
            _waveOut = null;
        }

        if (_mp3Reader != null)
        {
            _mp3Reader.Dispose();
            _mp3Reader = null;
        }
    }

    /// <summary>
    /// Dispose of audio resources.
    /// </summary>
    public void Dispose()
    {
        StopSound();
        _soundStream?.Dispose();
        _soundStream = null;
    }
}
