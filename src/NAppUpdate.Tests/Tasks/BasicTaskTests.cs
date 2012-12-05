using Microsoft.VisualStudio.TestTools.UnitTesting;
using AppUpdate.Tasks;

namespace NAppUpdate.Tests.Tasks
{
	[TestClass]
	public class BasicTaskTests
	{
		[TestMethod]
		public void TestTaskDefaultCharacteristics()
		{
			var task = new FileUpdateTask(); // just a random task object
			Assert.IsTrue(task.ExecutionStatus == TaskExecutionStatus.Pending);
		}
	}
}
