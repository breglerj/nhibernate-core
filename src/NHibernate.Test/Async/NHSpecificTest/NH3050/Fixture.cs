﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using NHibernate.Engine.Query;
using NHibernate.Util;

using NUnit.Framework;
using System.Linq;
using NHibernate.Linq;

namespace NHibernate.Test.NHSpecificTest.NH3050
{
	[TestFixture]
	public class FixtureAsync : BugTestCase
	{
		protected override bool AppliesTo(Dialect.Dialect dialect)
		{
			return TestDialect.SupportsEmptyInsertsOrHasNonIdentityNativeGenerator;
		}

		[Test]
		public async Task TestAsync()
		{
			// WARNING: This test case makes use of reflection resulting in failures if internals change, but reflection was needed to allow for simulation.
			// This test simulates a heavy load on the QueryPlanCache by making it use a SoftLimitMRUCache instance with a size of 1 instead of the default of 128.
			// Since this cache moves the most recently used query plans to the top, pushing down the less used query plans until they are removed from the cache,
			// the smaller the size of the cache the sooner the query plans will be dropped, making it easier to simulate the problem.
			//
			// What is the exact problem:
			// -> When executing a LINQ query that has a contains with only one element in the collection the same queryExpression string is generated by 2 different types
			//    of IQueryExpression, the 'NHibernate.Impl.ExpandedQueryExpression' and the 'NHibernate.Linq.NhLinqExpression' and that key is used to store a query plan
			//    in the QueryPlanCache if a query plan is requested and not found in the cache.
			// -> The 'NHibernate.Linq.NhLinqExpression' is typically added during the DefaultQueryProvider.PrepareQuery and the 'NHibernate.Impl.ExpandedQueryExpression' 
			//    less likely during the execution of the LINQ query
			// -> Unfortunately the PrepareQuery is casting the returned query plan's QueryExpression to a NhLinqExpression, which it assumes will always be the case, but this
			//    is not true in a heavy loaded environment where the cache entries are constantly moving when other queries are being executed at the same time.
			// -> If you look at the following method inside the DefaultQueryProvider class, then you'll see that by drilling down in the PrepareQuery and in the ExecuteQuery, that
			//    both operations are actually requesting the query plan from the QueryPlanCache at some point
			//    public virtual object Execute(Expression expression)
			//    {
			//      IQuery query;
			//      NhLinqExpression nhQuery;
			//      NhLinqExpression nhLinqExpression = PrepareQuery(expression, out query, out nhQuery);
			//      return ExecuteQuery(nhLinqExpression, query, nhQuery);
			//    }
			//    
			//    When they are requesting the corresponding query plan according to the QueryExpression's key the PrepareQuery assumes it will get back a NhLinqExpression, while it
			//    is perfectly possible that the corresponding query plan has a QueryExpression of type ExpandedQueryExpression that has been added during the ExecuteQuery because
			//    when a request was made for the query plan during the execution, the load on the cache has put the query plan with a QueryExpression of type NhLinqExpression and with 
			//    the same key somewhere at the bottom of the MRU cache and it might even have been removed from the cache, resulting in adding a query plan with a QueryExpression value
			//    of type ExpandedQueryExpression. When the same LINQ query is executed afterwards, it will go through the PrepareQuery again, assuming that what is returned is a 
			//    NhLinqExpression, while in reality it is an ExpandedQueryExpression, resulting in a cast exception. This problem might even go away due to the same load, pushing out 
			//    the cached query plan with a QueryExpression of ExpandedQueryExpression and have a NhLinqExpression added back again during the next Prepare.
			//
			//    So this test will simulate the pushing out by clearing the cache as long as the QueryExpression of the query plan is NhLinqExpression, once it is an ExpandedQueryExpression
			//    it will stop clearing the cache, and the exception will occur, resulting in a failure of the test. 
			//    The test will pass once all LINQ expression are executed (1000 max) and no exception occured

			var cache = new SoftLimitMRUCache(1);

			var queryPlanCacheType = typeof (QueryPlanCache);

			// get the planCache field on the QueryPlanCache and overwrite it with the restricted cache
			queryPlanCacheType
				.GetField("planCache", BindingFlags.Instance | BindingFlags.NonPublic)
				.SetValue(Sfi.QueryPlanCache, cache);

			// Initiate a LINQ query with a contains with one item in it, of which we know that the underlying IQueryExpression implementations
			// aka NhLinqExpression and the ExpandedQueryExpression generate the same key.
			IEnumerable<int> personIds = new List<int>
				{
					1
				};

			ISession session = null;

			try
			{
				session = OpenSession();

				var allLinqQueriesSucceeded = false;

				// Setup an action delegate that will be executed on a separate thread and that will execute the LINQ query above multiple times.
				// This will constantly interact with the cache (Once in the PrepareQuery method of the DefaultQueryProvider and once in the Execute)
				System.Action queryExecutor = () =>
					{
						var sessionToUse = Sfi.OpenSession();

						try
						{
							for (var i = 0; i < 1000; i++)
							{
								(from person in session.Query<Person>()
								 where personIds.Contains(person.Id)
								 select person).ToList();
							}

							allLinqQueriesSucceeded = true;
						}
						finally
						{
							if (sessionToUse != null && sessionToUse.IsOpen)
							{
								sessionToUse.Close();
							}
						}
					};

				await ((from person in session.Query<Person>()
				 where personIds.Contains(person.Id)
				 select person).ToListAsync());

				// the planCache now contains one item with a key of type HQLQueryPlanKey, 
				// so we are going to retrieve the generated key so that we can use it afterwards to interact with the cache.
				// The softReferenceCache field value from the SoftLimitMRUCache cache instance contains this key
				var field = cache.GetType().GetField("softReferenceCache", BindingFlags.NonPublic | BindingFlags.Instance);

				var softReferenceCache = (IEnumerable) field.GetValue(cache);

				// Since the cache only contains one item, the first one will be our key
				var queryPlanCacheKey = ((DictionaryEntry) softReferenceCache.Cast<object>().First()).Key;

				// Setup an action that will be run on another thread and that will do nothing more than clearing the cache as long
				// as the value stored behind the cachekey is not of type ExpandedQueryExpression, which triggers the error.
				// By running this constantly in concurrency with the thread executing the query, the odds of having the wrong
				// QueryExpression in the cache (wrong as in the PrepareQuery is not expecting it) augments, simulating the workings 
				// of the MRU algorithm under load.
				System.Action cacheCleaner = () =>
					{
						while (!allLinqQueriesSucceeded)
						{
							var hqlExpressionQueryPlan = (QueryExpressionPlan) cache[queryPlanCacheKey];
							if (hqlExpressionQueryPlan != null)
							{
								if (hqlExpressionQueryPlan.QueryExpression.GetType().FullName.Contains("NHibernate.Impl.ExpandedQueryExpression"))
								{
									// we'll stop clearing the cache, since the cache now has a different query expression type than expected by the code
									break;
								}
							}

							cache.Clear();

							// we sleep a little, just to make sure the cache is not constantly empty ;-)
							Thread.Sleep(50);
						}
					};

				var queryExecutorTask = Task.Run(queryExecutor);
				var cacheCleanerTask = Task.Run(cacheCleaner);

				queryExecutorTask.Wait();
				cacheCleanerTask.Wait();

				Assert.IsTrue(allLinqQueriesSucceeded);
			}
			finally
			{
				if (session != null)
				{
					session.Close();
				}
			}
		}
	}
}
