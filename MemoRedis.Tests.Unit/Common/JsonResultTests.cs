using System;
using System.Text.Json;
using MemoRedis.API.Common;
using Xunit;

namespace MemoRedis.Tests.Unit.Common
{
    public class JsonResultTests
    {
        private record Person(string Name, int Age);


        [Fact]
        public void ShouldSerializeImplicitly()
        {
            var person = new Person(Name: "Muhammad.M", Age: 512);

            JsonResult<Person> personJsonResult = person;
            string expectedJsonString = JsonSerializer.Serialize(person);

            Assert.Equal<string>(expectedJsonString, personJsonResult.JsonData);
        }

        [Fact]
        public void ShouldDeserializeImplicitly()
        {
            var person = new Person(Name: "Muhammad.M", Age: 512);

            JsonResult<Person> personJsonResult = person;
            Person personData = personJsonResult;

            Assert.Equal<int>(person.Age, personData.Age);
            Assert.Equal<string>(person.Name, personData.Name);
        }

        [Fact]
        public void ShouldThrowArgumentNullException()
        {
            Person person = null!;

            Assert.Throws<ArgumentNullException>(() =>
            {
                JsonResult<Person> personJsonResult = person;
            });
        }
    }
}