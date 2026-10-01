using System;
using System.IO;
using System.Media;

namespace SnakeGame;

public static class SoundManager
{
    public static bool IsMuted { get; set; } = false;

    private static SoundPlayer? _eatPlayer;
    private static SoundPlayer? _bonusPlayer;
    private static SoundPlayer? _diePlayer;
    private static SoundPlayer? _highScorePlayer;

    static SoundManager()
    {
        try
        {
            _eatPlayer = CreateTonePlayer(new[] { (523, 40), (659, 50) }); // C5 -> E5
            _bonusPlayer = CreateTonePlayer(new[] { (587, 45), (740, 45), (880, 60) }); // D5 -> F#5 -> A5
            _diePlayer = CreateTonePlayer(new[] { (400, 70), (310, 80), (220, 110) }); // descending
            _highScorePlayer = CreateTonePlayer(new[] { (523, 60), (659, 60), (784, 60), (1046, 120) }); // C5-E5-G5-C6
        }
        catch
        {
            // If audio initialization fails, sound will remain silent
        }
    }

    public static void PlayEat()
    {
        if (IsMuted) return;
        PlaySafely(_eatPlayer);
    }

    public static void PlayBonus()
    {
        if (IsMuted) return;
        PlaySafely(_bonusPlayer);
    }

    public static void PlayDie()
    {
        if (IsMuted) return;
        PlaySafely(_diePlayer);
    }

    public static void PlayHighScore()
    {
        if (IsMuted) return;
        PlaySafely(_highScorePlayer);
    }

    private static void PlaySafely(SoundPlayer? player)
    {
        if (player == null) return;
        try
        {
            player.Play();
        }
        catch
        {
            // Ignore sound card or player errors
        }
    }

    private static SoundPlayer? CreateTonePlayer((int freq, int durationMs)[] notes)
    {
        try
        {
            int sampleRate = 22050;
            short bitsPerSample = 16;
            short channels = 1;

            int totalDurationMs = 0;
            foreach (var note in notes) totalDurationMs += note.durationMs;

            int totalSamples = (int)(sampleRate * (totalDurationMs / 1000.0));
            int subchunk2Size = totalSamples * channels * (bitsPerSample / 8);
            int chunkSize = 36 + subchunk2Size;

            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            // RIFF chunk descriptor
            writer.Write(new[] { 'R', 'I', 'F', 'F' });
            writer.Write(chunkSize);
            writer.Write(new[] { 'W', 'A', 'V', 'E' });

            // "fmt " sub-chunk
            writer.Write(new[] { 'f', 'm', 't', ' ' });
            writer.Write(16); // Subchunk1Size for PCM
            writer.Write((short)1); // AudioFormat: 1 = PCM
            writer.Write(channels);
            writer.Write(sampleRate);
            writer.Write(sampleRate * channels * (bitsPerSample / 8)); // ByteRate
            writer.Write((short)(channels * (bitsPerSample / 8))); // BlockAlign
            writer.Write(bitsPerSample);

            // "data" sub-chunk
            writer.Write(new[] { 'd', 'a', 't', 'a' });
            writer.Write(subchunk2Size);

            // Generate tones with smooth attack/decay envelope to avoid clicks
            foreach (var (freq, durationMs) in notes)
            {
                int noteSamples = (int)(sampleRate * (durationMs / 1000.0));
                for (int i = 0; i < noteSamples; i++)
                {
                    double t = (double)i / sampleRate;
                    double angle = 2.0 * Math.PI * freq * t;
                    double rawSample = Math.Sin(angle);

                    // Soft envelope: linear fade-in and fade-out
                    double envelope = 1.0;
                    int fade = Math.Min(noteSamples / 4, 100);
                    if (i < fade)
                        envelope = (double)i / fade;
                    else if (i > noteSamples - fade)
                        envelope = (double)(noteSamples - i) / fade;

                    short sample = (short)(rawSample * envelope * 0.3 * short.MaxValue);
                    writer.Write(sample);
                }
            }

            writer.Flush();
            ms.Position = 0;
            var player = new SoundPlayer(new MemoryStream(ms.ToArray()));
            player.Load();
            return player;
        }
        catch
        {
            return null;
        }
    }
}
