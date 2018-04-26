﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System.Collections;
using NUnit.Framework;

namespace NHibernate.Test.Generatedkeys.Seqidentity
{
	using System.Threading.Tasks;
	[TestFixture]
	public class SequenceIdentityFixtureAsync : TestCase
	{
		protected override IList Mappings
		{
			get { return new[] { "Generatedkeys.Seqidentity.MyEntity.hbm.xml" }; }
		}

		protected override string MappingsAssembly
		{
			get { return "NHibernate.Test"; }
		}

		protected override bool AppliesTo(Dialect.Dialect dialect)
		{
			return dialect.SupportsSequences && !(dialect is Dialect.MsSql2012Dialect) && !(dialect is Dialect.HanaDialectBase);
		}

		[Test]
		public async Task SequenceIdentityGeneratorAsync()
		{
			ISession session = OpenSession();
			session.BeginTransaction();

			var e = new MyEntity{Name="entity-1"};
			await (session.SaveAsync(e));

			// this insert should happen immediately!
			Assert.AreEqual(1, e.Id, "id not generated through forced insertion");

			await (session.DeleteAsync(e));
			await (session.Transaction.CommitAsync());
			session.Close();
		}
	}
}
