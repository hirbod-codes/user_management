namespace user_management.Data.Logics.Filter;

using MongoDB.Driver;

public interface IFilterLogic<TDocument>
{
    public FilterDefinition<TDocument> BuildDefinition();
}