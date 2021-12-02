using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Options;

using Server.Configuration;
using Server.Models;

namespace Server.Services {
    public class AnnouncementAudioCreator {
        private readonly SpeechOptions speechOptions;
        private readonly ILogger<AnnouncementAudioCreator> logger;

        public AnnouncementAudioCreator(ILogger<AnnouncementAudioCreator> logger, IOptions<SpeechOptions> speechOptions) {
            this.speechOptions = speechOptions.Value;
            this.logger = logger;
        }

        // TODO: for hosted solutions, like Azure, we'd need to find a spot where we're actually
        // allowed to write files. That's no concern for now, so we'll just write to a Temp folder.
        public async Task<string?> CreateAudioFor(Announcement announcement) {
            var fileName = $"Temp/{announcement.Id}.mp3";
            var fileInfo = new FileInfo(fileName);

            if (fileInfo.Exists && fileInfo.Length > 0) { return fileName; }

            var config = SpeechConfig.FromSubscription(speechOptions.Key, speechOptions.Region);
            config.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Audio16Khz128KBitRateMonoMp3);

            using var fileOutput = AudioConfig.FromWavFileOutput(fileName);
            using var synthesizer = new SpeechSynthesizer(config, fileOutput);

            using var result = await synthesizer.SpeakTextAsync(announcement.Text);

            if (result.Reason == ResultReason.SynthesizingAudioCompleted) {
                logger.LogInformation("Speech synthesis done for {announcementId} - file saved to {fileName}", announcement.Id, fileName);

                return fileName;
            } else if (result.Reason == ResultReason.Canceled) {
                var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
                logger.LogWarning("Speech synthesis canceled for {announcementId} - reason={cancellationReason}", announcement.Id, cancellation.Reason);

                if (cancellation.Reason == CancellationReason.Error) {
                    logger.LogError("Speech synthesis canceled: ErrorCode={errorCode}", cancellation.ErrorCode);
                    logger.LogError("Speech synthesis canceled: ErrorDetails=[{errorDetails}]", cancellation.ErrorDetails);
                    logger.LogError("Speech synthesis canceled: Did you update the subscription info?");
                }
            }

            return null;
        }
    }
}