﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eventide.Libraries
{
    // TODO: Perhaps I should make libraries only reference themselves and the global library.
    //   This would avoid some of the memory gymnastics, and excessive resource duplication can be avoided by
    //   loading multi-active-scene assets in the global library only.
    // For max simplicity, I could make libraries only ever reference themselves. This would be best for performance.
    //   But the current setup (autoreferencing among all libraries) is the easiest to use. I can just load any asset
    //   into the current scene's libraries, and it'll automatically be moved as needed.

    public enum CopyState
    {
        unique,
        source,
        copy
    }
    
    public class Book<T> //where T : class
    {
        public T Item { get; set; }
        public CopyState CopyState { get; set; }

        public Book (T item, CopyState copyState)
        {
            this.Item = item;
            this.CopyState = copyState;
        }
    }
    
    public class Library<K, T> //where T : class
    {
        #region static
        /*

            STATIC PARAMETERS AND METHODS

        */

        static List<Library<K, T>> libraries;

        // This class must be Initialized before any other methods are called.
        // Currently handled automatically by Library.Initialize();
        public static void Initialize()
        {
            libraries = new List<Library<K, T>>();
        }

        // Libraries should always be created using this function to ensure they are added to the static list.
        public static Library<K, T> NewLibrary()
        {
            Library<K, T> library = new Library<K, T>();
            libraries.Add(library);
            return library;
        }
        // This alternative allows the inheritors to have custom constructors.
        public static void AddLibrary(Library<K, T> library)
        {
            libraries.Add(library);
        }

        // Libraries should always be removed from the static list via this function when they are no longer needed.
        // Otherwise, unneeded assets will not be garbage collected.
        // TODO: If I ever need to remove multiple libraries in a short time span, it would be more efficient to also
        //   have a method that flags libraries for removal, then handles them smartly to avoid juggling asset loading.
        public static void RemoveLibrary(Library<K, T> library)
        {
            library.Unload();
            libraries.Remove(library);
        }
        #endregion

        #region local
        /*

            LOCAL PARAMETERS AND METHODS

        */

        Dictionary<K, Book<T>> list;

        public Library()
        {
            list = new Dictionary<K, Book<T>>();
        }

        // This method checks if an asset has been loaded already, loads if necessary, then returns the reference.
        // --The local library is checked first.
        // --If an asset reference is found in another library, the reference is copied to the local library.
        public T Register(K key)
        {
            Book<T> book;

            // Search for loaded asset in local library and return if found.
            if (list.TryGetValue(key, out book)) return book.Item;

            // Search for loaded asset in all libraries and copy reference to local library if found.
            foreach (Library<K, T> tLibrary in libraries)
            {
                if (tLibrary == this) continue;
                if (tLibrary.list.TryGetValue(key, out book) && book.CopyState != CopyState.copy) // copy from the original only so that the state is set correctly
                {
                    list.Add(key, new Book<T>(book.Item, CopyState.copy));
                    book.CopyState = CopyState.source;
                    return book.Item;
                }
            }

            // Otherwise, load the asset and store a reference in the local library.
            book = new Book<T>(Load(key), CopyState.unique);
            list.Add(key, book);
            return book.Item;
        }

        protected virtual T Load(K key) { return default(T);  }

        /*
        // Removes the specified asset from the local library.
        // --Should only be used if absolutely certain the asset isn't being used locally and won't be used again soon.
        // --Should only be used when large amounts of data should be released prematurely for performance reasons.
        // Generally, garbage collection is sufficient to handle necessary removal whenever asset libraries are deleted.
        public void Remove(K key)
        {
            list.Remove(key);
        }
        // Slower alternative method.
        public void Remove(T item)
        {
            foreach (K key in list.Keys)
            {
                if (list[key].Equals(item))
                {
                    // Only delete the first matching instance, because multiple local instances should not exist.
                    list.Remove(key);
                    break;
                }
            }
        }
        // Individual item removal cannot work well for assets loaded by a ContentManager.
        // I guess I could create a ContentManger for each and every item... o_o
        */

        // Forcibly unload assets, and force external references to reload them in their own libraries.
        public virtual void Unload()
        {
            // TODO: Test this for bugs.
            foreach (K key in list.Keys)
            {
                Unload(list[key].Item);
                // TODO: if CopyState == copy, then the original might need change from "source" to "unique".
                //   Though the only penalty for keeping it as "source" is an unneccesary lookup check when the library
                //   is unloaded.
                if (list[key].CopyState == CopyState.source)
                {
                    Book<T> book;
                    Book<T> first = null;
                    foreach (Library<K, T> tLibrary in libraries)
                    {
                        if (tLibrary == this) continue;
                        if (tLibrary.list.TryGetValue(key, out book))
                        {
                            if (first == null)
                            {
                                // TODO: This forces unloaded content to reload, but this should only be necessary for
                                //   ContentManager content. Other content can suffice with reference passing and not
                                //   calling "Unload(item)" as above.
                                // Though perhaps it is fair to treat all content as ContentManager content,
                                //   since it easily could be in the final release.
                                first = book;
                                book.Item = Load(key);
                                book.CopyState = CopyState.unique;
                            }
                            else
                            {
                                first.CopyState = CopyState.source;
                                book.Item = first.Item;
                                book.CopyState = CopyState.copy;
                            }
                        }
                    }
                }
            }
            list.Clear();
        }
        protected virtual void Unload(T item) { }
        #endregion
    }
}
