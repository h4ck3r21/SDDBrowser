using System;
using System.Collections.Generic;
using System.Linq;

namespace SDDBrowser
{
    internal class BookmarkFolder
    {
        public List<Bookmark> Bookmarks = new List<Bookmark>();
        public List<BookmarkFolder> Folders = new List<BookmarkFolder>();
        public string Name;
        public BookmarkFolderJson JSONRepresentation;

        internal BookmarkFolder(string name)
        {
            this.Name = name;
            SetJSON();
        }

        internal BookmarkFolder(BookmarkFolderJson json, ContentPanel cp)
        {
            Name = json.Name;
            Bookmarks = json.Bookmarks.Select(b => new Bookmark(b, cp)).ToList();
            Folders = json.Folders.Select(f => new BookmarkFolder(f, cp)).ToList();
        }

        internal BookmarkFolder(string HTML, ContentPanel cp)
        {
            Name = ContentPanel.GetStringBetween(">", "</h3>", HTML);
            AddHTML(ContentPanel.GetStringBetween("<dl>", "</dl>", HTML), cp, 1);
        }

        private void SetJSON()
        {
            JSONRepresentation = new BookmarkFolderJson
            {
                Name = Name,
                Bookmarks = Bookmarks.Select(b => b.GetJSON()).ToList(),
                Folders = Folders.Select(f => f.GetJSON()).ToList()
            };
        }

        public BookmarkFolderJson GetJSON()
        {
            SetJSON();
            return JSONRepresentation;
        }

        public List<Bookmark> Find(Predicate<Bookmark> predicate)
        {
            List<Bookmark> finds = Bookmarks.FindAll(predicate);
            foreach (BookmarkFolder folder in Folders)
            {
                finds.AddRange(folder.Find(predicate));
            }
            return finds;
        }

        public void RemoveBookmark(Bookmark bookmark)
        {
            if (bookmark == null)
            {
                return;
            }
            else if (Bookmarks.Contains(bookmark))
            {
                Bookmarks.Remove(bookmark);
            }
            else
            {
                foreach (BookmarkFolder folder in Folders)
                {
                    folder.RemoveBookmark(bookmark);
                }
            }
        }

        public string ToHTML()
        {
            return $@"<dt>
                    <h3>{Name}</h3>
                        <dl>
                            <p>
                            </p>
                            {String.Join("\n", Bookmarks.Select(b => b.ToHTML()))}
                            {String.Join("\n", Folders.Select(b => b.ToHTML()))}
                        </d1><p>
                        </p>
                    </dt>";
        }

        public void AddHTML(string HTML, ContentPanel cp, int dlOffset)
        {
            dlOffset--;
            List<string> elements = ContentPanel.GetHTMLTagContent("dt", HTML);
            elements.RemoveAt(0);
            foreach (string element in elements)
            {
                if (element.Trim().StartsWith("<a"))
                {
                    Bookmarks.Add(new Bookmark(element, cp));
                }
                else if (element.Trim().StartsWith("<h3"))
                {

                }
            }
        }
    }
}
