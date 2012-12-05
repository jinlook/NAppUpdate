using System.Collections.Generic;
using AppUpdate.Tasks;

namespace AppUpdate.FeedReaders
{
    public interface IUpdateFeedReader
    {
        IList<IUpdateTask> Read(string feed);
    }
}
