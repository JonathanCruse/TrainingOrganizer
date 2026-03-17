namespace TrainingOrganizer.Shared.Enums;

public enum Visibility { Public = 0, MembersOnly = 1, InviteOnly = 2 }
public enum TrainingStatus { Draft = 0, Published = 1, Canceled = 2, Completed = 3 }
public enum ParticipationStatus { Confirmed = 0, Waitlisted = 1, Canceled = 2, PendingApproval = 3 }
public enum RecurrencePattern { Weekly = 0, Biweekly = 1, Monthly = 2 }
public enum RecurringTrainingStatus { Active = 0, Paused = 1, Ended = 2 }
public enum SessionStatus { Scheduled = 0, Canceled = 1, Completed = 2 }
