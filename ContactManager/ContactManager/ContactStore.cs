using System;
using System.Collections.Generic;
using ContactManager.Filters;
using DataStructures;
using System.Linq;

namespace ContactManager
{
    public class ContactStore : IContactStore
    {
        //BinaryTree<Contact> contacts = new BinaryTree<Contact>();
        SortedList<Contact> contacts = new SortedList<Contact>();

        //State level cache
       HashTable<string, BinaryTree<Contact>> stateCache = new HashTable<string, BinaryTree<Contact>>();

        int nextId = 1;
        int contactCount = 0;

        public IEnumerable<Contact> Contacts
        {
            get
            {
                return Contacts;
            }
        }

        public Contact Add(Contact contact)
        {


            if (contact.Equals(null))
            {
                Log.Error("Remove: null contact provided");
                throw new ArgumentNullException("Null contact provided to remove");
            }

            int id = contact.ID.HasValue ? contact.ID.Value : nextId++;
            nextId = Math.Max(nextId, id + 1);

            Contact withId = Contact.CreateWithId(id, contact);

            Log.Info("Add: adding new contact with ID {0} ({1} {2})", withId.ID, withId.FirstName, withId.LastName);

            Log.Verbose("Add: adding new contact with ID {0} ({1} {2})", withId.ID, withId.FirstName, withId.LastName);
            contacts.Add(withId);

            // Add to the state-level cache.
            // If the state does not currently exist in the cache - add the binary tree
            if (!stateCache.ContainsKey(withId.State))
            {
                stateCache.Add(withId.State, new BinaryTree<Contact>());
            }

            // now we know the state exists so add the contact
            stateCache[withId.State].Add(withId);

            Log.Verbose("Add: complete ({0})", withId.ID);

            return withId;
        }

        public IEnumerable<Contact> Add(IEnumerable<Contact> contacts)
        {
            if (contacts == null)
            {
                Log.Error("Add: null contacts provided");
                throw new ArgumentNullException("contacts");
            }

            int beforeCount = this.contacts.Count;

            foreach (Contact c in contacts)
            {
                Add(c);
            }

            Log.Info("Added {0} contacts", this.contacts.Count - beforeCount);

            return Contacts;
        }

        public IEnumerable<Contact> Load(IEnumerable<Contact> newContacts)
        {
            nextId = 1;

            //Add(newContacts);

            //Log.Info("Loaded {0} contacts", contacts.Count);

            return  Add(newContacts);
        }

        public bool Remove(ContactFieldFilter filter, out Contact removed)
        {
            Contact toRemove = Search(filter).FirstOrDefault();
            return Remove(toRemove, out removed);
        }

        public bool Remove(Contact contact, out Contact removed)
        {
            Contact kontact = new Contact();
            kontact = contact;
            if (contact.Equals(null))
            {
                Log.Error( "Remove: null contact provided");
                throw new ArgumentNullException("Null contact provided to remove");
            }
            else 
            { 
                if (contacts.Find(contact, out removed))
                {
                    contacts.Remove(removed);
                    Log.Info("Remove: removed contact{0} {1} {2}",removed.ID.Value,removed.FirstName,removed.LastName);
                    return true;
                }
               
            }
            

            Log.Warning("Remove: Contact not found.  No action taken.");
            removed = default;
            return false;
        }

        public IEnumerable<Contact> Search(ContactFieldFilter filter)
        {
            Log.Verbose("Searching for contacts with filter: {0}", filter);

            // If the file has a state component
            // get the items from the cache instead of checking everything
            if (filter.State.HasValue)
            {
                if (stateCache.ContainsKey(filter.State.Value))
                {
                    return filter.Apply(stateCache[filter.State.Value]);
                }
                else
                {
                    return new SortedList<Contact>();
                }
            }
            else
            {
                return filter.Apply(this.Contacts);
            }
        }
    }
}
