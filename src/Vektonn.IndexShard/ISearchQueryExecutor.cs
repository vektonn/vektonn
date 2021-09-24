using FluentValidation.Results;
using Vektonn.Contracts.ApiModels;

namespace Vektonn.IndexShard
{
    public interface ISearchQueryExecutor
    {
        ValidationResult ValidateSearchQuery(SearchQueryDto query);
        SearchResultDto[] ExecuteSearchQuery(SearchQueryDto query);
    }
}
