//-----------------------------------------------------------------------
// <copyright file="StorageActionsAccessor.cs" company="Hibernating Rhinos LTD">
//     Copyright (c) Hibernating Rhinos LTD. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System;
using Microsoft.Isam.Esent.Interop;
using Raven.Database.Storage;
using Raven.Storage.Esent.StorageActions;

namespace Raven.Storage.Esent
{
	[CLSCompliant(false)]
	public class StorageActionsAccessor : IStorageActionsAccessor
	{
		private readonly DocumentStorageActions inner;

		[CLSCompliant(false)]
		public StorageActionsAccessor(DocumentStorageActions inner)
		{
			this.inner = inner;
		}

		public ITransactionStorageActions Transactions
		{
			get { return inner; }
		}

		public IDocumentStorageActions Documents
		{
			get { return inner; }
		}

		public IQueueStorageActions Queue
		{
			get { return inner; }
		}

		public ITasksStorageActions Tasks
		{
			get { return inner; }
		}

		public IStalenessStorageActions Staleness
		{
			get { return inner; }
		}

		public IAttachmentsStorageActions Attachments
		{
			get { return inner; }
		}

		public IIndexingStorageActions Indexing
		{
			get { return inner; }
		}

		public IGeneralStorageActions General
		{
			get { return inner; }
		}

		public IMappedResultsStorageAction MappedResults
		{
			get { return inner; }
		}

		public event Action OnCommit
		{
			add { inner.OnCommit += value; }
			remove { inner.OnCommit -= value; }
		}

		public bool IsWriteConflict(Exception exception)
		{
			var esentErrorException = exception as EsentErrorException;
			if (esentErrorException == null)
				return false;
			switch (esentErrorException.Error)
			{
				case JET_err.WriteConflict:
				case JET_err.SessionWriteConflict:
				case JET_err.WriteConflictPrimaryIndex:
					return true;
				default:
					return false;
			}
		}
	}
}
