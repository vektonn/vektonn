using FluentValidation.Results;
using Vektonn.ApiContracts;

namespace Vektonn.IndexShard
{
    public interface ISearchQueryExecutor
    {
        ValidationResult ValidateSearchQuery(SearchQueryDto query);
        SearchResultDto[] ExecuteSearchQuery(SearchQueryDto query);
    }
}
