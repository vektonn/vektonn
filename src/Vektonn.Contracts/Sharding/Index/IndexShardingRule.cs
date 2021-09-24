namespace Vektonn.Contracts.Sharding.Index
{
    // note (andrew, 04.08.2021): 'AnyValue' rule is identical to 'BelongToComplementSet(EMPTY_SET)'
    public enum IndexShardingRule
    {
        BelongToSet = 0,
        BelongToComplementSet = 1,
    }
}
