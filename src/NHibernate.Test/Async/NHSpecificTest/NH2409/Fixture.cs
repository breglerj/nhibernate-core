﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using System.Linq;
using NHibernate.Criterion;
using NHibernate.Dialect;
using NHibernate.SqlCommand;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH2409
{
	using System.Threading.Tasks;
	[TestFixture]
	public class FixtureAsync : BugTestCase
	{
		protected override bool AppliesTo(Dialect.Dialect dialect)
		{
			return !(dialect is AbstractHanaDialect); // HANA does not support inserting a row without specifying any column values
		}

		[Test]
		public async Task BugAsync()
		{
			using (var session = OpenSession())
			using (var tx = session.BeginTransaction())
			{
				var contest1 = new Contest {Id = 1};
				var contest2 = new Contest {Id = 2};
				var user = new User();

				var message = new Message {Contest = contest2 };

				await (session.SaveAsync(contest1));
				await (session.SaveAsync(contest2));
				await (session.SaveAsync(user));

				await (session.SaveAsync(message));
				await (tx.CommitAsync());
			}

			using (var session = OpenSession())
			{
				var contest2 = await (session.CreateCriteria<Contest>().Add(Restrictions.IdEq(2)).UniqueResultAsync<Contest>());
				var user = (await (session.CreateCriteria<User>().ListAsync<User>())).Single();

				var msgs = await (session.CreateCriteria<Message>()
					.Add(Restrictions.Eq("Contest", contest2))
					.CreateAlias("Readings", "mr", JoinType.LeftOuterJoin, Restrictions.Eq("mr.User", user))
					.ListAsync<Message>());
				
				Assert.AreEqual(1, msgs.Count, "We should be able to find our message despite any left outer joins");
			}
		}

		protected override void OnTearDown()
		{
			using (var session = OpenSession())
			using (var tx = session.BeginTransaction())
			{
				session.Delete("from Contest");
				session.Delete("from User");
				session.Delete("from Message");
				session.Delete("from MessageReading");
				tx.Commit();
			}
			base.OnTearDown();
		}
	}
}
