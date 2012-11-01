namespace SignalKo.SystemMonitor.Common.Model
{
	public class HttpResponseTimeCheckDefinition : ICollectorDefinition
	{
		public DataCollectorType CollectorType { get; set; }

		public int CheckIntervalInSeconds { get; set; }

		public string CheckUrl { get; set; }

		public string Hostheader { get; set; }

		public int MaxResponseTimeInMilliseconds { get; set; }

		public bool IsValid()
		{
			return CollectorType.Equals(DataCollectorType.HttpResponseTimeCheck) && CheckIntervalInSeconds > 0 && string.IsNullOrWhiteSpace(this.CheckUrl) == false
			       && this.MaxResponseTimeInMilliseconds > 0;
		}
	}
}