using AppUpdate.Common;

namespace AppUpdate.Conditions
{
    public interface IUpdateCondition : INauFieldsHolder
    {
        bool IsMet(Tasks.IUpdateTask task);
    }
}
