using System.Collections.Generic;
using AppUpdate.FeedReaders;
using AppUpdate.Tasks;

namespace WinFormsProgressSample
{
	public class DummyReader : IUpdateFeedReader
	{
		public IList<IUpdateTask> Read(string feed)
		{
			return new List<IUpdateTask>
			       	{
			       		new LengthyTask {Description = "Some lengthy task to demo progress notifications"},
			       		new LengthyTask {Description = "Another lengthy task that doesn't really do anything"}
			       	};
		}
	}
}
