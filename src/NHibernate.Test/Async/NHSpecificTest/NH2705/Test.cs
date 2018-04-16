﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System.Collections.Generic;
using System.Linq;
using NHibernate.Linq;
using NUnit.Framework;

// ReSharper disable InconsistentNaming

namespace NHibernate.Test.NHSpecificTest.NH2705
{
	using System.Threading.Tasks;
	using System.Threading;
	[TestFixture]
	public class TestAsync : BugTestCase
	{
		private static async Task<IEnumerable<T>> GetAndFetchAsync<T>(string name, ISession session, CancellationToken cancellationToken = default(CancellationToken)) where T : ItemBase
		{
			// this is a valid abstraction, the calling code should be able to ask that a property is eagerly loaded/available
			// without having to know how it is mapped
			return await (session.Query<T>()
				.Fetch(p => p.SubItem).ThenFetch(p => p.Details) // should be able to fetch .Details when used with components (NH2615)
				.Where(p => p.SubItem.Name == name).ToListAsync(cancellationToken));
		}

		protected override bool AppliesTo(Dialect.Dialect dialect)
		{
			return TestDialect.SupportsEmptyInsertsOrHasNonIdentityNativeGenerator;
		}

		[Test]
		public void Fetch_OnComponent_ShouldNotThrowAsync()
		{
			using (ISession s = OpenSession())
			{
				Assert.That(() => GetAndFetchAsync<ItemWithComponentSubItem>("hello", s), Throws.Nothing);
			}
		}

		[Test]
		public void HqlQueryWithFetch_WhenDerivedClassesUseComponentAndManyToOne_DoesNotGenerateInvalidSqlAsync()
		{
			using (ISession s = OpenSession())
			{
				using (var log = new SqlLogSpy())
				{
					Assert.That(() => s.CreateQuery("from ItemWithComponentSubItem i left join fetch i.SubItem").ListAsync(), Throws.Nothing);
				}
			}
		}

		[Test]
		public void HqlQueryWithFetch_WhenDerivedClassesUseComponentAndEagerFetchManyToOne_DoesNotGenerateInvalidSqlAsync()
		{
			using (ISession s = OpenSession())
			{
				using (var log = new SqlLogSpy())
				{
					Assert.That(() => s.CreateQuery("from ItemWithComponentSubItem i left join fetch i.SubItem.Details").ListAsync(), Throws.Nothing);
				}
			}
		}

		[Test]
		public void LinqQueryWithFetch_WhenDerivedClassesUseComponentAndManyToOne_DoesNotGenerateInvalidSqlAsync()
		{
			using (ISession s = OpenSession())
			{
				using (var log = new SqlLogSpy())
				{
					Assert.That(() => s.Query<ItemBase>()
									   .Fetch(p => p.SubItem).ToListAsync(), Throws.Nothing);


					// fetching second level properties should work too
					Assert.That(() => s.Query<ItemWithComponentSubItem>()
									   .Fetch(p => p.SubItem).ThenFetch(p => p.Details).ToListAsync(), Throws.Nothing);
				}
			}
		}

		[Test, Ignore("Locked by re-linq")]
		public void LinqQueryWithFetch_WhenDerivedClassesUseComponentAndEagerFetchManyToOne_DoesNotGenerateInvalidSqlAsync()
		{
			using (ISession s = OpenSession())
			{
				using (var log = new SqlLogSpy())
				{
					Assert.That(() => s.Query<ItemWithComponentSubItem>().Fetch(p => p.SubItem.Details).ToListAsync(), Throws.Nothing);
				}
			}
		}
	}
}
