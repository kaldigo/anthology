using Anthology.Data;
using Anthology.Plugins.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anthology.Services
{
    public class PersonService : IPersonService
    {
        DatabaseContext _context;

        public PersonService(DatabaseContext context)
        {
            _context = context;
        }

        public List<Person> GetPeople()
        {
            return _context.People.ToList();
        }
        public List<Person> GetAllPeople(Metadata metadata = null)
        {
            var bookMetadata = _context.Books.Select(b => b.BookMetadata).ToList();
            if(metadata != null) bookMetadata.Add(metadata);
            var metadataAuthorStrings = bookMetadata.SelectMany(b => b.Authors).ToList();
            var metadataAuthors = metadataAuthorStrings.Select(a => new Person()
                { Name = a });

            var metadataNarratorStrings = bookMetadata.SelectMany(b => b.Narrators).ToList();
            var metadataNarrators = metadataNarratorStrings.Select(n => new Person()
                { Name = n });

            var metadataPeople = CleanPeople(metadataAuthors.Concat(metadataNarrators).ToList());

            var dbPeople = _context.People.ToList();
            var allPeople = dbPeople.Concat(metadataPeople).DistinctBy(c => c.Name)
                .ToList();
            return allPeople;
        }

        public Person GetPerson(Guid guid)
        {
            return _context.People.First(c => c.ID == guid);
        }

        public Person? GetPerson(string name)
        {
            return _context.People.FirstOrDefault(c => c.Name.ToLower() == name.ToLower() || c.Aliases.Any(a => a.Name.ToLower() == name.ToLower()));
        }

        public void SavePerson(Person person, bool newPerson = false)
        {
            if (newPerson) _context.People.Add(person);
            else _context.People.Update(person);
            _context.SaveChanges();
        }

        public void DeletePerson(Person person)
        {
            _context.People.Remove(person);
            _context.SaveChanges();
        }

        public List<Person> CleanPeople(List<Person> people)
        {
            var cleanedPeople = new List<Person>();

            foreach (var item in people)
            {
                var classification = GetPerson(item.Name);

                if (classification != null) cleanedPeople.Add(classification);
                else cleanedPeople.Add(item);
            }

            return cleanedPeople.DistinctBy(c => c.Name).ToList();
        }
    }
}
