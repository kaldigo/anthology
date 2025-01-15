using Anthology.Data;
using Anthology.Plugins.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching; // For MemoryCache
using System.Threading.Tasks;

namespace Anthology.Services
{
    public class PersonService : IPersonService
    {
        private readonly DatabaseContext _context;

        public PersonService(DatabaseContext context)
        {
            _context = context;
        }

        public List<Person> GetPeople()
        {
            return _context.People.ToList();
        }

        /// <summary>
        /// Fetches all metadata people (authors + narrators from BookMetadata) and caches them in MemoryCache.
        /// </summary>
        public void RefreshMetadataPeople()
        {
            // Access the default memory cache
            MemoryCache cache = MemoryCache.Default;
            const string cacheKey = "MetadataPeople";

            // If the cache is already populated, skip fetching
            if (cache.Contains(cacheKey))
            {
                return;
            }

            // Otherwise, fetch data and populate the cache
            var bookMetadata = _context.Books.Select(b => b.BookMetadata).ToList();

            var metadataAuthorStrings = bookMetadata.SelectMany(b => b.Authors).ToList();
            var metadataAuthors = metadataAuthorStrings.Select(a => new Person
            {
                Name = a
            });

            var metadataNarratorStrings = bookMetadata.SelectMany(b => b.Narrators).ToList();
            var metadataNarrators = metadataNarratorStrings.Select(n => new Person
            {
                Name = n
            });

            // Clean up duplicates or unify references
            var metadataPeople = CleanPeople(metadataAuthors.Concat(metadataNarrators).ToList());

            // Store in the cache with an expiration policy (adjust as needed)
            cache.Set(
                cacheKey,
                metadataPeople,
                new CacheItemPolicy
                {
                    AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(30) // Cache for 30 minutes
                }
            );
        }

        /// <summary>
        /// Gets all people, combining metadata people (from cache) and database people.
        /// </summary>
        public List<Person> GetAllPeople(Metadata metadata = null)
        {
            // Ensure the cache is populated
            RefreshMetadataPeople();

            // Retrieve from cache
            var cache = MemoryCache.Default;
            var cachedValue = cache.Get("MetadataPeople") as List<Person>;
            if (cachedValue == null) // In case the cache is unexpectedly empty
            {
                RefreshMetadataPeople();
                cachedValue = cache.Get("MetadataPeople") as List<Person>;
            }

            // If metadata is provided, combine it with the cached data
            List<Person> metadataPeople;
            if (metadata != null)
            {
                var metadataAuthors = metadata.Authors.Select(a => new Person
                {
                    Name = a
                });

                var metadataNarrators = metadata.Narrators.Select(n => new Person
                {
                    Name = n
                });

                metadataPeople = CleanPeople(metadataAuthors.Concat(metadataNarrators).Concat(cachedValue ?? new List<Person>()).ToList());
            }
            else
            {
                metadataPeople = cachedValue ?? new List<Person>();
            }

            // Fetch people from the database
            var dbPeople = _context.People.ToList();

            // Combine and deduplicate
            var allPeople = dbPeople.Concat(metadataPeople).DistinctBy(c => c.Name).ToList();
            return allPeople;
        }

        public List<Person> GetAllPeople(List<Book> books)
        {
            // Ensure the cache is populated
            RefreshMetadataPeople();

            // Retrieve from cache
            var cache = MemoryCache.Default;
            var cachedValue = cache.Get("MetadataPeople") as List<Person>;
            if (cachedValue == null)
            {
                RefreshMetadataPeople();
                cachedValue = cache.Get("MetadataPeople") as List<Person>;
            }

            var bookMetadata = books.Select(b => b.BookMetadata).ToList();

            var metadataAuthors = bookMetadata.SelectMany(b => b.Authors).Select(a => new Person
            {
                Name = a
            });

            var metadataNarrators = bookMetadata.SelectMany(b => b.Narrators).Select(n => new Person
            {
                Name = n
            });

            // Combine book metadata people with the cached people
            var metadataPeople = CleanPeople(metadataAuthors.Concat(metadataNarrators).Concat(cachedValue ?? new List<Person>()).ToList());

            // Fetch database people
            var dbPeople = _context.People.ToList();

            // Combine and deduplicate
            var allPeople = dbPeople.Concat(metadataPeople).DistinctBy(c => c.Name).ToList();
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
            if (newPerson)
            {
                _context.People.Add(person);
            }
            else
            {
                _context.People.Update(person);
            }
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
                var person = GetPerson(item.Name);

                if (person != null)
                {
                    cleanedPeople.Add(person);
                }
                else
                {
                    cleanedPeople.Add(item);
                }
            }

            return cleanedPeople.DistinctBy(c => c.Name).ToList();
        }
    }
}
