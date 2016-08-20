using System.Collections.Generic;

namespace ApiWithTokens.Models
{
    public class Person
    {
        public string Name { get; set; }
    }

    public class PersonDatabase : List<Person>
    {
        public PersonDatabase()
        {
            Add(new Person() { Name = "Richard" });
            Add(new Person() { Name = "Craig" });
            Add(new Person() { Name = "Kevin" });
            Add(new Person() { Name = "Pete" });
        }
    }
}