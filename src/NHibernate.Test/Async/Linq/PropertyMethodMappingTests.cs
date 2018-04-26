﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System.Linq;
using NUnit.Framework;
using NHibernate.Linq;

namespace NHibernate.Test.Linq
{
	using System.Threading.Tasks;
	[TestFixture]
	public class PropertyMethodMappingTestsAsync : LinqTestCase
	{
		protected override bool AppliesTo(Dialect.Dialect dialect)
		{
			return Dialect.SupportsScalarSubSelects && base.AppliesTo(dialect);
		}

		[Test]
		public async Task CanExecuteCountInSelectClauseAsync()
		{
			var results = await (db.Timesheets
				.Select(t => t.Entries.Count).OrderBy(s => s).ToListAsync());

			Assert.AreEqual(3, results.Count);
			Assert.AreEqual(0, results[0]);
			Assert.AreEqual(2, results[1]);
			Assert.AreEqual(4, results[2]);
		}

		[Test]
		public async Task CanExecuteCountInWhereClauseAsync()
		{
			var results = await (db.Timesheets
				.Where(t => t.Entries.Count >= 2).ToListAsync());

			Assert.AreEqual(2, results.Count);
		}
	}
}
