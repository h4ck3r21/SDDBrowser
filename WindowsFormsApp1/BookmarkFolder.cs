using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.WiFiDirect.Services;

namespace SDDBrowser
{
    internal class BookmarkFolder
    {
        public List<Bookmark> bookmarks = new List<Bookmark>();
        public List<BookmarkFolder> folders = new List<BookmarkFolder>();
        public string name;
        public BookmarkFolderJson JSONRepresentation;

        internal BookmarkFolder(string name)
        {
            this.name = name;
            setJSON();
        }

        internal BookmarkFolder(BookmarkFolderJson json, ContentPanel cp) 
        {
            name = json.name;
            bookmarks = json.bookmarks.Select(b => new Bookmark(b, cp)).ToList();
            folders = json.folders.Select(f => new BookmarkFolder(f, cp)).ToList();
        }

        internal BookmarkFolder(string HTML, ContentPanel cp)
        {
            name = ContentPanel.getStringBetween(">", "</h3>", HTML);
            addHTML(ContentPanel.getStringBetween("<dl>", "</dl>", HTML), cp, 1);
        }

        private void setJSON()
        {
            JSONRepresentation = new BookmarkFolderJson
            {
                name = name,
                bookmarks = bookmarks.Select(b => b.getJSON()).ToList(),
                folders = folders.Select(f => f.getJSON()).ToList()
            };
        }

        public BookmarkFolderJson getJSON()
        {
            setJSON();
            return JSONRepresentation;
        }

        public List<Bookmark> Find(Predicate<Bookmark> predicate)
        {
            List<Bookmark> finds = bookmarks.FindAll(predicate);
            foreach (BookmarkFolder folder in folders) 
            {
                finds.AddRange(folder.Find(predicate));
            }
            return finds;
        }

        public void removeBookmark(Bookmark bookmark)
        {
            if (bookmark == null)
            {
                return;
            }
            else if (bookmarks.Contains(bookmark))
            {
                bookmarks.Remove(bookmark);
            }
            else
            {
                foreach (BookmarkFolder folder in folders)
                {
                    folder.removeBookmark(bookmark);
                }
            }
        }

        public string toHTML()
        {
            return $@"<dt>
                    <h3>{name}</h3>
                        <dl>
                            <p>
                            </p>
                            {String.Join("\n", bookmarks.Select(b => b.toHTML()))}
                            {String.Join("\n", folders.Select(b => b.toHTML()))}
                        </d1><p>
                        </p>
                    </dt>";
        }

        public void addHTML(string HTML, ContentPanel cp, int dlOffset)
        {
            dlOffset--;
            List<string> elements = ContentPanel.getHTMLTagContent("dt", HTML);
            elements.RemoveAt(0);
            foreach (string element in elements)
            {
                if (element.Trim().StartsWith("<a"))
                {
                    bookmarks.Add(new Bookmark(element, cp));
                }
                else if (element.Trim().StartsWith("<h3"))
                {

                }
            }
        }
    }
}
