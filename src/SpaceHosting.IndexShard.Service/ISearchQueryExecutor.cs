using FluentValidation.Results;
using SpaceHosting.IndexShard.Models.ApiModels;

namespace SpaceHosting.IndexShard.Service
{
    public interface ISearchQueryExecutor
    {
        ValidationResult ValidateSearchQuery(SearchQueryDto query);
        SearchResultDto[] ExecuteSearchQuery(SearchQueryDto query);
    }
}
