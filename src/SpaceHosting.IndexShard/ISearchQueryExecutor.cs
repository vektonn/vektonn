using FluentValidation.Results;
using SpaceHosting.Contracts.ApiModels;

namespace SpaceHosting.IndexShard
{
    public interface ISearchQueryExecutor
    {
        ValidationResult ValidateSearchQuery(SearchQueryDto query);
        SearchResultDto[] ExecuteSearchQuery(SearchQueryDto query);
    }
}
