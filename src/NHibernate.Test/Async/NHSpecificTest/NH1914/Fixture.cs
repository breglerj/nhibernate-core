﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using System.Collections.Generic;
using NHibernate.Dialect;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH1914
{
	using System.Threading.Tasks;
	[TestFixture]
	public class FixtureAsync : BugTestCase
	{
		protected override bool AppliesTo(Dialect.Dialect dialect)
		{
			return TestDialect.SupportsEmptyInserts;
		}

		[Test]
		public async Task CascadeInsertAssignedAsync()
		{
			IDS _IDS = new IDS();
			_IDS.Identifier = Guid.NewGuid().ToString();
			_IDS.Name = "IDS";
			_IDS.CRSPLUTs = new Dictionary<String, ListOfHLUT>();
			_IDS.CRSPLUTs.Add("a", new ListOfHLUT());


			HLUT _HLUT = new HLUT();
			_HLUT.Identifier = 1123;
			_HLUT.Name = "HLUT";
			_HLUT.Entries = new List<Entry>();
			_HLUT.Entries.Add(new Entry(1.1, .1));
			_HLUT.Entries.Add(new Entry(2.2, .2));

			_IDS.CRSPLUTs["a"].Values.Add(_HLUT);

			using (ISession s = OpenSession())
			{
				ITransaction t = s.BeginTransaction();
				await (s.SaveAsync(_IDS));
				await (t.CommitAsync());
			}

			using (ISession s = OpenSession())
			{
				ITransaction t = s.BeginTransaction();
				IDS _IDSRead = await (s.LoadAsync<IDS>(_IDS.Identifier));
				
				Assert.IsNotNull(_IDSRead);
				Assert.IsNotNull(_IDSRead.CRSPLUTs);
				Assert.IsNotNull(_IDSRead.CRSPLUTs["a"]);
				Assert.IsNotNull(_IDSRead.CRSPLUTs["a"].Values[0]);
				Assert.IsNotNull(_IDSRead.CRSPLUTs["a"].Values[0].Entries);

				await (s.DeleteAsync(_IDSRead));
				await (t.CommitAsync());
			}
		}

	}
}
