using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevChallenge.Server.Scenarios.Fragmentation
{

    public class StorageException : Exception
    {
        public StorageException(string msg)
            : base(msg)
        {
        }
    }

    public class Crate
    {
        public int Id { get; private set; }
        public int Size {get; private set;}

        public Crate(int id, int size)
        {
            Id = id;
            Size = size;
        }
    }

    public class Storage
    {


        public IReadOnlyDictionary<int, Crate> Crates { get { return crates; } }

        public Crate GetCrate(int id)
        {
            try
            {
                return Crates.Single(x => x.Value.Id == id).Value;
            }
            catch (Exception)
            {
                throw new StorageException(String.Format("Crate with id {0} does not exist in storage", id));
            }
        }

        public Storage(int capacity)
        {
            Capacity = capacity;
        }

        public int Capacity { get; private set; }


        public bool Exists(Crate c)
        {
            return Crates.Any(x => x.Value.Id == c.Id);
        }

        public bool ValidPlacement(int position, int size)
        {
            return crates.All(x => x.Key >= position + size || position >= x.Key + x.Value.Size);
        }


        public bool CanAdd(Crate crate, int position)
        {
            if (Exists(crate)) return false;
            if (!ValidPlacement(position, crate.Size)) return false;
            return true;
        }


        public void Add(Crate crate, int position)
        {
            if (Exists(crate)) throw new StorageException("Crate Id already exists in storage");
            if (!ValidPlacement(position,crate.Size)) throw new StorageException("Crate does not fit in location");
            crates.Add(position, crate);
        }


        public void Remove(Crate crate)
        {
            crates.Remove(PositionOf(crate));
        }

        public void Relocate(Crate crate, int newpos)
        {
            Remove(crate);
            Add(crate, newpos);
        }

        public int PositionOf(Crate crate)
        {
            return crates.Single(x => x.Value == crate).Key;
        }


        Dictionary<int, Crate> crates = new Dictionary<int, Crate>();

    }
}
