using System;
using System.Collections.Generic;
using NUnit.Framework;
using ServiceStack.Webhooks.Azure.Table;

namespace ServiceStack.Webhooks.Azure.UnitTests.Table
{
    public class TableStorageQuerySpec
    {
        [TestFixture]
        public class GivenAContext
        {
            [SetUp]
            public void Initialize()
            {
            }

            [Test, Category("Unit")]
            public void WhenCtorWithNullParts_ThenThrows()
            {
                Assert.Throws<ArgumentNullException>(() => new TableStorageQuery(null));
            }

            [Test, Category("Unit")]
            public void WhenCtorWithNullPropertyName_ThenThrows()
            {
                Assert.Throws<ArgumentNullException>(() => new TableStorageQuery(null, QueryOperator.EQ, null));
            }

            [Test, Category("Unit")]
            public void WhenCtorWithSingleQuery_ThenCreatesQuery()
            {
                var result = new TableStorageQuery("apropertyname", QueryOperator.EQ, "avalue");

                Assert.That(result.Parts.Count, Is.EqualTo(1));
                Assert.That(result.Parts[0].PropertyName, Is.EqualTo("apropertyname"));
                Assert.That(result.Parts[0].Operation, Is.EqualTo(QueryOperator.EQ));
                Assert.That(result.Parts[0].Value, Is.EqualTo("avalue"));
                Assert.That(result.Query, Is.EqualTo("apropertyname eq 'avalue'"));
            }

            [Test, Category("Unit")]
            public void WhenCtorWithMultipleParts_ThenCreatesQuery()
            {
                var result = new TableStorageQuery(new List<QueryPart>
                {
                    new QueryPart("apropertyname1", QueryOperator.EQ, "avalue1"),
                    new QueryPart("apropertyname2", QueryOperator.EQ, "avalue2")
                });

                Assert.That(result.Parts.Count, Is.EqualTo(2));
                Assert.That(result.Parts[0].PropertyName, Is.EqualTo("apropertyname1"));
                Assert.That(result.Parts[0].Operation, Is.EqualTo(QueryOperator.EQ));
                Assert.That(result.Parts[0].Value, Is.EqualTo("avalue1"));
                Assert.That(result.Parts[1].PropertyName, Is.EqualTo("apropertyname2"));
                Assert.That(result.Parts[1].Operation, Is.EqualTo(QueryOperator.EQ));
                Assert.That(result.Parts[1].Value, Is.EqualTo("avalue2"));
                Assert.That(result.Query, Is.EqualTo("(apropertyname1 eq 'avalue1') and (apropertyname2 eq 'avalue2')"));
            }

            [Test, Category("Unit")]
            public void WhenCtorWithDateQuery_ThenCreatesDateTimeQuery()
            {
                var datum = DateTime.Today;
                var result = new TableStorageQuery("apropertyname", QueryOperator.EQ, datum);

                Assert.That(result.Parts.Count, Is.EqualTo(1));
                Assert.That(result.Parts[0].PropertyName, Is.EqualTo("apropertyname"));
                Assert.That(result.Parts[0].Operation, Is.EqualTo(QueryOperator.EQ));
                Assert.That(result.Parts[0].Value, Is.EqualTo(datum));
                Assert.That(result.Query, Is.EqualTo("apropertyname eq datetime'{0}'".Fmt(datum.ToUniversalTime().ToIso8601())));
            }

            [Test, Category("Unit")]
            public void WhenCtorWithDateQueryAndDateLessThanAzureMinDate_ThenCreatesDateTimeQuery()
            {
                var datum = DateTime.MinValue;
                var result = new TableStorageQuery("apropertyname", QueryOperator.EQ, datum);

                Assert.That(result.Parts.Count, Is.EqualTo(1));
                Assert.That(result.Parts[0].PropertyName, Is.EqualTo("apropertyname"));
                Assert.That(result.Parts[0].Operation, Is.EqualTo(QueryOperator.EQ));
                Assert.That(result.Parts[0].Value, Is.EqualTo(datum));
                Assert.That(result.Query, Is.EqualTo("apropertyname eq datetime'{0}'".Fmt(Azure.Table.DateTimeExtensions.MinAzureDateTime.ToIso8601())));
            }
        }
    }
}