namespace LibSumo.Net.lib.command
{
	public class CommandKey
	{
		private readonly byte projectId;
		private readonly byte clazzId;
		private readonly byte commandId;

		protected internal CommandKey(int projectId, int clazzId, int commandId)
		{
			this.projectId = (byte) projectId;
			this.clazzId = (byte) clazzId;
			this.commandId = (byte) commandId;
		}

		public static CommandKey commandKey(int projectId, int clazzId, int commandId)
		{
			return new CommandKey(projectId, clazzId, commandId);
		}

		public virtual byte ProjectId
		{
			get
			{
				return projectId;
			}
		}

		public virtual byte ClazzId
		{
			get
			{    
				return clazzId;
			}
		}

		public virtual byte CommandId
		{
			get
			{
				return commandId;
			}
		}
	}
}