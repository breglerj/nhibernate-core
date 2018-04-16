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
using NHibernate.Dialect;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH440
{
	using System.Threading.Tasks;
	/// <summary>
	///This is a test class for one_to_one_bug.Fruit and is intended
	///to contain all one_to_one_bug.Fruit Unit Tests
	///</summary>
	[TestFixture]
	public class FixtureAsync : BugTestCase
	{
		public override string BugNumber
		{
			get { return "NH440"; }
		}

		protected override IList Mappings
		{
			get { return new string[] {"NHSpecificTest.NH440.Fruit.hbm.xml", "NHSpecificTest.NH440.Apple.hbm.xml"}; }
		}

		protected override bool AppliesTo(Dialect.Dialect dialect)
		{
			return TestDialect.SupportsEmptyInserts;
		}


		protected override void OnSetUp()
		{
			base.OnSetUp();
			using (ISession session = OpenSession())
			using (ITransaction t = session.BeginTransaction())
			{
				// pump up the ids for one of the classes to avoid the tests passing coincidentally
				for (int i = 0; i < 10; i++)
				{
					session.Save(new Fruit());
				}

				session.Delete("from System.Object"); // clear everything from database
				t.Commit();
			}
		}

		protected override void OnTearDown()
		{
			using (ISession session = OpenSession())
			using (ITransaction t = session.BeginTransaction())
			{
				session.Delete("from System.Object"); // clear everything from database
				t.Commit();
			}
			base.OnTearDown();
		}

		[Test]
		public async Task StoreAndLookupAsync()
		{
			Apple apple = new Apple();
			Fruit fruit = new Fruit();

			using (ISession session = OpenSession())
			using (ITransaction tx = session.BeginTransaction())
			{
				await (session.SaveAsync(apple));
				await (session.SaveAsync(fruit));

				Assert.IsNotNull(await (session.GetAsync(typeof(Apple), apple.Id)));
				Assert.IsNotNull(await (session.GetAsync(typeof(Fruit), fruit.Id)));

				await (tx.CommitAsync());
			}
		}

		[Test]
		public async Task StoreWithLinksAndLookupAsync()
		{
			Apple apple = new Apple();
			Fruit fruit = new Fruit();

			apple.TheFruit = fruit;
			fruit.TheApple = apple;

			using (ISession session = OpenSession())
			using (ITransaction tx = session.BeginTransaction())
			{
				await (session.SaveAsync(apple));
				await (session.SaveAsync(fruit));

				await (tx.CommitAsync());
			}

			using (ISession session = OpenSession())
			using (ITransaction tx = session.BeginTransaction())
			{
				Apple apple2 = (Apple) await (session.GetAsync(typeof(Apple), apple.Id));
				Fruit fruit2 = (Fruit) await (session.GetAsync(typeof(Fruit), fruit.Id));

				Assert.IsNotNull(apple2);
				Assert.IsNotNull(fruit2);

				Assert.AreSame(apple2, fruit2.TheApple);
				Assert.AreSame(fruit2, apple2.TheFruit);
				await (tx.CommitAsync());
			}
		}

		[Test]
		public async Task StoreWithLinksAndLookupWithQueryFromFruitAsync()
		{
			Apple apple = new Apple();
			Fruit fruit = new Fruit();

			apple.TheFruit = fruit;
			fruit.TheApple = apple;

			using (ISession session = OpenSession())
			using (ITransaction tx = session.BeginTransaction())
			{
				await (session.SaveAsync(apple));
				await (session.SaveAsync(fruit));
				await (tx.CommitAsync());
			}

			using (ISession session = OpenSession())
			using (ITransaction tx = session.BeginTransaction())
			{
				Fruit fruit2 = (Fruit) await (session.GetAsync(typeof(Fruit), fruit.Id));
				Assert.IsNotNull(fruit2);
				IList results = await (session
					.CreateQuery("from Apple a where a.TheFruit = ?")
					.SetParameter(0, fruit2)
					.ListAsync());

				Assert.AreEqual(1, results.Count);
				Apple apple2 = (Apple) results[0];
				Assert.IsNotNull(apple2);

				Assert.AreSame(apple2, fruit2.TheApple);
				Assert.AreSame(fruit2, apple2.TheFruit);
				await (tx.CommitAsync());
			}
		}
	}
}
