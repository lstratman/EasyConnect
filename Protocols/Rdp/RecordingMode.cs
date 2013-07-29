namespace EasyConnect.Protocols.Rdp
{
	/// <summary>
	/// Indicates whether sound recording should be possible within the remote system.
	/// </summary>
	public enum RecordingMode
	{
		/// <summary>
		/// Recording source should come from the local computer.
		/// </summary>
		RecordFromThisComputer = 0,

		/// <summary>
		/// No recording should be possible.
		/// </summary>
		DoNotRecord = 1
	}
}