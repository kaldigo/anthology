using Anthology.Data;
using Anthology.Plugins.Models;

namespace Anthology.Services
{
    public interface IPersonService
    {
        void DeletePerson(Person person);
        Person GetPerson(Guid guid);
        Person? GetPerson(string name);
        List<Person> GetPeople();
        void RefreshMetadataPeople();
        List<Person> GetAllPeople(Metadata metadata = null);
        void SavePerson(Person person, bool newPerson = false);
        List<Person> CleanPeople(List<Person> people);

    }
}