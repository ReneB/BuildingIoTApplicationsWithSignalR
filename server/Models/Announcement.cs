namespace Server.Models {
    public abstract record Announcement {
        public abstract string Id { get; }
        public DateTime Timestamp { get; set; }
        public abstract string Text { get; }
    }

    public abstract record Announcement<T> : Announcement where T : PlaneMovement {
        protected T planeMovement;

        public override string Id => $"{planeMovement.Id}-{GetType().Name}";

        protected Announcement(T planeMovement) {
            this.planeMovement = planeMovement;
        }

    }
    public record ArrivalAnnouncement : Announcement<Models.Arrival> {
        public ArrivalAnnouncement(Arrival arr) : base(arr) {}

        public override string Text {
            get => $"Now arriving at gate {planeMovement.Gate} is flight {planeMovement.FlightNumber} from {planeMovement.Origin}.";
        }
    }

    public record GateOpenAnnouncement : Announcement<Departure> {
        public GateOpenAnnouncement(Departure d) : base(d) {}

        public override string Text {
            get => $"Would all passengers travelling to {planeMovement.Destination} on flight {planeMovement.FlightNumber} please have your boarding passes and passports ready for boarding. Flight {planeMovement.FlightNumber} now boarding at gate {planeMovement.Gate}.";
        }
    }

    public record FinalCallAnnouncement : Announcement<Departure> {
        public FinalCallAnnouncement(Departure d) : base(d) {}

        public override string Text {
            get => $"This is the final boarding call for passengers flying to {planeMovement.Destination} on flight {planeMovement.FlightNumber}. Please prepare to board at gate {planeMovement.Gate} immediately. The doors of the plane will close in five minutes. Final boarding call for passengers on flight {planeMovement.FlightNumber}.";
        }
    }
}