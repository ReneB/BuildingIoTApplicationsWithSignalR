using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

using Server.Configuration;
using Server.Hubs;
using Server.Models;
using Server.Services;

namespace Server.HostedServices {
    public class ScheduleProcessor : IHostedService {
        private readonly ILogger<ScheduleProcessor> logger;
        private readonly AirportOptions airportOptions;
        private Timer timer = null!;
        private readonly ScheduleFetcher scheduleFetcher;
        private readonly int GateOpenAnnouncementTimeOffsetInMinutes = -60;
        private readonly int FinalCallAnnouncementTimeOffsetInMinutes = -30;
        private readonly AnnouncementLog logbook;
        private readonly ITimeService timeService;
        private readonly IHubContext<MainHub, IDevice> hubContext;
        private readonly AnnouncementAudioCreator announcementAudioCreator;
        private readonly GateToGroupMap gateToGroupMap;

        public ScheduleProcessor(ILogger<ScheduleProcessor> logger,
                                 ScheduleFetcher scheduleFetcher,
                                 IOptions<AirportOptions> airportOptions,
                                 AnnouncementLog logbook,
                                 ITimeService timeService,
                                 IHubContext<MainHub, IDevice> hubContext,
                                 AnnouncementAudioCreator announcementAudioCreator,
                                 GateToGroupMap gateToGroupMap) {
            this.logger = logger;
            this.scheduleFetcher = scheduleFetcher;
            this.airportOptions = airportOptions.Value;
            this.logbook = logbook;
            this.timeService = timeService;
            this.hubContext = hubContext;
            this.announcementAudioCreator = announcementAudioCreator;
            this.gateToGroupMap = gateToGroupMap;
        }

        public Task StartAsync(CancellationToken cancellationToken) {
            logger.LogInformation("Starting up ScheduleProcessor.");

            timer = new Timer(async(_) => await ProcessSchedule(), null, TimeSpan.Zero, TimeSpan.FromSeconds(60));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken _) {
            logger.LogInformation("ScheduleProcessor background service is stopping.");

            timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        protected virtual async Task ProcessSchedule() {
            logger.LogInformation("ScheduleProcessor background service is processing schedule.");

            var flights = await scheduleFetcher.FetchSchedule();

            var upcomingAnnouncements = flights
                .SelectMany(ExtractPlaneMovements)
                .Where(movement => movement.Airport == airportOptions.Current)
                .SelectMany(PrepareAnnouncements)
                .Where(announcement => announcement.Timestamp < timeService.Now && !logbook.HasBeenBroadcast(announcement))
                .OrderBy(announcement => announcement.Timestamp);

            foreach (var announcement in upcomingAnnouncements) {
                var success = await DispatchAnnouncement(announcement);

                if (success) {
                    logbook.RegisterAsDone(announcement);
                }
            }
        }

        protected IEnumerable<PlaneMovement> ExtractPlaneMovements(Flight record) {
            var depTime = new DateTime(record.Year, record.Month, record.DayOfMonth, record.CRSDepTime / 100, record.CRSDepTime % 100, 0);
            var departure = new Departure {
                Timestamp = depTime,
                Airport = record.Origin!,
                Gate = record.DepGate,
                FlightNumber = $"{record.UniqueCarrier}{record.FlightNum}",
                Destination = record.DestCommonName!,
            };

            var arrTime = new DateTime(record.Year, record.Month, record.DayOfMonth, record.CRSArrTime / 100, record.CRSArrTime % 100, 0);
            var arrival = new Arrival {
                Timestamp = arrTime,
                Airport = record.Dest!,
                Gate = record.ArrGate,
                FlightNumber = $"{record.UniqueCarrier}{record.FlightNum}",
                Origin = record.OriginCommonName!,
            };

            return new List<PlaneMovement> {
                departure,
                arrival
            };
        }

        protected IEnumerable<Announcement> PrepareAnnouncements(PlaneMovement p) {
            switch (p) {
                case Arrival a: {
                    return new List<Announcement> {
                        new ArrivalAnnouncement(a) {
                            Timestamp = p.Timestamp,
                        }
                    };
                } case Departure d: {
                    return new List<Announcement> {
                        new GateOpenAnnouncement(d) {
                            Timestamp = p.Timestamp.Add(TimeSpan.FromMinutes(GateOpenAnnouncementTimeOffsetInMinutes)),
                        },
                        new FinalCallAnnouncement(d) {
                            Timestamp = p.Timestamp.Add(TimeSpan.FromMinutes(FinalCallAnnouncementTimeOffsetInMinutes)),
                        },
                    };
                } default: {
                    return new List<Announcement>();
                }
            }
        }

        protected async Task<bool> DispatchAnnouncement(Announcement announcement) {
            var audioFileName = await announcementAudioCreator.CreateAudioFor(announcement);

            if (audioFileName == null) {
                return false;
            }

            switch (announcement) {
                case ArrivalAnnouncement: {
                    await hubContext.Clients.All.Announce(announcement.Id, announcement.Text, File.ReadAllBytes(audioFileName));
                    break;
                }
                default: {
                    var groupName = gateToGroupMap.GroupForGate(announcement.Gate);

                    if (groupName != null) {
                        await hubContext.Clients.Group(groupName).Announce(announcement.Id, announcement.Text, File.ReadAllBytes(audioFileName));
                    }

                    break;
                }
            }

            return true;
        }
    }
}