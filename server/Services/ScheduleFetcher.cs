using System.Globalization;
using CsvHelper;

using Microsoft.Extensions.Options;

using Server.Configuration;
using Server.Models;

namespace Server.Services {
    public class ScheduleFetcher {
        private readonly ScheduleOptions scheduleOptions;

        public ScheduleFetcher(IOptions<ScheduleOptions> scheduleOptions) {
            this.scheduleOptions = scheduleOptions.Value;
        }

        public async Task<IEnumerable<Flight>> FetchSchedule() {
            var client = new HttpClient();

            var scheduleData = await client.GetStringAsync(new Uri(scheduleOptions.Url));

            using var reader = new StringReader(scheduleData);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            return csv.GetRecords<Flight>().ToList();
        }
    }
}
