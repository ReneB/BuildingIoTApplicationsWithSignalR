using Microsoft.Extensions.Options;

namespace Server.Services {
    public interface ITimeService {
        DateTime Now { get; }
    }

    public class TimeService : ITimeService {
        public DateTime Now => DateTime.Now;
    }

    public class TestTimeService : ITimeService {
        private readonly TimeSpan offset;

        public TestTimeService(IOptions<Configuration.Test.TimeBaseOptions> timeBaseOptions) {
            var startTime = timeBaseOptions.Value.ApplicationStartTime;
            offset = startTime == default ? TimeSpan.FromSeconds(0) : (startTime - DateTime.Now);
        }

        public DateTime Now => DateTime.Now.Add(offset);
    }
}