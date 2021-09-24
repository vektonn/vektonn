using System;
using System.Collections.Generic;
using System.Threading;
using Vektonn.Contracts;
using Vektonn.Index;

namespace Vektonn.IndexShard
{
    internal class IndexWithLocker<TVector> : IDisposable
        where TVector : IVector
    {
        private readonly IIndexStore<byte[], byte[], TVector> indexStore;
        private readonly ReaderWriterLockSlim locker = new ReaderWriterLockSlim();

        public IndexWithLocker(IIndexStore<byte[], byte[], TVector> indexStore)
        {
            this.indexStore = indexStore;
        }

        public long DataPointsCount => indexStore.Count;

        public long UpdateIndex(IndexDataPointOrTombstone<byte[], byte[], TVector>[] dataPointOrTombstones)
        {
            try
            {
                locker.EnterWriteLock();
                indexStore.UpdateIndex(dataPointOrTombstones);
                return indexStore.Count;
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }

        public IReadOnlyList<IndexSearchResultItem<byte[], byte[], TVector>> FindNearest(SearchQuery<TVector> query)
        {
            try
            {
                locker.EnterReadLock();
                return indexStore.FindNearest(query.QueryVectors, query.K);
            }
            finally
            {
                locker.ExitReadLock();
            }
        }

        public void Dispose()
        {
            try
            {
                locker.EnterWriteLock();
                indexStore.Dispose();
            }
            finally
            {
                locker.ExitWriteLock();
            }

            locker.Dispose();
        }
    }
}
