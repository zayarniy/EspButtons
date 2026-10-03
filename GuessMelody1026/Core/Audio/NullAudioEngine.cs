using System;
using System.IO;
//using NAudio.Vorbis;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace GuessMelody.Core.Audio
{
    /// <summary>
    /// Реализация IAudioEngine на NAudio.
    /// - один "фоновый" трек (mp3/wav/ogg) с паузой/seek
    /// - one-shot звуки поверх (тик, right/wrong) через микшер
    /// </summary>
    public sealed class NaAudioEngine : IAudioEngine
    {
        // --- Фоновое воспроизведение ---
        private WaveOutEvent _output;
        private WaveStream _reader;
        private VolumeSampleProvider _volumeProvider;
        private MixingSampleProvider _mixer;

        // --- Состояние ---
        private float _volume = 1.0f;
        private readonly object _lock = new object();

        public event EventHandler TrackEnded;

        public bool IsPlaying => _output?.PlaybackState == PlaybackState.Playing;
        public TimeSpan Position => _reader?.CurrentTime ?? TimeSpan.Zero;
        public TimeSpan Duration => _reader?.TotalTime ?? TimeSpan.Zero;

        public float Volume
        {
            get => _volume;
            set
            {
                _volume = Math.Max(0f, Math.Min(1f, value));
                if (_volumeProvider != null)
                    _volumeProvider.Volume = _volume;
            }
        }

        public NaAudioEngine()
        {
            EnsureOutput();
        }

        // =============================================================
        // Загрузка
        // =============================================================
        public void Load(string file)
        {
            if (string.IsNullOrWhiteSpace(file))
                throw new ArgumentException("Путь к файлу пуст.", nameof(file));
            if (!File.Exists(file))
                throw new FileNotFoundException("Файл не найден", file);

            lock (_lock)
            {
                StopInternal();

                _reader = CreateReader(file);
                _volumeProvider = new VolumeSampleProvider(_reader.ToSampleProvider())
                {
                    Volume = _volume
                };

                EnsureOutput();

                // Микшер: основной поток + one-shot'ы
                _mixer = new MixingSampleProvider(
                    WaveFormat.CreateIeeeFloatWaveFormat(
                        _reader.WaveFormat.SampleRate,
                        _reader.WaveFormat.Channels))
                {
                    ReadFully = true
                };
                _mixer.AddMixerInput(_volumeProvider);

                _output.Init(_mixer);

            }
        }

        private WaveStream CreateReader(string file)
        {
            string ext = Path.GetExtension(file).ToLowerInvariant();
            switch (ext)
            {
                //case ".ogg":
                //    return new VorbisWaveReader(file);

                case ".mp3":
                case ".wav":
                case ".aiff":
                case ".m4a":
                case ".aac":
                case ".flac":       // flac — только если MediaFoundation его поддерживает
                    return new AudioFileReader(file);

                default:
                    throw new NotSupportedException($"Формат {ext} не поддерживается.");
            }
        }

        private void EnsureOutput()
        {
            if (_output != null) return;
            _output = new WaveOutEvent { DesiredLatency = 100 };
            _output.PlaybackStopped += Output_PlaybackStopped;
        }

        // =============================================================
        // Управление
        // =============================================================
        public void Play()
        {
            if (_output == null || _reader == null) return;
            lock (_lock)
            {
                if (_output.PlaybackState == PlaybackState.Paused ||
                    _output.PlaybackState == PlaybackState.Stopped)
                {
                    // Если дошли до конца — сначала отмотаем в начало
                    if (_reader.Position >= _reader.Length - 1)
                        _reader.Position = 0;

                    _output.Play();
                }
            }
        }

        public void Pause()
        {
            lock (_lock)
            {
                if (_output?.PlaybackState == PlaybackState.Playing)
                    _output.Pause();
            }
        }

        public void Stop()
        {
            lock (_lock)
            {
                StopInternal();
            }
        }

        private void StopInternal()
        {
            try { _output?.Stop(); } catch { }
            if (_reader != null)
            {
                try { _reader.Position = 0; } catch { }
            }
        }

        public void Seek(TimeSpan position)
        {
            lock (_lock)
            {
                if (_reader == null) return;
                if (position < TimeSpan.Zero) position = TimeSpan.Zero;
                if (position > _reader.TotalTime) position = _reader.TotalTime;
                _reader.CurrentTime = position;
            }
        }

        // =============================================================
        // One-shot: накладываем поверх через микшер
        // =============================================================
        public void PlayOneShot(string file, float volume = 1.0f)
        {
            if (string.IsNullOrWhiteSpace(file) || !File.Exists(file)) return;

            try
            {
                var reader = CreateReader(file);
                var sp = reader.ToSampleProvider();

                // Подгоняем под формат микшера
                if (_mixer != null && sp.WaveFormat.SampleRate != _mixer.WaveFormat.SampleRate)
                {
                    sp = new WdlResamplingSampleProvider(sp, _mixer.WaveFormat.SampleRate);
                }
                if (_mixer != null && sp.WaveFormat.Channels != _mixer.WaveFormat.Channels)
                {
                    if (sp.WaveFormat.Channels == 1 && _mixer.WaveFormat.Channels == 2)
                        sp = new MonoToStereoSampleProvider(sp);
                    else if (sp.WaveFormat.Channels == 2 && _mixer.WaveFormat.Channels == 1)
                        sp = new StereoToMonoSampleProvider(sp);
                }

                var withVolume = new VolumeSampleProvider(sp) { Volume = volume };

                // Отдельный WaveOutEvent для одного звука — надёжнее,
                // не блокирует основной поток и не требует сложного учёта
                // жизненного цикла в микшере.
                var outEvent = new WaveOutEvent { DesiredLatency = 100 };
                outEvent.Init(withVolume);
                outEvent.PlaybackStopped += (_, __) =>
                {
                    try { outEvent.Dispose(); } catch { }
                    try { reader.Dispose(); } catch { }
                };
                outEvent.Play();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[NaAudioEngine] OneShot error: {ex.Message}");
            }
        }

        // =============================================================
        // События
        // =============================================================
        //private void Reader_PositionChanged(object sender, EventArgs e)
        //{
        //    if (_reader == null) return;
        //    if (_reader.Position >= _reader.Length - 1)
        //        TrackEnded?.Invoke(this, EventArgs.Empty);
        //}


        private void Output_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            // Ошибка воспроизведения
            if (e.Exception != null)
            {
                Console.Error.WriteLine($"[NaAudioEngine] Playback error: {e.Exception.Message}");
                return;
            }

            // Если проигрывание остановилось из-за достижения конца трека —
            // поднимаем TrackEnded. Проверяем по позиции ридера.
            if (_reader != null)
            {
                // Небольшой допуск: позиция может чуть-чуть не дойти до конца.
                var remaining = _reader.TotalTime - _reader.CurrentTime;
                if (remaining <= TimeSpan.FromMilliseconds(500))
                {
                    TrackEnded?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        // =============================================================
        // Dispose
        // =============================================================
        public void Dispose()
        {
            lock (_lock)
            {
                try { _output?.Stop(); } catch { }
                try { _output?.Dispose(); } catch { }
                _output = null;

                try { _reader?.Dispose(); } catch { }
                _reader = null;

                _mixer = null;
                _volumeProvider = null;
            }
        }
    }
}