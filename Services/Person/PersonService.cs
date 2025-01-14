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
        private static List<Person> _metadataPeopleCache;
        private static Task _refreshMetadataPeopleTask;
        private static bool _isCached;

        public PersonService(DatabaseContext context)
        {
            _context = context;
        }

        public List<Person> GetPeople()
        {
            return _context.People.ToList();
        }
        private static Task<string> RefreshMetadataPeopleTask(DatabaseContext context, PersonService instance)
        {
            if (!(_refreshMetadataPeopleTask != null && (_refreshMetadataPeopleTask.Status == TaskStatus.Running || _refreshMetadataPeopleTask.Status == TaskStatus.WaitingToRun || _refreshMetadataPeopleTask.Status == TaskStatus.WaitingForActivation)))
            {
                _refreshMetadataPeopleTask = Task.Factory.StartNew(() =>
                {
                    using (HttpClient client = new HttpClient())
                    {
                        var bookMetadata = context.Books.Select(b => b.BookMetadata).ToList();

                        var metadataAuthorStrings = bookMetadata.SelectMany(b => b.Authors).ToList();
                        var metadataAuthors = metadataAuthorStrings.Select(a => new Person()
                        { Name = a });

                        var metadataNarratorStrings = bookMetadata.SelectMany(b => b.Narrators).ToList();
                        var metadataNarrators = metadataNarratorStrings.Select(n => new Person()
                        { Name = n });

                        var metadataPeople = instance.CleanPeople(metadataAuthors.Concat(metadataNarrators).ToList());

                        _metadataPeopleCache = metadataPeople;
                        _isCached = true;
                    }
                });
            }
            _refreshMetadataPeopleTask.Wait();
            return Task.FromResult("Refresh complete");
        }

        public void RefreshMetadataPeople()
        {
            RefreshMetadataPeopleTask(_context, this);
        }
        public List<Person> GetAllPeople(Metadata metadata = null)
        {
            if (_isCached == false) RefreshMetadataPeople();
            List<Person> metadataPeople;
            if (metadata != null)
            {
                var metadataAuthors = metadata.Authors.Select(a => new Person()
                { Name = a });

                var metadataNarrators = metadata.Narrators.Select(n => new Person()
                { Name = n });

                metadataPeople = CleanPeople(metadataAuthors.Concat(metadataNarrators).Concat(_metadataPeopleCache).ToList());
            }
            else
            {
                metadataPeople = _metadataPeopleCache;
            }

            var dbPeople = _context.People.ToList();
            var allPeople = dbPeople.Concat(metadataPeople).DistinctBy(c => c.Name)
                .ToList();
            return allPeople;
        }

        public List<Person> GetAllPeople(List<Book> books)
        {
            if (_isCached == false) RefreshMetadataPeople();

            var bookMetadata = books.Select(b => b.BookMetadata).ToList();

            var metadataAuthors = bookMetadata.SelectMany(b => b.Authors).Select(a => new Person()
            { Name = a });

            var metadataNarrators = bookMetadata.SelectMany(b => b.Narrators).Select(n => new Person()
            { Name = n });

            var metadataPeople = CleanPeople(metadataAuthors.Concat(metadataNarrators).Concat(_metadataPeopleCache).ToList());

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
