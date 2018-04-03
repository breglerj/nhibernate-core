﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System.Linq;
using NHibernate.Dialect;
using NHibernate.Linq;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH2328
{
	using System.Threading.Tasks;
	[TestFixture]
	public class FixtureAsync : BugTestCase
	{
		protected override bool AppliesTo(Dialect.Dialect dialect)
		{
			return !(dialect is AbstractHanaDialect); // HANA does not support inserting a row without specifying any column values
		}

		protected override void OnSetUp()
		{
			base.OnSetUp();

			using (ISession s = OpenSession())
			using (ITransaction t = s.BeginTransaction())
			{
				var circle = new Circle();
				var square = new Square();

				s.Save(circle);
				s.Save(square);

				s.Save(new ToyBox() { Name = "Box1", Shape = circle });
				s.Save(new ToyBox() { Name = "Box2", Shape = square });
				t.Commit();
			}
		}

		protected override void OnTearDown()
		{
			base.OnTearDown();

			using (ISession s = OpenSession())
			using (ITransaction t = s.BeginTransaction())
			{
				s.CreateQuery("delete from ToyBox").ExecuteUpdate();
				s.CreateQuery("delete from Circle").ExecuteUpdate();
				s.CreateQuery("delete from Square").ExecuteUpdate();
				t.Commit();
			}
		}

		[Test]
		public async Task AnyIs_QueryOverAsync()
		{
			using (ISession s = OpenSession())
			{
				var boxes =
					await (s.QueryOver<ToyBox>()
						.Where(t => t.Shape is Square)
						.ListAsync());

				Assert.That(boxes.Count, Is.EqualTo(1));
				Assert.That(boxes[0].Name, Is.EqualTo("Box2"));
			}
		}

		[Test]
		public async Task AnyIs_LinqAsync()
		{
			using (ISession s = OpenSession())
			{
				var boxes =
					await ((from t in s.Query<ToyBox>()
					 where t.Shape is Square
					 select t).ToListAsync());

				Assert.That(boxes.Count, Is.EqualTo(1));
				Assert.That(boxes[0].Name, Is.EqualTo("Box2"));
			}
		}

		[Test]
		public async Task AnyIs_HqlWorksWithClassNameInTheRightAsync()
		{
			using (ISession s = OpenSession())
			{
				var boxes =
					await (s.CreateQuery("from ToyBox t where t.Shape.class = Square")
						.ListAsync<ToyBox>());

				Assert.That(boxes.Count, Is.EqualTo(1));
				Assert.That(boxes[0].Name, Is.EqualTo("Box2"));
			}
		}

		[Test]
		public async Task AnyIs_HqlWorksWithClassNameInTheLeftAsync()
		{
			using (ISession s = OpenSession())
			{
				var boxes =
					await (s.CreateQuery("from ToyBox t where Square = t.Shape.class")
						.ListAsync<ToyBox>());

				Assert.That(boxes.Count, Is.EqualTo(1));
				Assert.That(boxes[0].Name, Is.EqualTo("Box2"));
			}
		}
	}
}
