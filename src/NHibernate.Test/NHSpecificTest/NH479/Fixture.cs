using System;
using NHibernate.Dialect;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH479
{
	[TestFixture]
	public class Fixture : BugTestCase
	{
		public override string BugNumber
		{
			get { return "NH479"; }
		}

		protected override bool AppliesTo(Dialect.Dialect dialect)
		{
			return !(dialect is AbstractHanaDialect); // HANA does not support inserting a row without specifying any column values
		}

		[Test]
		public void MergeTest()
		{
			Main main = new Main();
			Aggregate aggregate = new Aggregate();

			main.Aggregate = aggregate;
			aggregate.Main = main;

			using (ISession s = OpenSession())
			using (ITransaction t = s.BeginTransaction())
			{
				s.Save(main);
				s.Save(aggregate);
				t.Commit();
			}

			using (ISession s = OpenSession())
			{
				using (ITransaction t = s.BeginTransaction())
				{
					s.Merge(main);
					s.Merge(aggregate);
					t.Commit();
				}

				using (ITransaction t = s.BeginTransaction())
				{
					s.Delete("from Aggregate");
					s.Delete("from Main");
					t.Commit();
				}
			}
		}
	}
}
